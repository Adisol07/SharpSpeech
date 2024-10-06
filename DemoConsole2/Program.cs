using System;
using SharpSpeech;

namespace DemoConsole2;

class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Loading speech recognition..");
        SpeechRecognition speechRecognition = new SpeechRecognition("./ggml.bin", "cs", Whisper.net.Ggml.GgmlType.Small);
        speechRecognition.RecognitionChanged += (text) =>
        {
            Console.ForegroundColor = ConsoleColor.Green;
            foreach (string txt in text)
                Console.WriteLine(txt);
            Console.ResetColor();
        };
        Console.WriteLine("Listening..");
        speechRecognition.Listen();
        Console.WriteLine("Stopped speech recognition");
    }
}