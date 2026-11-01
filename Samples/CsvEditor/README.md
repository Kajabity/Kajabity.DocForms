# CsvEditor sample

This Windows Forms sample uses `Kajabity.DocForms` for document commands and `Kajabity.Tools.Csv` to parse and write CSV records. It targets `net10.0-windows` and references the library project in this repository.

The document and manager can load quoted fields, embedded newlines and empty cells, and save records with CSV escaping. Saving replaces old file contents; the form's Save and Save As commands use KDF's safe-save path. Regression tests cover serialization, overwriting an existing file and failed saves. The document also tracks its row and column counts when cells are set.

The grid UI cannot add rows or columns, and editing the first column or clicking blank space can fail. It has no automated UI tests. The sample is useful for exploring the document lifecycle, but should not be treated as a complete CSV editor. Try it on copies of important files.

Build from a Visual Studio Developer PowerShell at the repository root:

```powershell
dotnet restore Kajabity.DocForms.sln
dotnet build Samples/CsvEditor/CsvEditor.csproj -c Release --no-restore
```

More about the library is in the [repository README](../../README.md).
