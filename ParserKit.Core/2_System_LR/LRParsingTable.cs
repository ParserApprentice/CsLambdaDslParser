//MIT, 2015-2017, ParserApprentice 

using System;
using System.Text;
using System.Collections.Generic;
namespace Parser.ParserKit.LR
{
    public partial class LRParsingTable
    {


        NTDefinition rootAugmentedNT;
        SymbolResolutionInfo symResolutionInfo;
        //--------------------------------------------

        TokenInfoCollection tokenInfoCollection;
        NTDefinition[] allNts;
        readonly SymbolSequence[] allSymbolSeqs;
        //-------------------------------------------- 
        internal ColumnBasedTable<TokenDefinition, LRItemTodo> tokenTable;
        internal ColumnBasedTable<NTDefinition, LRItemTodo> ntTable;

        //special table for dev mode
        ColumnBasedTable<TokenDefinition, DevLRItemTodoMoreInfo> devTokenMoreInfoTable;
        ColumnBasedTable<NTDefinition, DevLRItemTodoMoreInfo> devNtMoreInfoTable;
        //--------------------------------------------

        int[] ntCompactData;
        int[] tkCompactData;
        int tkColumnCount;
        int ntColumnCount;

#if DEBUG

        static int dbugTotalId;
        public readonly int dbugId = dbugTotalId++;
#endif
        public LRParsingTable(LRStyle lrstyle,
            NTDefinition augmentedNT,
            Dictionary<NTDefinition, NTDefinition> dicGrammars,
            SymbolResolutionInfo symResolutionInfo,
            TokenInfoCollection tokenInfoCollection)
        {

            this.tokenInfoCollection = tokenInfoCollection;

            tokenTable = new ColumnBasedTable<TokenDefinition, LRItemTodo>();
            devTokenMoreInfoTable = new ColumnBasedTable<TokenDefinition, DevLRItemTodoMoreInfo>();
            DefineTokenTableColumns();
            //------------------------------------------
            this.LRStyle = lrstyle;
            this.rootAugmentedNT = augmentedNT;
            this.symResolutionInfo = symResolutionInfo;
            //------------------------------------------

            int n = 0;
            List<SymbolSequence> sseqs = new List<SymbolSequence>();
            allNts = new NTDefinition[dicGrammars.Count];
            foreach (NTDefinition nt in dicGrammars.Values)
            {
                int sqcount = nt.SeqCount;
                nt.NtId = n;
                allNts[n] = nt;
                n++;
                for (int s = 0; s < sqcount; ++s)
                {
                    sseqs.Add(nt.GetSequence(s));
                }
            }
            ntTable = new ColumnBasedTable<NTDefinition, LRItemTodo>();
            devNtMoreInfoTable = new ColumnBasedTable<NTDefinition, DevLRItemTodoMoreInfo>();
            DefineNtTableColumns();
            this.allSymbolSeqs = sseqs.ToArray();
            //------------------------------------------

            OnInit();
        }
        void DefineTokenTableColumns()
        {
            int j = tokenInfoCollection.SnapAllTokenCount;
            for (int i = 0; i < j; ++i)
            {
                TokenDefinition tkdef = tokenInfoCollection.GetTokenInfoByIndex(i);
                tokenTable.AddColumn(tkdef);
                devTokenMoreInfoTable.AddColumn(tkdef);
            }
            tokenTable.FinishColumnsDefinition();
            devTokenMoreInfoTable.FinishColumnsDefinition();
        }
        void DefineNtTableColumns()
        {
            int j = allNts.Length;
            for (int i = 0; i < j; ++i)
            {
                NTDefinition nt = allNts[i];
                ntTable.AddColumn(nt);
                devNtMoreInfoTable.AddColumn(nt);
            }
            ntTable.FinishColumnsDefinition();
            devNtMoreInfoTable.FinishColumnsDefinition();
        }
        internal void ClearLRItemTables()
        {
            this.tokenTable.Clear();
            this.ntTable.Clear();
            this.tokenTable = null;
            this.ntTable = null;

        }

        public SymbolResolutionInfo SymbolResolutionInfo
        {
            get
            {
                return this.symResolutionInfo;
            }
        }

        public LRStyle LRStyle
        {
            get;
            private set;
        }
        public SymbolSequence GetSequence(int index)
        {
            return allSymbolSeqs[index];
        }

        public RRConflictHandler RRConflictHandler { get; set; }
        //----------------------------------------------------------------------------
        public List<TokenDefinition> GetAllExpectedTokens(int rowIndex)
        {
            List<TokenDefinition> tokenDefs = new List<TokenDefinition>();
            int j = this.tokenTable.columns.Count;
            for (int i = this.tokenTable.columns.Count - 1; i >= 0; --i)
            {
                LRItemTodo cell = this.tokenTable.GetCell(rowIndex, i);
                if (!cell.IsEmpty())
                {
                    tokenDefs.Add(tokenTable.columns[i].columnHeader);
                }
            }

            return tokenDefs;
        }
        internal LRItemTodo GetTodo(int rowIndex, TokenDefinition tkdef)
        {
            return this.tokenTable.GetCell(rowIndex, tkdef.TokenInfoNumber);
        }
        internal DevLRItemTodoMoreInfo DevGetTodoMoreInfo(int rowIndex, ISymbolDefinition symbol)
        {
            if (symbol.IsNT)
            {
                return devNtMoreInfoTable.GetCell(rowIndex, ((NTDefinition)symbol).NtId);
            }
            else
            {
                return devTokenMoreInfoTable.GetCell(rowIndex, ((TokenDefinition)symbol).TokenInfoNumber);
            }
        }
        internal LRItemTodo GetTodoFromToken(int rowIndex, int tokenId)
        {
            return this.tokenTable.GetCell(rowIndex, tokenId);
        }
        internal LRItemTodo GetTodo(int rowIndex, NTDefinition nt)
        {
            return this.ntTable.GetCell(rowIndex, nt.NtId);
        }
        //-----------------------------------------------------------------------

