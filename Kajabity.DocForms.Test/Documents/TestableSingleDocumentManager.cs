using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kajabity.DocForms.Documents;

namespace Kajabity.DocForms.Test.Documents
{
    public class TestableSingleDocumentManager : SingleDocumentManager<TestableDocument>
    {
        public override void NewDocument()
        {
            Document = new TestableDocument();
            base.NewDocument();
        }

        public override void Load(string filename)
        {
            Document = new TestableDocument( filename );
            base.Load(filename);
        }

        public override void Save(string filename)
        {
            base.Save(filename);
        }
    }
}
