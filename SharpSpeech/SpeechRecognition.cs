using System;
using SharperPortAudio;
using Whisper.net.Ggml;

namespace SharpSpeech;

public class SpeechRecognition
{
    private AudioRecorder? recorder;
    private bool running = false;

    public RecognitionChange? RecognitionChanged;

    public bool Running => running;
    public bool AutoStop { get; set; } = true;

    public SpeechRecognition()
    { }
    public SpeechRecognition(string modelFile, string language = "en", GgmlType modelType = GgmlType.Base)
    {
        Whisper.Initialize(modelFile, language, modelType);
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
        if (running) throw new Exception("SpeechRecognition is already listening");
        running = true;
        recorder = new AudioRecorder(inputDevice, 16000)
        {
            //FramesPerBuffer = 12288,
            FramesPerBuffer = 24576,
            SendBufferInsteadOfChunk = false,
        };
        recorder.Start();
        if (RecognitionChanged != null)
            recorder.DataReceived += RecordedSegment;
    }
    public async Task<string[]> Stop()
    {
        if (!running) throw new Exception("SpeechRecognition is already stopped");
        Audio audio = recorder?.Stop()!;
        running = false;
        return await Process(audio);
    }

    private async void RecordedSegment(Audio audio)
    {
        string[] text = await Process(audio);
        if (text.Length == 0) return;
        RecognitionChanged!(text);
        if (AutoStop)
            if (text.Last().Contains("[BLANK_AUDIO]")) await Stop();
    }

    private async Task<string[]> Process(Audio audio)
    {
        byte[] bytes = audio.ToPCMWave();
        List<string> segments = await Whisper.Process(bytes);
        return segments.ToArray();
    }

    public delegate void RecognitionChange(string[] text);
}