        internal void SetTodo(int rowIndex, ISymbolDefinition symbolDef, LRItemTodo todo)
        {
            SetTodo(rowIndex, symbolDef, todo, DevLRItemTodoMoreInfo.Empty);
        }

        internal void SetTodo(int rowIndex, ISymbolDefinition symbolDef, LRItemTodo todo, DevLRItemTodoMoreInfo devMoreInfo)
        {
            if (symbolDef.IsNT)
            {
                SetTodo(rowIndex, (NTDefinition)symbolDef, todo, devMoreInfo);
            }
            else
            {
                SetTodo(rowIndex, (TokenDefinition)symbolDef, todo, devMoreInfo);
            }
        }

        internal void SetTodo(int rowIndex, TokenDefinition tkdef, LRItemTodo todo, DevLRItemTodoMoreInfo devMoreInfo)
        {
            this.tokenTable.SetCell(rowIndex, tkdef.TokenInfoNumber, todo);
            this.devTokenMoreInfoTable.SetCell(rowIndex, tkdef.TokenInfoNumber, devMoreInfo);

        }
        internal void SetTodo(int rowIndex, NTDefinition nt, LRItemTodo todo, DevLRItemTodoMoreInfo devMoreInfo)
        {
            this.ntTable.SetCell(rowIndex, nt.NtId, todo);
            this.devNtMoreInfoTable.SetCell(rowIndex, nt.NtId, devMoreInfo);
        }
        internal void FillTodoForNt(int rowIndex, LRItemTodo todo)
        {
            for (int i = this.ntTable.columns.Count - 1; i >= 0; --i)
            {
                LRItemTodo existingCellData = ntTable.GetCell(rowIndex, i);
                if (!existingCellData.IsEmpty())
                {
                    //conflict!
                    if (todo.ItemKind == LRItemTodoKind.UnresolvedSwitch &&
                        existingCellData.ItemKind != LRItemTodoKind.UnresolvedSwitch)
                    {

                        //in this case, 
                        //resolve with not to replace the existing one
                        //Console.WriteLine("found un_resolved_sw- un_resolved_sw conflict");
                        continue;
                    }

                    throw new NotSupportedException();
                }
                ntTable.SetCell(rowIndex, i, todo);
            }
        }
        internal void FillTodoForToken(int rowIndex, LRItemTodo todo)
        {

            for (int i = this.tokenTable.columns.Count - 1; i >= 0; --i)
            {
                TokenDefinition tk = tokenTable.columns[i].columnHeader;
                LRItemTodo existingCellData = tokenTable.GetCell(rowIndex, i);
                if (!existingCellData.IsEmpty())
                {

                    //conflict!
                    if (todo.ItemKind == LRItemTodoKind.UnresolvedSwitch &&
                        existingCellData.ItemKind != LRItemTodoKind.UnresolvedSwitch)
                    {
                        //in this case, 
                        //resolve with not to replace the existing one
                        continue;
                    }

                    throw new NotSupportedException();
                }
                tokenTable.SetCell(rowIndex, i, todo);

            }
        }
        internal bool Contains(int rowIndex, NTDefinition nt)
        {
            return this.ntTable.GetCell(rowIndex, nt.NtId).IsEmpty();
        }
        internal bool Contains(int rowIndex, TokenDefinition tkdef)
        {
            return this.tokenTable.GetCell(rowIndex, tkdef.TokenInfoNumber).IsEmpty();
        }
        internal void Remove(int rowIndex, ISymbolDefinition symbol)
        {
            if (symbol.IsNT)
            {
                Remove(rowIndex, (NTDefinition)symbol);
            }
            else
            {
                Remove(rowIndex, (TokenDefinition)symbol);
            }

        }
        internal void Remove(int rowIndex, NTDefinition nt)
        {
            this.ntTable.SetCell(rowIndex, nt.NtId, LRItemTodo.Empty);
            devNtMoreInfoTable.SetCell(rowIndex, nt.NtId, DevLRItemTodoMoreInfo.Empty);
        }
        internal void Remove(int rowIndex, TokenDefinition tkdef)
        {
            this.tokenTable.SetCell(rowIndex, tkdef.TokenInfoNumber, LRItemTodo.Empty);
            devTokenMoreInfoTable.SetCell(rowIndex, tkdef.TokenInfoNumber, DevLRItemTodoMoreInfo.Empty);
        }
        //----------------------------------------------------------------------------

