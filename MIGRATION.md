# Migration from Kajabity.DocForms 0.3.1

[0.3.1](https://www.nuget.org/packages/Kajabity.DocForms/0.3.1) supplies a `net48` assembly. Version 0.4.0 supplies both `net48` and `net10.0-windows` in the same `Kajabity.DocForms` package.

## Upgrading from 0.4.0 to 0.4.1

The target frameworks and public type names are unchanged. The signed `AssemblyVersion` remains `0.4.0.0`, while the file and informational versions become `0.4.1.0` and `0.4.1`.

`MaximumRecentDocumentCount = 0` now disables in-memory recent-file history safely. Assigning a negative value throws `ArgumentOutOfRangeException`, and lowering a positive limit immediately removes the oldest entries. Check applications that set this property from user preferences or configuration. Existing registry values are not deleted by changing the limit; the opt-in registry store continues to use the same key.

The CsvEditor sample now serializes its document rows on save and counts columns correctly after edits. Its grid UI still has editing limitations, so treat it as a sample rather than a complete CSV editor.

## Choose a target

- Existing .NET Framework 4.8 Windows Forms applications can remain on `net48` and upgrade to 0.4.0 after reviewing the compatibility changes below.
- .NET 10 Windows Forms applications should target `net10.0-windows` and set `<UseWindowsForms>true</UseWindowsForms>`. Version 0.3.1 does not contain a .NET 10 asset; use package version 0.4.0 or later.
- The library is Windows-only; no cross-platform or general `net10.0` assembly is supplied.

## Source compatibility

`SDIForm` was removed. Replace inheritance from it with `SingleDocumentForm<TDocument>` and provide a `SingleDocumentManager<TDocument>` for your `Document` subtype. The separate obsolete `DocumentManager`, `TextDocument` and `TextDocumentManager` classes are still present, but new code should use the generic manager and its own document type. The old TextEditor demonstration was removed; [PlainTextEditor](Samples/PlainTextEditor/) is the `net48` reference sample. Public document and form methods retain their names, including the existing `InitialseRecentDocuments` spelling.

When overriding `NewDocument`, assign `Document` and then call `base.NewDocument()`. When overriding `Load`, read the file and assign `Document` before `base.Load(filename)`. When overriding `Save`, write the supplied path and then call `base.Save(filename)`. Do not assume the supplied path is the final destination: the form's safe-save route supplies a sibling temporary path before replacing the destination. Avoid creating a separate backup inside the override. Calling the manager's public `Save` directly bypasses safe replacement and the form's `Backup` policy.

`SingleDocumentForm<TDocument>` now lets application `FormClosing` handlers cancel while the document is still available. Check handlers that assumed it had already been closed. A cancelled Save As or failed save prevents closure and leaves the document modified. Backups are opt-in via `Backup = true`, with a `~` sibling file when an existing destination is replaced. There is no automatic crash recovery or backup browser.

Recent-file storage remains an explicit `InitialseRecentDocuments` opt-in using the current user's registry under `Software\<entry-assembly company>\<entry-assembly name>\Recent Documents`. Existing applications should retain their entry-assembly company/name if they need the same list. Window placement has a separate host-owned settings store; KDF does not migrate or write placement settings automatically.

## Window placement

Instantiate `WindowPlacementManager` for an independent top-level form after `InitializeComponent()`. Call `Restore(saved)` before the form is shown. After a successful close, persist `Capture()` only when it returns a non-null value, then dispose the manager. See [HexViewer](Samples/HexViewer/HexViewerMainForm.cs) and the [README](README.md#window-placement). `SingleDocumentForm<TDocument>` does not restore position on its own, so existing applications are unaffected until they opt in. MDI children are unsupported; modal dialogs normally use owner-relative placement.

## Assembly binding and manual checks

The package ID and signed assembly name remain `Kajabity.DocForms`. Version 0.4.0 changes the strong-name `AssemblyVersion` from `0.3.0.0` to `0.4.0.0` because `SDIForm` was removed. Rebuild .NET Framework applications against 0.4.0 and review any explicit references or binding redirects. Applications that still use `SDIForm` must migrate before upgrading; swapping DLLs in place is unsafe. The file version is `0.4.0.0` and the released informational version is `0.4.0`. Test existing WinForms Designer forms, saved files and backups, recent-file registry paths, close cancellation and real mixed-DPI multi-monitor behaviour in the host application before release.
