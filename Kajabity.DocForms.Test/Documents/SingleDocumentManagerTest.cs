using NUnit.Framework;
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

            Assert.AreEqual(false, manager.NewFile);
            Assert.AreEqual(false, manager.Opened);
            Assert.AreEqual(null, manager.Document);
            Assert.AreEqual(false, manager.Modified);
            Assert.AreEqual(null, manager.Filename);
        }

        //* Can get and set default extension property.
        [Test]
        public void TestSglDocMgrDefaultExtensionProperty()
        {
            TestableSingleDocumentManager manager = new TestableSingleDocumentManager();

            Assert.AreEqual(null, manager.DefaultExtension);

            string changedExtension = "abc";
            manager.DefaultExtension = changedExtension;

            Assert.AreEqual(changedExtension, manager.DefaultExtension);

            manager.NewDocument();

            string expectedFilename = TestableSingleDocumentManager.DEFAULT_DOCUMENT_NAME + "1." + changedExtension;

            Assert.AreEqual(expectedFilename, manager.Document.Name);
            Assert.AreEqual(expectedFilename, manager.Filename);
        }

        //* Can get and set default name property
        [Test]
        public void TestSglDocMgrDefaultNameProperty()
        {
            TestableSingleDocumentManager manager = new TestableSingleDocumentManager();

            Assert.AreEqual(TestableSingleDocumentManager.DEFAULT_DOCUMENT_NAME, manager.DefaultName);

            string changedName = "a-name";
            manager.DefaultName = changedName;

            Assert.AreEqual(changedName, manager.DefaultName);
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

            Assert.AreEqual(false, manager.NewFile);
            Assert.AreEqual(false, manager.Opened);
            Assert.AreEqual(null, manager.Document);
            Assert.AreEqual(false, called);

            manager.NewDocument();

            Assert.AreEqual(true, manager.NewFile);
            Assert.AreEqual(true, manager.Opened);
            Assert.AreNotEqual(null, manager.Document);
            Assert.AreEqual(true, called);
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

            Assert.AreEqual(false, manager.NewFile);
            Assert.AreEqual(true, manager.Opened);
            Assert.AreNotEqual(null, manager.Document);
            Assert.AreEqual(false, manager.Modified);
            Assert.AreEqual(documentName, manager.Document.Name);
            Assert.AreEqual(documentName, manager.Filename);
            Assert.AreEqual(true, called);
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

            Assert.AreEqual(true, manager.NewFile);
            Assert.AreEqual(true, manager.Opened);
            Assert.AreNotEqual(null, manager.Document);
            Assert.AreEqual(true, manager.Modified);
            Assert.AreEqual(false, called);

            string documentName = "example.txt";

            manager.Save(documentName);

            Assert.AreEqual(false, manager.NewFile);
            Assert.AreEqual(true, manager.Opened);
            Assert.AreNotEqual(null, manager.Document);
            Assert.AreEqual(false, manager.Modified);
            Assert.AreEqual(documentName, manager.Document.Name);
            Assert.AreEqual(documentName, manager.Filename);
            Assert.AreEqual(true, called);
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

            Assert.AreEqual(true, manager.NewFile);
            Assert.AreEqual(true, manager.Opened);
            Assert.AreNotEqual(null, manager.Document);
            Assert.AreEqual(true, manager.Modified);
            Assert.AreEqual(false, called);

            manager.Close();

            Assert.AreEqual(false, manager.NewFile);
            Assert.AreEqual(false, manager.Opened);
            Assert.AreEqual(null, manager.Document);
            Assert.AreEqual(false, manager.Modified);
            Assert.AreEqual(null, manager.Filename);
            Assert.AreEqual(false, called);
        }
    }
}