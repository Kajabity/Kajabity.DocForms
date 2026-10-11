using System;
using System.Drawing;

namespace Kajabity.DocForms.Forms
{
    /// <summary>
    /// The normal bounds and display context of a top-level window. Applications choose how to persist it.
    /// </summary>
    [Serializable]
    public sealed class WindowPlacement
    {
        /// <summary>The bounds occupied when the window is restored, in desktop coordinates.</summary>
        public Rectangle NormalBounds { get; set; }

        /// <summary>Whether to reopen the window maximised. A minimised window is stored as restored.</summary>
        public bool IsMaximized { get; set; }

        /// <summary>The display device name reported by Windows when the placement was captured.</summary>
        public string DisplayDeviceName { get; set; }

        /// <summary>The full bounds of the display when the placement was captured.</summary>
        public Rectangle DisplayBounds { get; set; }

        /// <summary>The usable working area of the display when the placement was captured.</summary>
        public Rectangle DisplayWorkingArea { get; set; }

        /// <summary>The effective DPI of that display, or 0 if it could not be determined.</summary>
        public int DisplayDpi { get; set; }
    }
}
