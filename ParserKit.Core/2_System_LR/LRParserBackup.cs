//MIT 2015-2017, ParserApprentice  
//using System;
//using System.Text;
//using System.Collections.Generic;
//namespace Parser.ParserKit.LR
//{
//    public partial class LRParser
//    {
//        ParseResult ParseFast_Backup(ParserSwitchContext swContext)
//        {
//            //convert to token  
//            TokenStreamReader reader = swContext.Reader;
//#if DEBUG
//            if (dbugWriteParseLog)
//            {
//                reader.dbugInitLogs(dbugLogFileName);
//            }
//#endif
//            //---------------------------------------------------------------------
//            LRParsingContext parsingContext = swContext.CurrentLRParsingContext;
//            StateStack states = parsingContext.states2;
//            ParseNodeStack symbolParseNodes = parsingContext.symbolParseNodes;
//            //------------------------------------------------------------------------
//            LRParsingTable myLRParsingTable = this.table;
//            ParserReporter reporter = swContext.GetReporter();

//            //reporter.parsingContext = parsingContext;
//            reporter.TokenStreamReader = reader;
//            //------------------------------------------------------------------------              

//            Token tk = reader.CurrentToken;
//            int current_state = 0;
//            states.PushNull();

//            ParseNode finalNode = null;

//            for (;;)
//            {

//#if DEBUG
//                dbugCounting++;
//                //if (dbugCounting == 21)
//                //{

//                //}
//                //if (dbugCounting >= 16)
//                //{
//                //}
//#endif

//                TokenDefinition tkinfo = tk.TkInfo;
//                int todo = myLRParsingTable.FastGetTodoFromToken(current_state, tkinfo.TokenInfoNumber);
//                //---------------------
//                reporter.CurrentToken = tk;
//                //---------------------
//                if (todo == 0)
//                {
//                    //check if switch row  
//                    //if token info is contextual ketword
//                    //then check current context that 
//                    //should it be a keyword or an identifier 
//                    todo = myLRParsingTable.FastGetTodo(current_state, TokenDefinition._switchToken);

//                    if (todo == 0 && swContext.WaitingParserCount > 0)
//                    {

//                        //switch back
//                        //before switch back just reduce here 
//                        todo = myLRParsingTable.FastGetTodo(current_state, TokenDefinition._eof);
//                        //reduce  
//                        //until return back ... 
//                        if (todo == 0)
//                        {
//                            //force switchback
//                            if (symbolParseNodes.Count == 0)
//                            {
//                                return new ParseResult(null, ParseResultKind.OK, swContext.Reader.InputIndex);
//                            }
//                        }
//                        else
//                        {
//                            tk = tk_eof;
//                        }
//                    }
//                    //-----------------------------------------------------------------
//                    //if (switchBack && todo == null)
//                    //{
//                    //    //force switch back
//                    //    if (symbolParseNodes.Count > 0)
//                    //    {
//                    //        return new ParseResult(symbolParseNodes.Peek(), ParseResultKind.OK, swContext.Reader.InputIndex);
//                    //    }
//                    //    else
//                    //    {
//                    //        return new ParseResult(null, ParseResultKind.OK, swContext.Reader.InputIndex);
//                    //    }
//                    //}
//                    //-----------------------------------------------------------------

//                    //TODO: check contextual keyword

//                    //if (tkinfo.IsContextualKeyword)
//                    //{
//                    //    todo = myLRParsingTable.FastGetTodo(current_state, TokenDefinition._identifier);
//                    //}
//                    //error recovery?
//                    //eg found unknown token

//                    if (todo == 0)
//                    {

//                        swContext.SwitchBackParseResult = new ParseResult();
//                        swHandler(swContext);
//                        ParseNode switchBackNode = swContext.SwitchBackParseNode;
//                        if (switchBackNode != null)
//                        {
//                            symbolParseNodes.Push(switchBackNode);
//                        }
//                        else
//                        {
//                            symbolParseNodes.Push(new NonTerminalParseNode(new EmptyParseNode(), null));
//                        }
//                        tk = reader.ReadNext();
//                        continue;

