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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

using Kajabity.DocForms.Documents;
using Microsoft.Win32;

namespace Kajabity.DocForms.Forms
{
    /// <summary>
    /// Extends System.Windows.Forms.Form to implement support for handling 
    /// Documents in a Single Document Interface (SDI) style.
    /// </summary>
    public class SingleDocumentForm<TDocument> : Form where TDocument : Document
    {
        //  ---------------------------------------------------------------------
        //  Attributes.
        //  ---------------------------------------------------------------------

        private SingleDocumentManager<TDocument> _manager;

        /// <summary>
        /// A reference to an instance of the SingleDocumentManager used to load and save
        /// documents for the application.
        /// </summary>
        protected SingleDocumentManager<TDocument> Manager
        {
            get
            {
                return _manager;
            }

            set
            {
                if (_manager != null)
                {
                    _manager.DocumentStatusChanged -= Manager_OnDocumentStatusChanged;
                }

                _manager = value;

                if (_manager != null)
                {
                    _manager.DocumentStatusChanged += Manager_OnDocumentStatusChanged;
                }
            }
        }

        private void Manager_OnDocumentStatusChanged(object sender, EventArgs eventArgs)
        {
            OnDocumentStatusChanged(eventArgs);
        }

        /// <summary>
        /// If true, when saving a document any existing file with the same name 
        /// will be renamed with a trailing '~' character as a backup.  Any 
        /// previous backup (i.e. with the same backup filename) will be deleted.
        /// </summary>
        [
        Category("Document Handling"),
        Description("Indicates whether to backup documents on save.")
        ]
        public bool Backup { get; set; } = false;

        //  ---------------------------------------------------------------------
        //  Recent Document Attributes.
        //  ---------------------------------------------------------------------

        /// <summary>
        /// The base path for each Recent Document entry in the registry.
        /// </summary>
        private const string RecentDocumentRegistryEntryName = "RecentDocument-";

        /// <summary>
        /// Maximum number of documents to store in the recent files list.
        /// </summary>
        [
        Category("Document Handling"),
        Description("The maximum number of recent documents to store in the recent files list.")
        ]
        public int MaximumRecentDocumentCount { get; set; } = 20;

        /// <summary>
        /// Maximum length of each document name and path entry in the recent documents menu.
        /// Longer names will be shortened to fit. 
        /// </summary>
        [
        Category("Document Handling"),
        Description("Maximum length of each document name and path entry in the recent documents menu.")
        ]
        public int MaximumRecentDocumentDisplayLength { get; set; } = 100;

        /// <summary>
        /// Holds a list of the most recent filenames used by the application.
        /// May be used to support Recent Files menu features.
        /// </summary>
        private readonly List<string> _recentFiles = new List<string>();

        /// <summary>
        /// A reference to a menu item to be used to contain a list of recently opened 
        /// files.
        /// </summary>
        private ToolStripMenuItem _recentItemsMenuItem;

        /// <summary>
        /// The registry path under which recent documents are stored.
        /// </summary>
        protected string RegistryPath;

        //  ---------------------------------------------------------------------
        //  Constructors.
        //  ---------------------------------------------------------------------

        /// <summary>
        /// Default constructor - required to enable the Visual Editor for Forms
        /// in Visual Studio to work.
        /// </summary>
        public SingleDocumentForm()
        {
        }

        /// <summary>
        /// Construct an SingleDocumentForm providing an instance of a document manager.
        /// </summary>
        /// <param name="manager">the SingleDocumentManager to be used by this form.</param>
        public SingleDocumentForm(SingleDocumentManager<TDocument> manager)
        {
            Manager = manager;
        }

        //  ---------------------------------------------------------------------
        //  Events
        //  ---------------------------------------------------------------------

        /// <summary>
        /// An event that clients can use to be notified whenever the Document is created, loaded or closed.
        /// </summary>
        [
        Category("Document Handling"),
        Description("Notifies whenever the Document is created, loaded or closed.")
        ]
        public event EventHandler<EventArgs> DocumentChanged;

        /// <summary>
        /// Called whenever a document is created, loaded or closed.
        /// </summary>
        protected virtual void OnDocumentChanged(EventArgs args)
        {
            DocumentChanged?.Invoke(this, args);
        }

