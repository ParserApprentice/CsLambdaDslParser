//MIT, 2015-2017, ParserApprentice
using System;
using System.Text;
using System.Collections.Generic;

namespace Parser.ParserKit
{


    public interface ISymbolDefinition
    {
        bool IsNT
        {
            get;
        }
        string SymbolName
        {
            get;
        }
    }


    public enum BasicTokenKind : byte
    {
        Unknown,
        Identifier,
        Keyword,
        ContextualKeyword,
        WhiteSpace,
        LineBreak,
        Terminal,
        LiteralString,
        LiteralNumber,
        Comment
    }

    public class TokenStream
    {
        CodeText codeText;
        public const int BUFF_LEN = 1024;
        protected int length;
        protected int currentPosInPage = -1;
        protected Token[] tokens = null;
        List<int> newLinePostions = new List<int>();

        public TokenStream()
        {
            AllocSpace();
        }
        public TokenStream(Token[] tokens)
            : this()
        {
            AddTokens(tokens);
            SetCurrentPosition(-1);
        }
        public TokenStream(CodeText codeText)
            : this()
        {
            this.codeText = codeText;
        }
        public int Length
        {
            get { return length; }
        }
        public bool IsEndPage
        {
            get { return currentPosInPage >= BUFF_LEN; }
        }

        void AllocSpace()
        {
            tokens = new Token[BUFF_LEN];
            currentPosInPage = 0;
            length = 0;
        }
        public void AddToken(Token tk)
        {
            if (currentPosInPage >= BUFF_LEN)
            {
                AllocSpace();
            }
            length++;
            tokens[currentPosInPage++] = tk;
        }
        public void AddTokens(Token[] tokens)
        {
            int j = tokens.Length;
            for (int i = 0; i < j; ++i)
            {
                AddToken(tokens[i]);
            }
        }

        public int CurrentPosInPage
        {
            get { return this.currentPosInPage; }
        }
        public Token GetToken(int index)
        {
            if (index < BUFF_LEN)
            {
                return tokens[index];
            }
            else
            {
                return null;
            }

        }
        public void SetCurrentPosition(int currentPos)
        {

            this.currentPosInPage = currentPos;
        }


        public void AddNewLinePos(int pos)
        {
            if (codeText != null)
            {
                codeText.LinePosList.Add(pos);
            }
        }
        public List<int> LinePosList
        {
            get { return this.newLinePostions; }
        }

        public Token ReadNextToken()
        {
            currentPosInPage++;
            if (currentPosInPage < BUFF_LEN)
            {
                //currentpos is in 
                return tokens[currentPosInPage];
            }
            else
            {
                //start new page***
                if (LoadMore())
                {

                    return tokens[currentPosInPage];
                }
                else
                {
                    //nor more
                    return null;
                }

            }
        }
        public virtual bool LoadMore()
        {
            return false;
        }

    }



    /// <summary>
    /// information about a token
    /// </summary>
    public class TokenDefinition : USymbol, ISymbolDefinition
    {

        //-----------------------------------------------------------------------------
        public static readonly TokenDefinition _eof;
        //-----------------------------------------------------------------------------
        public static readonly TokenDefinition _lineBreak = new TokenDefinition("'line_break", null, BasicTokenKind.LineBreak);
        public static readonly TokenDefinition _literalString = new TokenDefinition("'literal_string", "'literal_string", BasicTokenKind.LiteralString);
        public static readonly TokenDefinition _literalInteger = new TokenDefinition("'literal_integer", "'literal_integer", BasicTokenKind.LiteralNumber);

        public static readonly TokenDefinition _whitespace = new TokenDefinition("'white_space", null, BasicTokenKind.WhiteSpace);
        public static readonly TokenDefinition _comment = new TokenDefinition("'comment", null, BasicTokenKind.Comment);
        public static readonly TokenDefinition _identifier = new TokenDefinition("'identifier", "'identifier", BasicTokenKind.Identifier);

        //---
        //extension
        public static readonly TokenDefinition _switchToken = new TokenDefinition("'sw", null, BasicTokenKind.Unknown);

