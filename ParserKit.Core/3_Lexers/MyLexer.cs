//MIT, 2015-2017, ParserApprentice
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Parser.ParserKit;

namespace Parser.ParserKit.Lexers
{
    public delegate void LexEventDelegate(LexEventArgs e);

    public class LexEventArgs : EventArgs
    {
        internal char[] buffer;
        internal int start;
        public TokenDefinition tokenDef;
        internal int len;
        public LexToDoState fromLexState;
        public bool moveToCustomLexMode;
        public string GetLexString()
        {
            return new string(buffer, start, len);
        }
    }



    public class MyLexer
    {
        //topdown lexer
        List<TokenDef> lexemes = new List<TokenDef>();
        ColumnBasedTable<char, LexToDo> table = new ColumnBasedTable<char, LexToDo>();

        protected Token latestToken;

        //special lex state:         
        LexStateIdenMode lexStateIdenMode = new LexStateIdenMode();
        LexStateWhiteSpace lexStateWhiteSpaceMode = new LexStateWhiteSpace();
        LexStateNumber lexStateNumber = new LexStateNumber();
        LexStateNumberDecimalPart lexStateNumberDecimalPart = new LexStateNumberDecimalPart();
        LexStateLiteralString lexStateLiteralString = new LexStateLiteralString();
        LexStateLiteralCharacter lexStateLiteralCharacter = new LexStateLiteralCharacter();

        char[] terminalChars;
        char[] whitespaceChars;
        LexEventDelegate lexEventDel2;


        protected TokenInfoCollection tokenInfoCollection;
        public MyLexer()
        {

            lexStateNumber.DecimalPart = lexStateNumberDecimalPart;
        }
        public virtual void SetTokenInfoCollection(TokenInfoCollection tokenInfoCollection)
        {
            this.tokenInfoCollection = tokenInfoCollection;

        }
        public void AddTokenDef(TokenDef lexTk)
        {
            lexemes.Add(lexTk);
        }
        public void AddTokenDefs(IEnumerable<TokenDef> lexTk)
        {
            this.lexemes.AddRange(lexTk);
        }
        public void SetTerminalTokens(char[] terminalChars)
        {
            this.terminalChars = terminalChars;
        }
        public void SetWhitespaceChars(char[] whitespaceChars)
        {
            this.whitespaceChars = whitespaceChars;
        }
        public void SetLexEventListener(LexEventDelegate lexEventDel)
        {
            this.lexEventDel2 = lexEventDel;
        }
        protected virtual void EmitLexEvent(LexEventArgs e)
        {
            if (lexEventDel2 != null)
            {
                lexEventDel2(e);
            }
        }
        public void MakeTable()
        {

            //------------------------------------
            //init
            lexStateWhiteSpaceMode.SetWhiteSpace(whitespaceChars);
            //------------------------------------
            int lexemCount = lexemes.Count;
            LexStateSeq rootState = new LexStateSeq('\0', 0);
            for (int i = 0; i < lexemCount; ++i)
            {
                //set keyword id
                lexemes[i].KeywordId = i;
                rootState.AddKeyword(lexemes[i]);
            }
            //------------------------------------
            //start move next
            rootState.MakeNextState();//recursive ***
            //assign states  
            List<LexState> totalLexStates = new List<LexState>();
            rootState.FlattenLexStates(totalLexStates);//***
            //------------------------------------
            //special state for iden 
            totalLexStates.Add(lexStateIdenMode);
            totalLexStates.Add(lexStateWhiteSpaceMode);
            totalLexStates.Add(lexStateNumber);
            totalLexStates.Add(lexStateNumberDecimalPart);
            totalLexStates.Add(lexStateLiteralString);
            totalLexStates.Add(lexStateLiteralCharacter);

            //------------------------------------
            //assign state number 
            for (int i = totalLexStates.Count - 1; i >= 0; --i)
            {
                //assign state id***
                totalLexStates[i].LexStateId = i;
            }

            for (int i = totalLexStates.Count - 1; i >= 0; --i)
            {
                //assign state id***
                totalLexStates[i].IdenState = lexStateIdenMode;
            }

            //--------------------------------------
            //create table*** 
            //for all characters (255 chars ?)
            for (int i = 0; i < 255; i++)
            {
                table.AddColumn((char)i);
            }



            table.FinishColumnsDefinition();
            //--------------------------------------
            //then add row
            int totalLexStateCount = totalLexStates.Count;
            for (int i = 0; i < totalLexStateCount; ++i)
            {
                //table.AppendNewRow(null);
                table.AppendNewRow(new LexToDo());
                WriteToTable(i, totalLexStates[i]);
            }
            //--------------------------------------
            //back to start row
            //1. whitespace
            foreach (char c in whitespaceChars)
            {
                //at state 0 when found white char-> goto lexStateWhiteSpace mode
                table.SetCell(0, c, LexToDo.CreateReadNext(lexStateWhiteSpaceMode.LexStateId));
            }

            //2.
            foreach (char c in "0123456789")
            {   //at state 0 when found number -> goto number state
                table.SetCell(0, c, LexToDo.CreateReadNext(lexStateNumber.LexStateId));
            }

            table.SetCell(0, '.', LexToDo.CreateReadNext(lexStateNumberDecimalPart.LexStateId));
            table.SetCell(0, '\0', LexToDo.CreateAccept(LexToDoState.Finished));
            table.SetCell(0, '\"', LexToDo.CreateReadNext(lexStateLiteralString.LexStateId));
            table.SetCell(0, '\'', LexToDo.CreateReadNext(lexStateLiteralCharacter.LexStateId));


        }
        void WriteToTable(int rowId, LexState lexState)
        {
            lexState.WriteTableRow(table, rowId, terminalChars);
        }

