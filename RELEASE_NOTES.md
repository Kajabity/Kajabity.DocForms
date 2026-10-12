# Kajabity.DocForms 0.4.0

Kajabity.DocForms now builds for .NET Framework 4.8 and .NET 10 for Windows from one SDK-style library project. Both assemblies are included in one `Kajabity.DocForms` package. Windows Forms applications can remain on `net48` or move to `net10.0-windows`.

This release adds opt-in window placement capture and restoration for independent top-level forms, including maximised state and fallback when a saved monitor is unavailable. Application `FormClosing` handlers can now cancel closure while the document is still available. The existing interactive save path continues to protect files with a sibling temporary file.

**Breaking change:** the obsolete `SDIForm` class is removed. Use `SingleDocumentForm<TDocument>` with `SingleDocumentManager<TDocument>`. The signed assembly version changes from `0.3.0.0` to `0.4.0.0`; .NET Framework consumers should rebuild and review binding redirects. The old TextEditor demonstration is removed. See the [migration guide](https://github.com/Kajabity/Kajabity.DocForms/blob/main/MIGRATION.md) for source and behavioural changes.

The included samples are PlainTextEditor (`net48`) and HexViewer, CsvEditor and JavaPropertiesEditor (`net10.0-windows`). CsvEditor remains a read/display demonstration; its save implementation is incomplete. The package includes XML documentation, Source Link symbols and a Kajabity icon.
