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
using Kajabity.DocForms.Forms;

namespace HexViewer
{
    /// <summary>
    /// Description of HexViewerMainForm.
    /// </summary>
    public partial class HexViewerMainForm : SDIForm
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

        public override void DocumentChanged()
		{
			panel.BinaryDocument = (BinaryDocument) Manager.Document;

			//	Force a display update.
			base.Refresh();
		}

		void NewToolStripMenuItemClick(object sender, EventArgs e)
		{
			this.FileNewClick(sender, e);
		}
		
		void OpenToolStripMenuItemClick(object sender, EventArgs e)
		{
			this.FileOpenClick(sender, e);
		}
		
		void SaveToolStripMenuItemClick(object sender, EventArgs e)
		{
			this.FileSaveClick(sender, e);
		}
		
		void SaveAsToolStripMenuItemClick(object sender, EventArgs e)
		{
			this.FileSaveAsClick(sender, e);
		}
		
		void ExitToolStripMenuItemClick(object sender, EventArgs e)
		{
			this.FileExitClick(sender, e);
		}

        private void recentDocumentsToolStripMenuItem_Click( object sender, EventArgs e )
        {
            this.OnRecentDocumentMenuItemClicked( sender, e );
        }

        private void fileToolStripMenuItem_DropDownOpening( object sender, EventArgs e )
        {
            this.OnParentMenuDropDownOpening( sender, e );
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileCloseClick(sender, e);
        }
    }
}
