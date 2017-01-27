//MIT, 2015-2017, ParserApprentice
 
 
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