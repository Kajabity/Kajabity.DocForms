# Kajabity.DocForms

Build local, file-based Windows Forms applications with a reusable document lifecycle: New, Open, Save, Save As, Close, modified-state confirmation, optional backups and recent files. Window placement restoration is available separately as an opt-in helper. Your application supplies its document format, controls and placement settings storage.

This package contains signed assemblies for **.NET Framework 4.8** (`net48`) and **.NET 10 for Windows** (`net10.0-windows`). It requires Windows Forms on Windows.

```powershell
dotnet add package Kajabity.DocForms
```

Derive a `Document`, implement `SingleDocumentManager<TDocument>` to create/read/write it, and derive the application's form from `SingleDocumentForm<TDocument>`. Use the form's file-command handlers for interactive saves so its safe replacement and backup policy applies. `WindowPlacementManager` is opt-in for independent top-level forms; the host stores the captured `WindowPlacement`.

- [Quick start and complete compiling example](https://github.com/Kajabity/Kajabity.DocForms#minimal-application)
- [Migration from published 0.3.1](https://github.com/Kajabity/Kajabity.DocForms/blob/main/MIGRATION.md)
- [Changelog](https://github.com/Kajabity/Kajabity.DocForms/blob/main/CHANGELOG.md)
- [Samples](https://github.com/Kajabity/Kajabity.DocForms/tree/main/Samples)
- [Issues and support](https://github.com/Kajabity/Kajabity.DocForms/issues)

Apache-2.0 licensed. Version 0.4.0 adds the .NET 10 for Windows asset; see the migration guide when upgrading from 0.3.1.
