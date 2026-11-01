using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Kajabity.DocForms.Documents;
using Kajabity.DocForms.Forms;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Kajabity.DocForms.Test.Forms
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    [NonParallelizable]
    public class RecentFileHistoryTest
    {
        private SingleDocumentForm<TestDocument> _form;

        [SetUp]
        public void SetUp()
        {
            _form = new SingleDocumentForm<TestDocument>(new TestManager());
        }

        [TearDown]
        public void TearDown()
        {
            _form.Dispose();
        }

        [Test]
        public void ZeroLimitDoesNotAddAFile()
        {
            _form.MaximumRecentDocumentCount = 0;

            _form.AddFileHistory("first.txt");

            CollectionAssert.IsEmpty(RecentFiles());
        }

        [Test]
        public void ZeroLimitClearsExistingHistory()
        {
            _form.AddFileHistory("first.txt");
            _form.AddFileHistory("second.txt");

            _form.MaximumRecentDocumentCount = 0;

            CollectionAssert.IsEmpty(RecentFiles());
        }

        [Test]
        public void OneFileLimitKeepsOnlyTheMostRecentFile()
        {
            _form.MaximumRecentDocumentCount = 1;

            _form.AddFileHistory("first.txt");
            _form.AddFileHistory("second.txt");

            CollectionAssert.AreEqual(new[] { "second.txt" }, RecentFiles());
        }

        [Test]
        public void ReducingLimitRemovesOldestFilesImmediately()
        {
            _form.AddFileHistory("first.txt");
            _form.AddFileHistory("second.txt");
            _form.AddFileHistory("third.txt");
            _form.AddFileHistory("fourth.txt");

            _form.MaximumRecentDocumentCount = 2;

            CollectionAssert.AreEqual(new[] { "fourth.txt", "third.txt" }, RecentFiles());

            _form.AddFileHistory("fifth.txt");

            CollectionAssert.AreEqual(new[] { "fifth.txt", "fourth.txt" }, RecentFiles());
        }

        [Test]
        public void ExistingFileMovesToFrontWithoutDuplication()
        {
            _form.MaximumRecentDocumentCount = 3;
            _form.AddFileHistory("first.txt");
            _form.AddFileHistory("second.txt");
            _form.AddFileHistory("third.txt");

            _form.AddFileHistory("first.txt");

            CollectionAssert.AreEqual(new[] { "first.txt", "third.txt", "second.txt" }, RecentFiles());
        }

        [Test]
        public void NegativeLimitIsRejectedWithoutChangingHistory()
        {
            _form.AddFileHistory("first.txt");

            ClassicAssert.Throws<ArgumentOutOfRangeException>(() => _form.MaximumRecentDocumentCount = -1);

            ClassicAssert.AreEqual(20, _form.MaximumRecentDocumentCount);
            CollectionAssert.AreEqual(new[] { "first.txt" }, RecentFiles());
        }

        private string[] RecentFiles()
        {
            var field = typeof(SingleDocumentForm<TestDocument>).GetField(
                "_recentFiles", BindingFlags.Instance | BindingFlags.NonPublic);
            ClassicAssert.IsNotNull(field);
            return ((List<string>)field.GetValue(_form)).ToArray();
        }

        private sealed class TestDocument : Document
        {
        }

        private sealed class TestManager : SingleDocumentManager<TestDocument>
        {
        }
    }
}
