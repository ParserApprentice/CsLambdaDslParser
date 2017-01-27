//MIT, 2015-2017, ParserApprentice
using System; 
using System.Collections.Generic; 
using Parser.ParserKit.LR;
using Parser.ParserKit.SubParsers;

namespace Parser.ParserKit
{
    public abstract class SubParser
    {


        protected MiniGrammarSheet _miniGrammarSheet;
        protected NTDefinition _augmentedNTDefinition;
        protected TopUserNTDefinition  _rootNtDef;

        //------------------------------
        protected LRParsingTable _parsingTable;
        protected LRParser _actualLRParser;
        //------------------------------



        public SubParser()
        {

        }
        internal abstract List<SyncSequence> GetSyncSeqs();

        public bool TryUseTableDataCache { get; set; }

        protected abstract void InternalSetup(TokenInfoCollection tkInfoCollection);


        public void Setup(TokenInfoCollection tkInfoCollection)
        {


            //---------------------------------------
            InternalSetup(tkInfoCollection);
            //--------------------------------------- 
            if (!TryUseTableDataCache)
            {
                _parsingTable.MakeParsingTable();
            }
            else
            {
                ParserDataBinaryCache binaryCache = LRTableReaderWriter.LoadTableDataFromBinaryFile(
                    this.RootNtName + ".tablecache");
                if (binaryCache != null &&
                    binaryCache.SuccessLoaded &&
                    binaryCache.CompareWithTable(_parsingTable))
                {
                    //check compat 
                    //if compat then use the cache version ,else -> just recreate
                    //use that binary cache
                    _parsingTable.MakeParsingTableFromCache(binaryCache);
                }
                else
                {
                    _parsingTable.MakeParsingTable();
                    //autosave if use cache 
                    SubParserCache.SaveAsBinaryFile(this, this.RootNtName + ".tablecache");
                }
            }
            _actualLRParser = LRParsing.CreateRunner(_parsingTable);
            //---------------------------------------  
            //check root nt
#if DEBUG
            _parsingTable.dbugName = (this.GetType()).Name;
            if (this.RootNt == null)
            {
                throw new NotSupportedException();
            }
#endif
        }


        public LRParsingTable InternalParsingTable { get { return _parsingTable; } }

        public void SetParserSwitchHandler(LRParserSwitchHandler swHandler)
        {
            _actualLRParser.SetSwitchHandler(swHandler);
        }
        public void SetFastParseMode(bool enable)
        {
            if (_actualLRParser != null)
            {
                _actualLRParser.UseFastParseMode = enable;
            }
        }
        public void SetBreakOnShift(bool enable)
        {
            if (_actualLRParser != null)
            {
                _actualLRParser.EnableBreakOnShift = enable;
            }
        }
        public void SetEnableBreakOnReduce(bool enable)
        {
            if (_actualLRParser != null)
            {
                _actualLRParser.EnableBreakOnReduce = enable;
            }
        }
        public bool UseBreakOnReduce
        {
            get;
            set;
        }
        public void Parse(ParseNodeHolder holder, TokenStreamReader reader)
        {
            _actualLRParser.Parse(holder, reader);
        }

        public void Parse(ParserSwitchContext sw)
        {
            sw.BeginSwitch();

            //save prev value
            LR.LRParsingContext prevSw = sw.CurrentLRParsingContext;

            //set new one
            sw.CurrentLRParsingContext = sw.GetLRParsingContext();
            sw.SwitchBackParseResult = _actualLRParser.Parse(sw);
            sw.RelaseLRParsingContext(sw.CurrentLRParsingContext);

            sw.CurrentLRParsingContext = prevSw; //restore prev value

            sw.EndSwitch();

        }

        public bool StartWith(Token tk)
        {
            return _actualLRParser.StartWith(tk.TkInfo.TokenInfoNumber);
        }
        public ParseNode FinalNode
        {
            get { return _actualLRParser.FinalNode; }
        }


        protected UserNTDefinition RootNt { get { return _rootNtDef; } }


        public string RootNtName
        {
            get { return _rootNtDef.Name; }
        }



