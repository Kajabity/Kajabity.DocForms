using System.Drawing;

namespace Kajabity.DocForms.Forms
{
    /// <summary>A display description used to calculate window placement without a live Form.</summary>
    public sealed class WindowDisplay
    {
        /// <summary>The Windows display device name.</summary>
        public string DeviceName { get; set; }

        /// <summary>The full display bounds in desktop coordinates.</summary>
        public Rectangle Bounds { get; set; }

        /// <summary>The usable working area in desktop coordinates.</summary>
        public Rectangle WorkingArea { get; set; }

        /// <summary>Whether this is the primary display.</summary>
        public bool IsPrimary { get; set; }

        /// <summary>Effective DPI, or 0 when unknown.</summary>
        public int Dpi { get; set; }
    }
}
