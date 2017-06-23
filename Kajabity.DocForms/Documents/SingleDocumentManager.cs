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
using System.IO;

namespace Kajabity.DocForms.Documents
{
    /// <summary>
    /// A base class to manage the lifecycle of a Document by creating new, empty documents, 
    /// Opening existing documents, tracking the modification state of documents and Saving
    /// documents.
    /// 
    /// Extend this class to implement methods to handle specific document types.
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
        /// The filename of the current document.  Will be null of no current document.
        /// </summary>
        public string Filename { get; private set; }

        private bool _newFile = false;

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
        /// Performs base initialisation of the document name and status of a 
        /// new document.  Override this method to provide initialisation for
        /// specific Document types then call this base class 
        /// once document set (<code>base.NewDocument();</code>).
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
        /// Performs base class setup of loaded filename and status.  Override 
        /// to load a file into a Document instance then call this base class 
        /// once document set (<code>base.Load(filename);</code>).
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
        /// Performs base class setup of Saved filename and status.  Override 
        /// to save a file from a Document instance then call this base class 
        /// (<code>base.Save(filename);</code>).
        /// </summary>
        /// <param name="filename">filename and path to save to</param>
        public virtual void Save(string filename)
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
        /// Close any currently open document - setting state appropriately.  
        /// Override this method to perform any necessary cleanup (releasing 
        /// resources, etc.) for the Document instance then call this base 
        /// class (<code>base.Close();</code>).
        /// </summary>
        public virtual void Close()
        {
            Document = null;
            _newFile = false;
            Filename = null;
        }

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
