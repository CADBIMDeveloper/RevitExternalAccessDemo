/* 
 * Copyright 2014 © Victor Chekalin
 * 
 * THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
 * KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
 * PARTICULAR PURPOSE.
 * 
 */

#region Namespaces
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading.Tasks;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using RevitExternalAccessDemo.Properties;

#endregion

namespace RevitExternalAccessDemo
{
    class App : IExternalApplication
    {
        private const string ServiceUrlHttp = "http://localhost:9001/RevitExternalService";
        private const string ServiceUrlTcp = "net.tcp://localhost:9002/RevitExternalService";
        
        public Result OnStartup(UIControlledApplication a)
        {
            a.Idling += OnIdling;
            
            try
            {
                Task.Factory.StartNew(() =>
                    {
                        var serviceHost = new ServiceHost(typeof(RevitExternalService), new Uri(ServiceUrlHttp), new Uri(ServiceUrlTcp));

                        serviceHost.Description.Behaviors.Add(new ServiceMetadataBehavior());
                        serviceHost.AddServiceEndpoint(typeof(IRevitExternalService), new BasicHttpBinding(), ServiceUrlHttp);
                        serviceHost.AddServiceEndpoint(typeof(IRevitExternalService), new NetTcpBinding(), ServiceUrlTcp);
                        serviceHost.AddServiceEndpoint(typeof(IMetadataExchange), MetadataExchangeBindings.CreateMexHttpBinding(), "mex");
                        serviceHost.Open();
                    }, TaskCreationOptions.LongRunning);
            }
            catch (Exception ex)
            {                
                a.ControlledApplication
                    .WriteJournalComment(string.Format("{0}.\r\n{1}",  
                        Resources.CouldNotStartWCFService,
                        ex.ToString()),
                    true);
                
            }
            
            return Result.Succeeded;
        }

        private void OnIdling(object sender, IdlingEventArgs e)
        {
            var uiApp = sender as UIApplication;

            Debug.Print("OnIdling: {0}", DateTime.Now.ToString("HH:mm:ss.fff"));

            // be carefull. It loads CPU 
            e.SetRaiseWithoutDelay();

            if (!TaskContainer.Instance.HasTaskToPerform)
                return;

            try
            {
                Debug.Print("{0}: {1}", Resources.StartExecuteTask, DateTime.Now.ToString("HH:mm:ss.fff"));

                var task = TaskContainer.Instance.DequeueTask();
                task(uiApp);

                Debug.Print("{0}: {1}", Resources.EndExecuteTask, DateTime.Now.ToString("HH:mm:ss.fff"));
            }
            catch (Exception ex)
            {
                uiApp.Application.WriteJournalComment(
                    string.Format("RevitExternalService. {0}:\r\n{2}",
                    Resources.AnErrorOccuredWhileExecutingTheOnIdlingEvent,
                    ex.ToString()), true);

                Debug.WriteLine(ex);
            }

            //e.SetRaiseWithoutDelay();
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            a.Idling -= OnIdling;
            
            return Result.Succeeded;
        }
    }
}
