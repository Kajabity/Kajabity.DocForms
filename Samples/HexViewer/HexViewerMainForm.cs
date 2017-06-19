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

using System;
using System.Windows.Forms;
using Kajabity.DocForms.Forms;

namespace HexViewer
{
    /// <summary>
    /// The HexViewer MainForm.
    /// </summary>
    public partial class HexViewerMainForm: SingleDocumentForm<BinaryDocument>
    {
        public HexViewerMainForm()
            : base( new BinaryDocumentManager() )
        {
            //
            // The InitializeComponent() call is required for Windows Forms designer support.
            //
            InitializeComponent();

            // Hide the menu items we haven't implmented yet - not forgetting spurious separators.
            newToolStripButton.Visible = false;
            newToolStripMenuItem.Visible = false;
            toolStripSeparator1.Visible = false;
            saveAsToolStripMenuItem.Visible = false;
            saveToolStripButton.Visible = false;
            saveToolStripMenuItem.Visible = false;
            toolStripSeparator2.Visible = false;
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

        /// <summary>
        /// Update the view to show the changed document.
        /// </summary>
        private void HexViewerMainForm_DocumentChanged( object sender, EventArgs e )
        {
            panel.BinaryDocument = Manager.Document;
            Refresh();
        }


        /// <summary>
        /// Update the displayed filename.
        /// </summary>
        private void HexViewerMainForm_DocumentStatusChanged( object sender, EventArgs e )
        {
            string title = Application.ProductName;

            if( Manager.Opened )
            {
                //	Update main form heading.
                title += " - " + Manager.Document.Name;
            }

            if( Manager.Modified )
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
            closeToolStripMenuItem.Enabled = Manager.Opened;
        }

        void OpenToolStripMenuItemClick( object sender, EventArgs e )
        {
            FileOpenClick( sender, e );
        }

        void ExitToolStripMenuItemClick( object sender, EventArgs e )
        {
            FileExitClick( sender, e );
        }

        private void FileToolStripMenuItem_DropDownOpening( object sender, EventArgs e )
        {
            OnParentMenuDropDownOpening( sender, e );
        }

        private void MainForm_DragEnter( object sender, DragEventArgs e )
        {
            // Allow if a single file.
            if( e.Data.GetDataPresent( DataFormats.FileDrop ) )
            {
                string[] files = (string[]) e.Data.GetData( DataFormats.FileDrop );
                if( files.Length == 1 )
                {
                    e.Effect = DragDropEffects.Copy;
                }
            }
        }

        private void MainForm_DragDrop( object sender, DragEventArgs e )
        {
            // Open the file, if a single file.
            if( e.Data.GetDataPresent( DataFormats.FileDrop ) )
            {
                string[] files = (string[]) e.Data.GetData( DataFormats.FileDrop );
                if( files.Length == 1 )
                {
                    LoadDocument( files[ 0 ] );
                }
            }
        }

        private void CloseToolStripMenuItem_Click( object sender, EventArgs e )
        {
            FileCloseClick( sender, e );
        }

    }
}
