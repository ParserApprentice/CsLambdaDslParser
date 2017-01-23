//MIT 2015-2017, ParserApprentice 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using System.IO;
using Parser.CodeDom;
namespace Parser.AsmInfrastructures
{


    public class CodeDomToSourceCodeConverter
    {

        public void DumpToFile(string outputFile, CodeObject codeObject)
        {

            StringBuilder stBuilder = new StringBuilder();
            AsmIndentTextWriter writer = new AsmIndentTextWriter(stBuilder);
            if (codeObject is CodeNamespace)
            {
                GenerateNamespace((CodeNamespace)codeObject, writer);
            }
            else if (codeObject is CodeTypeDeclaration)
            {
                GenerateTypeDecl((CodeTypeDeclaration)codeObject, writer);
            }
            else if (codeObject is CodeMethodDeclaration)
            {
                GenerateTypeMember((CodeMethodDeclaration)codeObject, writer);
            }
            else
            {

            }

            FileStream fs = new FileStream(outputFile, FileMode.Create);
            BinaryWriter binWriter = new BinaryWriter(fs);
            binWriter.Write(stBuilder.ToString().ToCharArray());
            binWriter.Close();
            fs.Close();
            fs.Dispose();
            //---
        }
        public static void Generate(CodeTypeMember typeMember, AsmIndentTextWriter writer)
        {

            switch (typeMember.Kind)
            {


                case CodeObjectKind.Struct:
                case CodeObjectKind.Delegate:
                case CodeObjectKind.Enum:
                case CodeObjectKind.Class:
                case CodeObjectKind.Interface:
                    {
                        GenerateTypeDecl((CodeTypeDeclaration)typeMember, writer);
                    }
                    break;
                //--------------------------------------------------------------------
                //type member
                case CodeObjectKind.Field:
                    {
                        GenerateTypeMember((CodeFieldDeclaration)typeMember, writer);
                    }
                    break;
                case CodeObjectKind.ObjectConstructor:
                case CodeObjectKind.Method:
                    {
                        GenerateTypeMember((CodeMethodDeclaration)typeMember, writer);
                    }
                    break;
                case CodeObjectKind.Property:
                    {
                        GenerateTypeMember((CodePropertyDeclaration)typeMember, writer);
                    }
                    break;
                case CodeObjectKind.Event:
                    {
                        GenerateTypeMember((CodeEventDeclaration)typeMember, writer);
                    }
                    break;
                default:
                    {
                    }
                    break;
            }
        }
        static void GenerateTypeMember(CodeFieldDeclaration fieldDecl, AsmIndentTextWriter writer)
        {
            writer.Append(CreateTypeMemberModifier(fieldDecl));
            writer.Append(' ');
            writer.Append(fieldDecl.ReturnType.ToString() + " " + fieldDecl.Name);
            if (fieldDecl.InitExpression != null)
            {

                writer.Append("=");
                GenerateExpression(fieldDecl.InitExpression, writer);
            }
            writer.Append(";");
        }

