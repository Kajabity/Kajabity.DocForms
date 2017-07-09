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

using System.Windows.Forms;

namespace Kajabity.DocForms.Forms
{
    /// <summary>
    /// An implementation of <see cref="IDocumentSelector"/> to support the 
    /// standard Open/Save file functionality of 
    /// <see cref="SingleDocumentForm{TDocument}"/>.
    /// </summary>
    public class FileDocumentSelector : IDocumentSelector
    {
        /// <summary>
        /// A reference to a dialog for selecting a filename.
        /// </summary>
        private FileDialog _dialog;

        /// <summary>
        /// Constructor providing a pre-configured instance of <see cref="FileDialog"/>.
        /// </summary>
        /// <param name="dialog">An instance of <see cref="FileDialog"/> used to select a file.</param>
        public FileDocumentSelector(FileDialog dialog)
        {
            _dialog = dialog;
        }

        /// <summary>
        /// Gets a string containing the name of the file selected, if any.
        /// </summary>
        public string FileName
        {
            get
            {
                return _dialog.FileName;
            }
        }

        /// <summary>
        /// Calls <see cref="CommonDialog.ShowDialog()"/> to select a file.
        /// </summary>
        /// <param name="owner">usually the MainForm of the application.</param>
        /// <returns>The result of calling <see cref="CommonDialog.ShowDialog()"/>.</returns>
        public DialogResult ShowDialog(IWin32Window owner)
        {
            return _dialog.ShowDialog(owner);
        }
    }
}
