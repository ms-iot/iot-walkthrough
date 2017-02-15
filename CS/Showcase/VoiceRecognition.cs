using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Media.SpeechRecognition;
using Windows.UI.Core;

namespace Showcase
{
    class VoiceRecognition
    {
        public const String PLAY_COMMAND = "Play";
        public const String PAUSE_COMMAND = "Pause";
        public const String SLIDESHOW_COMMAND = "Start slideshow";
        public delegate void RecognitionCallback(String sentence);
        public RecognitionCallback recognitionCallback;
        private readonly SpeechRecognitionListConstraint validWords = new SpeechRecognitionListConstraint(new String[] { PLAY_COMMAND, PAUSE_COMMAND, SLIDESHOW_COMMAND });
        private SpeechRecognizer speechRecognizer;

        public VoiceRecognition()
        {
            speechRecognizer = new SpeechRecognizer();
            CompileConstrains();

            speechRecognizer.UIOptions.AudiblePrompt = "Say a command";
            speechRecognizer.UIOptions.ExampleText = "Choose Play, Pause or Start slideshow";
            speechRecognizer.UIOptions.IsReadBackEnabled = true;
            speechRecognizer.UIOptions.ShowConfirmation = true;

            speechRecognizer.Timeouts.InitialSilenceTimeout = TimeSpan.FromSeconds(5);
            speechRecognizer.Timeouts.BabbleTimeout = TimeSpan.FromSeconds(10);
            speechRecognizer.Timeouts.EndSilenceTimeout = TimeSpan.FromSeconds(3);
        }

        public async void CompileConstrains()
        {
            speechRecognizer.Constraints.Add(validWords);
            await speechRecognizer.CompileConstraintsAsync();
        }

        public async Task RunVoiceRecognition(CoreDispatcher uiThreadDispatcher)
        {
            await uiThreadDispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                Debug.WriteLine("Running dispatcher");
                SpeechRecognitionResult result = await speechRecognizer.RecognizeWithUIAsync();
                Debug.WriteLine("Recognition finished");
                recognitionCallback(result.Text);
            });
            Debug.WriteLine("Dispatched");
        }
    }
}
