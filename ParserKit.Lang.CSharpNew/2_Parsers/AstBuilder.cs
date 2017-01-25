//MIT, 2015-2017, ParserApprentice
//#define RETURN_NOW

using System;
using Parser.CodeDom;
using Parser.ParserKit;
using System.Collections.Generic;

namespace Parser.MyCs
{
    public static class ParseNodeHolderFactory
    {
        public static CsParseNodeHolder CreateCsParseNodeHolderForAst()
        {
            CsParseNodeHolder holder = new CsParseNodeHolder();
            holder.ClassWalker = new ClassBuilder();
            holder.NamespaceWalker = new NamespaceBuilder();
            holder.StatementBuilder = new StatementBuilder();
            holder.ArrayTypeWalker = new ArrayTypeBuilder();
            holder.ExpressionWalker = new ExpressionBuilder();
            holder.TypeBuilder = new TypeBuilder();
            return holder;
        }
    }

//    static class ParseNodeWalker
//    {
//        static void RecursiveInvoke(SeqReduction seqRd, CsParseNodeHolder csParseNodeHolder)
//        {
//            if (seqRd._prevNode != null)
//            {
//                RecursiveInvoke(seqRd._prevNode, csParseNodeHolder);
//                seqRd._symbolSeq.NotifyReductionEvent(csParseNodeHolder);
//            }
//            else
//            {

//#if DEBUG
//                csParseNodeHolder.CurrentContextSequence = seqRd._symbolSeq;
//#endif
//                SymbolSequence sq = seqRd._symbolSeq;
//                sq.NotifyReductionEvent(csParseNodeHolder);

//                //if (sq.HasSomeUserExpectedSymbolMonitor)
//                //{

//                //    ReductionMonitor[] rdMonitors = sq.GetAllUserExpectedSymbolReductionMonitors();
//                //    int i = 0;
//                //    NonTerminalParseNode nt = (NonTerminalParseNode)csParseNodeHolder.ContextOwner;
//                //    ParseNode c = nt.FirstChild;
//                //    int childCount = csParseNodeHolder.ContextChildCount;
//                //    int currentAstIndex = csParseNodeHolder.CurrentAstIndex - childCount;

//                //    switch (childCount)
//                //    {

//                //        case 0: break;
//                //        case 1:
//                //            {
//                //                ReductionMonitor m = rdMonitors[0];
//                //                if (m != null)
//                //                {
//                //                    csParseNodeHolder.CurrentChildAstIndex = currentAstIndex;
//                //                    m.NotifyReduction(csParseNodeHolder, c);
//                //                }
//                //            }
//                //            break;
//                //        case 2:
//                //            {
//                //                ReductionMonitor m = rdMonitors[0];
//                //                if (m != null)
//                //                {
//                //                    csParseNodeHolder.CurrentChildAstIndex = currentAstIndex;
//                //                    m.NotifyReduction(csParseNodeHolder, c);
//                //                }
//                //            }
//                //            {
//                //                c = c.NextSibling;
//                //                ReductionMonitor m = rdMonitors[1];
//                //                if (m != null)
//                //                {
//                //                    csParseNodeHolder.CurrentChildAstIndex = currentAstIndex + 1;
//                //                    m.NotifyReduction(csParseNodeHolder, c);
//                //                }
//                //            }
//                //            break;
//                //        case 3:
//                //            {
//                //                ReductionMonitor m = rdMonitors[0];
//                //                if (m != null)
//                //                {
//                //                    csParseNodeHolder.CurrentChildAstIndex = currentAstIndex;
//                //                    m.NotifyReduction(csParseNodeHolder, c);
//                //                }
//                //            }
//                //            {
//                //                c = c.NextSibling;
//                //                ReductionMonitor m = rdMonitors[1];
//                //                if (m != null)
//                //                {
//                //                    csParseNodeHolder.CurrentChildAstIndex = currentAstIndex + 1;
//                //                    m.NotifyReduction(csParseNodeHolder, c);
//                //                }
//                //            }
//                //            {
//                //                c = c.NextSibling;
//                //                ReductionMonitor m = rdMonitors[2];
//                //                if (m != null)
//                //                {
//                //                    csParseNodeHolder.CurrentChildAstIndex = currentAstIndex + 2;
//                //                    m.NotifyReduction(csParseNodeHolder, c);
//                //                }
//                //            }
//                //            break;
//                //        case 4:
//                //            {
//                //                ReductionMonitor m = rdMonitors[0];
//                //                if (m != null)
//                //                {
//                //                    csParseNodeHolder.CurrentChildAstIndex = currentAstIndex;
//                //                    m.NotifyReduction(csParseNodeHolder, c);
//                //                }
//                //            }
//                //            {
//                //                c = c.NextSibling;
//                //                ReductionMonitor m = rdMonitors[1];
//                //                if (m != null)
//                //                {
//                //                    csParseNodeHolder.CurrentChildAstIndex = currentAstIndex + 1;
//                //                    m.NotifyReduction(csParseNodeHolder, c);
//                //                }
//                //            }
//                //            {
//                //                c = c.NextSibling;
//                //                ReductionMonitor m = rdMonitors[2];
//                //                if (m != null)
//                //                {
//                //                    csParseNodeHolder.CurrentChildAstIndex = currentAstIndex + 2;
//                //                    m.NotifyReduction(csParseNodeHolder, c);
//                //                }
//                //            }
//                //            {
//                //                c = c.NextSibling;
//                //                ReductionMonitor m = rdMonitors[3];
//                //                if (m != null)
//                //                {
//                //                    csParseNodeHolder.CurrentChildAstIndex = currentAstIndex + 3;
//                //                    m.NotifyReduction(csParseNodeHolder, c);
//                //                }
//                //            }
//                //            break;
//                //        case 5:
//                //            {
//                //                ReductionMonitor m = rdMonitors[0];
//                //                if (m != null)
//                //                {
//                //                    csParseNodeHolder.CurrentChildAstIndex = currentAstIndex;
//                //                    m.NotifyReduction(csParseNodeHolder, c);
//                //                }
//                //            }
//                //            {
//                //                c = c.NextSibling;
//                //                ReductionMonitor m = rdMonitors[1];
//                //                if (m != null)
//                //                {
//                //                    csParseNodeHolder.CurrentChildAstIndex = currentAstIndex + 1;
//                //                    m.NotifyReduction(csParseNodeHolder, c);
//                //                }
//                //            }
//                //            {
//                //                c = c.NextSibling;
//                //                ReductionMonitor m = rdMonitors[2];
//                //                if (m != null)
//                //                {
//                //                    csParseNodeHolder.CurrentChildAstIndex = currentAstIndex + 2;
//                //                    m.NotifyReduction(csParseNodeHolder, c);
//                //                }
//                //            }
//                //            {
//                //                c = c.NextSibling;
//                //                ReductionMonitor m = rdMonitors[3];
//                //                if (m != null)
//                //                {
//                //                    csParseNodeHolder.CurrentChildAstIndex = currentAstIndex + 3;
//                //                    m.NotifyReduction(csParseNodeHolder, c);
//                //                }
//                //            }
//                //            {
//                //                c = c.NextSibling;
//                //                ReductionMonitor m = rdMonitors[4];
//                //                if (m != null)
//                //                {
//                //                    csParseNodeHolder.CurrentChildAstIndex = currentAstIndex + 4;
//                //                    m.NotifyReduction(csParseNodeHolder, c);
//                //                }
//                //            }
//                //            break;
//                //        case 6:
//                //            {
//                //                ReductionMonitor m = rdMonitors[0];
//                //                if (m != null)
//                //                {
//                //                    csParseNodeHolder.CurrentChildAstIndex = currentAstIndex;
//                //                    m.NotifyReduction(csParseNodeHolder, c);
//                //                }
//                //            }
//                //            {
//                //                c = c.NextSibling;
//                //                ReductionMonitor m = rdMonitors[1];
//                //                if (m != null)
//                //                {
//                //                    csParseNodeHolder.CurrentChildAstIndex = currentAstIndex + 1;
//                //                    m.NotifyReduction(csParseNodeHolder, c);
//                //                }
//                //            }
//                //            {
//                //                c = c.NextSibling;
//                //                ReductionMonitor m = rdMonitors[2];
//                //                if (m != null)
//                //                {
//                //                    csParseNodeHolder.CurrentChildAstIndex = currentAstIndex + 2;
//                //                    m.NotifyReduction(csParseNodeHolder, c);
//                //                }
//                //            }
//                //            {
//                //                c = c.NextSibling;
//                //                ReductionMonitor m = rdMonitors[3];
//                //                if (m != null)
//                //                {
//                //                    csParseNodeHolder.CurrentChildAstIndex = currentAstIndex + 3;
//                //                    m.NotifyReduction(csParseNodeHolder, c);
//                //                }
//                //            }
//                //            {
//                //                c = c.NextSibling;
//                //                ReductionMonitor m = rdMonitors[4];
//                //                if (m != null)
//                //                {
//                //                    csParseNodeHolder.CurrentChildAstIndex = currentAstIndex + 4;
//                //                    m.NotifyReduction(csParseNodeHolder, c);
//                //                }
//                //            }
//                //            {
//                //                c = c.NextSibling;
//                //                ReductionMonitor m = rdMonitors[5];
//                //                if (m != null)
//                //                {
//                //                    csParseNodeHolder.CurrentChildAstIndex = currentAstIndex + 5;
//                //                    m.NotifyReduction(csParseNodeHolder, c);
//                //                }
//                //            }
//                //            break;
//                //        case 7:
//                //            {
//                //                ReductionMonitor m = rdMonitors[0];
//                //                if (m != null)
//                //                {
//                //                    csParseNodeHolder.CurrentChildAstIndex = currentAstIndex;
//                //                    m.NotifyReduction(csParseNodeHolder, c);
//                //                }
//                //            }
//                //            {
//                //                c = c.NextSibling;
//                //                ReductionMonitor m = rdMonitors[1];
//                //                if (m != null)
//                //                {
//                //                    csParseNodeHolder.CurrentChildAstIndex = currentAstIndex + 1;
//                //                    m.NotifyReduction(csParseNodeHolder, c);
//                //                }
//                //            }
//                //            {
//                //                c = c.NextSibling;
//                //                ReductionMonitor m = rdMonitors[2];
//                //                if (m != null)
//                //                {
//                //                    csParseNodeHolder.CurrentChildAstIndex = currentAstIndex + 2;
//                //                    m.NotifyReduction(csParseNodeHolder, c);
//                //                }
//                //            }
//                //            {
//                //                c = c.NextSibling;
//                //                ReductionMonitor m = rdMonitors[3];
//                //                if (m != null)
//                //                {
//                //                    csParseNodeHolder.CurrentChildAstIndex = currentAstIndex + 3;
//                //                    m.NotifyReduction(csParseNodeHolder, c);
//                //                }
//                //            }
//                //            {
//                //                c = c.NextSibling;
//                //                ReductionMonitor m = rdMonitors[4];
//                //                if (m != null)
//                //                {
//                //                    csParseNodeHolder.CurrentChildAstIndex = currentAstIndex + 4;
//                //                    m.NotifyReduction(csParseNodeHolder, c);
//                //                }
//                //            }
//                //            {
//                //                c = c.NextSibling;
//                //                ReductionMonitor m = rdMonitors[5];
//                //                if (m != null)
//                //                {
//                //                    csParseNodeHolder.CurrentChildAstIndex = currentAstIndex + 5;
//                //                    m.NotifyReduction(csParseNodeHolder, c);
//                //                }
//                //            }
//                //            {
//                //                c = c.NextSibling;
//                //                ReductionMonitor m = rdMonitors[6];
//                //                if (m != null)
//                //                {
//                //                    csParseNodeHolder.CurrentChildAstIndex = currentAstIndex + 6;
//                //                    m.NotifyReduction(csParseNodeHolder, c);
//                //                }
//                //            }
//                //            break;
//                //        default:
//                //            while (c != null)
//                //            {
//                //                ReductionMonitor m = rdMonitors[i];
//                //                if (m != null)
//                //                {
//                //                    csParseNodeHolder.CurrentChildAstIndex = currentAstIndex;
//                //                    m.NotifyReduction(csParseNodeHolder, c);
//                //                }
//                //                //fill component 
//                //                c = c.NextSibling;
//                //                i++;
//                //                currentAstIndex++;
//                //            }
//                //            break;
//                //    }

//                //}
//            }
//        }


//        //public static void WalkNodes(ParseNode node, CsParseNodeHolder csParseNodeHolder)
//        //{

//        //    if (node.IsTerminalNode)
//        //    {
//        //        Token tk = (Token)node;
//        //        SeqReduction seqRd = node.LatestReduction;
//        //        if (seqRd != null)
//        //        {
//        //            csParseNodeHolder.ContextOwner = tk;
//        //            RecursiveInvoke(seqRd, csParseNodeHolder);
//        //        }
//        //        else
//        //        {

//        //        }

//        //    }
//        //    else
//        //    {

//        //        NonTerminalParseNode nt = (NonTerminalParseNode)node;
//        //        SeqReduction seqRd = nt.LatestReduction;
//        //        ParseNode c = nt.FirstChild;

//        //        object currentContextAst = csParseNodeHolder.ContextAst;
//        //        int currentContextIndex = csParseNodeHolder.CurrentAstIndex;

//        //        int count = 0;
//        //        while (c != null)
//        //        {
//        //            csParseNodeHolder.ContextAst = null;
//        //            WalkNodes(c, csParseNodeHolder);
//        //            csParseNodeHolder.AddContextAst(csParseNodeHolder.ContextAst);
//        //            //fill component 
//        //           // c = c.NextSibling;
//        //            count++;
//        //        }

//        //        //restore back
//        //        csParseNodeHolder.ContextAst = currentContextAst;
//        //        csParseNodeHolder.ContextChildCount = count;
//        //        //if (currentContextAst == null)
//        //        //{
//        //        //    if (nt is NonTerminalParseNodeList)
//        //        //    {

//        //        //    }
//        //        //}
//        //        if (seqRd != null)
//        //        {
//        //            csParseNodeHolder.CurrentChildAstIndex = currentContextIndex;
//        //            csParseNodeHolder.ContextOwner = nt;
//        //            //walk with context ast ***
//        //            RecursiveInvoke(seqRd, csParseNodeHolder);
//        //        }

//        //        csParseNodeHolder.Pop(count);
//        //    }
//        //}
//    }