        //------------------------------------------------------------------------------------------------
        static SubParser FindSubParser(Dictionary<string, SubParser> dic, string subParserName)
        {
            SubParser found;
            dic.TryGetValue(subParserName, out found);
            return found;
        }
        public void PrepareSwitchTable(Dictionary<string, SubParser> otherParsers)
        {

            List<SwitchDetail> swDetailRecords = _parsingTable.swDetailRecords;
            for (int i = swDetailRecords.Count - 1; i > 0; --i) //start at 1 ( so i >0)
            {
                SwitchDetail swDetail = swDetailRecords[i];
                for (int p = swDetail.Count - 1; p >= 0; --p)
                {
                    SwitchPair swpair = swDetail.GetSwPair(p);
                    swpair.resolvedSubParser = FindSubParser(otherParsers, swpair.symbolName);
#if DEBUG
                    if (swpair.resolvedSubParser == null)
                    {
                        DebugConsole.WriteLine("unresolved parser name " + swpair.symbolName);
                    }
#endif
                }

                swDetail.IsResolved = true;
                swDetail.PrepareSyncTable();
            }
        }
        public void CompactTable(bool alsoClearOriginalLRTable)
        {
            this._parsingTable.CompactTable(alsoClearOriginalLRTable);
        }

        struct LRItemToSource
        {
            public readonly LRItemTodo todo;
            public readonly SubParser fromSubParser;
            public LRItemToSource(LRItemTodo todo, SubParser fromSubParser)
            {
                this.todo = todo;
                this.fromSubParser = fromSubParser;
            }

        }

#if DEBUG
        static int dbugResolveCount;
#endif
    }




    public class LateSymbolCollection
    {
        List<USymbol> lateSymbols = new List<USymbol>();
        public void AddLateCreatedUserNt(USymbol lateSymbol)
        {
            this.lateSymbols.Add(lateSymbol);
        }
        public List<USymbol> GetLateSymbols()
        {
            return this.lateSymbols;
        }

    }

    static class UserNTSubParserExtension
    {
        static int totalAutoNum;
        static int GetAutoNum()
        {
            return totalAutoNum++;
        }

