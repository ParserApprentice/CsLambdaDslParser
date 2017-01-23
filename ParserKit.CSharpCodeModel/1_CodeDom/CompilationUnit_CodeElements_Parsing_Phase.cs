//MIT 2015-2017, ParserApprentice 
using System;
using System.Collections.Generic;
using System.Text;
using Parser.AsmInfrastructures;

namespace Parser.CodeDom
{

    partial class CompilationUnit
    {

        CodeNamespaceImportCollection nsImportCollection = new CodeNamespaceImportCollection();

        CodeNamespaceCollection namespaceCollection = new CodeNamespaceCollection();

        public const string CU_GLOBAL_NS_NAME = "@GlobalNs";


        public void AddNamespaceImport(CodeNamespaceImport nsImport)
        {
            this.nsImportCollection.AddCodeObject(nsImport);
        }
        public void AddNamespace(CodeNamespace ns)
        {

            this.namespaceCollection.AddCodeObject(ns);
        }
        public CodeNamespaceImportCollection NamespceImportCollection
        {
            get
            {
                return this.nsImportCollection;
            }
        }
        public CodeNamespaceCollection NamespaceCollection
        {
            get
            {
                return this.namespaceCollection;
            }
        }


        public CodeNamespaceImportCollection CodeNamespaceImportCollection
        {
            get
            {
                return NamespceImportCollection;

            }
        }


        public IEnumerable<CodeNamespace> NamespaceIter
        {
            get
            {
                foreach (CodeNamespace typeCollection in NamespaceCollection)
                {
                    yield return typeCollection;
                }
            }
        }

    }
}