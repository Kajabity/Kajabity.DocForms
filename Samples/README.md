# Kajabity.DocForms samples

The four Windows Forms applications use a project reference to KDF in this repository. Build on Windows with the .NET 10 SDK and Visual Studio Build Tools (including .NET Framework 4.8 targeting support). From a Visual Studio Developer PowerShell, restore once with `dotnet restore Kajabity.DocForms.sln`, then use the commands below from the repository root. Each application can also be opened and run in Visual Studio.

| Application | Target | Purpose | Extra package |
| --- | --- | --- | --- |
| [PlainTextEditor](PlainTextEditor/) | `net48` | Legacy compatibility example for New/Open/Save, modified state and recent files. | None |
| [HexViewer](HexViewer/) | `net10.0-windows` | Modern read-only hex/character viewer with drag-and-drop opening and XML-backed window placement. | None |
| [CsvEditor](CsvEditor/) | `net10.0-windows` | CSV loading, grid display and tested document serialization; grid editing remains limited. | `Kajabity.Tools.Csv` 0.2.1 |
| [JavaPropertiesEditor](JavaPropertiesEditor/) | `net10.0-windows` | Create, read and write Java `.properties` documents. | `Kajabity.Tools.Java` 0.4.0 |

```powershell
msbuild Samples/PlainTextEditor/PlainTextEditor.csproj /p:Configuration=Release /v:minimal /nologo
dotnet build Samples/HexViewer/HexViewer.csproj -c Release
dotnet build Samples/CsvEditor/CsvEditor.csproj -c Release
dotnet build Samples/JavaPropertiesEditor/JavaPropertiesEditor.csproj -c Release
```

Run the built applications on Windows from their `bin/Release` output directories, or use Visual Studio's Start command. `PlainTextEditor` uses the `net48` output path; the other applications use `bin/Release/net10.0-windows`. The samples are not automated UI tests. CsvEditor's CSV document and manager save paths have regression tests, but its grid editing has limitations and has not been verified as a complete editor; use copies when trying it with important files.

See the [main README](../README.md) for a minimal application, the [library guide](../Kajabity.DocForms/README.md) for window placement details, and each sample's own README for controls and screenshots. CsvEditor and JavaPropertiesEditor are planned to move to a future desktop samples repository after this release; they remain here for now.