//                    }
//                    //--------------------- 
//                    if (todo == 0)
//                    {
//                        List<TokenDefinition> expectedTokenDefs = myLRParsingTable.GetAllExpectedTokens(current_state);
//                        //check if token can apprear in the same seq or not
//                        //here ... we not found expected token
//                        if (expectedTokenDefs != null)
//                        {
//                            int expectTkCount = expectedTokenDefs.Count;
//                            switch (expectTkCount)
//                            {

//                                case 0:
//                                    throw new NotSupportedException();
//                                case 1:
//                                    {
//                                        //current token not matach with expected token
//                                        int state = current_state;
//                                        //so find   
//                                        LRItemTodo latestGoto = states.FindLatestGoto();
//                                        SymbolSequence seq = myLRParsingTable.GetSequence(latestGoto.NextItemSetNumber);
//                                        //check if the token can appear in seq
//                                        //heuristic
//                                        int foundIndex = -1;
//                                        int enter_index2 = reader.InputIndex;
//                                        int lookupNCount = 10;
//                                        int lookupTimes = 0;
//                                        for (;;)
//                                        {

//                                            foundIndex = CanTokenApprearInSeq(seq, tk.TkInfo);
//                                            lookupTimes++;
//                                            if (lookupTimes > lookupNCount)
//                                            {
//                                                break;
//                                            }

//                                            if (foundIndex < 0)
//                                            {
//                                                tk = reader.ReadNext();
//                                            }
//                                            else
//                                            {
//                                                break;
//                                            }
//                                        }


//                                        if (foundIndex > -1)
//                                        {
//                                            //try insert missing tokens
//                                            todo = myLRParsingTable.FastGetTodo(current_state, expectedTokenDefs[0]);
//                                            if (todo == 0)
//                                            {
//                                                return new ParseResult(null, ParseResultKind.Error, reader.InputIndex);
//                                            }
//                                            else
//                                            {
//                                                //insert missing token
//                                                //find expected token next
//                                                //reader.ReadNextUntil(expectedTokenDefs);  
//                                                tk = new Token(expectedTokenDefs[0]);
//                                                switch ((LRItemTodoKind)(todo & 0xFF))
//                                                {
//                                                    case LRItemTodoKind.Shift:
//                                                        {
//                                                            reader.SetIndex(reader.InputIndex - 1);
//                                                        }
//                                                        break;
//                                                    default: throw new NotSupportedException();

//                                                }
//                                            }
//                                        }
//                                        else
//                                        {
//                                            //we should skip this tk
//                                            todo = myLRParsingTable.FastGetTodo(current_state, expectedTokenDefs[0]);
//                                            if (todo == 0)
//                                            {
//                                                return new ParseResult(null, ParseResultKind.Error, reader.InputIndex);
//                                            }
//                                            else
//                                            {
//                                                //insert missing token
//                                                //find expected token next
//                                                //reader.ReadNextUntil(expectedTokenDefs);  
//                                                tk = new Token(expectedTokenDefs[0]);
//                                                switch ((LRItemTodoKind)(todo & 0xFF))
//                                                {
//                                                    case LRItemTodoKind.Shift:
//                                                        {

//                                                        }
//                                                        break;
//                                                    default: throw new NotSupportedException();
//                                                }
//                                            }
//                                        }

//                                    }
//                                    break;
//                                default:
//                                    {
//                                        LRItemTodo latestGoto = states.FindLatestGoto();
//                                        SymbolSequence seq = myLRParsingTable.GetSequence(latestGoto.NextItemSetNumber);
//                                        //check if the token can appear in seq
//                                        //heuristic
//                                        int foundIndex = -1;
//                                        int enter_index2 = reader.InputIndex;
//                                        int lookupNCount = 10;
//                                        int lookupTimes = 0;
//                                        for (;;)
//                                        {

//                                            foundIndex = CanTokenApprearInSeq(seq, tk.TkInfo);
//                                            lookupTimes++;
//                                            if (lookupTimes > lookupNCount)
//                                            {
//                                                break;
//                                            }

