using SharpSpeech;

namespace DemoConsole1;

internal class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Loading wake word..");
        WakeWordRecognition wakeWordRecognition = new WakeWordRecognition(["Hey"], "./ggml.bin");
        wakeWordRecognition.Recognized += (wakeWord) =>
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Detected: '" + wakeWord + "'");
            Console.ResetColor();

            wakeWordRecognition.Stop();
        };
        Console.WriteLine("Listening..");
        wakeWordRecognition.Listen();
        Console.WriteLine("Stopped wake word");
        Console.WriteLine("Loading speech recognition..");
        SpeechRecognition speechRecognition = new SpeechRecognition("./ggml.bin");
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