    public class ArrayTypeBuilder : ArrayTypeParser.Walker
    {
        public override void NewArrayType()
        {
            throw new NotImplementedException();
        }

        public override void NewRankSpecifier()
        {
            throw new NotImplementedException();
        }
    }
    public class TypeBuilder : TypeParser.Walker
    {
        public override void NewArrayType()
        {
            throw new NotImplementedException();
        }
        public override void NewRankSpecifier()
        {
            throw new NotImplementedException();
        }
        public void NewTypeParameterConstraints()
        {

            //a.A(new TypeParameterConstraints());
        }
        public void AddTypeParameterName()
        {

            //var b = a.ContextOwner;
            //a.A<TypeParameterConstraints>().TypeParameterName = b.ToString();
        }

        public void AddTypePar_TypeName()
        {
            //var b = a.ContextOwner;
            //a.A<TypeParameterConstraints>().AddConstraint(new TypeParameterConstraint() { Kind = TypeParameterConstraintKind.ClassName, Name = b.ToString() });
        }
        public void AddTypePar_Class()
        {

            //a.A<TypeParameterConstraints>().AddConstraint(new TypeParameterConstraint() { Kind = TypeParameterConstraintKind.Class });
        }
        public void AddTypePar_Struct()
        {

            //a.A<TypeParameterConstraints>().AddConstraint(new TypeParameterConstraint() { Kind = TypeParameterConstraintKind.Struct });
        }
        public void AddTypePar_New()
        {
            //a.A<TypeParameterConstraints>().AddConstraint(new TypeParameterConstraint() { Kind = TypeParameterConstraintKind.New });
        }
    }

