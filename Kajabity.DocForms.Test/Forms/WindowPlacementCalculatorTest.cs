using System.Collections.Generic;
using System.Drawing;
using Kajabity.DocForms.Forms;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Kajabity.DocForms.Test.Forms
{
    [TestFixture]
    public class WindowPlacementCalculatorTest
    {
        private static readonly WindowDisplay Primary = Display("PRIMARY", 0, 0, 1920, 1080, 1040, true, 96);
        private static readonly WindowDisplay Secondary = Display("SECONDARY", 1920, 0, 1920, 1080, 1040, false, 96);

        [Test]
        public void OriginalMonitorRestoresExactNormalBounds()
        {
            WindowPlacement saved = Save(Primary, new Rectangle(100, 100, 800, 600));
            RestoredWindowPlacement result = Calculate(saved, Secondary, Primary);
            ClassicAssert.AreEqual(saved.NormalBounds, result.NormalBounds);
            ClassicAssert.AreEqual("PRIMARY", result.DisplayDeviceName);
            ClassicAssert.IsFalse(result.IsMaximized);
        }

        [Test]
        public void MaximisedPlacementRetainsNormalBoundsAndState()
        {
            WindowPlacement saved = Save(Primary, new Rectangle(100, 100, 800, 600));
            saved.IsMaximized = true;
            RestoredWindowPlacement result = Calculate(saved, Primary);
            ClassicAssert.AreEqual(saved.NormalBounds, result.NormalBounds);
            ClassicAssert.IsTrue(result.IsMaximized);
        }

        [Test]
        public void PreviouslyMinimisedPlacementReopensNormal()
        {
            // Minimized is deliberately not a persisted state in the placement model.
            RestoredWindowPlacement result = Calculate(Save(Primary, new Rectangle(100, 100, 800, 600)), Primary);
            ClassicAssert.IsFalse(result.IsMaximized);
        }

        [Test]
        public void MissingMonitorFallsBackToPrimaryWorkingArea()
        {
            WindowPlacement saved = Save(Secondary, new Rectangle(2200, 100, 800, 600));
            RestoredWindowPlacement result = Calculate(saved, Primary);
            ClassicAssert.AreEqual("PRIMARY", result.DisplayDeviceName);
            ClassicAssert.IsTrue(Primary.WorkingArea.Contains(result.NormalBounds));
        }

        [Test]
        public void NegativeDesktopCoordinatesArePreserved()
        {
            WindowDisplay left = Display("LEFT", -1280, -200, 1280, 1024, 984, false, 96);
            Rectangle bounds = new Rectangle(-1100, -100, 700, 500);
            RestoredWindowPlacement result = Calculate(Save(left, bounds), Primary, left);
            ClassicAssert.AreEqual(bounds, result.NormalBounds);
        }

        [TestCase(1800, 900)]
        [TestCase(5000, 5000)]
        public void PartlyOrFullyOutsideWorkingAreaIsClamped(int x, int y)
        {
            RestoredWindowPlacement result = Calculate(Save(Primary, new Rectangle(x, y, 800, 600)), Primary);
            ClassicAssert.IsTrue(Primary.WorkingArea.Contains(result.NormalBounds));
        }

        [Test]
        public void OversizedWindowFitsWorkingArea()
        {
            WindowPlacement saved = Save(Primary, new Rectangle(0, 0, 1900, 1000));
            WindowDisplay smaller = Display("PRIMARY", 0, 0, 800, 600, 560, true, 96);
            RestoredWindowPlacement result = Calculate(saved, smaller);
            ClassicAssert.AreEqual(new Size(800, 560), result.NormalBounds.Size);
            ClassicAssert.IsTrue(smaller.WorkingArea.Contains(result.NormalBounds));
        }

        [Test]
        public void InvalidOrVerySmallSettingsUseWindowsDefault()
        {
            RestoredWindowPlacement result;
            ClassicAssert.IsFalse(TryCalculate(null, new[] { Primary }, out result));
            ClassicAssert.IsFalse(TryCalculate(Save(Primary, new Rectangle(10, 10, 0, 600)), new[] { Primary }, out result));
            ClassicAssert.IsFalse(TryCalculate(Save(Primary, new Rectangle(10, 10, 80, 70)), new[] { Primary }, out result));
            ClassicAssert.IsFalse(TryCalculate(Save(Primary, new Rectangle(2000000, 0, 800, 600)), new[] { Primary }, out result));
            ClassicAssert.IsNull(result);
        }

        [Test]
        public void ChangedResolutionKeepsRelativePlacement()
        {
            WindowDisplay smaller = Display("PRIMARY", 0, 0, 1280, 720, 680, true, 96);
            RestoredWindowPlacement result = Calculate(Save(Primary, new Rectangle(560, 220, 800, 400)), smaller);
            ClassicAssert.AreEqual(new Rectangle(240, 96, 800, 400), result.NormalBounds);
        }

        [Test]
        public void MixedDpiScalesNormalSizeAndKeepsWindowVisible()
        {
            WindowDisplay highDpi = Display("PRIMARY", 0, 0, 1920, 1080, 1040, true, 144);
            RestoredWindowPlacement result = Calculate(Save(Primary, new Rectangle(100, 100, 800, 500)), highDpi);
            ClassicAssert.AreEqual(new Size(1200, 750), result.NormalBounds.Size);
            ClassicAssert.IsTrue(highDpi.WorkingArea.Contains(result.NormalBounds));
        }

        [Test]
        public void ChangedDeviceNameUsesGeometricMatchAmongSimilarDisplays()
        {
            WindowPlacement saved = Save(Secondary, new Rectangle(2200, 100, 800, 600));
            WindowDisplay renamed = Display("RENAMED", 1920, 0, 1920, 1080, 1040, false, 96);
            RestoredWindowPlacement result = Calculate(saved, Primary, renamed);
            ClassicAssert.AreEqual("RENAMED", result.DisplayDeviceName);
            ClassicAssert.AreEqual(saved.NormalBounds, result.NormalBounds);
        }

        [Test]
        public void DeviceNameWinsWhenSimilarMonitorsChangePositions()
        {
            WindowPlacement saved = Save(Secondary, new Rectangle(2200, 100, 800, 600));
            WindowDisplay moved = Display("SECONDARY", -1920, 0, 1920, 1080, 1040, false, 96);
            RestoredWindowPlacement result = Calculate(saved, Primary, moved);
            ClassicAssert.AreEqual("SECONDARY", result.DisplayDeviceName);
            ClassicAssert.IsTrue(moved.WorkingArea.Contains(result.NormalBounds));
        }

        [Test]
        public void RepeatedRestorationDoesNotDrift()
        {
            WindowDisplay changed = Display("PRIMARY", 0, 0, 1280, 720, 680, true, 120);
            RestoredWindowPlacement first = Calculate(Save(Primary, new Rectangle(321, 123, 740, 480)), changed);
            RestoredWindowPlacement second = Calculate(Save(changed, first.NormalBounds), changed);
            ClassicAssert.AreEqual(first.NormalBounds, second.NormalBounds);
        }

        [Test]
        public void DisconnectionAndReconnectionRecoverPreferredMonitor()
        {
            WindowPlacement saved = Save(Secondary, new Rectangle(2200, 100, 800, 600));
            ClassicAssert.AreEqual("PRIMARY", Calculate(saved, Primary).DisplayDeviceName);
            RestoredWindowPlacement reconnected = Calculate(saved, Primary, Secondary);
            ClassicAssert.AreEqual("SECONDARY", reconnected.DisplayDeviceName);
            ClassicAssert.AreEqual(saved.NormalBounds, reconnected.NormalBounds);
        }

        [Test]
        public void ExistingAccessibleWindowDoesNotNeedLiveRecovery()
        {
            ClassicAssert.IsTrue(WindowPlacementCalculator.IsAccessible(new Rectangle(-20, 100, 800, 600),
                new[] { Primary }));
            ClassicAssert.IsFalse(WindowPlacementCalculator.IsAccessible(new Rectangle(5000, 100, 800, 600),
                new[] { Primary }));
        }

        [Test]
        public void MinimumAndMaximumSizesAreRespectedWhenPossible()
        {
            WindowPlacement saved = Save(Primary, new Rectangle(100, 100, 300, 200));
            RestoredWindowPlacement result;
            ClassicAssert.IsTrue(WindowPlacementCalculator.TryCalculate(saved, new[] { Primary },
                new Size(400, 300), new Size(600, 500), out result));
            ClassicAssert.AreEqual(new Size(400, 300), result.NormalBounds.Size);
        }

        private static bool TryCalculate(WindowPlacement saved, IReadOnlyList<WindowDisplay> displays,
            out RestoredWindowPlacement result)
        {
            return WindowPlacementCalculator.TryCalculate(saved, displays, new Size(240, 160), Size.Empty, out result);
        }

        private static RestoredWindowPlacement Calculate(WindowPlacement saved, params WindowDisplay[] displays)
        {
            RestoredWindowPlacement result;
            ClassicAssert.IsTrue(TryCalculate(saved, displays, out result));
            return result;
        }

        private static WindowPlacement Save(WindowDisplay display, Rectangle bounds)
        {
            return new WindowPlacement
            {
                NormalBounds = bounds,
                DisplayDeviceName = display.DeviceName,
                DisplayBounds = display.Bounds,
                DisplayWorkingArea = display.WorkingArea,
                DisplayDpi = display.Dpi
            };
        }

        private static WindowDisplay Display(string name, int x, int y, int width, int height,
            int workingHeight, bool primary, int dpi)
        {
            return new WindowDisplay
            {
                DeviceName = name,
                Bounds = new Rectangle(x, y, width, height),
                WorkingArea = new Rectangle(x, y, width, workingHeight),
                IsPrimary = primary,
                Dpi = dpi
            };
        }
    }
}
