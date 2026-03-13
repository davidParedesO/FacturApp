using FacturApp.Config;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Extensions.Logging;

namespace FacturApp.Services;

public interface IAzureSpeechService
{
    Task<string> TranscribeAsync(Stream audioStream, string audioFormat = "wav", CancellationToken ct = default);
}

public class AzureSpeechService(ILogger<AzureSpeechService> logger, AppSettingsService settings) : IAzureSpeechService
{
    public async Task<string> TranscribeAsync(Stream audioStream, string audioFormat = "wav", CancellationToken ct = default)
    {
        logger.LogInformation("Iniciando transcripción de audio...");

        var tempFile = Path.GetTempFileName() + "." + audioFormat;
        try
        {
            await using (var fs = File.Create(tempFile))
                await audioStream.CopyToAsync(fs, ct);

            var speechConfig = SpeechConfig.FromSubscription(settings.SpeechKey, settings.SpeechRegion);
            speechConfig.SpeechRecognitionLanguage = "es-ES";

            using var audioConfig = AudioConfig.FromWavFileInput(tempFile);
            using var recognizer = new SpeechRecognizer(speechConfig, audioConfig);

            var result = await recognizer.RecognizeOnceAsync();

            return result.Reason switch
            {
                ResultReason.RecognizedSpeech => result.Text,
                ResultReason.NoMatch => throw new Exception("No se reconoció ningún audio."),
                ResultReason.Canceled => throw new Exception($"Speech cancelado: {CancellationDetails.FromResult(result).ErrorDetails}"),
                _ => throw new Exception("Error desconocido en Azure Speech.")
            };
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }
}