    public class StatementBuilder : StatementParser.Walker
    {
        public override void NewBlockStatement()
        {
            //a.A(new CodeBlockStatement());

        }
        public override void AddBlockStatementContent()
        {
            //var blockStmt = a.A<CodeBlockStatement>();
            //List<CodeStatement> stmts = new List<CodeStatement>();
            //blockStmt.Body = stmts;
            //CsParseNodeHolder holder = (CsParseNodeHolder)a;
            //object stmts_list = holder.GetContextAst(holder.CurrentChildAstIndex);
            //if (stmts_list is CodeStatement)
            //{
            //    stmts.Add((CodeStatement)stmts_list);
            //}
            //else
            //{

            //}

        }
        public override void NewIfElseStatement()
        {
            //a.A(new CodeIfElseStatement());

        }
        public override void AddTestExpression()
        {
            //create if block
            //var ifElseStmt = a.A<CodeIfElseStatement>();
            //ifElseStmt.AddBlock(new CodeIfBlock(a.B<CodePrimitiveExpression>()));
        }
        public override void IfStatementAddTrueStatement()
        {
            //var ifElseStmt = a.A<CodeIfElseStatement>();
            //ifElseStmt.IfBlock.Body = a.B<CodeStatement>();
        }
        public override void IfStatementAddFalseStatement()
        {

            //var ifElseStmt = a.A<CodeIfElseStatement>();
            //var codeElseBlock = new CodeElseBlock(a.B<CodeStatement>());
            //ifElseStmt.AddBlock(codeElseBlock);
        }
        //-------------------------
        public override void NewLocalVarDecl()
        {
            //a.A(new CodeLocalVarDecl());
        }
        public override void AddLocalVarName()
        {
            //var b = a.ContextOwner;
            //a.A<CodeLocalVarDecl>().LocalVarName = b.ToString();
        }
        public override void AddLocalVarInitValue()
        {

        }
        //-------------------------
        public override void NewReturnStatement()
        {
            //a.A(new CodeReturnStatement());
        }
        public override void AddReturnExpression()
        {
            //a.A<CodeReturnStatement>().Expression = a.B<CodeExpression>();

        }
        //-------------------------
    }
    public class ExpressionBuilder : ExpressionParser.Walker
    {
        public override void NewLiteralTrue()
        {
            //var b = a.ContextOwner;
            //a.A(new CodePrimitiveExpression(CodeDom.PrimitiveTokenName.True, b.ToString()));
        }
        public override void NewLiteralFalse()
        {
            //var b = a.ContextOwner;
            //a.A(new CodePrimitiveExpression(CodeDom.PrimitiveTokenName.False, b.ToString()));
        }
        public override void NewLiteralInteger()
        {
            //var b = a.ContextOwner;
            //a.A(new CodePrimitiveExpression(CodeDom.PrimitiveTokenName.Integer, b.ToString()));
        }
        public override void NewLiteralString()
        {
            //var b = a.ContextOwner;
            //a.A(new CodePrimitiveExpression(CodeDom.PrimitiveTokenName.String, b.ToString()));
        }
        public override void NewBinOpExpression_Add_Sub()
        {
            //a.A(new CodeBinaryOperatorExpression()); //set host for child walk 
            //a.WalkChild(); 
            //ParseNode left, binopNode, right;
            //((NonTerminalParseNode)b).GetChild(out left, out binopNode, out right); 
            //Token tk_binop = (Token)binopNode; 
            //CodeDom.CodeBinaryOperatorName binOpName = CodeDom.CodeBinaryOperatorName.Plus;
            //switch (binopNode.ToString())
            //{
            //    case "+": binOpName = CodeDom.CodeBinaryOperatorName.Plus; break;
            //    case "-": binOpName = CodeDom.CodeBinaryOperatorName.Minus; break;
            //    default: throw new NotSupportedException();
            //}
            //newCodeBinOpExpr.BinaryOp = binOpName;
            //newCodeBinOpExpr.LeftExpression = (CodeExpression)left.Tag;
            //newCodeBinOpExpr.RightExpression = (CodeExpression)right.Tag;

        }
        public override void BinOp_AddLeft()
        {

            //a.A<CodeBinaryOperatorExpression>().LeftExpression = a.B<CodeExpression>();
        }
        public override void BinOp_AddRight()
        {
#if RETURN_NOW
            return;
#endif
            //a.A<CodeBinaryOperatorExpression>().RightExpression = a.B<CodeExpression>();
        }

