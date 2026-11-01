using System;
using System.IO;
using CsvEditor;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Kajabity.DocForms.Test.Documents
{
    [TestFixture]
    public class CsvDocumentManagerTest
    {
        [Test]
        public void LoadReadsQuotedFieldsAndEmbeddedNewline()
        {
            string filename = Path.GetTempFileName();
            try
            {
                // CsvReader currently treats a final record newline as an extra empty row.
                File.WriteAllText(filename, "\"a,b\",\"say \"\"hello\"\"\",\"line one\r\nline two\"");

                var manager = new CsvDocumentManager();
                manager.Load(filename);

                ClassicAssert.AreEqual(1, manager.Document.RowCount);
                ClassicAssert.AreEqual(3, manager.Document.ColumnCount);
                ClassicAssert.AreEqual("a,b", manager.Document.GetCell(0, 0));
                ClassicAssert.AreEqual("say \"hello\"", manager.Document.GetCell(0, 1));
                ClassicAssert.AreEqual("line one\r\nline two", manager.Document.GetCell(0, 2));
            }
            finally
            {
                File.Delete(filename);
            }
        }

        [Test]
        public void SaveRoundTripsRowsEmptyCellsAndEscapedValues()
        {
            string filename = Path.GetTempFileName();
            try
            {
                var writer = new CsvDocumentManager();
                writer.NewDocument();
                writer.Document.SetCell(0, 0, "account");
                writer.Document.SetCell(0, 1, "one,two");
                writer.Document.SetCell(0, 2, "say \"hello\"");
                writer.Document.SetCell(0, 3, "line one\r\nline two");
                writer.Document.SetCell(1, 1, "second row");
                writer.Document.SetCell(1, 3, "last column");

                writer.Save(filename);

                ClassicAssert.IsTrue(new FileInfo(filename).Length > 0, "A successful save must write data.");
                ClassicAssert.IsFalse(writer.Modified);

                var reader = new CsvDocumentManager();
                reader.Load(filename);

                ClassicAssert.AreEqual(2, reader.Document.RowCount);
                ClassicAssert.AreEqual(4, reader.Document.ColumnCount);
                ClassicAssert.AreEqual("account", reader.Document.GetCell(0, 0));
                ClassicAssert.AreEqual("one,two", reader.Document.GetCell(0, 1));
                ClassicAssert.AreEqual("say \"hello\"", reader.Document.GetCell(0, 2));
                ClassicAssert.AreEqual("line one\r\nline two", reader.Document.GetCell(0, 3));
                ClassicAssert.AreEqual(string.Empty, reader.Document.GetCell(1, 0));
                ClassicAssert.AreEqual("second row", reader.Document.GetCell(1, 1));
                ClassicAssert.AreEqual(string.Empty, reader.Document.GetCell(1, 2));
                ClassicAssert.AreEqual("last column", reader.Document.GetCell(1, 3));
            }
            finally
            {
                File.Delete(filename);
            }
        }

        [Test]
        public void SaveOverwritesLongerFileWithoutLeavingOldRows()
        {
            string filename = Path.GetTempFileName();
            try
            {
                File.WriteAllText(filename, "obsolete," + new string('x', 1024) + "\r\nold,row\r\n");

                var writer = new CsvDocumentManager();
                writer.NewDocument();
                writer.Document.SetCell(0, 0, "replacement");
                writer.Save(filename);

                var reader = new CsvDocumentManager();
                reader.Load(filename);

                ClassicAssert.AreEqual(1, reader.Document.RowCount);
                ClassicAssert.AreEqual(1, reader.Document.ColumnCount);
                ClassicAssert.AreEqual("replacement", reader.Document.GetCell(0, 0));
                ClassicAssert.IsFalse(File.ReadAllText(filename).Contains("obsolete"));
            }
            finally
            {
                File.Delete(filename);
            }
        }

        [Test]
        public void FailedSaveLeavesDocumentModifiedAndFilenameUnchanged()
        {
            var manager = new CsvDocumentManager();
            manager.NewDocument();
            manager.Document.SetCell(0, 0, "account");
            string originalFilename = manager.Filename;
            string missingDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            string filename = Path.Combine(missingDirectory, "data.csv");

            ClassicAssert.Throws<DirectoryNotFoundException>(() => manager.Save(filename));

            ClassicAssert.IsTrue(manager.Modified);
            ClassicAssert.IsTrue(manager.NewFile);
            ClassicAssert.AreEqual(originalFilename, manager.Filename);
        }
    }
}