        /// <summary>
        /// An event that clients can use to be notified whenever a document 
        /// name or modified status is (or may have been) changed.
        /// Handle this event to provide specific Forms handling; for example 
        /// update window title with filename or status display.
        /// </summary>
        [
        Category("Document Handling"),
        Description("Notifies whenever a document name or modified status is (or may have been) changed.")
        ]
        public event EventHandler<EventArgs> DocumentStatusChanged;

        /// <summary>
        /// Called whenever a document name or modified status is or may have been changed.
        /// </summary>
        protected virtual void OnDocumentStatusChanged(EventArgs args)
        {
            DocumentStatusChanged?.Invoke(this, args);
        }

        //  ---------------------------------------------------------------------
        //  Command Handling Methods.
        //  ---------------------------------------------------------------------

        /// <summary>
        /// A handler for the File->New command.  
        /// Closes any currently open document (prompting the user to save it if modified).
        /// Creates a new instance of the managed document type.
        /// Call this method your form's 'OnClick()' handler.
        /// </summary>
        /// <param name="sender">the object that sent the event - e.g. menu item or tool bar button.</param>
        /// <param name="e">Any additional arguments to the event.</param>
        public void FileNewClick(object sender, EventArgs e)
        {
            if (AttemptCloseDocument(sender, e))
            {
                NewDocument();
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
        public void FileOpenClick(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Open " + Application.ProductName + " file";
            dialog.Filter = Application.ProductName + " files (*." + Manager.DefaultExtension + ")|*." + Manager.DefaultExtension +
                "|All files (*.*)|*.*";

            dialog.AddExtension = true;
            dialog.CheckFileExists = true;
            dialog.CheckPathExists = true;
            dialog.ReadOnlyChecked = false;
            dialog.DefaultExt = Manager.DefaultExtension;
            //dialog.InitialDirectory = @"C:\";
            //dialog.ShowHelp = true; // Need to handle 'HelpRequest' event.

            if (dialog.ShowDialog() == DialogResult.OK && AttemptCloseDocument(sender, e))
            {
                Debug.WriteLine(Manager.DefaultExtension + " file: " + dialog.FileName);

                LoadDocument(dialog.FileName);
            }
        }

        /// <summary>
        /// A handler for the File->Close command.  
        /// Closes any currently open document (prompting the user to save it if modified).
        /// Call this method your form's 'OnClick()' handler.
        /// </summary>
        /// <param name="sender">the object that sent the event - e.g. menu item or tool bar button.</param>
        /// <param name="e">Any additional arguments to the event.</param>
        public void FileCloseClick(object sender, EventArgs e)
        {
            AttemptCloseDocument(sender, e);
        }

        /// <summary>
        /// A handler for the File->Save command.  
        /// Saves the currently open document - prompting for a filename if it is a new document.
        /// Call this method your form's 'OnClick()' handler.
        /// </summary>
        /// <param name="sender">the object that sent the event - e.g. menu item or tool bar button.</param>
        /// <param name="e">Any additional arguments to the event.</param>
        public void FileSaveClick(object sender, EventArgs e)
        {
            if (Manager.NewFile)
            {
                FileSaveAsClick(sender, e);
            }
            else
            {
                SaveDocument(Manager.Filename);
            }
        }

        /// <summary>
        /// A handler for the File->Save As command.  
        /// Prompts the user for a filename and path and saves the document to it.
        /// Call this method your form's 'OnClick()' handler.
        /// </summary>
        /// <param name="sender">the object that sent the event - e.g. menu item or tool bar button.</param>
        /// <param name="e">Any additional arguments to the event.</param>
        public void FileSaveAsClick(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.DefaultExt = Manager.DefaultExtension;
            dialog.Title = "Save " + Application.ProductName + " file";
            dialog.Filter = Application.ProductName + " files (*." + Manager.DefaultExtension + ")|*." + Manager.DefaultExtension +
                "|All files (*.*)|*.*";
            dialog.FileName = Manager.Filename;

            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                Debug.WriteLine(Manager.DefaultExtension + " file: " + dialog.FileName);

                SaveDocument(dialog.FileName);
            }
        }

        /// <summary>
        /// A handler for the File->Exit command.  
        /// Exits the application, closing any currently open document (prompting the user to save it if modified).
        /// Call this method your form's 'OnClick()' handler.
        /// </summary>
        /// <param name="sender">the object that sent the event - e.g. menu item or tool bar button.</param>
        /// <param name="e">Any additional arguments to the event.</param>
        public void FileExitClick(object sender, EventArgs e)
        {
            if (AttemptCloseDocument(sender, e))
            {
                Application.Exit();
            }
        }

        //  ---------------------------------------------------------------------
        //  File Handling Methods.
        //  ---------------------------------------------------------------------

        /// <summary>
        /// Helper to create a new document - triggers DocumentChanged().
        /// </summary>
        protected void NewDocument()
        {
            Debug.WriteLine("==== NewDocument() ====");
            if (Manager != null)
            {
                Manager.NewDocument();
            }

            OnDocumentChanged(new EventArgs());
            OnDocumentStatusChanged(new EventArgs());
        }

        /// <summary>
        /// Helper to load a document - triggers DocumentChanged().  
        /// </summary>
        /// <param name="filename">the path of the file to be loaded.</param>
        protected virtual void LoadDocument(string filename)
        {
            Debug.WriteLine("==== LoadDocument() ====");
            try
            {
                //	load the file
                Manager.Load(filename);

                // add successfully opened file to MRU list
                AddFileHistory(filename);

                //	Refresh display.
                OnDocumentChanged(new EventArgs());
            }
            catch (Exception ex)
            {
                // Report the error
                Debug.WriteLine("Exception in LoadDocument: " + ex);

                // Remove file from MRU list - if it exists
                RemoveFileHistory(filename);

                MessageBox.Show(this, ex.Message, "Error opening file", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Helper to save a document - triggers DocumentStatusChanged().
        /// </summary>
        /// <param name="filename">the filename and path to save the document into</param>
        private void SaveDocument(string filename)
        {
            Debug.WriteLine("==== SaveDocument() ====");
            try
            {
                if (Backup && File.Exists(filename))
                {
                    string backupFilename = filename + "~";

                    if (File.Exists(backupFilename))
                    {
                        File.Delete(backupFilename);
                    }

                    File.Move(filename, backupFilename);
                }

                //	Save the file
                Manager.Save(filename);

                // add successfully opened file to MRU list
                AddFileHistory(filename);
            }
            catch (Exception ex)
            {
                // Report the error
                Debug.WriteLine("Exception in SaveDocument: " + ex);

                // Remove file from MRU list - if it exists
                RemoveFileHistory(filename);

                MessageBox.Show(this, ex.Message, "Error Saving file", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Helper to close any currently open document - triggers DocumentChanged().
        /// </summary>
        private void CloseDocument()
        {
            Debug.WriteLine("==== CloseDocument() ====");
            Manager.Close();

            //	Refresh display.
            OnDocumentChanged(new EventArgs());
        }

        /// <summary>
        /// A "helper" menu action to try to close the currently loaded document, if there is one.
        /// The method will display a prompt to the user to save the file if it has been modified.
        /// </summary>
        /// <param name="sender">the object that sent the event - e.g. menu item or tool bar button.</param>
        /// <param name="e">Any additional arguments to the event.</param>
        /// <returns>returns true if the document is closed.</returns>
        protected bool AttemptCloseDocument(object sender, EventArgs e)
        {
            if (Manager.Modified)
            {
                DialogResult result = MessageBox.Show(this, Application.ProductName + " file " + Manager.Filename + " has been modified!\n\nDo you want to save it?",
                                                      Application.ProductName,
                                                      MessageBoxButtons.YesNoCancel,
                                                      MessageBoxIcon.Exclamation);

                if (result == DialogResult.Yes)
                {
                    FileSaveClick(sender, e);
                }
                else if (result == DialogResult.Cancel)
                {
                    return false;
                }
            }

            CloseDocument();

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
        public void InitialseRecentDocuments(ToolStripMenuItem recentItemsMenuItem)
        {
            _recentItemsMenuItem = recentItemsMenuItem;

            // Read any previously stored recent documents.
            try
            {
                // Set registry path from Application Settings (the 'Entry Assembly').
                Assembly assembly = Assembly.GetEntryAssembly();
                string companyName = ((AssemblyCompanyAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyCompanyAttribute), false)).Company;
                string appName = assembly.GetName().Name;

                RegistryPath = "Software\\" + companyName + "\\" + appName + "\\Recent Documents";
                Debug.WriteLine("Registry Path = " + RegistryPath);

                RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryPath);
                if (key != null)
                {
                    for (int rd = 0; rd < MaximumRecentDocumentCount; rd++)
                    {
                        string path = (string)key.GetValue(RecentDocumentRegistryEntryName + rd, "");
                        if (!string.IsNullOrEmpty(path))
                        {
                            // Convert to a canonical, none relative format.
                            string fullPath = Path.GetFullPath(path);

                            // Ensure unique entries only.
                            RemoveFileHistory(fullPath);

                            // Now add this to the head of the list.
                            _recentFiles.Add(fullPath);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error loading recent documents from registry key " + RegistryPath);
                Debug.Write(ex.Message);
            }
        }

        /// <summary>
        /// On Parent Menu DropDown Opening - e.g. fileToolStripMenuItem_DropDownOpening.
        /// This handler will populate the recent file entries in your Recent documents menu item,
        /// or disable it if there are no recent documents.
        /// </summary>
        protected void OnParentMenuDropDownOpening(object sender, EventArgs e)
        {
            if (_recentItemsMenuItem.DropDownItems.Count > 0)
            {
                _recentItemsMenuItem.DropDownItems.Clear();
            }

            _recentItemsMenuItem.Enabled = _recentFiles.Count > 0;

            int maxRd = Math.Min(_recentFiles.Count, MaximumRecentDocumentCount);
            for (int rd = 0; rd < maxRd; rd++)
            {
                string path = _recentFiles[rd];

                ToolStripMenuItem menuItem = new ToolStripMenuItem(WindowsHelper.GetShortPath(path, MaximumRecentDocumentDisplayLength));
                menuItem.Tag = path;
                menuItem.ToolTipText = path;
                menuItem.Click += OnRecentDocumentMenuItemClicked;

                _recentItemsMenuItem.DropDownItems.Add(menuItem);
            }
        }


        /// <summary>
        /// When the for closes, save the recent documents in the Registry.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (AttemptCloseDocument(this, e))
            {
                // Now, store recent document paths in registry.
                try
                {
                    RegistryKey key = Registry.CurrentUser.CreateSubKey(RegistryPath);
                    if (key != null)
                    {
                        int maxRd = Math.Min(_recentFiles.Count, MaximumRecentDocumentCount);

                        for (int rd = 0; rd < maxRd; rd++)
                        {
                            key.SetValue(RecentDocumentRegistryEntryName + rd, _recentFiles[rd]);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error writing recent documents from registry key " + RegistryPath);
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        /// <summary>
        /// Menu item handler for the added Recent Document entries.  Loads the specified file - or 
        /// removes them from the list if they can't be opened.  Uses LoadDocument().
        /// </summary>
        /// <param name="sender">unused.</param>
        /// <param name="e">unused.</param>
        protected virtual void OnRecentDocumentMenuItemClicked(object sender, EventArgs e)
        {
            string path = (string)((ToolStripMenuItem)sender).Tag;

            LoadDocument(path);
        }

        /// <summary>
        /// Add file name to MRU list.
        /// Call this function when file is opened successfully.
        /// If file already exists in the list, it is moved to the first place.
        /// </summary>
        /// <param name="file">The filename to be added.</param>
        public void AddFileHistory(string file)
        {
            RemoveFileHistory(file);

            // if array has maximum length, remove last element
            if (_recentFiles.Count == MaximumRecentDocumentCount)
                _recentFiles.RemoveAt(MaximumRecentDocumentCount - 1);

            // add new file name to the start of array
            _recentFiles.Insert(0, file);
        }

        /// <summary>
        /// Remove file name from MRU list.
        /// Call this function when File - Open operation failed.
        /// </summary>
        /// <param name="file">File Name</param>
        public void RemoveFileHistory(string file)
        {
            IEnumerator myEnumerator = _recentFiles.GetEnumerator();
            int i = 0;

            while (myEnumerator.MoveNext())
            {
                if ((string)myEnumerator.Current == file)
                {
                    _recentFiles.RemoveAt(i);
                    return;
                }

                i++;
            }
        }
        #endregion
    }
}