        public override void NewObjectInitialization()
        {

        }
        public override void AddObjectMemberInitializer()
        {

        }
        //----------------------------------------------------------------------
        public override void NewAnonymousObject()
        {
            //var objCreationExpr = new CodeAnonymousObjectCreationExpression();
            //var csHolder = (CsParseNodeHolder)a;
            //int childAt = csHolder.CurrentChildAstIndex;
            //var initilizer = (CodeObjectInitializer)csHolder.GetContextAst(childAt);
            //objCreationExpr.objInitializer = initilizer;
            //a.A(objCreationExpr);

        }
        public override void NewObjectInitializer()
        {

            //a.A(new CodeObjectInitializer());
        }
        public override void AddMemberDeclarators()
        {
            //a.A<CodeObjectInitializer>().memberDecls = a.B<CodeMemberDeclarators>();
        }
        public override void NewMemberDeclarators()
        {
            //var codeMemberDecls = new CodeMemberDeclarators();
            //var csHolder = (CsParseNodeHolder)a;
            //int childAt = csHolder.CurrentChildAstIndex;
            //int childLim = childAt + csHolder.ContextChildCount;

            //for (int i = childAt; i < childLim; ++i)
            //{
            //    object c = csHolder.GetContextAst(i);
            //    codeMemberDecls.declarators.Add((CodeMemberDeclarator)c);
            //    i++;
            //}


            //a.A(codeMemberDecls);
        }
        public override void NewMemberDeclaratorIdAssing()
        {
            //a.A(new CodeMemberDeclarator());

        }
        public override void MemberDeclAddName()
        {
            //var b = a.ContextOwner;
            //a.A<CodeMemberDeclarator>().id = b.ToString();

        }
        public override void MemberDeclAddValue()
        {
            //a.A<CodeMemberDeclarator>().expr = a.B<CodeExpression>();

        }
    }