        protected virtual void CustomLex(char[] buffer, int start, out int end)
        {
            end = start;
        }

        public int LineNumber
        {
            get;
            protected set;
        }
        public int ColumnNumber
        {
            get;
            protected set;
        }


        long[] _lexCompactData;
        int _colCount;
        void CompactTable()
        {

            int rowCount = table.RowCount;
            _colCount = table.columns.Count;
            _lexCompactData = new long[rowCount * _colCount];
            int i = 0;
            for (int r = 0; r < rowCount; ++r)
            {
                for (int c = 0; c < _colCount; ++c)
                {
                    LexToDo todo = table.GetCell(r, c);
                    if (todo.acceptKeyword != null)
                    {
                        _lexCompactData[i++] = ((long)((todo.nextState << 8) | (int)todo.todo)) | ((long)(todo.acceptKeyword.TokenInfoNumber) << 32);
                    }
                    else
                    {
                        _lexCompactData[i++] = (((todo.nextState << 8) | (int)todo.todo));
                    }

                    //_lexCompactData[i++] = table.GetCell(r, c);
                }
            }
        }


        LexEventArgs lexEventArgs = new LexEventArgs();
       

        public void Lex(char[] buffer, TokenStream tokenStream, int startPos, out int currentPos)
        {
            //lex and fill into tk stream
            //------------------
            if (_lexCompactData == null)
            {
                CompactTable();
            } 

            long[] lexCompactData = _lexCompactData;
            int compactColCount = _colCount;
            //------------------

            LineNumber = 0;
            ColumnNumber = 0;
            int j = buffer.Length;
            int currentState = 0;
            int start = 0;
            int appendLength = 0;


            lexEventArgs.buffer = buffer;


            int latestCandiateKeyword = 0;
            TokenInfoCollection tokenInfoCollection = this.tokenInfoCollection;

            int i = startPos;
            char c = buffer[i];
            int lineNumber = 0;

            for (; ; )
            {
                if (c == '\n')
                {
                    lineNumber++;                      
                    tokenStream.AddNewLinePos(i);
                    //record new line
                }

                //look at table 
                //LexToDo lexTodo = table.GetCell(currentState, c);
                long lexTodo = lexCompactData[(currentState * compactColCount) + c];
                //i++;
                //continue;
                //if (lexTodo == null) 
                switch ((LexToDoState)((0xff) & lexTodo))
                {
                    case LexToDoState.EmptyState:
                        {

                            if (latestCandiateKeyword != 0)
                            {
                                //PreToken pp = new PreToken();
                                //  ptks.Add(new PreToken());
                                //create token definition here
                                //lexEventArgs.fromLexState = LexToDoState.Keyword;
                                //lexEventArgs.len = appendLength;
                                //lexEventArgs.start = start;
                                //lexEventArgs.tokenDef = latestCandidateTk; 
                                //EmitLexEvent(lexEventArgs);


                                tokenStream.AddToken(//latestToken =
                                   new Token(tokenInfoCollection.GetTokenInfoByIndex(latestCandiateKeyword), startPos));

                                if (lexEventArgs.moveToCustomLexMode)
                                {
                                    CustomLex(buffer, i, out i);
                                    lexEventArgs.moveToCustomLexMode = false;
                                }

                                if (tokenStream.IsEndPage)
                                {
                                    currentPos = i;
                                    return;
                                }

                                appendLength = 0;
                                currentState = 0;//reset 
                                latestCandiateKeyword = 0;
                            }
                            else
                            {
                                //iden mode
                                if (appendLength == 0)
                                {
                                    start = i;
                                }
                                appendLength++;
                                i++;
                                currentState = lexStateIdenMode.LexStateId;
                            }

                        }
                        continue;

                    case LexToDoState.ReadNext:
                        {
                            if (appendLength == 0)
                            {
                                start = i;
                            }
                            currentState = (int)((lexTodo & 0xffffffff) >> 8);
                            appendLength++;
                            ++i;
                        }
                        break;
                    case LexToDoState.ReadNextWithCandidateKeyword:
                        {
                            if (appendLength == 0)
                            {
                                start = i;
                            }
                            // latestCandidateTk =  lexTodo.acceptKeyword;
                            // currentState = lexTodo.nextState;
                            currentState = (int)((lexTodo & 0xffffffff) >> 8);
                            //latestCandidateTk = tokenInfoCollection.GetTokenInfoByIndex((int)(lexTodo >> 32));
                            latestCandiateKeyword = (int)(lexTodo >> 32);
                            appendLength++;
                            ++i;
                        }
                        break;
                    case LexToDoState.Iden:
                        {
                            //accept current iden
                            if (appendLength > 0)
                            {

                                //ptks[pos++] = 1;// new PreToken();
                                //ptks.Add(new PreToken());
                                //lexEventArgs.len = appendLength;
                                //lexEventArgs.start = start;
                                //lexEventArgs.fromLexState = lexTodo.todo; 
                                //EmitLexEvent(lexEventArgs);
                                 
                                tokenStream.AddToken(//latestToken =
                                   new Token(TokenDefinition._identifier,
                                   //new string(buffer, start, appendLength),
                                   start,
                                   (ushort)appendLength));
                                 

                                if (tokenStream.IsEndPage)
                                {
                                    currentPos = i;
                                    return;
                                }

                                //tokens[pos++] = (start) 
                                appendLength = 0;
                            }
                            currentState = 0;//reset 
                        }
                        break;
                    case LexToDoState.Keyword:
                        {
                            //finished
                            //one word 
                            //read next  
                            if (appendLength > 0)
                            {
                                //ptks[pos++] = 1;// new PreToken();
                                //ptks.Add(new PreToken());
                                //lexEventArgs.len = appendLength;
                                //lexEventArgs.start = start;
                                //lexEventArgs.fromLexState = lexTodo.todo;
                                //lexEventArgs.tokenDef = lexTodo.acceptKeyword;//.TokenDefinition; 
                                //EmitLexEvent(lexEventArgs); 
                                //Token tk = latestToken = new Token(tokenInfoCollection.GetTokenInfoByIndex((int)(lexTodo >> 32)), lineNumber, columnNumber);

                                tokenStream.AddToken(//latestToken =
                                    new Token(tokenInfoCollection.GetTokenInfoByIndex((int)(lexTodo >> 32)), startPos));
                                if (tokenStream.IsEndPage)
                                {
                                    currentPos = i;
                                    return;
                                }

                                //Token tk = new Token(tokenInfoCollection.GetTokenInfoByIndex((int)(lexTodo >> 32)));
                                //tokens[pos++] = 0;
                            }
                            currentState = 0;
                            appendLength = 0;
                        }
                        break;
                    case LexToDoState.Error:
                        {

                        }
                        break;
                    case LexToDoState.Whitespace:
                        {
                            //collect whitespace first
                            currentState = 0;
                            appendLength = 0;
                        }
                        break;
                    case LexToDoState.Number:
                        {
                            if (appendLength > 0)
                            {

                                tokenStream.AddToken(//latestToken =
                                    new Token(TokenDefinition._literalInteger, new string(buffer, start, appendLength)));
                                if (tokenStream.IsEndPage)
                                {
                                    currentPos = i;
                                    return;
                                }
                                //Token tk = new Token(TokenDefinition._literalInteger, new string(buffer, start, appendLength));
                                //tokens[pos++] =0;

                                //ptks[pos++] = 0;// new PreToken();
                                //ptks.Add(new PreToken());
                                //Console.WriteLine(new string(buffer, start, appendLength)); 
                                //lexEventArgs.len = appendLength;
                                //lexEventArgs.start = start;
                                //lexEventArgs.fromLexState = lexTodo.todo; 
                                //EmitLexEvent(lexEventArgs); 
                            }

                            currentState = 0;
                            appendLength = 0;
                        }
                        break;
                    case LexToDoState.NumberDecimalPart:
                        {
                            if (appendLength > 0)
                            {
                                throw new NotSupportedException();
                                //ptks[pos++] = 0;// new PreToken();
                                //ptks.Add(new PreToken());
                                //lexEventArgs.len = appendLength;
                                //lexEventArgs.start = start;
                                //lexEventArgs.fromLexState = lexTodo.todo; 
                                //EmitLexEvent(lexEventArgs); 
                            }
                            currentState = 0;
                            appendLength = 0;
                        }
                        break;
                    case LexToDoState.Finished:
                        {
                            if (appendLength > 0)
                            {
                                throw new NotSupportedException();
                            }
                            i = j;
                        }
                        break;
                    case LexToDoState.LiteralString:
                        {

                            if (appendLength > 0)
                            {

                                tokenStream.AddToken(//latestToken =
                                     new Token(TokenDefinition._literalString,
                                    // new string(buffer, start, appendLength),
                                     start,
                                    (ushort)appendLength));

                                if (tokenStream.IsEndPage)
                                {
                                    currentPos = i;
                                    return;
                                }
                                // Token tk = new Token(TokenDefinition._literalString, new string(buffer, start, appendLength));
                                //tokens[pos++] = 0;
                                //ptks[pos++] = 0;// new PreToken();
                                //ptks.Add(new PreToken());
                                //lexEventArgs.len = appendLength;
                                //lexEventArgs.start = start;
                                //lexEventArgs.fromLexState = lexTodo.todo; 
                                //EmitLexEvent(lexEventArgs); 
                            }
                            appendLength++;
                            currentState = 0;
                            appendLength = 0;
                            i++;
                        }
                        break;
                    case LexToDoState.LiteralCharacter:
                        {

                            if (appendLength > 0)
                            {

                                tokenStream.AddToken(//latestToken =
                                     new Token(TokenDefinition._literalString,
                                    //new string(buffer, start, appendLength),
                                     start,
                                     (ushort)appendLength));

                                if (tokenStream.IsEndPage)
                                {
                                    currentPos = i;
                                    return;
                                }
                                //tokens[pos++] = 0;

                                //ptks[pos++] = 0;// new PreToken();
                                //ptks.Add(new PreToken());
                                //lexEventArgs.len = appendLength;
                                //lexEventArgs.start = start;
                                //lexEventArgs.fromLexState = lexTodo.todo; 
                                //EmitLexEvent(lexEventArgs);
                            }
                            appendLength++;
                            currentState = 0;
                            appendLength = 0;
                            i++;
                        }
                        break;
                    default:
                        {
                            throw new NotSupportedException();
                        }
                }

                //--------------------------------

                if (i < j)
                {
                    c = buffer[i]; //read next  
                }
                else
                {
                    //before return
                    if (appendLength > 0)
                    {
                        c = '\0';

                    }
                    else
                    {
                        break;
                    }
                }
            }

            //--------------------------------
            tokenStream.AddToken(new Token(TokenDefinition._eof));
            currentPos = i;

        }

    
        protected virtual void BeginLex()
        {

        }
    }
    public enum LexToDoState : byte
    {
        EmptyState,
        Error,
        Keyword,
        Iden,
        ReadNext,
        ReadNextWithCandidateKeyword,
        Comment,

