# Kajabity.DocForms 0.4.1

This patch release fixes recent-file history when `MaximumRecentDocumentCount` is zero or reduced. Zero disables history without throwing after a successful open or save; reducing the limit removes the oldest entries immediately. Negative values are now rejected when assigned.

The CsvEditor sample now counts columns correctly when editing cells and writes its CSV rows on save, including escaped values and embedded newlines. Its document and manager have regression tests. The grid UI still has editing limitations, so CsvEditor remains a sample rather than a complete CSV editor.

NuGet vulnerability auditing now covers every tracked project. Test and sample dependencies have been updated, and Markdown files use consistent LF line endings.

The package still contains signed `net48` and `net10.0-windows` assemblies. Its strong-name assembly version remains `0.4.0.0` for compatibility with 0.4.0; the file and informational versions are `0.4.1.0` and `0.4.1`. See the [migration guide](https://github.com/Kajabity/Kajabity.DocForms/blob/main/MIGRATION.md) for upgrade notes.
