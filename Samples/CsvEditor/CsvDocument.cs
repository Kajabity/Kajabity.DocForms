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

using System.Collections.Generic;
using Kajabity.DocForms.Documents;

namespace CsvEditor
{
    /// <summary>
    /// A CSV document to hold the rows and columns of the CSV file 
    /// as string arrays.
    /// </summary>
    public class CsvDocument: Document
    {
        /// <summary>
        /// The array of strings making up the rows and columns of the CSV file.
        /// </summary>
        private readonly List<List<string>> _rowsList;

        public int NumberOfColumns { get; private set; }

        public int NumberOfRows => _rowsList.Count;

        /// <summary>
        /// Construct and empty document with a default name.
        /// </summary>
        public CsvDocument()
        {
            _rowsList = new List<List<string>>();
            NumberOfColumns = 0;
        }

        /// <summary>
        /// Construct and empty document.
        /// </summary>
        /// <param name="name">the name of the document</param>
        public CsvDocument( string name )
                : base( name )
        {
            _rowsList = new List<List<string>>();
            NumberOfColumns = 0;
        }

        /// <summary>
        /// Construct a document passing in its name and the CSV data.
        /// </summary>
        /// <param name="name">the name of the document</param>
        /// <param name="records">the csv data</param>
        public CsvDocument( string name, string[][] records )
                : base( name )
        {
            _rowsList = new List<List<string>>();
            foreach( var column in records )
            {
                _rowsList.Add( new List<string>( column ) );

                if (NumberOfColumns < column.Length)
                {
                    NumberOfColumns = column.Length;
                }
            }
        }

        /// <summary>
        /// Retrieve the value of a single cell in the data array - or an empty string if the
        /// indices are outside the current size of the array.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column">value of the cell</param>
        /// <returns></returns>
        public string GetCell( int row, int column )
        {
            if( row < _rowsList.Count )
            {
                if( column < _rowsList[ row ].Count )
                {
                    return _rowsList[ row ][ column ];
                }
            }

            return string.Empty;
        }

        public void SetCell( int row, int column, string value )
        {
            string oldValue =GetCell(row, column);
            if( !string.Equals( oldValue, value ) )
            {
                while( row >= _rowsList.Count )
                {
                    _rowsList.Add( new List<string>() );
                }
                while( column >= _rowsList[ row ].Count )
                {
                    _rowsList[ row ].Add( string.Empty );
                }

                _rowsList[ row ][ column ] = value;
                Modified = true;

                if( NumberOfColumns < column )
                {
                    NumberOfColumns = column;
                }
            }
        }
    }
}
