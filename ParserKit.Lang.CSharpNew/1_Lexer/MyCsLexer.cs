//MIT, 2015-2017, ParserApprentice
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Parser.ParserKit;
using Parser.CodeDom;
using Parser.ParserKit.Lexers;

namespace Parser.MyCs
{


    public class MyCsTokenStream : TokenStream
    {
        //token stream with lexer
        char[] inputBuffer;
        MyCsLexer lexer;
        int startPos;
        int lastestPos;
        int totalCharCountLim;

        public MyCsTokenStream(CodeText codeText, MyCsLexer lexer)
            : base(codeText)
        {

            this.inputBuffer = codeText.SourceBuffer;
            this.lexer = lexer;
            this.totalCharCountLim = inputBuffer.Length - 1;
        }
        public void Lex()
        {
            int lastestPos;
            lexer.Lex(inputBuffer, this, this.startPos, out lastestPos);

            this.lastestPos = lastestPos;
            this.startPos = lastestPos;
        }
        public override bool LoadMore()
        {
            //lex again
            if (startPos < totalCharCountLim)
            {

                int currentPos = this.CurrentPosInPage;
                if (currentPos == TokenStream.BUFF_LEN)
                {
                    this.tokens[0] = tokens[currentPos];
                    currentPosInPage = 0;
                    this.length = 0;
                    Lex();
                }
                else
                {
                    int remainLen = TokenStream.BUFF_LEN - currentPos;
                    Array.Copy(tokens, currentPos, tokens, 0, remainLen);
                    //this.currentPage[0] = currentPage[currentPos];
                    currentPosInPage = remainLen;
                    this.length = remainLen;
                    Lex();
                    currentPosInPage = 1;
                }


                return true;
            }
            return false;
        }
    }

    public class MyCsLexer
    {
        TokenInfoCollection tokenInfoCollection;
        InternalCsLexer _internalCsLexer;
        TokenDefinition g_lt;
        TokenDefinition g_gt;

        struct MonitorToken
        {
            public readonly bool isSuspected;
            public readonly Token tk;
            public MonitorToken(Token tk, bool isSuspected)
            {

                this.tk = tk;
                this.isSuspected = isSuspected;
            }
        }

        enum SpecialTokenName
        {
            NotThisSpecialTk,


            BasicLt,

            BasicGt,
            GenericLt,
            GenericGt,
            Iden,
            //gen_gt_comfirm_tks
            Open_Paren,
            Close_Paren,
            Close_Bracket,
            Close_Brace,
            Colon,
            SemiColon,
            Comma,
            Dot,
            Quest, //?
            Equal, // ==
            NotEqual, //!=
            Pipe, // |
            Cap, //^


            StartCommentLine, //  //
            StartCommentBlock  //  /*

        }

        class InternalCsLexer : MyLexer
        {

            List<int> ltgts = new List<int>();
            SpecialTokenName commentMode;
            public InternalCsLexer()
            {
                Reset();
            }
            public List<int> LtGtTokens
            {
                get { return ltgts; }
            }
            public override void SetTokenInfoCollection(TokenInfoCollection tokenInfoCollection)
            {
                base.SetTokenInfoCollection(tokenInfoCollection);
                this.ltgts.Clear();
            }
            void Reset()
            {
            }
            protected override void BeginLex()
            {
                Reset();
            }

