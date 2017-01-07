using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Kajabity.DocForms.Documents;

namespace Kajabity.DocForms.Forms
{
    /// <summary>
    /// Extends the SDIForm to provide Recent Documents handling.  
    /// 
    /// Add a Recent Documents menu item and then base MainForm on this class, 
    /// tying in members where appropriate.
    /// </summary>
    public class SDIRecentDocumentsForm: SDIForm
    {
        /// <summary>
        /// Maximum length of path the document path entry in the list.
        /// </summary>
        private static int MaximumDisplayNameLength = 100;

        /// <summary>
        /// Maximum number of documents to remember
        /// </summary>
        private static int MaximumRecentDocumentCount = 20;

        /// <summary>
        /// The base path for each Recent Document entry in the registry.
        /// </summary>
        private static string RecentDocumentRegistryEntryName = "RecentDocument-";

        /// <summary>
        /// Registry path where settings and persistent data are stored.
        /// </summary>
        protected string RegistryPath;

        /// <summary>
        /// A reference to the 'Recent Documents' (or whatever) menu item.
        /// </summary>
        private ToolStripMenuItem recentItemsMenuItem;

        /// <summary>
        /// Contains the list of recent document names (full "Absolute" path).
        /// </summary>
        private List<string> recentDocuments = new List<string>();

        //  ---------------------------------------------------------------------
        //  Constructors.
        //  ---------------------------------------------------------------------

        /// <summary>
        /// Default constructor - required to enable the Visual Editor for Forms
        /// in Visual Studio to work.
        /// </summary>
        public SDIRecentDocumentsForm()
        {
            SetRegistryPath();
        }

        /// <summary>
        /// Construct an SDIRecentDocumentsForm providing an instance of a document manager.
        /// Initialises a default Registry path for storing recent documents.
        /// </summary>
        /// <param name="manager">the DocumentManager to be used by this form.</param>
        public SDIRecentDocumentsForm( DocumentManager manager )
            : base( manager )
        {
            SetRegistryPath();
        }

        //  ---------------------------------------------------------------------
        //  Methods.
        //  ---------------------------------------------------------------------

