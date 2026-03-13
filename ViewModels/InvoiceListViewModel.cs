using System.ComponentModel;
using System.Runtime.CompilerServices;
using FacturApp.DTOs;
using FacturApp.Services;

namespace FacturApp.ViewModels;

public class InvoiceListViewModel : INotifyPropertyChanged
{
    private readonly InvoiceFlowService _flowService;

    private bool _isLoading;
    public bool IsLoading { get => _isLoading; private set { _isLoading = value; Notify(); } }

    private List<FacturaDto> _facturas = [];
    public List<FacturaDto> Facturas { get => _facturas; private set { _facturas = value; Notify(); Notify(nameof(HasFacturas)); } }

    private string? _error;
    public string? ErrorMessage { get => _error; private set { _error = value; Notify(); } }

    private string _filtroOrigen = "TODOS";
    public string FiltroOrigen
    {
        get => _filtroOrigen;
        set { _filtroOrigen = value; Notify(); AplicarFiltro(); }
    }

    private List<FacturaDto> _todas = [];
    public bool HasFacturas => Facturas.Count > 0;
    public Action? StateChanged { get; set; }

    public InvoiceListViewModel(InvoiceFlowService flowService) => _flowService = flowService;

    public async Task LoadAsync()
    {
        IsLoading = true; ErrorMessage = null; StateChanged?.Invoke();
        try
        {
            _todas = await _flowService.GetFacturasAsync();
            AplicarFiltro();
        }
        catch (Exception ex) { ErrorMessage = $"Error: {ex.Message}"; }
        finally { IsLoading = false; StateChanged?.Invoke(); }
    }

    private void AplicarFiltro()
    {
        Facturas = FiltroOrigen == "TODOS"
            ? [.. _todas]
            : [.. _todas.Where(f => f.Origen == FiltroOrigen)];
        StateChanged?.Invoke();
    }

    public async Task<byte[]?> DownloadPdfAsync(int id) =>
        await _flowService.GetPdfBytesAsync(id);

    public event PropertyChangedEventHandler? PropertyChanged;
    private void Notify([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