            protected override void CustomLex(char[] buffer, int start, out int end)
            {
                //comment mode
                end = start;
                switch (commentMode)
                {
                    case SpecialTokenName.StartCommentLine:
                        {
                            // //
                            int len = buffer.Length;
                            for (int i = start; i < len; ++i)
                            {
                                //end at line break
                                if (buffer[i] == '\n')
                                {
                                    //stop at this 
                                    end = i + 1;
                                    //we attach comment to existing token
                                    if (latestToken != null)
                                    {
                                        latestToken.AddTrivialToken(new TrivialToken(
                                          TrivialTokenKind.CommentLine,
                                          new string(buffer, start, (i - start))));
                                    }
                                    LineNumber++;
                                    ColumnNumber = 0;
                                    return; //*** 
                                }
                            }

                            //------------------
                            {
                                if (latestToken != null)
                                {
                                    latestToken.AddTrivialToken(new TrivialToken(
                                        TrivialTokenKind.CommentLine,
                                        new string(buffer, start, (len - start))));
                                }
                            }

                            end = len;
                        }
                        break;
                    case SpecialTokenName.StartCommentBlock:
                        {
                            // /*
                            int state = 0;
                            int len = buffer.Length;
                            for (int i = start; i < len; ++i)
                            {

                                switch (state)
                                {
                                    case 0:
                                        {
                                            if (buffer[i] == '*')
                                            {
                                                //move next state
                                                state = 1;
                                            }

                                        }
                                        break;
                                    case 1:
                                        {
                                            if (buffer[i] == '/')
                                            {
                                                //
                                                //create comment token
                                                end = i + 1;
                                                if (latestToken != null)
                                                {
                                                    latestToken.AddTrivialToken(new TrivialToken(
                                                      TrivialTokenKind.CommentBlock,
                                                      new string(buffer, start, (i - start) - 1)));
                                                    //not include /* and  */                 
                                                }

                                                return; //***
                                            }
                                            else
                                            {
                                                state = 0;// zero again
                                            }
                                        }
                                        break;

                                }
                            }
                            //exit at this state
                            {
                                if (latestToken != null)
                                {
                                    latestToken.AddTrivialToken(new TrivialToken(
                                               TrivialTokenKind.CommentBlock,
                                    new string(buffer, start, (len - start) - 1)));
                                }
                            }

                            end = len;

                        }
                        break;
                }
            }
            //List<Token> myTokens = new List<Token>();
            //MemoryStream msMyTks = new MemoryStream();
            //BinaryWriter wr;

            //void AppendToken(Token tk)
            //{
            //    //if (_latestToken != null)
            //    //{
            //    //    _latestToken._nextTk = tk; 
            //    //}
            //    //tk.SetupLocation(this.LineNumber, this.ColumnNumber);
            //    ////_latestToken._nextTk = tk;
            //    ////myTokens.Add(tk);
            //    //tkCount++;
            //    //_latestToken = tk;


