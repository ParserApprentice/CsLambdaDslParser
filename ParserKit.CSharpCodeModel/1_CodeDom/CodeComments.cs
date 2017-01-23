//MIT 2015-2017, ParserApprentice 
using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.CodeDom
{
     
    public class CodeCommentCollection : CodeObjectCollection<CodeComment>
    {
        public override CodeObjectKind Kind
        {
            get { return CodeObjectKind.CodeCommentCollection; }
        }
        public override CodeObjectRoles CollectionMemberFunction
        {
            get { return CodeObjectRoles.CodeCommentCollection_CommentObject; }
        }
    } 
     
    public class CodeComment : CodeObject
    {
        string text;
        bool docComment;
        
        public CodeComment(string text) 
        {
            this.text = text;
        }
        public CodeComment(string text, bool docComment) 
        {
            this.text = text;
            this.docComment = docComment;
        }

        public override CodeObjectKind Kind
        {
            get { return CodeObjectKind.Comment; }
        }
        public bool DocComment
        {
            get
            {
                return docComment;
            }
        }
        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                text = value;
            }
        }  

#if DEBUG
        public override string ToString()
        {
            return text;
        }
#endif
    } 
      
    public class CodeCommentStatement : CodeStatement
    {   
        CodeComment codeComment;
        public CodeCommentStatement(CodeComment comment)
        {
            this.codeComment = comment;
            AcceptChild(comment, CodeObjectRoles.CodeCommentStatement_CommentObject);
        } 
        public CodeComment Comment
        {
            get
            {
                return codeComment;
            }
            set
            {
                codeComment = value;
                if (value != null)
                {
                    AcceptChild(value, CodeObjectRoles.CodeCommentStatement_CommentObject);
                } 
            }
        } 
        public override CodeStatementType StatementType
        {
            get
            {
                return CodeStatementType.CodeCommentStatement;
            }
        } 
    } 
   

}