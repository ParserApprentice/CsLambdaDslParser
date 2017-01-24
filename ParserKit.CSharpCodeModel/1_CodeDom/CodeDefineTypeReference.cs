//MIT, 2015-2017, ParserApprentice
using System;
using System.Collections.Generic;
using System.Text;
using System.IO; 
using Parser.CodeDom;
using Parser.AsmInfrastructures; 
 
namespace Parser.CodeDom
{
    public class CodeDefineTypeReference : CodeTypeReference
    {

        CodeDefineFieldExpressionCollection defFieldExprCollection; 
        public CodeDefineTypeReference(CodeDefineFieldExpressionCollection defFieldExprCollection) 
        {            
            this.defFieldExprCollection = defFieldExprCollection;
        }
        public CodeDefineFieldExpressionCollection DefineFieldExpCollection
        {
            get
            {
                return defFieldExprCollection;
            }
        }

        
        public void SetDefineTypeDecl(CodeDefineTypeDeclaration defineTypeDecl)
        {
            base.LateSetCodeTypeDeclation(defineTypeDecl);             
        }

    }


}