        static TokenDefinition()
        {
            _eof = new TokenDefinition("$", null, BasicTokenKind.Unknown);
            _eof.IsEOF = true;
        }
        public TokenDefinition() { }
        internal TokenDefinition(string grammarSymbolString, string presentationString, BasicTokenKind tokenKind)
        {
            this.GrammarSymbolString = grammarSymbolString;
            if (presentationString != null)
            {
                this.PresentationString = presentationString;
                this.PresentationStringLength = presentationString.Length;
            }
            this.UserTokenKind = tokenKind;
        }
        public bool IsEOF
        {
            get;
            private set;
        }

        public BasicTokenKind UserTokenKind
        {
            get;
            private set;
        }
        public bool IsNT
        {
            get
            {
                return false;
            }
        }

        public bool IsLineBreak
        {
            get
            {
                return this.UserTokenKind == BasicTokenKind.LineBreak;
            }
        }
        public bool IsIdentifier
        {
            get
            {
                return this.UserTokenKind == BasicTokenKind.Identifier;
            }
        }

        /// <summary>         
        ///real name of symbol in grammar, may be not equal with presentation string
        /// </summary>
        public string GrammarSymbolString
        {
            get;
            private set;
        }
        /// <summary>
        /// presentation string, in source code 
        /// for creating lexer
        /// </summary>
        public string PresentationString
        {
            get;
            private set;
        }
        public int PresentationStringLength
        {
            get;
            private set;
        }
        public bool IsContextualKeyword
        {
            get
            {
                return this.UserTokenKind == BasicTokenKind.ContextualKeyword;
            }
        }
        public override string ToString()
        {
            return this.PresentationString;
        }
        public int TokenInfoNumber
        {
            get;
            set;
        }
        public int TokenPrecedence
        {
            get;
            set;
        }
        public bool IsRightAssoc
        {
            get;
            set;
        }
        public int SpecialTokenName
        {
            get;
            set;
        }
        public string SymbolName { get { return GrammarSymbolString; } }


    }


    //====================================================================

    public enum NTDefintionKind
    {
        NormalNT,
        RootStartSymbol,
        UnknownNT
    }


    /// <summary>
    /// Nonterminal Symbol 
    /// </summary>
    public class NTDefinition : ISymbolDefinition
    {
        int ntDepthLevel;
        /// <summary>         
        /// store possible start tokens for this nt,         
        /// </summary>
        FirstTokenInfoCollectionDic firstTokensDic;
        Dictionary<TokenDefinition, int> followerTokens = new Dictionary<TokenDefinition, int>();
        TokenDefinition[] allPossibleFollowerCache;
        /// <summary>
        /// store possible sequences
        /// </summary>
        SymbolSequence[] allPossibleSqs;

#if DEBUG
        static int dbugTotalId = 0;
        public readonly int dbugId = dbugTotalId++;
#endif

        internal NTDefinition(string name, NTDefintionKind ntkind)
        {
            //#if DEBUG
            //            if (this.dbugId == 18 || this.dbugId == 19)
            //            { 
            //            } 
            //#endif 
            this.Name = name;
            this.NTKind = ntkind;
            firstTokensDic = new FirstTokenInfoCollectionDic(this);


        }

        public string SymbolName { get { return Name; } }
        internal int NtId
        {
            get;
            set;
        }
        /// <summary>
        /// can be empty epsilon
        /// </summary>
        public bool HasEmptyEpsilonForm
        {
            get;
            set;
        }
        internal bool HasSomeCyclicForm
        {
            get;
            set;
        }
        internal bool CyclicFormResolved
        {
            get;
            set;
        }

        public int NTDepthLevel
        {
            get
            {
                return this.ntDepthLevel;
            }
            internal set
            {
                //#if DEBUG
                //                if (this.dbugId == 197 || this.dbugId == 198)
                //                {
                //                }
                //#endif
                this.ntDepthLevel = value;
            }
        }