    public class NamespaceBuilder : NamespaceParser.Walker
    {
        public override void NewCompilationUnit()
        {
            //a.A(new CodeAstCompilationUnit());
        }
        public override void NewNamespaceDeclaration()
        {
            //a.A(new CodeNamespace());
        }
        public override void AddNamespaceToNamespaceMember()
        {

        }
        public override void AddTypeDeclToNamespaceMember()
        {
            //if (a.ContextAst is CodeAstCompilationUnit)
            //{
            //    a.A<CodeAstCompilationUnit>().AddMember(a.B<INamespaceMember>());
            //}
            //else if (a.ContextAst is CodeNamespace)
            //{
            //    a.A<CodeNamespace>().AddMember(a.B<INamespaceMember>());
            //}
        }
        public override void NewUsingDirective()
        {
            

            //var sq = a.CurrentContextSequence;
            //ParseNode owner = a.ContextOwner;
            //ParseNode p1, p2, p3;
            //a.PeekStack(out p1, out p2, out p3);
            //a.A(new CodeUsingNamespaceDirective(""));

            //if (sq.HasSomeUserExpectedSymbolMonitor)
            //{
            //    var monitors = sq.GetAllUserExpectedSymbolReductionMonitors();
            //    int j = monitors.Length;
            //    for (int i = 0; i < j; ++i)
            //    {
            //        var monitor = monitors[i];
            //        if (monitor != null)
            //        {
            //            monitor.NotifyReduction(a);
            //        }
            //    }
            //}

            //a.DontBuildNextParseTree();
        }
    }
    public class ClassBuilder : ClassDeclParser.Walker
    {
        public override void NewClassDeclaration()
        {
            //h.A(new CodeTypeDeclaration(CodeDom.CodeTypeDeclKind.Class));
        }
        //        public override void NewClassDeclaration(ParseNodeHolder a)
        //        {

