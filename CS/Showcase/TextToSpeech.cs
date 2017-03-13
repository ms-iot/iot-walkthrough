using System;
using System.Diagnostics;
using Windows.Media.SpeechSynthesis;
using Windows.UI.Xaml.Controls;

namespace Showcase
{
    class TextToSpeech
    {
        private SpeechSynthesizer _synthesizer;
        private string _text;

        public TextToSpeech(string text)
        {
            if (SpeechSynthesizer.AllVoices.Count == 0)
            {
                Debug.WriteLine("No voice packages available, skipping text-to-speech");
            }
            _text = text;
            _synthesizer = new SpeechSynthesizer();
        }

        public async void Play()
        {
            if (_synthesizer == null)
            {
                return;
            }
            var stream = await _synthesizer.SynthesizeTextToStreamAsync(_text);

            var speechElement = new MediaElement();
            speechElement.SetSource(stream, stream.ContentType);
            speechElement.Play();
        }
    }
}