        public void MakeParsingTable()
        {
            //from DragonBook 2007 ,page 270
            //create LALR table from input grammar***
            //1. find LR(0) items
            //2. create LALR(1) kernel from LR(0) by finding lookahead (both spontaneous and propagate)
            // and then attach
            //3. when LALR(1) kernel is ready then create LALR(1) parsing table

            //---------------------------------------------------------- 
            LRItemSetCollection lrItemSets = new LRItemSetCollection(this.LRStyle);
            lrItemSets.Clear();
            //----------------------------------------------------------
            //start at root

#if DEBUG
            System.Diagnostics.Stopwatch s01 = new System.Diagnostics.Stopwatch();
            s01.Start();
#endif
            var activeTkInfoCollection = new ActiveTokenInfoCollection(tokenInfoCollection, this.LRStyle);

            LRItemSet items_set0 = LRItemSet.CreateRootItemSet0(activeTkInfoCollection, rootAugmentedNT, this.LRStyle);
            lrItemSets.AddItemSet(items_set0);
            List<LRItemSet> set1 = new List<LRItemSet> { items_set0 };
            List<LRItemSet> set2 = new List<LRItemSet>();
            List<LRItemSet> inputSets = set1;
            List<LRItemSet> newsetForNextRound = set2;
            do
            {
                int j = inputSets.Count;
                for (int i = 0; i < j; ++i)
                {
                    MakeNextLRItemSet(inputSets[i], lrItemSets,
                        newsetForNextRound,
                        activeTkInfoCollection);
                }

                //swap
                List<LRItemSet> tmp = inputSets;
                inputSets = newsetForNextRound;
                tmp.Clear();
                newsetForNextRound = tmp;

            } while (inputSets.Count > 0);
#if DEBUG
            s01.Stop();
            long s01_s1 = s01.ElapsedMilliseconds;
#endif

            //=============================
            //generate real table***
            int itemsetCount = lrItemSets.Count;
#if DEBUG
            s01.Reset();
            s01.Start();
#endif
            symResolutionInfo.ClearPreviousRRConflictResolution();


            //use active row object to append data to table?
            CurrentRowHolder activeRow = new CurrentRowHolder(this);
            for (int i = 0; i < itemsetCount; ++i)
            {
                //start new Row
                this.tokenTable.AppendNewRow(LRItemTodo.Empty);
                this.ntTable.AppendNewRow(LRItemTodo.Empty);

                this.devTokenMoreInfoTable.AppendNewRow(DevLRItemTodoMoreInfo.Empty);
                this.devNtMoreInfoTable.AppendNewRow(DevLRItemTodoMoreInfo.Empty);

                activeRow.MoveNextRow();

                MakeActionSet(activeRow,
                     lrItemSets.GetItemSet(i),
                     symResolutionInfo,
                     activeTkInfoCollection);


                //-----------------------------------
            }

            if (symResolutionInfo.NeedToDoRRConflictResolution)
            {

            }
#if DEBUG
            s01.Stop();
            long s01_s2 = s01.ElapsedMilliseconds;
#endif
        }
        //=============================

        class CurrentRowHolder
        {
            LRParsingTable ownerTable;
            public CurrentRowHolder(LRParsingTable ownerTable)
            {
                this.RowNumber = -1;
                this.ownerTable = ownerTable;
            }
            public int RowNumber { get; private set; }
            public void MoveNextRow()
            {
                RowNumber++;
            }
            public override string ToString()
            {
                //for debug
                StringBuilder stbuilder = new StringBuilder();
                int j = this.ownerTable.tokenTable.columns.Count;
                for (int i = 0; i < j; ++i)
                {
                    TokenDefinition sym = (TokenDefinition)this.ownerTable.tokenTable.columns[i].columnHeader;
                    stbuilder.Append(sym);
                    stbuilder.Append(':');
                    var itemtodo = ownerTable.tokenTable.GetCell(this.RowNumber, i);
                    if (!itemtodo.IsEmpty())
                    {
                        stbuilder.Append(itemtodo);
                    }
                    else
                    {
                        stbuilder.Append(' ');
                    }
                }

                return stbuilder.ToString();

            }
            //--------------------------------------
            public LRItemTodo GetTodo(ISymbolDefinition expectedSymbol)
            {
                if (expectedSymbol.IsNT)
                {
                    return this.ownerTable.GetTodo(this.RowNumber, (NTDefinition)expectedSymbol);
                }
                else
                {
                    return this.ownerTable.GetTodo(this.RowNumber, (TokenDefinition)expectedSymbol);
                }
            }
            public void Remove(ISymbolDefinition symbol)
            {
                if (symbol.IsNT)
                {
                    this.ownerTable.Remove(this.RowNumber, (NTDefinition)symbol);
                }
                else
                {
                    this.ownerTable.Remove(this.RowNumber, (TokenDefinition)symbol);
                }
            }

            internal DevLRItemTodoMoreInfo DevGetTodoMoreInfo(ISymbolDefinition expectedSymbol)
            {
                return ownerTable.DevGetTodoMoreInfo(this.RowNumber, expectedSymbol);
            }

            public bool Contains(ISymbolDefinition symbol)
            {
                if (symbol.IsNT)
                {
                    return this.ownerTable.Contains(this.RowNumber, (NTDefinition)symbol);
                }
                else
                {
                    return this.ownerTable.Contains(this.RowNumber, (TokenDefinition)symbol);
                }

            }

