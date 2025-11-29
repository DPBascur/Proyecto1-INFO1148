using System;
using CommunityToolkit.Mvvm.ComponentModel;
using ProyectoFinal.Models;

namespace ProyectoFinal.ViewModels;

/// <summary>
/// ViewModel para mostrar un caso de prueba en el DataGrid.
/// Expone las propiedades necesarias para la visualización de evidencia.
/// </summary>
public partial class CasoPruebaViewModel : ObservableObject
{
    [ObservableProperty]
    private string _id = string.Empty;

    [ObservableProperty]
    private string _cadena = string.Empty;

    [ObservableProperty]
    private string _categoria = string.Empty;

    [ObservableProperty]
    private string _descripcion = string.Empty;

    [ObservableProperty]
    private int _longitud = 0;

    [ObservableProperty]
    private int _profundidad = 0;

    [ObservableProperty]
    private int _numeroOperadores = 0;

    [ObservableProperty]
    private int _numeroTerminales = 0;

    [ObservableProperty]
    private string _colorCategoria = "#95A5A6";

    public CasoPruebaViewModel()
    {
    }

    public CasoPruebaViewModel(CasoPrueba caso)
    {
        Id = caso.Id;
        Cadena = caso.Cadena;
        Categoria = ObtenerEtiquetaCategoria(caso.Categoria);
        Descripcion = caso.Descripcion;

        // Extraer métricas de metadata
        if (caso.Metadata.ContainsKey("num_tokens"))
        {
            Longitud = Convert.ToInt32(caso.Metadata["num_tokens"]);
        }

        if (caso.Metadata.ContainsKey("profundidad"))
        {
            Profundidad = Convert.ToInt32(caso.Metadata["profundidad"]);
        }

        if (caso.Metadata.ContainsKey("total_operadores"))
        {
            NumeroOperadores = Convert.ToInt32(caso.Metadata["total_operadores"]);
        }

        if (caso.Metadata.ContainsKey("num_terminales"))
        {
            NumeroTerminales = Convert.ToInt32(caso.Metadata["num_terminales"]);
        }

        // Asignar color según categoría
        ColorCategoria = ObtenerColorCategoria(caso.Categoria);
    }

    /// <summary>
    /// Obtiene la etiqueta de categoría según el formato requerido (valid, invalid, extreme).
    /// </summary>
    private string ObtenerEtiquetaCategoria(CategoriaCaso categoria)
    {
        return categoria switch
        {
            CategoriaCaso.Valido => "VALIDO",
            CategoriaCaso.Invalido => "INVALIDO",
            CategoriaCaso.Extremo => "EXTREMO",
            _ => "DESCONOCIDO"
        };
    }

    /// <summary>
    /// Obtiene el color asociado a la categoría para visualización.
    /// </summary>
    private string ObtenerColorCategoria(CategoriaCaso categoria)
    {
        return categoria switch
        {
            CategoriaCaso.Valido => "#27AE60",      // Verde
            CategoriaCaso.Invalido => "#E74C3C",    // Rojo
            CategoriaCaso.Extremo => "#F39C12",     // Naranja
            _ => "#95A5A6"                          // Gris
        };
    }
}
