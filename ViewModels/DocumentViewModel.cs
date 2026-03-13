using System.ComponentModel;
using System.Runtime.CompilerServices;
using FacturApp.DTOs;
using FacturApp.Services;

namespace FacturApp.ViewModels;

public class DocumentViewModel : INotifyPropertyChanged
{
    private readonly InvoiceFlowService _flowService;

    private bool _isLoading;
    public bool IsLoading { get => _isLoading; private set { _isLoading = value; Notify(); } }

    private string _status = "Selecciona un documento (PDF, JPG, PNG) para procesar";
    public string StatusMessage { get => _status; private set { _status = value; Notify(); } }

    private string? _archivoNombre;
    public string? ArchivoNombre { get => _archivoNombre; private set { _archivoNombre = value; Notify(); Notify(nameof(HasArchivo)); } }

    private FacturaDto? _factura;
    public FacturaDto? FacturaGenerada { get => _factura; private set { _factura = value; Notify(); Notify(nameof(HasFactura)); } }

    private string? _error;
    public string? ErrorMessage { get => _error; private set { _error = value; Notify(); Notify(nameof(HasError)); } }

    public bool HasArchivo => !string.IsNullOrEmpty(ArchivoNombre);
    public bool HasFactura => FacturaGenerada != null;
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public Action? StateChanged { get; set; }

    private byte[]? _bytes;
    private string? _contentType;

    public DocumentViewModel(InvoiceFlowService flowService) => _flowService = flowService;

    public void SetArchivo(byte[] bytes, string nombre, string contentType)
    {
        _bytes = bytes; _contentType = contentType; ArchivoNombre = nombre;
        ErrorMessage = null; FacturaGenerada = null;
        StatusMessage = $"Archivo cargado: {nombre}";
        StateChanged?.Invoke();
    }

    public async Task ProcessDocumentAsync()
    {
        if (_bytes == null) { ErrorMessage = "Selecciona un archivo primero."; StateChanged?.Invoke(); return; }
        IsLoading = true; ErrorMessage = null;
        StatusMessage = "Enviando a Azure Document Intelligence..."; StateChanged?.Invoke();
        try
        {
            var (factura, error) = await _flowService.ProcessDocumentAsync(_bytes, ArchivoNombre!, _contentType ?? "application/pdf");
            if (error != null) { ErrorMessage = error; StatusMessage = "Error en el procesamiento"; return; }
            FacturaGenerada = factura;
            StatusMessage = factura != null ? $"✅ Factura {factura.NumeroFactura} extraída" : "No se detectó factura";
        }
        catch (Exception ex) { ErrorMessage = $"Error: {ex.Message}"; StatusMessage = "Error"; }
        finally { IsLoading = false; StateChanged?.Invoke(); }
    }

    public void Reset()
    {
        _bytes = null; _contentType = null; ArchivoNombre = null;
        FacturaGenerada = null; ErrorMessage = null;
        StatusMessage = "Selecciona un documento (PDF, JPG, PNG) para procesar";
        StateChanged?.Invoke();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void Notify([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
