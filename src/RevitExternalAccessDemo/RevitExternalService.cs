/* 
 * Copyright 2014 © Victor Chekalin
 * 
 * THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
 * KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
 * PARTICULAR PURPOSE.
 * 
 */

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitExternalAccessDemo.Properties;

namespace RevitExternalAccessDemo
{
    class RevitExternalService : IRevitExternalService
    {
        private string currentDocumentPath;

        private static readonly object Locker = new object();

        private const int WaitTimeout = 10000; // 10 seconds timeout

        public string GetCurrentDocumentPath()
        {
            Debug.Print("{0}: {1}", Resources.PushTaskToTheContainer, DateTime.Now.ToString("HH:mm:ss.fff"));

            lock (Locker)
            {
                TaskContainer.Instance.EnqueueTask(GetDocumentPath);

                // Wait when the task is completed
                Monitor.Wait(Locker, WaitTimeout);
            }

            Debug.Print("{0}: {1}", Resources.FinishTask, DateTime.Now.ToString("HH:mm:ss.fff"));
            return currentDocumentPath;
        }

        private void GetDocumentPath(UIApplication uiapp)
        {
            try
            {
                currentDocumentPath = uiapp.ActiveUIDocument.Document.PathName;
            }
            // Always release locker in finally block
            // to ensure to unlock locker object.
            finally
            {
                lock (Locker)
                {
                    Monitor.Pulse(Locker);
                }
            }
        }

        public bool CreateWall(XYZ startPoint, XYZ endPoint)
        {

            Wall wall = null;

            lock (Locker)
            {

                TaskContainer.Instance.EnqueueTask(uiapp =>
                    {
                        try
                        {
                            var doc = uiapp.ActiveUIDocument.Document;

                            using (var t = new Transaction(doc, Resources.CreateWall))
                            {
                                t.Start();

                                Curve curve = Line.CreateBound(
                                    new Autodesk.Revit.DB.XYZ(startPoint.X, startPoint.Y, startPoint.Z),
                                    new Autodesk.Revit.DB.XYZ(endPoint.X, endPoint.Y, endPoint.Z));
                                var collector = new FilteredElementCollector(doc);

                                var level = collector
                                    .OfClass(typeof (Level))
                                    .ToElements()
                                    .OfType<Level>()
                                    .First();

                                wall = Wall.Create(doc, curve, level.Id, true);

                                t.Commit();
                            }
                        }
                        finally
                        {

                            lock (Locker)
                            {
                                Monitor.Pulse(Locker);
                            }
                        }

                    });

                Monitor.Wait(Locker);
            }
            return wall != null;
        }
    }
}