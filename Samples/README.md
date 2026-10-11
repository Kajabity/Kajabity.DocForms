Kajabity.DocForms Samples
=========================

This directory contains sample projects illustrating how to use Kajabity.DocForms.
All four use the KDF project in this repository.

| Application | Target framework | Purpose and other dependencies |
| --- | --- | --- |
| PlainTextEditor | `net48` | Legacy compatibility example for creating, reading and writing text files. |
| HexViewer | `net10.0-windows` | Modern KDF example that opens files and displays hexadecimal and character views. |
| CsvEditor | `net10.0-windows` | Loads and displays CSV documents using the published `Kajabity.Tools.Csv` NuGet package. Its save implementation is incomplete. |
| JavaPropertiesEditor | `net10.0-windows` | Creates, reads and writes Java `.properties` files using the published `Kajabity.Tools.Java` NuGet package. |

CsvEditor and JavaPropertiesEditor remain here for this release; moving them to a
separate desktop samples repository is planned for later.

More information is available at [http://www.kajabity.com/kajabity-tools/](http://www.kajabity.com/kajabity-tools/).