//                                            if (foundIndex < 0)
//                                            {
//                                                tk = reader.ReadNext();
//                                            }
//                                            else
//                                            {
//                                                break;
//                                            }
//                                        }

//                                        if (foundIndex > -1)
//                                        {
//                                            //try insert missing tokens
//                                            todo = myLRParsingTable.FastGetTodo(current_state, tk.TkInfo);
//                                            if (todo == 0)
//                                            {
//                                                return new ParseResult(null, ParseResultKind.Error, reader.InputIndex);
//                                            }
//                                            else
//                                            {
//                                                //insert missing token
//                                                //find expected token next
//                                                //reader.ReadNextUntil(expectedTokenDefs);  
//                                                //tk = new Token(expectedTokenDefs[0]);
//                                                switch ((LRItemTodoKind)(todo & 0xFF))
//                                                {
//                                                    case LRItemTodoKind.Shift:
//                                                        {
//                                                            // reader.SetIndex(reader.InputIndex - 1);
//                                                        }
//                                                        break;
//                                                    default: throw new NotSupportedException();

//                                                }
//                                            }
//                                        }
//                                        else
//                                        {
//                                            //we should skip this tk
//                                            todo = myLRParsingTable.FastGetTodo(current_state, expectedTokenDefs[0]);
//                                            if (todo == 0)
//                                            {
//                                                return new ParseResult(null, ParseResultKind.Error, reader.InputIndex);
//                                            }
//                                            else
//                                            {
//                                                //insert missing token
//                                                //find expected token next
//                                                //reader.ReadNextUntil(expectedTokenDefs);  
//                                                tk = new Token(expectedTokenDefs[0]);
//                                                switch ((LRItemTodoKind)(todo & 0xFF))
//                                                {
//                                                    case LRItemTodoKind.Shift:
//                                                        {

//                                                        }
//                                                        break;
//                                                    default: throw new NotSupportedException();
//                                                }
//                                            }
//                                        }
//                                    }
//                                    break;

//                            }
//                        }
//                        else
//                        {
//                            //force switch back
//                            return new ParseResult(null, ParseResultKind.Error, reader.InputIndex);
//                        }
//                    }
//                }

//                switch ((LRItemTodoKind)(todo & 0xFF))
//                {
//                    default:
//                    case LRItemTodoKind.Err:
//                        {
//#if DEBUG
//                            if (this.dbugWriteParseLog)
//                            {
//                                var dbugWriter = reader.dbugLogWriter;
//                                dbugWriter.Write('(');
//                                dbugWriter.Write(symbolParseNodes.Count.ToString());
//                                dbugWriter.Write(')');


//                                dbugWriter.Write(new string(' ', symbolParseNodes.Count * 2));
//                                dbugWriter.Write(tkinfo.ToString() + " err:" + (todo >> 8));
//                                dbugWriter.Write(' ');
//                                dbugWriter.Flush();

//                            }
//#endif

//                            throw new NotSupportedException();
//                        }
//                    case LRItemTodoKind.ResolveSwitch:
//                        {

//                            //------------------
//                            //consult switch table with switch record number
//                            //for 2 values
//                            //1. nextStateNumber
//                            //2. parseNameIndex 
//                            //------------------

//                            //resolve switch 
//                            //------------------
//                            //consult switch table with switch record number
//                            //for 2 values
//                            //1. nextStateNumber
//                            //2. parseNameIndex 
//                            //------------------

//                            //int toParserName, nextStateNumber;
//                            if (tk.TkInfo.IsEOF)
//                            {
//                                //no actual switch
//                                //symbolParseNodes.Push(new NonTerminalParseNode(new EmptyParseNode(), null));
//                                //this.table.GetUnresolvedSwitchRecord(todo.SwitchRecordNumber, out toParserName, out nextStateNumber);
//                                //states.Push(current_state = nextStateNumber, todo);
//                                continue;
//                            }
//                            //make switch decision *** 


