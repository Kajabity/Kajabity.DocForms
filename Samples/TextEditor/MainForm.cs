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

namespace TextEditor
{
    /// <summary>
    /// Description of MainForm.
    /// </summary>
    public partial class MainForm : SDIForm
	{
		public MainForm() :
			base( new TextDocumentManager() )
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			//
			// TODO: Add constructor code after the InitializeComponent() call.
			//
		}

		public override void DocumentChanged()
		{
			if( Manager.Opened )
			{
				textBox.Text = ((TextDocumentManager) Manager).TextDocument.Text;
			}

			//	Force a display update.
			Refresh();
		}
		
		private void UpdateDocument()
		{
			if( textBox.Modified)
			{
				((TextDocumentManager) Manager).TextDocument.Text = textBox.Text;
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
			UpdateDocument();
			FileSaveClick( sender, e );
		}

	    private void SaveAsToolStripMenuItemClick(object sender, EventArgs e)
		{
			UpdateDocument();
			FileSaveAsClick( sender, e );
		}

	    private void PageSetupToolStripMenuItemClick(object sender, EventArgs e)
		{
			UpdateDocument();
		}

	    private void PrintToolStripMenuItemClick(object sender, EventArgs e)
		{
			UpdateDocument();
		}

	    private void PrintPreviewToolStripMenuItemClick(object sender, EventArgs e)
		{
			UpdateDocument();
		}

	    private void ExitToolStripMenuItemClick(object sender, EventArgs e)
		{
			UpdateDocument();
			FileExitClick( sender, e );
		}

	    private void PrintToolStripButtonClick(object sender, EventArgs e)
		{
			
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

	    private void UndoToolStripMenuItemClick(object sender, EventArgs e)
		{
			textBox.Undo();
		}

	    private void RedoToolStripMenuItemClick(object sender, EventArgs e)
		{
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

	    private void BottomToolStripPanelClick(object sender, EventArgs e)
		{
			
		}
	}
}
