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
using System.Collections.Generic;
using Autodesk.Revit.UI;

namespace RevitExternalAccessDemo
{
    public class TaskContainer
    {
        private static readonly object LockObj = new object();
        private volatile static TaskContainer instance;

        private readonly Queue<Action<UIApplication>> tasks;

        private TaskContainer()
        {
            tasks = new Queue<Action<UIApplication>>();
        }

        public static TaskContainer Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (LockObj)
                    {
                        if (instance == null)
                        {
                            instance = new TaskContainer();
                        }
                    }
                }

                return instance;
            }
        }

        public void EnqueueTask(Action<UIApplication> task)
        {
            tasks.Enqueue(task);
        }

        public bool HasTaskToPerform
        {
            get { return tasks.Count > 0; }
        }

        public Action<UIApplication> DequeueTask()
        {
            return tasks.Dequeue();
        }
    }
}