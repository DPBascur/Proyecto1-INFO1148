# Proyecto Final INFO1148 - Analizador Lexico y Sintactico + Generador de Casos de Prueba

Aplicacion de escritorio desarrollada en C# con Avalonia UI que integra funcionalidades de analisis lexico, sintactico y generacion automatica de casos de prueba basados en gramaticas libres de contexto.

---

## Descripcion del Proyecto

Este proyecto fusiona dos aplicaciones independientes en una sola herramienta integral para el analisis de lenguajes formales y la generacion de casos de prueba:

1. **Analizador Lexico y Sintactico**: Permite analizar codigo Java, extraer tokens, construir gramaticas, eliminar recursion izquierda, calcular conjuntos FIRST/FOLLOW, generar tablas LL(1) y construir automatas finitos deterministas (AFD).

2. **Generador de Casos de Prueba**: Genera automaticamente casos de prueba validos, invalidos y extremos a partir de gramaticas libres de contexto, con soporte para derivacion formal, mutacion sintactica y exportacion a JSON.

---

## Requisitos Previos

- **.NET 8.0 SDK** instalado
- **Windows** (o Linux/macOS con Avalonia UI)
- **Visual Studio Code** o **Visual Studio 2022** (recomendado)

---

## Como Ejecutar el Programa

### Opcion 1: Desde Visual Studio Code
1. Abrir la carpeta del proyecto en VS Code
2. Presionar `F5` o ejecutar desde terminal:
   ```bash
   dotnet restore
   dotnet build
   dotnet run
   ```

### Opcion 2: Desde Terminal
```bash
cd Proyecto1-INFO1148
dotnet restore
dotnet build
dotnet run
```

La aplicacion se abrira en una ventana de 1400x800 pixeles.

---

## Funcionalidades Principales

### MODULO 1: Analisis Lexico

**Que hace?**
Analiza codigo fuente Java y extrae tokens (numeros, operadores, identificadores, etc.)

**Como usar:**
1. Click en **Cargar Archivo Java** para seleccionar tu propio archivo `.java`
   - O click en **Cargar Ejemplo** para usar el codigo de ejemplo incluido
2. El codigo se mostrara en el panel "Codigo Fuente"
3. Los tokens se listaran automaticamente en la tabla "Tokens Reconocidos"
4. Las expresiones aritmeticas apareceran en el panel lateral

**Exportar resultados:**
- Click en **Exportar Tokens**
- Los archivos se guardan en `Output/Tokens/` y `Output/Expresiones/`

**Tipos de tokens reconocidos:**
- NUM: Numeros (enteros y decimales)
- ID: Identificadores (variables)
- MAS, MENOS, MULT, DIV, MOD: Operadores aritmeticos
- PAR_IZQ, PAR_DER: Parentesis
- INT, DOUBLE, FLOAT: Palabras clave de Java

---

### MODULO 2: Gramaticas

**Funcionalidad 1: Exportar Gramatica Inicial**

**Que hace?**
Guarda la gramatica formal inicial para expresiones aritmeticas

**Como usar:**
1. Click en **Exportar Gramatica Inicial**
2. Se genera `Output/Gramaticas/grammar_initial.txt`

**Gramatica generada:**
```
E -> E + T | E - T | T
T -> T * F | T / F | F
F -> ( E ) | NUM | ID
```

**Funcionalidad 2: Eliminar Recursion por la Izquierda**

**Que hace?**
Transforma la gramatica para eliminar recursion izquierda

**Como usar:**
1. Click en **Eliminar Recursion por la Izquierda**
2. Se genera `Output/Gramaticas/grammar_sin_recursion_YYYYMMDD_HHMMSS.txt`

**Transformacion aplicada:**
```
E -> E + T | T  =>  E -> T E' ; E' -> + T E' | epsilon
T -> T * F | F  =>  T -> F T' ; T' -> * F T' | epsilon
```

---

### MODULO 3: FIRST y FOLLOW

**Que hace?**
Calcula el conjunto FIRST para cada no terminal de la gramatica

**Como usar:**
1. Click en **Calcular FIRST**
2. Se genera `Output/First/first_set_YYYYMMDD_HHMMSS.txt`

