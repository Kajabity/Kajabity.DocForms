# Library guide

This directory contains the signed Kajabity.DocForms library project. Start with the [repository README](../README.md) for installation, a compiling example, and the current framework and release status.

Kajabity.DocForms is available in several different forms:

-	As a project which you can clone or fork from GitHub and include into your own Visual Studio solutions.
-	Copy some or all the source files/code into your own projects.
-	Download the compiled DLL from [GitHub Releases](https://github.com/Kajabity/Kajabity.DocForms/releases).
-	Add the NuGet component to your project from [nuget.org](https://www.nuget.org/packages/Kajabity.DocForms/).

`Samples/PlainTextEditor` remains the .NET Framework 4.8 compatibility example;
`Samples/HexViewer` demonstrates .NET 10. The older TextEditor demonstration
project was removed previously. `SDIForm` has now been
removed from the library as an intentional breaking change in 0.4.0.
Applications deriving from it should migrate to
`SingleDocumentForm<TDocument>` and `SingleDocumentManager<TDocument>`.
The separate `TextDocument` and `TextDocumentManager` types remain obsolete.

See the Releases section on GitHub to download copies of code, DLL exe's and NuGets.

The older [Kajabity.com guide](https://www.kajabity.com/kajabity-tools/sdi-forms-document-classes/) describes APIs that have since changed; use the current [migration guide](../MIGRATION.md) and samples for this version.

Versioning
----------

Release tags use `X.Y.Z` (for example, `0.4.1`). The release workflow passes the
tag as `ReleaseVersion` to MSBuild and as the NuGet package version. The DLL file
version becomes `X.Y.Z.0` and its informational/product version becomes `X.Y.Z`.
To build the same versions locally, use Visual Studio MSBuild:

```powershell
msbuild Kajabity.DocForms.sln /m /p:Configuration=Release /p:ReleaseVersion=0.4.1
```

Without `ReleaseVersion`, builds use file version `0.4.1.0` and product version
`0.4.1-dev`. Version attributes are generated under `obj`; no source stamping is
required. The strong-name assembly version remains `0.4.0.0` for this patch
release, so applications built against 0.4.0 do not need a new binding redirect
for 0.4.1. It changed from `0.3.0.0` in 0.4.0 when `SDIForm` was removed; users
upgrading from 0.3.1 should rebuild and review binding redirects.

The SDK-style project targets .NET Framework 4.8 and .NET 10 for Windows. Package identity and
metadata are declared in the project file. Create a package with `dotnet pack`;
the release workflow passes `ReleaseVersion` and `PackageVersion` from the tag.

Window placement (opt in)
-------------------------

`WindowPlacementManager` in `Kajabity.DocForms.Forms` restores an independent
top-level form to its former display and working area, including its maximised
state. It is separate from `SingleDocumentForm<TDocument>`: merely using that
base class does not move a window. The host owns storage; KDF does not read or
write application settings. `WindowPlacement` is a serialisable data object
containing normal bounds, display device name, display and working-area bounds,
effective DPI, and maximised state.

Construct a manager after `InitializeComponent()` and restore before showing the
form. Capture after a successful close, then dispose the manager. For example,
in a form with application-specific `LoadPlacement()` and `SavePlacement(...)`:

```csharp
private WindowPlacementManager placement;

public MainForm()
{
    InitializeComponent();
    placement = new WindowPlacementManager(this);
    placement.Restore(LoadPlacement()); // false leaves Windows' default placement intact
}

protected override void OnFormClosed(FormClosedEventArgs e)
{
    if (placement != null)
    {
        WindowPlacement saved = placement.Capture();
        if (saved != null) SavePlacement(saved);
        placement.Dispose();
    }
    base.OnFormClosed(e);
}
```

`WindowPlacementCalculator.TryCalculate` accepts saved placement and a list of
`WindowDisplay` values for testing or previewing decisions without a live form.
Restoration first matches the saved display device name, then substantial
display-geometry overlap, then the primary display. It maps the position within
the old working area to the new one, scales size using known effective DPI,
clamps to the current working area, and respects the form's size constraints
where the available screen permits. Missing or unreasonable settings leave
normal Windows placement intact. Minimise is not persisted; a minimised form
reopens restored, or maximised if that was its last non-minimised state.

The manager listens for display changes only while its form exists. After
Windows handles a change, it intervenes only if the title bar is inaccessible.
It does not store intermediate move or resize events. It unsubscribes when the
form closes or is disposed. Calls to `Restore`, `Capture` and
`EnsureAccessible` belong on the form's UI thread. Independent secondary forms
can opt in with separate placement keys. An MDI parent is a top-level window;
MDI children are unsupported. Modal dialogs may opt in, but most should retain
their owner-relative Windows placement. If a form's hard minimum size exceeds
the working area, neither KDF nor Windows can keep the entire form visible.
Display names may change or be reused, and exact physical monitor identity
cannot always be recovered from WinForms display data. WinForms and Windows
still control autoscaling and DPI awareness; test mixed-DPI arrangements in the
application's actual DPI-awareness mode.
