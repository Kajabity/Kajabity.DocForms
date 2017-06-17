using System.Text;

namespace HexViewer
{
    /// <summary>
    /// A structure representing a breadth on the X axis with Left and Right values.
    /// </summary>
    public struct Breadth
    {
        /// <summary>
        /// The leftmost boundary of the breadth.
        /// </summary>
        public float Left { get; set; }

        /// <summary>
        /// The rightmost boundary of the breadth.
        /// </summary>
        public float Right
        {
            get
            {
                return Left + Width;
            }
        }

        /// <summary>
        /// The calculated width of the Breadth.
        /// </summary>
        public float Width { get; set; }

        /// <summary>
        /// Construct a Breadth with bound for Left and Right.
        /// </summary>
        /// <param name="left">left hand boundary</param>
        /// <param name="width">the width</param>
        public Breadth( float left, float width )
        {
            Left = left;
            Width = width;
        }

        /// <summary>
        /// Check if a value is contained within the Left and Right boundaries:
        /// </summary>
        /// <param name="x">the value to be tested</param>
        /// <returns>true if x is Greater than or equal to Left and less than or equal to Right.</returns>
        public bool Contains( float x )
        {
            return Left <= x && x <= Right;
        }

        /// <summary>
        /// Create a new Boundary with boundaries inflated by the specified amount.
        /// </summary>
        /// <param name="amount">the amount to inflate the new Breadth by</param>
        /// <returns>a new inflated Breadth</returns>
        public Breadth Inflate( float amount )
        {
            return new Breadth( Left - amount, Width + 2 * amount );
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append( "(" );
            sb.Append( Left );
            sb.Append( "," );
            sb.Append( Width );
            sb.Append( ")" );
            return sb.ToString();
        }
    }
}
