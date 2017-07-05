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
        }

        [Test]
        public void TestSglDocFrmOpenDocument()
        {
            string filename = Path.Combine(TestContext.CurrentContext.TestDirectory, "Forms\\test.txt");
            Debug.WriteLine("TestOpenDocument: filename = " + filename);

            PlainTextEditorMainForm underTest = new TestablePlainDocumentMainForm(filename);
            underTest.Show();

            ToolStripMenuItem menuItem = findMenuItem(underTest, "&Open");
            menuItem.PerformClick();

            Assert.AreEqual("Plain Text Editor - test.txt", underTest.Text);
        }

        // Save
        // Save As
        // Close Document
        // Exit

        // Modified - title ends with asterisk.

        // Modified, New - causes popup prompt to save.
        // Modified, Open - causes popup prompt to save.
        // Modified, Save - not modified.
        // Modified, Save As - not modified.
        // Modifed, Close - causes popup prompt to save.
        // Modifed, Exit - causes popup prompt to save.

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
    }
}
