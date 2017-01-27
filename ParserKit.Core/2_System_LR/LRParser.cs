//MIT, 2015-2017, ParserApprentice 
using System; 
using System.Collections.Generic;

namespace Parser.ParserKit.LR
{
    public partial class LRParser
    {
        LRParsingTable table;
        LRParserSwitchHandler swHandler;

        internal LRParser(LRParsingTable table)
        {
            this.table = table;
#if DEBUG
            this.dbugWriteParseLog = false;
#endif
        }
        public void SetSwitchHandler(LRParserSwitchHandler swHandler)
        {
            this.swHandler = swHandler;
        }
        public ParseNode FinalNode
        {
            get;
            private set;
        }

        public bool UseFastParseMode
        {
            get;
            set;
        }
        public bool EnableBreakOnShift
        {
            get;
            set;
        }
        public bool EnableBreakOnReduce
        {
            get;
            set;
        }

#if DEBUG
        public bool dbugWriteParseLog
        {
            get;
            set;
        }
#endif
        Dictionary<int, SymbolSequence> GetKnownRRConflict(int current_state, TokenDefinition tkinfo)
        {

            RRConflictOnRow knownRR = this.table.SymbolResolutionInfo.GetRRConflictFoundOrCreateIfNotExit(current_state);
            if (knownRR != null)
            {
                return knownRR.GetSeqChoices(tkinfo);
            }
            return null;
        }

        public bool StartWith(int tokenId)
        {
            int todo = this.table.FastGetTodoFromToken(0, tokenId);
            return (todo != 0) && ((LRItemTodoKind)(todo & 0xFF) != LRItemTodoKind.UnresolvedSwitch);
            //return todo != null && todo.ItemKind != LRItemTodoKind.UnknownSwitch; 
            //LRItemTodo todo = this.table.GetTodoFromToken(0, tokenId);
            //return todo != null && todo.ItemKind != LRItemTodoKind.UnknownSwitch;
        }
        public ParseNodeHolder ParseNodeHolder
        {
            get;
            set;
        }
        public ParseResult Parse(TokenStreamReader reader)
        {
            return Parse(ParseNodeHolder, reader);
        }
        public ParseResult Parse(ParseNodeHolder holder, TokenStreamReader reader)
        {
            if (reader.CurrentReadIndex < 0)
            {
                reader.ReadNext();
            }

            var sw = new ParserSwitchContext(holder, reader);
            sw.CurrentLRParsingContext = sw.GetLRParsingContext();
            sw.Holder = this.ParseNodeHolder = holder;

            ParseResult result = Parse(sw);

            sw.RelaseLRParsingContext(sw.CurrentLRParsingContext);
            return result;
        }
#if DEBUG
        public override string ToString()
        {
            return this.table.dbugName;
        }
#endif


        static int CanTokenApprearInSeq(SymbolSequence ss, TokenDefinition tk)
        {

            // WARNING: heuristic 
            int j = ss.RightSideCount;
            for (int i = 0; i < j; ++i)
            {
                var expTk = ss[i] as TokenDefinition;
                if (expTk != null && expTk == tk)
                {
                    return i;
                }
            }
            return -1;
        }


#if DEBUG
        static int dbugCounting = 0;
        string dbugLogFileName = "parseLog.txt";
#endif

        //[System.Diagnostics.DebuggerStepperBoundary]

        public ParseResult Parse(ParserSwitchContext swContext)
        {
            if (this.UseFastParseMode)
            {
                return ParseFast(swContext);
            }
            else
            {
                return ParseDev(swContext);
            }

        }