**Resultado esperado:**
```
FIRST(E) = { (, NUM, ID }
FIRST(T) = { (, NUM, ID }
FIRST(F) = { (, NUM, ID }
```

**Explicacion:**
- FIRST(X): Conjunto de terminales que pueden aparecer al inicio de una derivacion desde X
- Usado para construccion de analizadores sintacticos descendentes

---

### MODULO 4: Tabla LL(1)

**Que hace?**
Construye la tabla sintactica LL(1) para analisis descendente predictivo

**Como usar:**
1. Click en **Generar Tabla LL(1)**
2. Se generan dos archivos:
   - `Output/TablasLL1/tabla_ll1_YYYYMMDD_HHMMSS.txt` (reporte completo)
   - `Output/TablasLL1/tabla_ll1_YYYYMMDD_HHMMSS.csv` (importable a Excel)

**Estructura de la tabla:**
```
         +           *           id          (           )           $
E        E->T E'     -           E->T E'     E->T E'     -           -
E'       E'->+T E'   -           -           -           E'->ε       E'->ε
T        -           -           T->F T'     T->F T'     -           -
T'       T'->ε       T'->*F T'   -           -           T'->ε       T'->ε
F        -           -           F->id       F->(E)      -           -
```

**Aplicacion:**
Usada por analizadores sintacticos LL(1) para determinar que produccion aplicar segun el token actual.

---

### MODULO 5: Automata Finito Determinista (AFD)

**Que hace?**
Construye un automata finito determinista a partir de la gramatica

**Como usar:**
1. Click en **Construir AFD**
2. Se genera `Output/AFD/afd_YYYYMMDD_HHMMSS.txt`

**Contenido del archivo:**
- Estados del automata
- Simbolos del alfabeto
- Transiciones entre estados
- Estado inicial
- Estados de aceptacion

**Aplicacion:**
Reconocimiento de patrones lexicos y validacion de cadenas.

---

### MODULO 6: Generador de Casos de Prueba

**Funcionalidad 1: Generar Cadena con Historial**

**Que hace?**
Genera una sola cadena valida mostrando cada paso de la derivacion

**Como usar:**
1. Ajustar "Profundidad Maxima" (5-100)
2. Click en **Generar Cadena con Historial**
3. Ver el historial paso a paso en la pestana "Historial"

**Ejemplo de historial:**
```
Paso 1: E
Paso 2: E + T
Paso 3: T + T
Paso 4: F + T
Paso 5: id + T
Paso 6: id + F
Paso 7: id + id
```

---

**Funcionalidad 2: Generar Multiples Cadenas Validas**

**Que hace?**
Genera varias cadenas validas mediante derivacion leftmost

**Como usar:**
1. Ajustar "Cantidad de Cadenas Validas" (1-50)
2. Click en **Generar Multiples Cadenas**
3. Las cadenas se muestran en la lista de resultados

**Ejemplos generados:**
```
id + id
id * id + id
( id + id ) * id
id + id + id
```

---

**Funcionalidad 3: Generar Casos Invalidos (Mutacion)**

**Que hace?**
Genera casos invalidos mediante mutacion sintactica de cadenas validas

**Como usar:**
1. Ajustar "Cantidad de Casos Invalidos" (1-20)
2. Click en **Generar Casos Invalidos**

**Tipos de mutacion aplicados:**
- **Parentesis desbalanceados**: `( id + id`
- **Operadores duplicados**: `id + + id`
- **Operador al inicio**: `+ id * id`
- **Operador al final**: `id + id *`
- **Parentesis vacio**: `( )`
- **Operador faltante**: `id id`
- **Identificador faltante**: `id + * id`
- **Caracter invalido**: `id @ id`
- **Espacios en token**: `i d + id`

---

**Funcionalidad 4: Generar Casos Extremos**

**Que hace?**
Genera casos extremos para probar limites del sistema

**Como usar:**
1. Ajustar "Cantidad de Casos Extremos" (1-20)
2. Click en **Generar Casos Extremos**

**Tipos de casos extremos:**
- **Profundidad maxima**: Derivacion muy profunda
- **Profundidad minima**: Caso mas simple posible
- **Complejidad maxima**: Maximo numero de operadores
- **Expresion larga**: Muchos simbolos terminales
- **Expresion corta**: Minima expresion valida
- **Anidamiento maximo**: Multiples niveles de parentesis