        static void GenerateTypeMember(CodeMethodDeclaration methodDecl, AsmIndentTextWriter writer)
        {

            writer.Append(CreateTypeMemberModifier(methodDecl));
            if (methodDecl is CodeObjectConstructorDeclaration)
            {

                writer.Append(' ');
                if (methodDecl.TypeDeclaration == null)
                {
                    writer.Append(".ctor");
                }
                else
                {

                    writer.Append(methodDecl.TypeDeclaration.Name);
                }
            }
            else
            {
                writer.Append(' ');
                writer.Append(methodDecl.ReturnType.FullName); //return type                
                writer.Append(' ');
                writer.Append(methodDecl.Name);
            }

            writer.Append('(');
            if (methodDecl.ParameterList != null)
            {
                int j = methodDecl.ParameterList.Count;
                for (int i = 0; i < j; i++)
                {
                    GenerateExpression(methodDecl.ParameterList[i], writer);
                    if (i < j - 1)
                    {
                        writer.Append(',');
                    }
                }
            }

            if (methodDecl.Body != null)
            {
                writer.CloseLine(')');
                writer.CloseLineNoTab('{');
                writer.IndentLevel++;
                if (methodDecl.Body != null)
                {
                    foreach (CodeStatement stm in methodDecl.Body)
                    {
                        writer.OutputTabs();
                        GenerateStatement(stm, writer);
                        writer.CloseLineNoTab();
                    }
                }
                writer.IndentLevel--;
                writer.OutputTabs();
                writer.Append('}');
            }
            else
            {
                writer.CloseLine(");");
            }

        }
        static void GenerateTypeMember(CodePropertyDeclaration propertyDecl, AsmIndentTextWriter writer)
        {
            writer.Append(CreateTypeMemberModifier(propertyDecl));
            writer.Append(' ');
            writer.Append(propertyDecl.ReturnType.FullName);
            writer.Append(' ');
            writer.CloseLine(propertyDecl.Name);
            writer.CloseLineNoTab('{');

            if (propertyDecl.GetDeclaration != null)
            {
                writer.IndentLevel++;
                writer.OutputTabs();


                CodeMethodDeclaration getMethodDecl = propertyDecl.GetDeclaration;
                if (!getMethodDecl.IsExtern)
                {
                    writer.CloseLine("get");
                    writer.CloseLineNoTab('{');

                    if (getMethodDecl.Body != null)
                    {
                        writer.IndentLevel++;
                        foreach (CodeStatement stm in getMethodDecl.Body)
                        {
                            writer.OutputTabs();
                            GenerateStatement(stm, writer);
                            writer.CloseLineNoTab();
                        }
                        writer.IndentLevel--;
                    }
                    writer.CloseLine();
                    writer.CloseLineNoTab('}');
                }
                else
                {
                    writer.CloseLineNoTab("extern get;");
                }
                writer.IndentLevel--;
                //-------------------------------------
                if (propertyDecl.SetDeclaration == null)
                {
                    writer.OutputTabs();
                }
                //-------------------------------------
            }
            if (propertyDecl.SetDeclaration != null)
            {
                CodeMethodDeclaration setMethodDecl = propertyDecl.SetDeclaration;

                writer.IndentLevel++;
                writer.OutputTabs();
                if (!setMethodDecl.IsExtern)
                {
                    writer.CloseLine("set");
                    writer.CloseLineNoTab('{');

                    if (setMethodDecl.Body != null)
                    {
                        writer.IndentLevel++;
                        foreach (CodeStatement stm in setMethodDecl.Body)
                        {
                            writer.OutputTabs();
                            GenerateStatement(stm, writer);
                            writer.CloseLineNoTab();
                        }
                        writer.IndentLevel--;
                    }
                    writer.CloseLine();
                    writer.CloseLineNoTab('}');
                }
                else
                {
                    writer.CloseLineNoTab("extern set;");
                }
                writer.IndentLevel--;
                writer.OutputTabs();
            }
            writer.CloseLine('}');
        }
        static void GenerateTypeMember(CodeEventDeclaration eventDecl, AsmIndentTextWriter writer)
        {

            writer.Append(CreateTypeMemberModifier(eventDecl));
            writer.Append(' ');
            writer.Append("event ");
            writer.Append(eventDecl.ReturnType.FullName);
            writer.Append(' ');
            writer.Append(eventDecl.Name);
            writer.CloseLine(";");

        }
        static void GenerateNamespace(CodeNamespace codeNamespace, AsmIndentTextWriter writer)
        {

            writer.CloseLine("namespace " + codeNamespace.Name.ToString()); //namespace name
            writer.CloseLineNoTab("{");

            writer.IndentLevel++;
            foreach (INamespaceMember nsMb in codeNamespace.GetMemberIterForward())
            {
                writer.OutputTabs();
                if (nsMb is CodeTypeDeclaration)
                {
                    GenerateTypeDecl((CodeTypeDeclaration)nsMb, writer);
                }
                else if (nsMb is CodeNamespace)
                {
                    GenerateNamespace((CodeNamespace)nsMb, writer);
                }
                writer.CloseLineNoTab();
            }

            writer.IndentLevel--;
            writer.CloseLine("}");

        }
        static string CreateTypeMemberModifier(CodeTypeMember typeMember)
        {

            string st = string.Empty;
            switch (typeMember.MemberAccessibility)
            {
                case CodeTypeMemberAccessibility.Public:
                    {
                        st = "public ";
                    }
                    break;
                case CodeTypeMemberAccessibility.Private:
                    {
                        st = "private ";
                    }
                    break;
                case CodeTypeMemberAccessibility.Family:
                    {
                        st = "protected";
                    }
                    break;
                case CodeTypeMemberAccessibility.FamilyAndAssembly:
                    {
                        st = "protected internal";
                    }
                    break;
                case CodeTypeMemberAccessibility.Assembly:
                    {
                        st = "internal ";
                    }
                    break;
            }

            //---------------------------            
            if (typeMember.IsStatic)
            {
                st += "static ";
            }
            //---------------------------
            if (typeMember.IsAbstract)
            {
                st += "abstract ";
            }
            if (typeMember.IsVirtual)
            {
                st += "virtual ";
            }
            if (typeMember.IsOverride)
            {
                st += "override ";
            }

            if (typeMember.IsSealed)
            {
                st += "sealed ";
            }
            if (typeMember.IsReadonly)
            {
                st += "readonly ";
            }
            if (typeMember.IsConst)
            {
                st += "const ";
            }
            if (typeMember.IsExtern)
            {
                st += "extern ";
            }
            return st;
        }