        [System.Diagnostics.DebuggerStepperBoundary]
        ParseResult ParseDev(ParserSwitchContext swContext)
        {
            //convert to token  
            TokenStreamReader reader = swContext.Reader;
#if DEBUG
            if (dbugWriteParseLog)
            {
                reader.dbugInitLogs(dbugLogFileName);
            }
#endif
            //---------------------------------------------------------------------
            LRParsingContext parsingContext = swContext.CurrentLRParsingContext;
            StateStackDev states = parsingContext.stateStackDev;
            ParseNodeStack symbolParseNodes = parsingContext.symbolParseNodes;
            //------------------------------------------------------------------------
            LRParsingTable myLRParsingTable = this.table;
            ParserReporter reporter = swContext.GetReporter();

            //reporter.parsingContext = parsingContext;
            reporter.TokenStreamReader = reader;
            //------------------------------------------------------------------------              

            Token tk = reader.CurrentToken;
            int current_state = 0;
            states.PushNull();

            ParseNode finalNode = null;
            // bool switchBack = false;
            bool breakOnReduce = this.EnableBreakOnReduce;
            bool breakOnShift = this.EnableBreakOnShift;
            ParseNodeHolder holder = this.ParseNodeHolder = swContext.Holder;

            holder.Reporter = reporter;
            int lineNumber = 0, columnNumber = 0;
            holder.ParseNodeStack = symbolParseNodes;

            for (; ; )
            {

#if DEBUG
                dbugCounting++;
                //if (dbugCounting > 51)
                //{
                //}
                //if (dbugCounting == 21)
                //{

                //}
                //if (dbugCounting >= 16)
                //{
                //}
#endif

                TokenDefinition tkinfo = tk.TkInfo;
                LRItemTodo todo = myLRParsingTable.GetTodo(current_state, tkinfo);
                //---------------------
                reporter.CurrentToken = tk;
                //---------------------
                if (todo.IsEmpty())
                {
                    //check if switch row  
                    //if token info is contextual keyword
                    //then check current context that 
                    //should it be a keyword or an identifier 
                    todo = myLRParsingTable.GetTodo(current_state, TokenDefinition._switchToken);

                    if (todo.IsEmpty() && swContext.WaitingParserCount > 0)
                    {

                        //switch back
                        //before switch back just reduce here 
                        todo = myLRParsingTable.GetTodo(current_state, TokenDefinition._eof);
                        //reduce  
                        //until return back ... 
                        if (todo.IsEmpty())
                        {
                            //force switchback
                            if (symbolParseNodes.Count == 0)
                            {
                                return new ParseResult(null, ParseResultKind.OK, swContext.Reader.CurrentReadIndex);
                            }
                        }
                        else
                        {
                            tk = new Token(TokenDefinition._eof);
                        }
                    }
                    //-----------------------------------------------------------------
                    //if (switchBack && todo == null)
                    //{
                    //    //force switch back
                    //    if (symbolParseNodes.Count > 0)
                    //    {
                    //        return new ParseResult(symbolParseNodes.Peek(), ParseResultKind.OK, swContext.Reader.InputIndex);
                    //    }
                    //    else
                    //    {
                    //        return new ParseResult(null, ParseResultKind.OK, swContext.Reader.InputIndex);
                    //    }
                    //}
                    //-----------------------------------------------------------------
                    //if (tkinfo.IsContextualKeyword)
                    //{
                    //    todo = myLRParsingTable.GetTodo(current_state, TokenDefinition._identifier);
                    //}
                    //TODO: check contextual keyword
                    //error recovery?
                    //eg found unknown token

                    if (todo.IsEmpty())
                    {
                        //nothing todo then ask the manager
                        swContext.SwitchBackParseResult = new ParseResult();
                        swHandler(swContext);
                        ParseNode switchBackNode = swContext.SwitchBackParseNode;
                        if (switchBackNode != null)
                        {
                            symbolParseNodes.Push(switchBackNode);
                            tk = reader.ReadNext();
                            continue;
                        }
                        else
                        {
                            //skip or reduce?
                            todo = myLRParsingTable.GetTodo(current_state, TokenDefinition._eof);
                            tk = new Token(TokenDefinition._eof);
                            // symbolParseNodes.Push(new NTn1(new EmptyParseNode()));
                            continue;
                        }
                        // symbolParseNodes.Push(swContext.SwitchBackParseNode);


                    }

                    //--------------------- 
                    if (todo.IsEmpty())
                    {
                        List<TokenDefinition> expectedTokenDefs = myLRParsingTable.GetAllExpectedTokens(current_state);
                        //check if token can apprear in the same seq or not
                        //here ... we not found expected token
                        if (expectedTokenDefs != null)
                        {
                            int expectTkCount = expectedTokenDefs.Count;
                            switch (expectTkCount)
                            {

                                case 0:
                                    throw new NotSupportedException();
                                case 1:
                                    {
                                        //current token not matach with expected token
                                        int state = current_state;
                                        //so find   
                                        LRItemTodo latestGoto = states.FindLatestGoto();
                                        SymbolSequence seq = myLRParsingTable.GetSequence(latestGoto.NextItemSetNumber);
                                        //check if the token can appear in seq
                                        //heuristic
                                        int foundIndex = -1;
                                        int enter_index2 = reader.CurrentReadIndex;
                                        int lookupNCount = 10;
                                        int lookupTimes = 0;
                                        for (; ; )
                                        {

                                            foundIndex = CanTokenApprearInSeq(seq, tk.TkInfo);
                                            lookupTimes++;
                                            if (lookupTimes > lookupNCount)
                                            {
                                                break;
                                            }

                                            if (foundIndex < 0)
                                            {
                                                tk = reader.ReadNext();
                                            }
                                            else
                                            {
                                                break;
                                            }
                                        }


                                        if (foundIndex > -1)
                                        {
                                            //try insert missing tokens
                                            todo = myLRParsingTable.GetTodo(current_state, expectedTokenDefs[0]);
                                            if (todo.IsEmpty())
                                            {
                                                return new ParseResult(null, ParseResultKind.Error, reader.CurrentReadIndex);
                                            }
                                            else
                                            {
                                                //insert missing token
                                                //find expected token next
                                                //reader.ReadNextUntil(expectedTokenDefs);  
                                                tk = new Token(expectedTokenDefs[0]);
                                                switch (todo.ItemKind)
                                                {
                                                    case LRItemTodoKind.Shift:
                                                        {
                                                            reader.SetIndex(reader.CurrentReadIndex - 1);
                                                        }
                                                        break;
                                                    default: throw new NotSupportedException();

                                                }
                                            }
                                        }
                                        else
                                        {
                                            //we should skip this tk
                                            todo = myLRParsingTable.GetTodo(current_state, expectedTokenDefs[0]);
                                            if (todo.IsEmpty())
                                            {
                                                return new ParseResult(null, ParseResultKind.Error, reader.CurrentReadIndex);
                                            }
                                            else
                                            {
                                                //insert missing token
                                                //find expected token next
                                                //reader.ReadNextUntil(expectedTokenDefs);  
                                                tk = new Token(expectedTokenDefs[0]);
                                                switch (todo.ItemKind)
                                                {
                                                    case LRItemTodoKind.Shift:
                                                        {

                                                        }
                                                        break;
                                                    default: throw new NotSupportedException();
                                                }
                                            }
                                        }

                                    }
                                    break;
                                default:
                                    {
                                        LRItemTodo latestGoto = states.FindLatestGoto();
                                        SymbolSequence seq = myLRParsingTable.GetSequence(latestGoto.NextItemSetNumber);
                                        //check if the token can appear in seq
                                        //heuristic
                                        int foundIndex = -1;
                                        int enter_index2 = reader.CurrentReadIndex;
                                        int lookupNCount = 10;
                                        int lookupTimes = 0;
                                        for (; ; )
                                        {

                                            foundIndex = CanTokenApprearInSeq(seq, tk.TkInfo);
                                            lookupTimes++;
                                            if (lookupTimes > lookupNCount)
                                            {
                                                break;
                                            }

                                            if (foundIndex < 0)
                                            {
                                                tk = reader.ReadNext();
                                            }
                                            else
                                            {
                                                break;
                                            }
                                        }

                                        if (foundIndex > -1)
                                        {
                                            //try insert missing tokens
                                            todo = myLRParsingTable.GetTodo(current_state, tk.TkInfo);
                                            if (todo.IsEmpty())
                                            {
                                                return new ParseResult(null, ParseResultKind.Error, reader.CurrentReadIndex);
                                            }
                                            else
                                            {
                                                //insert missing token
                                                //find expected token next
                                                //reader.ReadNextUntil(expectedTokenDefs);  
                                                //tk = new Token(expectedTokenDefs[0]);
                                                switch (todo.ItemKind)
                                                {
                                                    case LRItemTodoKind.Shift:
                                                        {
                                                            // reader.SetIndex(reader.InputIndex - 1);
                                                        }
                                                        break;
                                                    default: throw new NotSupportedException();

                                                }
                                            }
                                        }
                                        else
                                        {
                                            //we should skip this tk
                                            todo = myLRParsingTable.GetTodo(current_state, expectedTokenDefs[0]);
                                            if (todo.IsEmpty())
                                            {
                                                return new ParseResult(null, ParseResultKind.Error, reader.CurrentReadIndex);
                                            }
                                            else
                                            {
                                                //insert missing token
                                                //find expected token next
                                                //reader.ReadNextUntil(expectedTokenDefs);  
                                                tk = new Token(expectedTokenDefs[0]);
                                                switch (todo.ItemKind)
                                                {
                                                    case LRItemTodoKind.Shift:
                                                        {

                                                        }
                                                        break;
                                                    default: throw new NotSupportedException();
                                                }
                                            }
                                        }
                                    }
                                    break;

                            }
                        }
                        else
                        {
                            //force switch back
                            return new ParseResult(null, ParseResultKind.Error, reader.CurrentReadIndex);
                        }
                    }
                }

                switch (todo.ItemKind)
                {
                    default:
                    case LRItemTodoKind.Err:
                        {
#if DEBUG
                            if (this.dbugWriteParseLog)
                            {
                                var dbugWriter = reader.dbugLogWriter;
                                dbugWriter.Write('(');
                                dbugWriter.Write(symbolParseNodes.Count.ToString());
                                dbugWriter.Write(')');


                                dbugWriter.Write(new string(' ', symbolParseNodes.Count * 2));
                                dbugWriter.Write(tkinfo.ToString() + " err:" + todo.ReduceToSequenceNumber);
                                dbugWriter.Write(' ');
                                dbugWriter.Flush();

                            }
#endif

                            throw new NotSupportedException();
                        }
                    case LRItemTodoKind.ResolveSwitch:
                        {
                            //------------------
                            //consult switch table with switch record number
                            //for 2 values
                            //1. nextStateNumber
                            //2. parseNameIndex 
                            //------------------

                            //known switch 
                            // int toParserName, nextStateNumber;
                            if (tk.TkInfo.IsEOF)
                            {
                                //no actual switch
                                //symbolParseNodes.Push(new NonTerminalParseNode(new EmptyParseNode(), null));
                                //this.table.GetSwitchRecord(todo.SwitchRecordNumber, out toParserName, out nextStateNumber);
                                //states.Push(current_state = nextStateNumber, todo);

                                continue;
                            }
                            //make switch decision *** 
                            //var prevLookFor = swContext.LookFor;
                            ////-----------------------------------------------------------------------------------------------------



                            swContext.SwitchDetail = table.GetSwitchDetail(todo.SwitchRecordNumber);
                            swContext.SwitchBackParseResult = new ParseResult();//result 
                            swHandler(swContext);
                            symbolParseNodes.Push(swContext.SwitchBackParseNode);
                            states.Push(current_state = swContext.SwitchBackState, todo);

                            tk = reader.CurrentToken;
                            continue;
                        }
                    case LRItemTodoKind.UnresolvedSwitch:
                        {
                            //------------------
                            //consult switch table with switch record number
                            //for 2 values
                            //1. nextStateNumber
                            //2. parseNameIndex 
                            //------------------

                            //int toParserName, nextStateNumber;
                            if (tk.TkInfo.IsEOF)
                            {
                                //no actual switch
                                //symbolParseNodes.Push(new NonTerminalParseNode(new EmptyParseNode(), null));
                                //this.table.GetUnresolvedSwitchRecord(todo.SwitchRecordNumber, out toParserName, out nextStateNumber);
                                //states.Push(current_state = nextStateNumber, todo);
                                continue;
                            }
                            //make switch decision ***   

                            swContext.SwitchDetail = table.GetSwitchDetail(todo.SwitchRecordNumber);
                            swContext.SwitchBackParseResult = new ParseResult();//result 


                            swHandler(swContext);

                            symbolParseNodes.Push(swContext.SwitchBackParseNode);
                            states.Push(current_state = swContext.SwitchBackState, todo);

                            tk = reader.CurrentToken;
                            continue;
                        }
                    case LRItemTodoKind.Shift:
                        {
#if DEBUG
                            if (this.dbugWriteParseLog)
                            {
                                var dbugWriter = reader.dbugLogWriter;
                                dbugWriter.Write('(');
                                dbugWriter.Write(symbolParseNodes.Count.ToString());
                                dbugWriter.Write(')');

                                dbugWriter.Write(new string(' ', symbolParseNodes.Count * 2));
                                dbugWriter.WriteLine(tk.ToString() + " ,s:" + todo.NextItemSetNumber);
                            }
#endif


                            if (breakOnShift)
                            {
                                SymbolSequence toSq = myLRParsingTable.GetSequence(todo.OriginalSeqNumberForShift);
                                 
                                UserExpectedSymbol exp = toSq.GetOriginalUserExpectedSymbol(todo.SampleUserExpectedSymbolPos);

                                if (exp.onStepDel != null)
                                {
                                    exp.onStepDel(holder);
                                }
                                else
                                {
                                    toSq.NotifyEvent(ParseEventKind.Shift, holder);
                                }

                            }

                            symbolParseNodes.Push(tk);
                            states.Push(current_state = todo.StateNumber, todo);
                            tk = reader.ReadNext();


                        }
                        break;
                    case LRItemTodoKind.ConflictRR:
                        {

#if DEBUG
                            if (this.dbugWriteParseLog)
                            {
                                var dbugWriter = reader.dbugLogWriter;
                                dbugWriter.Write('(');
                                dbugWriter.Write(symbolParseNodes.Count.ToString());
                                dbugWriter.Write(')');


                                dbugWriter.Write(new string(' ', symbolParseNodes.Count * 2));
                                dbugWriter.WriteLine(tk.ToString() + " ,rr:" + todo.NextItemSetNumber);
                            }
#endif

                            var seqChoices = GetKnownRRConflict(current_state, tkinfo);
                            if (seqChoices == null)
                            {
                                throw new NotSupportedException();
                            }

                            current_state = states.PopAndPeek();
                            ParseNode singleWaitNode = symbolParseNodes.Pop();

                            //just pop away?, as if it dose not exist? 
                            NonTerminalParseNode nt_parseNode = null;
                            if (singleWaitNode.IsTerminalNode)
                            {
                                throw new NotSupportedException();
                                //terminal node -> add node info
                                //make reduction ***
                                //nt_parseNode = new NonTerminalParseNode((Token)singleWaitNode, swContext.GetNewTicket());
                            }
                            else
                            {
                                throw new NotSupportedException();
                                //if there is one node and it is nt
                                //we don't create new nt,just add reduction info 
                                nt_parseNode = (NonTerminalParseNode)singleWaitNode;
                                //?
                                throw new NotSupportedException();
                            }
                        }
                        break;
                    case LRItemTodoKind.Accept:
                        {
                            if (swContext.WaitingParserCount == 0 &&
                                symbolParseNodes.Count != 1)
                            {
                                throw new NotSupportedException();
                            }

                            //if (symbolParseNodes.Count != 1)
                            //{
                            //    throw new NotSupportedException();
                            //}

                            finalNode = symbolParseNodes.Pop();
                            this.FinalNode = finalNode;
                            goto EXIT;
                        }
                    case LRItemTodoKind.Goto:
                        {
                        }
                        break;
                    case LRItemTodoKind.Reduce:
                        {
                            //swContext.LookFor = new SwitchToLookingFor();
                            //same as state number
                            SymbolSequence reduceToSq = myLRParsingTable.GetSequence(todo.ReduceToSequenceNumber);
                            //check if we need  
                            //1. a new node or 
                            //2. just add reduction information to existing node
                            //if number of symbol > 1 or just one terminal symbol 
                            //then we create a nonterminal and wrap it in this step 

                            int symbolCount = reduceToSq.RightSideCount;
                            //------------------------------------------------------------ 
                            if (breakOnReduce)
                            {
                                holder.CurrrentRightSymbolCount = symbolCount;
                                holder.ContextOwner = symbolParseNodes.Peek();
                                //invoke for break step only 
                                reduceToSq.NotifyEvent(ParseEventKind.Reduce, holder);
                            }
                            //------------------------------------------------------------

#if DEBUG
                            if (this.dbugWriteParseLog)
                            {
                                var dbugWriter = reader.dbugLogWriter;
                                dbugWriter.Write('(');
                                dbugWriter.Write(symbolParseNodes.Count.ToString());
                                dbugWriter.Write(')');


                                dbugWriter.Write(new string(' ', symbolParseNodes.Count * 2));
                                dbugWriter.Write("r:" + todo.ReduceToSequenceNumber);
                                dbugWriter.Write(' ');
                                dbugWriter.Write(reduceToSq.ToString());
                            }

#endif
                            //state
                            current_state = states.Pop(symbolCount);

                            //parse tree
                            if (holder.CancelNextParseTree)
                            {
                                //just clear
                                symbolParseNodes.Clear(symbolCount);
                                //add auto tree
                                symbolParseNodes.Push(holder.GetContextNtAstNode());
                                holder.ResetBuildNextTree();
                            }
                            else
                            {
                                //auto create parse tree
                                switch (symbolCount)
                                {
                                    case 0:
                                        {
                                            throw new NotSupportedException();
                                        }
                                    case 1:
                                        {
                                            if (reduceToSq.FromBifurcation)
                                            {
                                                symbolParseNodes.Push(symbolParseNodes.PopReverseIntoObject(new NTn1()));
                                            }
                                            else
                                            {  //use Peek()***
                                                symbolParseNodes.Peek().AddInheritUpSequence(reduceToSq);
                                            }
                                        }
                                        break;
                                    case 2:
                                        {

                                            if (reduceToSq.CreatedFromListOfNt)
                                            {

                                                ParseNode p1, p2;
                                                symbolParseNodes.Pop(out p1, out p2);
                                                if (p1.IsList)
                                                {
                                                    NonTerminalParseNodeList ntList = (NonTerminalParseNodeList)p1;
                                                    ntList.AddParseNode(p2);//just member no sep
                                                    symbolParseNodes.Push(ntList);
                                                }
                                                else
                                                {
                                                    //just create and
                                                    NonTerminalParseNodeList ntList = new NonTerminalParseNodeList(reduceToSq);
                                                    ntList.AddParseNode(p1);//just member no sep
                                                    ntList.AddParseNode(p2);//just member no sep
                                                    symbolParseNodes.Push(ntList);
                                                }
                                            }
                                            else
                                            {

                                                var newnode = symbolParseNodes.PopReverseIntoObject(new NTn2());
                                                //symbolParseNodes.Push(new NonTerminalParseNode(symbolParseNodes.PopReverseIntoArray(2), reduceToSq));
                                                symbolParseNodes.Push(newnode);
                                            }
                                        }
                                        break;
                                    case 3:
                                        {


                                            //auto create parse tree
                                            if (reduceToSq.CreatedFromListOfNt)
                                            {

                                                ParseNode p1, p2, p3;
                                                symbolParseNodes.Pop(out p1, out p2, out p3);
                                                if (p1.IsList)
                                                {
                                                    NonTerminalParseNodeList ntList = (NonTerminalParseNodeList)p1;
                                                    ntList.AddParseNode(p2); //member
                                                    ntList.AddSepNode(p3); //sep
                                                    symbolParseNodes.Push(ntList);
                                                }
                                                else
                                                {   //just create and
                                                    NonTerminalParseNodeList ntList = new NonTerminalParseNodeList(reduceToSq);
                                                    //add member
                                                    ntList.AddParseNode(p1); //member
                                                    ntList.AddSepNode(p2); //sep
                                                    ntList.AddParseNode(p3); //member
                                                    symbolParseNodes.Push(ntList);
                                                }
                                            }
                                            else
                                            {
                                                symbolParseNodes.Push(symbolParseNodes.PopReverseIntoObject(new NTn3()));
                                            }

                                        }
                                        break;
                                    case 4:
                                        {

                                            symbolParseNodes.Push(symbolParseNodes.PopReverseIntoObject(new NTn4()));
                                        }
                                        break;
                                    case 5:
                                        {

                                            symbolParseNodes.Push(symbolParseNodes.PopReverseIntoObject(new NTn5()));
                                        }
                                        break;
                                    case 6:
                                        {

                                            symbolParseNodes.Push(symbolParseNodes.PopReverseIntoObject(new NTn6()));
                                        }
                                        break;
                                    case 7:
                                        {

                                            symbolParseNodes.Push(symbolParseNodes.PopReverseIntoObject(new NTn7()));
                                        }
                                        break;
                                    case 8:
                                        {

                                            symbolParseNodes.Push(symbolParseNodes.PopReverseIntoObject(new NTn8()));
                                        }
                                        break;
                                    case 9:
                                        {

                                            symbolParseNodes.Push(symbolParseNodes.PopReverseIntoObject(new NTn9()));
                                        }
                                        break;
                                    case 10:
                                        {

                                            symbolParseNodes.Push(symbolParseNodes.PopReverseIntoObject(new NTn10()));
                                        }
                                        break;
                                    case 11:
                                        {

                                            symbolParseNodes.Push(symbolParseNodes.PopReverseIntoObject(new NTn11()));
                                        }
                                        break;
                                    case 12:
                                        {

                                            symbolParseNodes.Push(symbolParseNodes.PopReverseIntoObject(new NTn12()));
                                        }
                                        break;
                                    case 13:
                                        {
                                            symbolParseNodes.Push(symbolParseNodes.PopReverseIntoObject(new NTn13()));
                                        }
                                        break;
                                    default:
                                        {
                                            symbolParseNodes.Push(symbolParseNodes.PopReverseIntoObject(new NTnN(symbolCount)));
                                        }
                                        break;
                                }
                            }


#if DEBUG
                            ////-------------------------------------------------------------------
                            //if (this.dbugWriteParseLog)
                            //{
                            //    var writer = reader.dbugLogWriter;
                            //}
                            ////-------------------------------------------------------------------
#endif

                            //reporter.Result = nt_parseNode;
                            //reporter.selectedSymbolSq = reduceToSq;
                            //-------------------------------------------------------------------
                            //dev mode: may invoke notification here                             
                            //reduceToSq.NotifyEvent(reporter);
                            //-------------------------------------------------------------------

                            //goto
                            // LRItemTodo todo2 = myLRParsingTable.GetTodo(states.Peek(), reduceToSq.LeftSideNT);
                            var todoNext = myLRParsingTable.GetTodo(current_state, reduceToSq.LeftSideNT);
                            states.Push(current_state = todoNext.StateNumber, todoNext);


                        }
                        break;

                }
            }
        //---------------
        EXIT:
            return new ParseResult(finalNode, ParseResultKind.OK, swContext.Reader.CurrentReadIndex);
        }

