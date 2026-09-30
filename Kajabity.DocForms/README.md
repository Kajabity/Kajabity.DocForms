Kajabity.DocForms
=================

This directory contains the Kajabity.DocForms DLL project and it produces 
a Strongly Named DLL which is packaged as a NuGet.

Kajabity.DocForms is available in several different forms:

-	As a project which you can clone or fork from GitHub and include into your own Visual Studio solutions.
-	Copy some or all the source files/code into your own projects.
-	Download the compiled DLL from [GitHub Releases](https://github.com/Kajabity/Kajabity.DocForms/releases).
-	Add the NuGet component to your project from [nuget.org](https://www.nuget.org/packages/Kajabity.DocForms/).

The maintained text-editor example is `Samples/PlainTextEditor`. The older
`Samples/Deprecated/TextEditor` example has been removed. Its public supporting
types, `SDIForm`, `TextDocument` and `TextDocumentManager`, remain marked obsolete
for compatibility with existing .NET Framework consumers; new applications should
use `SingleDocumentForm<TDocument>` and the `PlainTextEditor` example instead.

See the Releases section on GitHub to download copies of code, DLL exe's and NuGets.

Full documentation is available at [http://www.kajabity.com/kajabity-tools/](http://www.kajabity.com/kajabity-tools/).

Versioning
----------

Release tags use `X.Y.Z` (for example, `0.3.0`). The release workflow passes the
tag as `ReleaseVersion` to MSBuild and as the NuGet package version. The DLL file
version becomes `X.Y.Z.0` and its informational/product version becomes `X.Y.Z`.
To build the same versions locally, use Visual Studio MSBuild:

```powershell
msbuild Kajabity.DocForms.sln /m /p:Configuration=Release /p:ReleaseVersion=0.3.0
```

Without `ReleaseVersion`, builds use file version `0.3.0.0` and product version
`0.3.0-dev`. Version attributes are generated under `obj`; no source stamping is
required. The strong-name assembly version stays at `0.3.0.0` for compatible
`0.3.x` releases. Review it when making an incompatible release. This replaces
the old automatically changing `0.2.*` identity; existing .NET Framework
consumers may need rebuilding or a binding redirect when upgrading.
