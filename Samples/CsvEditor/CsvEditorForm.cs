﻿/*
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
using System.Text;
using System.Windows.Forms;
using Kajabity.DocForms.Forms;
using Kajabity.Tools.Csv;

namespace CsvEditor
{
    public partial class CsvEditorForm : SDIForm
    {
        // Remember the maximum number of columns so we can add headings if they increase.
        /// <summary>
        // Constructor for the CSV Editor main form.
        /// </summary>
        public CsvEditorForm()
            : base( new CsvDocumentManager() )
        {
            InitializeComponent();
        }

        public override void DocumentChanged()
        {
            DocumentStatusChanged();

            // Clear the list.
            listView.Items.Clear();
            listView.Columns.Clear();

            if( manager.Opened )
            {
                CsvDocument doc = (CsvDocument) manager.Document;
                ListViewItem[] lvItems = new ListViewItem[ doc.Rows.Length ];
                int counter = 0;
                int maxColumns = 1;

                foreach( string[] row in doc.Rows )
                {
                    var lvItem = new ListViewItem( row );
                    lvItems[ counter ] = lvItem;
                    counter++;

                    if( row.Length > maxColumns )
                    {
                        maxColumns = row.Length;
                    }
                }

                for( int col = 0; col < maxColumns; col++ )
                {
                    listView.Columns.Add( columnName( col ) );
                }

                listView.BeginUpdate();
                listView.Items.AddRange( lvItems );
                listView.EndUpdate();

            }

            saveToolStripButton.Enabled = manager.Opened;
            saveToolStripMenuItem.Enabled = manager.Opened;

            saveAsToolStripMenuItem.Enabled = manager.Opened;

            printToolStripButton.Enabled = false; // manager.Opened;
            printToolStripMenuItem.Enabled = false; //manager.Opened;
            printPreviewToolStripMenuItem.Enabled = false; //manager.Opened;

            DocumentStatusChanged();

            //	Force a display update.
            //this.Refresh();
        }

        /// <summary>
        /// Update the displayed filename.
        /// </summary>
        public override void DocumentStatusChanged()
        {
            string title = Application.ProductName;

            if( manager.Opened )
            {
                //	Update main form heading.
                title += " - " + manager.Document.Name;
            }

            if( manager.Modified )
            {
                //	Update main form heading.
                Text += "*";
            }

            Text = title;
        }

        /// <summary>
        /// Convert a number into an alphabetic column header - A-Z, then AA, AB - ZZ, AAA...
        /// <para>
        /// This could be done much more efficiently, but not for this sample.
        /// </para>
        /// </summary>
        /// <param name="col">the index of the column starting at zero for A.</param>
        /// <returns>the alphabetic column name</returns>
        private string columnName( int col )
        {
            StringBuilder buf = new StringBuilder();
            do
            {
                buf.Append( (char) ( 'A' + ( col % 26 ) ) );
                col = col / 26;
            } while( col > 0 );

            return buf.ToString();
        }

        private void newToolStripMenuItem_Click( object sender, EventArgs e )
        {
            FileNewClick( sender, e );
        }

        private void openToolStripMenuItem_Click( object sender, EventArgs e )
        {
            try
            {
                FileOpenClick( sender, e );
            }
            catch( CsvParseException parseEx )
            {
                MessageBox.Show(this, parseEx.Message, "Error parsing file", MessageBoxButtons.OK, MessageBoxIcon.Error );
            }
            catch( Exception ex )
            {
                MessageBox.Show( this, ex.Message, "Error opening file", MessageBoxButtons.OK, MessageBoxIcon.Error );
            }
        }

        private void saveToolStripMenuItem_Click( object sender, EventArgs e )
        {
            FileSaveClick( sender, e );
        }

        private void saveAsToolStripMenuItem_Click( object sender, EventArgs e )
        {
            FileSaveAsClick( sender, e );
        }

        private void exitToolStripMenuItem_Click( object sender, EventArgs e )
        {
            FileExitClick( sender, e );
        }

        private void aboutToolStripMenuItem_Click( object sender, EventArgs e )
        {

        }
    }
}
