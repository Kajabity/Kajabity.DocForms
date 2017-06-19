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
using System.Diagnostics;
using System.Windows.Forms;

namespace CsvEditor
{
    public partial class CsvEditorMainForm: SingleDocumentForm<CsvDocument>
    {
        public CsvEditorMainForm()
            : base( new CsvDocumentManager() )
        {
            //
            // The InitializeComponent() call is required for Windows Forms designer support.
            //
            InitializeComponent();

            // Hide the menu items we haven't implmented yet - not forgetting spurious separators.
            printToolStripMenuItem.Visible = false;
            printPreviewToolStripMenuItem.Visible = false;
            toolStripSeparator8.Visible = false;
            editToolStripMenuItem.Visible = false;
            toolsToolStripMenuItem.Visible = false;
            helpToolStripMenuItem.Visible = false;

            // Hide the tool strip buttons we haven't implmented yet - not forgetting spurious separators.
            printToolStripButton.Visible = false;
            toolStripSeparator6.Visible = false;
            cutToolStripButton.Visible = false;
            copyToolStripButton.Visible = false;
            pasteToolStripButton.Visible = false;
            helpToolStripButton.Visible = false;
            toolStripSeparator7.Visible = false;

            // Load recent documents.
            InitialseRecentDocuments( recentDocumentsToolStripMenuItem );
        }

        private void CsvEditorMainForm_DocumentChanged( object sender, EventArgs e )
        {
            dataGridUserControl.CsvDocument = Manager.Document;
            Refresh();
        }

        /// <summary>
        /// Update the displayed filename.
        /// </summary>
        private void CsvEditorMainForm_DocumentStatusChanged( object sender, EventArgs e )
        {
            string title = Application.ProductName;
            Debug.WriteLine( "==== HexViewerMainForm_DocumentStatusChanged() ====" );

            if( Manager.Opened )
            {
                //	Update main form heading.
                title += " - " + Manager.Document.Name;
                Debug.WriteLine( "\tManager.Document.Name=" + Manager.Document.Name );
            }

            if( Manager.Modified )
            {
                //	Update main form heading.
                Text += "*";
            }

            Text = title;

            UpdateCommands();
        }

        private void UpdateCommands()
        {
            // Some commands depend on whether or not the document is open.
            saveToolStripButton.Enabled = Manager.Opened;
            saveToolStripMenuItem.Enabled = Manager.Opened;

            saveAsToolStripMenuItem.Enabled = Manager.Opened;

            closeToolStripMenuItem.Enabled = Manager.Opened;
        }

        private void newToolStripMenuItem_Click( object sender, EventArgs e )
        {
            FileNewClick( sender, e );
        }

        private void openToolStripMenuItem_Click( object sender, EventArgs e )
        {
            FileOpenClick( sender, e );
        }

        private void saveToolStripMenuItem_Click( object sender, EventArgs e )
        {
            FileSaveClick( sender, e );
        }

        private void saveAsToolStripMenuItem_Click( object sender, EventArgs e )
        {
            FileSaveAsClick( sender, e );
        }

        private void closeToolStripMenuItem_Click( object sender, EventArgs e )
        {
            FileCloseClick( sender, e );
        }

        private void exitToolStripMenuItem_Click( object sender, EventArgs e )
        {
            FileExitClick( sender, e );
        }

        private void fileToolStripMenuItem_DropDownOpening( object sender, EventArgs e )
        {
            OnParentMenuDropDownOpening( sender, e );
        }
    }
}
