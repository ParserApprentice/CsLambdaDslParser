//MIT, 2015-2017, ParserApprentice
using System;
using System.Text;
using System.Collections.Generic;

namespace Parser.ParserKit
{
    public enum ParseNodeKind : byte
    {
        Tk,
        Nt,
        List,
    }

    /// <summary>
    /// base class of token node and nonterminal node
    /// </summary>
    public abstract class ParseNode
    {
        internal ParseNodeKind _kind;
        public ParseNode()
        {
        }
        public abstract bool IsTerminalNode
        {
            get;
        }

        internal bool IsList
        {
            get { return _kind == ParseNodeKind.List; }
        }
        internal void AddInheritUpSequence(SymbolSequence ss)
        {
            //NOTE!: collect only  seq that has listener
            //if (ss.HasReductionListener)
            //{
            //    this._latestReduction = new SeqReduction(_latestReduction, ss);
            //}
        }
        //public SeqReduction LatestReduction
        //{
        //    get { return this._latestReduction; }
        //} 
        public abstract LocationCodeArea GetLocation(ParseNodeLocator locator);
    }

    public class SeqReduction
    {
        public readonly SeqReduction _prevNode;
        public readonly SymbolSequence _symbolSeq;

        internal SeqReduction(SeqReduction prevNode, SymbolSequence ss)
        {
            _prevNode = prevNode;
            _symbolSeq = ss;
        }
    }


    public enum TrivialTokenKind
    {
        Unknown,
        CommentLine,
        CommentBlock
    }
    public class TrivialToken
    {
        TrivialTokenKind kind;
        internal TrivialToken next;
        string lexeme;
        public TrivialToken(TrivialTokenKind kind, string lexeme)
        {
            this.lexeme = lexeme;
            this.kind = kind;
        }
        public LocationCodeArea Location
        {
            get;
            set;
        }
    }


    public class Token : ParseNode
    {
        public readonly int startAt;
        internal readonly ushort _tokenNumber;
        ushort charCount;

        TokenDefinition tokenInfo;
        TrivialToken firstTrivialToken;
        object lexeme;

#if DEBUG
        static int dbugTotalId = 0;
        public int dbugTkId = dbugTotalId++;
#endif
        public Token(TokenDefinition tokenInfo, int startAt, ushort appendLength)
        {

#if DEBUG
            if (tokenInfo == null)
            {
                throw new NotSupportedException();
            }
#endif

            this.tokenInfo = tokenInfo;
            this.startAt = startAt;
            _tokenNumber = (ushort)tokenInfo.TokenInfoNumber;
            this.charCount = (ushort)appendLength;

        }


        //        public Token(TokenDefinition tokenInfo, string lexeme, int startAt, ushort appendLength)
        //        {

        //#if DEBUG
        //            if (tokenInfo == null)
        //            {
        //                throw new NotSupportedException();
        //            }
        //#endif

        //            this.tokenInfo = tokenInfo;
        //            this.startAt = startAt;
        //            this.lexeme = lexeme;
        //            _tokenNumber = (ushort)tokenInfo.TokenInfoNumber;
        //            this.charCount = (ushort)appendLength;

        //        }
        public Token(TokenDefinition tokenInfo, string str)
        {

#if DEBUG
            if (tokenInfo == null)
            {
                throw new NotSupportedException();
            }
#endif

            this.tokenInfo = tokenInfo;
            this.lexeme = str;
            _tokenNumber = (ushort)tokenInfo.TokenInfoNumber;
            this.charCount = (ushort)str.Length;

        }
        public Token(TokenDefinition tokenInfo, int startAt)
        {
            this.tokenInfo = tokenInfo;
            this.startAt = startAt;
            _tokenNumber = (ushort)tokenInfo.TokenInfoNumber;
        }
        public Token(TokenDefinition tokenInfo)
        {
            this.tokenInfo = tokenInfo;
            _tokenNumber = (ushort)tokenInfo.TokenInfoNumber;
        }
        public void AddTrivialToken(TrivialToken trivialToken)
        {
            if (firstTrivialToken == null)
            {
                firstTrivialToken = trivialToken;
            }
            else
            {
                //attach to latest
                TrivialToken tt = firstTrivialToken;
                while (tt.next != null)
                {
                    tt = tt.next;
                }
                //append last
                tt.next = trivialToken;
            }
        }
        public TrivialToken FirstTrivialToken
        {
            get { return firstTrivialToken; }
        }

