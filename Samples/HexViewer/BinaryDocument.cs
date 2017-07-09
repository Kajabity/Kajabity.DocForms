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

namespace HexViewer
{
    /// <summary>
    /// A read-only document class which contains a file's contents as a byte array.
    /// </summary>
    public class BinaryDocument: Document
    {
        /// <summary>
        /// The entire contents of a file.
        /// </summary>
        public byte[] Data { get; }

        /// <summary>
        /// Construct an empty instance with a dummy byte as content.
        /// </summary>
        public BinaryDocument()
        {
            Data = new byte[ 1 ];
            Data[ 0 ] = 0;
        }

        /// <summary>
        /// Construct a BinaryDocument passing the contents of a file.
        /// </summary>
        /// <param name="data">the entire contents of a file.</param>
        public BinaryDocument( byte[] data )
        {
            Data = data;
        }
    }
}