        Whitespace,
        Number,
        NumberDecimalPart,
        Finished,
        LiteralString,
        LiteralCharacter,

        LastRemaining,
    }

    struct LexToDo
    {
        public readonly LexToDoState todo;//1
        public readonly TokenDefinition acceptKeyword;
        public readonly int nextState;

        //-------------------------------------------------
        public readonly static LexToDo Error = new LexToDo(LexToDoState.Error, null, 0);

        //#if DEBUG
        //        static int dbugTotalId;
        //        public readonly int dbugId;
        //#endif
        private LexToDo(LexToDoState todo, TokenDef tkdef, int nextState)
        {
            this.nextState = nextState;
            this.todo = todo;
            if (tkdef != null)
            {
                acceptKeyword = tkdef.TokenDefinition;
            }
            else
            {
                acceptKeyword = null;
            }

            //#if DEBUG
            //            dbugId = dbugTotalId++;
            //#endif
            //if (dbugId == 985)
            //{

            //} 
        }
        public static LexToDo CreateAcceptKeyword(TokenDef keyword)
        {
            return new LexToDo(LexToDoState.Keyword, keyword, 0);
        }
        public static LexToDo CreateReadNext(int nextState, TokenDef candidateKeyword)
        {
            return new LexToDo(LexToDoState.ReadNextWithCandidateKeyword, candidateKeyword, nextState);
        }
        public static LexToDo CreateReadNext(int nextState)
        {
            return new LexToDo(LexToDoState.ReadNext, null, nextState);

        }
        public static LexToDo CreateAccept(LexToDoState acceptState)
        {
            return new LexToDo(acceptState, null, 0);
        }

    }

