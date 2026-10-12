# Kajabity.DocForms

[![CI build and test](https://github.com/Kajabity/Kajabity.DocForms/actions/workflows/build.yml/badge.svg?branch=main)](https://github.com/Kajabity/Kajabity.DocForms/actions/workflows/build.yml) [![Current NuGet version](https://img.shields.io/nuget/v/Kajabity.DocForms.svg?label=NuGet)](https://www.nuget.org/packages/Kajabity.DocForms/) [![Cumulative NuGet downloads](https://img.shields.io/nuget/dt/Kajabity.DocForms.svg?label=downloads)](https://www.nuget.org/packages/Kajabity.DocForms/) [![Supported frameworks: .NET Framework 4.8 and .NET 10 for Windows](https://img.shields.io/badge/frameworks-net48%20%7C%20net10.0--windows-blue.svg)](https://github.com/Kajabity/Kajabity.DocForms/blob/main/Kajabity.DocForms/Kajabity.DocForms.csproj) [![Licence: Apache-2.0](https://img.shields.io/badge/licence-Apache--2.0-blue.svg)](https://www.apache.org/licenses/LICENSE-2.0)

**Kajabity.DocForms (KDF)** is a lightweight .NET framework for rapidly building document-based, Single Document Interface (SDI) Windows desktop applications using Windows Forms.

KDF handles the familiar application essentials — New, Open, Save, Save As, Close, unsaved-changes confirmation, recent files and optional document backups — so you can concentrate on your application's documents and user interface.

Supporting .NET 10 for Windows and .NET Framework 4.8, KDF also provides optional window-position restoration and lets applications choose their own document formats and settings storage.

## Key features

- New, Open, Save, Save As and Close with unsaved-changes confirmation.
- Opt-in recent files and document backups.
- Opt-in window placement restoration, including maximised state and monitor fallback.

## Install

Add the latest published `Kajabity.DocForms` package to a Windows Forms project with NuGet:

```powershell
dotnet add package Kajabity.DocForms
```

Use version 0.4.0 or later for .NET 10 for Windows; the previously published 0.3.1 package contains only the .NET Framework 4.8 asset. See [MIGRATION.md](MIGRATION.md) before upgrading an existing application.

## Minimal application

Create a Windows Forms project with `<UseWindowsForms>true</UseWindowsForms>` and a reference to KDF. This single-file example uses the same extension points as [PlainTextEditor](Samples/PlainTextEditor/). Put it in `Program.cs` and run the project on Windows:

```csharp
using System;
using System.IO;
using System.Windows.Forms;
using Kajabity.DocForms.Documents;
using Kajabity.DocForms.Forms;

public sealed class Note : Document
{
    private string _text = "";
    public string Text
    {
        get { return _text; }
        set { _text = value; Modified = true; }
    }
}

public sealed class NoteManager : SingleDocumentManager<Note>
{
    public NoteManager() { DefaultName = "Note"; DefaultExtension = "txt"; }
    public override void NewDocument()
    {
        Document = new Note();
        base.NewDocument();
    }
    public override void Load(string filename)
    {
        Document = new Note { Text = File.ReadAllText(filename) };
        base.Load(filename);
    }
    public override void Save(string filename)
    {
        File.WriteAllText(filename, Document.Text);
        base.Save(filename);
    }
}

public sealed class NoteForm : SingleDocumentForm<Note>
{
    private readonly TextBox _editor = new TextBox { Dock = DockStyle.Fill, Multiline = true };

    public NoteForm() : base(new NoteManager())
    {
        var file = new ToolStripMenuItem("File");
        file.DropDownItems.Add("New", null, FileNewClick);
        file.DropDownItems.Add("Open...", null, FileOpenClick);
        file.DropDownItems.Add("Save", null, FileSaveClick);
        file.DropDownItems.Add("Save As...", null, FileSaveAsClick);
        file.DropDownItems.Add("Close", null, FileCloseClick);
        var menu = new MenuStrip();
        menu.Items.Add(file);
        MainMenuStrip = menu;
        Controls.Add(_editor);
        Controls.Add(menu);
        DocumentChanged += (sender, args) =>
        {
            _editor.Text = Manager.Opened ? Manager.Document.Text : "";
            if (Manager.Opened) Manager.Document.Modified = false;
        };
        _editor.TextChanged += (sender, args) =>
        {
            if (Manager.Opened) Manager.Document.Text = _editor.Text;
        };
    }
}

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new NoteForm());
    }
}
```

`NoteManager` performs format-specific I/O and calls the base method **after** each successful operation. The form commands handle file selection, modified-document prompts and safe replacement of existing files. Connect your own controls to `DocumentChanged` and set `Document.Modified` when users edit content. If a close or Save As is cancelled, the current document remains open. Calling a manager's public `Save` directly bypasses the form's temporary-file replacement and backup path; use the form commands for interactive saves.

Set `Backup = true` on the form to request a sibling `~` backup when replacing an existing file. Recent files are opt-in: call `InitialseRecentDocuments(menuItem)` after creating the menu, and connect the parent File menu opening event to `OnParentMenuDropDownOpening`. KDF stores that list under the application's `HKEY_CURRENT_USER\Software\<company>\<application>\Recent Documents` key; the entry assembly needs `AssemblyCompanyAttribute`. KDF does not supply a general settings system or automatic recovery after an interrupted process.

## Window placement

`WindowPlacementManager` restores an independent top-level form when the application opts in. Construct it after `InitializeComponent()`, call `Restore(savedPlacement)` before showing the form, then call `Capture()` after a successful close and persist the returned `WindowPlacement` using your own settings code. A `null` capture means there is no valid placement to overwrite an existing value. Dispose the manager when the form closes. [HexViewer](Samples/HexViewer/HexViewerMainForm.cs) shows the complete integration and an XML settings file.

```csharp
private WindowPlacementManager _placement;

// In the form constructor, after InitializeComponent():
_placement = new WindowPlacementManager(this);
_placement.Restore(LoadPlacement()); // false leaves Windows' default position intact

// In OnFormClosed, after a successful close:
WindowPlacement saved = _placement.Capture();
if (saved != null) SavePlacement(saved);
_placement.Dispose();
```

The helper keeps normal bounds and a maximised flag, avoids reopening minimised, and adapts to changed monitor working areas and DPI where Windows provides the information. It falls back to the primary display if the original cannot be found; invalid settings leave normal Windows placement intact. It only moves an already open form after a display change if its title bar becomes inaccessible. Each independent form needs its own saved placement. MDI children are unsupported; modal dialogs normally use their owner's placement. Mixed-DPI and physical monitor reconnection still require testing on the application's actual Windows configuration. More detail is in the [library guide](Kajabity.DocForms/README.md#window-placement-opt-in).

## Repository and help

- [Samples](Samples/README.md): PlainTextEditor (`net48`), HexViewer, CsvEditor and JavaPropertiesEditor (`net10.0-windows`).
- [Migration guide](MIGRATION.md) and [changelog](CHANGELOG.md).
- [Issues](https://github.com/Kajabity/Kajabity.DocForms/issues) for bugs and feature requests; [pull requests](https://github.com/Kajabity/Kajabity.DocForms/pulls) for contributions. Include target framework, Windows version and reproduction steps.

Licensed under [Apache-2.0](LICENSE).
