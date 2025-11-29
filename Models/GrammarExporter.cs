using System;
using System.IO;
using System.Threading.Tasks;

namespace ProyectoFinal.Models;

public static class ExportadorGramatica
{
    public static async Task<string> ExportarGramaticaInicialAsync(Gramatica gramatica)
    {
        var rutaGramaticas = AdministradorRutas.ObtenerRutaGramaticas();
        var rutaArchivo = Path.Combine(rutaGramaticas, "grammar_initial.txt");
        
        await File.WriteAllTextAsync(rutaArchivo, gramatica.ATexto());
        return rutaArchivo;
    }
}