        static void GenerateCustomAttribute(CodeAttributeDeclarationCollection customAttrCollection, AsmIndentTextWriter writer)
        {

            if (customAttrCollection != null)
            {
                foreach (CodeAttributeDeclaration attrDecl in customAttrCollection)
                {
                    writer.Append('[');
                    writer.Append(attrDecl.AttributeType.FullName);
                    if (attrDecl.Arguments != null)
                    {
                        int j = attrDecl.Arguments.Count;
                        int i = 0;
                        if (j > 0)
                        {
                            writer.Append('(');
                            foreach (CodeAttributeArgument attrArg in attrDecl.Arguments)
                            {
                                GenerateExpression(attrArg.Value, writer);
                                if (i < j - 1)
                                {
                                    writer.Append(',');
                                }
                                i++;
                            }
                            writer.Append(')');
                        }
                    }
                    writer.Append(']');
                    writer.CloseLine();
                }
            }
        }
        static void GenerateTypeDecl(CodeTypeDeclaration typedecl, AsmIndentTextWriter writer)
        {


            if (typedecl.CustomAttributes != null)
            {
                GenerateCustomAttribute(typedecl.CustomAttributes, writer);
            }

            writer.Append(CreateTypeMemberModifier(typedecl));

            if (typedecl.IsPartial)
            {
                writer.Append("partial ");
            }

            string codeTypeWhat = "";
            if (typedecl.IsClass)
            {
                codeTypeWhat = "class";
            }
            else if (typedecl.IsStruct)
            {
                codeTypeWhat = "struct";
            }
            else if (typedecl.IsEnum)
            {
                codeTypeWhat = "enum";
            }
            else if (typedecl.IsInterface)
            {
                codeTypeWhat = "interface";
            }
            else if (typedecl.IsDelegate)
            {
                codeTypeWhat = "delegate";
            }
            else
            {
                codeTypeWhat = "_class";
            }

            writer.Append(codeTypeWhat + " " + typedecl.Name);
            GenerateBaseTypeSig(typedecl, writer);
            writer.CloseLine();
            writer.CloseLineNoTab('{');
            writer.IndentLevel++;
            foreach (CodeTypeMember typeMember in typedecl.Members)
            {

                writer.OutputTabs();
                Generate(typeMember, writer);
                writer.CloseLineNoTab();
            }
            writer.IndentLevel--;
            writer.OutputTabs();
            writer.Append('}');
        }
        static void GenerateBaseTypeSig(CodeTypeDeclaration typedecl, AsmIndentTextWriter writer)
        {
            if (typedecl.IsClass || typedecl.IsInterface)
            {

                CodeTypeReferenceCollection refCollection = typedecl.BaseTypes;
                if (refCollection != null)
                {
                    int j = refCollection.Count;
                    if (j > 0)
                    {
                        if (typedecl is CodeTypeDeclaration)
                        {
                            writer.Append(':');
                        }

                        if (refCollection[0].FullName != "System.Object")
                        {
                            writer.Append(refCollection[0].FullName);
                            if (j > 1)
                            {
                                writer.Append(',');
                            }
                        }
                        //----------------------------
                        for (int i = 1; i < j; i++)
                        {

                            writer.Append(refCollection[i].FullName);
                            if (i < j - 1)
                            {
                                writer.Append(',');
                            }
                        }
                    }
                }
            }

        }


