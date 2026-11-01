# Working on Kajabity.DocForms

## Maintainers

- KDF is a Windows Forms document lifecycle library. The signed `Kajabity.DocForms` project targets `net48` and `net10.0-windows`; do not change either target or the package/assembly identity casually.
- `Documents/Document.cs` owns name and modified status. `SingleDocumentManager<TDocument>` is the file-format extension point. `Forms/SingleDocumentForm<TDocument>` owns interactive New/Open/Save/Close, confirmation, recent files and optional backup. `Forms/WindowPlacementManager` is separate and opt-in; the host persists `WindowPlacement`.
- Keep UI work on the form's UI thread. Preserve `*.Designer.cs`/`*.resx` relationships and test the Visual Studio Designer manually after form or resource changes. Keep document I/O in a manager; call the corresponding base method after successful I/O. Do not bypass the form's safe-save route in interactive commands.
- Run `dotnet restore Kajabity.DocForms.sln`, `msbuild Kajabity.DocForms.sln /m /p:Configuration=Release /v:minimal /nologo`, and `dotnet test Kajabity.DocForms.Test/Kajabity.DocForms.Test.csproj --framework net48 --configuration Release --no-build --settings Kajabity.DocForms.Test/DocForms.runsettings` (repeat with `--framework net10.0-windows`). Use a Visual Studio Developer PowerShell so `msbuild` is on PATH. On Windows, use Visual Studio Build Tools and the .NET 10 SDK.
- Samples are in `Samples/`: PlainTextEditor (`net48`), HexViewer, CsvEditor and JavaPropertiesEditor (`net10.0-windows`). CsvEditor's document and manager save paths have tests, but grid editing remains limited; do not present it as a complete editor. The samples reference the library project in this repository.
- Preserve public APIs and XML documentation when editing behaviour. Record private investigation notes only in ignored `SJW/`; public docs belong in tracked files. Do not commit or publish build artefacts.

## Consuming the package

- Derive a document type from `Document`, derive its manager from `SingleDocumentManager<TDocument>`, then derive the main form from `SingleDocumentForm<TDocument>` and pass a manager to its constructor. Connect menu commands such as `FileOpenClick` and `FileSaveClick`; respond to `DocumentChanged` and set `Modified` when the user edits.
- Call `InitialseRecentDocuments` only if the application wants registry-backed recent files. Set `Backup` only if it wants `~` backup files.
- For placement, construct `WindowPlacementManager` after `InitializeComponent`, call `Restore` before display, and save non-null `Capture()` after a successful close. The application chooses the persistence format and key. MDI children are unsupported.