        public override LocationCodeArea GetLocation(ParseNodeLocator locator)
        {
            int lineNo = locator.GetLineNumber(this.startAt);
            throw new NotSupportedException();
            return new LocationCodeArea();
        }

        public TokenDefinition TkInfo
        {
            get
            {
                return this.tokenInfo;
            }
        }
        public override bool IsTerminalNode
        {
            get
            {
                return true;
            }
        }
        public void ChangeTokenInfo(TokenDefinition tokenInfo)
        {
            this.tokenInfo = tokenInfo;
        }

        public int TokenCharacterCount
        {
            get
            {
                //return 0;
                return charCount;
            }
        }
        public string GetLexeme()
        {
            if (lexeme == null)
            {
                return TkInfo.PresentationString;
            }
            else
            {
                return (string)lexeme;
            }

        }
        internal object GetOriginalLexeme()
        {
            return lexeme;
        }

        public LocationCodeArea CodeArea
        {
            get
            {
                return new LocationCodeArea();
                //return new Parser.ParserKit.LocationCodeArea(this.BeginLine,
                //  (short)BeginColumn,
                //  this.BeginLine,
                //  (short)(BeginColumn + this.TokenCharacterCount));
            }
        }

        //public void SetupLocation(int lineNum, int colNum)
        //{
        //    //_beginLineNo = lineNum;
        //    //_beginColNo = (ushort)colNum;
        //}
        public override string ToString()
        {
            return this.GetLexeme();
        }

    }

    class EmptyParseNode : ParseNode
    {
        public EmptyParseNode()
        {
        }
        public override LocationCodeArea GetLocation(ParseNodeLocator locator)
        {
            return new LocationCodeArea();
        }
        public override bool IsTerminalNode
        {
            get { return false; }
        }
    }

    public class NonTerminalParseNodeList : NonTerminalParseNode
    {

