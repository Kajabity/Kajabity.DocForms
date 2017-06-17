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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

using Kajabity.DocForms.Documents;
using Microsoft.Win32;

namespace Kajabity.DocForms.Forms
{
    /// <summary>
    /// Extends System.Windows.Forms.Form to implement support for handling 
    /// Documents in a Single Document Interface (SDI) style.
    /// </summary>
    public class SDIForm: Form
    {
        //  ---------------------------------------------------------------------
        //  Attributes.
        //  ---------------------------------------------------------------------

        /// <summary>
        /// A reference to an instance of the DocumentManager used to load and save
        /// documents for the application.
        /// </summary>
        protected DocumentManager manager;


        /// <summary>
        /// If true, when saving a document any existing file with the same name 
        /// will be renamed with a trailing '~' character as a backup.  Any 
        /// previous backup (i.e. with the same backup filename) will be deleted.
        /// </summary>
        protected bool backup = false;

        //  ---------------------------------------------------------------------
        //  Recent Document Attributes.
        //  ---------------------------------------------------------------------

        /// <summary>
        /// The base path for each Recent Document entry in the registry.
        /// </summary>
        private static string RecentDocumentRegistryEntryName = "RecentDocument-";

        /// <summary>
        /// Maximum number of documents to remember
        /// </summary>
        private static int MaximumRecentDocumentCount = 20;

        /// <summary>
        /// Maximum length of path the document path entry in the list.
        /// </summary>
        private static int MaximumDisplayNameLength = 100;

        /// <summary>
        /// Holds a list of the most recent filenames used by the application.
        /// May be used to support Recent Files menu features.
        /// </summary>
        protected List<string> recentFiles = new List<string>();

        /// <summary>
        /// A reference to a menu item to be used to contain a list of recently opened 
        /// files.
        /// </summary>
        protected ToolStripMenuItem recentItemsMenuItem = null;

        /// <summary>
        /// The registry path under which recent documents are stored.
        /// </summary>
        protected string registryPath;

        //  ---------------------------------------------------------------------
        //  Constructors.
        //  ---------------------------------------------------------------------

        /// <summary>
        /// Default constructor - required to enable the Visual Editor for Forms
        /// in Visual Studio to work.
        /// </summary>
        public SDIForm()
        {
            Load += new EventHandler( SDIForm_Load );
        }

        /// <summary>
        /// Construct an SDIForm providing an instance of a document manager.
        /// </summary>
        /// <param name="manager">the DocumentManager to be used by this form.</param>
        public SDIForm( DocumentManager manager )
        {
            this.manager = manager;
            Load += new EventHandler( SDIForm_Load );
        }

        //  ---------------------------------------------------------------------
        //  Methods.
        //  ---------------------------------------------------------------------

        private void SDIForm_Load( object sender, EventArgs e )
        {
            newDocument();
            //DocumentChanged();
        }

        /// <summary>
        /// A handler for the File->New command.  
        /// Closes any currently open document (prompting the user to save it if modified).
        /// Creates a new instance of the managed document type.
        /// Call this method your form's 'OnClick()' handler.
        /// </summary>
        /// <param name="sender">the object that sent the event - e.g. menu item or tool bar button.</param>
        /// <param name="e">Any additional arguments to the event.</param>
        public void FileNewClick( object sender, EventArgs e )
        {
            if( canCloseDocument( sender, e ) )
            {
                newDocument();
            }
        }

        /// <summary>
        /// A handler for the File->Open command.   
        /// Uses the OpenFileDialog to select and open a file.
        /// Closes any currently open document (prompting the user to save it if modified).
        /// Call this method your form's 'OnClick()' handler.
        /// </summary>
        /// <param name="sender">the object that sent the event - e.g. menu item or tool bar button.</param>
        /// <param name="e">Any additional arguments to the event.</param>
        public void FileOpenClick( object sender, EventArgs e )
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Open " + Application.ProductName + " file";
            dialog.Filter = Application.ProductName + " files (*." + manager.DefaultExtension + ")|*." + manager.DefaultExtension +
                "|All files (*.*)|*.*";

            //			dialog.InitialDirectory = @"C:\";
            dialog.AddExtension = true;
            dialog.CheckFileExists = true;
            dialog.CheckPathExists = true;
            //			dialog.ShowHelp = true; // Need to handle 'HelpRequest' event.
            dialog.ReadOnlyChecked = false;
            dialog.DefaultExt = manager.DefaultExtension;

