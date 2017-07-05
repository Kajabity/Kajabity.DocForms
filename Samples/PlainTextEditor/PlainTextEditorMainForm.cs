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

using Kajabity.DocForms.Forms;
using System;
using System.Windows.Forms;

namespace PlainTextEditor
{
    /// <summary>
    /// The main window of the application.
    /// </summary>
    public partial class PlainTextEditorMainForm : SingleDocumentForm<PlainTextDocument>
    {
        public PlainTextEditorMainForm() :
            base(new PlainTextDocumentManager())
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

            InitialseRecentDocuments(recentFilesToolStripMenuItem);
        }

        /// <summary>
        /// Update the view to show the changed document.
        /// </summary>
        private void MainForm_DocumentChanged(object sender, EventArgs e)
        {
            if (Manager.Opened)
            {
                textBox.Enabled = true;
                textBox.BackColor = System.Drawing.SystemColors.Window;

                textBox.Text = Manager.Document.Text;
                Manager.Document.Modified = false;
            }
            else
            {
                textBox.Enabled = false;
                textBox.BackColor = System.Drawing.SystemColors.AppWorkspace;
                textBox.Text = "";
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
            // Would use 'Application.ProductName' but this doesn't work with NUnit tests!
            string title = "Plain Text Editor"; 

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
            undoToolStripMenuItem.Enabled = textBox.Modified;
        }

        //  ---------------------------------------------------------------------
        #region File Menu Handlers
        //  ---------------------------------------------------------------------

        private void FileToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            OnParentMenuDropDownOpening(sender, e);
        }

        private void NewToolStripMenuItemClick(object sender, EventArgs e)
        {
            FileNewClick(sender, e);
        }

        private void OpenToolStripMenuItemClick(object sender, EventArgs e)
        {
            FileOpenClick(sender, e);
        }

        private void closeToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            FileCloseClick(sender, e);
        }

        private void SaveToolStripMenuItemClick(object sender, EventArgs e)
        {
            FileSaveClick(sender, e);
        }

        private void SaveAsToolStripMenuItemClick(object sender, EventArgs e)
        {
            FileSaveAsClick(sender, e);
        }

        private void ExitToolStripMenuItemClick(object sender, EventArgs e)
        {
            FileExitClick(sender, e);
        }
        #endregion

        //  ---------------------------------------------------------------------
        #region Edit Menu Handlers
        //  ---------------------------------------------------------------------

        private void UndoToolStripMenuItemClick(object sender, EventArgs e)
        {
            textBox.Undo();
        }

        private void CutToolStripButtonClick(object sender, EventArgs e)
        {
            textBox.Cut();
        }

        private void CopyToolStripButtonClick(object sender, EventArgs e)
        {
            textBox.Copy();
        }

        private void PasteToolStripButtonClick(object sender, EventArgs e)
        {
            textBox.Paste();
        }

        private void SelectAllToolStripMenuItemClick(object sender, EventArgs e)
        {
            textBox.SelectAll();
        }

        private void DeleteToolStripMenuItemClick(object sender, EventArgs e)
        {
            textBox.SelectedText = "";
        }
        #endregion

        //  ---------------------------------------------------------------------
        #region TextBox Event Handlers
        //  ---------------------------------------------------------------------

        /// <summary>
        /// Update the model (Document) each time the text box contents are modified.
        /// This updates Manager.Document.Modified and triggers the DocumentStatusChanged event, handled above.
        /// </summary>
        /// <param name="sender">The window or control originating the event.</param>
        /// <param name="e">Event details.</param>
        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            if (Manager.Opened)
            {
                Manager.Document.Text = textBox.Text;
            }
        }
        #endregion

        //  ---------------------------------------------------------------------
    }
}
