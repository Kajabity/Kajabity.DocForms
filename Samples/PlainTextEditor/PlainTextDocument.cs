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

namespace PlainTextEditor
{
    /// <summary>
    /// Description of TextDocument.
    /// </summary>
    public class PlainTextDocument : Document
    {
        /// <summary>
        /// A variable to hold the document's text - initialised to an empty string.
        /// </summary>
        private string _text = string.Empty;

        /// <summary>
        /// The document's text as a string. Initialised to an empty string.
        /// </summary>
        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value;
                Modified = true;
            }
        }

        //  ---------------------------------------------------------------------
        //  Constructors.
        //  ---------------------------------------------------------------------

        /// <summary>
        /// Construct an empty TextDocument.
        /// </summary>
        public PlainTextDocument()
        {
        }

        /// <summary>
        /// Construct an emtpy TextDocument specifying the filename.
        /// </summary>
        /// <param name="filename">the filename of the text document.</param>
        public PlainTextDocument( string filename )
            : base( filename )
        {
        }

        //  ---------------------------------------------------------------------
        //  Methods.
        //  ---------------------------------------------------------------------

    }
}
