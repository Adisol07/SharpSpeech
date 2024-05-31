using System;
using NAudio.Wave;
using SharperPortAudio;

namespace SharpSpeech;

public static class AudioExtensions
{
    public static byte[] ToPCMWave(this Audio audio)
    {
        byte[] pcm16BitSamples = ConvertFloatToPcm16(audio.Samples!);

        WaveFormat format = new WaveFormat(audio.SampleRate, 16, audio.Channels);
        using MemoryStream memoryStream = new MemoryStream();
        using (WaveFileWriter waveFileWriter = new WaveFileWriter(memoryStream, format))
        {
            waveFileWriter.Write(pcm16BitSamples, 0, pcm16BitSamples.Length);
        }

        return memoryStream.ToArray();
    }

    private static byte[] ConvertFloatToPcm16(float[] floatSamples)
    {
        byte[] pcm16BitSamples = new byte[floatSamples.Length * 2];
        for (int i = 0; i < floatSamples.Length; i++)
        {
            float sample = Math.Clamp(floatSamples[i], -1.0f, 1.0f);
            short intSample = (short)(sample * short.MaxValue);
            pcm16BitSamples[i * 2] = (byte)(intSample & 0xff);
            pcm16BitSamples[i * 2 + 1] = (byte)((intSample >> 8) & 0xff);
        }
        return pcm16BitSamples;
    }
}