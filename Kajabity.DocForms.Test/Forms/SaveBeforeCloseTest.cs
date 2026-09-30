using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Kajabity.DocForms.Documents;
using Kajabity.DocForms.Forms;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Kajabity.DocForms.Test.Forms
{
    // Issue #3: exercise the real command handlers without showing modal dialogs.
    // Each test writes only inside its own temporary directory.
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    [NonParallelizable]
    [Category("Issue3")]
    public class SaveBeforeCloseTest
    {
        private RecordingManager _manager;
        private TestForm _form;
        private TestDocument _original;
        private string _originalFilename;
        private string _originalName;
        private bool _originalNewFile;
        private string _testDirectory;

        [SetUp]
        public void SetUp()
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), "DocFormsTests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_testDirectory);
            _manager = new RecordingManager();
            _form = new TestForm(_manager, _testDirectory);
        }

        [TearDown]
        public void TearDown()
        {
            // Dispose directly: teardown must not invoke the close/save workflow.
            _form.Dispose();
            Directory.Delete(_testDirectory, true);
        }

        [Test]
        public void CancelledSaveAsPreservesDocument(
            [Values("Close", "New", "Open", "Recent")] string command)
        {
            PrepareDocument(true);
            _form.SaveAsResult = DialogResult.Cancel;

            RunCommand(command);

            AssertPreserved();
            ClassicAssert.AreEqual(1, _form.PromptCount);
            ClassicAssert.AreEqual(1, _form.SaveAsCount);
            ClassicAssert.AreEqual(0, _manager.SaveCount);
            ClassicAssert.IsNull(_form.SaveError);
        }

        [Test]
        public void FailedSavePreservesDocument(
            [Values("Close", "New", "Open", "Recent")] string command,
            [Values(false, true)] bool newFile)
        {
            PrepareDocument(newFile);
            _manager.SaveException = new IOException("Simulated write failure.");

            RunCommand(command);

            AssertPreserved();
            ClassicAssert.AreEqual(1, _manager.SaveCount);
            ClassicAssert.AreEqual(newFile ? 1 : 0, _form.SaveAsCount);
            ClassicAssert.AreSame(_manager.SaveException, _form.SaveError);
            ClassicAssert.AreEqual(1, _form.ErrorCount);
        }

        [Test]
        public void CancelledConfirmationPreservesDocument(
            [Values("Close", "New", "Open", "Recent")] string command)
        {
            PrepareDocument(false);
            _form.PromptResult = DialogResult.Cancel;

            RunCommand(command);

            AssertPreserved();
            ClassicAssert.AreEqual(1, _form.PromptCount);
            ClassicAssert.AreEqual(0, _form.SaveAsCount);
            ClassicAssert.AreEqual(0, _manager.SaveCount);
        }

        [Test]
        public void SuccessfulSaveAllowsCommand(
            [Values("Close", "New", "Open", "Recent")] string command,
            [Values(false, true)] bool newFile)
        {
            PrepareDocument(newFile);

            RunCommand(command);

            ClassicAssert.AreEqual(1, _manager.SaveCount);
            ClassicAssert.AreEqual("Unsaved edits", _manager.SavedText);
            string destination = newFile ? _form.SavePath : _originalFilename;
            ClassicAssert.AreEqual("Unsaved edits", File.ReadAllText(destination));
            ClassicAssert.AreNotEqual(destination, _manager.SavedFilename, "Write to a temporary file first.");
            ClassicAssert.AreEqual(Path.GetDirectoryName(destination), Path.GetDirectoryName(_manager.SavedFilename));
            ClassicAssert.IsFalse(File.Exists(_manager.SavedFilename));
            ClassicAssert.AreEqual(newFile ? 1 : 0, _form.SaveAsCount);
            ClassicAssert.IsFalse(_original.Modified);
            ClassicAssert.IsNull(_form.SaveError);
            AssertCommandCompleted(command);
        }

        [Test]
        public void DiscardAllowsCommandWithoutSaving(
            [Values("Close", "New", "Open", "Recent")] string command)
        {
            PrepareDocument(false);
            _form.PromptResult = DialogResult.No;

            RunCommand(command);

            ClassicAssert.AreEqual(1, _form.PromptCount);
            ClassicAssert.AreEqual(0, _manager.SaveCount);
            ClassicAssert.AreEqual(0, _form.SaveAsCount);
            AssertCommandCompleted(command);
        }

        [Test]
        public void UnmodifiedDocumentNeedsNoPromptOrSave(
            [Values("Close", "New", "Open", "Recent")] string command)
        {
            PrepareDocument(false);
            _original.Modified = false;

            RunCommand(command);

            ClassicAssert.AreEqual(0, _form.PromptCount);
            ClassicAssert.AreEqual(0, _manager.SaveCount);
            AssertCommandCompleted(command);
        }

        [TestCase(DialogResult.Yes, DialogResult.Cancel, true, false, false)]
        [TestCase(DialogResult.Yes, DialogResult.OK, true, true, false)]
        [TestCase(DialogResult.Yes, DialogResult.OK, false, true, false)]
        [TestCase(DialogResult.Yes, DialogResult.OK, true, false, true)]
        [TestCase(DialogResult.Yes, DialogResult.OK, false, false, true)]
        [TestCase(DialogResult.No, DialogResult.OK, false, false, true)]
        [TestCase(DialogResult.Cancel, DialogResult.OK, false, false, false)]
        public void AttemptCloseReportsWhetherDocumentWasClosed(
            DialogResult confirmation, DialogResult saveAs, bool newFile,
            bool saveFails, bool expectedClosed)
        {
            PrepareDocument(newFile);
            _form.PromptResult = confirmation;
            _form.SaveAsResult = saveAs;
            if (saveFails)
                _manager.SaveException = new IOException("Simulated write failure.");

            bool closed = _form.AttemptClose();

            ClassicAssert.AreEqual(expectedClosed, closed, "The caller must know whether it may continue.");
            if (expectedClosed)
                AssertCommandCompleted("Close");
            else
                AssertPreserved();
        }

        private void PrepareDocument(bool newFile)
        {
            if (newFile)
                _manager.NewDocument();
            else
            {
                string filename = Path.Combine(_testDirectory, "original.txt");
                File.WriteAllText(filename, "Original contents");
                _manager.Load(filename);
            }

            _original = _manager.Document;
            _original.Text = "Unsaved edits";
            _original.Modified = true;
            _originalFilename = _manager.Filename;
            _originalName = _original.Name;
            _originalNewFile = _manager.NewFile;
            _manager.NewCount = 0;
            _manager.LoadCount = 0;
        }

        [TestCase(DialogResult.Cancel, DialogResult.OK, false)]
        [TestCase(DialogResult.Yes, DialogResult.Cancel, false)]
        [TestCase(DialogResult.Yes, DialogResult.OK, true)]
        public void WindowCloseIsCancelledWhenDocumentIsPreserved(
            DialogResult confirmation, DialogResult saveAs, bool saveFails)
        {
            PrepareDocument(true);
            _form.PromptResult = confirmation;
            _form.SaveAsResult = saveAs;
            if (saveFails)
                _manager.SaveException = new IOException("Simulated write failure.");

            ClassicAssert.IsFalse(_form.AttemptWindowClose());
            AssertPreserved();
        }

        [TestCase(false)]
        [TestCase(true)]
        [Category("SaveSafety")]
        public void RepeatedSaveReplacesExistingFileAndUpdatesBackup(bool backup)
        {
            PrepareDocument(false);
            _form.Backup = backup;
            File.WriteAllText(_originalFilename + "~", "Older backup");
            File.WriteAllText(_originalFilename + ".tmp", "Unrelated temporary file");

            _form.FileSaveClick(_form, EventArgs.Empty);

            AssertSuccessfulSave(_originalFilename, "Unsaved edits");
            ClassicAssert.AreEqual(backup ? "Original contents" : "Older backup",
                File.ReadAllText(_originalFilename + "~"));
            _original.Text = "Second edit";
            _original.Modified = true;

            _form.FileSaveClick(_form, EventArgs.Empty);

            AssertSuccessfulSave(_originalFilename, "Second edit");
            ClassicAssert.AreEqual(backup ? "Unsaved edits" : "Older backup",
                File.ReadAllText(_originalFilename + "~"));
            ClassicAssert.AreEqual("Unrelated temporary file", File.ReadAllText(_originalFilename + ".tmp"));
        }

        [TestCase(false, false)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        [Category("SaveSafety")]
        public void SaveAsCommitsFinalFilename(bool backup, bool destinationExists)
        {
            PrepareDocument(true);
            _form.Backup = backup;
            if (destinationExists)
                File.WriteAllText(_form.SavePath, "Previous destination");

            // Observers must never see a temporary filename or a prematurely clean document.
            _manager.DocumentStatusChanged += (sender, args) =>
            {
                ClassicAssert.AreEqual(_form.SavePath, _manager.Filename);
                ClassicAssert.AreEqual("Unsaved edits", File.ReadAllText(_form.SavePath));
            };

            _form.FileSaveAsClick(_form, EventArgs.Empty);

            AssertSuccessfulSave(_form.SavePath, "Unsaved edits");
            ClassicAssert.AreEqual(backup && destinationExists, File.Exists(_form.SavePath + "~"));
            if (backup && destinationExists)
                ClassicAssert.AreEqual("Previous destination", File.ReadAllText(_form.SavePath + "~"));
        }

        [TestCase(false)]
        [TestCase(true)]
        [Category("SaveSafety")]
        public void PartialWriteFailurePreservesOriginalAndBackup(bool backup)
        {
            PrepareDocument(false);
            _form.Backup = backup;
            File.WriteAllText(_originalFilename + "~", "Previous backup");
            _manager.SaveException = new IOException("Simulated partial write.");

            _form.FileSaveClick(_form, EventArgs.Empty);

            AssertPreserved();
            ClassicAssert.AreSame(_manager.SaveException, _form.SaveError);
            ClassicAssert.AreEqual("Original contents", File.ReadAllText(_originalFilename));
            ClassicAssert.AreEqual("Previous backup", File.ReadAllText(_originalFilename + "~"));
            ClassicAssert.IsFalse(File.Exists(_manager.SavedFilename), "Clean up the partial temporary file.");
        }

        [TestCase(false, false)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        [Category("SaveSafety")]
        public void ReplacementFailurePreservesStateAndAllowsRetry(bool backup, bool newFile)
        {
            PrepareDocument(newFile);
            _form.Backup = backup;
            string destination = newFile ? _form.SavePath : _originalFilename;
            File.WriteAllText(destination, "Original contents");
            File.WriteAllText(destination + "~", "Previous backup");
            int statusChanges = 0;
            _manager.DocumentStatusChanged += (sender, args) => statusChanges++;

            using (new FileStream(destination, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                ClassicAssert.IsFalse(_form.AttemptClose());
                AssertPreserved();
                ClassicAssert.IsInstanceOf<IOException>(_form.SaveError);
                ClassicAssert.AreEqual(0, statusChanges, "Do not report a successful save before replacement.");
                ClassicAssert.IsFalse(File.Exists(_manager.SavedFilename));
            }

            ClassicAssert.AreEqual("Original contents", File.ReadAllText(destination));
            ClassicAssert.AreEqual("Previous backup", File.ReadAllText(destination + "~"));
            _form.SaveError = null;
            _form.FileSaveClick(_form, EventArgs.Empty);
            AssertSuccessfulSave(destination, "Unsaved edits");
        }

        private void AssertSuccessfulSave(string filename, string text)
        {
            ClassicAssert.IsNull(_form.SaveError);
            ClassicAssert.AreEqual(text, File.ReadAllText(filename));
            ClassicAssert.AreSame(_original, _manager.Document);
            ClassicAssert.AreEqual(filename, _manager.Filename);
            ClassicAssert.AreEqual(Path.GetFileName(filename), _original.Name);
            ClassicAssert.IsFalse(_manager.Modified);
            ClassicAssert.IsFalse(_manager.NewFile);
            ClassicAssert.IsFalse(File.Exists(_manager.SavedFilename));
        }

        private void RunCommand(string command)
        {
            switch (command)
            {
                case "Close": _form.FileCloseClick(_form, EventArgs.Empty); break;
                case "New": _form.FileNewClick(_form, EventArgs.Empty); break;
                case "Open": _form.FileOpenClick(_form, EventArgs.Empty); break;
                case "Recent": _form.OpenRecent(); break;
                default: throw new ArgumentException("Unknown command", nameof(command));
            }
        }

        private void AssertPreserved()
        {
            Assert.Multiple((Action)(() =>
            {
                ClassicAssert.AreSame(_original, _manager.Document, "Keep the same in-memory document.");
                ClassicAssert.IsTrue(_manager.Opened);
                ClassicAssert.IsTrue(_manager.Modified, "Unsaved changes must remain marked as modified.");
                ClassicAssert.AreEqual("Unsaved edits", _original.Text);
                ClassicAssert.AreEqual(_originalFilename, _manager.Filename);
                ClassicAssert.AreEqual(_originalName, _original.Name);
                ClassicAssert.AreEqual(_originalNewFile, _manager.NewFile);
                ClassicAssert.AreEqual(0, _manager.CloseCount);
                ClassicAssert.AreEqual(0, _manager.NewCount, "Do not replace the document with a new one.");
                ClassicAssert.AreEqual(0, _manager.LoadCount, "Do not load the requested replacement.");
                ClassicAssert.AreEqual(0, _form.DocumentChangedCount);
            }));
        }

        private void AssertCommandCompleted(string command)
        {
            ClassicAssert.AreEqual(1, _manager.CloseCount);
            ClassicAssert.AreEqual(command == "New" ? 1 : 0, _manager.NewCount);
            ClassicAssert.AreEqual(command == "Open" || command == "Recent" ? 1 : 0, _manager.LoadCount);
            if (command == "Close")
                ClassicAssert.IsFalse(_manager.Opened);
            else
            {
                ClassicAssert.AreNotSame(_original, _manager.Document);
                ClassicAssert.IsTrue(_manager.Opened);
                if (command == "New")
                    ClassicAssert.IsTrue(_manager.NewFile);
                else
                    ClassicAssert.AreEqual(_form.OpenPath, _manager.Filename);
            }
        }

        private sealed class TestDocument : Document
        {
            public string Text { get; set; }
        }

        private sealed class RecordingManager : SingleDocumentManager<TestDocument>
        {
            public int NewCount;
            public int LoadCount;
            public int CloseCount;
            public int SaveCount;
            public Exception SaveException;
            public string SavedFilename;
            public string SavedText;

            public RecordingManager()
            {
                DefaultExtension = "txt";
            }

            public override void NewDocument()
            {
                NewCount++;
                Document = new TestDocument();
                base.NewDocument();
            }

            public override void Load(string filename)
            {
                LoadCount++;
                Document = new TestDocument();
                base.Load(filename);
            }

            public override void Save(string filename)
            {
                SaveCount++;
                SavedFilename = filename;
                File.WriteAllText(filename, "Partial write");
                if (SaveException != null)
                    throw SaveException;
                SavedText = Document.Text;
                File.WriteAllText(filename, SavedText);
                base.Save(filename);
            }

            public override void Close()
            {
                CloseCount++;
                base.Close();
            }
        }

        private sealed class TestForm : SingleDocumentForm<TestDocument>
        {
            public DialogResult PromptResult = DialogResult.Yes;
            public DialogResult SaveAsResult = DialogResult.OK;
            public int PromptCount;
            public int SaveAsCount;
            public int ErrorCount;
            public int DocumentChangedCount;
            public Exception SaveError;
            public readonly string SavePath;
            public readonly string OpenPath;

            public TestForm(RecordingManager manager, string directory) : base(manager)
            {
                SavePath = Path.Combine(directory, "save-as.txt");
                OpenPath = Path.Combine(directory, "replacement.txt");
                Backup = false;
                DocumentChanged += (sender, args) => DocumentChangedCount++;
            }

            protected override DialogResult PromptToSaveChanges()
            {
                PromptCount++;
                return PromptResult;
            }

            protected override void ShowSaveError(Exception exception)
            {
                ErrorCount++;
                SaveError = exception;
            }

            protected override IDocumentSelector GetSaveAsDocumentSelector()
            {
                SaveAsCount++;
                return new StubSelector(SavePath, SaveAsResult);
            }

            protected override IDocumentSelector GetOpenDocumentSelector()
            {
                return new StubSelector(OpenPath, DialogResult.OK);
            }

            public bool AttemptClose()
            {
                return AttemptCloseDocument(this, EventArgs.Empty);
            }

            public bool AttemptWindowClose()
            {
                var args = new FormClosingEventArgs(CloseReason.UserClosing, false);
                OnFormClosing(args);
                return !args.Cancel;
            }

            public void OpenRecent()
            {
                using (var item = new ToolStripMenuItem { Tag = OpenPath })
                    OnRecentDocumentMenuItemClicked(item, EventArgs.Empty);
            }
        }

        private sealed class StubSelector : IDocumentSelector
        {
            private readonly DialogResult _result;
            public string FileName { get; private set; }

            public StubSelector(string filename, DialogResult result)
            {
                FileName = filename;
                _result = result;
            }

            public DialogResult ShowDialog(IWin32Window owner)
            {
                return _result;
            }
        }
    }
}
