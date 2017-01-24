//MIT, 2015-2017, ParserApprentice
using System;
using System.Collections.Generic;
using System.Text;

using Parser.CodeDom;


namespace Parser.AsmInfrastructures
{

    public class ParserReportMsg : BuildMessage
    {
        readonly ICodeParseNode token;
        CompilationUnit cu;
        int lineNum;
        int columnNum;
        public ParserReportMsg(CompilationUnit cu,
             CodeErrorDescription errDescription,
             ICodeParseNode token,
             int lineNum, int columnNum)
            : base(errDescription)
        {
            this.token = token;
            this.cu = cu;
            this.lineNum = lineNum;
            this.columnNum = columnNum;
        }
        public ParserReportMsg(CompilationUnit cu, CodeErrorDescription errDescription,
             ICodeParseNode token, int lineNum, int columnNum, string additionalInfo)
            : base(errDescription, additionalInfo)
        {
            this.token = token;
            this.cu = cu;
            this.lineNum = lineNum;
            this.columnNum = columnNum;
        }
        public override string FileName
        {
            get { return cu.Filename; }
        }
        public override int ColumnNumber
        {
            get { return columnNum; }
        }
        public override int LineNumber
        {
            get { return lineNum; }
        }
    }
    public class SemanticReportMessage : BuildMessage
    {
        readonly CodeObject codeObject;
        CompilationUnit cu;
        CodeTypeReference typeref;
        ICodeParseNode codeSyntaxNode;
        public SemanticReportMessage(CompilationUnit cu, CodeErrorDescription errDescription, CodeObject codeObject)
            : base(errDescription)
        {
            this.codeObject = codeObject;
            this.cu = cu;

        }
        public SemanticReportMessage(CompilationUnit cu, CodeErrorDescription errDescription, CodeTypeReference typeref)
            : base(errDescription)
        {
            this.typeref = typeref;
            this.cu = cu;
        }
        public SemanticReportMessage(CompilationUnit cu, CodeErrorDescription errDescription, ICodeParseNode codeSyntaxNode)
            : base(errDescription)
        {
            this.codeSyntaxNode = codeSyntaxNode;
            this.cu = cu;
        }
        public override string FileName
        {
            get
            {
                if (codeObject != null)
                {
                    if (!codeObject.SourceLocation.IsEmpty)
                    {
                        return cu.Filename;
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
                else
                {
                    return string.Empty;
                }
            }
        }
        public override int ColumnNumber
        {
            get
            {
                if (codeObject != null)
                {
                    if (!codeObject.SourceLocation.IsEmpty)
                    {
                        return codeObject.SourceLocation.BeginColumnNumber;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    return 0;
                }
            }

        }
        public override int LineNumber
        {
            get
            {
                if (codeObject != null)
                {
                    return codeObject.SourceLocation.BeginLineNumber;

                }
                else
                {
                    return 0;
                }
            }
        }
    }
    public class ProjectSourceReportMessage : BuildMessage
    {
        public ProjectSourceReportMessage(CodeErrorDescription errDescription)
            : base(errDescription, null)
        {
        }
        public override int ColumnNumber
        {
            get { return 0; }
        }
        public override string FileName
        {
            get { return ""; }
        }
        public override int LineNumber
        {
            get { return 0; }
        }
    }

    public class CodeDomTransformMessage : BuildMessage
    {
        readonly CodeObject codeObject;
        CompilationUnit cu;
        public CodeDomTransformMessage(CompilationUnit cu, CodeErrorDescription errDescription, CodeObject codeObject)
            : base(errDescription)
        {
            this.codeObject = codeObject;
            this.cu = cu;
        }
        public override int ColumnNumber
        {
            get { return 0; }
        }
        public override string FileName
        {
            get { return cu.Filename; }
        }
        public override int LineNumber
        {
            get { return 0; }
        }
    }
}