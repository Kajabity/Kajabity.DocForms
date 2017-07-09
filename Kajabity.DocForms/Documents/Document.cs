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

namespace Kajabity.DocForms.Documents
{
    /// <summary>
    /// An abstract base class for all types of document that can be created, 
    /// loaded, modified and saved by the SingleDocumentForm methods.  Extend this class
    /// to add properties and methods appropriate for your particular type of document
    /// or data.
    /// </summary>
    public abstract class Document
    {
        private bool _modified;

        /// <summary>
        /// Gets or sets a flag to indicate if the contents of the document
        /// have been modified and may require saving.
        /// </summary>
        public bool Modified
        {
            get
            {
                return _modified;
            }
            set
            {
                if( _modified != value )
                {
                    _modified = value;
                    OnStatusChanged( new EventArgs() );
                }
            }
        }

        private string _name;

        /// <summary>
        /// Gets or sets the name of the document - usually derived from the filename.
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if( !string.Equals( _name, value ) )
                {
                    _name = value;
                    OnStatusChanged( new EventArgs() );
                }
            }
        }

        //  ---------------------------------------------------------------------
        //  Constructors.
        //  ---------------------------------------------------------------------

        /// <summary>
        /// Empty contructor used to create an unnamed document - the name will be set later.
        /// </summary>
        protected Document()
        {
        }

        /// <summary>
        /// Construct a document providing a name.  The name may be changed later - e.g. when saved.
        /// </summary>
        /// <param name="name">The name of the document</param>
        protected Document( string name )
        {
            _name = name;
        }

        /// <summary>
        /// An event that clients can use to be notified whenever document
        /// name or modified status are changed.
        /// Handle this event to provide specific Forms handling; for example 
        /// update window title with filename or status display.
        /// </summary>
        [
        Category( "Document" ),
        Description( "Notifies whenever document name or modified status are changed." )
        ]
        public event EventHandler<EventArgs> StatusChanged;

        /// <summary>
        /// Called whenever a document name or modified status is or may have been changed.
        /// </summary>
        protected virtual void OnStatusChanged( EventArgs args )
        {
            StatusChanged?.Invoke( this, args );
        }
    }
}
