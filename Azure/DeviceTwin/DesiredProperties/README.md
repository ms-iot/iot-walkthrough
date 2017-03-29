---
---
# Saving application keys on Azure

## Introduction

Keys to access external services (eg. Bing news and OpenWeatherMap) should not be hardcoded in the device. We will save them in Azure and receive them during runtime using the Azure Device Twin.

The Azure connection is managed by the background app and many keys are used by the foreground app; thus, the app service will be used to communicate key updates.

## Device Twin

The Azure Device Twin will be used for storage of keys. The Device Twin works like a cloud representation of the physical device. The back end is allowed to update device properties in the cloud (called Desired Properties) and the device will be informed of the update if it's connected or see the update whenever it powers up. [Documentation is available here.](https://docs.microsoft.com/en-us/azure/iot-hub/iot-hub-devguide-device-twins)

## Listening for changes

To listen for changes, call `SetDesiredPropertyUpdateCallback` at the `DeviceClient` instance. The callback will be called whenever a property is updated. To retrieve the initial state of the whole twin, call `GetTwinAsync` at the `DeviceClient` instance.

For example, after connecting to Azure (using the device ID and key or using TPM tokens), the following sets a callback for updates and calls the callback with the initial state:

```cs
await _deviceClient.SetDesiredPropertyUpdateCallback(OnDesiredPropertyChanged, null);
var twin = await _deviceClient.GetTwinAsync();
await OnDesiredPropertyChanged(twin.Properties.Desired, null);
```

The `OnDesiredPropertyChanged` function must send the property changes through the app service. In this example, we will only support JSON types that can be converted to strings, meaning nested types (objects and arrays) won't be supported:

```cs
private async Task OnDesiredPropertyChanged(TwinCollection desiredProperties, object userContext)
{
    ValueSet properties = new ValueSet
    {
        ["version"] = desiredProperties.Version
    };
    foreach (var prop in desiredProperties)
    {
        var pair = (KeyValuePair<string, object>)prop;
        var value = pair.Value as JValue;
        if (value == null)
        {
            Debug.WriteLine("Twin key " + pair.Key + " has unsupported type");
            continue;
        }
        properties.Add(pair.Key, pair.Value.ToString());
    }
    await AppServiceBridge.SendMessageAsync(properties);
}
```

**Note:** There is a race condition in the above code if the `version` field is not sent through the app service. If some property is updated after the `GetTwinAsync` call and before the `twin.Properties.Desired` properties are advertised, the applications connected to the app service would first receive the updated value and then the old value, in the wrong order. Connected applications should check the `version` field before using the value.

Applications connected to the app service should listen for app service requests and act upon it. For example, to use keys saved on the Device Twin to fetch news from Bing, the following code can be added to `BingNews.cs`:

```cs
class BingNews
{
    private string _key;
    private long _keyVersion = -1;
    private object _keyVersionLock = new object();

    public BingNews()
    {
        // Install event handler on constructor.
        AppServiceBridge.RequestReceived += PropertyUpdate;
        // Request the current value of bingKey. If available, the response will have no version information, since we requested only the bingKey key.
        AppServiceBridge.RequestUpdate("bingKey");
    }

    private void PropertyUpdate(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
    {
        // Check if bingKey is contained in the message.
        args.Request.Message.TryGetValue("bingKey", out object key);
        if (key != null)
        {
            lock (_keyVersionLock)
            {
                if (args.Request.Message.TryGetValue("version", out object version))
                {
                    var receivedVersion = (long)version;
                    if (receivedVersion >= _keyVersion)
                    {
                        _keyVersion = receivedVersion;
                    }
                }
                else if (_keyVersion != -1)
                {
                    // Do nothing if we have already received a key with version information
                    // and the newer one has no version.
                    return;
                }
                _key = (string)key;
            }
            // Run a timer to update news.
            InitTimer();
        }
    }
}
```

[The full `BingNews.cs` class is available here.](https://github.com/ms-iot/iot-walkthrough/blob/master/CS/Showcase/BingNews.cs)
