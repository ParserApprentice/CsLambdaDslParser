//MIT, 2015-2017, ParserApprentice
 

namespace Parser.CodeDom
{   
    public class DomLocationFinder : CodeDomWalker
    {   
        
        
        int findLineNum;
        int findColumnNum;
        CodeObject lastestCandidateCodeObject;

       
        public void FindCodeObjectAtNearestLocation(CodeStatementCollection stmCollection, int findLineNum, int findColumnNum)
        {
            lastestCandidateCodeObject = null;
            this.Active = true;
            this.findLineNum = findLineNum;
            this.findColumnNum = findColumnNum; 
            WalkCodeStatementCollection(stmCollection, null);
        }
        public CodeObject LastestCandidateCodeObject
        {
            get
            {
                return lastestCandidateCodeObject;
            }
        }
        protected override bool OnPreVisitCodeExpression(CodeExpression expr, CodeDomVisitor visitor)
        {
            return CheckLocationCodeArea(expr, expr.SourceLocation);
        }
        protected override bool OnPreVisitCodeExpressionCollection(CodeExpressionCollection exprs, CodeDomVisitor visitor)
        {
            return CheckLocationCodeArea(exprs, exprs.SourceLocation);
        }
        protected override bool OnPreVisitCodeStatementCollection(CodeStatementCollection stms, CodeDomVisitor visitor)
        {
            return CheckLocationCodeArea(stms, stms.SourceLocation);
        }
        protected override bool OnPreVisitCodeStatement(CodeStatement stm, CodeDomVisitor visitor)
        {
            return CheckLocationCodeArea(stm, stm.SourceLocation);
        }
        bool CheckLocationCodeArea(CodeObject codeObject, LocationCodeArea loca)
        {
            if (!loca.IsEmpty)
            {
                if (loca.BeginLineNumber <= findLineNum && loca.BeginColumnNumber <= findColumnNum)
                {
                    if (loca.EndColumnNumber == 0 && loca.EndLineNumber == 0)
                    {
                    
                        lastestCandidateCodeObject = codeObject;
                        return true;
                    }
                    else if (loca.EndLineNumber >= findLineNum && loca.EndColumnNumber >= findColumnNum)
                    { 
                        lastestCandidateCodeObject = codeObject;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                   
                    Active = false;
                    return false;
                }
            } 
            return true;
        }
    }
}