            internal LRItemTodo AddShiftTask(ISymbolDefinition symbol, LRItemSet nextItemSet, int sampleExpectedSymbolPos)
            {


                //get only 1 sample
                LRItemTodo todo = LRItemTodo.CreateShiftTask(
                    nextItemSet.ItemSetNumber,
                    LRItemSet.GetFirstKernelItemForSample(nextItemSet).OriginalSeq.TotalSeqNumber,  //-extension
                    sampleExpectedSymbolPos);  //-extension
                //-----------------------------
                this.ownerTable.SetTodo(this.RowNumber, symbol, todo, new DevLRItemTodoMoreInfo(null, nextItemSet));

                return todo;
            }

            internal void AddReductionTask(ISymbolDefinition tkinfo, LRItem prevLRItem)
            {

                this.ownerTable.SetTodo(this.RowNumber, tkinfo,
                    LRItemTodo.CreateReduceTask(prevLRItem.OriginalSeq.TotalSeqNumber),
                    new DevLRItemTodoMoreInfo(prevLRItem, null));
            }

            public void AddGotoTask(ISymbolDefinition symbol, int stateNumber)
            {
                this.ownerTable.SetTodo(this.RowNumber, symbol, LRItemTodo.CreateGotoTask(stateNumber));
            }
            public void AddAcceptTask(ISymbolDefinition symbol)
            {
                this.ownerTable.SetTodo(this.RowNumber, symbol, LRItemTodo.CreateAcceptTask());
            }

            /// <summary>
            /// add reduce -reduce (RR) 
            /// </summary>
            /// <param name="tkinfo"></param>
            /// <param name="stateNumber"></param>
            public void AddRRTask(ISymbolDefinition symbol, int stateNumber)
            {
                this.ownerTable.SetTodo(this.RowNumber, symbol, LRItemTodo.CreateRRConflictHandlerTask(stateNumber));
            }
        }

        //======================================================
        //LRParsing Table Cache features 



        internal void CompactTable(bool alsoClearOriginalLRTables)
        {
            //------------------------------
            //nt compact data
            {
                int rowCount = ntTable.RowCount;
                int colCount = ntTable.columns.Count;
                ntColumnCount = colCount;
                ntCompactData = new int[rowCount * colCount];
                int running = 0;
                for (int r = 0; r < rowCount; ++r)
                {
                    for (int c = 0; c < colCount; ++c)
                    {
                        LRItemTodo todo = ntTable.GetCell(r, c);
                        if (todo.IsEmpty())
                        {
                            ntCompactData[running] = 0;
                        }
                        else
                        {
                            if ((todo.StateNumber << 8) < (1 << 24))
                            {

                                ntCompactData[running] = (todo.StateNumber << 8) | (int)todo.ItemKind;

#if DEBUG
                                int data = (todo.StateNumber << 8) | (int)todo.ItemKind;
                                if (data < 0)
                                {
                                }
                                int testback = ntCompactData[running] >> 8;
                                int todoKind = (ntCompactData[running] & 0xFF);
                                if (testback != todo.StateNumber && todoKind != (int)todo.ItemKind)
                                {
                                    throw new NotSupportedException();
                                }
#endif
                            }
                            else
                            {
                                throw new NotSupportedException();
                            }
                        }
                        running++;
                    }
                }
            }
            //------------------------------
            //tk compact data
            {
                int rowCount = tokenTable.RowCount;
                int colCount = tokenTable.columns.Count;
                tkColumnCount = colCount;
                tkCompactData = new int[rowCount * colCount];
                int running = 0;
                for (int r = 0; r < rowCount; ++r)
                {
                    for (int c = 0; c < colCount; ++c)
                    {
                        LRItemTodo todo = tokenTable.GetCell(r, c);
                        if (todo.IsEmpty())
                        {
                            tkCompactData[running] = 0;
                        }
                        else
                        {
                            if ((todo.StateNumber << 8) < (1 << 24))
                            {

                                tkCompactData[running] = (todo.StateNumber << 8) | (int)todo.ItemKind;

#if DEBUG
                                int data = (todo.StateNumber << 8) | (int)todo.ItemKind;
                                if (data < 0)
                                {
                                }
                                int testback = data >> 8;
                                int todoKind = data & 0xFF;
                                if (testback != todo.StateNumber && todoKind != (int)todo.ItemKind)
                                {
                                    throw new NotSupportedException();
                                }
#endif
                            }
                            else
                            {
                                throw new NotSupportedException();
                            }
                        }
                        running++;
                    }
                }
            }
            //------------------------------

            if (alsoClearOriginalLRTables)
            {
                this.ntTable.Clear();
                this.tokenTable.Clear();
                this.ntTable = null;
                this.tokenTable = null;
            }
        }
        internal int FastGetTodo(int rowIndex, TokenDefinition tkdef)
        {
            return tkCompactData[(rowIndex * tkColumnCount) + tkdef.TokenInfoNumber];
        }
        internal int FastGetTodoFromToken(int rowIndex, int tokenId)
        {
            return tkCompactData[(rowIndex * tkColumnCount) + tokenId];
        }
        internal int FastGetTodo(int rowIndex, NTDefinition nt)
        {
            return ntCompactData[(rowIndex * ntColumnCount) + nt.NtId];
        }
        internal int FastGetTodoFromNt(int rowIndex, int ntId)
        {
            return ntCompactData[(rowIndex * ntColumnCount) + ntId];
        }
#if DEBUG
        public string dbugName
        {
            get;
            set;
        }
        static List<LRItemSet> dbugGetViewer(LRItemSetCollection collection, int index, int backstep, int nextstep)
        {
            int startFrom = index - backstep;
            if (startFrom < 0)
            {
                startFrom = 0;
            }
            int endAt = index + nextstep;
            if (endAt > collection.Count - 1)
            {
                endAt = collection.Count;
            }
            var results = new List<LRItemSet>();
            for (int i = startFrom; i < endAt; i++)
            {
                results.Add(collection.GetItemSet(i));
            }

            return results;

        }
#endif



