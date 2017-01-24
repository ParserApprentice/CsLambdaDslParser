//MIT, 2015-2017, ParserApprentice
using System;
using System.Collections.Generic;
using System.Text;


using Parser.AsmInfrastructures;

namespace Parser.CodeDom
{

    public sealed partial class CompilationUnit : CodeObject
    {
        string filename;
        public CompilationUnit(string filename)
        {

            this.filename = filename;
        }
        public override CodeObjectKind Kind
        {
            get { return CodeObjectKind.Unknown; }
        }

        public string Filename
        {
            get
            {
                return this.filename;
            }
        }


    }
}
