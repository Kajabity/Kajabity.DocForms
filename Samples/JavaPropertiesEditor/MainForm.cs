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
using Kajabity.Tools.Java;

namespace JavaPropertiesEditor
{
    /// <summary>
    /// Description of MainForm.
    /// </summary>
    public partial class MainForm : SingleDocumentForm<JavaPropertiesDocument>
	{
		public MainForm()
            : base( new JavaPropertiesDocumentManager() )
        {
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
		}

		public MainForm( string filename )
            : base( new JavaPropertiesDocumentManager() )
        {
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

            LoadDocument( filename );
		}

		public string Status
		{
			set
			{
				if( value == null )
				{
					toolStripMessageLabel.Text = "Ready.";
				}
				else
				{
					toolStripMessageLabel.Text = value;
				}
			}
			get
			{
				return toolStripMessageLabel.Text;
			}
		}

        //      public override void DocumentChanged()
        //      {
        //}

        private void MainForm_DocumentChanged( object sender, EventArgs e )
        {
            // Clear the list.
            listView1.Items.Clear();

            if( Manager.Opened )
            {
                //	Update main form heading.
                JavaProperties properties = Manager.Document.Properties;

                //	Update the content window.
                foreach( String key in properties.Keys )
                {
                    ListViewItem item = new ListViewItem( key );
                    String text = properties.GetProperty( key );
                    item.SubItems.Add( new ListViewItem.ListViewSubItem( item, text ) );

                    listView1.Items.Add( item );
                }
            }

            //	Force a display update.
            Refresh();
        }

        private void MainForm_DocumentStatusChanged( object sender, EventArgs e )
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
                Text += @"*";
            }

            Text = title;
        }

        void OnFileNew(object sender, EventArgs e)
		{
            FileNewClick( sender, e );
        }
		
		void OnFileOpen(object sender, EventArgs e)
		{
            FileOpenClick( sender, e );
        }
		
		void OnFileClose(object sender, EventArgs e)
		{
            FileCloseClick( sender, e );
		}
		
		void OnFileSave(object sender, EventArgs e)
		{
            FileSaveClick( sender, e );
        }
		
		void OnFileSaveAs(object sender, EventArgs e)
		{
            FileSaveAsClick( sender, e );
        }
		
		void OnFileExit(object sender, EventArgs e)
		{
            FileExitClick( sender, e );
        }
    }
}