        public NTReductionChain ReductionChain
        {
            get;
            internal set;
        }
        /// <summary>
        /// check dependency bus
        /// </summary>
        /// <param name="depbus"></param>
        internal void CheckDependency(NTDependencyBus depbus)
        {
            if (this.NTKind == NTDefintionKind.UnknownNT)
            {
                return;
            }
            int j = allPossibleSqs.Length;
            if (depbus.EnterNT(this))
            {
                for (int i = 0; i < j; ++i)
                {
                    SymbolSequence sq = allPossibleSqs[i];
                    if (sq[0].IsNT)
                    {
                        //if start with nt then explore more

                        NTDefinition nt = (NTDefinition)sq[0];
                        if (nt != this)
                        {

                            if (!depbus.AlreadyHasCyclic(nt))
                            {

                                nt.CheckDependency(depbus);
                            }
                            else
                            {
                                //found cyclic,                                                                  
                                depbus.SnapCyclicBranch(nt);
                            }
                        }
                    }
                }

                depbus.ExitNT();
            }
            else
            {

                depbus.SnapCyclicBranch(this);
            }
        }


        public UserNTDefinition UserNT
        {
            get;
            set;
        }
        public NTDefintionKind NTKind
        {
            get;
            set;
        }
        public bool IsNT
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// load sq once
        /// </summary>
        /// <param name="symSeqs"></param>
        public void LoadSequences(SymbolSequence[] symSeqs)
        {
            if (this.allPossibleSqs != null)
            {
                //once
                throw new NotSupportedException();
            }

            //------------------------------
            int j = symSeqs.Length;
            for (int i = 0; i < j; ++i)
            {

                if (symSeqs[i].LeftSideNT != null)
                {
                    throw new NotSupportedException();
                }
                else
                {
                    symSeqs[i].SetLeftSideNt(this, i);
                }
            }
            this.allPossibleSqs = symSeqs;
        }
        /// <summary>
        /// add tokens except empty epsilon 
        /// </summary>
        /// <param name="tks"></param>
        /// <param name="foundEmptyEpsilon"></param>
        public void AddPossibleFollowerTokensExceptEmptyEpsilon(NTDefinition fromNt, out bool foundEmptyEpsilon)
        {
            TokenDefinition[] tks = fromNt.GetAllFirstTokens();
            allPossibleFollowerCache = null;//reset
            int j = tks.Length;
            foundEmptyEpsilon = false;
            for (int i = 0; i < j; ++i)
            {
                TokenDefinition tk = tks[i];
                if (!followerTokens.ContainsKey(tk))
                {

                    followerTokens.Add(tk, 0);
                }
            }
        }

        public bool AddPossibleFollowerToken(TokenDefinition tk, NTDefinition fromNT)
        {
            //if (this.Name == "_type_decl")
            //{
            //    //if (tk != TokenInfo._eof)
            //    //{
            //    //    if (tk.PresentationString == ",")
            //    //    {
            //    //        Console.WriteLine(tk.ToString() + " from " + fromNT.Name);
            //    //    }
            //    //}

            //}
            allPossibleFollowerCache = null;//reset

            if (!followerTokens.ContainsKey(tk))
            {
                followerTokens.Add(tk, 0);
                return true;
            }
            else
            {
                return false;
            }
        }

        public TokenDefinition[] GetAllPossibleFollowerTokens()
        {
            //if (this.dbugId == 5 || this.dbugId == 3)
            //{

            //}
            if (allPossibleFollowerCache != null)
            {
                return this.allPossibleFollowerCache;
            }

            TokenDefinition[] ees = new TokenDefinition[followerTokens.Count];
            this.allPossibleFollowerCache = ees;
            int i = 0;
            foreach (TokenDefinition ee in this.followerTokens.Keys)
            {
                ees[i] = ee;
                i++;
            }
            return ees;
        }
        public string Name
        {
            get;
            private set;
        }

        public int SeqCount
        {
            get
            {
                switch (this.NTKind)
                {
                    case NTDefintionKind.UnknownNT:
                        return 0;
                    default:
                        return this.allPossibleSqs.Length;
                }
            }
        }
        public SymbolSequence GetSequence(int index)
        {
            return this.allPossibleSqs[index];
        }

        public override string ToString()
        {
            return this.Name;
        }
        public TokenDefinition[] GetAllFirstTokens()
        {
            return this.firstTokensDic.GetAllFirstTokens();
        }

