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

using System.IO;
using Kajabity.DocForms.Documents;
using Kajabity.Tools.Csv;

namespace CsvEditor
{
    public class CsvDocumentManager: SingleDocumentManager<CsvDocument>
    {
        public CsvDocumentManager()
        {
            DefaultName = "CsvDocument";
            DefaultExtension = "csv";
        }

        public override void NewDocument()
        {
            Document = new CsvDocument();
            base.NewDocument();
        }

        public override void Load( string filename )
        {

            using( FileStream fileStream = File.OpenRead( filename ) )
            {
                CsvReader reader = new CsvReader( fileStream );

                string[][] records = reader.ReadAll();
                Document = new CsvDocument( filename, records );
            }

            base.Load( filename );
        }

        public override void Save( string filename )
        {
            using( FileStream outStream = File.OpenWrite( filename ) )
            {
                outStream.SetLength( 0L );  // Truncate the file if it exists.

                CsvWriter writer = new CsvWriter( outStream );

                //for( int row = 0; row < document.)
                //writer.WriteAll( CsvDocument );
                outStream.Flush();
            }

            base.Save( filename );
        }
    }
}
