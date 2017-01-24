//MIT, 2015-2017, ParserApprentice
using System;
using System.Text;
using System.Collections.Generic;

namespace Parser.ParserKit
{
    public enum VisitStyle
    {
        Unknown,
        WalkBottomUp,
        WalkTopDown
    }
    public delegate void NTNodeVisitEventHandle(NonTerminalParseNode nt);
    public delegate void TokenVisitEventHandler(Token nt);

//    public class ParseNodeWalker
//    {

//#if DEBUG
//        List<NonTerminalParseNode> ntStacks = new List<NonTerminalParseNode>();
//#endif
//        NTNodeVisitEventHandle NtNodeVisit;
//        TokenVisitEventHandler TokenNodeVisit;
//        bool hasTokenVisit = false;
//        VisitStyle visitStyle;
//        public ParseNodeWalker(NTNodeVisitEventHandle ntNodeVisit, bool bottomUp)
//        {

//            this.NtNodeVisit = ntNodeVisit;
//            this.WalkBottomUp = bottomUp;
//            if (bottomUp)
//            {
//                this.visitStyle = VisitStyle.WalkBottomUp;
//            }
//            else
//            {
//                this.visitStyle = VisitStyle.WalkTopDown;
//            }
//        }
//        public ParseNodeWalker(TokenVisitEventHandler tkvisitHanlder)
//        {
//            this.TokenNodeVisit = tkvisitHanlder;
//            this.hasTokenVisit = (tkvisitHanlder != null);
//        }
//        public bool WalkBottomUp
//        {
//            get;
//            private set;
//        }
//        public bool HasTokenVisitHandler
//        {
//            get { return hasTokenVisit; }
//        }

//        public virtual void Enter(NonTerminalParseNode ntnode)
//        {
//#if DEBUG
//            //ntStacks.Add(ntnode); //push
//#endif
//            switch (this.visitStyle)
//            {
//                case VisitStyle.WalkTopDown:
//                    {
//                        NtNodeVisit(ntnode);

//                    } break;
//            }
//            //if (!this.WalkBottomUp && NtNodeVisit != null)
//            //{
//            //    
//            //    NtNodeVisit(ntnode);
//            //}
//        }
//        public virtual void Exit(NonTerminalParseNode ntnode)
//        {

//#if DEBUG
//            // ntStacks.RemoveAt(ntStacks.Count - 1);//pop
//#endif
//            switch (this.visitStyle)
//            {
//                case VisitStyle.WalkBottomUp:
//                    {
//                        NtNodeVisit(ntnode);
//                    } break;
//            }
//            //if (this.WalkBottomUp && NtNodeVisit != null)
//            //{  
//            //    NtNodeVisit(ntnode);
//            //}
//        }
//        public virtual void HitToken(Token token)
//        {
//            if (TokenNodeVisit != null)
//            {
//                TokenNodeVisit(token);
//            }
//        }
//        public void Walk(NonTerminalParseNode nt)
//        {
//            bool hasTokenVisit = false;
//            this.Enter(nt); 

//            ParseNode cnode = nt.FirstChild;
//            while (cnode != null)
//            {
//                if (!cnode.IsTerminalNode)
//                {
//                    this.Walk((NonTerminalParseNode)cnode);
//                }
//                else if (hasTokenVisit)
//                {
//                    //if is token
//                    this.HitToken((Token)cnode);
//                }
//                cnode = cnode.NextSibling;
//            }

             
//            //------------------------------------------------- 
//            this.Exit(nt);
//        }
//    }
}