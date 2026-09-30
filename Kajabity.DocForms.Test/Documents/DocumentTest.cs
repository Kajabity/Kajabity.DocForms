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
using NUnit.Framework.Legacy;
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
            ClassicAssert.AreEqual(null, underTest.Name);
            ClassicAssert.AreEqual(false, underTest.Modified);
        }

        [Test]
        public void TestDocumentConstructionWithName()
        {
            const String name = "test document name";
            Document underTest = new TestableDocument(name);
            ClassicAssert.AreEqual(name, underTest.Name );
            ClassicAssert.AreEqual(false, underTest.Modified);
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

            ClassicAssert.AreEqual(name, underTest.Name);
            ClassicAssert.AreEqual(true, called);

            // Changing the name doesn't count as changing the document.
            ClassicAssert.AreEqual(false, underTest.Modified);
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

            ClassicAssert.AreEqual(name, underTest.Name);
            ClassicAssert.AreEqual(true, called);

            // Changing the name doesn't count as changing the document.
            ClassicAssert.AreEqual(false, underTest.Modified);
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

            ClassicAssert.AreEqual(name, underTest.Name);
            ClassicAssert.AreEqual(false, called);

            // Changing the name doesn't count as changing the document.
            ClassicAssert.AreEqual(false, underTest.Modified);
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

            ClassicAssert.AreEqual(name, underTest.Name);
            ClassicAssert.AreEqual(false, called);

            // Changing the name doesn't count as changing the document.
            ClassicAssert.AreEqual(false, underTest.Modified);
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

            ClassicAssert.AreEqual(newValue, underTest.Modified);
            ClassicAssert.AreEqual(true, called);

            // Now set it back again.

            newValue = false;
            called = false;

            underTest.Modified = newValue;

            ClassicAssert.AreEqual(newValue, underTest.Modified);
            ClassicAssert.AreEqual(true, called);
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

            ClassicAssert.AreEqual(newValue, underTest.Modified);
            ClassicAssert.AreEqual(false, called);

            // Now try true to true.

            newValue = true;
            underTest.Modified = newValue; // Ignored.

            called = false;

            underTest.Modified = newValue;

            ClassicAssert.AreEqual(newValue, underTest.Modified);
            ClassicAssert.AreEqual(false, called);
        }
    }
}