            if( dialog.ShowDialog() == DialogResult.OK && canCloseDocument( sender, e ) )
            {
                Debug.WriteLine( manager.DefaultExtension + " file: " + dialog.FileName );

                loadDocument( dialog.FileName );
            }
        }

        /// <summary>
        /// A handler for the File->Close command.  
        /// Closes any currently open document (prompting the user to save it if modified).
        /// Call this method your form's 'OnClick()' handler.
        /// </summary>
        /// <param name="sender">the object that sent the event - e.g. menu item or tool bar button.</param>
        /// <param name="e">Any additional arguments to the event.</param>
        public void FileCloseClick( object sender, EventArgs e )
        {
            if( canCloseDocument( sender, e ) )
            {
                closeDocument();
            }
        }

        /// <summary>
        /// A handler for the File->Save command.  
        /// Saves the currently open document - prompting for a filename if it is a new document.
        /// Call this method your form's 'OnClick()' handler.
        /// </summary>
        /// <param name="sender">the object that sent the event - e.g. menu item or tool bar button.</param>
        /// <param name="e">Any additional arguments to the event.</param>
        public void FileSaveClick( object sender, EventArgs e )
        {
            if( manager.NewFile )
            {
                FileSaveAsClick( sender, e );
            }
            else
            {
                saveDocument( manager.Filename );
            }
        }

        /// <summary>
        /// A handler for the File->Save As command.  
        /// Prompts the user for a filename and path and saves the document to it.
        /// Call this method your form's 'OnClick()' handler.
        /// </summary>
        /// <param name="sender">the object that sent the event - e.g. menu item or tool bar button.</param>
        /// <param name="e">Any additional arguments to the event.</param>
        public void FileSaveAsClick( object sender, EventArgs e )
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.DefaultExt = manager.DefaultExtension;
            dialog.Title = "Save " + Application.ProductName + " file";
            dialog.Filter = Application.ProductName + " files (*." + manager.DefaultExtension + ")|*." + manager.DefaultExtension +
                "|All files (*.*)|*.*";
            dialog.FileName = manager.Filename;