        internal int[] CacheBitFirstTokens
        {
            get;
            set;
        }

        internal bool IsStaticNT
        {
            //static nt -> nt that dose not have
            //sq that start with nt
            //this is assign at phase 1
            get;
            set;
        }
        internal bool FirstTokenDicChanged
        {
            get
            {
                return this.firstTokensDic.IsChanged;
            }
        }
        internal int NonStaticSeqCount
        {
            get;
            private set;
        }

        /// <summary>
        /// collect all possible terminal that can be at start  point of this nt         
        /// </summary>
        internal bool CollectFirstTerminalPhase1()
        {
            //if (this.dbugId == 3 || this.dbugId == 5)
            //{

            //}


            this.firstTokensDic.Clear();

            if (this.NTKind == NTDefintionKind.UnknownNT)
            {

                firstTokensDic.AddSequence(TokenDefinition._switchToken, null);
                this.IsStaticNT = true;
                this.SnapFirstTokenDic();
                return this.IsStaticNT;

            }
            //------------------------------------------------------------------

            SymbolSequence[] selectedSymbolList = null;
            if (this.allPossibleSqs != null)
            {
                selectedSymbolList = this.allPossibleSqs;

            }
            else
            {

                throw new NotSupportedException();
            }
            //===============
            int incompleteSqCount = 0;
            foreach (SymbolSequence sq in selectedSymbolList)
            {

                int sym_count = sq.RightSideCount;
                int pos = 0;

                ISymbolDefinition exSymbol = sq.GetExpectedSymbol(pos);

                if (exSymbol.IsNT)
                {
                    //if this is nt,
                    //first round, we assume that it not pass terminal part, so we skip
                    //in LR case, this may has left recursive
                    incompleteSqCount++;
                }
                else
                {
                    //if this sq start with token, this complete at phase 1
                    sq.IsClosed = true;
                    TokenDefinition tkinfo = (TokenDefinition)exSymbol;
                    this.firstTokensDic.AddSequence((TokenDefinition)exSymbol, sq);
                }
            }
            //-----------------------------------------------
            this.NonStaticSeqCount = incompleteSqCount;
            if (incompleteSqCount == 0)
            {
                //if no nt in its component
                //set this to be static nt
                this.IsStaticNT = true;
            }
            //-----------------------------------------------             
            this.SnapFirstTokenDic();
            return this.IsStaticNT;
        }