        void MakeActionSet(
            CurrentRowHolder row,
            LRItemSet itemset,
            SymbolResolutionInfo symResolutionInfo,
            ActiveTokenInfoCollection activeTkInfoCollection)
        {

            LRStyle lrstyle = itemset.LRStyle;
            //-----------------
            //shift and goto 
            //my extension------------------------
            int shiftCount = 0;
            TokenDefinition singleShiftTk = null;
            LRItemSet singleNextStateItem = null;
            ISymbolDefinition singleShiftNextTk = null;
            int reducCount = 0;
            TokenDefinition singleReducTk = null;
            //------------------------------------

            foreach (LRItemSet nextItemSet in itemset.GetNextItemSetIter())
            {
                ISymbolDefinition jumpOverSymbol = nextItemSet.JumpOverFromSymbol;
                if (jumpOverSymbol.IsNT)
                {
                    //original
                    //AddLRItemGotoTask(row, jumpOverSymbol, nextItemSet, symResolutionInfo);
                    //---------------
                    //extension
                    NTDefinition ntDef = (NTDefinition)jumpOverSymbol;
                    if (ntDef.NTKind == NTDefintionKind.UnknownNT)
                    {
                        AddLRItemSwitchToTask(row, jumpOverSymbol, nextItemSet, symResolutionInfo);

                    }
                    else
                    {
                        AddLRItemGotoTask(row, jumpOverSymbol, nextItemSet, symResolutionInfo);
                    }
                }
                else
                {

                    TokenDefinition tkinfo = (TokenDefinition)jumpOverSymbol;
                    if (!tkinfo.IsEOF)
                    {
                        //all shift is create here 
                        //--------------------------------------------------------------------- 
                        //my extension ***
                        LRItem sampleItem = LRItemSet.GetFirstKernelItemForSample(nextItemSet);
                        UserExpectedSymbol sampleUserExpectedSymbol = sampleItem.OriginalSeq.GetOriginalUserExpectedSymbol(sampleItem.DotPos - 1);
                        AddLRItemShiftTask(row, jumpOverSymbol, nextItemSet, symResolutionInfo, (short)(sampleItem.DotPos - 1));

                        //---------------------------------------------------------------------
                        singleShiftNextTk = nextItemSet.JumpOverFromSymbol;
                        singleShiftTk = tkinfo;
                        singleNextStateItem = nextItemSet;
                        shiftCount++;
                    }
                }
            }
            //-----------------
            //reduce 
            //non kernel is item that has 'dot' at left most 
            //-----------------

            foreach (LRItem item in itemset.GetOnlyKernelItemIterForward())
            {
                if (!item.IsEnd)
                {
                    //skip
                    continue;
                }

                SymbolSequence ss = item.OriginalSeq;
                int symcount = ss.RightSideCount;
                ISymbolDefinition lastSymbol = ss[symcount - 1];
                ISymbolDefinition beforelast = null;
                if (symcount > 1)
                {
                    beforelast = ss[symcount - 2];
                }
                if (ss.LeftSideNT != null)
                {

                    LRItem prevItem = item.PrevItem;

                    //LR0 and LR1 have only 1  prevItem  
                    //LALR may has prev item more than 1 (because of merging process)

                    foreach (TokenDefinition lookAhead in item.GetReductionHintIter(activeTkInfoCollection, lrstyle))
                    {
                        //1. lookahead is  token
                        if (lookAhead.IsEOF)
                        {
                            if (prevItem == null)
                            {
                                if (prevItem == null)
                                {
                                    prevItem = item;
                                }
                                AddLRItemReduceTask(row, beforelast, lookAhead, prevItem, symResolutionInfo);
                                reducCount++;
                                singleReducTk = lookAhead;
                            }
                            else
                            {
                                if (prevItem.OriginalSeq.LeftSideNT.NTKind == NTDefintionKind.RootStartSymbol)
                                {
                                    AddLRItemAcceptTask(row, beforelast, lookAhead, prevItem, symResolutionInfo);
                                }
                                else
                                {
                                    AddLRItemReduceTask(row, beforelast, lookAhead, prevItem, symResolutionInfo);
                                    reducCount++;
                                    singleReducTk = lookAhead;
                                }
                            }
                        }
                        else
                        {
                            if (prevItem == null)
                            {
                                prevItem = item;
                            }

                            AddLRItemReduceTask(row, beforelast, lookAhead, prevItem, symResolutionInfo);
                            reducCount++;
                            singleReducTk = lookAhead;
                        }
                    }
                }
            }


            if (shiftCount == 1 && reducCount == 0)
            {


            }
            else if (shiftCount == 0 && reducCount == 1)
            {

            }

        }
        static LRItemTodo AddLRItemShiftTask(CurrentRowHolder row,
            ISymbolDefinition jumpOverSymbol,
            LRItemSet nextItemSet,
            SymbolResolutionInfo symResolutionInfo,
            int sampleExpectedSymbolPos /* my extension*/)
        {

            LRItemTodo existingTask = row.GetTodo(jumpOverSymbol);

            if (existingTask.IsEmpty())
            {
                //no exisiting item
                return row.AddShiftTask(jumpOverSymbol, nextItemSet, sampleExpectedSymbolPos);
            }
            else
            {
                //conflict
                //already exist=> conflict found,
                //report to user to find conflict resolving methods 
                switch (existingTask.ItemKind)
                {
                    case LRItemTodoKind.UnresolvedSwitch:
                        //replace existing switch
                        return row.AddShiftTask(jumpOverSymbol, nextItemSet, sampleExpectedSymbolPos);

                    default:
                        symResolutionInfo.AddResolveMessage("cc_conflict_found: shift " + row.ToString());
                        return LRItemTodo.Empty;
                }
            }
        }

#if DEBUG
        struct dbugTestPair
        {
            public readonly NTDefinition a;
            public readonly NTDefinition b;
            public dbugTestPair(NTDefinition a, NTDefinition b)
            {
                this.a = a;
                this.b = b;
            }
        }

