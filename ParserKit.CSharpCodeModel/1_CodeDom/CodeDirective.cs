//MIT, 2015-2017, ParserApprentice
using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.CodeDom
{

    public enum CodeDirectiveType
    {
        Unknown,
        DefineDirective,
        ConditionIfBlock,
        ConditionElseIfBlock,
        ConditionElseBlock,
        ConditionEndIfBlock
    }

    public class CodeDirective : CodeObject
    {
        protected CodeDirective()
        {
        }
        public override CodeObjectKind Kind
        {
            get { return CodeObjectKind.Directive; }
        }
        public virtual CodeDirectiveType DirectiveName
        {
            get
            {
                return CodeDirectiveType.Unknown;
            }
        }
    }
    public class CodeDefineDirective : CodeDirective
    {
        public string defineString;
        public CodeDefineDirective(string defineString)
        {
            this.defineString = defineString;
        }
        public override CodeDirectiveType DirectiveName
        {
            get
            {
                return CodeDirectiveType.DefineDirective;
            }
        }
    }


    public class CodeCompilationConditionIfBlock : CodeDirective
    {
        public string ifCondition;
        public List<CodeCompilationConditionElseIfBlock> elseIfBlocks;
        public CodeCompilationConditionElseBlock elseBlock;
        public CodeCompilationConditionEndIf endIf;


        public CodeCompilationConditionIfBlock(string ifCondition)
        {
            this.ifCondition = ifCondition;
        }

        public override CodeDirectiveType DirectiveName
        {
            get
            {
                return CodeDirectiveType.ConditionIfBlock;
            }
        }
    }



    public class CodeCompilationConditionElseIfBlock : CodeDirective
    {
        public string elseifCondition;

        public CodeCompilationConditionElseIfBlock(string directiveString)
        {
            this.elseifCondition = directiveString;
        }
        public override CodeDirectiveType DirectiveName
        {
            get
            {
                return CodeDirectiveType.ConditionElseIfBlock;
            }
        } 
    }

    public class CodeCompilationConditionElseBlock : CodeDirective
    {
        public CodeCompilationConditionElseBlock()
        {
        }
        public override CodeDirectiveType DirectiveName
        {
            get
            {
                return CodeDirectiveType.ConditionElseBlock;
            }
        }
    }

    public class CodeCompilationConditionEndIf : CodeDirective
    {

        public override CodeDirectiveType DirectiveName
        {
            get
            {
                return CodeDirectiveType.ConditionEndIfBlock;
            }
        }
    }
}