        /// <summary>
        /// Initialises the Recent Documents feature.  Must be called in the 
        /// Form's constructor after initialising components.
        /// Provides the menu item for displaying recent documents.  Loads 
        /// recent documents from the registry.
        /// </summary>
        /// <param name="menuItem"></param>
        protected void InitialseRecentDocuments( ToolStripMenuItem menuItem )
        {
            // Capture the Menu Item.
            this.recentItemsMenuItem = menuItem;

            // Current Directory

            // Now, read any previously stored recent documents.
            try
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey( RegistryPath );
                if( key != null )
                {
                    for( int rd = 0; rd < MaximumRecentDocumentCount; rd++ )
                    {
                        string path = (string) key.GetValue( RecentDocumentRegistryEntryName + rd, "" );
                        if( path != null || path.Length > 0 )
                        {
                            // Convert to a canonical, none relative format.
                            string fullPath = Path.GetFullPath( path );

                            // Ensure unique entries only.
                            RemoveRecentDocument( fullPath );

                            // Now add this to the head of the list.
                            recentDocuments.Add( fullPath );
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            catch( Exception )
            {
                Debug.WriteLine( "Error loading recent documents from registry key " + RegistryPath );
            }
        }

        /// <summary>
        /// Called whenever a document is created or loaded (or closed).
        /// Override, calling the base class implementation, to provide specific Forms handling when the document is changed.
        /// </summary>
        public override void DocumentChanged()
        {
            if( manager.Filename != null )
            {
                AddRecentDocument( manager.Filename );
            }

            base.DocumentChanged();
        }

        /// <summary>
        /// Helper to load a document - triggers DocumentChanged().  
        /// </summary>
        protected override void loadDocument( string filename )
        {
            RemoveRecentDocument( filename );

            base.loadDocument( filename );
        }

        /// <summary>
        /// Add a file to the top of the recent documents list - removing any 
        /// matching entries lower down the list if necessary.  Determins and 
        /// uses the full path to the file.
        /// </summary>
        /// <param name="path">the file path to be added</param>
        protected void AddRecentDocument( string path )
        {
            // Convert to a canonical, none relative format.
            string fullPath = Path.GetFullPath( path );

            // Ensure unique entries only.
            RemoveRecentDocument( fullPath );

            // Now add this to the head of the list.
            recentDocuments.Insert( 0, fullPath );
        }

        /// <summary>
        /// Searches the recent documents list for entries matching the (full) 
        /// path provided and removes them from the list.
        /// </summary>
        /// <param name="path">the file path to be removed</param>
        protected void RemoveRecentDocument( string path )
        {
            // Convert to a canonical, none relative format.
            string fullPath = Path.GetFullPath( path );

            // Iterate through the list to remove the matching entry
            for( int rd = 0; rd < recentDocuments.Count; )
            {
                if( recentDocuments[ rd ].Equals( fullPath ) )
                {
                    recentDocuments.RemoveAt( rd );
                }
                else
                {
                    rd++;
                }
            }
        }

        /// <summary>
        /// This method should be attached in extending classes to the Menu 
        /// Item Drop Down Opening event of the parent of the Recent Documents 
        /// menu item - e.g. fileToolStripMenuItem_DropDownOpening
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnParentMenuDropDownOpening( object sender, EventArgs e )
        {
            if( recentItemsMenuItem.DropDownItems.Count > 0 )
            {
                recentItemsMenuItem.DropDownItems.Clear();
            }

            recentItemsMenuItem.Enabled = recentDocuments.Count > 0;

            int maxRd = Math.Min( recentDocuments.Count, MaximumRecentDocumentCount );
            for( int rd = 0; rd < maxRd; rd++ )
            {
                string path = recentDocuments[ rd ];

                ToolStripMenuItem menuItem = new ToolStripMenuItem( GetShortPath( path ) );
                menuItem.Tag = path;
                menuItem.Click += OnRecentDocumentMenuItemClicked;

                recentItemsMenuItem.DropDownItems.Add( menuItem );
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
                RemoveRecentDocument( path );

                MessageBox.Show( this, "Failed to open \"" + path + "\".", "Open Recent",
                    MessageBoxButtons.OK );
            }
            else
            {
                // Move it to the top of the list.
                AddRecentDocument( path );
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnFormClosing( FormClosingEventArgs e )
        {
            // Now, store recent document paths in registry.
            try
            {
                RegistryKey key = Registry.CurrentUser.CreateSubKey( RegistryPath );
                if( key != null )
                {
                    int maxRd = Math.Min( recentDocuments.Count, MaximumRecentDocumentCount );

                    for( int rd = 0; rd < maxRd; rd++ )
                    {
                        key.SetValue( RecentDocumentRegistryEntryName + rd, recentDocuments[ rd ] );
                    }
                }
            }
            catch( Exception )
            {
                Debug.WriteLine( "Error writing recent documents from registry key " + RegistryPath );
            }

            base.OnFormClosing( e );
        }

        //  ---------------------------------------------------------------------
        //  Private Methods.
        //  ---------------------------------------------------------------------

        /// <summary>
        /// Sets a default registry path for storing recent documents based on values from AssemblyInfo.
        /// </summary>
        private void SetRegistryPath()
        {
            // Set registry path from Application Settings.
            Assembly assembly = Assembly.GetExecutingAssembly();
            string companyName = ( (AssemblyCompanyAttribute) Attribute.GetCustomAttribute( assembly, typeof( AssemblyCompanyAttribute ), false ) ).Company;
            string appName = assembly.GetName().Name;

            this.RegistryPath = "Software\\" + companyName + "\\" + appName + "\\Recent Documents";
            Debug.WriteLine( "Registry Path = " + RegistryPath );
        }

        /// <summary>
        /// Truncates a path to fit within a certain number of characters by 
        /// replacing path components with ellipses.
        /// <strong>Remarks</strong>
        /// The '/' separator will be used instead of '\' if the original 
        /// string used it. If pszSrc points to a file name that is too long, 
        /// instead of a path, the file name will be truncated to cchMax 
        /// characters, including the ellipsis and the terminating NULL 
        /// character. For example, if the input file name is "My Filename" 
        /// and cchMax is 10, PathCompactPathEx will return "My Fil...".
        /// </summary>
        /// <param name="pszOut">The address of the string that has been altered.</param>
        /// <param name="pszSrc ">A pointer to a null-terminated string of length MAX_PATH that contains the path to be altered.</param>
        /// <param name="cchMax">
        /// The maximum number of characters to be contained in the new string, including the terminating null character. 
        /// For example, if cchMax = 8, the resulting string can contain a maximum of 7 characters plus the terminating null character.
        /// </param>
        /// <param name="dwFlags">Reserved</param>
        /// <returns>Returns TRUE if successful, or FALSE otherwise.</returns>
        [DllImport( "shlwapi.dll", CharSet = CharSet.Auto )]
        static extern bool PathCompactPathEx( [Out] StringBuilder pszOut, string pszSrc, int cchMax, int dwFlags );

        /// <summary>
        /// Truncates the full path of a recent document to fit within MaximumDisplayNameLength characters.
        /// </summary>
        /// <param name="longPath">the original file path.</param>
        /// <returns>a truncated file path - using elipses where necessary</returns>
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
    }
}