            if( dialog.ShowDialog( this ) == DialogResult.OK )
            {
                Debug.WriteLine( manager.DefaultExtension + " file: " + dialog.FileName );

                saveDocument( dialog.FileName );
            }
        }

        /// <summary>
        /// A handler for the File->Exit command.  
        /// Exits the application, closing any currently open document (prompting the user to save it if modified).
        /// Call this method your form's 'OnClick()' handler.
        /// </summary>
        /// <param name="sender">the object that sent the event - e.g. menu item or tool bar button.</param>
        /// <param name="e">Any additional arguments to the event.</param>
        public void FileExitClick( object sender, EventArgs e )
        {
            if( canCloseDocument( sender, e ) )
            {
                Application.Exit();
            }
        }

        /// <summary>
        /// Called whenever a document is created or loaded (or closed).
        /// Override, calling the base class implementation, to provide specific Forms handling when the document is changed.
        /// </summary>
        public virtual void DocumentChanged()
        {
            //	Force a display update.
            Refresh();
        }

        /// <summary>
        /// Called whenever a document is saved - only the filename and modified status will need updating.
        /// Override, calling the base class implementation, to provide specific Forms handling.
        /// </summary>
        public virtual void DocumentStatusChanged()
        {
            // Derived forms could update filename or status display.
        }

        /// <summary>
        /// Helper to create a new document - triggers DocumentChanged().
        /// </summary>
        protected void newDocument()
        {
            if( manager != null )
            {
                manager.NewDocument();
            }

            //	Refresh display.
            DocumentChanged();
        }

        /// <summary>
        /// Helper to load a document - triggers DocumentChanged().  
        /// </summary>
        protected virtual void loadDocument( string filename )
        {
            try
            {
                //	load the file
                manager.Load( filename );

                // add successfully opened file to MRU list
                AddFileHistory( filename );

                //	Refresh display.
                DocumentChanged();
            }
            catch( Exception ex )
            {
                // Report the error
                Debug.WriteLine( ex.ToString() );

                // Remove file from MRU list - if it exists
                RemoveFileHistory( filename );

                // Let the calling methods handle it.
                throw ex;
            }
        }

        /// <summary>
        /// Helper to save a document - triggers DocumentStatusChanged().
        /// </summary>
        /// <param name="filename">the filename and path to save the document into</param>
        private void saveDocument( string filename )
        {
            try
            {
                if( backup && File.Exists( filename ) )
                {
                    string backupFilename = filename + "~";

                    if( File.Exists( backupFilename ) )
                    {
                        File.Delete( backupFilename );
                    }

                    File.Move( filename, backupFilename );
                }

                //	Save the file
                manager.Save( filename );

                // add successfully opened file to MRU list
                AddFileHistory( filename );

                //	Refresh display of document status.
                DocumentStatusChanged();
            }
            catch( Exception ex )
            {
                // Report the error
                Debug.WriteLine( ex.ToString() );

                // Remove file from MRU list - if it exists
                RemoveFileHistory( filename );

                // Let the calling methods handle it.
                throw ex;
            }
        }

        /// <summary>
        /// Helper to close any currently open document - triggers DocumentChanged().
        /// </summary>
        private void closeDocument()
        {
            manager.Close();

            //	Refresh display.
            DocumentChanged();
        }

        /// <summary>
        /// A "helper" menu action to try to close the currently loaded file if there is one.
        /// The method will display a prompt to the user to save the file if modified.
        /// </summary>
        /// <param name="sender">the object that sent the event - e.g. menu item or tool bar button.</param>
        /// <param name="e">Any additional arguments to the event.</param>
        /// <returns>returns true if the document is closed.</returns>
        protected bool canCloseDocument( object sender, EventArgs e )
        {
            if( manager.Modified )
            {
                DialogResult result = MessageBox.Show( this, Application.ProductName + " file " + manager.Filename + " has been modified!\n\nDo you want to save it?",
                                                      Application.ProductName,
                                                      MessageBoxButtons.YesNoCancel,
                                                      MessageBoxIcon.Exclamation );

                if( result == DialogResult.Yes )
                {
                    FileSaveClick( sender, e );
                }
                else if( result == DialogResult.Cancel )
                {
                    return false;
                }
            }

            closeDocument();

            return true;
        }

        #region Recent Document Methods
        //  ---------------------------------------------------------------------
        //  Recent Document Methods.
        //  ---------------------------------------------------------------------

        /// <summary>
        /// A method to initialise the Recent Documents feature - pass in a menu item and,
        /// when the menu is opened, it will have he recent files added.
        /// Recent files are stored in the Registry using the path:
        /// HKEY_CURRENT_USER\Software\(company-name)\(application-name)\Recent Documents
        /// </summary>
        /// <param name="recentItemsMenuItem">a reference to the "Recent Documents..." menu item</param>
        public void InitialseRecentDocuments( ToolStripMenuItem recentItemsMenuItem )
        {
            this.recentItemsMenuItem = recentItemsMenuItem;

            // Read any previously stored recent documents.
            try
            {
                // Set registry path from Application Settings.
                Assembly assembly = Assembly.GetEntryAssembly();
                string companyName = ( (AssemblyCompanyAttribute) Attribute.GetCustomAttribute( assembly, typeof( AssemblyCompanyAttribute ), false ) ).Company;
                string appName = assembly.GetName().Name;

                registryPath = "Software\\" + companyName + "\\" + appName + "\\Recent Documents";
                Debug.WriteLine( "Registry Path = " + registryPath );

                RegistryKey key = Registry.CurrentUser.OpenSubKey( registryPath );
                if( key != null )
                {
                    for( int rd = 0; rd < MaximumRecentDocumentCount; rd++ )
                    {
                        string path = (string) key.GetValue( RecentDocumentRegistryEntryName + rd, "" );
                        if( path != null && path.Length > 0 )
                        {
                            // Convert to a canonical, none relative format.
                            string fullPath = Path.GetFullPath( path );

                            // Ensure unique entries only.
                            RemoveFileHistory( fullPath );

                            // Now add this to the head of the list.
                            recentFiles.Add( fullPath );
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            catch( Exception ex )
            {
                Debug.WriteLine( "Error loading recent documents from registry key " + registryPath );
                Debug.Write( ex.Message );
            }
        }

        /// <summary>
        /// On Parent Menu DropDown Opening - e.g. fileToolStripMenuItem_DropDownOpening
        /// </summary>
        protected void OnParentMenuDropDownOpening( object sender, EventArgs e )
        {
            if( recentItemsMenuItem.DropDownItems.Count > 0 )
            {
                recentItemsMenuItem.DropDownItems.Clear();
            }

            recentItemsMenuItem.Enabled = recentFiles.Count > 0;

            int maxRd = Math.Min( recentFiles.Count, MaximumRecentDocumentCount );
            for( int rd = 0; rd < maxRd; rd++ )
            {
                string path = recentFiles[ rd ];

                ToolStripMenuItem menuItem = new ToolStripMenuItem( GetShortPath( path ) );
                menuItem.Tag = path;
                menuItem.Click += OnRecentDocumentMenuItemClicked;

                recentItemsMenuItem.DropDownItems.Add( menuItem );
            }
        }

        /// <summary>
        /// Truncates a path to fit within a certain number of characters by replacing path components with ellipses.
        /// <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/bb773578(v=vs.85).aspx"/>
        /// </summary>
        /// <param name="pszOut">The address of the string that has been altered.</param>
        /// <param name="pszSrc">A pointer to a null-terminated string of length MAX_PATH that contains the path to be altered.</param>
        /// <param name="cchMax">
        /// The maximum number of characters to be contained in the new string, including the terminating null character. 
        /// For example, if cchMax = 8, the resulting string can contain a maximum of 7 characters plus the terminating null character.
        /// </param>
        /// <param name="dwFlags">Reserved</param>
        /// <returns>Returns TRUE if successful, or FALSE otherwise.</returns>
        [DllImport( "shlwapi.dll", CharSet = CharSet.Auto )]
        static extern bool PathCompactPathEx( [Out] StringBuilder pszOut, string pszSrc, int cchMax, int dwFlags );

        private string GetShortPath( string longPath )
        {
            StringBuilder shortPath = new StringBuilder( MaximumDisplayNameLength );
            if( PathCompactPathEx( shortPath, longPath, shortPath.Capacity, 0 ) )
            {
                return shortPath.ToString();
            }
            else
            {
                return longPath;
            }
        }

        /// <summary>
        /// When the for closes, save the recent documents in the Registry.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnFormClosing( System.Windows.Forms.FormClosingEventArgs e )
        {
            if( canCloseDocument( this, e ) )
            {
                // Now, store recent document paths in registry.
                try
                {
                    RegistryKey key = Registry.CurrentUser.CreateSubKey( registryPath );
                    if( key != null )
                    {
                        int maxRd = Math.Min( recentFiles.Count, MaximumRecentDocumentCount );

                        for( int rd = 0; rd < maxRd; rd++ )
                        {
                            key.SetValue( RecentDocumentRegistryEntryName + rd, recentFiles[ rd ] );
                        }
                    }
                }
                catch( Exception ex )
                {
                    Debug.WriteLine( "Error writing recent documents from registry key " + registryPath );
                    Debug.WriteLine( ex.Message );
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnRecentDocumentMenuItemClicked( object sender, EventArgs e )
        {
            string path = (string) ( (ToolStripMenuItem) sender ).Tag;

            loadDocument( path );

            if( !manager.Opened )
            {
                // Clearly not a valid entry any more - so remove it from the list.
                RemoveFileHistory( path );

                MessageBox.Show( this, "Failed to open \"" + path + "\".", "Open Recent",
                    MessageBoxButtons.OK );
            }
            else
            {
                // Move it to the top of the list.
                AddFileHistory( path );
            }
        }

        /// <summary>
        /// Add file name to MRU list.
        /// Call this function when file is opened successfully.
        /// If file already exists in the list, it is moved to the first place.
        /// </summary>
        /// <param name="file">File Name</param>
        public void AddFileHistory( string file )
        {
            RemoveFileHistory( file );

            // if array has maximum length, remove last element
            if( recentFiles.Count == MaximumRecentDocumentCount )
                recentFiles.RemoveAt( MaximumRecentDocumentCount - 1 );

            // add new file name to the start of array
            recentFiles.Insert( 0, file );
        }

        /// <summary>
        /// Remove file name from MRU list.
        /// Call this function when File - Open operation failed.
        /// </summary>
        /// <param name="file">File Name</param>
        public void RemoveFileHistory( string file )
        {
            int i = 0;

            IEnumerator myEnumerator = recentFiles.GetEnumerator();

            while( myEnumerator.MoveNext() )
            {
                if( (string) myEnumerator.Current == file )
                {
                    recentFiles.RemoveAt( i );
                    return;
                }

                i++;
            }
        }
        #endregion
    }
}
