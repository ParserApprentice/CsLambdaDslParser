//MIT, 2015-2017, ParserApprentice
 

namespace Parser.CodeDom
{

    public class CodeDefineTypeDeclaration : CodeTypeDeclaration
    {
        bool passSemanticCheck = false;
        CodeDefineFieldExpressionCollection defineFieldExpressionCollection;
        public CodeDefineTypeDeclaration(CompilationUnit cu,
            CodeDefineFieldExpressionCollection defineFieldExpressionCollection,
            string typenameSpace, string typeName)
            : base(cu, CodeObjectKind.DefineClass, typenameSpace, typeName)
        {

            this.defineFieldExpressionCollection = defineFieldExpressionCollection;

        }
        public bool PassSemanticCheck
        {
            get
            {
                return passSemanticCheck;
            }
            set
            {
                passSemanticCheck = value;
            }
        }
        public CodeDefineFieldExpressionCollection DefineFieldCollection
        {
            get
            {
                return defineFieldExpressionCollection;
            }

        }

    }
}