        Dictionary<dbugTestPair, int> dbugDicTestPairs = new Dictionary<dbugTestPair, int>();

#endif

        void AddLRItemReduceTask(CurrentRowHolder row,
          ISymbolDefinition beforeLast,
          TokenDefinition jumpOverSymbol,
          LRItem prevItem,
          SymbolResolutionInfo symResolutionInfo)
        {

            LRItemTodo existingTask = row.GetTodo(jumpOverSymbol);
            if (existingTask.IsEmpty())
            {

                row.AddReductionTask(jumpOverSymbol, prevItem);
            }
            else
            {

                switch (existingTask.ItemKind)
                {
                    default:
                        {
                            throw new NotSupportedException();
                        }
                    case LRItemTodoKind.UnresolvedSwitch:
                        {
                            //  throw new NotSupportedException();
                            //throw new NotSupportedException();
                            row.AddReductionTask(jumpOverSymbol, prevItem);
                        }
                        break;
                    case LRItemTodoKind.ConflictRR:
                        {
                            //?

                        }
                        break;
                    case LRItemTodoKind.Shift:
                        {
                            //SR- conflivt
                            //check conflict and try resolve it

                            if (beforeLast != null)
                            {

                                if (beforeLast == jumpOverSymbol)
                                {
                                    //shift-reduce conflict

                                    //check associative rules...
                                    //for left associative, choose reduction first -> change task to reduction

                                    if (!symResolutionInfo.IsRightAssoc(jumpOverSymbol))
                                    {
                                        //remove the old one 
                                        row.Remove(jumpOverSymbol);
                                        //replace with the new one
                                        row.AddReductionTask(jumpOverSymbol, prevItem);
                                    }
                                }
                                else
                                {
                                    int beforeLast_precedence = symResolutionInfo.GetSymbolPrecedence(beforeLast);
                                    int jumpOverSymbol_precedence = symResolutionInfo.GetSymbolPrecedence(jumpOverSymbol);
                                    if (jumpOverSymbol_precedence < beforeLast_precedence)
                                    {

                                        //if beforeLast.SymbolStringValue has more precendence
                                        //then jump over -> then change to reduction

                                        //remove the old one 
                                        row.Remove(jumpOverSymbol);
                                        //replace with the new one
                                        row.AddReductionTask(jumpOverSymbol, prevItem);
                                    }
                                    else if (jumpOverSymbol_precedence == beforeLast_precedence)
                                    {
                                        //check if left or right association
                                        //if already shift -> read next 

                                        //if symbol is left assoc -> choose reduction
                                        //if right assoc -> read next

                                        if (!symResolutionInfo.IsRightAssoc(jumpOverSymbol))
                                        {
                                            row.Remove(jumpOverSymbol);
                                            row.AddReductionTask(jumpOverSymbol, prevItem);
                                        }

                                    }
                                    else if (jumpOverSymbol_precedence > beforeLast_precedence)
                                    {
                                        //do nothing
                                    }
                                    else
                                    {
                                        throw new NotSupportedException();
                                    }
                                }
                            }
                            else
                            {

                                //shift reduce
                                //if existing task is shift task

                                //dbugLRItemTodoMoreInfo moreInfo = row.dbugGetTodo(jumpOverSymbol);
                                //LRItemSet existingShiftToItemSet = moreInfo.NextItemSet;

                                //row.Remove(jumpOverSymbol);

                                //row.AddReductionTask(jumpOverSymbol, prevItem);

                                //symResolutionInfo.AddResolveMessage(
                                // "conflict_S-R" + prevItem.OriginalSeq.SeqNumber + " " + existingTask.StateNumber);
                            }
                        }
                        break;
                    case LRItemTodoKind.Reduce:
                        {
                            //reduce-reduce 

                            DevLRItemTodoMoreInfo moreInfo = row.DevGetTodoMoreInfo(jumpOverSymbol);

                            //check if the two have the same destination point

                            SymbolSequence existingSeq = moreInfo.ReductionToSeq;
                            SymbolSequence newSeq = prevItem.OriginalSeq;
                            NTDefinition existing_nt = existingSeq.LeftSideNT;
                            NTDefinition new_nt = newSeq.LeftSideNT;

                            //if not the same nt, check if they are on the same 'chain'

                            if (new_nt != existing_nt)
                            {
                                //rr conflict 
                                if (new_nt.ReductionChain != existing_nt.ReductionChain)
                                {
                                    if (this.RRConflictHandler != null)
                                    {
                                        var selectedNt = this.RRConflictHandler(
                                            jumpOverSymbol,
                                            existing_nt,
                                            new_nt,
                                            symResolutionInfo);

                                        if (selectedNt != null &&
                                            selectedNt != existing_nt)
                                        {

                                            //-----------------------------
                                            StringBuilder stbuilder = new StringBuilder();
                                            stbuilder.Append("c-R-R: remove ");
                                            stbuilder.Append(jumpOverSymbol.ToString());
                                            stbuilder.Append(" from ");
                                            stbuilder.Append(existing_nt.ToString());
                                            stbuilder.Append(" then add ");
                                            stbuilder.Append(jumpOverSymbol.ToString());
                                            stbuilder.Append(" from ");
                                            stbuilder.Append(new_nt.ToString());
                                            symResolutionInfo.AddResolveMessage(stbuilder.ToString());
                                            //-----------------------------
                                            row.Remove(jumpOverSymbol);
                                            row.AddReductionTask(jumpOverSymbol, prevItem);
                                        }
                                        else
                                        {

                                        }
                                    }
                                    else
                                    {
                                    }
                                }
                                else
                                {
                                    //they are on the same chain
                                    if ((new_nt.NTDepthLevel > 0) && new_nt.NTDepthLevel < existing_nt.NTDepthLevel)
                                    {
                                        //reduction chain  

                                        if (this.RRConflictHandler != null)
                                        {
                                            var selectedNt = this.RRConflictHandler(jumpOverSymbol, existing_nt, new_nt, symResolutionInfo);
                                            if (selectedNt != null &&
                                                selectedNt != existing_nt)
                                            {
                                                //replace...
                                                row.Remove(jumpOverSymbol);
                                                row.AddReductionTask(jumpOverSymbol, prevItem);

                                                StringBuilder stbuilder = new StringBuilder();
                                                stbuilder.Append("c-R-R: remove ");
                                                stbuilder.Append(jumpOverSymbol.ToString());
                                                stbuilder.Append(" from ");
                                                stbuilder.Append(existing_nt.ToString());
                                                stbuilder.Append(" then add ");
                                                stbuilder.Append(jumpOverSymbol.ToString());
                                                stbuilder.Append(" from ");
                                                stbuilder.Append(new_nt.ToString());

                                                symResolutionInfo.AddResolveMessage(stbuilder.ToString());

                                            }
                                            else
                                            {

                                            }
                                        }
                                        else
                                        {
                                            row.Remove(jumpOverSymbol);
                                            row.AddReductionTask(jumpOverSymbol, prevItem);

                                            StringBuilder stbuilder = new StringBuilder();
                                            stbuilder.Append("c-R-R: remove ");
                                            stbuilder.Append(jumpOverSymbol.ToString());
                                            stbuilder.Append(" from ");
                                            stbuilder.Append(existing_nt.ToString());
                                            stbuilder.Append(" then add ");
                                            stbuilder.Append(jumpOverSymbol.ToString());
                                            stbuilder.Append(" from ");
                                            stbuilder.Append(new_nt.ToString());

                                            symResolutionInfo.AddResolveMessage(stbuilder.ToString());
                                        }

                                    }
                                    else
                                    {

#if DEBUG
                                        dbugTestPair testPair = new dbugTestPair(new_nt, existing_nt);
                                        if (!this.dbugDicTestPairs.ContainsKey(testPair))
                                        {
                                            this.dbugDicTestPairs.Add(testPair, 0);
                                        }
#endif

                                    }
                                }

                            }
                            else
                            {

                            }
                        }
                        break;
                }

            }
        }
        static void AddLRItemAcceptTask(CurrentRowHolder row, ISymbolDefinition beforeLast, TokenDefinition jumpOverSymbol, LRItem prevItem, SymbolResolutionInfo symResolutionInfo)
        {

            LRItemTodo existingTask = row.GetTodo(jumpOverSymbol);
            if (existingTask.IsEmpty())//existingTask.IsEmpty)
            {
                row.AddAcceptTask(jumpOverSymbol);
            }
            else
            {
                switch (existingTask.ItemKind)
                {
                    case LRItemTodoKind.UnresolvedSwitch:
                        //replace existing switch
                        row.AddAcceptTask(jumpOverSymbol);
                        break;
                    default:
                        symResolutionInfo.AddResolveMessage("conflict_ACCEPT " + row.ToString());
                        break;
                }
            }
        }