        /// <summary>
        /// collect remaining terminal
        /// </summary>
        /// <param name="incompleteNts"></param>
        internal bool CollectFirstTerminalPhase2()
        {
            //if (this.dbugId == 3 || this.dbugId == 5)
            //{

            //}
            SymbolSequence[] selectedSymbolList = null;
            if (this.IsStaticNT)
            {
                return false;
            }
            if (this.allPossibleSqs != null && this.allPossibleSqs.Length > 0)
            {
                selectedSymbolList = this.allPossibleSqs;
            }
            else
            {
                throw new NotSupportedException();
            }
            //=============== 
            int nonstaticSeqCount = 0;
            foreach (SymbolSequence sq in selectedSymbolList)
            {

                int pos = 0;
            RECUR:
                if (sq.IsClosed)
                {

                    continue;
                }

                //phase2 we use only non static sq
                //(it always start with nt)
                ISymbolDefinition symbol = sq.GetExpectedSymbol(pos);
                if (!symbol.IsNT)
                {


                    //if this is not nt
                    //may be previus item is nt with
                    //empty epsilon, then it translate to here

                    this.firstTokensDic.AddTokenIfNotExist((TokenDefinition)symbol);
                    continue;
                }

                //always nt
                NTDefinition resolvedNT = sq.GetExpectedSymbol(pos) as NTDefinition;

                if (resolvedNT == this)
                {

                    //skip because of left recursive
                    //nonstaticSeqCount++;
                    continue;
                }
                //-------------------------------------------------------------------                

                //even this nt is not complete , we just add current first tokens
                this.firstTokensDic.FillDataWithAnotherIfNotExist(sq, resolvedNT.firstTokensDic);
                if (!resolvedNT.IsStaticNT)
                {
                    //if some resolved nt is not static
                    //then this seq is not static sq
                    sq.IsClosed = false;
                }
                //--------------------------------------------------------------------
                if (resolvedNT.HasEmptyEpsilonForm)
                {

                    if (pos + 1 < sq.RightSideCount)
                    {

                        pos++;
                        goto RECUR;
                    }
                }
                if (resolvedNT.IsStaticNT)
                {
                    sq.IsClosed = true;
                }
                if (!sq.IsClosed)
                {
                    nonstaticSeqCount++;
                }
            }


            bool hasSomeChanged = (this.NonStaticSeqCount != nonstaticSeqCount) || firstTokensDic.IsChanged;

            this.NonStaticSeqCount = nonstaticSeqCount;
            if (nonstaticSeqCount == 0)
            {
                this.IsStaticNT = true;
            }
            this.SnapFirstTokenDic();
            return hasSomeChanged;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal bool CollectFirstTerminalPhase3()
        {
            //if (this.dbugId == 3 || this.dbugId == 5)
            //{

            //}
            SymbolSequence[] selectedSymbolList = null;
            if (this.allPossibleSqs != null && this.allPossibleSqs.Length > 0)
            {
                selectedSymbolList = this.allPossibleSqs;
            }
            else
            {
                throw new NotSupportedException();
            }
            //=============== 
            int nonstaticSeqCount = 0;
            foreach (SymbolSequence sq in selectedSymbolList)
            {

                int pos = 0;
            RECUR:
                if (sq.IsClosed)
                {
                    //if this static sq then skip 
                    continue;
                }

                //phase2 : we use only non static sq
                //(start with nt)
                ISymbolDefinition symbol = sq.GetExpectedSymbol(pos);
                if (!symbol.IsNT)
                {

                    this.firstTokensDic.AddTokenIfNotExist((TokenDefinition)symbol);
                    continue;
                }

                NTDefinition resolvedNT = sq.GetExpectedSymbol(pos) as NTDefinition;

                if (resolvedNT == this)
                {

                    //skip because of left recursive
                    continue;
                }

                this.firstTokensDic.FillDataWithAnotherIfNotExist(sq, resolvedNT.firstTokensDic);

                if (resolvedNT.HasEmptyEpsilonForm)
                {

                    if (pos + 1 < sq.RightSideCount)
                    {

                        pos++;
                        goto RECUR;
                    }
                }


                if (resolvedNT.IsStaticNT)
                {
                    sq.IsClosed = true;
                }

            }


            bool hasSomeChanged = (this.NonStaticSeqCount != nonstaticSeqCount) || firstTokensDic.IsChanged;

            this.NonStaticSeqCount = nonstaticSeqCount;

            this.SnapFirstTokenDic();
            return hasSomeChanged;
        }
        internal void SnapFirstTokenDic()
        {
            //if (this.dbugId == 3 || this.dbugId == 5)
            //{

            //}
            this.firstTokensDic.SnapTokenDic();
        }

        internal bool AddFirstTerminalPhase3IfNotExists(Dictionary<TokenDefinition, int> tks)
        {
            //if (this.dbugId == 3 || this.dbugId == 5)
            //{

            //}
            bool changed = false;
            foreach (TokenDefinition tk in tks.Keys)
            {
                if (this.firstTokensDic.AddTokenIfNotExist(tk))
                {

                    changed = true;
                }
            }
            return changed;
        }

    }

    public class UserTokenDefinition : USymbol
    {

        string grammarString;
        public UserTokenDefinition(string grammarString)
        {
            this.grammarString = grammarString;
        }
        public UserTokenDefinition(TokenDefinition tkdef)
        {
            this.TkDef = tkdef;
        }
        public TokenDefinition TkDef
        {
            get;
            set;
        }

        public string GrammarString
        {
            get
            {
                if (TkDef != null)
                {
                    return TkDef.GrammarSymbolString;
                }
                else
                {
                    return grammarString;
                }
            }

        }
        public static implicit operator TokenDefinition(UserTokenDefinition u)
        {
            return u.TkDef;
        }
    }

}