using System;
using System.Collections.Generic;
using System.Drawing;

namespace Kajabity.DocForms.Forms
{
    /// <summary>Calculates safe top-level window bounds without accessing WinForms controls.</summary>
    public static class WindowPlacementCalculator
    {
        private const int MinimumSavedWidth = 100;
        private const int MinimumSavedHeight = 80;

        /// <summary>
        /// Calculates placement against the current working areas. Returns false for missing or
        /// unreasonable saved information so Windows can choose its normal initial placement.
        /// </summary>
        public static bool TryCalculate(WindowPlacement saved, IReadOnlyList<WindowDisplay> displays,
            Size minimumSize, Size maximumSize, out RestoredWindowPlacement result)
        {
            result = null;
            if (saved == null || displays == null || displays.Count == 0 ||
                !Reasonable(saved.NormalBounds, MinimumSavedWidth, MinimumSavedHeight) ||
                !Reasonable(saved.DisplayBounds, MinimumSavedWidth, MinimumSavedHeight) ||
                !Reasonable(saved.DisplayWorkingArea, MinimumSavedWidth, MinimumSavedHeight))
                return false;

            WindowDisplay target = SelectDisplay(saved, displays);
            if (target == null)
                return false;

            Rectangle area = target.WorkingArea;
            double scale = saved.DisplayDpi > 0 && target.Dpi > 0
                ? (double)target.Dpi / saved.DisplayDpi : 1.0;
            int minWidth = Math.Min(area.Width, Math.Max(MinimumSavedWidth, minimumSize.Width));
            int minHeight = Math.Min(area.Height, Math.Max(MinimumSavedHeight, minimumSize.Height));
            int maxWidth = maximumSize.Width > 0 ? Math.Min(area.Width, maximumSize.Width) : area.Width;
            int maxHeight = maximumSize.Height > 0 ? Math.Min(area.Height, maximumSize.Height) : area.Height;
            // The working area wins when a Form's minimum exceeds the available display.
            maxWidth = Math.Max(minWidth, maxWidth);
            maxHeight = Math.Max(minHeight, maxHeight);
            int width = Clamp(Scaled(saved.NormalBounds.Width, scale), minWidth, maxWidth);
            int height = Clamp(Scaled(saved.NormalBounds.Height, scale), minHeight, maxHeight);

            int x = MapPosition(saved.NormalBounds.X, saved.DisplayWorkingArea.X,
                saved.DisplayWorkingArea.Width, saved.NormalBounds.Width,
                area.X, area.Width, width);
            int y = MapPosition(saved.NormalBounds.Y, saved.DisplayWorkingArea.Y,
                saved.DisplayWorkingArea.Height, saved.NormalBounds.Height,
                area.Y, area.Height, height);

            result = new RestoredWindowPlacement
            {
                NormalBounds = new Rectangle(x, y, width, height),
                IsMaximized = saved.IsMaximized,
                DisplayDeviceName = target.DeviceName
            };
            return true;
        }

        /// <summary>Tests whether enough of a normal window's top edge remains usable on any display.</summary>
        public static bool IsAccessible(Rectangle bounds, IReadOnlyList<WindowDisplay> displays)
        {
            if (displays == null || !Reasonable(bounds, MinimumSavedWidth, MinimumSavedHeight))
                return false;

            Rectangle titleArea = new Rectangle(bounds.X, bounds.Y, bounds.Width, Math.Min(32, bounds.Height));
            foreach (WindowDisplay display in displays)
            {
                if (display == null || !Reasonable(display.WorkingArea, 1, 1))
                    continue;
                Rectangle visible = Rectangle.Intersect(titleArea, display.WorkingArea);
                if (visible.Width >= Math.Min(64, bounds.Width) && visible.Height >= Math.Min(16, titleArea.Height))
                    return true;
            }
            return false;
        }

        private static WindowDisplay SelectDisplay(WindowPlacement saved, IReadOnlyList<WindowDisplay> displays)
        {
            WindowDisplay primary = null;
            WindowDisplay first = null;
            WindowDisplay geometric = null;
            long bestOverlap = 0;
            foreach (WindowDisplay display in displays)
            {
                if (display == null || !Reasonable(display.Bounds, MinimumSavedWidth, MinimumSavedHeight) ||
                    !Reasonable(display.WorkingArea, MinimumSavedWidth, MinimumSavedHeight))
                    continue;
                if (first == null)
                    first = display;
                if (display.IsPrimary)
                    primary = display;
                if (!string.IsNullOrEmpty(saved.DisplayDeviceName) &&
                    string.Equals(saved.DisplayDeviceName, display.DeviceName, StringComparison.OrdinalIgnoreCase))
                    return display;

                Rectangle overlap = Rectangle.Intersect(saved.DisplayBounds, display.Bounds);
                long overlapArea = (long)overlap.Width * overlap.Height;
                long smallerArea = Math.Min((long)saved.DisplayBounds.Width * saved.DisplayBounds.Height,
                    (long)display.Bounds.Width * display.Bounds.Height);
                if (overlapArea > bestOverlap && overlapArea * 2 >= smallerArea)
                {
                    geometric = display;
                    bestOverlap = overlapArea;
                }
            }
            return geometric ?? primary ?? first;
        }

        private static bool Reasonable(Rectangle rectangle, int minWidth, int minHeight)
        {
            return rectangle.Width >= minWidth && rectangle.Height >= minHeight &&
                rectangle.Width <= 100000 && rectangle.Height <= 100000 &&
                Math.Abs((long)rectangle.X) <= 1000000 && Math.Abs((long)rectangle.Y) <= 1000000 &&
                Math.Abs((long)rectangle.X + rectangle.Width) <= 1000000 &&
                Math.Abs((long)rectangle.Y + rectangle.Height) <= 1000000;
        }

        private static int MapPosition(int oldPosition, int oldAreaPosition, int oldAreaSize, int oldSize,
            int newAreaPosition, int newAreaSize, int newSize)
        {
            int oldTravel = Math.Max(0, oldAreaSize - oldSize);
            int newTravel = Math.Max(0, newAreaSize - newSize);
            double fraction = oldTravel == 0 ? 0 : (double)((long)oldPosition - oldAreaPosition) / oldTravel;
            fraction = Math.Max(0, Math.Min(1, fraction));
            return newAreaPosition + (int)Math.Round(fraction * newTravel, MidpointRounding.AwayFromZero);
        }

        private static int Scaled(int value, double ratio)
        {
            return (int)Math.Min(100000, Math.Round(value * ratio, MidpointRounding.AwayFromZero));
        }

        private static int Clamp(int value, int minimum, int maximum)
        {
            return Math.Min(maximum, Math.Max(minimum, value));
        }
    }
}
