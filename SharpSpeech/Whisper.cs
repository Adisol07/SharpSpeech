using Whisper.net;
using Whisper.net.Ggml;

namespace SharpSpeech;

public static class Whisper
{
    private static WhisperProcessor? processor;
    private static bool initialized = false;

    public static string? ModelFile { get; private set; } = "";

    public static void Initialize(string modelFile, string language = "auto")
    {
        if (initialized) return;

        ModelFile = modelFile;
        DownloadModel(modelFile, GgmlType.Base);
        WhisperFactory whisperFactory = WhisperFactory.FromPath(ModelFile!);
        processor = whisperFactory.CreateBuilder()
            .WithLanguage(language)
            .WithPrintResults()
            .Build();
        initialized = true;
    }

    public static async Task<List<string>> Process(byte[] soundData)
    {
        if (processor == null)
            throw new InvalidOperationException("Whisper processor isn't initialized! Call Initialize method first.");

        await DisposeProcessor();
        Initialize(ModelFile!, "en");

        using Stream stream = new MemoryStream(soundData);

        List<string> segments = new List<string>();
        await foreach (SegmentData result in processor!.ProcessAsync(stream))
        {
            segments.Add(result.Text);
        }
        return segments;
    }

    public static async Task Close() => await Dispose();
    public static async Task Dispose()
    {
        await DisposeProcessor();
        ModelFile = null;
    }
    public static async Task DisposeProcessor()
    {
        initialized = false;
        await processor!.DisposeAsync();
        processor = null;
    }

    public static void DownloadModel(string modelFile, GgmlType type)
    {
        if (!File.Exists(modelFile))
        {
            using var modelStream = WhisperGgmlDownloader.GetGgmlModelAsync(type).Result;
            using var fileWriter = File.OpenWrite(modelFile);
            modelStream.CopyTo(fileWriter);
        }
    }
}
