using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;

namespace Rvt.Background
{
    /// <summary>
    /// The background processor does arbitrary work in the background while Revit is Idle.
    /// It contains a queue of work items (of any user defined type) for later processing.
    /// Good practice for choosing a work item type is to not use a Revit API type (e.g., use an integer for element ID).
    /// Work processing is performed by a delegate / action provided to the constructors. 
    /// When an OnIdle or DocumentChanged event occurs, work will be extracted out of the queue
    /// and executed using the provided action.
    /// This will be repeated until the maximum MSec per batch is reached. 
    /// You can pause processing, execute all items immediately, or do other work. 
    /// </summary>
    public class BackgroundProcessor<T> : IDisposable
    {
        public readonly Queue<T> Queue = new Queue<T>();
        public int MaxMSecPerBatch { get; set; } = 100;
        public bool ExecuteNextIdleImmediately { get; set; }
        public readonly Action<T> Action;
        public readonly UIApplication UIApp;
        public Application App => UIApp.Application;
        public bool PauseProcessing { get; set; }
        public event EventHandler<Exception> ExceptionEvent;
        public readonly Stopwatch WorkStopwatch = new Stopwatch();
        public int WorkProcessedCount = 0;
        private bool _enabled = false;
        public bool DoWorkDuringIdle { get; set; } = true;
        public bool DoWorkDuringProgress { get; set; } = true;

        public bool Enabled
        {
            get => _enabled;
            set
            {

                if (value)
                    Attach();
                else
                    Detach();
            }
        }

        public BackgroundProcessor(Action<T> action, UIApplication uiApp)
        {
            Action = action;
            UIApp = uiApp;
            Attach();
        }

        public void Detach()
        {
            if (!_enabled)
                return;
            App.ProgressChanged -= App_ProgressChanged;
            UIApp.Idling -= UiApp_Idling;
            _enabled = true;
        }

        public void Attach()
        {
            if (_enabled)
                return;
            App.ProgressChanged += App_ProgressChanged;
            UIApp.Idling += UiApp_Idling;
            _enabled = true;
        }

        private void App_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (!DoWorkDuringProgress || PauseProcessing)
                return;
            ProcessWork();
        }

        public void Dispose()
        {
            ExceptionEvent = null;
            Detach();
        }

        private void UiApp_Idling(object sender, Autodesk.Revit.UI.Events.IdlingEventArgs e)
        {
            if (!DoWorkDuringIdle || PauseProcessing)
                return;
            ProcessWork();
            if (ExecuteNextIdleImmediately)
                e.SetRaiseWithoutDelay();
        }

        /// <summary>
        /// Note that this can be called outside of the Revit API context. 
        /// </summary>
        public void EnqueueWork(IEnumerable<T> items)
        {
            foreach (var item in items)
                Queue.Enqueue(item);
        }

        public bool HasWork
            => Queue.Count > 0;

        public void ClearWork()
            => Queue.Clear();

        public void ResetStats()
        {
            WorkStopwatch.Reset();
            WorkProcessedCount = 0;
        }

        public void ProcessWork(bool doAllNow = false)
        {
            var startedTime = WorkStopwatch.ElapsedMilliseconds;
            WorkStopwatch.Start();
            try
            {
                while (HasWork)
                {
                    var item = Queue.Dequeue();
                    Action(item);
                    WorkProcessedCount++;

                    var elapsedTime = WorkStopwatch.ElapsedMilliseconds - startedTime;
                    if (elapsedTime > MaxMSecPerBatch && !doAllNow)
                        break;
                }
            }
            catch (Exception ex)
            {
                ExceptionEvent?.Invoke(this, ex);
            }
            finally
            {
                WorkStopwatch.Stop();
            }
        }
    }
}