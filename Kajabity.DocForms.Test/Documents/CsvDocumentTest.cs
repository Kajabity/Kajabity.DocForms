using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CsvEditor;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Kajabity.DocForms.Test.Documents
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    [NonParallelizable]
    public class CsvDocumentTest
    {
        [Test]
        public void FirstCellCountsAsOneColumn()
        {
            var document = new CsvDocument();

            document.SetCell(0, 0, "account");

            ClassicAssert.AreEqual(1, document.ColumnCount);
            ClassicAssert.AreEqual(1, document.RowCount);
            ClassicAssert.AreEqual("account", document.GetCell(0, 0));
        }

        [Test]
        public void NonAdjacentCellCountsAllColumnsThroughItsIndex()
        {
            var document = new CsvDocument();

            document.SetCell(0, 2, "balance");

            ClassicAssert.AreEqual(3, document.ColumnCount);
            ClassicAssert.AreEqual(string.Empty, document.GetCell(0, 0));
            ClassicAssert.AreEqual(string.Empty, document.GetCell(0, 1));
            ClassicAssert.AreEqual("balance", document.GetCell(0, 2));
        }

        [Test]
        public void UpdatingCellAndAddingShorterRowPreservesMaximumColumnCount()
        {
            var document = new CsvDocument("records", new[] { new[] { "first", "second", "third" } });

            document.SetCell(0, 1, "updated");
            document.SetCell(1, 0, "short row");

            ClassicAssert.AreEqual(3, document.ColumnCount);
            ClassicAssert.AreEqual(2, document.RowCount);
            ClassicAssert.AreEqual("updated", document.GetCell(0, 1));
            ClassicAssert.AreEqual("third", document.GetCell(0, 2));
        }

        [Test]
        public void GridIncludesLastColumnAddedBySetCell()
        {
            var document = new CsvDocument();
            document.SetCell(0, 2, "balance");

            using (var grid = new DataGridUserControl { CsvDocument = document })
            {
                var listView = grid.Controls.OfType<ListView>().Single();

                ClassicAssert.AreEqual(3, listView.Columns.Count);
                ClassicAssert.AreEqual(3, listView.Items[0].SubItems.Count);
                ClassicAssert.AreEqual("balance", listView.Items[0].SubItems[2].Text);
            }
        }
    }
}
