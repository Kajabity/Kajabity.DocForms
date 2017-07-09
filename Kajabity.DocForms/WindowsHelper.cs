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

using System.Runtime.InteropServices;
using System.Text;

namespace Kajabity.DocForms
{
    /// <summary>
    /// Imported Windows DLL utility methods.
    /// </summary>
    public class WindowsHelper
    {
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

        /// <summary>
        /// Truncates a path to fit within a certain number of characters by replacing path components with ellipses.
        /// </summary>
        /// <param name="longPath">a string of length MAX_PATH that contains the path to be altered</param>
        /// <param name="maximumLength">The maximum number of characters to be contained in the new string, including the terminating null character. 
        /// For example, if cchMax = 8, the resulting string can contain a maximum of 7 characters plus the terminating null character.</param>
        /// <returns></returns>
        public static string GetShortPath( string longPath, int maximumLength )
        {
            StringBuilder shortPath = new StringBuilder( maximumLength );
            if( PathCompactPathEx( shortPath, longPath, shortPath.Capacity, 0 ) )
            {
                return shortPath.ToString();
            }

            return longPath;
        }
    }
}
