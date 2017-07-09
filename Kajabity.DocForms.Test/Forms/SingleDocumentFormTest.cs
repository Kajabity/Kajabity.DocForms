using NUnit.Framework;
using PlainTextEditor;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace Kajabity.DocForms.Test.Forms
{
    [TestFixture]
    public class SingleDocumentFormTest
    {
        [Test]
        public void TestSglDocFrmVisible()
        {
            PlainTextEditorMainForm underTest = new PlainTextEditorMainForm();
            underTest.Show();

            Assert.AreEqual(true, underTest.Visible);
            Assert.AreEqual("Plain Text Editor", underTest.Text);
            Assert.AreEqual(false, underTest.Manager.Opened);
            Assert.AreEqual(false, underTest.Manager.Modified);
            Assert.AreEqual(false, underTest.Manager.NewFile);
            Assert.AreEqual(null, underTest.Manager.Document);
            Assert.AreEqual("Text Document", underTest.Manager.DefaultName);
            Assert.AreEqual("txt", underTest.Manager.DefaultExtension);
            Assert.AreEqual(null, underTest.Manager.Filename);
        }

        [Test]
        public void TestSglDocFrmNewDocument()
        {
            PlainTextEditorMainForm underTest = new PlainTextEditorMainForm();
            underTest.Show();

            ToolStripMenuItem newToolStripMenuItem = findMenuItem(underTest, "&New");
            newToolStripMenuItem.PerformClick();

            Assert.AreEqual("Plain Text Editor - Text Document1.txt", underTest.Text);
            Assert.AreEqual(true, underTest.Manager.Opened);
            Assert.AreEqual(false, underTest.Manager.Modified);
            Assert.AreEqual(true, underTest.Manager.NewFile);
            Assert.AreNotEqual(null, underTest.Manager.Document);
            Assert.AreEqual("Text Document", underTest.Manager.DefaultName);
            Assert.AreEqual("txt", underTest.Manager.DefaultExtension);
            Assert.AreEqual("Text Document1.txt", underTest.Manager.Filename);

            Control ctrl = findControl(underTest, "textBox");
            ctrl.Text = "Add some text to the control.";

            Assert.AreEqual("Plain Text Editor - Text Document1.txt*", underTest.Text);
            Assert.AreEqual(true, underTest.Manager.Opened);
            Assert.AreEqual(true, underTest.Manager.Modified);
            Assert.AreEqual(true, underTest.Manager.NewFile);
            Assert.AreNotEqual(null, underTest.Manager.Document);
            Assert.AreEqual("Text Document1.txt", underTest.Manager.Filename);
        }

        [Test]
        public void TestSglDocFrmOpenDocument()
        {
            string filename = Path.Combine(TestContext.CurrentContext.TestDirectory, "Forms\\test.txt");

            PlainTextEditorMainForm underTest = new TestablePlainDocumentMainForm(filename);
            underTest.Show();

            ToolStripMenuItem menuItem = findMenuItem(underTest, "&Open");
            menuItem.PerformClick();

            Assert.AreEqual("Plain Text Editor - test.txt", underTest.Text);
            Assert.AreEqual(true, underTest.Manager.Opened);
            Assert.AreEqual(false, underTest.Manager.Modified);
            Assert.AreEqual(false, underTest.Manager.NewFile);
            Assert.AreNotEqual(null, underTest.Manager.Document);
            Assert.AreEqual(filename, underTest.Manager.Filename);

            Control ctrl = findControl(underTest, "textBox");
            ctrl.Text = "Add some text to the control.";

            Assert.AreEqual("Plain Text Editor - test.txt*", underTest.Text);
            Assert.AreEqual(true, underTest.Manager.Opened);
            Assert.AreEqual(true, underTest.Manager.Modified);
            Assert.AreEqual(false, underTest.Manager.NewFile);
            Assert.AreNotEqual(null, underTest.Manager.Document);
            Assert.AreEqual(filename, underTest.Manager.Filename);
        }


        [Test]
        public void TestSglDocFrmSaveDocument()
        {
            string filename = Path.Combine(TestContext.CurrentContext.TestDirectory, "Forms\\test-save.txt");
            FileInfo fileInfo = new FileInfo(filename);
            if (fileInfo.Exists)
            {
                fileInfo.Delete();
            }

            PlainTextEditorMainForm underTest = new TestablePlainDocumentMainForm(filename);
            underTest.Show();

            ToolStripMenuItem newToolStripMenuItem = findMenuItem(underTest, "&New");
            newToolStripMenuItem.PerformClick();

            Control ctrl = findControl(underTest, "textBox");
            ctrl.Text = "Add some text to be saved.";

            ToolStripMenuItem menuItem = findMenuItem(underTest, "&Save");
            menuItem.PerformClick();

            Assert.AreEqual("Plain Text Editor - test-save.txt", underTest.Text);
            Assert.AreEqual(true, underTest.Manager.Opened);
            Assert.AreEqual(false, underTest.Manager.Modified);
            Assert.AreEqual(false, underTest.Manager.NewFile);
            Assert.AreNotEqual(null, underTest.Manager.Document);
            Assert.AreEqual(filename, underTest.Manager.Filename);

            if (fileInfo.Exists)
            {
                fileInfo.Delete();
            }
        }

        // Save As

        [Test]
        public void TestSglDocFrmCloseDocument()
        {
            string filename = Path.Combine(TestContext.CurrentContext.TestDirectory, "Forms\\test.txt");

            PlainTextEditorMainForm underTest = new TestablePlainDocumentMainForm(filename);
            underTest.Show();

            ToolStripMenuItem menuItem = findMenuItem(underTest, "&Open");
            menuItem.PerformClick();

            Assert.AreEqual("Plain Text Editor - test.txt", underTest.Text);
            Assert.AreEqual(true, underTest.Manager.Opened);
            Assert.AreEqual(false, underTest.Manager.Modified);
            Assert.AreEqual(false, underTest.Manager.NewFile);
            Assert.AreNotEqual(null, underTest.Manager.Document);
            Assert.AreEqual(filename, underTest.Manager.Filename);

            menuItem = findMenuItem(underTest, "&Close");
            menuItem.PerformClick();

            Assert.AreEqual("Plain Text Editor", underTest.Text);
            Assert.AreEqual(false, underTest.Manager.Opened);
            Assert.AreEqual(false, underTest.Manager.Modified);
            Assert.AreEqual(false, underTest.Manager.NewFile);
            Assert.AreEqual(null, underTest.Manager.Document);
            Assert.AreEqual(null, underTest.Manager.Filename);
        }

        // Modified, New - causes popup prompt to save.
        // Modified, Open - causes popup prompt to save.
        // Modified, Save As - not modified.
        // Modifed, Close - causes popup prompt to save.
        // Modifed, Exit - causes popup prompt to save.

        // Exit

        ToolStripMenuItem findMenuItem(Form form, string name)
        {
            foreach (ToolStripMenuItem menu in form.MainMenuStrip.Items)
            {
                if (menu.Name.Equals(name) || menu.Text.Equals(name))
                {
                    return menu;
                }
                else if (menu.DropDownItems.Count > 0)
                {
                    ToolStripMenuItem subItem = findSubMenuItem(menu, name);
                    if (subItem != null)
                    {
                        return subItem;
                    }
                }
            }

            return null;
        }

        ToolStripMenuItem findSubMenuItem(ToolStripMenuItem parentMenu, string name)
        {
            foreach (ToolStripItem item in parentMenu.DropDownItems)
            {
                if (item is ToolStripMenuItem)
                {
                    ToolStripMenuItem menu = (ToolStripMenuItem)item;
                    if (menu.Name.Equals(name) || menu.Text.Equals(name))
                    {
                        return (ToolStripMenuItem)item;
                    }
                    else if (menu.DropDownItems.Count > 0)
                    {
                        ToolStripMenuItem subItem = findSubMenuItem(menu, name);
                        if (subItem != null)
                        {
                            return subItem;
                        }
                    }
                }
            }

            return null;
        }

        Control findControl(Control control, string name)
        {
            foreach (Control childControl in control.Controls)
            {
                if (childControl.Name.Equals(name) || childControl.Text.Equals(name))
                {
                    return childControl;
                }
                else if (childControl.Controls.Count > 0)
                {
                    Control subItem = findControl(childControl, name);
                    if (subItem != null)
                    {
                        return subItem;
                    }
                }
            }

            return null;
        }
    }
}
