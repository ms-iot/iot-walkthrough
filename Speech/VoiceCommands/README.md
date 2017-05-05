---
---
# Receiving voice commands

## Introduction

Voice commands will allow for basic operation without a keyboard/mouse. We'll use a physical button to start voice recognition and a LED to warn the user that voice is being recorded. There will also be a clickable button on the navigation pane to start voice recognition. The button is connected externally to GPIO 35 and the LED is on GPIO 21 (the board's internal green LED will be used).

If the button is not desired, one can use [the continuous dictation API](https://docs.microsoft.com/en-us/windows/uwp/input-and-devices/enable-continuous-dictation) to continuously listen for user input. It won't be used in this walkthrough project.

## Initializing the GPIO

The `GpioController` class will be used to access the button and the LED. If we are running the application without a GPIO controller (e.g. testing on Desktop), we want to skip GPIO initialization and silently ignore LED commands. Create a `VoiceCommand` class and use this initialization code:

```cs
class VoiceCommand
{
    private const int LED_PIN = 21;
    private const int BUTTON_PIN = 35;

    private static GpioPin _led, _button;
    private static CoreDispatcher _uiDispatcher;

    public void Init(CoreDispatcher uiDispatcher)
    {
        _uiDispatcher = uiDispatcher;
        InitGPIO();
    }

    private void InitGPIO()
    {
        var gpio = GpioController.GetDefault();

        if (gpio == null)
        {
            // If testing on Desktop, ignore LED/buttons.
            Debug.WriteLine("No GPIO controller found");
            return;
        }

        _led = gpio.OpenPin(LED_PIN);
        SetLed(GpioPinValue.Low);
        _led.SetDriveMode(GpioPinDriveMode.Output);

        _button = gpio.OpenPin(BUTTON_PIN);
        _button.SetDriveMode(GpioPinDriveMode.InputPullDown);
        _button.ValueChanged += Button_ValueChanged;
    }

    private async void Button_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
    {
        if (sender.Read() == GpioPinValue.High)
        {
            await _uiDispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => { await RunVoiceRecognition(); });
        }
    }

    private void SetLed(GpioPinValue value)
    {
        if (_led != null)
        {
            _led.Write(value);
        }
    }
}
```

To enable the LED during voice recognition, call `SetLet`:

```cs
class VoiceCommand
{
    private static SpeechRecognizer _speechRecognizer;

    public async Task RunVoiceRecognition()
    {
        lock (_speechRecognizer)
        {
            if (_running)
            {
                // Skip if user accidentally pressed the button twice.
                Debug.WriteLine("Skipping voice recognition: Already running.");
                return;
            }
            _running = true;
        }

        try
        {
            if (_commandCallback.Count == 0)
            {
                Debug.WriteLine("No voice commands available");
                new TextToSpeech("No voice commands available").Play();
                return;
            }

            // ...
            // Initialize speech...
            // ...
            SetLed(GpioPinValue.High);
            // ...
            // Record speech/run recognition...
            // ...
            SetLed(GpioPinValue.Low);

            // ...
            // Do callbacks of the selected option...
            // ...
        }
        finally
        {
            _running = false;
        }
    }
}
```

## Receiving voice commands

Before your app is able to access a microphone, a capability must be declared in your project.

* Double click the `Package.appxmanifest` file from your foreground application;
* Go to the *Capabilities* tab;
* Select *Microphone*;
* Save and close.

Or, alternatively:

* Select the `Package.appxmanifest` file from your foreground application and press *F7* (or *Right Click > View Code*);
* Add `<DeviceCapability Name="microphone" />` to the `Capabilities` block.
* Save and close.

We will use the UWP class `SpeechRecognizer` with a small dictionary of possible commands. If a user chooses a command, a callback will be called. The commands and callbacks will be kept in a `Dictionary<String, RoutedEventHandler>`.

* The following functions are added to the `VoiceCommand` class to add/remove callbacks:

```cs
class VoiceCommand
{
    private static Dictionary<String, RoutedEventHandler> _commandCallback = new Dictionary<string, RoutedEventHandler>();

    public void AddCommand(string command, RoutedEventHandler callback)
    {
        _commandCallback[command] = callback;
    }

    public void AddCommands(Dictionary<string, RoutedEventHandler> commands)
    {
        foreach (var pair in commands)
        {
            AddCommand(pair.Key, pair.Value);
        }
    }

    public void RemoveCommand(string command)
    {
        _commandCallback.Remove(command);
    }

    public void RemoveCommands(Dictionary<string, RoutedEventHandler> commands)
    {
        foreach (var key in commands.Keys)
        {
            RemoveCommand(key);
        }
    }
}
```

* The `SpeechRecognizer` is instantiated in the `Init` method. We also save a `CoreDispatcher` for the UI thread to be able to run voice recognition.

```cs
class VoiceCommand
{
    private static SpeechRecognizer _speechRecognizer;

    public void Init(CoreDispatcher uiDispatcher)
    {
        _uiDispatcher = uiDispatcher;

        _speechRecognizer = new SpeechRecognizer();
        _speechRecognizer.Timeouts.BabbleTimeout = TimeSpan.FromSeconds(5);
        _speechRecognizer.Timeouts.EndSilenceTimeout = TimeSpan.FromSeconds(2);

        InitGPIO();
    }
}
```

* Whenever speech recognition is triggered, we set the constraints to the keys in our dictionary and call `SpeechRecognizer.RecognizeAsync`. Then we call the callback or invoke a help text if recognition failed:

```cs
class VoiceCommand
{
    private static SpeechRecognizer _speechRecognizer;
    private static Dictionary<String, RoutedEventHandler> _commandCallback = new Dictionary<string, RoutedEventHandler>();
    private static bool _running;

    public async Task RunVoiceRecognition()
    {
        lock (_speechRecognizer)
        {
            if (_running)
            {
                Debug.WriteLine("Skipping voice recognition: Already running.");
                return;
            }
            _running = true;
        }

        try
        {
            if (_commandCallback.Count == 0)
            {
                Debug.WriteLine("No voice commands available");
                new TextToSpeech("No voice commands available").Play();
                return;
            }

            _speechRecognizer.Constraints.Clear();
            _speechRecognizer.Constraints.Add(new SpeechRecognitionListConstraint(_commandCallback.Keys));
            await _speechRecognizer.CompileConstraintsAsync();

            SetLed(GpioPinValue.High);
            new TextToSpeech("Listening").Play();
            SpeechRecognitionResult result = await _speechRecognizer.RecognizeAsync();
            SetLed(GpioPinValue.Low);

            if (result.Status != SpeechRecognitionResultStatus.Success || String.IsNullOrEmpty(result.Text))
            {
                Debug.WriteLine($"Recognition failed: {result.Status} - {result.Text}");
                ShowHelp("Sorry, didn't catch that.");
            }
            else if (String.IsNullOrEmpty(result.Text) || result.Text == "Help")
            {
                ShowHelp();
            }
            else if (_commandCallback.TryGetValue(result.Text, out RoutedEventHandler callback))
            {
                callback?.Invoke(this, null);
            }
        }
        finally
        {
            _running = false;
        }
    }

    private static void ShowHelp(string helpText = "")
    {
        new TextToSpeech($"{helpText} Choose one of {String.Join(", ", _commandCallback.Keys)}").Play();
    }
}
```

[The full `VoiceCommand.cs` file is available here.](https://github.com/ms-iot/iot-walkthrough/blob/master/CS/Showcase/VoiceCommand.cs)

## Adding voice commands to views

Adding commands with the `VoiceCommand` class is pretty easy:

* Have a dictionary of commands and callbacks. In the `MainPage`, for example, we add commands to navigate to pages and call the same callbacks used for button clicks:

```cs
_voiceCallbacks = new Dictionary<string, RoutedEventHandler>()
{
    { "Start slideShow", SlideShow_Click },
    { "Start media player", MediaPlayer_Click },
    { "Start WiFI connection", WiFiConnection_Click },
    { "Start news and weather", NewsAndWeather_Click },
};
```

* Call `_voiceCommand.AddCommands` on the `Loaded` callback;
* Call `_voiceCommand.RemoveCommands` on the `Unloaded` callback;

[The `MainPage.xaml.cs` file can be found here.](https://github.com/ms-iot/iot-walkthrough/blob/master/CS/Showcase/Views/MainPage.xaml.cs)