        //            a.A(new CodeTypeDeclaration(CodeDom.CodeTypeDeclKind.Class));
        //        }
        //        public override void AddClassName(ParseNodeHolder a)
        //        {

        //            var b = a.ContextOwner;
        //            a.A<CodeTypeDeclaration>().SetTypeName(b.ToString());
        //        }
        //        public override void MakePartial(ParseNodeHolder a)
        //        {
        //        }
        //        public override void AddTypeParameterList(ParseNodeHolder a)
        //        {
        //        }
        //        public override void AddTypeParameterConstraintClauses(ParseNodeHolder a)
        //        {

        //            a.A<CodeTypeDeclaration>().TypeParameterConstraints = a.B<TypeParameterConstraints>();
        //        }
        //        public override void AddClassBody(ParseNodeHolder a)
        //        {
        //        }
        //        public override void AddClassAttributes(ParseNodeHolder a)
        //        {
        //        }
        //        public override void AddClassBase(ParseNodeHolder a)
        //        {
        //        }
        //        public override void AddClassModifiers(ParseNodeHolder a)
        //        {
        //#if RETURN_NOW
        //            return;
        //#endif
        //            var classDecl = a.A<CodeTypeDeclaration>();
        //            // NonTerminalParseNode nt = (NonTerminalParseNode)b;
        //            //int j = nt.ChildCount;
        //            //for (int i = 0; i < j; ++i)
        //            //{
        //            //    ParseNode modifier = nt.GetChildNode(i);
        //            //    if (modifier.Ast is CsTypeModifier)
        //            //    {
        //            //        CsTypeModifier typeMod = (CsTypeModifier)modifier.Ast;
        //            //        switch (typeMod)
        //            //        {
        //            //            case CsTypeModifier.Public:
        //            //                {
        //            //                    classDecl.Visibility = TypeVisibility.Public;
        //            //                } break;
        //            //            case CsTypeModifier.Private:
        //            //                {
        //            //                    classDecl.Visibility = TypeVisibility.Private;
        //            //                } break;
        //            //            case CsTypeModifier.Protected:
        //            //                {
        //            //                    classDecl.Visibility = TypeVisibility.Protected;
        //            //                } break;
        //            //            case CsTypeModifier.Internal:
        //            //                {
        //            //                    classDecl.Visibility = TypeVisibility.Internal;
        //            //                } break;
        //            //            case CsTypeModifier.Static:
        //            //                {
        //            //                    classDecl.IsStatic = true;
        //            //                } break;
        //            //            case CsTypeModifier.Sealed:
        //            //                {
        //            //                    classDecl.ChainKind = TypeChainKind.Sealed;
        //            //                } break;
        //            //            case CsTypeModifier.New:
        //            //                {
        //            //                    classDecl.ChainKind = TypeChainKind.New;

