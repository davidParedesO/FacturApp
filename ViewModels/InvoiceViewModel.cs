using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using FacturApp.DTOs;
using FacturApp.Services;

namespace FacturApp.ViewModels;

public class ChatMessage
{
    public string Role { get; set; } = string.Empty; // "User" o "IA"
    public string Text { get; set; } = string.Empty;
    public byte[]? AudioBytes { get; set; }
    public FacturaDto? Factura { get; set; }
    public bool IsLoading { get; set; }
}

/// <summary>
/// ViewModel - pantalla de chat para generación de facturas (voz y texto).
/// Inyecta InvoiceFlowService.
/// </summary>
public class InvoiceViewModel : INotifyPropertyChanged
{
    private readonly AudioRecorderService _audioService;
    private readonly InvoiceFlowService _flowService;

    public ObservableCollection<ChatMessage> Messages { get; } = new();

    private string _textInput = string.Empty;
    public string TextInput { get => _textInput; set { _textInput = value; Notify(); Notify(nameof(CanSendText)); } }

    private bool _isRecording;
    public bool IsRecording { get => _isRecording; private set { _isRecording = value; Notify(); Notify(nameof(CanRecord)); } }

    private bool _isLoading;
    public bool IsLoading { get => _isLoading; private set { _isLoading = value; Notify(); Notify(nameof(CanRecord)); Notify(nameof(CanSendText)); } }

    private string _status = "Escribe un mensaje o pulsa el micrófono";
    public string StatusMessage { get => _status; private set { _status = value; Notify(); } }

    private string? _error;
    public string? ErrorMessage { get => _error; private set { _error = value; Notify(); Notify(nameof(HasError)); } }

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public bool CanRecord => !IsLoading;
    public bool CanSendText => !IsLoading && !string.IsNullOrWhiteSpace(TextInput);

    public Action? StateChanged { get; set; }

    public InvoiceViewModel(AudioRecorderService audioService, InvoiceFlowService flowService)
    {
        _audioService = audioService;
        _flowService = flowService;
    }

    public async Task SendMessageAsync()
    {
        if (!CanSendText) return;
        
        var textoMensaje = TextInput.Trim();
        TextInput = string.Empty;
        ErrorMessage = null;
        
        Messages.Add(new ChatMessage { Role = "User", Text = textoMensaje });
        
        var aiMessage = new ChatMessage { Role = "IA", IsLoading = true };
        Messages.Add(aiMessage);
        
        IsLoading = true;
        StatusMessage = "Generando factura...";
        StateChanged?.Invoke();
        
        try
        {
            var history = Messages.Where(m => !m.IsLoading && !string.IsNullOrEmpty(m.Text))
                                  .Select(m => new ChatTurn(m.Role, m.Text)).ToList();
                                  
            var (textReply, factura, error) = await _flowService.ProcessChatAsync(history);
            aiMessage.IsLoading = false;
            
            if (error != null)
            {
                ErrorMessage = error;
                aiMessage.Text = "Hubo un error al procesar tu mensaje.";
                StatusMessage = "Error";
            }
            else if (factura != null)
            {
                aiMessage.Factura = factura;
                aiMessage.Text = $"¡Factura {factura.NumeroFactura} generada exitosamente!";
                StatusMessage = "Esperando nueva solicitud";
            }
            else if (textReply != null)
            {
                aiMessage.Text = textReply;
                StatusMessage = "Esperando nueva solicitud";
            }
            else
            {
                aiMessage.Text = "No se pudo extraer información.";
                StatusMessage = "Esperando nueva solicitud";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error: {ex.Message}";
            aiMessage.IsLoading = false;
            aiMessage.Text = "Ocurrió una excepción al atender la solicitud.";
        }
        finally
        {
            IsLoading = false;
            StateChanged?.Invoke();
        }
    }

    public async Task ToggleRecordingAsync()
    {
        if (IsRecording) await StopAndProcessAsync();
        else await StartRecordingAsync();
    }

    private async Task StartRecordingAsync()
    {
        ErrorMessage = null;
        var ok = await _audioService.StartRecordingAsync();
        if (ok) { IsRecording = true; StatusMessage = "Grabando... Pulsa de nuevo para detener"; }
        else { ErrorMessage = "No se pudo iniciar la grabación. Verifica permisos."; StatusMessage = "Error al grabar"; }
        StateChanged?.Invoke();
    }

    private async Task StopAndProcessAsync()
    {
        IsRecording = false; IsLoading = true;
        StatusMessage = "Procesando audio..."; StateChanged?.Invoke();
        try
        {
            var audioBytes = await _audioService.StopRecordingAsync();
            if (audioBytes == null || audioBytes.Length == 0)
            { ErrorMessage = "No se capturó audio."; StatusMessage = "Sin audio"; return; }

            // Añadir el mensaje de audio del usuario a la UI inmediatamente
            Messages.Add(new ChatMessage { Role = "User", AudioBytes = audioBytes });
            
            var aiMessage = new ChatMessage { Role = "IA", IsLoading = true };
            Messages.Add(aiMessage);

            StatusMessage = "Transcribiendo..."; StateChanged?.Invoke();

            var (transcripcion, error) = await _flowService.TranscribeAudioAsync(audioBytes, "grabacion.wav");
            
            if (error != null) { 
                ErrorMessage = error; 
                aiMessage.IsLoading = false;
                aiMessage.Text = $"Error transcribiendo: {error}\n({transcripcion})";
                StatusMessage = "Error procesando"; 
                return; 
            }
            
            // Add transcription to the user's audio bubble so it's included in history
            var userMessage = Messages.LastOrDefault(m => m.Role == "User" && m.AudioBytes != null);
            if (userMessage != null) { userMessage.Text = transcripcion; }

            StatusMessage = "Analizando intención..."; StateChanged?.Invoke();

            var history = Messages.Where(m => !m.IsLoading && !string.IsNullOrEmpty(m.Text))
                                  .Select(m => new ChatTurn(m.Role, m.Text)).ToList();
                                  
            var (textReply, factura, chatError) = await _flowService.ProcessChatAsync(history);
            
            aiMessage.IsLoading = false;
            
            if (chatError != null) { 
                ErrorMessage = chatError; 
                aiMessage.Text = $"Error de IA: {chatError}";
                StatusMessage = "Error procesando chat"; 
                return; 
            }
            
            if (factura != null)
            {
                aiMessage.Factura = factura;
                aiMessage.Text = $"¡Factura {factura.NumeroFactura} generada!\nHemos entendido:\n\"{transcripcion}\"";
            }
            else if (textReply != null)
            {
                aiMessage.Text = $"\"{transcripcion}\"\n\n{textReply}";
            }
            else
            {
                aiMessage.Text = $"Detectado: \"{transcripcion}\" pero no se pudo generar.";
            }

            StatusMessage = "Esperando nueva solicitud";
        }
        catch (Exception ex) { ErrorMessage = $"Error: {ex.Message}"; StatusMessage = "Error de proceso"; }
        finally { IsLoading = false; StateChanged?.Invoke(); }
    }

    public void Reset()
    {
        Messages.Clear();
        ErrorMessage = null; TextInput = string.Empty;
        StatusMessage = "Escribe un mensaje o pulsa el micrófono";
        StateChanged?.Invoke();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void Notify([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