---

**Funcionalidad 5: Generar Suite Completa**

**Que hace?**
Genera una suite completa de casos de prueba (validos, invalidos y extremos) con metricas

**Como usar:**
1. Ajustar todos los parametros (profundidad, cantidades)
2. Click en **Generar Suite Completa**
3. Ver casos en la tabla "Casos de Prueba"
4. Ver metricas en la pestana "Metricas"

**Metricas generadas:**
- Distribucion porcentual por categoria
- Longitud promedio de expresiones
- Profundidad maxima del arbol sintactico
- Operadores generados por tipo (+, *, -, /)
- Tipos de mutacion aplicados
- Tipos de casos extremos generados
- Tiempo de ejecucion

---

**Funcionalidad 6: Exportar a JSON**

**Que hace?**
Exporta todos los casos de prueba generados a formato JSON estructurado

**Como usar:**
1. Despues de generar casos, click en **Exportar a JSON**
2. El archivo se guarda en la carpeta del proyecto: `casos_prueba_YYYYMMDD_HHMMSS.json`

**Estructura del JSON:**
```json
{
  "metadata": {
    "fecha_generacion": "2025-11-29T10:30:00",
    "total_casos": 50,
    "gramatica": "E -> E + T | T..."
  },
  "casos": [
    {
      "id": "CASO_001",
      "cadena": "id + id",
      "categoria": "valido",
      "descripcion": "Caso valido generado por derivacion",
      "metadata": {
        "profundidad": 7,
        "num_tokens": 3,
        "total_operadores": 1
      }
    }
  ],
  "metricas": {
    "distribucion": {
      "validos": 40,
      "invalidos": 30,
      "extremos": 30
    }
  }
}
```

---

**Funcionalidad 7: Cargar Gramatica desde TXT**

**Que hace?**
Permite cargar una gramatica personalizada desde un archivo de texto

**Como usar:**
1. Click en **Cargar Gramatica desde TXT**
2. Seleccionar archivo `.txt` con formato:
   ```
   E -> E + T
   E -> T
   T -> T * F
   T -> F
   F -> ( E )
   F -> id
   ```
3. La gramatica se carga y reemplaza la predeterminada

**Formato del archivo:**
- Una produccion por linea
- Formato: `NoTerminal -> Simbolo1 Simbolo2 ...`
- Comentarios con `#`
- Terminales en minusculas, no terminales en mayusculas

---

## Arquitectura del Proyecto

### Estructura de Carpetas

```
Proyecto1-INFO1148/
├── App.axaml
├── App.axaml.cs
├── Program.cs
├── ViewLocator.cs
├── ProyectoFinal.csproj
├── app.manifest
├── Assets/
│   ├── gramatica_ejemplo.txt
│   └── ArithmeticExamples.java
├── Models/
│   ├── Token.cs
│   ├── Lexer.cs
│   ├── FileProcessor.cs
│   ├── Grammar.cs
│   ├── GrammarService.cs
│   ├── GrammarExporter.cs
│   ├── CalculadorFirst.cs
│   ├── CalculadorFollow.cs
│   ├── EliminadorRecursion.cs
│   ├── GeneradorTablaLL1.cs
│   ├── ConstructorAFD.cs
│   ├── ExportadorFirst.cs
│   ├── ExportadorRecursion.cs
│   ├── ExportadorTablaLL1.cs
│   ├── ExportadorAFD.cs
│   ├── AdministradorRutas.cs
│   ├── Symbol.cs
│   ├── Terminal.cs
│   ├── NonTerminal.cs
│   ├── Production.cs
│   ├── ContextFreeGrammar.cs
│   ├── ParserGramatica.cs
│   ├── GramaticaExpresionesAritmeticas.cs
│   ├── GeneradorDerivaciones.cs
│   ├── Mutador.cs
│   ├── GeneradorCasosExtremos.cs
│   ├── Clasificador.cs
│   ├── GeneradorMetricas.cs
│   ├── ExportadorJSON.cs
│   └── ReportGenerator.cs
├── ViewModels/
│   ├── ViewModelBase.cs
│   ├── MainWindowViewModel.cs
│   └── CasoPruebaViewModel.cs
└── Views/
    ├── MainWindow.axaml
    └── MainWindow.axaml.cs
```

