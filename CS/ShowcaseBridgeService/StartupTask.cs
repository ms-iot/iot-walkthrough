using System;
using System.Diagnostics;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace ShowcaseBridgeService
{
    class ValueChangedEventArgs : EventArgs
    {
        private ValueSet _changedValues;

        public ValueChangedEventArgs(ValueSet changedValues)
        {
            _changedValues = changedValues;
        }

        public ValueSet ChangedValues { get { return _changedValues; } }
    }

    public sealed class StartupTask : IBackgroundTask
    {
        private BackgroundTaskDeferral _deferral;
        private AppServiceConnection _connection;
        private static ValueSet _valueStore;
        private static EventHandler ValueChanged;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            taskInstance.Canceled += OnTaskCanceled;
            Debug.WriteLine("ShowcaseBridgeService FamilyName: " + Windows.ApplicationModel.Package.Current.Id.FamilyName);

            if (_valueStore == null)
            {
                _valueStore = new ValueSet();
            }

            if (SetupConnection(taskInstance.TriggerDetails as AppServiceTriggerDetails))
            {
                _deferral = taskInstance.GetDeferral();
            }
        }

        private bool SetupConnection(AppServiceTriggerDetails triggerDetails)
        {
            if (triggerDetails == null)
            {
                Debug.WriteLine("ForegroundBridgeService started without details, exiting");
                return false;
            }
            if (!triggerDetails.Name.Equals("com.microsoft.showcase.bridge"))
            {
                Debug.WriteLine("Trigger details name doesn't match com.microsoft.showcase.bridge, exiting");
                return false;
            }
            Debug.WriteLine("New service connection");
            _connection = triggerDetails.AppServiceConnection;
            _connection.RequestReceived += OnRequestReceived;
            ValueChanged += BroadcastReceivedMessage;

            return true;
        }

        private void OnTaskCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
           Debug.WriteLine("Cancellation, reason: " + reason);
           ValueChanged -= BroadcastReceivedMessage;
           if (_deferral != null)
           {
               _deferral.Complete();
           }
        }

        private async void BroadcastReceivedMessage(object sender, EventArgs args)
        {
            await _connection.SendMessageAsync(((ValueChangedEventArgs)args).ChangedValues);
        }

        private async void OnRequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            // Use args.GetDeferral() (and release the deferral later) if args is to be used after awaited calls
            var requestedValues = new ValueSet();
            var values = new ValueSet();
            foreach (var element in args.Request.Message)
            {
                if (element.Value != null)
                {
                    _valueStore[element.Key] = element.Value;
                    values.Add(element.Key, element.Value);
                }
                else
                {
                    var key = element.Key;
                    _valueStore.TryGetValue(key, out object value);
                    requestedValues.Add(key, value);
                }
            }
            if (values.Count != 0)
            {
                ValueChanged?.Invoke(this, new ValueChangedEventArgs(values));
            }
            if (requestedValues.Count != 0)
            {
                await _connection.SendMessageAsync(requestedValues);
            }
        }
    }
}