            //    //tkCount++;
            //    ////_latestToken._nextTk = tk;
            //    //PreToken preToken = new PreToken();
            //    //preToken.lineNum = this.LineNumber;
            //    //preToken.colNum = (ushort)this.ColumnNumber;
            //    //preToken.tokenInfo = e.tokenDef;
            //    //myTokens.Add(preToken);
            //}
            void AppendToken(TokenDefinition tkdef, LexEventArgs e)
            {
                // return;
                //if (_latestToken != null)
                //{
                //    _latestToken._nextTk = tk; 
                //}
                //tk.SetupLocation(this.LineNumber, this.ColumnNumber);
                ////_latestToken._nextTk = tk;
                ////myTokens.Add(tk);
                //tkCount++;
                //_latestToken = tk;
                //tkCount++;
                //_latestToken._nextTk = tk;
                //PreToken preToken = new PreToken();
                //preToken.lineNum = this.LineNumber;
                //preToken.colNum = (ushort)this.ColumnNumber;
                //preToken.tokenInfo = tkdef;
                //preToken.data = e.GetLexString();
                //myTokens.Add(preToken);
            }
            void AppendToken(TokenDefinition tkdef)
            {
                //if (_latestToken != null)
                //{
                //    _latestToken._nextTk = tk; 
                //}
                //tk.SetupLocation(this.LineNumber, this.ColumnNumber);
                ////_latestToken._nextTk = tk;
                ////myTokens.Add(tk);
                //tkCount++;
                //_latestToken = tk;


                //tkCount++;
                //_latestToken._nextTk = tk;
                //return;
                //PreToken preToken = new PreToken();
                //preToken.lineNum = this.LineNumber;
                //preToken.colNum = (ushort)this.ColumnNumber;
                //preToken.tokenInfo = tkdef;
                //myTokens.Add(preToken);
            }
            protected override void EmitLexEvent(LexEventArgs e)
            {


                switch (e.fromLexState)
                {
                    case LexToDoState.Keyword:
                        {
                            TokenDefinition tkdef = e.tokenDef;

                            switch ((SpecialTokenName)tkdef.SpecialTokenName)
                            {
                                default:
                                    {
                                        // outputTokens.Append(_latestToken = new Token(tkdef));
                                        //AppendToken(new Token(tkdef));
                                        AppendToken(tkdef);
                                        // _latestToken.SetupLocation(this.LineNumber, this.ColumnNumber);
                                    }
                                    break;
                                case SpecialTokenName.BasicLt:
                                case SpecialTokenName.BasicGt:
                                    {

                                        throw new NotSupportedException();
                                        //ltgts.Add(myTokens.Count);
                                        //  AppendToken(new Token(tkdef));
                                        AppendToken(tkdef);
                                        //outputTokens.Append(_latestToken = new Token(tkdef));
                                        //_latestToken.SetupLocation(this.LineNumber, this.ColumnNumber);

                                    }
                                    break;
                                case SpecialTokenName.StartCommentBlock:
                                    //change to another lexer mode
                                    e.moveToCustomLexMode = true;
                                    commentMode = SpecialTokenName.StartCommentBlock;
                                    break;
                                case SpecialTokenName.StartCommentLine:
                                    //change to another lexer mode
                                    e.moveToCustomLexMode = true;
                                    commentMode = SpecialTokenName.StartCommentLine;
                                    break;
                            }
                        }
                        break;
                    case LexToDoState.LiteralString:
                        {

                        }
                        break;
                    case LexToDoState.LiteralCharacter:
                        {

                        }
                        break;
                    case LexToDoState.Number:
                        {
                            AppendToken(TokenDefinition._literalInteger, e);
                            // AppendToken(new Token(TokenDefinition._literalInteger, e.LexString));
                            //outputTokens.Append(_latestToken = new Token(TokenDefinition._literalInteger, e.LexString));
                            //_latestToken.SetupLocation(this.LineNumber, this.ColumnNumber);
                        }
                        break;
                    case LexToDoState.Whitespace:
                        {

                        }
                        break;
                    case LexToDoState.Iden:
                        {

                            AppendToken(TokenDefinition._identifier, e);
                            //AppendToken(tkdef);
                            // AppendToken(new Token(TokenDefinition._identifier, e.LexString));
                            //outputTokens.Append(_latestToken = new Token(TokenDefinition._identifier, e.LexString));
                            //_latestToken.SetupLocation(this.LineNumber, this.ColumnNumber);
                        }
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }
        }

        public MyCsLexer(TokenInfoCollection tokenInfoCollection)
        {
            this.tokenInfoCollection = tokenInfoCollection;
            this._internalCsLexer = new InternalCsLexer();

            PrepareAndMakeTable(tokenInfoCollection);
            this._internalCsLexer.SetTokenInfoCollection(tokenInfoCollection);
        }

        List<TokenDefinition> GetTerminals(string space_sep_terms)
        {
            string[] terms = space_sep_terms.Split(' ');
            int j = terms.Length;
            List<TokenDefinition> foundList = new List<TokenDefinition>(j);
            for (int i = 0; i < j; ++i)
            {
                foundList.Add(GetTokenInfo(terms[i]));
            }
            return foundList;
        }
        TokenDefinition GetTokenInfo(string grammarSymbolString)
        {
            return this.tokenInfoCollection.GetTokenInfo(grammarSymbolString);
        }

