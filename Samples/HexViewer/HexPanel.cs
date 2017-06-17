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
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace HexViewer
{
    /// <summary>
    /// Display the contents of a BinaryDocument as hexadecimal bytes and characters with offset.
    /// Higlight bytes and corresponding chars under mouse.
    /// </summary>
    public class HexPanel: Panel
    {
        /// <summary>
        /// Fixed number of byte/char columns.
        /// </summary>
        private const int NumberOfColumns = 16;

        private Breadth _columnAddressBackground;
        private Breadth _columnAddress;

        private Breadth _columnByteBackground;
        private Breadth[] _columnBytes;

        private Breadth _columnCharBackground;
        private Breadth[] _columnChars;

        private float _heightRow;
        private float _widthByte;
        private float _widthChar;

        private Point _mousePosition;
        private int _selectedOffset = -1;

        private BinaryDocument _document;

        /// <summary>
        /// Get or set the panel's document - updating the size when it is set.
        /// </summary>
        public BinaryDocument BinaryDocument
        {
            get
            {
                return _document;
            }
            set
            {
                _document = value;
                CalculateSizes();
                AutoScrollPosition = new Point( 0, 0 );
                Refresh();
            }
        }

        /// <summary>
        /// Construct the HexPanel and set the size of the panel and it's elements.
        /// </summary>
        public HexPanel()
        {
            // Double buffer the control
            SetStyle( ControlStyles.AllPaintingInWmPaint |
              ControlStyles.UserPaint |
              ControlStyles.ResizeRedraw |
              ControlStyles.OptimizedDoubleBuffer, true );

            //
            // The InitializeComponent() call is required for Windows Forms designer support.
            //
            InitializeComponent();

            CalculateSizes();
        }

        /// <summary>
        /// Maintained by Visual Studio designer.
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // HexPanel
            // 
            this.AutoScroll = true;
            this.Font = new System.Drawing.Font( "Microsoft Sans Serif", 8.25F );
            this.Padding = new System.Windows.Forms.Padding( 4 );
            this.Scroll += new System.Windows.Forms.ScrollEventHandler( this.HexPanel_Scroll );
            this.MouseLeave += new System.EventHandler( this.HexPanel_MouseLeave );
            this.MouseMove += new System.Windows.Forms.MouseEventHandler( this.HexPanel_MouseMove );
            this.ResumeLayout( false );

        }

        /// <summary>
        /// Calculate the sizes of the on-screen elements and the overall view size.
        /// </summary>
        private void CalculateSizes()
        {
            // Calculate the size of display elements.
            using( Graphics g = CreateGraphics() )
            {
                string sampleHex = "00";
                SizeF sizeByte = g.MeasureString( sampleHex, Font );
                _heightRow = sizeByte.Height;
                _widthByte = sizeByte.Width;

                string sampleChars = "m";
                SizeF sizeChar = g.MeasureString( sampleChars, Font );
                _widthChar = sizeChar.Width;
            }

            float widthAddress = _widthByte * 3;
            float widthSpace = _widthByte / 2;
            float widthCharSpace = 1;

            float backgroundPadding = 1;
            _columnAddress = new Breadth( Padding.Left + backgroundPadding, widthAddress );
            _columnAddressBackground = _columnAddress.Inflate( backgroundPadding );

            _columnByteBackground = new Breadth( _columnAddressBackground.Right + widthSpace,
                backgroundPadding + NumberOfColumns * (_widthByte + widthSpace) - widthSpace + backgroundPadding );

            float hexX = _columnByteBackground.Left + backgroundPadding;
            _columnBytes = new Breadth[ NumberOfColumns ];
            for( int b = 0; b < NumberOfColumns; b++ )
            {
                _columnBytes[ b ] = new Breadth( hexX, _widthByte );
                hexX += _widthByte + widthSpace;
            }

            _columnCharBackground = new Breadth( _columnByteBackground.Right + widthSpace,
                backgroundPadding + NumberOfColumns * (_widthChar + widthCharSpace) - widthCharSpace + backgroundPadding );

            float charX = _columnCharBackground.Left + backgroundPadding;
            _columnChars = new Breadth[ NumberOfColumns ];
            for( int b = 0; b < NumberOfColumns; b++ )
            {
                _columnChars[ b ] = new Breadth( charX, _widthChar );
                charX += _widthChar + widthCharSpace;
            }

            if( _document == null )
            {
                AutoScrollMinSize = ClientSize;
            }
            else
            {
                int nRows = (_document.Data.Length + NumberOfColumns - 1) / NumberOfColumns;

                float widthF =
                        _columnCharBackground.Right +
                        Padding.Right;

                float heightF =
                        Padding.Top +
                        nRows * _heightRow +
                        Padding.Bottom;

                AutoScrollMinSize = new Size( (int) widthF, (int) heightF );
            }

            Debug.WriteLine( "Display Size " + AutoScrollMinSize +
                ", Address Background " + _columnAddressBackground +
                ", Address " + _columnAddress +
                ", Bytes Background " + _columnByteBackground +
                ", Chars Background " + _columnCharBackground );
        }

        /// <summary>
        /// Overridden to draw the hex dump on the panel - or a message in an empty panel.
        /// </summary>
        /// <param name="ev">provides the Graphics context</param>
        protected override void OnPaint( PaintEventArgs ev )
        {
            base.OnPaint( ev );

            // Clear the background.
            using( Brush brushBack = new SolidBrush( BackColor ) )
            {
                ev.Graphics.FillRectangle( brushBack, ClientRectangle );
            }

            // What to Draw?
            if( _document == null )
            {
                DrawCenteredText( ev.Graphics, "Open a document to see it's bytes." );
            }
            else if( _document.Data.Length == 0 )
            {
                DrawCenteredText( ev.Graphics, "Document is empty." );
            }
            else
            {
                Draw( ev.Graphics );
            }
        }


        /// <summary>
        /// Draw the given text centered in the window.
        /// </summary>
        /// <param name="g">the graphics context</param>
        /// <param name="text">the text to be drawn.</param>
        public void DrawCenteredText( Graphics g, string text )
        {
            RectangleF drawRect = RectangleF.Inflate( ClientRectangle, -Padding.Left, -Padding.Top );

            StringFormat sf = new StringFormat
            {
                LineAlignment = StringAlignment.Center,
                Alignment = StringAlignment.Center
            };

            using( Brush brushText = new SolidBrush( ForeColor ) )
            {
                g.DrawString( text, Font, brushText, drawRect, sf );
            }
        }

        /// <summary>
        /// Draw the main panel containing the document text.  
        /// Expects the document to be present and not empty.
        /// </summary>
        /// <param name="g">the graphics context</param>
        public void Draw( Graphics g )
        {
            g.TranslateTransform( AutoScrollPosition.X, AutoScrollPosition.Y );

            StringFormat format = new StringFormat
            {
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Near
            };

            //TODO: draw colour backing under Address, Hex and Chars.

            // Calculate the first row, rounding down.
            int firstRow = (int) ((-AutoScrollPosition.Y + Padding.Top) / _heightRow);
            int offset = NumberOfColumns * firstRow;

            float rowY = Padding.Top + firstRow * _heightRow;
            float maxY = -AutoScrollPosition.Y + ClientRectangle.Height;
            float rowX = Padding.Left;

            using( Brush brushColumnBg = new SolidBrush( Color.Azure ),
                brushAddress = new SolidBrush( Color.Black ),
                brushBytes = new SolidBrush( Color.Blue ),
                brushChars = new SolidBrush( Color.DarkGreen ),
                brushSelected = new SolidBrush( Color.Yellow ) )
            {
                // Fill the background columns.
                var rect = new Rectangle( (int) _columnAddressBackground.Left, -AutoScrollPosition.Y,
                    (int) _columnAddressBackground.Width, ClientRectangle.Height );
                g.FillRectangle( brushColumnBg, rect );

                rect = new Rectangle( (int) _columnByteBackground.Left, -AutoScrollPosition.Y,
                    (int) _columnByteBackground.Width + 1, ClientRectangle.Height );
                g.FillRectangle( brushColumnBg, rect );

                rect = new Rectangle( (int) _columnCharBackground.Left, -AutoScrollPosition.Y,
                    (int) _columnCharBackground.Width + 1, ClientRectangle.Height );
                g.FillRectangle( brushColumnBg, rect );

                // Draw each row.
                do
                {
                    // Draw the address in hex.
                    float addressX = rowX;
                    g.DrawString( offset.ToString( "X6" ), Font, brushAddress, addressX, rowY, format );

                    // Draw the bytes.
                    int limit = Math.Min( NumberOfColumns, _document.Data.Length - offset );

                    for( int b = 0; b < limit; b++ )
                    {
                        // Draw the mouse hover highlight.
                        if( b + offset == _selectedOffset )
                        {
                            rect = new Rectangle( (int) _columnBytes[ b ].Left, (int) rowY, (int) _widthByte + 1, (int) _heightRow );
                            g.FillRectangle( brushSelected, rect );

                            rect = new Rectangle( (int) _columnChars[ b ].Left, (int) rowY, (int) _widthChar + 1, (int) _heightRow );
                            g.FillRectangle( brushSelected, rect );
                        }

                        byte value = _document.Data[b+offset];

                        g.DrawString( value.ToString( "X2" ), Font, brushBytes, _columnBytes[ b ].Left, rowY, format );

                        char ch = (char) value;
                        if( ch > 32 && ch < 128 )
                        {
                            g.DrawString( ch.ToString(), Font, brushChars, _columnChars[ b ].Left, rowY, format );
                        }
                    }

                    rowY += _heightRow;
                    offset += NumberOfColumns;
                } while( offset < _document.Data.Length && rowY <= maxY );
            }
        }

        /// <summary>
        /// As the mouse moves over the panel, determine if it is over any of 
        /// the bytes or chars and set _selectedOffset.
        /// </summary>
        /// <param name="sender">unused.</param>
        /// <param name="e">includes the location of the mouse.</param>
        private void HexPanel_MouseMove( object sender, MouseEventArgs e )
        {
            _mousePosition = e.Location;
            UpdateMousePosition();
        }

        /// <summary>
        /// When the mouse leaves the panel, clear any selected offset and highlight.
        /// </summary>
        /// <param name="sender">unused.</param>
        /// <param name="e">unused.</param>
        private void HexPanel_MouseLeave( object sender, EventArgs e )
        {
            _selectedOffset = -1;
            _mousePosition = new Point( -1, -1 );
            Refresh();
        }

        /// <summary>
        /// When the user scrolls, the panel moves but the mouse doesn't...
        /// </summary>
        /// <param name="sender">unused.</param>
        /// <param name="e">unused.</param>
        private void HexPanel_Scroll( object sender, ScrollEventArgs e )
        {
            UpdateMousePosition();
        }

        /// <summary>
        /// When the mouse wheel is turned, the panel moves but the mouse doesn't...
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseWheel( MouseEventArgs e )
        {
            base.OnMouseWheel( e );  // Call the base class so the image actually moves, then...
            UpdateMousePosition();
        }

        /// <summary>
        /// Handle mouse moves over the panel - locate the byte or char under the mouse.
        /// </summary>
        private void UpdateMousePosition()
        {
            Point p = _mousePosition;

            int newSelectedOffset = -1;
            if( _document != null && ClientRectangle.Contains( p ) )
            {
                // Transform the location using the AutoScrollPosition.
                p.Offset( -AutoScrollPosition.X, -AutoScrollPosition.Y );

                int row = (int) ((p.Y - Padding.Top) / _heightRow);
                int offset = NumberOfColumns * row;

                int index = -1;
                for( int b = 0; b < NumberOfColumns; b++ )
                {
                    if( _columnBytes[ b ].Contains( p.X ) || _columnChars[ b ].Contains( p.X ) )
                    {
                        index = b + offset;
                    }
                }

                if( index < 0 || index > _document.Data.Length )
                {
                    newSelectedOffset = -1;
                }
                else
                {
                    newSelectedOffset = index;
                }
            }

            if( newSelectedOffset != _selectedOffset )
            {
                _selectedOffset = newSelectedOffset;
                Refresh();
            }
        }
    }
}
