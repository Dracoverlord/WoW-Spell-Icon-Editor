# WoW Spell Icon Editor

Aplicación de escritorio para Windows que permite:
- importar una imagen PNG/JPG
- recortarla o escalarla a un icono cuadrado
- previsualizar el resultado
- exportarlo al formato BLP usado en WoW

## Requisitos
- Windows 10/11
- .NET 8 SDK
- Visual Studio 2022 o VS Code con C# extension

## Ejecutar
1. Abre la carpeta del proyecto.
2. Ejecuta:
   ```bash
   dotnet build
   dotnet run --project WoWSpellIconEditor/WoWSpellIconEditor.csproj
   ```
3. O abre la solución en Visual Studio y presiona F5.

## Uso
1. Haz clic en "Importar imagen".
2. Selecciona la imagen que quieres convertir.
3. Elige el tamaño del icono (56, 64, 128, etc.).
4. Vista previa del resultado en la ventana principal.
5. Haz clic en "Exportar a BLP".
6. Elige el nombre del archivo y guarda.

## Nota importante sobre BLP
Este proyecto genera un archivo BLP1 básico con transparencia en formato directo (sin paleta), adecuado para prototipos y herramientas de edición de iconos de WoW. Para uso en clientes de juego finales, es recomendable validarlo con un editor de assets o una herramienta específica de Blizzard.

## Licencia
MIT
