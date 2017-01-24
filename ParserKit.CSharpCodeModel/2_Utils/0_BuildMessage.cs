//MIT, 2015-2017, ParserApprentice
using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.AsmInfrastructures
{


    public abstract class CodeErrorDescription
    {
        int code = -1;
        string description;
        public CodeErrorDescription(int code, string description)
        {
            this.code = code;
            this.description = description;
        }
        public int Code
        {
            get
            {
                return code;
            }
        }
        public string Description
        {
            get
            {
                return description;
            }
        }
        public override string ToString()
        {
            return "err: " + code + "," + description;
        }
    }


    public abstract class BuildMessage
    {

        CodeErrorDescription codeErrorDescription;
        string additionalInfo;
        public BuildMessage(CodeErrorDescription errDesc)
        {
            codeErrorDescription = errDesc;
        }
        public BuildMessage(CodeErrorDescription errDesc, string additionalInfo)
        {
            codeErrorDescription = errDesc;
            this.additionalInfo = additionalInfo;
        }
        public int Code
        {
            get
            {
                return codeErrorDescription.Code;
            }
        }
        public virtual string Description
        {
            get
            {
                return codeErrorDescription.Description;
            }
        }
        public string GetErrorMessage()
        {
            return this.FileName + "[" + this.LineNumber + "," + this.ColumnNumber + "]" + Code + "," + Description;
        }
        public override string ToString()
        {
            return GetErrorMessage();
        }
        public abstract string FileName
        {
            get;
        }
        public abstract int LineNumber
        {
            get;
        }
        public abstract int ColumnNumber
        {
            get;
        }
    }



}
