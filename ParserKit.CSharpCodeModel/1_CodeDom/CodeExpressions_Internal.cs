//MIT 2015-2017, ParserApprentice 
using System;
using System.Collections.Generic;
using System.Text;
using Parser.AsmInfrastructures;

namespace Parser.CodeDom
{
    /// <summary>
    /// for 'this' assumption only, (user dose not explcit 'this', so
    /// when resolved this expression may changed     
    /// </summary>
    public class CodeImplicitThisReferenceExpression : CodeThisReferenceExpression
    {
        //eg. xxx()

        public CodeImplicitThisReferenceExpression()
        {

        }

    }




}