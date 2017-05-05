---
---
# Usage of text-to-speech

## Introduction

Text-to-speech and speech recognition will be used for operation without a keyboard and mouse. We will use text-to-speech for short sentences to give the user quick information.

## Text-to-speech scenarios

In the showcase application, text-to-speech will be used when:

* The application launches, to give a "Welcome" message to the user;
* The board is listening to commands, to let the user know that speech is being recorded;
* We couldn't understand the user's speech and need to give an error message.

## Text-to-speech code

The `SpeechSynthesizer` class is part of UWP. To create an audio stream, call `SynthesizeTextToStreamAsync` with a string. Options for voices can be optionally set, but we'll use the default system voice. After creating the stream, it can be played with a `MediaElement`.

Create a `TextToSpeech` class inside the foreground application:

```cs
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
```

The `MainPage.xaml.cs` file will have a line to say "Welcome" at startup:

```cs
private async void OnLoaded(object sender, RoutedEventArgs e)
{
    // ...
    new TextToSpeech("Welcome").Play();
    // ...
}
```

The `TextToSpeech` class will also be used in the next tutorial, [Receiving voice commands](../VoiceCommands/README.md).
