/*
 * Copyright 2009-17 Williams Technologies Limited.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 * Kajbity is a trademark of Williams Technologies Limited.
 *
 * http://www.kajabity.com
 */

using Kajabity.DocForms.Documents;
using NUnit.Framework;
using System;

namespace Kajabity.DocForms.Test.Documents
{
    [TestFixture]
    public class DocumentTest
    {
        [Test]
        public void TestDocumentConstructionWithoutName()
        {
            Document underTest = new TestableDocument();
            Assert.AreEqual(null, underTest.Name);
            Assert.AreEqual(false, underTest.Modified);
        }

        [Test]
        public void TestDocumentConstructionWithName()
        {
            const String name = "test document name";
            Document underTest = new TestableDocument(name);
            Assert.AreEqual(name, underTest.Name );
            Assert.AreEqual(false, underTest.Modified);
        }

        [Test]
        public void TestDocumentSetNameEvent()
        {
            const String name = "test document name";
            Document underTest = new TestableDocument();

            bool called = false;
            underTest.StatusChanged += delegate (object sender, EventArgs e)
            {
                called = true;
            };

            underTest.Name = name;

            Assert.AreEqual(name, underTest.Name);
            Assert.AreEqual(true, called);

            // Changing the name doesn't count as changing the document.
            Assert.AreEqual(false, underTest.Modified);
        }

        [Test]
        public void TestDocumentChangeNameEvent()
        {
            const String name = "test document name";
            Document underTest = new TestableDocument("Original name");

            bool called = false;
            underTest.StatusChanged += delegate (object sender, EventArgs e)
            {
                called = true;
            };

            underTest.Name = name;

            Assert.AreEqual(name, underTest.Name);
            Assert.AreEqual(true, called);

            // Changing the name doesn't count as changing the document.
            Assert.AreEqual(false, underTest.Modified);
        }

        [Test]
        public void TestDocumentSetSameNameEvent()
        {
            const String name = null;
            Document underTest = new TestableDocument();

            bool called = false;
            underTest.StatusChanged += delegate (object sender, EventArgs e)
            {
                called = true;
            };

            underTest.Name = name;

            Assert.AreEqual(name, underTest.Name);
            Assert.AreEqual(false, called);

            // Changing the name doesn't count as changing the document.
            Assert.AreEqual(false, underTest.Modified);
        }

        [Test]
        public void TestDocumentChangeSameNameEvent()
        {
            const String name = "test document name";
            Document underTest = new TestableDocument(name);

            bool called = false;
            underTest.StatusChanged += delegate (object sender, EventArgs e)
            {
                called = true;
            };

            underTest.Name = name;

            Assert.AreEqual(name, underTest.Name);
            Assert.AreEqual(false, called);

            // Changing the name doesn't count as changing the document.
            Assert.AreEqual(false, underTest.Modified);
        }

        [Test]
        public void TestDocumentChangeModifiedEvent()
        {
            bool newValue = true;
            Document underTest = new TestableDocument();

            bool called = false;
            underTest.StatusChanged += delegate (object sender, EventArgs e)
            {
                called = true;
            };

            underTest.Modified = newValue;

            Assert.AreEqual(newValue, underTest.Modified);
            Assert.AreEqual(true, called);

            // Now set it back again.

            newValue = false;
            called = false;

            underTest.Modified = newValue;

            Assert.AreEqual(newValue, underTest.Modified);
            Assert.AreEqual(true, called);
        }

        [Test]
        public void TestDocumentSameModifiedNoEvent()
        {
            bool newValue = false;
            Document underTest = new TestableDocument();

            bool called = false;
            underTest.StatusChanged += delegate (object sender, EventArgs e)
            {
                called = true;
            };

            underTest.Modified = newValue;

            Assert.AreEqual(newValue, underTest.Modified);
            Assert.AreEqual(false, called);

            // Now try true to true.

            newValue = true;
            underTest.Modified = newValue; // Ignored.

            called = false;

            underTest.Modified = newValue;

            Assert.AreEqual(newValue, underTest.Modified);
            Assert.AreEqual(false, called);
        }
    }
}