        public static void GenerateStatement(IEnumerable<CodeStatement> codeStatements, AsmIndentTextWriter writer)
        {
            foreach (CodeStatement stmt in codeStatements)
            {
                GenerateStatement(stmt, writer);
            }
        }
        public static void GenerateStatement(CodeStatement codeStatement, AsmIndentTextWriter writer)
        {

            if (codeStatement.CompilerReplaceStatement != null)
            {
                GenerateStatement(codeStatement.CompilerReplaceStatement, writer);
                return;
            }

            switch (codeStatement.StatementType)
            {
                case CodeStatementType.CodeDefineStatement:
                    {
                        GenerateStatement((CodeDefineStatement)codeStatement, writer);

                    }
                    break;
                case CodeStatementType.CodeBreakPointStatement:
                    {
                        writer.CloseLineNoTab("@.breakpoint;");
                    }
                    break;
                case CodeStatementType.CodeBreakStatement:
                    {
                        writer.CloseLineNoTab("break;");
                    }
                    break;
                case CodeStatementType.CodeIfElseStatement:
                    {
                        GenerateStatement((CodeIfElseStatement)codeStatement, writer);
                    }
                    break;
                case CodeStatementType.CodeContinueStatement:
                    {
                        writer.CloseLineNoTab("continue;");
                    }
                    break;
                case CodeStatementType.CodeDoWhileStatement:
                    {
                        GenerateStatement((CodeDoWhileStatement)codeStatement, writer);

                    }
                    break;
                case CodeStatementType.CodeEmptyStatement:
                    {
                        writer.CloseLineNoTab("//empty");
                    }
                    break;
                case CodeStatementType.CodeEndSpecialSegment:
                    {

                    }
                    break;
                case CodeStatementType.CodeExpressionStatement:
                    {
                        GenerateExpression(((CodeExpressionStatement)codeStatement).Expression, writer);
                        writer.CloseLineNoTab(';');
                    }
                    break;
                case CodeStatementType.CodeForEachStatement:
                    {
                        GenerateStatement((CodeForEachStatement)codeStatement, writer);
                    }
                    break;
                case CodeStatementType.CodeForLoopStatement:
                    {

                        GenerateStatement((CodeForLoopStatement)codeStatement, writer);


                    }
                    break;
                case CodeStatementType.CodeGoToStatement:
                    {

                        CodeGotoStatement gotoStm = (CodeGotoStatement)codeStatement;
                        writer.CloseLineNoTab("goto " + gotoStm.LabelName + ";");

                    }
                    break;
                case CodeStatementType.CodeLeaveStatement:
                    {

                        CodeLeaveStatement leaveStm = (CodeLeaveStatement)codeStatement;
                        writer.CloseLineNoTab("leave " + leaveStm.LabelName + ";");

                    }
                    break;
                case CodeStatementType.CodeLabelStatement:
                    {
                        CodeLabelStatement lblStm = (CodeLabelStatement)codeStatement;
                        writer.CloseLineNoTab("@_LABEL_" + lblStm.LabelName + ":");


                    }
                    break;

                case CodeStatementType.CodeReturnStatement:
                    {
                        CodeReturnStatement retStm = (CodeReturnStatement)codeStatement;
                        if (retStm.Expression != null)
                        {
                            writer.Append("return ");
                            GenerateExpression(retStm.Expression, writer);
                            writer.CloseLineNoTab(";");
                        }
                        else
                        {
                            writer.CloseLineNoTab("return;");
                        }

                    }
                    break;
                case CodeStatementType.CodeStartSpecialSegment:
                    {

                    }
                    break;
                case CodeStatementType.CodeBlockStatement:
                    {
                        CodeBlockStatement stmBlock = (CodeBlockStatement)codeStatement;
                        GenerateStatement(stmBlock.Body, writer);
                        writer.CloseLineNoTab();

                    }
                    break;
                case CodeStatementType.CodeSwitchStatement:
                    {
                        GenerateStatement((CodeSwitchStatement)codeStatement, writer);
                    }
                    break;
                case CodeStatementType.CodeTryCatchFinallyStatement:
                    {
                        GenerateStatement((CodeTryCatchFinallyStatement)codeStatement, writer);


                    }
                    break;
                case CodeStatementType.CodeWhileStatement:
                    {
                        GenerateStatement((CodeWhileStatement)codeStatement, writer);
                    }
                    break;
                case CodeStatementType.CodeYieldBreakStatement:
                    {
                        writer.CloseLineNoTab("yield break;");
                    }
                    break;
                case CodeStatementType.CodeYieldReturnStatement:
                    {
                        CodeYieldReturnStatement yretStm = (CodeYieldReturnStatement)codeStatement;
                        if (yretStm.Expression != null)
                        {
                            writer.Append("yield return ");
                            GenerateExpression(yretStm.Expression, writer);
                            writer.CloseLineNoTab(";");
                        }
                        else
                        {
                            writer.CloseLineNoTab("yield return;");
                        }

                    }
                    break;
                case CodeStatementType.CodeThrowExceptionStatement:
                    {
                        CodeThrowExceptionStatement throwStm = (CodeThrowExceptionStatement)codeStatement;

                        writer.Append("throw ");
                        GenerateExpression(throwStm.ToThrow, writer);
                        writer.CloseLineNoTab(';');

                    }
                    break;
                case CodeStatementType.CodeVariableDeclarationStatement:
                    {
                        CodeVariableDeclarationStatement varDeclStm = (CodeVariableDeclarationStatement)codeStatement;
                        //type 
                        int declCount = varDeclStm.DeclaratorCount;
                        CodeVariableDeclaratorExpression first = varDeclStm.GetDeclarator(0);

                        if (first.VariableType != null)
                        {
                            writer.Append(first.VariableType.FullName + " ");
                        }
                        else
                        {
                            writer.Append("var ");
                        }
                        //var name  
                        for (int i = 1; i < declCount; ++i)
                        {
                            var dec = varDeclStm.GetDeclarator(i);
                            writer.Append(dec.VariableNameAsString);
                            if (dec.InitExpression != null)
                            {
                                writer.Append('=');
                                GenerateExpression(dec.InitExpression, writer);
                            }
                            if (i > 0 && i < declCount - 1)
                            {
                                writer.Append(',');
                            }
                        }
                        writer.Append(";");


                    }
                    break;
                case CodeStatementType.CodeCommentStatement:
                    {

                        CodeCommentStatement cmtStm = (CodeCommentStatement)codeStatement;
                        writer.Append(cmtStm.Comment.ToString());

                    }
                    break;
                default:
                    {
                        throw new NotSupportedException();
                    }
            }
        }

