HexViewer Sample
================

A simple application to open and display a file (Document) as a hexadecimal 
byte array, illustrated below.

![Screenshot](screenshot.png)

The sample illustrates how to use some features of Kajabity.DocForms module including the following features:
-	Use of SDIForm base class to provide file handling features.
-	Open a file.
-	Recent file handling.
-	Extending the Document and DocumentManager base classes to implement BinaryDocument and BinaryDocumentManager classes.

The display is implemented using the HexPanel class which extends Panel and draws the hexadecimal and character bytes using double buffering.  Mouse tracking is used to highlight individual hex and character bytes as the mouse moves over the panel.  Unused menu items have been hidden - perhaps to be implemented later.

More information is available at [http://www.kajabity.com/kajabity-tools/](http://www.kajabity.com/kajabity-tools/).
