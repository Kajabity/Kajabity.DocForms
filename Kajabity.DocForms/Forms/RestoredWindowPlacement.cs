using System.Drawing;

namespace Kajabity.DocForms.Forms
{
    /// <summary>A safe placement calculated for the current display arrangement.</summary>
    public sealed class RestoredWindowPlacement
    {
        internal RestoredWindowPlacement() { }

        /// <summary>Normal bounds within the chosen display's working area.</summary>
        public Rectangle NormalBounds { get; internal set; }

        /// <summary>Whether to maximise after applying normal bounds.</summary>
        public bool IsMaximized { get; internal set; }

        /// <summary>The selected display device name.</summary>
        public string DisplayDeviceName { get; internal set; }
    }
}