        void AssignSpecialNameToToken(string grammarSymbolString, SpecialTokenName specialTokenName)
        {
#if DEBUG
            //check during debug
            if ((SpecialTokenName)(GetTokenInfo(grammarSymbolString).SpecialTokenName) != SpecialTokenName.NotThisSpecialTk)
            {
                throw new NotSupportedException();
            }
#endif
            GetTokenInfo(grammarSymbolString).SpecialTokenName = (int)specialTokenName;
        }

        void PrepareAndMakeTable(TokenInfoCollection tokenInfoCollection)
        {

            int tokenCount = tokenInfoCollection.SnapAllTokenCount;
            //=================================== 
            //assign special name to token definition
            //for handle generic <> ambiguous
            AssignSpecialNameToToken("<", SpecialTokenName.BasicLt);
            AssignSpecialNameToToken(">", SpecialTokenName.BasicGt);
            AssignSpecialNameToToken("'identifier", SpecialTokenName.Iden);

            //"( ) ] } : ; , . ? == != | ^"
            AssignSpecialNameToToken("(", SpecialTokenName.Open_Paren);
            AssignSpecialNameToToken(")", SpecialTokenName.Close_Paren);
            AssignSpecialNameToToken("]", SpecialTokenName.Close_Bracket);
            AssignSpecialNameToToken("}", SpecialTokenName.Close_Brace);
            AssignSpecialNameToToken(":", SpecialTokenName.Colon);
            AssignSpecialNameToToken(";", SpecialTokenName.SemiColon);
            AssignSpecialNameToToken(",", SpecialTokenName.Comma);
            AssignSpecialNameToToken(".", SpecialTokenName.Dot);
            AssignSpecialNameToToken("?", SpecialTokenName.Quest);
            AssignSpecialNameToToken("==", SpecialTokenName.Equal);
            AssignSpecialNameToToken("!=", SpecialTokenName.NotEqual);
            AssignSpecialNameToToken("|", SpecialTokenName.Pipe);
            AssignSpecialNameToToken("^", SpecialTokenName.Cap);

            AssignSpecialNameToToken("//", SpecialTokenName.StartCommentLine);
            AssignSpecialNameToToken("/*", SpecialTokenName.StartCommentBlock);

            (g_lt = GetTokenInfo("<t")).SpecialTokenName = (int)SpecialTokenName.GenericLt;
            (g_gt = GetTokenInfo(">t")).SpecialTokenName = (int)SpecialTokenName.GenericGt;

            //=================================== 

            Dictionary<string, bool> uniqueCheck = new Dictionary<string, bool>();
            //build token list
            for (int i = 0; i < tokenCount; ++i)
            {
                TokenDefinition tokenDef = tokenInfoCollection.GetTokenInfoByIndex(i);
                if (tokenDef.PresentationString == null)
                {
                    continue;
                }

                switch (tokenDef.UserTokenKind)
                {
                    case BasicTokenKind.Terminal:
                    case BasicTokenKind.Keyword:
                    case BasicTokenKind.ContextualKeyword:
                        {
                            //use presentation string 
                            TokenDef newLexeme = new TokenDef(tokenDef.PresentationString);
                            newLexeme.TokenDefinition = tokenDef;
                            _internalCsLexer.AddTokenDef(newLexeme);
                            uniqueCheck.Add(tokenDef.GrammarSymbolString, true);
                        }
                        break;
                    default:
                        {

                        }
                        break;
                }
            }

            _internalCsLexer.SetWhitespaceChars(" \t\r\n".ToCharArray());

            List<char> singleTermsTokens = new List<char>(tokenInfoCollection.GetSingleTerminals());

            singleTermsTokens.AddRange(" \0".ToCharArray());

            _internalCsLexer.SetTerminalTokens(singleTermsTokens.ToArray());
            _internalCsLexer.MakeTable();
        }