    public class TokenDef
    {
        char[] lexemeChars = null;

        public TokenDef(string lexeme)
        {
            this.lexemeChars = lexeme.ToCharArray();
        }
        public int KeywordId
        {
            get;
            set;
        }
        public char GetChar(int index)
        {
            return lexemeChars[index];
        }
        public bool IsKeyword { get; set; }
        public int WordCount
        {
            get { return this.lexemeChars.Length; }
        }
        public TokenDefinition TokenDefinition
        {
            get;
            set;
        }
        public override string ToString()
        {
            return new string(lexemeChars);
        }

    }

    abstract class LexState
    {
        public virtual void FlattenLexStates(List<LexState> output)
        {
        }
        public abstract void WriteTableRow(ColumnBasedTable<char, LexToDo> table, int rowId, char[] terminalChars);
        public int LexStateId
        {
            get;
            set;
        }
        public LexState IdenState
        {
            get;
            set;
        }
    }

    class LexStateSeq : LexState
    {
        public TokenDef stopKeyword;
        List<TokenDef> possibleKeywords = new List<TokenDef>();
        public Dictionary<char, LexStateSeq> dicGoNext;
        public readonly char jumpOver;

#if DEBUG
        static int dbugTotalId;
        public readonly int dbugId = dbugTotalId++;
#endif
        public LexStateSeq(char jumpOver, int dotpos)
        {
            this.jumpOver = jumpOver;
            this.DotPos = dotpos;

        }