### Patrones de Diseno Utilizados

1. **MVVM (Model-View-ViewModel)**: Separacion de logica de negocio y UI
2. **Repository Pattern**: Gestion de datos y persistencia
3. **Factory Pattern**: Creacion de objetos complejos (gramaticas, casos de prueba)
4. **Strategy Pattern**: Diferentes estrategias de mutacion y generacion
5. **Observer Pattern**: Propiedades observables con CommunityToolkit.Mvvm

---

## Tecnologias Utilizadas

- **C# 12** (.NET 8.0)
- **Avalonia UI 11.3.8** (Framework multiplataforma)
- **CommunityToolkit.Mvvm 8.2.1** (MVVM Toolkit)
- **Avalonia.Controls.DataGrid** (Tablas de datos)

---

## Casos de Uso Principales

### Caso de Uso 1: Analizar Archivo Java Completo
1. Abrir pestana "Analisis Lexico"
2. Click en "Cargar Archivo Java"
3. Seleccionar archivo `.java`
4. Revisar tokens y expresiones
5. Click en "Exportar Tokens" para guardar resultados

### Caso de Uso 2: Generar Tabla LL(1) para Gramatica
1. Abrir pestana "Gramaticas"
2. Click en "Exportar Gramatica Inicial"
3. Abrir pestana "Gramaticas" > "Eliminar Recursion"
4. Abrir pestana "FIRST y FOLLOW" > "Calcular FIRST"
5. Abrir pestana "Tabla LL(1)" > "Generar Tabla LL(1)"
6. Abrir archivo CSV generado en Excel

### Caso de Uso 3: Generar Suite de Pruebas Completa
1. Abrir pestana "Generador de Casos"
2. Ajustar parametros (Profundidad: 30, Validos: 20, Invalidos: 15, Extremos: 15)
3. Click en "Generar Suite Completa"
4. Esperar a que termine la generacion
5. Revisar casos en la tabla "Casos de Prueba"
6. Ver metricas en pestana "Metricas"
7. Click en "Exportar a JSON" para guardar

---

## Archivos de Salida

Todos los archivos generados se guardan en la carpeta `Output/` con la siguiente estructura:

```
Output/
├── Tokens/
│   └── tokens_YYYYMMDD_HHMMSS.txt
├── Expresiones/
│   └── expresiones_YYYYMMDD_HHMMSS.txt
├── Gramaticas/
│   ├── grammar_initial.txt
│   └── grammar_sin_recursion_YYYYMMDD_HHMMSS.txt
├── First/
│   └── first_set_YYYYMMDD_HHMMSS.txt
├── TablasLL1/
│   ├── tabla_ll1_YYYYMMDD_HHMMSS.txt
│   └── tabla_ll1_YYYYMMDD_HHMMSS.csv
└── AFD/
    └── afd_YYYYMMDD_HHMMSS.txt
```

Archivos JSON de casos de prueba se guardan en la raiz del proyecto:
```
casos_prueba_YYYYMMDD_HHMMSS.json
```

---

## Solucionar Problemas Comunes

### Error: "No se encuentra el archivo .java"
- Verificar que el archivo existe en la ruta especificada
- Asegurarse de tener permisos de lectura

### Error: "No se puede generar la tabla LL(1)"
- Primero eliminar recursion izquierda
- Luego calcular FIRST
- Finalmente generar tabla LL(1)

### Error: "La gramatica no es LL(1)"
- Verificar que no haya conflictos FIRST/FIRST
- Verificar que no haya conflictos FIRST/FOLLOW
- Considerar transformar la gramatica

### Error al cargar gramatica desde TXT
- Verificar formato: `NoTerminal -> simbolos`
- Una produccion por linea
- No terminales en MAYUSCULAS
- Terminales en minusculas o simbolos

---

## Creditos

**Basado en:**
- Proyecto 1: Analizador Lexico y Sintactico
- Miniproyecto 2: Generador de Casos de Prueba

---

## Licencia

Este proyecto es de uso academico y educativo.

---