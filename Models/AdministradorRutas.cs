using System;
using System.IO;

namespace ProyectoFinal.Models;

/// <summary>
/// Administrador de rutas de salida para archivos generados
/// Organiza los archivos en carpetas separadas en la raíz del proyecto
/// </summary>
public static class AdministradorRutas
{
    /// <summary>
    /// Obtiene la ruta raíz del proyecto (no la carpeta bin)
    /// </summary>
    private static string ObtenerRaizProyecto()
    {
        // Desde bin/Debug/net8.0 subir 3 niveles para llegar a la raíz del proyecto
        var directorioBase = AppDomain.CurrentDomain.BaseDirectory;
        var directorioRaiz = Directory.GetParent(directorioBase)?.Parent?.Parent?.FullName;
        
        if (directorioRaiz == null || !Directory.Exists(directorioRaiz))
        {
            // Fallback: crear en la carpeta actual
            directorioRaiz = directorioBase;
        }
        
        return directorioRaiz;
    }
    
    /// <summary>
    /// Obtiene la ruta para archivos de tokens
    /// </summary>
    public static string ObtenerRutaTokens()
    {
        var rutaTokens = Path.Combine(ObtenerRaizProyecto(), "Output", "Tokens");
        Directory.CreateDirectory(rutaTokens);
        return rutaTokens;
    }
    
    /// <summary>
    /// Obtiene la ruta para archivos de expresiones
    /// </summary>
    public static string ObtenerRutaExpresiones()
    {
        var rutaExpresiones = Path.Combine(ObtenerRaizProyecto(), "Output", "Expresiones");
        Directory.CreateDirectory(rutaExpresiones);
        return rutaExpresiones;
    }
    
    /// <summary>
    /// Obtiene la ruta para archivos de gramáticas
    /// </summary>
    public static string ObtenerRutaGramaticas()
    {
        var rutaGramaticas = Path.Combine(ObtenerRaizProyecto(), "Output", "Gramaticas");
        Directory.CreateDirectory(rutaGramaticas);
        return rutaGramaticas;
    }
    
    /// <summary>
    /// Obtiene la ruta para conjuntos FIRST
    /// </summary>
    public static string ObtenerRutaFirst()
    {
        var rutaFirst = Path.Combine(ObtenerRaizProyecto(), "Output", "First");
        Directory.CreateDirectory(rutaFirst);
        return rutaFirst;
    }
    
    /// <summary>
    /// Obtiene la ruta para tablas LL(1)
    /// </summary>
    public static string ObtenerRutaTablas()
    {
        var rutaTablas = Path.Combine(ObtenerRaizProyecto(), "Output", "TablasLL1");
        Directory.CreateDirectory(rutaTablas);
        return rutaTablas;
    }
    
    /// <summary>
    /// Obtiene la ruta para AFDs (Autómatas Finitos Deterministas)
    /// </summary>
    public static string ObtenerRutaAFD()
    {
        var rutaAFD = Path.Combine(ObtenerRaizProyecto(), "Output", "AFD");
        Directory.CreateDirectory(rutaAFD);
        return rutaAFD;
    }
    
    /// <summary>
    /// Genera un nombre de archivo con timestamp
    /// </summary>
    public static string GenerarNombreArchivo(string prefijo, string extension = "txt")
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        return $"{prefijo}_{timestamp}.{extension}";
    }
}
