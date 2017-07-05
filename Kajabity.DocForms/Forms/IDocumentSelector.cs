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
    /// This interface abstracts the key features of the 
    /// <see cref="OpenFileDialog"/> and <see cref="SaveFileDialog"/> so that 
    /// they can be overriden in subclasses to provide alternative behaviour.
    /// For example, to select a directory or for testing purposes.
    /// </summary>
    public interface IDocumentSelector
    {
        /// <summary>
        /// As with FileDialog.ShowDialog(), return <see cref="DialogResult.OK"/> 
        /// or appropriate alternative.
        /// </summary>
        /// <param name="owner">the parent Form or Window.</param>
        /// <returns><see cref="DialogResult.OK"/> when a file has been selected.</returns>
        DialogResult ShowDialog(IWin32Window owner);

        /// <summary>
        /// Returns the filename selected, if the result was <see cref="DialogResult.OK"/>.
        /// </summary>
        string FileName { get; }
    }
}