        public void AddKeyword(TokenDef word)
        {
            possibleKeywords.Add(word);
        }
        public void MakeNextState()
        {
            //if (this.dbugId == 13)
            //{

            //}
            //recursive
            int j = possibleKeywords.Count;
            List<TokenDef> finishedList = null;
            this.dicGoNext = new Dictionary<char, LexStateSeq>();
            for (int i = 0; i < j; ++i)
            {
                TokenDef possibleWord = this.possibleKeywords[i];
                if (this.DotPos < possibleWord.WordCount)
                {
                    char c = possibleWord.GetChar(this.DotPos);
                    LexStateSeq childState;
                    if (!dicGoNext.TryGetValue(c, out childState))
                    {
                        childState = new LexStateSeq(c, DotPos + 1);
                        dicGoNext.Add(c, childState);
                    }
                    childState.AddKeyword(possibleWord);
                }
                else
                {
                    //finished
                    if (finishedList == null)
                    {
                        finishedList = new List<TokenDef>();
                    }
                    finishedList.Add(possibleWord);
                }
            }

            foreach (LexStateSeq lexState in this.dicGoNext.Values)
            {
                //recursive
                lexState.MakeNextState();
            }
            //if (this.dbugId == 13)
            //{

            //}
            if (finishedList != null)
            {
                switch (finishedList.Count)
                {
                    case 0: return;
                    case 1:
                        {
                            this.stopKeyword = finishedList[0];
                        }
                        break;
                    default:
                        {
                            this.stopKeyword = finishedList[0];
                        }
                        break;

                }
            }

        }
        public int DotPos
        {
            get;
            private set;
        }
        public override void FlattenLexStates(List<LexState> output)
        {
            //recursive
            output.Add(this);
            if (dicGoNext != null)
            {
                foreach (var lexState in this.dicGoNext.Values)
                {
                    lexState.FlattenLexStates(output);
                }
            }
        }
        public override void WriteTableRow(ColumnBasedTable<char, LexToDo> table, int rowId, char[] terminalChars)
        {

            if (this.stopKeyword != null)
            {
                if (this.jumpOver != '\0')
                {
                    //write finished here
                    if (this.stopKeyword.IsKeyword)
                    {
                        //not match then just move to iden mode
                        //need exact match
                        for (int i = terminalChars.Length - 1; i >= 0; --i)
                        {
                            table.SetCell(rowId, terminalChars[i], LexToDo.CreateAcceptKeyword(this.stopKeyword));
                        }
                    }
                    else
                    {
                        for (int i = terminalChars.Length - 1; i >= 0; --i)
                        {
                            table.SetCell(rowId, terminalChars[i], LexToDo.CreateAcceptKeyword(this.stopKeyword));
                        }
                    }
                }
                else
                {
                }
            }
            else
            {
                //no accept word here  
                //for (int i = 255; i >= 0; --i)
                for (int i = 0; i < 255; ++i)
                {
                    table.SetCell(rowId, i, LexToDo.CreateReadNext(this.IdenState.LexStateId));
                }
                for (int i = terminalChars.Length - 1; i >= 0; --i)
                {
                    table.SetCell(rowId, terminalChars[i], LexToDo.CreateAccept(LexToDoState.Iden));
                }

            }

            //write content of this to lex
            //1. finished at this state
            Dictionary<char, LexStateSeq> goNext = this.dicGoNext;
            ////2. go next
            if (goNext != null && goNext.Count > 0)
            {
                foreach (var kp in goNext)
                {
                    //read next
                    var nextState = kp.Value;
                    if (nextState.stopKeyword != null)
                    {
                        //create readnext with candidate keyword
                        table.SetCell(rowId, kp.Key, LexToDo.CreateReadNext(kp.Value.LexStateId, nextState.stopKeyword));
                    }
                    else
                    {
                        table.SetCell(rowId, kp.Key, LexToDo.CreateReadNext(kp.Value.LexStateId));
                    }
                }
            }

            //at all state  if found terminal then terminate


        }


    }