        static void GenerateStatement(CodeIfElseStatement ifelseStm, AsmIndentTextWriter writer)
        {
            //if-else condition

            int limit = ifelseStm.ConditionsCount;
            int i = 0;
            foreach (CodeIfBlock condBlock in ifelseStm.GetConditionBlocksIter())
            {
                GenerateIfBlock(condBlock, writer);
                i++;
                if (i < limit)
                {

                    writer.OutputTabs();
                    writer.CloseLine("else ");
                }
            }
            if (ifelseStm.ElseBlock != null)
            {
                writer.OutputTabs();
                writer.CloseLine("else ");
                GenerateElseBlock(ifelseStm.ElseBlock, writer);
            }

        }
        static void GenerateElseBlock(CodeElseBlock elseBlock, AsmIndentTextWriter writer)
        {
            GenerateStatement(elseBlock.Body, writer);
            writer.CloseLineNoTab();
        }
        static void GenerateIfBlock(CodeIfBlock conditionBlock, AsmIndentTextWriter writer)
        {

            writer.Append("if (");
            GenerateExpression(conditionBlock.ConditionExpression, writer);
            writer.CloseLine(')');
            GenerateStatement(conditionBlock.Body, writer);
            writer.CloseLineNoTab();
        }
        static void WriteStatementBlock(CodeStatementCollection stmCollection, AsmIndentTextWriter writer)
        {
            writer.CloseLineNoTab('{');
            writer.IndentLevel++;
            foreach (CodeStatement stm in stmCollection)
            {
                writer.OutputTabs();
                GenerateStatement(stm, writer);

            }
            writer.IndentLevel--;
            writer.OutputTabs();
            writer.Append('}');
        }
        static void GenerateStatement(CodeForLoopStatement forloopStm, AsmIndentTextWriter writer)
        {
            writer.Append("for (");
            if (forloopStm.ForInitializer != null)
            {
                GenerateExpressionCollection(forloopStm.ForInitializer, writer);
            }
            writer.Append(';');
            //--------------------------------------------------------------------------------------------------------------------
            if (forloopStm.ConditionExpression != null)
            {
                GenerateExpression(forloopStm.ConditionExpression, writer);
            }
            writer.Append(';');
            if (forloopStm.ForIncrementors != null)
            {
                GenerateExpressionCollection(forloopStm.ForIncrementors, writer);
            }
            writer.CloseLine(')');
            //WriteStatementBlock(forloopStm.Body, writer);
            GenerateStatement(forloopStm.Body, writer);
            writer.CloseLineNoTab();
        }
        static void GenerateStatement(CodeForEachStatement foreachStm, AsmIndentTextWriter writer)
        {
            writer.Append("foreach(");
            //writer.Append(foreachStm.ForItemStatement.VariableType.TypeName);

            if (foreachStm.ForItemStatement.GetDeclarator(0).VariableType != null)
            {
                writer.Append(foreachStm.ForItemStatement.GetDeclarator(0).VariableType.FullName);
            }
            else
            {
                writer.Append("var");
            }


            writer.Append(' ');

            writer.Append(foreachStm.ForItemStatement.GetDeclarator(0).VariableNameAsString);

            writer.Append(" in ");
            GenerateExpression(foreachStm.InWhatExpression, writer);
            writer.CloseLine(')');
            //WriteStatementBlock(foreachStm.Body, writer);
            GenerateStatement(foreachStm.Body, writer);
            writer.CloseLineNoTab();

        }
        static void GenerateStatement(CodeDoWhileStatement doWhileStm, AsmIndentTextWriter writer)
        {
            //ประโยค do - while () 
            writer.CloseLine("do");
            GenerateStatement(doWhileStm.Body, writer);
            writer.CloseLine("while (");
            GenerateExpression(doWhileStm.Condition, writer);
            writer.CloseLineNoTab(");");
        }
        static void GenerateStatement(CodeDefineStatement defineStatement, AsmIndentTextWriter writer)
        {


            writer.CloseLine("def " + defineStatement.DefineName + "(");
            if (defineStatement.DefineFieldExpressionCollection != null)
            {
                CodeDefineFieldExpressionCollection defExprCollection = defineStatement.DefineFieldExpressionCollection;
                int j = defExprCollection.Count;
                for (int i = 0; i < j; i++)
                {
                    GenerateExpression(defExprCollection[i], writer);
                    if (i < j - 1)
                    {
                        writer.Append(',');
                    }
                }
            }

            writer.CloseLineNoTab(");");
        }

