/*
 * Copyright 2009-17 Williams Technologies Limited.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 * Kajbity is a trademark of Williams Technologies Limited.
 *
 * http://www.kajabity.com
 */

using Kajabity.DocForms.Documents;
using Kajabity.DocForms.Forms;
using System;
using System.Windows.Forms;

namespace PlainTextEditor
{
    /// <summary>
    /// Description of MainForm.
    /// </summary>
    public partial class MainForm : SingleDocumentForm<PlainTextDocument>
	{
		public MainForm() :
			base( new PlainTextDocumentManager() )
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();

            // Hide the menu items we haven't implmented yet - not forgetting spurious separators.
            toolsToolStripMenuItem.Visible = false;
            helpToolStripMenuItem.Visible = false;

            pageSetupToolStripMenuItem.Enabled = false;
            printToolStripMenuItem.Enabled = false;
            printPreviewToolStripMenuItem.Enabled = false;
            redoToolStripMenuItem.Visible = false;

            printToolStripButton.Enabled = false;
            helpToolStripButton.Enabled = false;

            // There will always be a document.
            NewDocument();
        }

        /// <summary>
        /// Update the view to show the changed document.
        /// </summary>
        private void MainForm_DocumentChanged(object sender, EventArgs e)
        {
            if (Manager.Opened)
            {
                textBox.Text = Manager.Document.Text;
                Manager.Document.Modified = false;
            }

            UpdateCommands();

            //	Force a display update.
            Refresh();
        }


        /// <summary>
        /// Update the displayed filename.
        /// </summary>
        private void MainForm_DocumentStatusChanged(object sender, EventArgs e)
        {
            string title = Application.ProductName;

            if (Manager.Opened)
            {
                //	Update main form heading.
                title += " - " + Manager.Document.Name;
            }

            if (Manager.Modified)
            {
                //	Update main form heading.
                title += @"*";
            }

            Text = title;

            UpdateCommands();
        }

        private void UpdateCommands()
        {
            // Some commands depend on whether or not the document is open.
            undoToolStripMenuItem.Enabled = textBox.CanUndo;
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            // Allow if a single file.
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length == 1)
                {
                    e.Effect = DragDropEffects.Copy;
                }
            }
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            // Open the file, if a single file.
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length == 1)
                {
                    LoadDocument(files[0]);
                }
            }
        }

        private void NewToolStripMenuItemClick(object sender, EventArgs e)
		{
			FileNewClick( sender, e );
		}

	    private void OpenToolStripMenuItemClick(object sender, EventArgs e)
		{
			FileOpenClick( sender, e );
		}

	    private void SaveToolStripMenuItemClick(object sender, EventArgs e)
		{
			FileSaveClick( sender, e );
		}

	    private void SaveAsToolStripMenuItemClick(object sender, EventArgs e)
		{
			FileSaveAsClick( sender, e );
		}

        private void ExitToolStripMenuItemClick(object sender, EventArgs e)
		{
			FileExitClick( sender, e );
		}

        private void CutToolStripButtonClick(object sender, EventArgs e)
		{
			textBox.Cut();
            UpdateCommands();
        }

        private void CopyToolStripButtonClick(object sender, EventArgs e)
		{
			textBox.Copy();
            UpdateCommands();
        }

        private void PasteToolStripButtonClick(object sender, EventArgs e)
		{
			textBox.Paste();
            UpdateCommands();
        }

        private void UndoToolStripMenuItemClick(object sender, EventArgs e)
		{
			textBox.Undo();
            UpdateCommands();
		}

        private void SelectAllToolStripMenuItemClick(object sender, EventArgs e)
		{
			textBox.SelectAll();
		}

	    private void DeleteToolStripMenuItemClick(object sender, EventArgs e)
		{
			textBox.SelectedText = "";
		}

	    private void EditToolStripMenuItemDropDownOpening(object sender, EventArgs e)
		{
			undoToolStripMenuItem.Enabled = textBox.CanUndo;
		}

        private void CloseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileCloseClick(sender, e);
        }

        private void textBox_TextChanged(object sender, EventArgs e)
        {
            Manager.Document.Modified = true;
        }
    }
}