    class LexStateIdenMode : LexState
    {

        public override void WriteTableRow(ColumnBasedTable<char, LexToDo> table, int rowId, char[] terminalChars)
        {
            for (int i = 0; i < 255; i++)
            {
                //accept 
                table.SetCell(rowId, i, LexToDo.CreateReadNext(this.LexStateId));
            }
            for (int i = terminalChars.Length - 1; i >= 0; --i)
            {
                table.SetCell(rowId, terminalChars[i], LexToDo.CreateAccept(LexToDoState.Iden));
            }
        }
    }
    class LexStateWhiteSpace : LexState
    {
        char[] whitespaceChars;
        public void SetWhiteSpace(char[] whitespaceChars)
        {
            this.whitespaceChars = whitespaceChars;
        }
        public override void WriteTableRow(ColumnBasedTable<char, LexToDo> table, int rowId, char[] terminalChars)
        {
            //if not whitespace mode then exit the whitespace mode
            for (int i = 0; i < 255; ++i)
            {
                table.SetCell(rowId, i, LexToDo.CreateAccept(LexToDoState.Whitespace));
            }

            foreach (var c in whitespaceChars)
            {
                //still in the same state
                table.SetCell(rowId, c, LexToDo.CreateReadNext(this.LexStateId));
            }
        }
    }