        public void Lex(char[] input, TokenStream tkStream, int startPos, out int latestPos)
        {


            _internalCsLexer.Lex(input, tkStream, startPos, out latestPos);

            //PreToken[] tks = _internalCsLexer.ToTokenArray();
            //--------------------------------------------- 
            //resolve ambiguos  
            int j = 0;
            List<int> ltgtTokens = _internalCsLexer.LtGtTokens;
            if ((j = ltgtTokens.Count) == 0)
            {

            }

            //Stack<MonitorToken> openLtStack = new Stack<MonitorToken>();
            //int tokenCountLim = tks.Length - 1;
            //for (int i = 0; i < j; ++i)
            //{
            //    int pos = ltgtTokens[i];
            //    //get current pos
            //    Token tk = tks[pos];
            //    bool isSuspected = false;
            //    TokenDefinition currentTokenInfo = (TokenDefinition)tk.TkInfo;
            //    switch ((SpecialTokenName)currentTokenInfo.SpecialTokenName)
            //    {
            //        case SpecialTokenName.BasicLt:
            //            {
            //                Token nextTk = null;
            //                if (pos < tokenCountLim)
            //                {
            //                    //not at the end
            //                    nextTk = tks[pos + 1];
            //                    if ((SpecialTokenName)nextTk.TkInfo.SpecialTokenName == SpecialTokenName.Iden)
            //                    {
            //                        isSuspected = true;
            //                    }
            //                }
            //                openLtStack.Push(new MonitorToken(tk, isSuspected));
            //            }
            //            break;
            //        case SpecialTokenName.BasicGt:
            //            {
            //                //prev token
            //                if (pos > 0)
            //                {
            //                    Token prev_tk = tks[pos - 1];
            //                    if ((SpecialTokenName)prev_tk.TkInfo.SpecialTokenName == SpecialTokenName.Iden)
            //                    {
            //                        isSuspected = true;
            //                    }
            //                    if (isSuspected)
            //                    {
            //                        Token nextTk = null;
            //                        if (pos < tokenCountLim)
            //                        {
            //                            nextTk = tks[pos + 1];
            //                            //test again
            //                            //isSuspected = gen_gt_confirm_tks.Contains((TokenDefinition)nextTk.TkInfo);
            //                            switch ((SpecialTokenName)(nextTk.TkInfo.SpecialTokenName))
            //                            {
            //                                //gen_gt_comfirm_tks
            //                                case SpecialTokenName.Open_Paren:
            //                                case SpecialTokenName.Close_Paren:
            //                                case SpecialTokenName.Close_Bracket:
            //                                case SpecialTokenName.Close_Brace:
            //                                case SpecialTokenName.Colon:
            //                                case SpecialTokenName.SemiColon:
            //                                case SpecialTokenName.Comma:
            //                                case SpecialTokenName.Dot:
            //                                case SpecialTokenName.Quest:
            //                                case SpecialTokenName.Equal:
            //                                case SpecialTokenName.NotEqual:
            //                                case SpecialTokenName.Pipe:
            //                                case SpecialTokenName.Cap:
            //                                    isSuspected = true;
            //                                    break;
            //                                default:
            //                                    isSuspected = false;
            //                                    break;
            //                            }
            //                        }
            //                    }
            //                }
            //                if (openLtStack.Count > 0)
            //                {
            //                    MonitorToken openLT = openLtStack.Pop();
            //                    if (isSuspected && openLT.isSuspected)
            //                    {
            //                        openLT.tk.ChangeTokenInfo(g_lt);
            //                        tk.ChangeTokenInfo(g_gt);
            //                    }
            //                }
            //            }
            //            break;
            //    }
            //}
            //return tkstream;
        }
        public void Lex(string input, TokenStream tkStream)
        {
            char[] buffer = input.ToCharArray();
            int latestPos;
            Lex(buffer, tkStream, 0, out latestPos);

        }
    }
}