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
using System.Diagnostics;
using System.IO;

namespace HexViewer
{
    /// <summary>
    /// A SingleDocumentManager for BinaryDocuments supporting file loading.
    /// New and Save operations are unnecessary for this View Only application.
    /// </summary>
    public class BinaryDocumentManager: SingleDocumentManager<BinaryDocument>
    {
        /// <summary>
        /// Construct a BinaryDocumentManager.
        /// </summary>
	    public BinaryDocumentManager()
        {
        }

        /// <summary>
        /// Load a file into a byte array and hold in the BinaryDocument.
        /// </summary>
        /// <param name="filename">the file to load as a byte array</param>
		public override void Load( string filename )
        {
            Debug.WriteLine( "Loading " + filename );

            FileStream fileStream = new FileStream( filename, FileMode.Open, FileAccess.Read );
            try
            {
                int length = (int) fileStream.Length;   // get file length
                var buffer = new byte[length];          // a buffer to hold the file contents.
                int count;                              // actual number of bytes read
                int sum = 0;                            // total number of bytes read

                // read until Read method returns 0 (end of the stream has been reached)
                while( (count = fileStream.Read( buffer, sum, length - sum )) > 0 )
                {
                    sum += count;  // sum is a buffer offset for next reading
                }

                Document = new BinaryDocument( buffer );
            }
            finally
            {
                fileStream.Close();
            }

            base.Load( filename );
        }
    }
}