    class LexStateNumber : LexState
    {
        public LexStateNumberDecimalPart DecimalPart { get; set; }
        public override void WriteTableRow(ColumnBasedTable<char, LexToDo> table, int rowId, char[] terminalChars)
        {
            //if number then collect
            //if . then switch to another mode
            for (int i = terminalChars.Length - 1; i >= 0; --i)
            {
                table.SetCell(rowId, terminalChars[i], LexToDo.CreateAccept(LexToDoState.Number));
            }
            //----------
            foreach (var c in "012345678")
            {
                table.SetCell(rowId, c, LexToDo.CreateReadNext(this.LexStateId));
            }
            //----------
            table.SetCell(rowId, '.', LexToDo.CreateReadNext(DecimalPart.LexStateId));
        }
    }
    class LexStateNumberDecimalPart : LexState
    {

        public override void WriteTableRow(ColumnBasedTable<char, LexToDo> table, int rowId, char[] terminalChars)
        {
            //in the decimal part
            //after dot
            for (int i = terminalChars.Length - 1; i >= 0; --i)
            {
                table.SetCell(rowId, terminalChars[i], LexToDo.CreateAccept(LexToDoState.NumberDecimalPart));
            }

            foreach (var c in "012345678")
            {
                table.SetCell(rowId, c, LexToDo.CreateReadNext(this.LexStateId));
            }

            table.SetCell(rowId, 'd', LexToDo.CreateReadNext(this.LexStateId));
            table.SetCell(rowId, 'f', LexToDo.CreateReadNext(this.LexStateId));
            table.SetCell(rowId, 'm', LexToDo.CreateReadNext(this.LexStateId));
        }
    }


    class LexStateLiteralString : LexState
    {
        public override void WriteTableRow(ColumnBasedTable<char, LexToDo> table, int rowId, char[] terminalChars)
        {
            //collect any string 
            //except found escape string again 
            for (int i = 0; i < 255; i++)
            {
                table.SetCell(rowId, i, LexToDo.CreateReadNext(this.LexStateId));
            }
            //in this state if we found " then accept
            table.SetCell(rowId, '\"', LexToDo.CreateAccept(LexToDoState.LiteralString));
        }
    }

    class LexStateLiteralCharacter : LexState
    {
        public override void WriteTableRow(ColumnBasedTable<char, LexToDo> table, int rowId, char[] terminalChars)
        {
            //collect any string 
            //except found escape string again 
            for (int i = 0; i < 255; i++)
            {
                table.SetCell(rowId, i, LexToDo.CreateReadNext(this.LexStateId));
            }
            table.SetCell(rowId, '\'', LexToDo.CreateAccept(LexToDoState.LiteralCharacter));
        }

    }


}