//                            swContext.SwitchDetail = table.GetSwitchDetail(todo >> 8);
//                            swContext.SwitchBackParseResult = new ParseResult();//result 
//                            swHandler(swContext);

//                            symbolParseNodes.Push(swContext.SwitchBackParseNode);
//                            states.Push(current_state = swContext.SwitchBackState);

//                            tk = reader.CurrentToken;
//                            continue;
//                        }
//                    case LRItemTodoKind.UnresolvedSwitch:
//                        {
//                            //int toParserName, nextStateNumber;
//                            if (tk.TkInfo.IsEOF)
//                            {
//                                //no actual switch
//                                //symbolParseNodes.Push(new NonTerminalParseNode(new EmptyParseNode(), null));
//                                //this.table.GetUnresolvedSwitchRecord(todo.SwitchRecordNumber, out toParserName, out nextStateNumber);
//                                //states.Push(current_state = nextStateNumber, todo);
//                                continue;
//                            }
//                            //make switch decision *** 
//                            //var prevLookFor = swContext.LookFor;


//                            swContext.SwitchDetail = table.GetSwitchDetail(todo >> 8);
//                            swContext.SwitchBackParseResult = new ParseResult();//result 
//                            swHandler(swContext);

//                            symbolParseNodes.Push(swContext.SwitchBackParseNode);
//                            states.Push(current_state = swContext.SwitchBackState);

//                            tk = reader.CurrentToken;
//                            continue;
//                        }
//                    case LRItemTodoKind.Shift:
//                        {
//                            symbolParseNodes.Push(tk);
//                            states.Push(current_state = (todo >> 8));
//                            tk = reader.ReadNext();
//                        }
//                        break;
//                    case LRItemTodoKind.ConflictRR:
//                        {
//                            var seqChoices = GetKnownRRConflict(current_state, tkinfo);
//                            if (seqChoices == null)
//                            {
//                                throw new NotSupportedException();
//                            }

//                            current_state = states.PopAndPeek();
//                            ParseNode singleWaitNode = symbolParseNodes.Pop();

//                            //just pop away?, as if it dose not exist? 
//                            NonTerminalParseNode nt_parseNode = null;
//                            if (singleWaitNode.IsTerminalNode)
//                            {
//                                throw new NotSupportedException();
//                                //terminal node -> add node info
//                                //make reduction ***
//                                //nt_parseNode = new NonTerminalParseNode((Token)singleWaitNode, swContext.GetNewTicket());
//                            }
//                            else
//                            {
//                                throw new NotSupportedException();
//                                //if there is one node and it is nt
//                                //we don't create new nt,just add reduction info 
//                                nt_parseNode = (NonTerminalParseNode)singleWaitNode;
//                                //?
//                                throw new NotSupportedException();
//                            }
//                        }
//                        break;
//                    case LRItemTodoKind.Accept:
//                        {
//                            if (symbolParseNodes.Count != 1)
//                            {
//                                throw new NotSupportedException();
//                            }

//                            finalNode = symbolParseNodes.Pop();
//                            this.FinalNode = finalNode;
//                            goto EXIT;
//                        }
//                    case LRItemTodoKind.Goto:
//                        {
//                        }
//                        break;
//                    case LRItemTodoKind.Reduce:
//                        {

//                            //same as state number
//                            SymbolSequence reduceToSq = myLRParsingTable.GetSequence((todo >> 8));
//                            //check if we need  
//                            //1. a new node or 
//                            //2. just add reduction information to existing node
//                            //if number of symbol > 1 or just one terminal symbol 
//                            //then we create a nonterminal and wrap it in this step

//                            int symbolCount = reduceToSq.RightSideCount;

//                            switch (symbolCount)
//                            {
//                                case 0:
//                                    {
//                                        throw new NotSupportedException();
//                                    }
//                                case 1:
//                                    {
//                                        current_state = states.PopAndPeek();
//                                        if (reduceToSq.FromBifurcation)
//                                        {
//                                            symbolParseNodes.Push(new NonTerminalParseNode(symbolParseNodes.Pop(), reduceToSq));
//                                        }
//                                        else
//                                        {
//                                            //use Peek()***
//                                            symbolParseNodes.Peek().AddInheritUpSequence(reduceToSq);
//                                        }

