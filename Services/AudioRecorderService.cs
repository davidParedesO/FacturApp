using Microsoft.Extensions.Logging;
using Plugin.Maui.Audio;

namespace FacturApp.Services;

/// <summary>
/// Encapsula la grabación de audio del micrófono multiplataforma (Windows + Android)
/// </summary>
public class AudioRecorderService(IAudioManager audioManager, ILogger<AudioRecorderService> logger)
{
    private IAudioRecorder? _recorder;
    private bool _isRecording;

    public bool IsRecording => _isRecording;

    public async Task<bool> StartRecordingAsync()
    {
        try
        {
            // Verificar permiso de micrófono
            var status = await Permissions.RequestAsync<Permissions.Microphone>();
            if (status != PermissionStatus.Granted)
            {
                logger.LogWarning("Permiso de micrófono denegado.");
                return false;
            }

            _recorder = audioManager.CreateRecorder();
            await _recorder.StartAsync();
            _isRecording = true;
            logger.LogInformation("Grabación iniciada.");
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error iniciando grabación.");
            _isRecording = false;
            return false;
        }
    }

    public async Task<byte[]?> StopRecordingAsync()
    {
        if (!_isRecording || _recorder == null)
            return null;

        try
        {
            var audio = await _recorder.StopAsync();
            _isRecording = false;
            logger.LogInformation("Grabación finalizada.");

            // Leer los bytes del audio grabado
            await using var stream = audio.GetAudioStream();
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            return ms.ToArray();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deteniendo grabación.");
            _isRecording = false;
            return null;
        }
    }
}
