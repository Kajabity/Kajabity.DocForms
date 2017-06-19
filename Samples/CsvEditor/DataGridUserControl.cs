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
using System.Text;
using System.Windows.Forms;

namespace CsvEditor
{
    public partial class DataGridUserControl: UserControl
    {
        private ListViewHitTestInfo _hitinfo;
        private readonly TextBox _editbox = new TextBox();

        private CsvDocument _document;

        /// <summary>
        /// Get or set the panel's document - updating the size when it is set.
        /// </summary>
        public CsvDocument CsvDocument
        {
            get
            {
                return _document;
            }
            set
            {
                _document = value;

                // Clear the list.
                listView.Items.Clear();
                listView.Columns.Clear();

                if( _document != null )
                {
                    ListViewItem[] lvItems = new ListViewItem[ _document.NumberOfRows ];

                    for( int row = 0; row < _document.NumberOfRows; row++ )
                    {
                        var lvItem = new ListViewItem( _document.GetCell( row, 0 ) );
                        for( int col = 1; col < _document.NumberOfColumns; col++ )
                        {
                            ListViewItem.ListViewSubItem lvSubItem  =
                                new ListViewItem.ListViewSubItem( lvItem, _document.GetCell( row, col ));
                            lvSubItem.Tag = new[] { row, col };
                            lvItem.SubItems.Add( lvSubItem );
                        }

                        lvItems[ row ] = lvItem;
                    }

                    for( int col = 0; col < _document.NumberOfColumns; col++ )
                    {
                        listView.Columns.Add( columnName( col ) );
                    }

                    listView.BeginUpdate();
                    listView.Items.AddRange( lvItems );
                    listView.EndUpdate();
                }

                Refresh();
            }
        }

        public DataGridUserControl()
        {
            InitializeComponent();

            _editbox.Parent = listView;
            _editbox.Hide();
            _editbox.LostFocus += editbox_LostFocus;
            _editbox.KeyPress += Editbox_OnKeyPress;
        }

        private void Editbox_OnKeyPress( object sender, KeyPressEventArgs keyPressEventArgs )
        {
            if( keyPressEventArgs.KeyChar == (char) Keys.Return )
            {
                editbox_LostFocus( sender, EventArgs.Empty );
            }
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
                buf.Append( (char) ('A' + (col % 26)) );
                col = col / 26;
            } while( col > 0 );

            return buf.ToString();
        }

        private void listView_MouseClick( object sender, MouseEventArgs e )
        {
            _hitinfo = listView.HitTest( e.X, e.Y );
            _editbox.Bounds = _hitinfo.SubItem.Bounds;
            _editbox.Text = _hitinfo.SubItem.Text;
            _editbox.Focus();
            _editbox.Show();
        }

        private void editbox_LostFocus( object sender, EventArgs e )
        {
            _hitinfo.SubItem.Text = _editbox.Text;
            int[] pos = (int[]) _hitinfo.SubItem.Tag;
            _document.SetCell( pos[ 0 ], pos[ 1 ], _editbox.Text );
            _editbox.Hide();
        }
    }
}