//                                    }
//                                    break;
//                                case 2:
//                                    {
//                                        current_state = states.Pop(2);
//                                        if (reduceToSq.CreatedFromListOfNt)
//                                        {

//                                            ParseNode p1, p2;
//                                            symbolParseNodes.Pop(out p1, out p2);
//                                            if (p1.IsList)
//                                            {
//                                                NonTerminalParseNodeList ntList = (NonTerminalParseNodeList)p1;
//                                                ntList.AddParseNode(p2);
//                                                symbolParseNodes.Push(ntList);
//                                            }
//                                            else
//                                            {
//                                                //just create and
//                                                NonTerminalParseNodeList ntList = new NonTerminalParseNodeList(reduceToSq);
//                                                //add member
//                                                ntList.AddParseNode(p1);
//                                                ntList.AddParseNode(p2);
//                                                symbolParseNodes.Push(ntList);
//                                            }
//                                        }
//                                        else
//                                        {
//                                            symbolParseNodes.Push(new NonTerminalParseNode(symbolParseNodes.PopReverseIntoArray(2), reduceToSq));
//                                        }
//                                    }
//                                    break;
//                                case 3:
//                                    {

//                                        current_state = states.Pop(3);
//                                        if (reduceToSq.CreatedFromListOfNt)
//                                        {

//                                            ParseNode p1, p2, p3;
//                                            symbolParseNodes.Pop(out p1, out p2, out p3);
//                                            if (p1.IsList)
//                                            {
//                                                NonTerminalParseNodeList ntList = (NonTerminalParseNodeList)p1;
//                                                ntList.AddParseNode(p2);
//                                                ntList.AddParseNode(p3);
//                                                symbolParseNodes.Push(ntList);
//                                            }
//                                            else
//                                            {   //just create and
//                                                NonTerminalParseNodeList ntList = new NonTerminalParseNodeList(reduceToSq);
//                                                //add member
//                                                ntList.AddParseNode(p1);
//                                                ntList.AddParseNode(p2);
//                                                ntList.AddParseNode(p3);
//                                                symbolParseNodes.Push(ntList);
//                                            }
//                                        }
//                                        else
//                                        {
//                                            symbolParseNodes.Push(new NonTerminalParseNode(symbolParseNodes.PopReverseIntoArray(3), reduceToSq));
//                                        }
//                                    }
//                                    break;
//                                default:
//                                    {
//                                        current_state = states.Pop(symbolCount);
//                                        symbolParseNodes.Push(new NonTerminalParseNode(symbolParseNodes.PopReverseIntoArray(symbolCount), reduceToSq));
//                                    }
//                                    break;
//                            }

//                            //------------------------------------------------------------ 
//                            //if (breakOnReduce)
//                            //{
//                            //    //reporter.Result = nt_parseNode;
//                            //    //invoke for break step only
//                            //    reduceToSq.NotifyEvent(ParseEventKind.Reduce, reporter);
//                            //}
//                            //------------------------------------------------------------

//#if DEBUG
//                            ////-------------------------------------------------------------------
//                            //if (this.dbugWriteParseLog)
//                            //{
//                            //    var writer = reader.dbugLogWriter;
//                            //}
//                            ////-------------------------------------------------------------------
//#endif

//                            //reporter.Result = nt_parseNode;
//                            //reporter.selectedSymbolSq = reduceToSq;
//                            //-------------------------------------------------------------------
//                            //dev mode: may invoke notification here                             
//                            //reduceToSq.NotifyEvent(reporter);
//                            //-------------------------------------------------------------------

//                            //goto

//                            states.Push(current_state = (myLRParsingTable.FastGetTodo(current_state, reduceToSq.LeftSideNT) >> 8));

//                        }
//                        break;

//                }
//            }
//            //---------------
//            EXIT:
//            return new ParseResult(finalNode, ParseResultKind.OK, swContext.Reader.InputIndex);
//        }
//    }
//}