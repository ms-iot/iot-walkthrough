---
---
# Communication between applications

## Introduction

We will extend the basic [app service created in the last tutorial](../Creation/README.md) to forward messages from and to multiple apps.

Messages are exchanged as `ValueSet` structures, which are maps with `String` keys and any serializable `Object` as values. Messages received from any app connected to the app service will be forwarded to all other apps. The app service will also keep the most recent value of each key and allow applications to query this value.

## Receiving and sending messages on the app service

The app service will have a `private static ValueSet` field to keep the received values. It will be shared among all `StartupTask` instances.

From the last tutorial, we will now add a `private static ValueSet _valueStorage` field and update its value at the callback:

```cs
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
        private static ValueSet _valueStorage = new ValueSet();

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            taskInstance.Canceled += OnTaskCanceled;
            Debug.WriteLine($"ShowcaseBridgeService FamilyName: {Windows.ApplicationModel.Package.Current.Id.FamilyName}.");

            if (SetupConnection(taskInstance.TriggerDetails as AppServiceTriggerDetails))
            {
                _deferral = taskInstance.GetDeferral();
            }
        }

        private bool SetupConnection(AppServiceTriggerDetails triggerDetails)
        {
            if (triggerDetails == null)
            {
                Debug.WriteLine("ForegroundBridgeService started without details, exiting.");
                return false;
            }
            if (!triggerDetails.Name.Equals("com.microsoft.showcase.appservice"))
            {
                Debug.WriteLine("Trigger details name doesn't match com.microsoft.showcase.bridge, exiting.");
                return false;
            }
            Debug.WriteLine("New service connection.");
            _connection = triggerDetails.AppServiceConnection;
            _connection.RequestReceived += OnRequestReceived;

            return true;
        }

        private void OnTaskCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
           Debug.WriteLine($"Cancellation, reason: {reason}.");
           if (_deferral != null)
           {
               _deferral.Complete();
           }
        }

        private async void OnRequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            // Use args.GetDeferral() (and release the deferral later) if args is to be used after awaited calls
            foreach (var element in args.Request.Message)
            {
                if (element.Value != null)
                {
                    _valueStorage[element.Key] = element.Value;
                }
            }
        }
    }
}
```

To allow applications to query the current value of a given key, we will treat a key with a `null` value as a query operation instead of a set operation. `OnRequestReceived` is changed to:

```cs
private async void OnRequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
{
    // Use args.GetDeferral() (and release the deferral later) if args is to be used after awaited calls
    var requestedValues = new ValueSet();
    var values = new ValueSet();
    foreach (var element in args.Request.Message)
    {
        if (element.Value != null)
        {
            _valueStorage[element.Key] = element.Value;
            values.Add(element.Key, element.Value);
        }
        else
        {
            var key = element.Key;
            _valueStorage.TryGetValue(key, out object value);
            requestedValues.Add(key, value);
        }
    }
    if (values.Count != 0)
    {
        // Broadcast to all applications that these values were updated.
    }
    if (requestedValues.Count != 0)
    {
        // Send the current value (or null) back to the application that requested it.
        await _connection.SendMessageAsync(requestedValues);
    }
}
```

To broadcast changes to all applications, we'll have a delegate that calls `SendMessageAsync` on all instances of the app service:

* Create a delegate that receives a `ValueSet` as its single argument and a static instance of it:

```cs
private delegate void ValueChangedHandler(ValueSet args);
private static ValueChangedHandler ValueChanged;
```

* Create a function with the delegate's signature that sends the message on the current app service instance:

```cs
private async void BroadcastReceivedMessage(ValueSet changedValues)
{
    await _connection.SendMessageAsync(changedValues);
}
```

* Add `ValueChanged += BroadcastReceivedMessage;` to the `SetupConnection` method and `ValueChanged -= BroadcastReceivedMessage;` to `OnTaskCanceled`;
* Call `ValueChanged?.Invoke(values);` whenever you need to broadcast an update to all app service instances.

[The full code for the app service `StartupTask`, including broadcast of updates, is available here.](https://github.com/ms-iot/iot-walkthrough/blob/master/CS/ShowcaseAppService/StartupTask.cs)
