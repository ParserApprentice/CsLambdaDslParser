//MIT, 2015-2017, ParserApprentice 
 
namespace Parser.CodeDom
{
    public class CodeDefineStatement : CodeStatement
    {

        CodeDefineFieldExpressionCollection defineFieldExpressionCollection;

        public CodeDefineStatement(CodeDefineFieldExpressionCollection defineFieldExpressionCollection)
        {

            this.defineFieldExpressionCollection = defineFieldExpressionCollection;
        }
        public string DefineName
        {
            get
            {
                return defineFieldExpressionCollection.DefineCollectionName;
            }
            set
            {
                defineFieldExpressionCollection.DefineCollectionName = value;
            }
        }
        public CodeDefineFieldExpressionCollection DefineFieldExpressionCollection
        {
            get
            {
                return defineFieldExpressionCollection;
            }
        }
        public override CodeStatementType StatementType
        {
            get { return CodeStatementType.CodeDefineStatement; }
        }

    }

}