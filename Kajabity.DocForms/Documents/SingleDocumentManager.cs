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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace Kajabity.DocForms.Documents
{
    /// <summary>
    /// Coordinates the state of one <see cref="Document"/>. Derive a manager to create
    /// and read the document format, and to write it to the path supplied by the caller.
    /// The companion form handles interactive prompts, backup and safe replacement.
    /// </summary>
    public abstract class SingleDocumentManager<TDocument> where TDocument : Document
    {
        /// <summary>
        /// The default document name if none is provided by sub-classes.
        /// </summary>
        public const string DEFAULT_DOCUMENT_NAME = "Document";

        /// <summary>
        /// The filename extension (without the dot) for the type of files managed by this class.
        /// </summary>
        public string DefaultExtension { get; set; }

        /// <summary>
        /// The default name or filename (without path or extension) for a file created by this class.
        /// </summary>
        public string DefaultName { get; set; } = DEFAULT_DOCUMENT_NAME;

        /// <summary>
        /// The filename of the current document, or null when none is open.
        /// </summary>
        public string Filename { get; private set; }

        private bool _newFile = false;
        private bool _savingToTemporaryFile;

        /// <summary>
        ///	Returns true if the current document (if any) is new and unsaved.
        /// </summary>
        public bool NewFile
        {
            get
            {
                return Opened && _newFile;
            }
        }

        /// <summary>
        /// Counts the number of documents openned by this instance of the Document Manager to 
        /// allow default filenames to be a little more unique.
        /// </summary>
        private int _docCount;

        private TDocument _document;
        /// <summary>
        /// A reference to the currently loaded document - or null if none loaded.
        /// </summary>
        public TDocument Document
        {
            get { return _document; }
            protected set
            {
                if (_document != null)
                {
                    _document.StatusChanged -= Document_OnStatusChanged;
                }

                _document = value;

                if (_document != null)
                {
                    _document.StatusChanged += Document_OnStatusChanged;
                }
            }
        }

        /// <summary>
        /// Handle the Document StatusChanged event and bubble up to the SingleDocumentForm
        /// </summary>
        /// <param name="sender">unused.</param>
        /// <param name="eventArgs">event arguments from the document passed through.</param>
        private void Document_OnStatusChanged(object sender, EventArgs eventArgs)
        {
            OnDocumentStatusChanged(eventArgs);
        }

        /// <summary>
        /// Returns true this instance has a document.
        /// </summary>
        public bool Opened => Document != null;

        /// <summary>
        /// Tests if a document is both Opened and modified.
        /// </summary>
        public bool Modified => Opened && Document.Modified;

        //  ---------------------------------------------------------------------
        //  Constructors.
        //  ---------------------------------------------------------------------

        //  ---------------------------------------------------------------------
        //  Methods.
        //  ---------------------------------------------------------------------

        /// <summary>
        /// Assign a new <see cref="Document"/> in an override, then call the base method
        /// to assign its default filename, mark it unsaved and clear its modified flag.
        /// </summary>
        public virtual void NewDocument()
        {
            string documentName = string.Format(DefaultName + "{0}." + DefaultExtension, ++_docCount);
            Filename = documentName;
            _newFile = true;

            if (Document != null)
            {
                Document.Name = documentName;
                Document.Modified = false;
            }
        }

        /// <summary>
        /// Read the file and assign <see cref="Document"/> in an override, then call the
        /// base method to commit the filename and clear its modified flag. Do not call
        /// the base method after a failed read.
        /// </summary>
        /// <param name="filename">filename and path loaded</param>
        public virtual void Load(string filename)
        {
            Filename = filename;
            _newFile = false;

            if (Document != null)
            {
                FileInfo fileInfo = new FileInfo(filename);
                Document.Name = fileInfo.Name;
                Document.Modified = false;
            }

            OnDocumentStatusChanged(new EventArgs());
        }

        /// <summary>
        /// Write the document to <paramref name="filename"/> in an override, then call
        /// the base method. During a form save this path is a sibling temporary file;
        /// the form replaces the destination only after the write succeeds. Calling
        /// this method directly does not apply the form's backup or safe-save policy.
        /// </summary>
        /// <param name="filename">filename and path to save to</param>
        public virtual void Save(string filename)
        {
            // SaveSafely commits the document status only after the destination is replaced.
            if (!_savingToTemporaryFile)
                SetSavedFilename(filename);
        }

        /// <summary>
        /// Uses the existing Save override to write beside the destination, then commits
        /// the file and document status. Save overrides must call base.Save after writing.
        /// </summary>
        internal void SaveSafely(string filename, bool backup)
        {
            string destination = Path.GetFullPath(filename);
            string temporaryFilename = Path.Combine(Path.GetDirectoryName(destination),
                Guid.NewGuid().ToString("N") + ".tmp" + Path.GetExtension(destination));
            bool temporaryFileCreated = false;

            try
            {
                // Reserve a unique path without touching any pre-existing temporary file.
                using (new FileStream(temporaryFilename, FileMode.CreateNew, FileAccess.Write))
                    temporaryFileCreated = true;

                _savingToTemporaryFile = true;
                try
                {
                    Save(temporaryFilename);
                }
                finally
                {
                    _savingToTemporaryFile = false;
                }

                if (File.Exists(destination))
                    File.Replace(temporaryFilename, destination, backup ? destination + "~" : null);
                else
                    File.Move(temporaryFilename, destination);

                SetSavedFilename(filename);
            }
            finally
            {
                if (temporaryFileCreated)
                {
                    try
                    {
                        File.Delete(temporaryFilename);
                    }
                    catch (IOException ex)
                    {
                        Debug.WriteLine("Could not remove temporary document: " + ex);
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        Debug.WriteLine("Could not remove temporary document: " + ex);
                    }
                }
            }
        }

        private void SetSavedFilename(string filename)
        {
            Filename = filename;
            _newFile = false;

            if (Document != null)
            {
                FileInfo fileInfo = new FileInfo(filename);
                Document.Name = fileInfo.Name;
                Document.Modified = false;
            }
        }

        /// <summary>
        /// Clear the current document and filename. Override for format-specific cleanup,
        /// then call the base method. Interactive close confirmation belongs to the form.
        /// </summary>
        public virtual void Close()
        {
            Document = null;
            _newFile = false;
            Filename = null;
        }

        //  ---------------------------------------------------------------------
        //  Events.
        //  ---------------------------------------------------------------------

        /// <summary>
        /// An event that clients can use to be notified whenever a document's
        /// name or modified status are changed.
        /// Handle this event to provide specific Forms handling; for example 
        /// update window title with filename or status display.
        /// </summary>
        [
        Category("Document"),
        Description("Notifies whenever a document's name or modified status are changed.")
        ]
        public event EventHandler<EventArgs> DocumentStatusChanged;

        /// <summary>
        /// Called whenever a document name or modified status is or may have been changed.
        /// </summary>
        protected virtual void OnDocumentStatusChanged(EventArgs args)
        {
            DocumentStatusChanged?.Invoke(this, args);
        }
    }
}
