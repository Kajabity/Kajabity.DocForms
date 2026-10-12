using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Kajabity.DocForms.Forms
{
    /// <summary>
    /// Opt-in WinForms integration for one independent top-level form. Construct on the
    /// form's UI thread, restore before showing it, save a non-null capture after a
    /// successful close, and dispose the manager. The application owns persistence.
    /// MDI children are unsupported; the manager unsubscribes on close or disposal.
    /// </summary>
    public sealed class WindowPlacementManager : IDisposable
    {
        private readonly Form _form;
        private WindowPlacement _lastPlacement;
        private FormWindowState _lastNonMinimizedState = FormWindowState.Normal;
        private bool _applying;
        private volatile bool _displayChangePending;
        private bool _subscribedToSystemEvents;
        private bool _disposed;

        /// <summary>Begins tracking a form. No placement is applied until Restore is called.</summary>
        public WindowPlacementManager(Form form)
        {
            _form = form ?? throw new ArgumentNullException(nameof(form));
            if (form.IsMdiChild)
                throw new ArgumentException("MDI children are not desktop windows.", nameof(form));
            _form.Resize += OnResize;
            _form.HandleCreated += OnHandleCreated;
            _form.FormClosed += OnFormClosed;
            _form.Disposed += OnFormDisposed;
            if (_form.IsHandleCreated)
                SubscribeToDisplayChanges();
        }

        /// <summary>
        /// Applies saved placement before the form is shown. Returns false without changing the
        /// form when settings are absent or invalid, allowing normal Windows placement.
        /// </summary>
        public bool Restore(WindowPlacement saved)
        {
            ThrowIfDisposed();
            List<WindowDisplay> displays = GetDisplays();
            RestoredWindowPlacement placement;
            if (!WindowPlacementCalculator.TryCalculate(saved, displays, _form.MinimumSize,
                _form.MaximumSize, out placement))
                return false;

            _applying = true;
            try
            {
                _form.StartPosition = FormStartPosition.Manual;
                _form.WindowState = FormWindowState.Normal;
                _form.Bounds = placement.NormalBounds;
                _form.WindowState = placement.IsMaximized ? FormWindowState.Maximized : FormWindowState.Normal;
                _lastNonMinimizedState = placement.IsMaximized ? FormWindowState.Maximized : FormWindowState.Normal;
                _lastPlacement = CreatePlacement(placement.NormalBounds, placement.IsMaximized,
                    FindDisplay(displays, placement.DisplayDeviceName));
            }
            finally
            {
                _applying = false;
            }
            return true;
        }

        /// <summary>
        /// Captures normal bounds and the last non-minimised state. Returns the last valid
        /// placement during a display change or when bounds are invalid; returns null if
        /// no valid placement has yet been observed. Do not overwrite stored settings with null.
        /// </summary>
        public WindowPlacement Capture()
        {
            if (_displayChangePending || _form.IsDisposed)
                return _lastPlacement;

            Rectangle normal = _form.WindowState == FormWindowState.Normal
                ? _form.Bounds : _form.RestoreBounds;
            List<WindowDisplay> displays = GetDisplays();
            if (!WindowPlacementCalculator.IsAccessible(normal, displays))
                return _lastPlacement;

            WindowDisplay display = FindDisplayForBounds(normal, displays);
            if (display == null)
                return _lastPlacement;

            bool maximized = _form.WindowState == FormWindowState.Maximized ||
                (_form.WindowState == FormWindowState.Minimized &&
                 _lastNonMinimizedState == FormWindowState.Maximized);
            _lastPlacement = CreatePlacement(normal, maximized, display);
            return _lastPlacement;
        }

        /// <summary>
        /// After Windows has processed a display change, moves the form only if its title bar is
        /// no longer accessible. Windows retains control of ordinary live window repositioning.
        /// </summary>
        public bool EnsureAccessible()
        {
            if (_disposed || _form.IsDisposed || _form.WindowState == FormWindowState.Minimized)
                return false;

            List<WindowDisplay> displays = GetDisplays();
            Rectangle normal = _form.WindowState == FormWindowState.Normal
                ? _form.Bounds : _form.RestoreBounds;
            if (WindowPlacementCalculator.IsAccessible(normal, displays))
                return false;

            WindowPlacement source = _lastPlacement;
            if (source == null)
            {
                WindowDisplay fallback = FindPrimary(displays);
                if (fallback == null || normal.Width < 100 || normal.Height < 80)
                    return false;
                source = CreatePlacement(normal, _form.WindowState == FormWindowState.Maximized, fallback);
            }
            return Restore(source);
        }

        private void OnResize(object sender, EventArgs e)
        {
            if (!_applying && _form.WindowState != FormWindowState.Minimized)
                _lastNonMinimizedState = _form.WindowState;
        }

        private void OnHandleCreated(object sender, EventArgs e)
        {
            SubscribeToDisplayChanges();
        }

        private void SubscribeToDisplayChanges()
        {
            if (_subscribedToSystemEvents || _disposed)
                return;
            try
            {
                SystemEvents.DisplaySettingsChanged += OnDisplaySettingsChanged;
                _subscribedToSystemEvents = true;
            }
            catch (InvalidOperationException)
            {
                // Some hosts do not provide the system-events window. Initial restore still works.
            }
        }

        private void OnDisplaySettingsChanged(object sender, EventArgs e)
        {
            if (_disposed || _displayChangePending || !_form.IsHandleCreated || _form.IsDisposed)
                return;
            _displayChangePending = true;
            try
            {
                _form.BeginInvoke((Action)(() =>
                {
                    try { EnsureAccessible(); }
                    finally { _displayChangePending = false; }
                }));
            }
            catch (ObjectDisposedException) { _displayChangePending = false; }
            catch (InvalidOperationException) { _displayChangePending = false; }
        }

        private void OnFormClosed(object sender, FormClosedEventArgs e)
        {
            Dispose();
        }

        private void OnFormDisposed(object sender, EventArgs e)
        {
            Dispose();
        }

        /// <summary>Unsubscribes from Form and system display events.</summary>
        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;
            _form.Resize -= OnResize;
            _form.HandleCreated -= OnHandleCreated;
            _form.FormClosed -= OnFormClosed;
            _form.Disposed -= OnFormDisposed;
            if (_subscribedToSystemEvents)
                SystemEvents.DisplaySettingsChanged -= OnDisplaySettingsChanged;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(WindowPlacementManager));
        }

        private static WindowPlacement CreatePlacement(Rectangle bounds, bool maximized, WindowDisplay display)
        {
            return new WindowPlacement
            {
                NormalBounds = bounds,
                IsMaximized = maximized,
                DisplayDeviceName = display.DeviceName,
                DisplayBounds = display.Bounds,
                DisplayWorkingArea = display.WorkingArea,
                DisplayDpi = display.Dpi
            };
        }

        private static WindowDisplay FindDisplay(IReadOnlyList<WindowDisplay> displays, string name)
        {
            foreach (WindowDisplay display in displays)
                if (string.Equals(display.DeviceName, name, StringComparison.OrdinalIgnoreCase))
                    return display;
            return FindPrimary(displays);
        }

        private static WindowDisplay FindPrimary(IReadOnlyList<WindowDisplay> displays)
        {
            foreach (WindowDisplay display in displays)
                if (display.IsPrimary)
                    return display;
            return displays.Count > 0 ? displays[0] : null;
        }

        private static WindowDisplay FindDisplayForBounds(Rectangle bounds, IReadOnlyList<WindowDisplay> displays)
        {
            WindowDisplay best = null;
            long bestArea = 0;
            foreach (WindowDisplay display in displays)
            {
                Rectangle intersection = Rectangle.Intersect(bounds, display.WorkingArea);
                long area = (long)intersection.Width * intersection.Height;
                if (area > bestArea)
                {
                    best = display;
                    bestArea = area;
                }
            }
            return best;
        }

        private static List<WindowDisplay> GetDisplays()
        {
            var result = new List<WindowDisplay>();
            foreach (Screen screen in Screen.AllScreens)
            {
                result.Add(new WindowDisplay
                {
                    DeviceName = screen.DeviceName,
                    Bounds = screen.Bounds,
                    WorkingArea = screen.WorkingArea,
                    IsPrimary = screen.Primary,
                    Dpi = GetDisplayDpi(screen.Bounds)
                });
            }
            return result;
        }

        private static int GetDisplayDpi(Rectangle bounds)
        {
            try
            {
                var centre = new NativePoint
                {
                    X = bounds.X + bounds.Width / 2,
                    Y = bounds.Y + bounds.Height / 2
                };
                IntPtr monitor = MonitorFromPoint(centre, 2);
                uint x, y;
                if (monitor != IntPtr.Zero && GetDpiForMonitor(monitor, 0, out x, out y) == 0 && x > 0)
                    return (int)x;
            }
            catch (DllNotFoundException) { }
            catch (EntryPointNotFoundException) { }
            return 0;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NativePoint { public int X; public int Y; }

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromPoint(NativePoint point, uint flags);

        [DllImport("shcore.dll")]
        private static extern int GetDpiForMonitor(IntPtr monitor, int dpiType, out uint dpiX, out uint dpiY);
    }
}
