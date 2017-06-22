Kajabity.DocForms GitHub Repository
===================================

[![Build status](https://ci.appveyor.com/api/projects/status/a3r57ga2s2urpv0b/branch/master?svg=true)](https://ci.appveyor.com/project/Kajabity/kajabity-docforms/branch/master)

Kajabity.DocForms is a collection of WinForms classes to provide all those standard 'Boiler Plate'
file handling features of a Windows Desktop application.  These classes have been refactored 
from applications I've written over recent years and I am find I use them regularly for quickly
putting together new applications.

This repository contains several sub-projects:

-	**Kajabity.DocForms** - a DLL project providing a collection of utilities for .NET projects.
-	**Samples** - a collection of sample Desktop application projects illustrating the use of some of Kajabity.DocForms utility classes, as well as other NuGets I have published.

See the Releases section on GitHub to download copies of code, DLL exe's and NuGets.

Kajabity.DocForms DLL is a strongly named assembly and is available from nuget.org as Kajabity.DocForms.

This code originally formed part of the Kajabity Tools library.  
Full documentation is available at [http://www.kajabity.com/kajabity-tools/](http://www.kajabity.com/kajabity-tools/).

Features
--------

-	``SingleDocumentForm`` is an extension of ``System.Windows.Forms.Form`` supporting standard file handling functionality linked to a document type.
-	``Document`` abstract base class to extend to represent each file (document) type.
-	``SingleDocumentManager`` abstract base class to extend to implement Load/Save/New document functionality.

File handling:

-	Open file
-	New File
    - Set default filename and extension
-	Save File, Save File as
    - Backup original file on Save
-	Close file
-	Open recent file
    - Configurable number of recent files and display length
-	Exit application

In addition, these features easily support:

-	Standard file handling menu items and tool strip items.
-	Drag and drop files onto application.