        static UserExpectedSymbol CreateUserExpectedSymbol(LateSymbolCollection lateSymbols, object expSS)
        {
            SymbolWithStepInfo symbolReduction = expSS as SymbolWithStepInfo;
            ReductionMonitor reductionDel = null;
            ParserNotifyDel shiftDel = null;
            if (symbolReduction != null)
            {
                //this is symbol reduction
                expSS = symbolReduction.symbol;
                reductionDel = symbolReduction.reductionDel;
                shiftDel = symbolReduction.shiftDel;
            }

            OptSymbol optSymbol = expSS as OptSymbol;
            bool isOpt = false;
            if (optSymbol != null)
            {
                isOpt = true;
                expSS = optSymbol.ss;
            }
            //--------------------------------------------------------
            TokenInfoCollection tkinfoCollection = ReflectionSubParser.s_tkInfoCollection;
            //content is user nt
            UserNTDefinition ntss = expSS as UserNTDefinition;
            if (ntss != null)
            {
                return new UserExpectedSymbol(ntss, isOpt, shiftDel) { ReductionDel = reductionDel };
            }
            //----------------------------------------------------------
            //content is string
            TokenDefinition tkdef = null;
            if (expSS is string)
            {
                //should be sometoken , not nt

                tkdef = tkinfoCollection.GetTokenInfo((string)expSS);
                if (tkdef != null)
                {
                    return new UserExpectedSymbol(tkdef, isOpt, shiftDel) { ReductionDel = reductionDel };
                }
            }
            //----------------------------------------------------------
            UserTokenDefinition utkdef = expSS as UserTokenDefinition;
            if (utkdef != null)
            {
                tkdef = utkdef.TkDef;
                return new UserExpectedSymbol(tkdef, isOpt, shiftDel) { ReductionDel = reductionDel };
            }
            //---------------------------------------------------------- 
            //content is TokenDefiniton
            tkdef = expSS as TokenDefinition;
            if (tkdef != null)
            {
                return new UserExpectedSymbol(tkdef, isOpt, shiftDel) { ReductionDel = reductionDel };
            }
            //----------------------------------------------------------
            OneOfSymbol oneOf = expSS as OneOfSymbol;
            if (oneOf != null)
            {
                //auto create oneof symbol 
                //throw new NotSupportedException();
                UserNTDefinition newOneOfNt = new UserNTDefinition("oneof_" + (GetAutoNum()).ToString());
                object[] symbols = oneOf.symbols;
                int j = symbols.Length;
                for (int i = 0; i < j; ++i)
                {
                    UserExpectedSymbol ues1 = CreateUserExpectedSymbol(lateSymbols, symbols[i]);
                    var seq1 = new UserSymbolSequence(newOneOfNt);
                    seq1.AppendLast(ues1);
                    newOneOfNt.AddSymbolSequence(seq1);
                }
                newOneOfNt.IsAutoGen = true;
                lateSymbols.AddLateCreatedUserNt(newOneOfNt);
                return new UserExpectedSymbol(newOneOfNt, isOpt, shiftDel) { ReductionDel = reductionDel };
            }
            //----------------------------------------------------------
            //content is listof ...
            ListSymbol listOf = expSS as ListSymbol;
            if (listOf != null)
            {
                //list of what?
                UserExpectedSymbol ues1 = CreateUserExpectedSymbol(lateSymbols, listOf.ss); //for seq1
                UserExpectedSymbol ues2 = CreateUserExpectedSymbol(lateSymbols, listOf.ss); //for seq2 
                UserExpectedSymbol sep_ss = null;
                if (listOf.sep != null)
                {
                    //sep must be token (in form of string or tokendefinition
                    TokenDefinition sepTk = null;
                    if (listOf.sep is string)
                    {
                        sepTk = tkinfoCollection.GetTokenInfo((string)(listOf.sep));
                    }
                    else if (listOf.sep is TokenDefinition)
                    {
                        sepTk = (TokenDefinition)listOf.sep;
                    }

                    if (sepTk == null)
                    {
                        //can't convert
                        throw new NotSupportedException();
                    }
                    sep_ss = new UserExpectedSymbol(sepTk, false, shiftDel) { ReductionDel = reductionDel };
                }

                UserNTDefinition newListOfUserNT = new UserNTDefinition("list_of" + (GetAutoNum()).ToString());
                newListOfUserNT.IsAutoGen = true;
                var seq1 = new UserSymbolSequence(newListOfUserNT);
                seq1.AppendLast(ues1);
                seq1.CreatedFromListOfNt = true;
                var seq2 = new UserSymbolSequence(newListOfUserNT);
                seq2.AppendLast(new UserExpectedSymbol(newListOfUserNT));
                seq2.CreatedFromListOfNt = true;
                if (sep_ss != null)
                {
                    seq2.AppendLast(sep_ss);
                }
                seq2.AppendLast(ues2);
                newListOfUserNT.AddSymbolSequence(seq1);
                newListOfUserNT.AddSymbolSequence(seq2);
                //create autogenerate user nt for list symbol  
                lateSymbols.AddLateCreatedUserNt(newListOfUserNT);

                return new UserExpectedSymbol(newListOfUserNT, isOpt, shiftDel) { ReductionDel = reductionDel };
            }
            //----------
            //unknown
            throw new NotSupportedException();
        }

        internal static UserSymbolSequence CreateUserSymbolSeq(UserNTDefinition ntdef, params object[] expectedSymbols)
        {
            UserSymbolSequence ss = new UserSymbolSequence(ntdef);
            LateSymbolCollection lateSymbols = new LateSymbolCollection();

            int j = expectedSymbols.Length;
            //check if symbol or delegate
            for (int i = 0; i < j; ++i)
            {
                ss.AppendLast(CreateUserExpectedSymbol(lateSymbols, expectedSymbols[i]));
            }
            ss.ClearParserReductionNotifyDel();
            ntdef.AddSymbolSequence(ss);

            List<USymbol> lateSymbolList = lateSymbols.GetLateSymbols();
            foreach (USymbol usymbol in lateSymbolList)
            {
                if (usymbol is UserNTDefinition)
                {
                    ntdef.AddLateCreatedUserNt((UserNTDefinition)usymbol);
                }
                else
                {
                    throw new NotSupportedException();
                }

            }
            return ss;
        }
    }




}