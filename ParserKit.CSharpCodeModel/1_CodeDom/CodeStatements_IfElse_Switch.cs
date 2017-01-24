//MIT, 2015-2017, ParserApprentice
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;


namespace Parser.CodeDom
{
    public class CodeIfBlock : CodeObject
    {
        CodeExpression conditionExpression;
        CodeStatement body;
        public CodeIfBlock(CodeExpression conditionExpression, CodeStatement body)
        {
            this.ConditionExpression = conditionExpression;
            this.Body = body;
        }
        public CodeIfBlock(CodeExpression conditionExpression)
        {
            this.ConditionExpression = conditionExpression;

        }
        public CodeStatement Body
        {
            get
            {
                return this.body;
            }
            set
            {
                this.body = value;
                if (value != null)
                {
                    AcceptChild(value, CodeObjectRoles.CodeIfBlock_Body);
                }
            }
        }

        public CodeExpression ConditionExpression
        {
            get
            {
                return conditionExpression;
            }
            set
            {
                this.conditionExpression = value;
                if (value != null)
                {
                    AcceptChild(value, CodeObjectRoles.CodeIfBlock_ConditionExpression);
                }
            }
        }
        public override CodeObjectKind Kind
        {
            get
            {
                return CodeObjectKind.ConditionBlock;
            }
        }
    }


    public class CodeElseBlock : CodeObject
    {
        CodeStatement body;
        public CodeElseBlock(CodeStatement body)
        {
            this.Body = body;
        }
        public CodeStatement Body
        {
            get
            {
                return this.body;
            }
            set
            {
                this.body = value;
                if (value != null)
                {
                    AcceptChild(value, CodeObjectRoles.CodeElseBlock_Body);
                }
            }
        }
        public override CodeObjectKind Kind
        {
            get { return CodeObjectKind.ElseBlock; }
        }
    }


    public class CodeIfElseStatement : CodeStatement
    {

        List<CodeIfBlock> allConditions = new List<CodeIfBlock>();
        CodeElseBlock elseBlock;
        CodeIfBlock ifTrueBlock;

        public CodeIfElseStatement()
        {

        }

        public void AddBlock(CodeElseBlock elseBlock)
        {
            this.elseBlock = elseBlock;
            AcceptChild(elseBlock, CodeObjectRoles.CodeElseConditionBlock);
        }
        public void AddBlock(CodeIfBlock conditionBlock)
        {
            allConditions.Add(conditionBlock);
            AcceptChild(conditionBlock, CodeObjectRoles.CodeIfConditionBlock);
            ifTrueBlock = conditionBlock;
        }
        public CodeIfBlock IfBlock { get { return ifTrueBlock; } }
        public CodeElseBlock ElseBlock
        {
            get
            {
                return elseBlock;
            }
        }
        public int ConditionsCount
        {
            get
            {
                return allConditions.Count;
            }
        }
        public override CodeReplacingResult ReplaceChildExpression(CodeExpression childExpr)
        {
            return base.ReplaceChildExpression(childExpr);
        }
        public IEnumerable<CodeIfBlock> GetConditionBlocksIter()
        {
            int j = allConditions.Count;
            for (int i = 0; i < j; i++)
            {
                yield return allConditions[i];
            }
        }
        public sealed override CodeStatementType StatementType
        {
            get
            {
                return CodeStatementType.CodeIfElseStatement;
            }
        }
    }


    public class CodeSwitchStatement : CodeStatement
    {

        CodeExpression switchExpr;
        List<CodeSwitchBlock> codeSwitchBlocks = new List<CodeSwitchBlock>();
        CodeSwitchDefaultBlock switchDefaultBlock;

        public CodeSwitchStatement()
        {

        }

