# WoW Spell Icon Editor

Aplicación de escritorio para Windows que permite:
- importar una imagen PNG/JPG/BMP
- ajustar la imagen a tamaño de icono cuadrado
- previsualizar con fondo transparente
- exportar a formato BLP1 para WoW

## Requisitos
- Windows 10/11
- .NET 8 SDK
- Visual Studio 2022 o VS Code con C#

## Ejecutar
```bash
dotnet build
dotnet run --project WoWSpellIconEditor/WoWSpellIconEditor.csproj
```

## Uso
1. Importa la imagen.
2. Ajusta el tamaño del icono.
3. Revisa la vista previa.
4. Pulsa "Exportar a BLP".
5. Guarda el archivo final con extensión .blp.

## Nota
Esta herramienta genera un archivo BLP1 de base para prototipos y edición de iconos de WoW. Si necesitas compatibilidad más extrema con el formato original de Blizzard, se recomienda validarlo con herramientas específicas de referencia del juego.

## Licencia
MIT
