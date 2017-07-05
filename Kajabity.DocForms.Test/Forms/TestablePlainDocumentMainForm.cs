using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlainTextEditor;
using Kajabity.DocForms.Forms;
using System.Windows.Forms;

namespace Kajabity.DocForms.Test.Forms
{
    public class TestablePlainDocumentMainForm : PlainTextEditorMainForm
    {
        string _filename;

        public TestablePlainDocumentMainForm(string filename)
        {
            _filename = filename;
        }

        protected override IDocumentSelector GetOpenDocumentSelector()
        {

            return new TestableDocumentSelector(_filename);
        }

        protected override IDocumentSelector GetSaveAsDocumentSelector()
        {

            return new TestableDocumentSelector(_filename);
        }

        public class TestableDocumentSelector : IDocumentSelector
        {
            string _filename;

            public TestableDocumentSelector(string filename)
            {
                _filename = filename;
            }

            public string FileName
            {
                get
                {
                    return _filename;
                }
            }

            public DialogResult ShowDialog(IWin32Window owner)
            {
                return DialogResult.OK;
            }
        }
    }
}
