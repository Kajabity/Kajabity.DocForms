using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kajabity.DocForms.Test.Documents
{
    [TestFixture]
    public class SingleDocumentManagerTest
    {
        [Test]
        public void TestSglDocMgrConstructor()
        {
            TestableSingleDocumentManager manager = new TestableSingleDocumentManager();

            ClassicAssert.AreEqual(false, manager.NewFile);
            ClassicAssert.AreEqual(false, manager.Opened);
            ClassicAssert.AreEqual(null, manager.Document);
            ClassicAssert.AreEqual(false, manager.Modified);
            ClassicAssert.AreEqual(null, manager.Filename);
        }

        //* Can get and set default extension property.
        [Test]
        public void TestSglDocMgrDefaultExtensionProperty()
        {
            TestableSingleDocumentManager manager = new TestableSingleDocumentManager();

            ClassicAssert.AreEqual(null, manager.DefaultExtension);

            string changedExtension = "abc";
            manager.DefaultExtension = changedExtension;

            ClassicAssert.AreEqual(changedExtension, manager.DefaultExtension);

            manager.NewDocument();

            string expectedFilename = TestableSingleDocumentManager.DEFAULT_DOCUMENT_NAME + "1." + changedExtension;

            ClassicAssert.AreEqual(expectedFilename, manager.Document.Name);
            ClassicAssert.AreEqual(expectedFilename, manager.Filename);
        }

        //* Can get and set default name property
        [Test]
        public void TestSglDocMgrDefaultNameProperty()
        {
            TestableSingleDocumentManager manager = new TestableSingleDocumentManager();

            ClassicAssert.AreEqual(TestableSingleDocumentManager.DEFAULT_DOCUMENT_NAME, manager.DefaultName);

            string changedName = "a-name";
            manager.DefaultName = changedName;

            ClassicAssert.AreEqual(changedName, manager.DefaultName);
        }

        //* new document - default name and extension
        //*  - event????
        //*  - filename
        //*  - flags - modified, new file, opened
        [Test]
        public void TestSglDocMgrNewDocument()
        {
            TestableSingleDocumentManager manager = new TestableSingleDocumentManager();

            bool called = false;
            manager.DocumentStatusChanged += delegate (object sender, EventArgs e)
            {
                called = true;
            };

            ClassicAssert.AreEqual(false, manager.NewFile);
            ClassicAssert.AreEqual(false, manager.Opened);
            ClassicAssert.AreEqual(null, manager.Document);
            ClassicAssert.AreEqual(false, called);

            manager.NewDocument();

            ClassicAssert.AreEqual(true, manager.NewFile);
            ClassicAssert.AreEqual(true, manager.Opened);
            ClassicAssert.AreNotEqual(null, manager.Document);
            ClassicAssert.AreEqual(true, called);
        }

        //* Load document
        //*  - filename
        //*  - event
        //*  - flags - modified, new file, opened
        [Test]
        public void TestSglDocMgrLoadDocument()
        {
            TestableSingleDocumentManager manager = new TestableSingleDocumentManager();

            bool called = false;
            manager.DocumentStatusChanged += delegate (object sender, EventArgs e)
            {
                called = true;
            };

            string documentName = "example.txt";

            manager.Load(documentName);

            ClassicAssert.AreEqual(false, manager.NewFile);
            ClassicAssert.AreEqual(true, manager.Opened);
            ClassicAssert.AreNotEqual(null, manager.Document);
            ClassicAssert.AreEqual(false, manager.Modified);
            ClassicAssert.AreEqual(documentName, manager.Document.Name);
            ClassicAssert.AreEqual(documentName, manager.Filename);
            ClassicAssert.AreEqual(true, called);
        }

        //* Save document
        //*  - filename
        //*  - event
        //*  - flags - modified, new file, opened
        [Test]
        public void TestSglDocMgrSaveDocument()
        {
            TestableSingleDocumentManager manager = new TestableSingleDocumentManager();

            // Setup a dummy document.
            manager.NewDocument();
            manager.Document.Modified = true;

            bool called = false;
            manager.DocumentStatusChanged += delegate (object sender, EventArgs e)
            {
                called = true;
            };

            ClassicAssert.AreEqual(true, manager.NewFile);
            ClassicAssert.AreEqual(true, manager.Opened);
            ClassicAssert.AreNotEqual(null, manager.Document);
            ClassicAssert.AreEqual(true, manager.Modified);
            ClassicAssert.AreEqual(false, called);

            string documentName = "example.txt";

            manager.Save(documentName);

            ClassicAssert.AreEqual(false, manager.NewFile);
            ClassicAssert.AreEqual(true, manager.Opened);
            ClassicAssert.AreNotEqual(null, manager.Document);
            ClassicAssert.AreEqual(false, manager.Modified);
            ClassicAssert.AreEqual(documentName, manager.Document.Name);
            ClassicAssert.AreEqual(documentName, manager.Filename);
            ClassicAssert.AreEqual(true, called);
        }

        //* Close document
        //*  - event???
        //*  - filename
        //*  - flags - modified, new file, opened
        [Test]
        public void TestSglDocMgrCloseDocument()
        {
            TestableSingleDocumentManager manager = new TestableSingleDocumentManager();

            // Setup a dummy document.
            manager.NewDocument();
            manager.Document.Modified = true;

            bool called = false;
            manager.DocumentStatusChanged += delegate (object sender, EventArgs e)
            {
                called = true;
            };

            ClassicAssert.AreEqual(true, manager.NewFile);
            ClassicAssert.AreEqual(true, manager.Opened);
            ClassicAssert.AreNotEqual(null, manager.Document);
            ClassicAssert.AreEqual(true, manager.Modified);
            ClassicAssert.AreEqual(false, called);

            manager.Close();

            ClassicAssert.AreEqual(false, manager.NewFile);
            ClassicAssert.AreEqual(false, manager.Opened);
            ClassicAssert.AreEqual(null, manager.Document);
            ClassicAssert.AreEqual(false, manager.Modified);
            ClassicAssert.AreEqual(null, manager.Filename);
            ClassicAssert.AreEqual(false, called);
        }
    }
}