        //ParseNode latestParseNode;
        List<ParseNode> nodes = new List<ParseNode>();
        List<ParseNode> sepNodes;
        public NonTerminalParseNodeList(SymbolSequence ss)
            : base(ss)
        {
            this._kind = ParseNodeKind.List;
        }
        public NonTerminalParseNodeList()
        {
            this._kind = ParseNodeKind.List;
        }
        public void AddParseNode(ParseNode p)
        {
            if (p.IsTerminalNode)
            {
                //Token.ReleaseToPool((Token)p);
                nodes.Add(null);
            }
            else
            {
                nodes.Add(p);
            }
        }
        public void AddSepNode(ParseNode p)
        {
            if (sepNodes == null)
            {
                sepNodes = new List<ParseNode>();
            }

            if (p.IsTerminalNode)
            {
                sepNodes.Add(p);
            }
            else
            {
                sepNodes.Add(p);
            }
        }
        public override PNode GetChild(int index)
        {
            throw new NotImplementedException();
        }
        public override string ToString()
        {
            StringBuilder stbuilder = new StringBuilder();
            int j = nodes.Count;
            if (sepNodes == null)
            {
                for (int i = 0; i < j; ++i)
                {
                    ParseNode p = nodes[i];
                    if (p != null)
                    {
                        stbuilder.Append(p.ToString());
                        stbuilder.Append(" ");
                    }
                }
            }
            else
            {
                int sepNodeCount = sepNodes.Count;
                for (int i = 0; i < j; ++i)
                {
                    ParseNode p = nodes[i];
                    if (p != null)
                    {
                        stbuilder.Append(p.ToString());
                        stbuilder.Append(" ");
                    }
                    if (i < sepNodeCount)
                    {
                        ParseNode sep = sepNodes[i];
                        if (sep != null)
                        {
                            stbuilder.Append(sep.ToString());
                            stbuilder.Append(" ");
                        }
                    }


                }


            }

            return stbuilder.ToString();
        }
        public override LocationCodeArea GetLocation(ParseNodeLocator locator)
        {
            throw new NotImplementedException();
        }
    }


    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit)]
    public struct PNode
    {

        [System.Runtime.InteropServices.FieldOffset(0)]
        public int startAt;

        [System.Runtime.InteropServices.FieldOffset(4)]
        public ParseNode ntNode;

        [System.Runtime.InteropServices.FieldOffset(4)]
        public TokenDefinition tkDef;

        [System.Runtime.InteropServices.FieldOffset(4)]
        public object otherInfo;

        public PNode(ParseNode ntNode)
        {
            this.startAt = 0;
            this.tkDef = null;
            this.otherInfo = null;
            this.ntNode = ntNode;
        }
        public override string ToString()
        {
            if (otherInfo != null)
            {
                return otherInfo.ToString();
            }
            return "";

        }
    }

    public abstract class NonTerminalParseNode : ParseNode
    {

#if DEBUG
        static int dbugTotalId;
        public readonly int dbugId = dbugTotalId++;
#endif

        internal NonTerminalParseNode(ParseNode firstNode)
        {
#if DEBUG
            //if (ss == null)
            //{
            //}
            //if (dbugId == 8699 || dbugId == 8759 || dbugId == 8789)
            //{

            //}
#endif

            _kind = ParseNodeKind.Nt;
        }
        internal NonTerminalParseNode()
        {
            _kind = ParseNodeKind.Nt;
        }
        internal NonTerminalParseNode(SymbolSequence ss)
        {
            _kind = ParseNodeKind.Nt;
            AddInheritUpSequence(ss);
        }
        public override bool IsTerminalNode
        {
            get
            {
                return false;
            }
        }
        public abstract PNode GetChild(int index);

        public override LocationCodeArea GetLocation(ParseNodeLocator locator)
        {
            throw new NotImplementedException();
        }
    }



    public class NTAstNode : NonTerminalParseNode
    {
        //for hold ast nodeb
        object astObject;
        public NTAstNode(object astObject)
        {
            this.astObject = astObject;
        }
        public override PNode GetChild(int index)
        {
            //no child node ?
            throw new NotImplementedException();
        }
        public override string ToString()
        {
            return astObject.ToString();
        }
        public override LocationCodeArea GetLocation(ParseNodeLocator locator)
        {
            throw new NotImplementedException();
        }
    }


    public class NTn1 : NonTerminalParseNode
    {
        public PNode n1;
        public NTn1(ParseNode n1)
        {
            this.n1 = new PNode(n1);
        }
        public NTn1() { }
        public override PNode GetChild(int index)
        {
            switch (index)
            {
                case 0: return n1;
                default: return new PNode();
            }
        }
        public override LocationCodeArea GetLocation(ParseNodeLocator locator)
        {
            throw new NotSupportedException();
        }
    }

    public class NTn2 : NonTerminalParseNode
    {
        public PNode n1;
        public PNode n2;
        public NTn2(ParseNode n1, ParseNode n2)
        {
            this.n1 = new PNode(n1);
            this.n2 = new PNode(n2);
        }
        public NTn2() { }
        public override PNode GetChild(int index)
        {
            switch (index)
            {
                case 0: return n1;
                case 1: return n2;
                default: return new PNode();
            }
        }
        public override string ToString()
        {
            return n1.ToString() + " " + n2.ToString();
        }

        public override LocationCodeArea GetLocation(ParseNodeLocator locator)
        {
            return new LocationCodeArea();
            //LocationCodeArea loca1 = new LocationCodeArea();
            //if (n1.ntNode is NonTerminalParseNode)
            //{
            //    loca1 = ((NonTerminalParseNode)n1.ntNode).GetLocation(locator);
            //}
            //int lineNo1 = locator.GetLineNumber(n1.startAt);
            //int lineNo2 = locator.GetLineNumber(n2.startAt);

            //throw new NotSupportedException();
        }
    }
    public class NTn3 : NonTerminalParseNode
    {
        public PNode n1;
        public PNode n2;
        public PNode n3;
        public NTn3(ParseNode n1, ParseNode n2, ParseNode n3)
        {

            this.n1 = new PNode(n1);
            this.n2 = new PNode(n2);
            this.n3 = new PNode(n3);
        }
        public NTn3() { }
        public override PNode GetChild(int index)
        {
            switch (index)
            {
                case 0: return n1;
                case 1: return n2;
                case 2: return n3;
                default: return new PNode();
            }
        }
        public override string ToString()
        {
            return n1.ToString() + " " + n2.ToString() + " " + n3.ToString();
        }
        public override LocationCodeArea GetLocation(ParseNodeLocator locator)
        {
            LocationCodeArea loca1 = new LocationCodeArea();
            int lineStart = 0;
            if (n1.ntNode is NonTerminalParseNode)
            {
                loca1 = ((NonTerminalParseNode)n1.ntNode).GetLocation(locator);
                lineStart = loca1.BeginLineNumber;
            }
            else if (n1.tkDef is TokenDefinition)
            {
                //get token 
                lineStart = locator.GetLineNumber(n1.startAt);
            }
            else
            {

            }
            return loca1;
        }

    }

    public class NTn4 : NonTerminalParseNode
    {
        public PNode n1;
        public PNode n2;
        public PNode n3;
        public PNode n4;
        public NTn4(ParseNode n1, ParseNode n2, ParseNode n3, ParseNode n4)
        {
            this.n1 = new PNode(n1);
            this.n2 = new PNode(n2);
            this.n3 = new PNode(n3);
            this.n4 = new PNode(n4);
        }
        public NTn4() { }
        public override PNode GetChild(int index)
        {
            switch (index)
            {
                case 0: return n1;
                case 1: return n2;
                case 2: return n3;
                case 3: return n4;
                default: return new PNode();
            }
        }
        public override string ToString()
        {
            return n1.ToString() + " " + n2.ToString()
                + " " + n3.ToString()
                + " " + n4.ToString();
        }
    }

    public class NTn5 : NonTerminalParseNode
    {
        public PNode n1;
        public PNode n2;
        public PNode n3;
        public PNode n4;
        public PNode n5;
        public NTn5(ParseNode n1, ParseNode n2, ParseNode n3, ParseNode n4, ParseNode n5)
        {
            this.n1 = new PNode(n1);
            this.n2 = new PNode(n2);
            this.n3 = new PNode(n3);
            this.n4 = new PNode(n4);
            this.n5 = new PNode(n5);
        }
        public NTn5() { }
        public override PNode GetChild(int index)
        {
            switch (index)
            {
                case 0: return n1;
                case 1: return n2;
                case 2: return n3;
                case 3: return n4;
                case 4: return n5;
                default: return new PNode();
            }
        }
        public override string ToString()
        {
            return n1.ToString() + " " + n2.ToString()
                + " " + n3.ToString()
                + " " + n4.ToString() + " " +
                n5.ToString();

        }
    }
    public class NTn6 : NonTerminalParseNode
    {
        public PNode n1;
        public PNode n2;
        public PNode n3;
        public PNode n4;
        public PNode n5;
        public PNode n6;

        public NTn6(ParseNode n1, ParseNode n2, ParseNode n3, ParseNode n4, ParseNode n5, ParseNode n6)
        {
            this.n1 = new PNode(n1);
            this.n2 = new PNode(n2);
            this.n3 = new PNode(n3);
            this.n4 = new PNode(n4);
            this.n5 = new PNode(n5);
            this.n6 = new PNode(n6);
        }
        public NTn6() { }
        public override PNode GetChild(int index)
        {
            switch (index)
            {
                case 0: return n1;
                case 1: return n2;
                case 2: return n3;
                case 3: return n4;
                case 4: return n5;
                case 5: return n6;
                default: return new PNode();
            }
        }
        public override string ToString()
        {
            return n1.ToString() + " " + n2.ToString()
                + " " + n3.ToString()
                + " " + n4.ToString() + " " +
                n5.ToString() + " " +
                n6.ToString();

        }
    }
    public class NTn7 : NonTerminalParseNode
    {
        public PNode n1;
        public PNode n2;
        public PNode n3;
        public PNode n4;
        public PNode n5;
        public PNode n6;
        public PNode n7;

        public NTn7(ParseNode n1, ParseNode n2, ParseNode n3, ParseNode n4, ParseNode n5, ParseNode n6, ParseNode n7)
        {
            this.n1 = new PNode(n1);
            this.n2 = new PNode(n2);
            this.n3 = new PNode(n3);
            this.n4 = new PNode(n4);
            this.n5 = new PNode(n5);
            this.n6 = new PNode(n6);
            this.n7 = new PNode(n7);
        }
        public NTn7() { }
        public override PNode GetChild(int index)
        {
            switch (index)
            {
                case 0: return n1;
                case 1: return n2;
                case 2: return n3;
                case 3: return n4;
                case 4: return n5;
                case 5: return n6;
                case 6: return n7;
                default: return new PNode();
            }
        }
        public override string ToString()
        {
            return n1.ToString() + " " + n2.ToString()
           + " " + n3.ToString()
           + " " + n4.ToString() + " " +
           n5.ToString() + " " +
           n6.ToString() + " " +
           n7.ToString();
        }
    }
    public class NTn8 : NonTerminalParseNode
    {
        public PNode n1;
        public PNode n2;
        public PNode n3;
        public PNode n4;
        public PNode n5;
        public PNode n6;
        public PNode n7;
        public PNode n8;
        public NTn8(ParseNode n1, ParseNode n2, ParseNode n3, ParseNode n4, ParseNode n5, ParseNode n6, ParseNode n7, ParseNode n8)
        {
            this.n1 = new PNode(n1);
            this.n2 = new PNode(n2);
            this.n3 = new PNode(n3);
            this.n4 = new PNode(n4);
            this.n5 = new PNode(n5);
            this.n6 = new PNode(n6);
            this.n7 = new PNode(n7);
            this.n8 = new PNode(n8);
        }
        public NTn8() { }
        public override PNode GetChild(int index)
        {
            switch (index)
            {
                case 0: return n1;
                case 1: return n2;
                case 2: return n3;
                case 3: return n4;
                case 4: return n5;
                case 5: return n6;
                case 6: return n7;
                case 7: return n8;
                default: return new PNode();
            }
        }
        public override string ToString()
        {
            return n1.ToString() + " " + n2.ToString()
             + " " + n3.ToString()
             + " " + n4.ToString() + " " +
             n5.ToString() + " " +
             n6.ToString() + " " +
             n7.ToString() + " " + n8.ToString();
        }
    }

    public class NTn9 : NonTerminalParseNode
    {
        public PNode n1;
        public PNode n2;
        public PNode n3;
        public PNode n4;
        public PNode n5;
        public PNode n6;
        public PNode n7;
        public PNode n8;
        public PNode n9;
        public NTn9(ParseNode n1, ParseNode n2, ParseNode n3, ParseNode n4, ParseNode n5, ParseNode n6, ParseNode n7, ParseNode n8, ParseNode n9)
        {
            this.n1 = new PNode(n1);
            this.n2 = new PNode(n2);
            this.n3 = new PNode(n3);
            this.n4 = new PNode(n4);
            this.n5 = new PNode(n5);
            this.n6 = new PNode(n6);
            this.n7 = new PNode(n7);
            this.n8 = new PNode(n8);
            this.n9 = new PNode(n9);
        }
        public NTn9() { }
        public override PNode GetChild(int index)
        {
            switch (index)
            {
                case 0: return n1;
                case 1: return n2;
                case 2: return n3;
                case 3: return n4;
                case 4: return n5;
                case 5: return n6;
                case 6: return n7;
                case 7: return n8;
                case 8: return n9;
                default: return new PNode();
            }
        }
        public override string ToString()
        {

            return n1.ToString() + " " + n2.ToString()
             + " " + n3.ToString()
             + " " + n4.ToString() + " " +
             n5.ToString() + " " +
             n6.ToString() + " " +
             n7.ToString() + " " + n8.ToString() + " " + n9.ToString();

        }
    }
    public class NTn10 : NonTerminalParseNode
    {
        public PNode n1;
        public PNode n2;
        public PNode n3;
        public PNode n4;
        public PNode n5;
        public PNode n6;
        public PNode n7;
        public PNode n8;
        public PNode n9;
        public PNode n10;
        public NTn10(ParseNode n1, ParseNode n2, ParseNode n3, ParseNode n4,
            ParseNode n5, ParseNode n6, ParseNode n7, ParseNode n8, ParseNode n9, ParseNode n10)
        {
            this.n1 = new PNode(n1);
            this.n2 = new PNode(n2);
            this.n3 = new PNode(n3);
            this.n4 = new PNode(n4);
            this.n5 = new PNode(n5);
            this.n6 = new PNode(n6);
            this.n7 = new PNode(n7);
            this.n8 = new PNode(n8);
            this.n9 = new PNode(n9);
            this.n10 = new PNode(n10);
        }
        public NTn10() { }
        public override PNode GetChild(int index)
        {
            switch (index)
            {
                case 0: return n1;
                case 1: return n2;
                case 2: return n3;
                case 3: return n4;
                case 4: return n5;
                case 5: return n6;
                case 6: return n7;
                case 7: return n8;
                case 8: return n9;
                case 9: return n10;
                default: return new PNode();
            }
        }
        public override string ToString()
        {
            return n1.ToString() + " " + n2.ToString()
            + " " + n3.ToString()
            + " " + n4.ToString() + " " +
            n5.ToString() + " " +
            n6.ToString() + " " +
            n7.ToString() + " " + n8.ToString() + " " + n9.ToString() +
            n10.ToString();

        }
    }
    public class NTn11 : NonTerminalParseNode
    {
        public PNode n1;
        public PNode n2;
        public PNode n3;
        public PNode n4;
        public PNode n5;
        public PNode n6;
        public PNode n7;
        public PNode n8;
        public PNode n9;
        public PNode n10;
        public PNode n11;

        public NTn11(ParseNode n1, ParseNode n2, ParseNode n3, ParseNode n4, ParseNode n5, ParseNode n6, ParseNode n7, ParseNode n8, ParseNode n9, ParseNode n10, ParseNode n11)
        {
            this.n1 = new PNode(n1);
            this.n2 = new PNode(n2);
            this.n3 = new PNode(n3);
            this.n4 = new PNode(n4);
            this.n5 = new PNode(n5);
            this.n6 = new PNode(n6);
            this.n7 = new PNode(n7);
            this.n8 = new PNode(n8);
            this.n9 = new PNode(n9);
            this.n10 = new PNode(n10);
            this.n11 = new PNode(n11);
        }
        public NTn11() { }
        public override PNode GetChild(int index)
        {
            switch (index)
            {
                case 0: return n1;
                case 1: return n2;
                case 2: return n3;
                case 3: return n4;
                case 4: return n5;
                case 5: return n6;
                case 6: return n7;
                case 7: return n8;
                case 8: return n9;
                case 9: return n10;
                case 10: return n11;
                default: return new PNode();
            }
        }
        public override string ToString()
        {
            return n1.ToString() + " " + n2.ToString()
          + " " + n3.ToString()
          + " " + n4.ToString() + " " +
          n5.ToString() + " " +
          n6.ToString() + " " +
          n7.ToString() + " " + n8.ToString() + " " + n9.ToString() +
          n10.ToString() + " " + n11.ToString();
        }
    }
    public class NTn12 : NonTerminalParseNode
    {
        public PNode n1;
        public PNode n2;
        public PNode n3;
        public PNode n4;
        public PNode n5;
        public PNode n6;
        public PNode n7;
        public PNode n8;
        public PNode n9;
        public PNode n10;
        public PNode n11;
        public PNode n12;

        public NTn12(ParseNode n1, ParseNode n2, ParseNode n3, ParseNode n4, ParseNode n5, ParseNode n6, ParseNode n7, ParseNode n8, ParseNode n9, ParseNode n10, ParseNode n11, ParseNode n12)
        {
            this.n1 = new PNode(n1);
            this.n2 = new PNode(n2);
            this.n3 = new PNode(n3);
            this.n4 = new PNode(n4);
            this.n5 = new PNode(n5);
            this.n6 = new PNode(n6);
            this.n7 = new PNode(n7);
            this.n8 = new PNode(n8);
            this.n9 = new PNode(n9);
            this.n10 = new PNode(n10);
            this.n11 = new PNode(n11);
            this.n12 = new PNode(n12);
        }
        public NTn12() { }
        public override PNode GetChild(int index)
        {
            switch (index)
            {
                case 0: return n1;
                case 1: return n2;
                case 2: return n3;
                case 3: return n4;
                case 4: return n5;
                case 5: return n6;
                case 6: return n7;
                case 7: return n8;
                case 8: return n9;
                case 9: return n10;
                case 10: return n11;
                case 11: return n12;
                default: return new PNode();
            }
        }
    }
    public class NTn13 : NonTerminalParseNode
    {
        public PNode n1;
        public PNode n2;
        public PNode n3;
        public PNode n4;
        public PNode n5;
        public PNode n6;
        public PNode n7;
        public PNode n8;
        public PNode n9;
        public PNode n10;
        public PNode n11;
        public PNode n12;
        public PNode n13;

        public NTn13(ParseNode n1, ParseNode n2, ParseNode n3, ParseNode n4, ParseNode n5, ParseNode n6, ParseNode n7,
            ParseNode n8, ParseNode n9, ParseNode n10, ParseNode n11, ParseNode n12, ParseNode n13)
        {
            this.n1 = new PNode(n1);
            this.n2 = new PNode(n2);
            this.n3 = new PNode(n3);
            this.n4 = new PNode(n4);
            this.n5 = new PNode(n5);
            this.n6 = new PNode(n6);
            this.n7 = new PNode(n7);
            this.n8 = new PNode(n8);
            this.n9 = new PNode(n9);
            this.n10 = new PNode(n10);
            this.n11 = new PNode(n11);
            this.n12 = new PNode(n12);
            this.n13 = new PNode(n13);
        }
        public NTn13() { }
        public override PNode GetChild(int index)
        {
            switch (index)
            {
                case 0: return n1;
                case 1: return n2;
                case 2: return n3;
                case 3: return n4;
                case 4: return n5;
                case 5: return n6;
                case 6: return n7;
                case 7: return n8;
                case 8: return n9;
                case 9: return n10;
                case 10: return n11;
                case 11: return n12;
                case 12: return n13;
                default: return new PNode();
            }
        }
    }
    public class NTnN : NonTerminalParseNode
    {
        public readonly PNode[] nodes;
        public NTnN(int n)
        {
            nodes = new PNode[n];
        }
        public override PNode GetChild(int index)
        {
            return nodes[index];
        }
    }



    public struct LocationCodeArea
    {
        public static readonly LocationCodeArea Empty = new LocationCodeArea();

        int locFlags;
        int beginLineNumber;
        int endLineNumber;
        short beginColumnNumber;
        short endColumnNumber;


        public LocationCodeArea(int beginLineNumber, short beginColumnNumber,
            int endLineNumber, short endColumnNumber)
        {
            this.locFlags = 1;
            this.beginLineNumber = beginLineNumber;
            this.beginColumnNumber = beginColumnNumber;
            this.endLineNumber = endLineNumber;
            this.endColumnNumber = endColumnNumber;
        }
        public bool IsEmpty
        {
            get
            {
                return locFlags == 0;
            }
        }


        public LocationCodeArea Combine(LocationCodeArea another)
        {
            if (another.IsEmpty)
            {

                return this;
            }
            else
            {
                if (this.IsEmpty)
                {
                    return another;
                }


                int finalBeginLineNum = this.beginLineNumber;
                short finalBeginCol = this.beginColumnNumber;

                int finalEndLineNum = this.endLineNumber;
                short finalEndCol = this.endColumnNumber;

                //1. at start point 

                if (another.beginLineNumber < finalBeginLineNum)
                {

                    finalBeginLineNum = another.beginLineNumber;
                    finalBeginCol = another.beginColumnNumber;
                }
                else if (another.beginLineNumber == finalBeginLineNum)
                {

                    if (another.beginColumnNumber < finalBeginCol)
                    {
                        finalBeginCol = another.beginColumnNumber;
                    }
                }
                else
                {


                }
                //2. end point 
                if (another.endLineNumber > finalEndLineNum)
                {
                    finalEndLineNum = another.endLineNumber;
                    finalEndCol = another.endColumnNumber;
                }
                else if (another.endLineNumber == finalEndLineNum)
                {
                    if (another.endColumnNumber > finalEndCol)
                    {
                        finalEndCol = another.endColumnNumber;
                    }
                }
                else
                {
                }
                return new LocationCodeArea(finalBeginLineNum, finalBeginCol, finalEndLineNum, finalEndCol);
            }
        }
        public override string ToString()
        {
            return GetSourceLocationString();
        }

        public string GetSourceLocationString()
        {
            return "(" + beginLineNumber + "," + beginColumnNumber + ") , (" +
               endLineNumber + "," + endColumnNumber + ")";
        }
        public int BeginLineNumber
        {
            get
            {
                return beginLineNumber;
            }
        }
        public int BeginColumnNumber
        {
            get
            {
                return beginColumnNumber;
            }
        }
        public int EndLineNumber
        {
            get
            {
                return endLineNumber;
            }
        }
        public int EndColumnNumber
        {
            get
            {
                return endColumnNumber;
            }
        }
    }


    public class CodeText
    {
        char[] sourceBuffer;
        List<int> linePosList = new List<int>();
        public CodeText(char[] sourceBuffer)
        {
            linePosList.Add(0);
            this.sourceBuffer = sourceBuffer;
        }
        public List<int> LinePosList
        {
            get { return linePosList; }
            set { linePosList = value; }
        }
        public char[] SourceBuffer
        {
            get
            {
                return sourceBuffer;
            }

        }
    }
    public class ParseNodeLocator
    {
        CodeText codeText;
        List<int> linePosList;
        public ParseNodeLocator(CodeText codeText)
        {
            this.codeText = codeText;
            this.linePosList = codeText.LinePosList;
        }
        public int GetLineNumber(int charIndex)
        {

            //search line location
            //do binary search 
            int j = linePosList.Count;
            int lineNo;
            if (DoBinarySearch(linePosList, 0, j - 1, charIndex, out lineNo))
            {
                return lineNo;
            }
            return -1;//not found
        }
        static bool DoBinarySearch(List<int> linePosList, int start, int end, int value, out int lineNo)
        {
            if (value > end)
            {
                lineNo = -1;
                return false;
            }
            else if (value < start)
            {
                lineNo = -1;
                return false;
            }
            int midpos = (end + start) / 2;
            int midValue = linePosList[midpos];
            int startValue = linePosList[start];
            int endValue = linePosList[end];

            if (value == midValue)
            {
                lineNo = start;
                return true;
            }
            else if (value == startValue)
            {
                lineNo = start;
                return true;
            }
            else if (value == endValue)
            {
                lineNo = end;
                return true;
            }
            else if (value < midValue)
            {
                //search a part
                if (DoBinarySearch(linePosList, start, end / 2, value, out lineNo))
                {
                    //recursive
                    return true;
                }
                return false;
            }
            else
            {
                if (DoBinarySearch(linePosList, end / 2, end, value, out lineNo))
                {
                    //recursive
                    return true;
                }
                return false;
            }
        }
    }
    public struct TextSpan
    {

    }

}