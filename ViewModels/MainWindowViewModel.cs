using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProyectoFinal.Models;

namespace ProyectoFinal.ViewModels;

/// <summary>
/// ViewModel principal que fusiona todas las funcionalidades:
/// - Análisis Léxico y Sintáctico (Proyecto 1)
/// - Generador de Casos de Prueba (Proyecto 2)
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
{
    #region Propiedades Observables - Análisis Léxico y Sintáctico

    [ObservableProperty]
    private string _sourceCode = string.Empty;

    [ObservableProperty]
    private string _selectedFilePath = "No se ha seleccionado ningún archivo";

    [ObservableProperty]
    private string _tokenOutput = string.Empty;

    [ObservableProperty]
    private string _statusMessage = "Listo";

    [ObservableProperty]
    private bool _isProcessing = false;

    [ObservableProperty]
    private int _totalTokens = 0;

    [ObservableProperty]
    private int _totalExpressions = 0;

    #endregion

    #region Propiedades Observables - Generador de Casos de Prueba

    [ObservableProperty]
    private string _gramaticaInfo = string.Empty;

    [ObservableProperty]
    private int _profundidadMaxima = 20;

    [ObservableProperty]
    private int _cantidadCadenas = 5;

    [ObservableProperty]
    private int _cantidadInvalidos = 5;

    [ObservableProperty]
    private int _cantidadExtremos = 10;

    [ObservableProperty]
    private string _resultadosGeneracion = string.Empty;

    [ObservableProperty]
    private string _historialDerivacion = string.Empty;

    [ObservableProperty]
    private string _reporteMetricas = string.Empty;

    [ObservableProperty]
    private bool _mostrarHistorial = false;

    [ObservableProperty]
    private bool _mostrarMetricas = false;

    [ObservableProperty]
    private int _longitudMaxima = 100;

    [ObservableProperty]
    private string _archivoGramaticaCargado = "Gramática predeterminada";

    #endregion

    #region Colecciones Observables

    // Análisis Léxico
    public ObservableCollection<Token> Tokens { get; } = new();
    public ObservableCollection<string> ExpressionsList { get; } = new();

    // Generador de Casos de Prueba
    public ObservableCollection<string> CadenasGeneradas { get; } = new();
    
    /// <summary>
    /// Colección observable de casos de prueba para el DataGrid.
    /// Incluye: cadena, categoría, longitud, profundidad y métricas asociadas.
    /// </summary>
    public ObservableCollection<CasoPruebaViewModel> CasosPrueba { get; } = new();

    #endregion

    #region Variables Privadas - Análisis Léxico

    private List<Token>? _currentTokens;
    private List<List<Token>>? _currentExpressions;

    #endregion

    #region Variables Privadas - Generador de Casos de Prueba

    private GramaticaExpresionesAritmeticas? _gramaticaDefault;
    private ContextFreeGrammar? _gramaticaActual;
    private GeneradorDerivaciones? _generador;
    private Mutador? _mutador;
    private GeneradorCasosExtremos? _generadorExtremos;
    private Clasificador? _clasificador;
    private GeneradorMetricas? _generadorMetricas;
    private ExportadorJSON? _exportador;
    private List<CasoPrueba> _todosLosCasos = new();

    #endregion

    #region Constructor

    public MainWindowViewModel()
    {
        InicializarComponentes();
        InicializarGramatica();
    }

    /// <summary>
    /// Inicializa los componentes del sistema de generación de pruebas.
    /// </summary>
    private void InicializarComponentes()
    {
        _mutador = new Mutador();
        _clasificador = new Clasificador();
        _generadorMetricas = new GeneradorMetricas();
        _exportador = new ExportadorJSON();
    }

    /// <summary>
    /// Inicializa la gramática de expresiones aritméticas predeterminada.
    /// </summary>
    private void InicializarGramatica()
    {
        _gramaticaDefault = new GramaticaExpresionesAritmeticas();
        _gramaticaActual = _gramaticaDefault.Gramatica;
        GramaticaInfo = _gramaticaDefault.ToString();
    }

    #endregion

    #region Comandos - Análisis Léxico y Sintáctico (Proyecto 1)

    /// <summary>
    /// Carga un archivo Java y realiza el análisis léxico.
    /// </summary>
    [RelayCommand]
    private async Task LoadFileAsync()
    {
        try
        {
            IsProcessing = true;
            StatusMessage = "Cargando archivo...";

            var filePath = await ShowOpenFileDialogAsync();
            
            if (string.IsNullOrEmpty(filePath))
            {
                StatusMessage = "Operación cancelada";
                return;
            }

            await ProcessFileAsync(filePath);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
        }
    }

    /// <summary>
    /// Procesa un archivo Java: tokeniza y extrae expresiones aritméticas.
    /// </summary>
    private async Task ProcessFileAsync(string filePath)
    {
        SelectedFilePath = filePath;
        StatusMessage = "Procesando archivo...";

        // Leer y procesar el archivo
        var (tokens, sourceCode) = await ProcesadorArchivos.ProcesarArchivoJavaAsync(filePath);
        
        _currentTokens = tokens;
        SourceCode = sourceCode;
        TotalTokens = tokens.Count;

        // Extraer expresiones aritméticas
        _currentExpressions = ProcesadorArchivos.ExtraerExpresionesAritmeticas(tokens);
        TotalExpressions = _currentExpressions.Count;

        // Actualizar la colección de tokens
        Tokens.Clear();
        foreach (var token in tokens.Take(100)) // Limitamos a 100 para rendimiento
        {
            Tokens.Add(token);
        }

        // Actualizar la lista de expresiones
        ExpressionsList.Clear();
        for (int i = 0; i < _currentExpressions.Count; i++)
        {
            var expr = _currentExpressions[i];
            var exprString = string.Join(" ", expr.Select(t => t.Valor));
            ExpressionsList.Add($"Expr {i + 1}: {exprString}");
        }

        // Generar salida de texto
        GenerateTokenOutput();

        StatusMessage = $"Procesamiento completo: {TotalTokens} tokens, {TotalExpressions} expresiones";
    }

    /// <summary>
    /// Genera la salida de texto para mostrar los tokens.
    /// </summary>
    private void GenerateTokenOutput()
    {
        if (_currentTokens == null) return;

        var output = new System.Text.StringBuilder();
        output.AppendLine("=== TOKENS RECONOCIDOS ===\n");

        foreach (var token in _currentTokens.Take(50))
        {
            output.AppendLine($"{token.Tipo,-15} {token.Valor,-20} (L:{token.Linea}, C:{token.Columna})");
        }

        if (_currentTokens.Count > 50)
        {
            output.AppendLine($"\n... y {_currentTokens.Count - 50} tokens más");
        }

        TokenOutput = output.ToString();
    }

    /// <summary>
    /// Exporta los tokens y expresiones a archivos de texto.
    /// </summary>
    [RelayCommand]
    private async Task ExportTokensAsync()
    {
        if (_currentTokens == null || _currentTokens.Count == 0)
        {
            StatusMessage = "No hay tokens para exportar";
            return;
        }

        try
        {
            IsProcessing = true;
            StatusMessage = "Exportando tokens...";

            // Usar rutas organizadas
            var rutaTokens = AdministradorRutas.ObtenerRutaTokens();
            var rutaExpresiones = AdministradorRutas.ObtenerRutaExpresiones();

            var tokensFile = Path.Combine(rutaTokens, AdministradorRutas.GenerarNombreArchivo("tokens"));
            var expressionsFile = Path.Combine(rutaExpresiones, AdministradorRutas.GenerarNombreArchivo("expresiones"));

            // Guardar tokens
            await ProcesadorArchivos.GuardarTokensEnArchivoAsync(_currentTokens, tokensFile);

            // Guardar expresiones
            if (_currentExpressions != null && _currentExpressions.Count > 0)
            {
                await ProcesadorArchivos.GuardarExpresionesEnArchivoAsync(_currentExpressions, expressionsFile);
            }

            StatusMessage = $"Archivos exportados a: Output/Tokens y Output/Expresiones";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error al exportar: {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
        }
    }

    /// <summary>
    /// Exporta la gramática inicial de expresiones aritméticas.
    /// </summary>
    [RelayCommand]
    private async Task ExportGrammarAsync()
    {
        try
        {
            IsProcessing = true;
            StatusMessage = "Exportando gramática inicial...";

            var grammar = ServicioGramatica.ObtenerGramaticaAritmeticaInicial();
            var exportedPath = await ExportadorGramatica.ExportarGramaticaInicialAsync(grammar);

            StatusMessage = $"Gramática exportada a: {exportedPath}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error al exportar gramática: {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
        }
    }

    /// <summary>
    /// Elimina la recursión por la izquierda de la gramática.
    /// </summary>
    [RelayCommand]
    private async Task EliminateRecursionAsync()
    {
        try
        {
            IsProcessing = true;
            StatusMessage = "Eliminando recursión por la izquierda...";

            // Obtener gramática original
            var gramaticaOriginal = ServicioGramatica.ObtenerGramaticaAritmeticaInicial();
            
            // Aplicar algoritmo de eliminación de recursión
            var gramaticaTransformada = EliminadorRecursion.EliminarRecursionIzquierda(gramaticaOriginal);
            
            // Exportar gramática transformada con reporte completo
            var exportedPath = await ExportadorRecursion.ExportarGramaticaTransformadaAsync(
                gramaticaOriginal, 
                gramaticaTransformada);

            StatusMessage = $"Recursión eliminada. Archivo guardado en: {exportedPath}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error al eliminar recursión: {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
        }
    }

    /// <summary>
    /// Calcula el conjunto FIRST para la gramática.
    /// </summary>
    [RelayCommand]
    private async Task CalculateFirstAsync()
    {
        try
        {
            IsProcessing = true;
            StatusMessage = "Calculando conjunto FIRST...";

            // Obtener gramática sin recursión (transformada)
            var gramaticaOriginal = ServicioGramatica.ObtenerGramaticaAritmeticaInicial();
            var gramaticaTransformada = EliminadorRecursion.EliminarRecursionIzquierda(gramaticaOriginal);
            
            // Calcular conjunto FIRST
            var firstSets = CalculadorFirst.CalcularFirst(gramaticaTransformada);
            
            // Exportar resultado con reporte completo
            var exportedPath = await ExportadorFirst.ExportarConjuntoFirstAsync(
                gramaticaTransformada, 
                firstSets);

            StatusMessage = $"Conjunto FIRST calculado. Archivo guardado en: {exportedPath}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error al calcular FIRST: {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
        }
    }

    /// <summary>
    /// Genera la tabla sintáctica LL(1) para la gramática.
    /// </summary>
    [RelayCommand]
    private async Task GenerateLL1TableAsync()
    {
        try
        {
            IsProcessing = true;
            StatusMessage = "Generando tabla sintáctica LL(1)...";

            // Obtener gramática transformada
            var gramaticaOriginal = ServicioGramatica.ObtenerGramaticaAritmeticaInicial();
            var gramaticaTransformada = EliminadorRecursion.EliminarRecursionIzquierda(gramaticaOriginal);
            
            // Calcular FIRST y FOLLOW
            var firstSets = CalculadorFirst.CalcularFirst(gramaticaTransformada);
            var followSets = CalculadorFollow.CalcularFollow(gramaticaTransformada, firstSets);
            
            // Generar tabla LL(1)
            var tabla = GeneradorTablaLL1.GenerarTabla(gramaticaTransformada, firstSets, followSets);
            
            // Exportar en ambos formatos (TXT y CSV)
            var (txtPath, csvPath) = await ExportadorTablaLL1.ExportarTablaAmbasFormatosAsync(
                tabla, 
                gramaticaTransformada, 
                firstSets, 
                followSets);

            StatusMessage = $"Tabla LL(1) generada. TXT: {txtPath} | CSV: {csvPath}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error al generar tabla LL(1): {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
        }
    }

    /// <summary>
    /// Construye el Autómata Finito Determinista (AFD) a partir de la gramática.
    /// </summary>
    [RelayCommand]
    private async Task ConstructAFDAsync()
    {
        try
        {
            IsProcessing = true;
            StatusMessage = "Construyendo AFD (Autómata Finito Determinista)...";

            // Obtener gramática transformada
            var gramaticaOriginal = ServicioGramatica.ObtenerGramaticaAritmeticaInicial();
            var gramaticaTransformada = EliminadorRecursion.EliminarRecursionIzquierda(gramaticaOriginal);
            
            // Calcular FIRST y FOLLOW
            var firstSets = CalculadorFirst.CalcularFirst(gramaticaTransformada);
            var followSets = CalculadorFollow.CalcularFollow(gramaticaTransformada, firstSets);
            
            // Generar tabla LL(1)
            var tabla = GeneradorTablaLL1.GenerarTabla(gramaticaTransformada, firstSets, followSets);
            
            // Construir AFD
            var afd = ConstructorAFD.ConstruirAFD(gramaticaTransformada, tabla, firstSets, followSets);
            
            // Validar expresiones aritméticas con el AFD
            List<(string expresion, bool resultado, List<(int, string, int)> traza)>? validaciones = null;
            
            if (_currentExpressions != null && _currentExpressions.Count > 0)
            {
                StatusMessage = "Validando expresiones con el AFD...";
                validaciones = new List<(string, bool, List<(int, string, int)>)>();
                
                foreach (var expr in _currentExpressions.Take(5)) // Validar máximo 5 expresiones
                {
                    var expresionStr = string.Join(" ", expr.Select(t => t.Valor));
                    var (valido, traza) = ConstructorAFD.ValidarCadena(afd, expr);
                    validaciones.Add((expresionStr, valido, traza));
                }
            }
            
            // Exportar en todos los formatos (TXT, CSV, DOT)
            var (txtPath, csvPath, dotPath) = await ExportadorAFD.ExportarAFDTodosFormatosAsync(afd, validaciones);

            StatusMessage = $"AFD construido. TXT: {txtPath} | CSV: {csvPath} | DOT: {dotPath}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error al construir AFD: {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
        }
    }

    /// <summary>
    /// Carga un archivo de ejemplo con código Java.
    /// </summary>
    [RelayCommand]
    private async Task LoadSampleFile()
    {
        // Crear un archivo de ejemplo
        var sampleCode = @"public class ArithmeticExample {
    public static void main(String[] args) {
        int a = 10;
        int b = 5;
        int sum = a + b;
        int diff = a - b;
        int product = a * b;
        int quotient = a / b;
        int remainder = a % b;
        
        double x = 3.14;
        double y = 2.5;
        double result = (x + y) * 2 - 1;
        
        int complex = (a + b) * (a - b) / 2;
    }
}";

        SourceCode = sampleCode;
        SelectedFilePath = "Archivo de ejemplo cargado";
        
        // Procesar el código de ejemplo
        var analizador = new AnalizadorLexico(sampleCode);
        _currentTokens = analizador.Tokenizar();
        TotalTokens = _currentTokens.Count;

        _currentExpressions = ProcesadorArchivos.ExtraerExpresionesAritmeticas(_currentTokens);
        TotalExpressions = _currentExpressions.Count;

        Tokens.Clear();
        foreach (var token in _currentTokens.Take(100))
        {
            Tokens.Add(token);
        }

        ExpressionsList.Clear();
        for (int i = 0; i < _currentExpressions.Count; i++)
        {
            var expr = _currentExpressions[i];
            var exprString = string.Join(" ", expr.Select(t => t.Valor));
            ExpressionsList.Add($"Expr {i + 1}: {exprString}");
        }

        GenerateTokenOutput();
        
        // Exportar automáticamente los archivos de evidencia
        try
        {
            // Usar rutas organizadas
            var rutaTokens = AdministradorRutas.ObtenerRutaTokens();
            var rutaExpresiones = AdministradorRutas.ObtenerRutaExpresiones();

            var tokensFile = Path.Combine(rutaTokens, AdministradorRutas.GenerarNombreArchivo("tokens_ejemplo"));
            var expressionsFile = Path.Combine(rutaExpresiones, AdministradorRutas.GenerarNombreArchivo("expresiones_ejemplo"));

            await ProcesadorArchivos.GuardarTokensEnArchivoAsync(_currentTokens, tokensFile);
            await ProcesadorArchivos.GuardarExpresionesEnArchivoAsync(_currentExpressions, expressionsFile);

            StatusMessage = $"Ejemplo cargado: {TotalTokens} tokens, {TotalExpressions} expresiones. Evidencias exportadas a Output/Tokens y Output/Expresiones";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ejemplo cargado: {TotalTokens} tokens, {TotalExpressions} expresiones. Error al exportar: {ex.Message}";
        }
    }

    /// <summary>
    /// Método público para que la vista pueda procesar un archivo seleccionado.
    /// </summary>
    public async Task ProcessFileFromPathAsync(string filePath)
    {
        await ProcessFileAsync(filePath);
    }

    /// <summary>
    /// Método placeholder para diálogo de archivo.
    /// En la implementación real se usará el diálogo de Avalonia.
    /// </summary>
    private Task<string?> ShowOpenFileDialogAsync()
    {
        // Por ahora devolvemos null, se implementará con Avalonia.Platform.Storage
        return Task.FromResult<string?>(null);
    }

    #endregion

    #region Comandos - Generador de Casos de Prueba (Proyecto 2)

    /// <summary>
    /// Genera una sola cadena con historial detallado de derivación.
    /// </summary>
    [RelayCommand]
    private void GenerarCadenaConHistorial()
    {
        if (_gramaticaActual == null) return;

        try
        {
            _generador = new GeneradorDerivaciones(_gramaticaActual, ProfundidadMaxima);
            var cadena = _generador.GenerarCadena();

            ResultadosGeneracion = $"✅ Cadena generada:\n{cadena}\n\n" +
                                   $"Pasos de derivación: {_generador.HistorialDerivacion.Count}";

            HistorialDerivacion = _generador.ObtenerHistorialTexto();
            MostrarHistorial = true;
            MostrarMetricas = false;
        }
        catch (Exception ex)
        {
            ResultadosGeneracion = $"❌ Error al generar cadena:\n{ex.Message}";
            HistorialDerivacion = string.Empty;
            MostrarHistorial = false;
        }
    }

    /// <summary>
    /// Genera múltiples cadenas válidas según la gramática.
    /// </summary>
    [RelayCommand]
    private void GenerarMultiplesCadenas()
    {
        if (_gramaticaActual == null) return;

        try
        {
            _generador = new GeneradorDerivaciones(_gramaticaActual, ProfundidadMaxima);
            CadenasGeneradas.Clear();

            var cadenas = _generador.GenerarMultiplesCadenas(CantidadCadenas);

            foreach (var cadena in cadenas)
            {
                CadenasGeneradas.Add(cadena);
            }

            ResultadosGeneracion = $"✅ Se generaron {cadenas.Count} cadenas válidas exitosamente.\n\n" +
                                   $"Profundidad máxima: {ProfundidadMaxima}\n" +
                                   $"Cadenas solicitadas: {CantidadCadenas}";

            MostrarHistorial = false;
            MostrarMetricas = false;
        }
        catch (Exception ex)
        {
            ResultadosGeneracion = $"❌ Error al generar cadenas:\n{ex.Message}";
            MostrarHistorial = false;
        }
    }

    /// <summary>
    /// Genera casos inválidos mediante mutación de cadenas válidas.
    /// </summary>
    [RelayCommand]
    private void GenerarCasosInvalidos()
    {
        if (_gramaticaActual == null || _mutador == null) return;

        try
        {
            CadenasGeneradas.Clear();
            
            // Primero generar una cadena válida
            _generador = new GeneradorDerivaciones(_gramaticaActual, ProfundidadMaxima);
            var cadenaValida = _generador.GenerarCadena();

            // Mutar para crear casos inválidos
            var casosInvalidos = _mutador.GenerarCasosInvalidos(cadenaValida, CantidadInvalidos);

            foreach (var caso in casosInvalidos)
            {
                CadenasGeneradas.Add($"[{caso.TipoMutacion}] {caso.Cadena}");
            }

            ResultadosGeneracion = $"✅ Se generaron {casosInvalidos.Count} casos inválidos por mutación.\n\n" +
                                   $"Cadena original: {cadenaValida}\n" +
                                   $"Tipos de mutación aplicados: {casosInvalidos.Count}";

            MostrarHistorial = false;
            MostrarMetricas = false;
        }
        catch (Exception ex)
        {
            ResultadosGeneracion = $"❌ Error al generar casos inválidos:\n{ex.Message}";
        }
    }

    /// <summary>
    /// Genera casos extremos (profundidad máxima/mínima, complejidad, etc.).
    /// </summary>
    [RelayCommand]
    private void GenerarCasosExtremos()
    {
        if (_gramaticaActual == null) return;

        try
        {
            CadenasGeneradas.Clear();
            _generadorExtremos = new GeneradorCasosExtremos(_gramaticaActual);
            
            var casosExtremos = _generadorExtremos.GenerarCasosExtremos(cantidadPorTipo: 2);

            foreach (var caso in casosExtremos)
            {
                CadenasGeneradas.Add($"[{caso.Tipo}] {caso.Cadena}");
            }

            ResultadosGeneracion = $"✅ Se generaron {casosExtremos.Count} casos extremos.\n\n" +
                                   $"Tipos generados: profundidad máxima/mínima, complejidad, anidamiento, etc.";

            MostrarHistorial = false;
            MostrarMetricas = false;
        }
        catch (Exception ex)
        {
            ResultadosGeneracion = $"❌ Error al generar casos extremos:\n{ex.Message}";
        }
    }

    /// <summary>
    /// Genera una suite completa de casos de prueba (válidos, inválidos y extremos).
    /// </summary>
    [RelayCommand]
    private async Task GenerarSuiteCompleta()
    {
        if (_gramaticaActual == null || _clasificador == null || _generadorMetricas == null) return;

        try
        {
            _generadorMetricas.IniciarMedicion();
            _todosLosCasos.Clear();
            _clasificador.ReiniciarContador();

            ResultadosGeneracion = "⏳ Generando suite completa de casos de prueba...\n";

            // 1. Generar casos válidos
            _generador = new GeneradorDerivaciones(_gramaticaActual, ProfundidadMaxima);
            var cadenasValidas = _generador.GenerarMultiplesCadenas(CantidadCadenas);
            var casosValidos = _clasificador.ClasificarCasosValidos(cadenasValidas, _generador);
            _todosLosCasos.AddRange(casosValidos);

            // 2. Generar casos inválidos
            if (cadenasValidas.Count > 0 && _mutador != null)
            {
                var cadenaBase = cadenasValidas.First();
                var casosInvalidos = _mutador.GenerarCasosInvalidos(cadenaBase, CantidadInvalidos);
                foreach (var ci in casosInvalidos)
                {
                    _todosLosCasos.Add(_clasificador.ClasificarCasoInvalido(ci));
                }
            }

            // 3. Generar casos extremos
            _generadorExtremos = new GeneradorCasosExtremos(_gramaticaActual);
            var casosExtremos = _generadorExtremos.GenerarCasosExtremos(cantidadPorTipo: 1);
            foreach (var ce in casosExtremos)
            {
                _todosLosCasos.Add(_clasificador.ClasificarCasoExtremo(ce));
            }

            _generadorMetricas.DetenerMedicion();

            // Generar métricas y reporte
            var reporte = _generadorMetricas.GenerarReporte(_todosLosCasos);
            ReporteMetricas = _generadorMetricas.GenerarReporteTexto(reporte);
            
            ResultadosGeneracion = $"✅ Suite completa generada exitosamente!\n\n" +
                                   $"Total de casos: {_todosLosCasos.Count}\n" +
                                   $"• Válidos: {reporte.CasosValidos}\n" +
                                   $"• Inválidos: {reporte.CasosInvalidos}\n" +
                                   $"• Extremos: {reporte.CasosExtremos}\n\n" +
                                   $"Tiempo: {reporte.TiempoEjecucionMs} ms";

            MostrarHistorial = false;
            MostrarMetricas = true;

            // Mostrar algunos ejemplos en lista
            CadenasGeneradas.Clear();
            foreach (var caso in _todosLosCasos.Take(20))
            {
                CadenasGeneradas.Add($"[{caso.Categoria}] {caso.Cadena}");
            }

            // Actualizar DataGrid con todos los casos
            CasosPrueba.Clear();
            foreach (var caso in _todosLosCasos)
            {
                CasosPrueba.Add(new CasoPruebaViewModel(caso));
            }
        }
        catch (Exception ex)
        {
            ResultadosGeneracion = $"❌ Error al generar suite completa:\n{ex.Message}";
            MostrarMetricas = false;
        }
    }

    /// <summary>
    /// Exporta los casos de prueba generados a un archivo JSON.
    /// </summary>
    [RelayCommand]
    private async Task ExportarJSON()
    {
        if (_todosLosCasos.Count == 0)
        {
            ResultadosGeneracion = "⚠️ No hay casos para exportar. Genera una suite completa primero.";
            return;
        }

        try
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var nombreArchivo = $"casos_prueba_{timestamp}.json";
            var rutaProyecto = AppDomain.CurrentDomain.BaseDirectory;
            var rutaArchivo = Path.Combine(rutaProyecto, nombreArchivo);

            _exportador?.ExportarACaso(
                _todosLosCasos,
                _gramaticaActual!,
                rutaArchivo,
                new Dictionary<string, object>
                {
                    { "profundidad_maxima", ProfundidadMaxima },
                    { "cantidad_validos", CantidadCadenas },
                    { "cantidad_invalidos", CantidadInvalidos },
                    { "cantidad_extremos", CantidadExtremos }
                },
                _generadorMetricas?.ObtenerTiempoMs() ?? 0
            );

            ResultadosGeneracion = $"✅ Casos exportados exitosamente!\n\n" +
                                   $"Archivo: {nombreArchivo}\n" +
                                   $"Ubicación: Carpeta del proyecto\n" +
                                   $"Total de casos: {_todosLosCasos.Count}";
        }
        catch (Exception ex)
        {
            ResultadosGeneracion = $"❌ Error al exportar JSON:\n{ex.Message}";
        }
    }

    /// <summary>
    /// Carga una gramática personalizada desde un archivo de texto.
    /// </summary>
    [RelayCommand]
    private async Task CargarGramatica()
    {
        try
        {
            // Intentar cargar desde carpeta Assets primero
            var rutaProyecto = AppDomain.CurrentDomain.BaseDirectory;
            var rutaArchivo = Path.Combine(rutaProyecto, "Assets", "gramatica_ejemplo.txt");

            if (File.Exists(rutaArchivo))
            {
                var parser = new ParserGramatica();
                _gramaticaActual = parser.CargarDesdeArchivo(rutaArchivo);
                
                GramaticaInfo = _gramaticaActual.ToString();
                ArchivoGramaticaCargado = "Assets/gramatica_ejemplo.txt";
                
                ResultadosGeneracion = $"✅ Gramática cargada exitosamente!\n\n" +
                                       $"Archivo: Assets/gramatica_ejemplo.txt\n" +
                                       $"Variables: {_gramaticaActual.Variables.Count}\n" +
                                       $"Terminales: {_gramaticaActual.Terminales.Count}\n" +
                                       $"Producciones: {_gramaticaActual.Producciones.Count}";
            }
            else
            {
                ResultadosGeneracion = "ℹ️ Para cargar una gramática personalizada:\n\n" +
                                       "1. Crea un archivo 'gramatica_ejemplo.txt' en:\n" +
                                       $"   {Path.Combine(rutaProyecto, "Assets")}\n\n" +
                                       "2. Formato del archivo:\n" +
                                       "   E -> E + T\n" +
                                       "   E -> T\n" +
                                       "   T -> T * F\n" +
                                       "   T -> F\n" +
                                       "   F -> ( E )\n" +
                                       "   F -> id\n\n" +
                                       "3. Convenciones:\n" +
                                       "   • Mayúsculas = No terminales\n" +
                                       "   • Minúsculas/símbolos = Terminales\n" +
                                       "   • Usa '->' para separar lados\n" +
                                       "   • Un espacio entre símbolos\n" +
                                       "   • Líneas con # son comentarios\n\n" +
                                       "Por ahora, se usa la gramática predeterminada de expresiones aritméticas.";
            }
        }
        catch (Exception ex)
        {
            ResultadosGeneracion = $"❌ Error al cargar gramática:\n{ex.Message}\n\n" +
                                   "Revisa el formato del archivo. Debe ser:\n" +
                                   "NoTerminal -> Simbolo1 Simbolo2 ...\n\n" +
                                   "Ejemplo válido:\n" +
                                   "E -> E + T\n" +
                                   "E -> T";
            
            // Volver a la gramática predeterminada
            InicializarGramatica();
        }
    }

    /// <summary>
    /// Limpia todos los resultados y colecciones generados.
    /// </summary>
    [RelayCommand]
    private void LimpiarResultados()
    {
        ResultadosGeneracion = string.Empty;
        HistorialDerivacion = string.Empty;
        ReporteMetricas = string.Empty;
        CadenasGeneradas.Clear();
        CasosPrueba.Clear();
        _todosLosCasos.Clear();
        MostrarHistorial = false;
        MostrarMetricas = false;
    }

    #endregion
}
