using System;
using System.Diagnostics;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace ShowcaseBridgeService
{
    public sealed class StartupTask : IBackgroundTask
    {
        private BackgroundTaskDeferral _deferral;
        private AppServiceConnection _connection;
        private static ValueStore _store;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();
            Debug.WriteLine("ShowcaseBridgeService FamilyName: " + Windows.ApplicationModel.Package.Current.Id.FamilyName);

            if (_store == null)
            {
                _store = new ValueStore();
            }

            if (SetupConnection(taskInstance.TriggerDetails as AppServiceTriggerDetails))
            {
                taskInstance.Canceled += OnTaskCanceled;
                _store.ValueChanged += BroadcastReceivedMessage;
            }
            else
            {
                _deferral.Complete();
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
            return true;
        }

        private void OnTaskCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
           Debug.WriteLine("Cancellation, reason: " + reason);
           if (_deferral != null)
            {
                _deferral.Complete();
            }
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
                    values.Add(element.Key, element.Value);
                }
                else
                {
                    var key = element.Key;
                    requestedValues.Add(key, _store.GetSetting(key));
                }
            }
            if (values.Count != 0)
            {
                _store.SetSettings(values);
            }
            if (requestedValues.Count != 0)
            {
                await _connection.SendMessageAsync(requestedValues);
            }
        }

        private async void BroadcastReceivedMessage(object sender, EventArgs args)
        {
            var changedArgs = args as ValueChangedEventArgs;
            await _connection.SendMessageAsync(changedArgs.ChangedValues);
        }
    }
}