        //Token eof = Token.FromPool(TokenDefinition._eof, 0, 0);
        //[System.Diagnostics.DebuggerStepperBoundary]
        ParseResult ParseFast(ParserSwitchContext swContext)
        {
            //convert to token  
            TokenStreamReader reader = swContext.Reader;
#if DEBUG
            if (dbugWriteParseLog)
            {
                reader.dbugInitLogs(dbugLogFileName);
            }
#endif
            //---------------------------------------------------------------------
            LRParsingContext parsingContext = swContext.CurrentLRParsingContext;
            StateStack states = parsingContext.states2;
            ParseNodeStack symbolParseNodes = parsingContext.symbolParseNodes;
            //------------------------------------------------------------------------
            LRParsingTable myLRParsingTable = this.table;
            ParserReporter reporter = swContext.GetReporter();

            //reporter.parsingContext = parsingContext;
            reporter.TokenStreamReader = reader;
            //------------------------------------------------------------------------              
            ParseNodeHolder holder = swContext.Holder;
            holder.ParseNodeStack = symbolParseNodes;
            holder.Reporter = reporter;

            Token tk = reader.CurrentToken;
            int current_state = 0;
            states.PushNull();

            ParseNode finalNode = null;

            for (; ; )
            {

                if (tk == null)
                {
                    break;
                }
#if DEBUG
                dbugCounting++;
                //if (dbugCounting == 21)
                //{

                //}
                //if (dbugCounting >= 16)
                //{
                //}
#endif


                int todo = myLRParsingTable.FastGetTodoFromToken(current_state, tk._tokenNumber);
                //---------------------
                reporter.CurrentToken = tk;
                //---------------------
                if (todo == 0)
                {
                    //check if switch row  
                    //if token info is contextual ketword
                    //then check current context that 
                    //should it be a keyword or an identifier 
                    todo = myLRParsingTable.FastGetTodo(current_state, TokenDefinition._switchToken);

                    if (todo == 0 && swContext.WaitingParserCount > 0)
                    {

                        //switch back
                        //before switch back just reduce here 
                        todo = myLRParsingTable.FastGetTodo(current_state, TokenDefinition._eof);
                        //reduce  
                        //until return back ... 
                        if (todo == 0)
                        {
                            //force switchback
                            if (symbolParseNodes.Count == 0)
                            {
                                return new ParseResult(null, ParseResultKind.OK, swContext.Reader.CurrentReadIndex);
                            }
                        }
                        else
                        {
                            tk = swContext.eof; // Token.FromPool(TokenDefinition._eof, 0, 0);
                            //Console.WriteLine("eof" + tk.dbugTkId);
                        }
                    }
                    //-----------------------------------------------------------------
                    //if (switchBack && todo == null)
                    //{
                    //    //force switch back
                    //    if (symbolParseNodes.Count > 0)
                    //    {
                    //        return new ParseResult(symbolParseNodes.Peek(), ParseResultKind.OK, swContext.Reader.InputIndex);
                    //    }
                    //    else
                    //    {
                    //        return new ParseResult(null, ParseResultKind.OK, swContext.Reader.InputIndex);
                    //    }
                    //}
                    //----------------------------------------------------------------- 
                    //TODO: check contextual keyword 
                    //if (tkinfo.IsContextualKeyword)
                    //{
                    //    todo = myLRParsingTable.FastGetTodo(current_state, TokenDefinition._identifier);
                    //}
                    //error recovery?
                    //eg found unknown token
                    //-----------------------------------------------------------------

                    if (todo == 0)
                    {
                        swContext.SwitchBackParseResult = new ParseResult();
                        swHandler(swContext);
                        ParseNode switchBackNode = swContext.SwitchBackParseNode;
                        if (switchBackNode != null)
                        {
                            symbolParseNodes.Push(switchBackNode);
                        }
                        else
                        {
                            symbolParseNodes.Push(new NTn1(new EmptyParseNode()));
                        }
                        tk = reader.ReadNext();
                        continue;

                    }
                    //--------------------- 
                    if (todo == 0)
                    {
                        List<TokenDefinition> expectedTokenDefs = myLRParsingTable.GetAllExpectedTokens(current_state);
                        //check if token can apprear in the same seq or not
                        //here ... we not found expected token
                        if (expectedTokenDefs != null)
                        {
                            int expectTkCount = expectedTokenDefs.Count;
                            switch (expectTkCount)
                            {

                                case 0:
                                    throw new NotSupportedException();
                                case 1:
                                    {
                                        //current token not matach with expected token
                                        int state = current_state;
                                        //so find   
                                        LRItemTodo latestGoto = states.FindLatestGoto();
                                        SymbolSequence seq = myLRParsingTable.GetSequence(latestGoto.NextItemSetNumber);
                                        //check if the token can appear in seq
                                        //heuristic
                                        int foundIndex = -1;
                                        int enter_index2 = reader.CurrentReadIndex;
                                        int lookupNCount = 10;
                                        int lookupTimes = 0;
                                        for (; ; )
                                        {

                                            foundIndex = CanTokenApprearInSeq(seq, tk.TkInfo);
                                            lookupTimes++;
                                            if (lookupTimes > lookupNCount)
                                            {
                                                break;
                                            }

                                            if (foundIndex < 0)
                                            {
                                                tk = reader.ReadNext();
                                            }
                                            else
                                            {
                                                break;
                                            }
                                        }


                                        if (foundIndex > -1)
                                        {
                                            //try insert missing tokens
                                            todo = myLRParsingTable.FastGetTodo(current_state, expectedTokenDefs[0]);
                                            if (todo == 0)
                                            {
                                                return new ParseResult(null, ParseResultKind.Error, reader.CurrentReadIndex);
                                            }
                                            else
                                            {
                                                //insert missing token
                                                //find expected token next
                                                //reader.ReadNextUntil(expectedTokenDefs);  
                                                tk = new Token(expectedTokenDefs[0]);
                                                switch ((LRItemTodoKind)(todo & 0xFF))
                                                {
                                                    case LRItemTodoKind.Shift:
                                                        {
                                                            reader.SetIndex(reader.CurrentReadIndex - 1);
                                                        }
                                                        break;
                                                    default: throw new NotSupportedException();

                                                }
                                            }
                                        }
                                        else
                                        {
                                            //we should skip this tk
                                            todo = myLRParsingTable.FastGetTodo(current_state, expectedTokenDefs[0]);
                                            if (todo == 0)
                                            {
                                                return new ParseResult(null, ParseResultKind.Error, reader.CurrentReadIndex);
                                            }
                                            else
                                            {
                                                //insert missing token
                                                //find expected token next
                                                //reader.ReadNextUntil(expectedTokenDefs);  
                                                tk = new Token(expectedTokenDefs[0]);
                                                switch ((LRItemTodoKind)(todo & 0xFF))
                                                {
                                                    case LRItemTodoKind.Shift:
                                                        {

                                                        }
                                                        break;
                                                    default: throw new NotSupportedException();
                                                }
                                            }
                                        }

                                    }
                                    break;
                                default:
                                    {
                                        LRItemTodo latestGoto = states.FindLatestGoto();
                                        SymbolSequence seq = myLRParsingTable.GetSequence(latestGoto.NextItemSetNumber);
                                        //check if the token can appear in seq
                                        //heuristic
                                        int foundIndex = -1;
                                        int enter_index2 = reader.CurrentReadIndex;
                                        int lookupNCount = 10;
                                        int lookupTimes = 0;
                                        for (; ; )
                                        {

                                            foundIndex = CanTokenApprearInSeq(seq, tk.TkInfo);
                                            lookupTimes++;
                                            if (lookupTimes > lookupNCount)
                                            {
                                                break;
                                            }

                                            if (foundIndex < 0)
                                            {
                                                tk = reader.ReadNext();
                                            }
                                            else
                                            {
                                                break;
                                            }
                                        }

                                        if (foundIndex > -1)
                                        {
                                            //try insert missing tokens
                                            todo = myLRParsingTable.FastGetTodo(current_state, tk.TkInfo);
                                            if (todo == 0)
                                            {
                                                return new ParseResult(null, ParseResultKind.Error, reader.CurrentReadIndex);
                                            }
                                            else
                                            {
                                                //insert missing token
                                                //find expected token next
                                                //reader.ReadNextUntil(expectedTokenDefs);  
                                                //tk = new Token(expectedTokenDefs[0]);
                                                switch ((LRItemTodoKind)(todo & 0xFF))
                                                {
                                                    case LRItemTodoKind.Shift:
                                                        {
                                                            // reader.SetIndex(reader.InputIndex - 1);
                                                        }
                                                        break;
                                                    default: throw new NotSupportedException();

                                                }
                                            }
                                        }
                                        else
                                        {
                                            //we should skip this tk
                                            todo = myLRParsingTable.FastGetTodo(current_state, expectedTokenDefs[0]);
                                            if (todo == 0)
                                            {
                                                return new ParseResult(null, ParseResultKind.Error, reader.CurrentReadIndex);
                                            }
                                            else
                                            {
                                                //insert missing token
                                                //find expected token next
                                                //reader.ReadNextUntil(expectedTokenDefs);  
                                                tk = new Token(expectedTokenDefs[0]);
                                                //switch ((LRItemTodoKind)(todo & 0xFF))
                                                //{
                                                //    case LRItemTodoKind.Shift:
                                                //        {

                                                //        }
                                                //        break;
                                                //    default: throw new NotSupportedException();
                                                //}
                                            }
                                        }
                                    }
                                    break;

                            }
                        }
                        else
                        {
                            //force switch back
                            return new ParseResult(null, ParseResultKind.Error, reader.CurrentReadIndex);
                        }
                    }
                }

                switch ((LRItemTodoKind)(todo & 0xFF))
                {
                    default:
                    case LRItemTodoKind.Err:
                        {
#if DEBUG
                            if (this.dbugWriteParseLog)
                            {
                                var dbugWriter = reader.dbugLogWriter;
                                dbugWriter.Write('(');
                                dbugWriter.Write(symbolParseNodes.Count.ToString());
                                dbugWriter.Write(')');


                                dbugWriter.Write(new string(' ', symbolParseNodes.Count * 2));
                                dbugWriter.Write(tk.TkInfo.ToString() + " err:" + (todo >> 8));
                                dbugWriter.Write(' ');
                                dbugWriter.Flush();

                            }
#endif

                            throw new NotSupportedException();
                        }
                    case LRItemTodoKind.ResolveSwitch:
                        {

                            //------------------
                            //consult switch table with switch record number
                            //for 2 values
                            //1. nextStateNumber
                            //2. parseNameIndex 
                            //------------------

                            //resolve switch 
                            //------------------
                            //consult switch table with switch record number
                            //for 2 values
                            //1. nextStateNumber
                            //2. parseNameIndex 
                            //------------------

                            //int toParserName, nextStateNumber;
                            if (tk.TkInfo.IsEOF)
                            {
                                //no actual switch
                                //symbolParseNodes.Push(new NonTerminalParseNode(new EmptyParseNode(), null));
                                //this.table.GetUnresolvedSwitchRecord(todo.SwitchRecordNumber, out toParserName, out nextStateNumber);
                                //states.Push(current_state = nextStateNumber, todo);
                                continue;
                            }
                            //make switch decision *** 


                            swContext.SwitchDetail = table.GetSwitchDetail(todo >> 8);
                            swContext.SwitchBackParseResult = new ParseResult();//result 
                            swHandler(swContext);

                            symbolParseNodes.Push(swContext.SwitchBackParseNode);
                            states.Push(current_state = swContext.SwitchBackState);

                            tk = reader.CurrentToken;
                            continue;
                        }
                    case LRItemTodoKind.UnresolvedSwitch:
                        {
                            //int toParserName, nextStateNumber;
                            if (tk.TkInfo.IsEOF)
                            {
                                //no actual switch
                                //symbolParseNodes.Push(new NonTerminalParseNode(new EmptyParseNode(), null));
                                //this.table.GetUnresolvedSwitchRecord(todo.SwitchRecordNumber, out toParserName, out nextStateNumber);
                                //states.Push(current_state = nextStateNumber, todo);
                                continue;
                            }
                            //make switch decision *** 
                            //var prevLookFor = swContext.LookFor;


                            swContext.SwitchDetail = table.GetSwitchDetail(todo >> 8);
                            swContext.SwitchBackParseResult = new ParseResult();//result 
                            swHandler(swContext);

                            symbolParseNodes.Push(swContext.SwitchBackParseNode);
                            states.Push(current_state = swContext.SwitchBackState);

                            tk = reader.CurrentToken;
                            continue;
                        }
                    case LRItemTodoKind.Shift:
                        {
                            symbolParseNodes.Push(tk);
                            states.Push(current_state = (todo >> 8));
                            tk = reader.ReadNext();
                        }
                        break;
                    case LRItemTodoKind.ConflictRR:
                        {
                            TokenDefinition tkinfo = tk.TkInfo;
                            var seqChoices = GetKnownRRConflict(current_state, tkinfo);
                            if (seqChoices == null)
                            {
                                throw new NotSupportedException();
                            }

                            current_state = states.PopAndPeek();
                            ParseNode singleWaitNode = symbolParseNodes.Pop();

                            //just pop away?, as if it dose not exist? 
                            NonTerminalParseNode nt_parseNode = null;
                            if (singleWaitNode.IsTerminalNode)
                            {
                                throw new NotSupportedException();
                                //terminal node -> add node info
                                //make reduction ***
                                //nt_parseNode = new NonTerminalParseNode((Token)singleWaitNode, swContext.GetNewTicket());
                            }
                            else
                            {
                                throw new NotSupportedException();
                                //if there is one node and it is nt
                                //we don't create new nt,just add reduction info 
                                nt_parseNode = (NonTerminalParseNode)singleWaitNode;
                                //?
                                throw new NotSupportedException();
                            }
                        }
                        break;
                    case LRItemTodoKind.Accept:
                        {
                            if (symbolParseNodes.Count != 1)
                            {
                                throw new NotSupportedException();
                            }

                            finalNode = symbolParseNodes.Pop();
                            this.FinalNode = finalNode;
                            goto EXIT;
                        }
                    case LRItemTodoKind.Goto:
                        {
                        }
                        break;
                    case LRItemTodoKind.Reduce:
                        {
                            //  Console.WriteLine("rd");
                            //same as state number
                            SymbolSequence reduceToSq = myLRParsingTable.GetSequence((todo >> 8));
                            int symbolCount = reduceToSq.RightSideCount;
                            if (reduceToSq.HasReductionListener)
                            {

                                holder.ContextOwner = symbolParseNodes.Peek();
                                holder.CurrrentRightSymbolCount = symbolCount;
                                reduceToSq.NotifyReduceEvent(holder);
                            }

                            //check if we need  
                            //1. a new node or 
                            //2. just add reduction information to existing node
                            //if number of symbol > 1 or just one terminal symbol 
                            //then we create a nonterminal and wrap it in this step

                            ParseNode peekNode = null;
                            switch (symbolCount)
                            {
                                case 0:
                                    {
                                        throw new NotSupportedException();
                                    }
                                case 1:
                                    {
                                        current_state = states.PopAndPeek();
                                        if (reduceToSq.FromBifurcation)
                                        {
                                            symbolParseNodes.Push(peekNode = symbolParseNodes.PopReverseIntoObject(new NTn1()));
                                        }
                                        else
                                        {
                                            //use Peek()***
                                            peekNode = symbolParseNodes.Peek();//.AddInheritUpSequence(reduceToSq);
                                        }
                                    }
                                    break;
                                case 2:
                                    {
                                        current_state = states.Pop(2);
                                        if (reduceToSq.CreatedFromListOfNt)
                                        {

                                            ParseNode p1, p2;
                                            symbolParseNodes.Pop(out p1, out p2);
                                            if (p1.IsList)
                                            {
                                                NonTerminalParseNodeList ntList = (NonTerminalParseNodeList)p1;
                                                ntList.AddParseNode(p2);
                                                symbolParseNodes.Push(peekNode = ntList);
                                            }
                                            else
                                            {
                                                //just create and
                                                NonTerminalParseNodeList ntList = new NonTerminalParseNodeList();
                                                //add member
                                                ntList.AddParseNode(p1);
                                                ntList.AddParseNode(p2);
                                                symbolParseNodes.Push(peekNode = ntList);
                                            }
                                        }
                                        else
                                        {
                                            symbolParseNodes.Push(peekNode = symbolParseNodes.PopReverseIntoObject(new NTn2()));
                                        }
                                    }
                                    break;
                                case 3:
                                    {

                                        current_state = states.Pop(3);
                                        if (reduceToSq.CreatedFromListOfNt)
                                        {

                                            ParseNode p1, p2, p3;
                                            symbolParseNodes.Pop(out p1, out p2, out p3);
                                            if (p1.IsList)
                                            {
                                                NonTerminalParseNodeList ntList = (NonTerminalParseNodeList)p1;
                                                ntList.AddParseNode(p2);
                                                ntList.AddSepNode(p3);
                                                symbolParseNodes.Push(peekNode = ntList);
                                            }
                                            else
                                            {   //just create and
                                                NonTerminalParseNodeList ntList = new NonTerminalParseNodeList();
                                                //add member
                                                ntList.AddParseNode(p1);
                                                ntList.AddSepNode(p2);
                                                ntList.AddParseNode(p3);
                                                symbolParseNodes.Push(peekNode = ntList);
                                            }
                                        }
                                        else
                                        {
                                            symbolParseNodes.Push(peekNode = symbolParseNodes.PopReverseIntoObject(new NTn3()));
                                        }
                                    }
                                    break;
                                case 4:
                                    {
                                        current_state = states.Pop(4);
                                        symbolParseNodes.Push(symbolParseNodes.PopReverseIntoObject(new NTn4()));
                                    }
                                    break;
                                case 5:
                                    {
                                        current_state = states.Pop(5);
                                        symbolParseNodes.Push(symbolParseNodes.PopReverseIntoObject(new NTn5()));
                                    }
                                    break;
                                case 6:
                                    {
                                        current_state = states.Pop(6);
                                        symbolParseNodes.Push(symbolParseNodes.PopReverseIntoObject(new NTn6()));
                                    }
                                    break;
                                case 7:
                                    {
                                        current_state = states.Pop(7);
                                        symbolParseNodes.Push(symbolParseNodes.PopReverseIntoObject(new NTn7()));
                                    }
                                    break;
                                case 8:
                                    {
                                        current_state = states.Pop(8);
                                        symbolParseNodes.Push(symbolParseNodes.PopReverseIntoObject(new NTn8()));
                                    }
                                    break;
                                case 9:
                                    {
                                        current_state = states.Pop(9);
                                        symbolParseNodes.Push(symbolParseNodes.PopReverseIntoObject(new NTn9()));
                                    }
                                    break;
                                case 10:
                                    {
                                        current_state = states.Pop(10);
                                        symbolParseNodes.Push(symbolParseNodes.PopReverseIntoObject(new NTn10()));
                                    }
                                    break;
                                case 11:
                                    {
                                        current_state = states.Pop(11);
                                        symbolParseNodes.Push(symbolParseNodes.PopReverseIntoObject(new NTn11()));
                                    }
                                    break;
                                case 12:
                                    {
                                        current_state = states.Pop(12);
                                        symbolParseNodes.Push(symbolParseNodes.PopReverseIntoObject(new NTn12()));
                                    }
                                    break;
                                case 13:
                                    {
                                        current_state = states.Pop(13);
                                        symbolParseNodes.Push(symbolParseNodes.PopReverseIntoObject(new NTn13()));
                                    }
                                    break;
                                default:
                                    {
                                        current_state = states.Pop(symbolCount);
                                        symbolParseNodes.Push(symbolParseNodes.PopReverseIntoObject(new NTnN(symbolCount)));
                                    }
                                    break;
                            }

                            //------------------------------------------------------------ 
                            //if (breakOnReduce)
                            //{
                            //    //reporter.Result = nt_parseNode;
                            //    //invoke for break step only
                            //reporter.Result = symbolParseNodes.Peek(); 

                            //if (reduceToSq.HasReductionListener)
                            //{
                            //    holder.ContextOwner = peekNode;
                            //    reduceToSq.NotifyReduceEvent(holder);
                            //}
                            // Console.WriteLine("a");

                            //}
                            //------------------------------------------------------------

#if DEBUG
                            ////-------------------------------------------------------------------
                            //if (this.dbugWriteParseLog)
                            //{
                            //    var writer = reader.dbugLogWriter;
                            //}
                            ////-------------------------------------------------------------------
#endif

                            //reporter.Result = nt_parseNode;
                            //reporter.selectedSymbolSq = reduceToSq;
                            //-------------------------------------------------------------------
                            //dev mode: may invoke notification here                             
                            //reduceToSq.NotifyEvent(reporter);
                            //-------------------------------------------------------------------

                            //goto

                            states.Push(current_state = (myLRParsingTable.FastGetTodo(current_state, reduceToSq.LeftSideNT) >> 8));

                        }
                        break;

                }
            }
        //---------------
        EXIT:
            return new ParseResult(finalNode, ParseResultKind.OK, swContext.Reader.CurrentReadIndex);
        }




    }


}