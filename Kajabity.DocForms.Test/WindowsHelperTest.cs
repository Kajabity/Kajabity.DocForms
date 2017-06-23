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

using NUnit.Framework;

namespace Kajabity.DocForms.Test
{
    [TestFixture]
    public class WindowsHelperTest
    {
        [Test]
        public void TestGetShortPathShortStrings()
        {
            const int length = 50;
            string[] short_strings =
            {
                null,
                "",
                "a",
                "abcdefg",
                "temp.txt",
                "D:\\.gitignore",
                "D:\\appveyor.yml",
                "D:\\Kajabity.DocForms\\bin",
                "D:\\Kajabity.DocForms\\Documents",
                "D:\\Kajabity.DocForms\\Forms",
                "D:\\Kajabity.DocForms\\Kajabity.DocForms.csproj"
            };

            foreach (string s in short_strings)
            {
                string result = WindowsHelper.GetShortPath(s, length);

                Assert.AreEqual(s, result);
            }
        }

        [Test]
        public void TestGetShortPathLongStrings()
        {
            const int length = 50;
            string[] short_strings =
            {
                "D:\\Kajabity.DocForms\\Kajabity.DocForms.csproj.use",
                "D:\\Kajabity.DocForms\\Kajabity.DocForms.csproj.user",
                "D:\\Kajabity.DocForms\\obj\\Debug\\Kajabity.DocForms.csprojResolveAssemblyReference.cache",
                "D:\\Kajabity.DocForms.Test\\obj\\Debug\\TemporaryGeneratedFile_E7A71F73-0F8D-4B9B-B56E-8E70B10BC5D3.cs",
                "D:\\packages\\Microsoft.NETCore.Portable.Compatibility.1.0.1\\lib\\netstandard1.0\\System.ComponentModel.DataAnnotations.dll",
                "D:\\packages\\System.Runtime.InteropServices.RuntimeInformation.4.0.0\\runtimes\\win\\lib\\netstandard1.1\\System.Runtime.InteropServices.RuntimeInformation.dll"
            };

            foreach (string s in short_strings)
            {
                string result = WindowsHelper.GetShortPath(s, length);

                Assert.AreEqual(length-1, result.Length);
            }
        }
    }
}