        static void GenerateStatement(CodeWhileStatement whileStm, AsmIndentTextWriter writer)
        {

            writer.Append("while(");
            GenerateExpression(whileStm.Condition, writer);
            writer.CloseLine(')');
            //WriteStatementBlock(whileStm.Body, writer);
            GenerateStatement(whileStm.Body, writer);
            writer.CloseLine();
        }
        static void GenerateStatement(CodeTryCatchFinallyStatement tryCatchFinallyStm, AsmIndentTextWriter writer)
        {
            writer.CloseLine("try");
            //WriteStatementBlock(tryCatchFinallyStm.TryClause.Body, writer);
            GenerateStatement(tryCatchFinallyStm.TryClause.Body, writer);
            writer.CloseLineNoTab();
            foreach (CodeCatchClause catchClause in tryCatchFinallyStm.GetCatchClauseIter())
            {
                writer.OutputTabs();
                writer.Append("catch");
                if (catchClause.CatchExceptionType != null)
                {

                    writer.Append('(');

                    writer.Append(catchClause.CatchExceptionType.FullName);
                    if (catchClause.LocalNameAsString != null)
                    {

                        writer.Append(' ');
                        writer.Append(catchClause.LocalNameAsString);
                    }
                    writer.Append(')');
                }
                writer.CloseLine();
                //WriteStatementBlock(catchClause.Body, writer);
                GenerateStatement(catchClause.Body, writer);
                writer.CloseLineNoTab();
            }
            if (tryCatchFinallyStm.FinallyClause != null)
            {
                writer.OutputTabs();
                writer.CloseLine("finally");
                GenerateStatement(tryCatchFinallyStm.FinallyClause.Body, writer);
                writer.CloseLineNoTab();
            }
        }
        static void GenerateStatement(CodeSwitchStatement switchStm, AsmIndentTextWriter writer)
        {
            writer.Append("switch(");
            GenerateExpression(switchStm.SwitchExpression, writer);
            writer.CloseLine(')');
            writer.CloseLineNoTab('{');
            writer.IndentLevel++;

            foreach (CodeSwitchBlock block in switchStm.GetSwitchBlockIter())
            {
                writer.OutputTabs();

                if (!(block is CodeSwitchDefaultBlock))
                {

                    writer.Append("case ");
                    GenerateExpression(block.Label, writer);
                    writer.CloseLine(':');
                }
                else
                {
                    writer.CloseLine("default:");
                }

                GenerateStatement(block.Body, writer);
                writer.CloseLineNoTab();
            }
            writer.IndentLevel--;
            writer.OutputTabs();
            writer.CloseLine('}');
        }
        static void GenerateExpressionCollection(CodeArgumentList arglist, AsmIndentTextWriter writer)
        {
            int j = arglist.Count - 1;
            int i = 0;
            foreach (CodeArgument arg in arglist)
            {
                if (string.IsNullOrEmpty(arg.Name))
                {
                    writer.Append(arg.Name);
                    writer.Append(':');
                }

                GenerateExpression(arg.Value, writer);

                if (i < j)
                {
                    writer.Append(',');
                }
                i++;
            }

        }
        static void GenerateExpressionCollection(CodeExpressionCollection exprCollection, AsmIndentTextWriter writer)
        {
            int j = exprCollection.Count - 1;
            int i = 0;
            foreach (CodeExpression expr in exprCollection)
            {
                GenerateExpression(expr, writer);
                if (i < j)
                {
                    writer.Append(',');
                }
                i++;
            }
        }
        public static void GenerateExpression(CodeExpression codeExpression, AsmIndentTextWriter writer)
        {

            switch (codeExpression.ExpressionKind)
            {

                case CodeExpressionKind.ArrayCreateExpression:
                    {
                        CodeArrayCreateExpression arrCreateExpr = (CodeArrayCreateExpression)codeExpression;
                        if (arrCreateExpr.Initializer != null)
                        {
                            writer.Append("new " + arrCreateExpr.CreateType.FullName + "[]{");
                            GenerateExpressionCollection(arrCreateExpr.Initializer, writer);
                            writer.Append('}');
                        }
                        else
                        {

                            writer.Append("new " + arrCreateExpr.CreateType.FullName + "[");
                            GenerateExpression(arrCreateExpr.InitSizeExpression, writer);
                            writer.Append(']');
                        }
                    }
                    break;
                case CodeExpressionKind.IndexerAccessExpression:
                    {
                        CodeIndexerAccessExpression arrIndexExpr = (CodeIndexerAccessExpression)codeExpression;
                        GenerateExpression(arrIndexExpr.TargetExpression, writer);
                        writer.Append('[');
                        GenerateExpressionCollection(arrIndexExpr.ArgList, writer);
                        writer.Append(']');

                    }
                    break;
                case CodeExpressionKind.BaseCtorInvoke:
                    {
                        CodeBaseCtorInvoke baseCtorInvoke = (CodeBaseCtorInvoke)codeExpression;
                        int j = baseCtorInvoke.ParameterCount;
                        if (j == 0)
                        {
                            writer.Append("base::.ctor()");
                        }
                        else
                        {
                            writer.Append("base::.ctor(");
                            for (int i = 0; i < j; i++)
                            {
                                GenerateExpression(baseCtorInvoke.Arguments[i], writer);
                                if (i < j - 1)
                                {
                                    writer.Append(',');
                                }

                            }
                            writer.Append(")");
                        }
                    }
                    break;
                case CodeExpressionKind.ThisCtorInvoke:
                    {
                        CodeBaseCtorInvoke baseCtorInvoke = (CodeBaseCtorInvoke)codeExpression;
                        int j = baseCtorInvoke.ParameterCount;
                        if (j == 0)
                        {
                            writer.Append("this::.ctor()");
                        }
                        else
                        {
                            writer.Append("this::.ctor(");
                            for (int i = 0; i < j; i++)
                            {
                                GenerateExpression(baseCtorInvoke.Arguments[i], writer);
                                if (i < j - 1)
                                {
                                    writer.Append(',');
                                }

                            }
                            writer.Append(")");
                        }
                    }
                    break;
                case CodeExpressionKind.BaseRefererneceExpression:
                    {
                        writer.Append("base");

                    }
                    break;


                case CodeExpressionKind.BinaryOperatorExpression:
                    {

                        CodeBinaryOperatorExpression binOpExpr = (CodeBinaryOperatorExpression)codeExpression;
                        string exp2str = string.Empty;
                        string exp1str = string.Empty;
                        if (binOpExpr.LeftExpression != null)
                        {
                            GenerateExpression(binOpExpr.LeftExpression, writer);
                        }
                        writer.Append(' ');
                        writer.Append(binOpExpr.BinaryOp.ToString());
                        writer.Append(' ');
                        if (binOpExpr.RightExpression != null)
                        {
                            GenerateExpression(binOpExpr.RightExpression, writer);
                        }
                    }
                    break;
                case CodeExpressionKind.ExplicitConversionExpression:
                    {
                        CodeTypeConversionExpression castExpr = (CodeTypeConversionExpression)codeExpression;
                        writer.Append("(" + castExpr.TargetType.FullName + ")");
                        GenerateExpression(castExpr.CastedExpression, writer);
                    }
                    break;
                case CodeExpressionKind.AsConversionExpression:
                    {
                        CodeTypeConversionExpression castExpr = (CodeTypeConversionExpression)codeExpression;
                        GenerateExpression(castExpr.CastedExpression, writer);
                        writer.Append(" as ");
                        writer.Append(castExpr.TargetType.FullName);

                    }
                    break;

                case CodeExpressionKind.DelegateCreateExpression:
                    {
                        CodeDelegateCreateExpression delCreateExpr = (CodeDelegateCreateExpression)codeExpression;
                        writer.Append("new " + delCreateExpr.DelegateType.FullName + "(");
                        GenerateExpression(delCreateExpr.TargetObject, writer);
                        writer.Append('.');
                        writer.Append(delCreateExpr.MethodName + ")");
                    }
                    break;
                case CodeExpressionKind.DynamicListExpression:
                    {
                        CodeDynamicListExpression dynamicListExpr = (CodeDynamicListExpression)codeExpression;
                        writer.Append('[');
                        GenerateExpressionCollection(dynamicListExpr.MemberExpressionCollection, writer);
                        writer.Append(']');

                    }
                    break;
                case CodeExpressionKind.ParenthizedExpression:
                    {
                        CodeParenthizedExpression exprBlock = (CodeParenthizedExpression)codeExpression;
                        writer.Append('(');
                        if (exprBlock.ContentExpression != null)
                        {
                            GenerateExpression(exprBlock.ContentExpression, writer);
                        }
                        writer.Append(')');
                    }
                    break;
                case CodeExpressionKind.JsonObjectExpression:
                    {
                        CodeJsonObjectExpression jsonObjExpr = (CodeJsonObjectExpression)codeExpression;
                        int j = jsonObjExpr.ContentCollection.Count;
                        int i = 0;
                        writer.Append('{');
                        foreach (CodeNamedExpression exp in jsonObjExpr.ContentCollection)
                        {
                            GenerateExpression(exp, writer);
                            if (i < j - 1)
                            {
                                writer.Append(',');
                            }
                            i++;
                        }
                        writer.Append('}');

                    }
                    break;
                case CodeExpressionKind.IdExpression:
                    {
                        CodeIdExpression idExpr = (CodeIdExpression)codeExpression;
                        writer.Append(idExpr.SimpleName.FullName);

                    }
                    break;
                case CodeExpressionKind.IncDecOperatorExpression:
                    {

                        CodeIncrementDecrementOperatorExpression incDecrOpExpression = (CodeIncrementDecrementOperatorExpression)codeExpression;
                        GenerateExpression(incDecrOpExpression.TargetExpression, writer);
                        writer.Append(incDecrOpExpression.Operator.ToString());
                    }
                    break;
                case CodeExpressionKind.LambdaExpression:
                    {
                        CodeLambdaExpression lambdaExpr = (CodeLambdaExpression)codeExpression;
                        if (lambdaExpr.ParameterList == null)
                        {
                            writer.Append("()=>");
                        }
                        else
                        {
                            writer.Append('(');
                            int i = 0;
                            int j = lambdaExpr.ParameterList.Count;
                            foreach (CodeParameterDeclarationExpression par in lambdaExpr.ParameterList)
                            {
                                GenerateExpression(par, writer);
                                i++;
                                if (i != j)
                                {
                                    writer.Append(',');
                                }
                            }
                            writer.Append(")=>");
                        }
                        if (lambdaExpr.SingleExpression != null)
                        {
                            GenerateExpression(lambdaExpr.SingleExpression, writer);
                        }
                        else if (lambdaExpr.MethodBody != null)
                        {

                            WriteStatementBlock(lambdaExpr.MethodBody, writer);

                        }
                        else
                        {
                            writer.Append("{}");
                        }
                    }
                    break;
                case CodeExpressionKind.LinqQueryExpression:
                    {
                        //not support linq yet


                    }
                    break;
                case CodeExpressionKind.MeReferenceExpression:
                    {

                        writer.Append("me");
                    }
                    break;
                case CodeExpressionKind.MemberAccessExpression:
                    {
                        CodeMemberAccessExpression mbAccessExpr = (CodeMemberAccessExpression)codeExpression;
                        if (!(mbAccessExpr.TargetExpression is CodeImplicitThisReferenceExpression))
                        {
                            GenerateExpression(mbAccessExpr.TargetExpression, writer);
                            writer.Append('.');
                            GenerateExpression(mbAccessExpr.MemberExpression, writer);

                        }
                        else
                        {
                            GenerateExpression(mbAccessExpr.MemberExpression, writer);
                        }
                    }
                    break;
                case CodeExpressionKind.MethodInvokeExpression:
                    {
                        CodeMethodInvokeExpression methodInvokeEpxr = (CodeMethodInvokeExpression)codeExpression;
                        GenerateExpression(methodInvokeEpxr.Method, writer);
                        writer.Append('(');
                        GenerateExpressionCollection(methodInvokeEpxr.Arguments, writer);
                        writer.Append(')');

                    }
                    break;
                case CodeExpressionKind.MethodReferenceExpression:
                    {

                        CodeMethodReferenceExpression methodRefExpr = (CodeMethodReferenceExpression)codeExpression;
                        if (!(methodRefExpr.Target is CodeImplicitThisReferenceExpression))
                        {
                            GenerateExpression(methodRefExpr.Target, writer);
                            writer.Append('.');
                        }
                        writer.Append(methodRefExpr.MethodNameAsString);

                    }
                    break;
                case CodeExpressionKind.NamedExpression:
                    {
                        CodeNamedExpression namedExpression = (CodeNamedExpression)codeExpression;
                        writer.Append(namedExpression.NameAsString + ":");
                        GenerateExpression(namedExpression.Expression, writer);

                    }
                    break;
                case CodeExpressionKind.NullExpression:
                    {
                        writer.Append("null");
                    }
                    break;
                case CodeExpressionKind.ObjectCreateExpression:
                    {
                        CodeObjectCreateExpression objCreateExpr = (CodeObjectCreateExpression)codeExpression;
                        writer.Append("new ");
                        writer.Append(objCreateExpr.ObjectType.FullName);
                        writer.Append('(');
                        GenerateExpressionCollection(objCreateExpr.Arguments, writer);
                        writer.Append(')');
                    }
                    break;
                case CodeExpressionKind.ParameterDeclarationExpression:
                    {
                        CodeParameterDeclarationExpression parDeclExpr = (CodeParameterDeclarationExpression)codeExpression;
                        if (parDeclExpr.ParameterType != null)
                        {
                            writer.Append(parDeclExpr.ParameterType.FullName);
                        }
                        writer.Append(' ');
                        writer.Append(parDeclExpr.ParameterNameAsString);

                    }
                    break;
                case CodeExpressionKind.DefineFieldDeclarationExpression:
                    {
                        CodeDefineFieldDeclarationExpression defineField = (CodeDefineFieldDeclarationExpression)codeExpression;
                        writer.Append(defineField.FieldType.FullName);
                        if (defineField.MayBeMultipleFields)
                        {
                            writer.Append('%');
                        }
                        writer.Append(' ');
                        writer.Append(defineField.FieldNameAsString);
                    }
                    break;
                case CodeExpressionKind.UnaryOperatorExpression:
                    {
                        CodeUnaryOperatorExpression preUnaryOpExpr = (CodeUnaryOperatorExpression)codeExpression;
                        writer.Append(preUnaryOpExpr.BinaryOp.ToString());
                        GenerateExpression(preUnaryOpExpr.RightExpression, writer);

                    }
                    break;
                case CodeExpressionKind.PrimitiveExpression:
                    {
                        CodePrimitiveExpression primExpr = (CodePrimitiveExpression)codeExpression;
                        writer.Append(primExpr.Value);

                    }
                    break;
                case CodeExpressionKind.DecorationExpression:
                    {
                        CodeDecorationExpression decorExpr = (CodeDecorationExpression)codeExpression;
                        GenerateExpression(decorExpr.TargetExpression, writer);
                        WriteStatementBlock(decorExpr.DecorationStatements, writer);

                    }
                    break;
                case CodeExpressionKind.ThisReferenceExpression:
                    {
                        if (!(codeExpression is CodeImplicitThisReferenceExpression))
                        {

                            writer.Append("this");
                        }
                    }
                    break;
                case CodeExpressionKind.DirectionExpression:
                    {
                        CodeDirectionExpression codeDirectionExpr = (CodeDirectionExpression)codeExpression;
                        switch (codeDirectionExpr.FieldDirection)
                        {
                            case FieldDirection.In:
                                {
                                    GenerateExpression(codeDirectionExpr.Expression, writer);
                                }
                                break;
                            case FieldDirection.Out:
                                {
                                    writer.Append(" out ");
                                    GenerateExpression(codeDirectionExpr.Expression, writer);
                                }
                                break;
                            case FieldDirection.Ref:
                                {
                                    writer.Append(" ref ");
                                    GenerateExpression(codeDirectionExpr.Expression, writer);
                                }
                                break;
                            default:
                                {
                                    throw new NotSupportedException();
                                }

                        }
                    }
                    break;
                default:
                    {
                        throw new NotSupportedException();

                    }
            }
        }


    }


}