using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Serialization;
using Kajabity.DocForms.Forms;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Kajabity.DocForms.Test.Forms
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    [NonParallelizable]
    public class WindowPlacementManagerTest
    {
        [Test]
        public void InvalidSettingsDoNotChangeInitialFormPlacement()
        {
            using (var form = new Form { StartPosition = FormStartPosition.CenterScreen })
            using (var manager = new WindowPlacementManager(form))
            {
                ClassicAssert.IsFalse(manager.Restore(null));
                ClassicAssert.AreEqual(FormStartPosition.CenterScreen, form.StartPosition);
            }
        }

        [Test]
        public void CaptureUsesNormalBoundsAfterMaximiseAndMinimise()
        {
            Rectangle area = Screen.PrimaryScreen.WorkingArea;
            if (area.Width < 400 || area.Height < 300)
                Assert.Ignore("A usable primary display is required for this WinForms integration check.");
            Rectangle expected = new Rectangle(area.X + 40, area.Y + 40, 320, 240);
            using (var form = new Form { StartPosition = FormStartPosition.Manual, Bounds = expected })
            using (var manager = new WindowPlacementManager(form))
            {
                form.Show();
                form.WindowState = FormWindowState.Maximized;
                WindowPlacement maximised = manager.Capture();
                ClassicAssert.IsTrue(maximised.IsMaximized);
                ClassicAssert.AreEqual(expected, maximised.NormalBounds);

                form.WindowState = FormWindowState.Minimized;
                WindowPlacement minimised = manager.Capture();
                ClassicAssert.IsTrue(minimised.IsMaximized);
                ClassicAssert.AreEqual(expected, minimised.NormalBounds);
            }
        }

        [Test]
        public void MinimiseFromNormalDoesNotPersistMinimisedState()
        {
            Rectangle area = Screen.PrimaryScreen.WorkingArea;
            if (area.Width < 400 || area.Height < 300)
                Assert.Ignore("A usable primary display is required for this WinForms integration check.");
            Rectangle expected = new Rectangle(area.X + 40, area.Y + 40, 320, 240);
            using (var form = new Form { StartPosition = FormStartPosition.Manual, Bounds = expected })
            using (var manager = new WindowPlacementManager(form))
            {
                form.Show();
                form.WindowState = FormWindowState.Minimized;
                WindowPlacement captured = manager.Capture();
                ClassicAssert.IsFalse(captured.IsMaximized);
                ClassicAssert.AreEqual(expected, captured.NormalBounds);
            }
        }

        [Test]
        public void DisposingFormAlsoDisposesManager()
        {
            var form = new Form();
            var manager = new WindowPlacementManager(form);
            form.Dispose();
            Assert.Throws<ObjectDisposedException>((Action)(() => manager.Restore(null)));
        }

        [Test]
        public void SuccessfulFormCloseCanCaptureBeforeBaseFormClosed()
        {
            Rectangle area = Screen.PrimaryScreen.WorkingArea;
            if (area.Width < 400 || area.Height < 300)
                Assert.Ignore("A usable primary display is required for this WinForms integration check.");
            Rectangle expected = new Rectangle(area.X + 40, area.Y + 40, 320, 240);
            using (var form = new CaptureOnClosedForm { StartPosition = FormStartPosition.Manual, Bounds = expected })
            {
                form.Manager = new WindowPlacementManager(form);
                form.Show();
                form.Close();
                ClassicAssert.IsNotNull(form.Captured);
                ClassicAssert.AreEqual(expected, form.Captured.NormalBounds);
            }
        }

        [Test]
        public void MaximisedRestoreBeforeShowKeepsNormalBounds()
        {
            Screen screen = Screen.PrimaryScreen;
            Rectangle area = screen.WorkingArea;
            if (area.Width < 400 || area.Height < 300)
                Assert.Ignore("A usable primary display is required for this WinForms integration check.");
            Rectangle expected = new Rectangle(area.X + 40, area.Y + 40, 320, 240);
            var saved = new WindowPlacement
            {
                NormalBounds = expected,
                IsMaximized = true,
                DisplayDeviceName = screen.DeviceName,
                DisplayBounds = screen.Bounds,
                DisplayWorkingArea = area
            };
            using (var form = new Form())
            using (var manager = new WindowPlacementManager(form))
            {
                ClassicAssert.IsTrue(manager.Restore(saved));
                form.Show();
                WindowPlacement captured = manager.Capture();
                ClassicAssert.IsTrue(captured.IsMaximized);
                ClassicAssert.AreEqual(expected, captured.NormalBounds);
            }
        }

        [Test]
        public void PlacementCanBeSerializedWithoutKdfStorageDependency()
        {
            var original = new WindowPlacement
            {
                NormalBounds = new Rectangle(-700, 20, 600, 400),
                DisplayBounds = new Rectangle(-1280, 0, 1280, 1024),
                DisplayWorkingArea = new Rectangle(-1280, 0, 1280, 984),
                DisplayDeviceName = @"\\.\DISPLAY2",
                DisplayDpi = 144,
                IsMaximized = true
            };
            var serializer = new XmlSerializer(typeof(WindowPlacement));
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, original);
                stream.Position = 0;
                var copy = (WindowPlacement)serializer.Deserialize(stream);
                ClassicAssert.AreEqual(original.NormalBounds, copy.NormalBounds);
                ClassicAssert.AreEqual(original.DisplayBounds, copy.DisplayBounds);
                ClassicAssert.AreEqual(original.DisplayWorkingArea, copy.DisplayWorkingArea);
                ClassicAssert.AreEqual(original.DisplayDeviceName, copy.DisplayDeviceName);
                ClassicAssert.AreEqual(original.DisplayDpi, copy.DisplayDpi);
                ClassicAssert.IsTrue(copy.IsMaximized);
            }
        }

        private sealed class CaptureOnClosedForm : Form
        {
            internal WindowPlacementManager Manager;
            internal WindowPlacement Captured;

            protected override void OnFormClosed(FormClosedEventArgs e)
            {
                Captured = Manager.Capture();
                Manager.Dispose();
                base.OnFormClosed(e);
            }
        }
    }
}