        //            //                } break;
        //            //            default:
        //            //                throw new NotSupportedException();
        //            //        }
        //            //    }
        //            //    else
        //            //    {
        //            //        switch (modifier.ToString())
        //            //        {
        //            //            case "public":
        //            //                break;
        //            //        }
        //            //    }
        //            //}
        //        }


        //        public override void AddModifier_Public(ParseNodeHolder a)
        //        {
        //            a.A<CodeTypeDeclaration>().MemberAccessibility = CodeDom.CodeTypeMemberAccessibility.Public;
        //        }
        //        public override void AddModifier_Private(ParseNodeHolder a)
        //        {
        //            a.A<CodeTypeDeclaration>().MemberAccessibility = CodeDom.CodeTypeMemberAccessibility.Private;
        //        }
        //        public override void AddModifier_Protected(ParseNodeHolder a)
        //        {
        //            var typedecl = a.A<CodeTypeDeclaration>();
        //            if (typedecl.MemberAccessibility == CodeDom.CodeTypeMemberAccessibility.Assembly)
        //            {
        //                typedecl.MemberAccessibility = CodeDom.CodeTypeMemberAccessibility.FamilyAndAssembly;
        //            }
        //            else
        //            {   //TODO: check here again
        //                typedecl.MemberAccessibility = CodeDom.CodeTypeMemberAccessibility.Family;
        //            }

        //        }
        //        public override void AddModifier_Internal(ParseNodeHolder a)
        //        {
        //            var typedecl = a.A<CodeTypeDeclaration>();
        //            if (typedecl.MemberAccessibility == CodeDom.CodeTypeMemberAccessibility.Family)
        //            {
        //                typedecl.MemberAccessibility = CodeDom.CodeTypeMemberAccessibility.FamilyAndAssembly;
        //            }
        //            else
        //            {   //TODO: check here again
        //                typedecl.MemberAccessibility = CodeDom.CodeTypeMemberAccessibility.Assembly;
        //            }
        //        }
    }
}