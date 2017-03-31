using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.Media.SpeechRecognition;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace Showcase
{
    class VoiceCommand
    {
        private const int LED_PIN = 21;
        private const int BUTTON_PIN = 35;

        private static SpeechRecognizer _speechRecognizer;
        private static Dictionary<String, RoutedEventHandler> _commandCallback = new Dictionary<string, RoutedEventHandler>();
        private static GpioPin _led, _button;
        private static CoreDispatcher _uiDispatcher;
        private static bool _running;

        public void Init(CoreDispatcher uiDispatcher)
        {
            _uiDispatcher = uiDispatcher;

            _speechRecognizer = new SpeechRecognizer();
            _speechRecognizer.Timeouts.BabbleTimeout = TimeSpan.FromSeconds(5);
            _speechRecognizer.Timeouts.EndSilenceTimeout = TimeSpan.FromSeconds(2);

            InitGPIO();
        }

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
                else if (result.Text == "Help")
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
}