        public CodeExpression SwitchExpression
        {
            get
            {
                return switchExpr;
            }
            set
            {
                switchExpr = value;
                if (value != null)
                {
                    AcceptChild(value, CodeObjectRoles.CodeSwitchStatement_SwitchExpr);
                }
            }
        }
        public void AddBlock(CodeSwitchBlock labelBlock)
        {
            codeSwitchBlocks.Add(labelBlock);
            if (labelBlock is CodeSwitchDefaultBlock)
            {
                switchDefaultBlock = (CodeSwitchDefaultBlock)labelBlock;
                AcceptChild(labelBlock, CodeObjectRoles.CodeSwitchDefaultBlock);
            }
            else
            {
                AcceptChild(labelBlock, CodeObjectRoles.CodeSwitchLabelBlock);
            }

        }
        public override CodeReplacingResult ReplaceChildExpression(CodeExpression childExpr)
        {
            //replace 
            CodeObjectRoles objRole = childExpr.CodeObjectRole;
            switch (objRole)
            {
                case CodeObjectRoles.CodeSwitchStatement_SwitchExpr:
                    {

                        SwitchExpression = childExpr;
                        return CodeReplacingResult.Ok;
                    }
                default:
                    {
                        return base.ReplaceChildExpression(childExpr);
                    }
            }
        }
        public bool HasDefaultLabel
        {
            get
            {
                return switchDefaultBlock != null;
            }
        }

        public IEnumerable<CodeSwitchBlock> GetSwitchBlockIter()
        {

            foreach (CodeSwitchBlock labeledBlock in codeSwitchBlocks)
            {
                yield return labeledBlock;
            }
        }


        public IEnumerable<CodeExpression> GetAllLabelExpressionIter()
        {

            int j = codeSwitchBlocks.Count;
            for (int i = 0; i < j; i++)
            {
                if (!(codeSwitchBlocks[i] is CodeSwitchDefaultBlock))
                {

                    yield return codeSwitchBlocks[i].Label;
                }
            }
        }

        public IEnumerable<CodeSwitchBlock> GetAllSwitchBlockIter()
        {
            int j = codeSwitchBlocks.Count;
            for (int i = 0; i < j; i++)
            {
                yield return codeSwitchBlocks[i];
            }

        }
        public List<CodeSwitchBlock> GetAllSwitchBlocks()
        {
            return this.codeSwitchBlocks;
        }
        public sealed override CodeStatementType StatementType
        {
            get
            {
                return CodeStatementType.CodeSwitchStatement;
            }
        }

        public int AllBlockCount
        {
            get
            {
                return codeSwitchBlocks.Count;
            }
        }
    }
    public class CodeSwitchLabel
    {
        public CodeSwitchLabel(CodeExpression labelExpression)
        {
            this.LabelExpression = LabelExpression;

        }
        public CodeExpression LabelExpression
        {
            get;
            set;
        }
    }

    public class CodeSwitchBlock : CodeObject
    {
        CodeExpression label;
        CodeStatement body;
        List<CodeExpression> moreLabels;
        public CodeSwitchBlock(CodeStatement body)
        {
            this.Body = body;
        }
        public CodeSwitchBlock(CodeExpression label, CodeStatement body)
        {
            this.label = label;
            if (label != null)
            {
                AcceptChild(label, CodeObjectRoles.CodeSwitchLabelBlock_LabelExpression);
            }
            this.Body = body;
        }
        public CodeStatement Body
        {
            get
            {
                return this.body;
            }
            private set
            {
                this.body = value;
                if (value != null)
                {
                    AcceptChild(value, CodeObjectRoles.CodeSwitchLabelBlock_Body);
                }
            }
        }
        public CodeExpression Label
        {
            get
            {
                return label;
            }
        }
        public override CodeObjectKind Kind
        {
            get
            {
                return CodeObjectKind.SwitchLabelBlock;
            }
        }
        public void AddSwitchLabel(CodeExpression label)
        {
            if (this.label == null)
            {

                this.label = label;
            }
            else
            {
                if (moreLabels == null)
                {
                    moreLabels = new List<CodeExpression>();
                    moreLabels.Add(this.label);
                }
                moreLabels.Add(label);

            }
        }
    }

    public class CodeSwitchDefaultBlock : CodeSwitchBlock
    {
        public CodeSwitchDefaultBlock(CodeStatement body)
            : base(null, body)
        {

        }
        public override CodeObjectKind Kind
        {
            get
            {
                return CodeObjectKind.DefaultLabelBlock;
            }
        }
    }

}