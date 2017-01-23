//MIT 2015-2017, ParserApprentice 
using System;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using Parser.ParserKit.LR;
using Parser.ParserKit.SubParsers;

namespace Parser.ParserKit
{
    public abstract class SubParser
    {
        protected class RootHolder
        {
            public UserNTDefinition _rootNtDef;
            public RootHolder(UserNTDefinition rootNtDef)
            {
                this._rootNtDef = rootNtDef;
            }
            public string Name
            {
                get
                {
                    return this._rootNtDef.Name;
                }
            }
        }
        protected TokenInfoCollection _tkInfoCollection;
        protected MiniGrammarSheet _miniGrammarSheet;
        protected NTDefinition _augmentedNTDefinition;
        protected RootHolder _rootNtDef;

        //------------------------------
        protected LRParsingTable _parsingTable;
        protected LRParser _actualLRParser;
        //------------------------------

        int _autoNumber;
        protected internal UserSymbolSequence currentSq;

        public SubParser()
        {

        }
        internal List<SyncSequence> GetSyncSeqs()
        {
            return _syncSeqs;
        }
        public bool TryUseTableDataCache { get; set; }

        protected abstract void InternalSetup(TokenInfoCollection tkInfoCollection);


        public void Setup(TokenInfoCollection tkInfoCollection)
        {
            _autoNumber = 0;
            _tkInfoCollection = tkInfoCollection;
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


        internal LRParsingTable InternalParsingTable { get { return _parsingTable; } }
        internal int GetNewAutoNumber()
        {
            return _autoNumber++;
        }
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



        protected abstract void Define();
        protected UserNTDefinition RootNt { get { return _rootNtDef._rootNtDef; } }


        public string RootNtName
        {
            get { return _rootNtDef.Name; }
        }
        internal List<SyncSequence> _syncSeqs;
        internal static void SetCurrentUserSymbolSeq(SubParser subParser, UserSymbolSequence userSymbolSeq)
        {
            subParser.currentSq = userSymbolSeq;
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


    public abstract class ReflectionSubParser : SubParser
    {

        List<UserNTDefinition> _initUserNts;
        List<UserNTDefinition> _lateNts;
        public void AddLateCreatedUserNt(UserNTDefinition lateNt)
        {
            if (_lateNts == null)
            {
                _lateNts = new List<UserNTDefinition>();
            }
            _lateNts.Add(lateNt);
        }
        public TokenDefinition GetTokenDefintion(string grammarPresentationString)
        {
            return _tkInfoCollection.GetTokenInfo(grammarPresentationString);
        }
        public abstract string GetTokenPresentationName(string fieldname);

        protected OptSymbol opt(TokenDefinition tk)
        {
            return new OptSymbol(tk);
        }
        protected OptSymbol opt(UserNTDefinition nt)
        {
            return new OptSymbol(nt);
        }
        protected OptSymbol opt(ListSymbol listSymbol)
        {
            return new OptSymbol(listSymbol);
        }
        protected OptSymbol opt(OneOfSymbol oneofSymbol)
        {
            return new OptSymbol(oneofSymbol);
        }
        protected ListSymbol list(UserNTDefinition nt)
        {
            return new ListSymbol(nt);
        }
        protected ListSymbol list(TokenDefinition tk)
        {
            return new ListSymbol(tk);
        }
        protected ListSymbol list(UserNTDefinition nt, TokenDefinition sep)
        {
            return new ListSymbol(nt, sep);
        }
        protected ListSymbol list(TokenDefinition tk, TokenDefinition sep)
        {
            return new ListSymbol(tk, sep);
        }
        protected OneOfSymbol oneof(params object[] symbols)
        {
            return new OneOfSymbol(symbols);
        }

        protected void set_prec(int prec)
        {
            //assign prec to current seq
            currentSq.Precedence = prec;
            for (int i = currentSq.RightCount - 1; i >= 0; --i)
            {
                UserExpectedSymbol ues = currentSq[i];
                if (ues.SymbolKind == UserExpectedSymbolKind.Nonterminal)
                {
                    UserNTDefinition unt = ues.ResolvedUserNtDef;
                    if (unt.IsAutoGen)
                    {
                        unt.NTPrecedence = prec;

                    }
                    else
                    {
                        // Console.WriteLine(unt);
                    }
                }
            }
        }
        protected void sync(params TokenDefinition[] syncTks)
        {
            if (_syncSeqs == null)
            {
                _syncSeqs = new List<SyncSequence>();
            }

            int j = syncTks.Length;
            SeqSyncCmd[] syncCmds = new SeqSyncCmd[j];
            for (int i = 0; i < j; ++i)
            {
                syncCmds[i] = new SeqSyncCmd(SyncCmdName.Match, syncTks[i]);
            }

            _syncSeqs.Add(new SyncSequence(syncCmds));
        }
        protected void sync_start(TokenDefinition startSync)
        {
            if (_syncSeqs == null)
            {
                _syncSeqs = new List<SyncSequence>();
            }
            SeqSyncCmd[] syncCmds = new SeqSyncCmd[]{
                 new SeqSyncCmd(SyncCmdName.First, startSync)
            };
            _syncSeqs.Add(new SyncSequence(syncCmds));
        }
        internal SyncSymbol skip(TokenDefinition begin, TokenDefinition end)
        {
            //ignor this pair
            return new SyncSymbol(begin, end, SyncSymbolKind.Ignor);
        }

        protected override void InternalSetup(TokenInfoCollection tkInfoCollection)
        {
            _initUserNts = new List<UserNTDefinition>();


            Type t = this.GetType();
            FieldInfo[] fields = t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);


            //init field
            foreach (FieldInfo f in fields)
            {

                if (f.FieldType == typeof(TopUserNtDefinition))
                {
                    var unt = new TopUserNtDefinition(f.Name);
                    unt.OwnerSubParser = this;
                    _initUserNts.Add(unt);
                    f.SetValue(this, unt);
                    //---------------------
                    if (_rootNtDef != null)
                    {
                        //must has only1
                        throw new NotSupportedException();
                    }
                    this._rootNtDef = new RootHolder(unt);
                    //---------------------
                }
                else if (f.FieldType == typeof(UserNTDefinition))
                {
                    //init this field
                    //user field name              
                    var unt = new UserNTDefinition(f.Name);
                    unt.OwnerSubParser = this;
                    _initUserNts.Add(unt);
                    f.SetValue(this, unt);
                }
                else if (f.FieldType == typeof(UserTokenDefinition))
                {
                    var fieldValue = f.GetValue(this) as UserTokenDefinition;
                    if (fieldValue == null)
                    {
                        //no init value 
                        f.SetValue(this, new UserTokenDefinition(tkInfoCollection.GetTokenInfo(GetTokenPresentationName(f.Name))));
                    }
                    else
                    {
                        //use presentation string 
                        fieldValue.TkDef = tkInfoCollection.GetTokenInfo(fieldValue.GrammarString);
                    }

                }
                else if (f.FieldType == typeof(TokenDefinition))
                {
                    //get token from grammar sheet
                    //get existing value 
                    var fieldValue = f.GetValue(this) as TokenDefinition;
                    if (fieldValue == null)
                    {
                        f.SetValue(this, tkInfoCollection.GetTokenInfo(GetTokenPresentationName(f.Name)));
                    }
                }
                else
                {

                }
            }
            //---------------------------------------
            Define();
            //--------------------------------------- 
            if (_lateNts != null)
            {
                _initUserNts.AddRange(_lateNts);
            }

            //check if all user nt is define
            for (int i = _initUserNts.Count - 1; i >= 0; --i)
            {
                UserNTDefinition unt = _initUserNts[i];
                if (unt.UserSeqCount == 0)
                {
                    //this nt is not defined!
                    unt.MarkedAsUnknownNT = true;
                }
            }

            //---------------------------------------
            _miniGrammarSheet = new MiniGrammarSheet();
            _miniGrammarSheet.LoadTokenInfo(tkInfoCollection);
            _miniGrammarSheet.LoadUserNts(_initUserNts);
            //--------------------------------------- 
            _augmentedNTDefinition = _miniGrammarSheet.PrepareUserGrammarForAnyLR(this.RootNt);
            _parsingTable = _miniGrammarSheet.CreateLR1Table(_augmentedNTDefinition);
            //can use cache table 
            //---------------------------------------    
            //sync parser table
            //if (_syncNts != null && _syncNts.Count > 0)
            //{
            //    _syncParser = new SyncParser();
            //    _syncParser.Setup(tkInfoCollection, this._syncNts);
            //}
        }
    }



    public abstract class ReflectionSubParser<T> : ReflectionSubParser
    {

        GetWalkerDel<T> getWalkerDel;

        public GetWalkerDel<T> GetWalker
        {
            get { return getWalkerDel; }
            set { getWalkerDel = value; }
        }


        protected NtDefAssignSet<T> _(BuilderDel3<T> reductionDel, UserExpectedSymbolDef<T> s1)
        {
            return new NtDefAssignSet<T>(getWalkerDel, null, reductionDel, new[] { s1 });
        }
        protected NtDefAssignSet<T> _(BuilderDel3<T> reductionDel,
            UserExpectedSymbolDef<T> s1,
            UserExpectedSymbolDef<T> s2)
        {
            return new NtDefAssignSet<T>(getWalkerDel, null, reductionDel, new[] { s1, s2 });
        }
        protected NtDefAssignSet<T> _(BuilderDel3<T> reductionDel,
            UserExpectedSymbolDef<T> s1,
            UserExpectedSymbolDef<T> s2,
            UserExpectedSymbolDef<T> s3)
        {
            return new NtDefAssignSet<T>(getWalkerDel, null, reductionDel, new[] { s1, s2, s3 });
        }
        protected NtDefAssignSet<T> _(BuilderDel3<T> reductionDel,
            UserExpectedSymbolDef<T> s1,
            UserExpectedSymbolDef<T> s2,
            UserExpectedSymbolDef<T> s3,
            UserExpectedSymbolDef<T> s4)
        {
            return new NtDefAssignSet<T>(getWalkerDel, null, reductionDel, new[] { s1, s2, s3, s4 });
        }
        protected NtDefAssignSet<T> _(BuilderDel3<T> reductionDel,
             UserExpectedSymbolDef<T> s1,
             UserExpectedSymbolDef<T> s2,
             UserExpectedSymbolDef<T> s3,
             UserExpectedSymbolDef<T> s4,
             UserExpectedSymbolDef<T> s5)
        {
            return new NtDefAssignSet<T>(getWalkerDel, null, reductionDel, new[] { s1, s2, s3, s4, s5 });
        }
        protected NtDefAssignSet<T> _(BuilderDel3<T> reductionDel,
            UserExpectedSymbolDef<T> s1,
            UserExpectedSymbolDef<T> s2,
            UserExpectedSymbolDef<T> s3,
            UserExpectedSymbolDef<T> s4,
            UserExpectedSymbolDef<T> s5,
            UserExpectedSymbolDef<T> s6)
        {
            return new NtDefAssignSet<T>(getWalkerDel, null, reductionDel, new[] { s1, s2, s3, s4, s5, s6 });
        }
        protected NtDefAssignSet<T> _(BuilderDel3<T> reductionDel,
           UserExpectedSymbolDef<T> s1,
           UserExpectedSymbolDef<T> s2,
           UserExpectedSymbolDef<T> s3,
           UserExpectedSymbolDef<T> s4,
           UserExpectedSymbolDef<T> s5,
           UserExpectedSymbolDef<T> s6,
           UserExpectedSymbolDef<T> s7)
        {
            return new NtDefAssignSet<T>(getWalkerDel, null, reductionDel, new[] { s1, s2, s3, s4, s5, s6, s7 });
        }
        protected NtDefAssignSet<T> _(BuilderDel3<T> reductionDel,
           UserExpectedSymbolDef<T> s1,
           UserExpectedSymbolDef<T> s2,
           UserExpectedSymbolDef<T> s3,
           UserExpectedSymbolDef<T> s4,
           UserExpectedSymbolDef<T> s5,
           UserExpectedSymbolDef<T> s6,
           UserExpectedSymbolDef<T> s7,
           UserExpectedSymbolDef<T> s8)
        {
            return new NtDefAssignSet<T>(getWalkerDel, null, reductionDel, new[] { s1, s2, s3, s4, s5, s6, s7, s8 });
        }
        protected NtDefAssignSet<T> _(BuilderDel3<T> reductionDel,
           UserExpectedSymbolDef<T> s1,
           UserExpectedSymbolDef<T> s2,
           UserExpectedSymbolDef<T> s3,
           UserExpectedSymbolDef<T> s4,
           UserExpectedSymbolDef<T> s5,
           UserExpectedSymbolDef<T> s6,
           UserExpectedSymbolDef<T> s7,
           UserExpectedSymbolDef<T> s8,
           UserExpectedSymbolDef<T> s9)
        {
            return new NtDefAssignSet<T>(getWalkerDel, null, reductionDel, new[] { s1, s2, s3, s4, s5, s6, s7, s8, s9 });
        }
        protected NtDefAssignSet<T> _(BuilderDel3<T> reductionDel,
            UserExpectedSymbolDef<T> s1,
            UserExpectedSymbolDef<T> s2,
            UserExpectedSymbolDef<T> s3,
            UserExpectedSymbolDef<T> s4,
            UserExpectedSymbolDef<T> s5,
            UserExpectedSymbolDef<T> s6,
            UserExpectedSymbolDef<T> s7,
            UserExpectedSymbolDef<T> s8,
            UserExpectedSymbolDef<T> s9,
            UserExpectedSymbolDef<T> s10
            )
        {
            return new NtDefAssignSet<T>(getWalkerDel, null, reductionDel, new[] { s1, s2, s3, s4, s5, s6, s7, s8, s9, s10 });
        }
        protected NtDefAssignSet<T> _(BuilderDel3<T> reductionDel,
           UserExpectedSymbolDef<T> s1,
           UserExpectedSymbolDef<T> s2,
           UserExpectedSymbolDef<T> s3,
           UserExpectedSymbolDef<T> s4,
           UserExpectedSymbolDef<T> s5,
           UserExpectedSymbolDef<T> s6,
           UserExpectedSymbolDef<T> s7,
           UserExpectedSymbolDef<T> s8,
           UserExpectedSymbolDef<T> s9,
           UserExpectedSymbolDef<T> s10,
           UserExpectedSymbolDef<T> s11
           )
        {
            return new NtDefAssignSet<T>(getWalkerDel, null, reductionDel, new[] { s1, s2, s3, s4, s5, s6, s7, s8, s9, s10, s11 });
        }
        protected NtDefAssignSet<T> _(BuilderDel3<T> reductionDel,
           UserExpectedSymbolDef<T> s1,
           UserExpectedSymbolDef<T> s2,
           UserExpectedSymbolDef<T> s3,
           UserExpectedSymbolDef<T> s4,
           UserExpectedSymbolDef<T> s5,
           UserExpectedSymbolDef<T> s6,
           UserExpectedSymbolDef<T> s7,
           UserExpectedSymbolDef<T> s8,
           UserExpectedSymbolDef<T> s9,
           UserExpectedSymbolDef<T> s10,
           UserExpectedSymbolDef<T> s11,
           params UserExpectedSymbolDef<T>[] _else
           )
        {
            int j = _else.Length;
            UserExpectedSymbolDef<T>[] total = new UserExpectedSymbolDef<T>[j + 11];
            total[0] = s1; total[1] = s2; total[2] = s3; total[3] = s4; total[4] = s5;
            total[5] = s6; total[6] = s7; total[7] = s8; total[8] = s9; total[9] = s10;
            total[10] = s11;

            for (int i = 0; i < j; ++i)
            {
                total[i + 11] = _else[i];
            }

            return new NtDefAssignSet<T>(getWalkerDel, null, reductionDel, total);
        }

        protected NtDefAssignSet<T> _oneof(NtDefAssignSet<T> c1, NtDefAssignSet<T> c2, params NtDefAssignSet<T>[] others)
        {
            //at least 2 choices
            int j = others.Length;
            NtDefAssignSet<T>[] choices = new NtDefAssignSet<T>[j + 2];
            choices[0] = c1;
            choices[1] = c2;
            for (int i = 0; i < j; ++i)
            {
                choices[2 + i] = others[i];
            }
            return new NtDefAssignSet<T>(getWalkerDel, choices);
        }


        protected NtDefAssignSet<T> _oneof(
         UserExpectedSymbolDef<T> c1,
         UserExpectedSymbolDef<T> c2)
        {

            return new NtDefAssignSet<T>(getWalkerDel, new NtDefAssignSet<T>[] {
                     new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c1 }),
                     new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c2 })
                });
        }
        protected NtDefAssignSet<T> _oneof(
            UserExpectedSymbolDef<T> c1,
            UserExpectedSymbolDef<T> c2,
            UserExpectedSymbolDef<T> c3)
        {

            return new NtDefAssignSet<T>(getWalkerDel, new NtDefAssignSet<T>[] {
                     new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c1 }),
                     new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c2 }),
                     new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c3 })
                });
        }
        protected NtDefAssignSet<T> _oneof(
           UserExpectedSymbolDef<T> c1,
           UserExpectedSymbolDef<T> c2,
           UserExpectedSymbolDef<T> c3,
           UserExpectedSymbolDef<T> c4)
        {

            return new NtDefAssignSet<T>(getWalkerDel, new NtDefAssignSet<T>[] {
                     new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c1 }),
                     new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c2 }),
                     new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c3 }),
                     new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c4 })
                });
        }
        protected NtDefAssignSet<T> _oneof(
          UserExpectedSymbolDef<T> c1,
          UserExpectedSymbolDef<T> c2,
          UserExpectedSymbolDef<T> c3,
          UserExpectedSymbolDef<T> c4,
          UserExpectedSymbolDef<T> c5)
        {

            return new NtDefAssignSet<T>(getWalkerDel, new NtDefAssignSet<T>[] {
                     new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c1 }),
                     new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c2 }),
                     new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c3 }),
                     new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c4 }),
                     new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c5 })
                });
        }
        protected NtDefAssignSet<T> _oneof(
          UserExpectedSymbolDef<T> c1,
          UserExpectedSymbolDef<T> c2,
          UserExpectedSymbolDef<T> c3,
          UserExpectedSymbolDef<T> c4,
          UserExpectedSymbolDef<T> c5,
          UserExpectedSymbolDef<T> c6)
        {

            return new NtDefAssignSet<T>(getWalkerDel, new NtDefAssignSet<T>[] {
                     new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c1 }),
                     new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c2 }),
                     new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c3 }),
                     new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c4 }),
                     new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c5 }),
                     new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c6 }),
                });
        }
        protected NtDefAssignSet<T> _oneof(
         UserExpectedSymbolDef<T> c1,
         UserExpectedSymbolDef<T> c2,
         UserExpectedSymbolDef<T> c3,
         UserExpectedSymbolDef<T> c4,
         UserExpectedSymbolDef<T> c5,
         UserExpectedSymbolDef<T> c6,
         UserExpectedSymbolDef<T> c7)
        {

            return new NtDefAssignSet<T>(getWalkerDel, new NtDefAssignSet<T>[] {
                     new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c1 }),
                     new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c2 }),
                     new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c3 }),
                     new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c4 }),
                     new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c5 }),
                     new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c6 }),
                     new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c7 }),
                });
        }
        protected NtDefAssignSet<T> _oneof(
         UserExpectedSymbolDef<T> c1,
         UserExpectedSymbolDef<T> c2,
         UserExpectedSymbolDef<T> c3,
         UserExpectedSymbolDef<T> c4,
         UserExpectedSymbolDef<T> c5,
         UserExpectedSymbolDef<T> c6,
         UserExpectedSymbolDef<T> c7,
         params UserExpectedSymbolDef<T>[] others)
        {

            int j = others.Length;
            NtDefAssignSet<T>[] choices = new NtDefAssignSet<T>[j + 7];
            choices[0] = new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c1 });
            choices[1] = new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c2 });
            choices[2] = new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c3 });
            choices[3] = new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c4 });
            choices[4] = new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c5 });
            choices[5] = new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c6 });
            choices[6] = new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c7 });

            for (int i = 0; i < j; ++i)
            {
                choices[7 + i] = new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { others[i] });
            }
            return new NtDefAssignSet<T>(getWalkerDel, choices);
        }



        protected NtDefAssignSet<T> _(UserExpectedSymbolDef<T> s1)
        {
            return new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { s1 });
        }
        protected NtDefAssignSet<T> _(
            UserExpectedSymbolDef<T> s1,
            UserExpectedSymbolDef<T> s2)
        {
            return new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { s1, s2 });
        }
        protected NtDefAssignSet<T> _(
            UserExpectedSymbolDef<T> s1,
            UserExpectedSymbolDef<T> s2,
            UserExpectedSymbolDef<T> s3)
        {
            return new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { s1, s2, s3 });
        }
        protected NtDefAssignSet<T> _(
            UserExpectedSymbolDef<T> s1,
            UserExpectedSymbolDef<T> s2,
            UserExpectedSymbolDef<T> s3,
            UserExpectedSymbolDef<T> s4)
        {
            return new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { s1, s2, s3, s4 });
        }
        protected NtDefAssignSet<T> _(
             UserExpectedSymbolDef<T> s1,
             UserExpectedSymbolDef<T> s2,
             UserExpectedSymbolDef<T> s3,
             UserExpectedSymbolDef<T> s4,
             UserExpectedSymbolDef<T> s5)
        {
            return new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { s1, s2, s3, s4, s5 });
        }
        protected NtDefAssignSet<T> _(
            UserExpectedSymbolDef<T> s1,
            UserExpectedSymbolDef<T> s2,
            UserExpectedSymbolDef<T> s3,
            UserExpectedSymbolDef<T> s4,
            UserExpectedSymbolDef<T> s5,
            UserExpectedSymbolDef<T> s6)
        {
            return new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { s1, s2, s3, s4, s5, s6 });
        }
        protected NtDefAssignSet<T> _(
           UserExpectedSymbolDef<T> s1,
           UserExpectedSymbolDef<T> s2,
           UserExpectedSymbolDef<T> s3,
           UserExpectedSymbolDef<T> s4,
           UserExpectedSymbolDef<T> s5,
           UserExpectedSymbolDef<T> s6,
           UserExpectedSymbolDef<T> s7)
        {
            return new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { s1, s2, s3, s4, s5, s6, s7 });
        }
        protected NtDefAssignSet<T> _(
           UserExpectedSymbolDef<T> s1,
           UserExpectedSymbolDef<T> s2,
           UserExpectedSymbolDef<T> s3,
           UserExpectedSymbolDef<T> s4,
           UserExpectedSymbolDef<T> s5,
           UserExpectedSymbolDef<T> s6,
           UserExpectedSymbolDef<T> s7,
           UserExpectedSymbolDef<T> s8)
        {
            return new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { s1, s2, s3, s4, s5, s6, s7, s8 });
        }
        protected NtDefAssignSet<T> _(
           UserExpectedSymbolDef<T> s1,
           UserExpectedSymbolDef<T> s2,
           UserExpectedSymbolDef<T> s3,
           UserExpectedSymbolDef<T> s4,
           UserExpectedSymbolDef<T> s5,
           UserExpectedSymbolDef<T> s6,
           UserExpectedSymbolDef<T> s7,
           UserExpectedSymbolDef<T> s8,
           UserExpectedSymbolDef<T> s9)
        {
            return new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { s1, s2, s3, s4, s5, s6, s7, s8, s9 });
        }
        protected NtDefAssignSet<T> _(
            UserExpectedSymbolDef<T> s1,
            UserExpectedSymbolDef<T> s2,
            UserExpectedSymbolDef<T> s3,
            UserExpectedSymbolDef<T> s4,
            UserExpectedSymbolDef<T> s5,
            UserExpectedSymbolDef<T> s6,
            UserExpectedSymbolDef<T> s7,
            UserExpectedSymbolDef<T> s8,
            UserExpectedSymbolDef<T> s9,
            UserExpectedSymbolDef<T> s10
            )
        {
            return new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { s1, s2, s3, s4, s5, s6, s7, s8, s9, s10 });
        }
        protected NtDefAssignSet<T> _(
           UserExpectedSymbolDef<T> s1,
           UserExpectedSymbolDef<T> s2,
           UserExpectedSymbolDef<T> s3,
           UserExpectedSymbolDef<T> s4,
           UserExpectedSymbolDef<T> s5,
           UserExpectedSymbolDef<T> s6,
           UserExpectedSymbolDef<T> s7,
           UserExpectedSymbolDef<T> s8,
           UserExpectedSymbolDef<T> s9,
           UserExpectedSymbolDef<T> s10,
           UserExpectedSymbolDef<T> s11
           )
        {
            return new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { s1, s2, s3, s4, s5, s6, s7, s8, s9, s10, s11 });
        }
        protected NtDefAssignSet<T> _(
           UserExpectedSymbolDef<T> s1,
           UserExpectedSymbolDef<T> s2,
           UserExpectedSymbolDef<T> s3,
           UserExpectedSymbolDef<T> s4,
           UserExpectedSymbolDef<T> s5,
           UserExpectedSymbolDef<T> s6,
           UserExpectedSymbolDef<T> s7,
           UserExpectedSymbolDef<T> s8,
           UserExpectedSymbolDef<T> s9,
           UserExpectedSymbolDef<T> s10,
           UserExpectedSymbolDef<T> s11,
           params UserExpectedSymbolDef<T>[] _else
           )
        {
            int j = _else.Length;
            UserExpectedSymbolDef<T>[] total = new UserExpectedSymbolDef<T>[j + 11];
            total[0] = s1; total[1] = s2; total[2] = s3; total[3] = s4; total[4] = s5;
            total[5] = s6; total[6] = s7; total[7] = s8; total[8] = s9; total[9] = s10;
            total[10] = s11;

            for (int i = 0; i < j; ++i)
            {
                total[i + 11] = _else[i];
            }

            return new NtDefAssignSet<T>(getWalkerDel, null, null, total);
        }


    }



    static class UserNTSubParserExtension
    {
        static UserExpectedSymbol CreateUserExpectedSymbol(ReflectionSubParser ownerSubParser, object expSS)
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
                tkdef = ownerSubParser.GetTokenDefintion((string)expSS);
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
                UserNTDefinition newOneOfNt = new UserNTDefinition("oneof_" + (ownerSubParser.GetNewAutoNumber()).ToString());
                object[] symbols = oneOf.symbols;
                int j = symbols.Length;
                for (int i = 0; i < j; ++i)
                {
                    UserExpectedSymbol ues1 = CreateUserExpectedSymbol(ownerSubParser, symbols[i]);
                    var seq1 = new UserSymbolSequence(newOneOfNt);
                    seq1.AppendLast(ues1);
                    newOneOfNt.AddSymbolSequence(seq1);
                }
                newOneOfNt.IsAutoGen = true;
                ownerSubParser.AddLateCreatedUserNt(newOneOfNt);
                return new UserExpectedSymbol(newOneOfNt, isOpt, shiftDel) { ReductionDel = reductionDel };
            }
            //----------------------------------------------------------
            //content is listof ...
            ListSymbol listOf = expSS as ListSymbol;
            if (listOf != null)
            {
                //list of what?
                UserExpectedSymbol ues1 = CreateUserExpectedSymbol(ownerSubParser, listOf.ss); //for seq1
                UserExpectedSymbol ues2 = CreateUserExpectedSymbol(ownerSubParser, listOf.ss); //for seq2 
                UserExpectedSymbol sep_ss = null;
                if (listOf.sep != null)
                {
                    //sep must be token (in form of string or tokendefinition
                    TokenDefinition sepTk = null;
                    if (listOf.sep is string)
                    {
                        sepTk = ownerSubParser.GetTokenDefintion((string)(listOf.sep));
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

                UserNTDefinition newListOfUserNT = new UserNTDefinition("list_of" + (ownerSubParser.GetNewAutoNumber()).ToString());
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
                ownerSubParser.AddLateCreatedUserNt(newListOfUserNT);
                return new UserExpectedSymbol(newListOfUserNT, isOpt, shiftDel) { ReductionDel = reductionDel };
            }
            //----------
            //unknown
            throw new NotSupportedException();
        }

        internal static UserSymbolSequence CreateUserSymbolSeq(UserNTDefinition ntdef, params object[] expectedSymbols)
        {
            UserSymbolSequence ss = new UserSymbolSequence(ntdef);
            ReflectionSubParser ownerSubParser = ntdef.OwnerSubParser as ReflectionSubParser;
            SubParser.SetCurrentUserSymbolSeq(ownerSubParser, ss);
            int j = expectedSymbols.Length;
            //check if symbol or delegate
            for (int i = 0; i < j; ++i)
            {
                ss.AppendLast(CreateUserExpectedSymbol(ownerSubParser, expectedSymbols[i]));
            }
            ntdef.AddSymbolSequence(ss);
            ss.ClearParserReductionNotifyDel();
            return ss;
        }
    }




}