        static void AddLRItemGotoTask(
            CurrentRowHolder row,
            ISymbolDefinition jumpOverSymbol,
            LRItemSet nextItemSet,
            SymbolResolutionInfo symResolutionInfo)
        {

            LRItemTodo existingTask = row.GetTodo(jumpOverSymbol);
            if (existingTask.IsEmpty())//existingTask.IsEmpty)
            {
                row.AddGotoTask(jumpOverSymbol, nextItemSet.ItemSetNumber);
            }
            else
            {
                switch (existingTask.ItemKind)
                {
                    case LRItemTodoKind.UnresolvedSwitch:
                        //conflict
                        row.AddGotoTask(jumpOverSymbol, nextItemSet.ItemSetNumber);
                        break;
                    default:
                        symResolutionInfo.AddResolveMessage("conflict_GOTO " + row.ToString());
                        break;
                }
            }

        }



        //#if DEBUG
        //        static int dbug_MakeNextLR_step = 0;
        //        static int dbug_MakeNextLR_step2 = 0;
        //#endif

        static Stack<Dictionary<ISymbolDefinition, ProtoLRItemSet>> freeProtoLRItemPools = new Stack<Dictionary<ISymbolDefinition, ProtoLRItemSet>>();
        static Dictionary<ISymbolDefinition, ProtoLRItemSet> GetFreeProtoLRItems()
        {
            if (freeProtoLRItemPools.Count == 0)
            {
                return new Dictionary<ISymbolDefinition, ProtoLRItemSet>();
            }
            else
            {
                return freeProtoLRItemPools.Pop();
            }
        }
        static void ReleaseProtoLRItems(Dictionary<ISymbolDefinition, ProtoLRItemSet> protoLRItems)
        {
            protoLRItems.Clear();
            freeProtoLRItemPools.Push(protoLRItems);
        }
        static void MakeNextLRItemSet(
            LRItemSet startItemSet,
            LRItemSetCollection existingLRItemSets,
            List<LRItemSet> newSetsForNextRound,
            ActiveTokenInfoCollection tks)
        {


            Dictionary<ISymbolDefinition, ProtoLRItemSet> protoLRItems = GetFreeProtoLRItems(); //  new Dictionary<ISymbolDefinition, ProtoLRItemSet>();
            foreach (LRItem item in startItemSet.GetAllItemIterForward())
            {
                //dragon book page 261 (GOTO)
                ISymbolDefinition jumpOverSymbolX; // symbol X in page 261 Goto(I,X)
                LRItem nextStepLRItem = item.CreateNextStepLRItem(startItemSet, out jumpOverSymbolX);

                //------------------
                //this is an extension ****
                //if (jumpOverSymbolX != null && !jumpOverSymbolX.IsNT)
                //{
                //    if (((TokenDefinition)jumpOverSymbolX).GrammarSymbolString == "{")
                //    {
                //        startItemSet.hasSyncToken = true;

                //    }
                //} 
                //*************************
                // SymbolSequence originalSq = item.OriginalSeq;
                //------------------
                if (nextStepLRItem == null)
                {
                    //finish that LRItem, go next item
                    continue;
                }

                ProtoLRItemSet existingProtoLRItemSet;
                if (!protoLRItems.TryGetValue(jumpOverSymbolX, out existingProtoLRItemSet))
                {
                    existingProtoLRItemSet = new ProtoLRItemSet(startItemSet, jumpOverSymbolX);
                    protoLRItems.Add(jumpOverSymbolX, existingProtoLRItemSet);
                }

                existingProtoLRItemSet.AddKernelItem(nextStepLRItem);
            }

            //create LRSet in the same level first 
            //when finish all in one level
            //then go deeper
            foreach (ProtoLRItemSet protoLRItemSet in protoLRItems.Values)
            {

                //check if we have create the group before (dragon book page 261)       
                LRItemSet exitingSet = existingLRItemSets.FindCompatibleItemSet(protoLRItemSet);

                if (exitingSet == null)
                {
                    //if no match
                    //convert protoLRItem set to LRItemSet
                    LRItemSet newItemset = protoLRItemSet.CreateRealLRItemSet(existingLRItemSets.Count);
                    existingLRItemSets.AddItemSet(newItemset);
                    newSetsForNextRound.Add(newItemset);
                }
                else
                {
                    //check if it is resued
                    //if yes ->  find destination of LR  
                    if (exitingSet.LRStyle == LRStyle.LALR)
                    {
                        //merge with the existing one
                        protoLRItemSet.StartFromItemSet.AddPossibleNextItemSet(exitingSet);
                        exitingSet.AddLALR1LookAheads(protoLRItemSet, tks);
                    }
                    else
                    {
                        protoLRItemSet.StartFromItemSet.AddPossibleNextItemSet(exitingSet);
                    }
                }
            }
            //---------------------------------------------
            startItemSet.ClearNonKernelCollection();
            ReleaseProtoLRItems(protoLRItems);
        }



        //---------------------------------------------------------------

        internal TokenInfoCollection DevGetTokenInfoCollection()
        {
            return this.tokenInfoCollection;
        }
        internal NTDefinition[] DevGetAllNts()
        {
            return this.allNts;
        }

#if DEBUG
        public override string ToString()
        {
            StringBuilder stbuilder = new StringBuilder();
            return stbuilder.ToString();
        }
#endif
    }

}