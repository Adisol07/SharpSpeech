using System;
using SharperPortAudio;

namespace SharpSpeech;

public class WakeWordRecognition
{
    private AudioRecorder? recorder;
    private bool running = false;

    public Waked? Recognized;

    public bool Running => running;
    public List<string> WakeWords { get; set; }
    public bool OneToOneMapping { get; set; } = false;

    public WakeWordRecognition(params string[] wakeWords)
    {
        WakeWords = wakeWords.ToList();
    }
    public WakeWordRecognition(string[] wakeWords, string modelFile)
    {
        WakeWords = wakeWords.ToList();
        Whisper.Initialize(modelFile, "en");
    }

    public void Listen() => Listen(Device.DefaultInputDevice);
    public void Listen(Device inputDevice)
    {
        Start(inputDevice);
        while (Running) Task.Delay(1);
    }

    public void Start() => Start(Device.DefaultInputDevice);
    public void Start(Device inputDevice)
    {
        if (running) throw new Exception("WakeWordRecognition is already listening");
        running = true;
        recorder = new AudioRecorder(inputDevice, 16000)
        {
            //FramesPerBuffer = 12288,
            FramesPerBuffer = 24576,
            SendBufferInsteadOfChunk = false,
        };
        recorder.Start();
        recorder.DataReceived += RecordedSegment;
    }
    public void Stop()
    {
        if (!running) throw new Exception("WakeWordRecognition is already stopped");
        recorder?.Stop();
        running = false;
    }

    private async void RecordedSegment(Audio audio)
    {
        byte[] bytes = audio.ToPCMWave();
        List<string> segments = await Whisper.Process(bytes);
        foreach (string segment in segments)
        {
            string seg = segment.TrimStart().TrimEnd().ToLower();
            if (!OneToOneMapping)
                seg = seg.Replace(",", "").Replace(".", "");

            foreach (string wakeWord in WakeWords)
            {
                string ww = wakeWord.TrimStart().TrimEnd().ToLower();
                if (!OneToOneMapping)
                    ww = ww.Replace(",", "").Replace(".", "");

                if (seg.Contains(ww))
                {
                    if (Recognized != null) Recognized(wakeWord);
                    return;
                }
            }
        }
    }

    public delegate void Waked(string wakeWord);
}