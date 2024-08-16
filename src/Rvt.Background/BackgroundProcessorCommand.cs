using System;
using System.Globalization;
using System.Linq;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;

namespace Rvt.Background
{
    /// <summary>
    /// This is a command for use with Bowerbird. 
    /// </summary>
    public class BackgroundProcessorCommand 
    {
        public UIApplication App;
        public string Name => "Background Processor";
        public BackgroundProcessor<int> Processor;
        public BackgroundForm Form;
        public ExternalEvent EnableProcessorEvent;
        public ExternalEvent DisableProcessorEvent;
        public ExternalEvent DisposeProcessorEvent;
        public ExternalEvent DoSomeWorkEvent;
        public ExternalEvent DoAllWorkEvent;

        public void Execute(object arg)
        {
            App = arg as UIApplication;
            Processor = new BackgroundProcessor<int>(Process, App);
            Form = new BackgroundForm();
            Form.Update();
            Form.Show();
            Form.FormClosing += Form_FormClosing;
            Form.checkBoxProcessDuringIdle.Click += CheckBoxProcessDuringIdle_Click;
            Form.checkBoxProcessDuringProgress.Click += CheckBoxProcessDuringProgress_Click;
            Form.checkBoxIdleEventNoDelay.Click += CheckBoxIdleEventNoDelay_Click;
            Form.checkBoxEnabled.Click += CheckBoxEnabled_Click;
            Form.checkBoxPaused.Click += CheckBoxPauseProcessingOnClick;
            Form.numericUpDownMsecPerBatch.ValueChanged += NumericUpDownMsecPerBatch_ValueChanged;
            Form.buttonClearWork.Click += ButtonClearWork_Click;
            Form.buttonResetStats.Click += ButtonResetStats_Click;
            Form.buttonProcessAll.Click += ButtonProcessAll_Click;
            Form.buttonProcessSome.Click += ButtonProcessSome_Click;
            EnableProcessorEvent = ApiContext.CreateEvent(_ => Processor.Enabled = true, "Enable processor");
            DisableProcessorEvent = ApiContext.CreateEvent(_ => Processor.Enabled = false, "Disable processor");

            // Very important that we don't forget to disconnect Revit events in Bowerbird, otherwise we have to restart 
            App.Application.DocumentChanged += ApplicationOnDocumentChanged;

            DisposeProcessorEvent = ApiContext.CreateEvent(_ =>
            {
                // Cleaning up the Revit event
                App.Application.DocumentChanged -= ApplicationOnDocumentChanged;
                Processor.Dispose();
                Processor = null;
            }, "Disposing processor and closing form");

            DoSomeWorkEvent = ApiContext.CreateEvent(_ => Processor.ProcessWork(), "Processing some work");
            DoAllWorkEvent = ApiContext.CreateEvent(_ => Processor.ProcessWork(true), "Processing all work");
        }

        private void ButtonProcessSome_Click(object sender, EventArgs e)
        {
            DoSomeWorkEvent.Raise();
        }

        private void ButtonProcessAll_Click(object sender, EventArgs e)
        {
            DoAllWorkEvent.Raise();
        }

        private void CheckBoxEnabled_Click(object sender, EventArgs e)
        {
            if (Form.checkBoxEnabled.Checked)
                EnableProcessorEvent.Raise();
            else
                DisableProcessorEvent.Raise();
        }

        private void CheckBoxProcessDuringProgress_Click(object sender, EventArgs e)
        {
            Processor.DoWorkDuringProgress = Form.checkBoxProcessDuringProgress.Checked;
        }

        private void CheckBoxProcessDuringIdle_Click(object sender, EventArgs e)
        {
            Processor.DoWorkDuringIdle = Form.checkBoxProcessDuringIdle.Checked;
        }

        private void Form_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            DisposeProcessorEvent.Raise();
        }

        private void ButtonResetStats_Click(object sender, EventArgs e)
        {
            Processor.ResetStats();
            UpdateForm();
        }

        private void ButtonClearWork_Click(object sender, EventArgs e)
        {
            Processor.ClearWork();
            UpdateForm();
        }

        private void NumericUpDownMsecPerBatch_ValueChanged(object sender, EventArgs e)
        {
            Processor.MaxMSecPerBatch = (int)Form.numericUpDownMsecPerBatch.Value;
        }

        private void CheckBoxPauseProcessingOnClick(object sender, EventArgs e)
        {
            Processor.PauseProcessing = Form.checkBoxPaused.Checked;
        }

        private void CheckBoxIdleEventNoDelay_Click(object sender, EventArgs e)
        {
            Processor.ExecuteNextIdleImmediately = Form.checkBoxIdleEventNoDelay.Checked;
        }

        private void ApplicationOnDocumentChanged(object sender, DocumentChangedEventArgs e)
        {
            Processor.EnqueueWork(e.GetAddedElementIds().Select(eid => eid.IntegerValue));
            Processor.EnqueueWork(e.GetDeletedElementIds().Select(eid => eid.IntegerValue));
            Processor.EnqueueWork(e.GetModifiedElementIds().Select(eid => eid.IntegerValue));
            UpdateForm();
        }

        public void Process(int id)
        {
            UpdateForm();
        }

        public void UpdateForm()
        {
            Form.textBoxCpuTimeOnWork.Text = (Processor.WorkStopwatch.ElapsedMilliseconds / 1000f).ToString(CultureInfo.InvariantCulture);
            Form.textBoxItemsQueued.Text = Processor.Queue.Count.ToString();
            Form.textBoxWorkItemProcessed.Text = Processor.WorkProcessedCount.ToString();
            Form.checkBoxPaused.Checked = Processor.PauseProcessing;
            Form.checkBoxEnabled.Checked = Processor.Enabled;
            Form.checkBoxProcessDuringIdle.Checked = Processor.DoWorkDuringIdle;
            Form.checkBoxProcessDuringProgress.Checked = Processor.DoWorkDuringProgress;
            Form.checkBoxIdleEventNoDelay.Checked = Processor.ExecuteNextIdleImmediately;
            Form.numericUpDownMsecPerBatch.Value = Processor.MaxMSecPerBatch;
            System.Windows.Forms.Application.DoEvents();
        }
    }
}