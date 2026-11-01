# JavaPropertiesEditor sample

This Windows Forms sample creates, opens and saves Java `.properties` documents with `Kajabity.DocForms` document commands and `Kajabity.Tools.Java` 0.4.0. It targets `net10.0-windows` and references the library project in this repository.

Build from a Visual Studio Developer PowerShell at the repository root:

```powershell
dotnet restore Kajabity.DocForms.sln
dotnet build Samples/JavaPropertiesEditor/JavaPropertiesEditor.csproj -c Release --no-restore
```

See the [samples overview](../README.md) and [repository guide](../../README.md) for more information.
