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

using System.Text;

namespace Kajabity.DocForms.Documents
{
    /// <summary>
    /// Encapsulates the name of a type of document and it's default extension.
    /// Used in File Open/Save dialogs and to provide the default filename for
    /// New documents.
    /// <see href="https://msdn.microsoft.com/en-us/library/system.windows.forms.filedialog.filter(v=vs.110).aspx"/>
    /// </summary>
    public class DocumentType
    {
        /// <summary>
        /// The name of a type of document which is used both in the Open/Save 
        /// dialogs and as the basis for a default document name for new
        /// Documents.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The filename filter pattern to apply in File Open/Save dialogs.
        /// </summary>
        public string Pattern { get; }

        //  ---------------------------------------------------------------------
        //  Constructors.
        //  ---------------------------------------------------------------------

        /// <summary>
        /// Construct a document type for a given name and filename filter pattern.
        /// </summary>
        /// <param name="name">the display name of the document type.</param>
        /// <param name="pattern">one or more filename patterns using '*' wildcard and separated by ';'.</param>
        public DocumentType( string name, string pattern )
        {
            Name = name;
            Pattern = pattern;
            //TODO: Validate and split the pattern.
            //TODO: support string array in alternate constructor.
        }

        //  ---------------------------------------------------------------------
        //  Methods.
        //  ---------------------------------------------------------------------

        /// <summary>
        /// Converts the DocumentType to an individual file type entry for an OpenFileDialog or OpenSaveDialog filter.
        /// </summary>
        /// <returns>a string forming part of a file dialog filter entry.</returns>
        public string ToFilterPattern()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append( Name );
            sb.Append( " files (" );
            sb.Append( Pattern );
            sb.Append( ")|" );
            sb.Append( Pattern );
            return sb.ToString();
        }
    }
}
