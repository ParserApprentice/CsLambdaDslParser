//MIT 2015-2017, ParserApprentice  

using System;
using System.Text;
using System.Collections.Generic;
namespace Parser.ParserKit.LR
{

    enum LRParserEventKind
    {
        Unknown,
        //------------
        LRReduction,
        LRShift,
        LRGoto,
        LRError,
        LRAccept,
        //------------
        LLGoto,
        LLNext
    }

    /// <summary>
    /// create parsing table / or parser
    /// </summary>
    public static class LRParsing
    {
        public static LRParser CreateRunner(LRParsingTable table)
        {
            return new LRParser(table);
        }
    }
    class LRParsingContext
    {
        //reusable LRParsingContext 
        internal readonly LR.StateStackDev stateStackDev = new LR.StateStackDev();
        internal readonly LR.StateStack states2 = new StateStack();
        internal readonly LR.ParseNodeStack symbolParseNodes = new ParseNodeStack();

#if DEBUG
        public readonly int dbugId = dbugTotalId++;
        static int dbugTotalId;
#endif
        public LRParsingContext()
        {
            // Console.WriteLine("ctx" + dbugId);

        }
        public void Clear()
        {
            states2.Clear();
            stateStackDev.Clear();
            symbolParseNodes.Clear();
        }
    }



    /// <summary>
    /// info for debugging
    /// </summary>
    struct AlphaPart
    {
        //this part is in front of dot        
        readonly LRItem ownerLRItem;
        public AlphaPart(LRItem ownerLRItem)
        {
            this.ownerLRItem = ownerLRItem;

        }
        public bool IsEmpty
        {
            get
            {
                return ownerLRItem == null;
            }
        }
        public override string ToString()
        {
            return ownerLRItem.ToString();
        }
    }
#if DEBUG
    struct dbugBetaRemainingPart
    {

        //beta part is after B part
        readonly LRItem ownerLRItem;
        public dbugBetaRemainingPart(LRItem ownerLRItem)
        {
            this.ownerLRItem = ownerLRItem;
        }
        public bool IsEmpty
        {
            get
            {
                return ownerLRItem == null;
            }
        }

        public override string ToString()
        {
            return ownerLRItem.ToString() + " : first_symbol_of_beta:" + this.GetFirstSymbolOfBeta();
        }
        public ISymbolDefinition GetFirstSymbolOfBeta()
        {
            int dotpos = ownerLRItem.DotPos;
            ISymbolDefinition current_s = ownerLRItem.OriginalSeq[dotpos];

            if (dotpos < ownerLRItem.OriginalSeq.RightSideCount - 1)
            {

                //find follower for current_s
                return ownerLRItem.OriginalSeq[dotpos + 1];
                //check if nextS first tokens has empty epsilon in the component or not                 

            }
            else
            {
                return null;
            }
        }
    }
#endif

    class SymbolHash
    {
        const int INT32_SIZE = 4;
        byte[] int32x1 = new byte[INT32_SIZE];
        byte[] int32x2 = new byte[INT32_SIZE * 2];
        byte[] int32x3 = new byte[INT32_SIZE * 3];
        byte[] int32x4 = new byte[INT32_SIZE * 4];
        byte[] int32x5 = new byte[INT32_SIZE * 5];
        System.IO.MemoryStream ms;
        System.IO.BinaryWriter writer;
        public SymbolHash()
        {
            ms = new System.IO.MemoryStream();
            writer = new System.IO.BinaryWriter(ms);
        }
        public int GetHash(int data1)
        {
            ms.Position = 0;
            writer.Write(data1);
            writer.Flush();

            ms.Position = 0;
            ms.Read(int32x1, 0, INT32_SIZE * 1);

            return CRC32Calculator.CalculateCrc32(int32x1);
        }
        public int GetHash(int data1, int data2)
        {

            ms.Position = 0;
            writer.Write(data1);
            writer.Write(data2);
            writer.Flush();
            ms.Position = 0;
            ms.Read(int32x2, 0, INT32_SIZE * 2);

            return CRC32Calculator.CalculateCrc32(int32x2);
        }
        public int GetHash(int data1, int data2, int data3)
        {

            ms.Position = 0;
            writer.Write(data1);
            writer.Write(data2);
            writer.Write(data3);
            writer.Flush();


            ms.Position = 0;
            ms.Read(int32x3, 0, INT32_SIZE * 3);

            return CRC32Calculator.CalculateCrc32(int32x3);
        }
        public int GetHash(int data1, int data2, int data3, int data4)
        {

            ms.Position = 0;
            writer.Write(data1);
            writer.Write(data2);
            writer.Write(data3);
            writer.Write(data4);
            writer.Flush();

            ms.Position = 0;
            ms.Read(int32x4, 0, INT32_SIZE * 4);

            return CRC32Calculator.CalculateCrc32(int32x4);
        }
        public int GetHash(int data1, int data2, int data3, int data4, int data5)
        {

            ms.Position = 0;
            writer.Write(data1);
            writer.Write(data2);
            writer.Write(data3);
            writer.Write(data4);
            writer.Write(data5);

            writer.Flush();
            ms.Position = 0;
            ms.Read(int32x5, 0, INT32_SIZE * 5);

            return CRC32Calculator.CalculateCrc32(int32x5);
        }
        public int GetHash(int[] data1)
        {
            ms.Position = 0;
            int j = data1.Length;
            for (int i = 0; i < j; ++i)
            {
                writer.Write(data1[i]);
            }
            writer.Flush();
            ms.Position = 0;
            int bufferSize = INT32_SIZE * j;
            byte[] databuffer = new byte[bufferSize];
            ms.Read(databuffer, 0, bufferSize);

            return CRC32Calculator.CalculateCrc32(databuffer);
        }

        ~SymbolHash()
        {

            if (writer != null)
            {
                writer.Close();
                writer = null;
            }

            if (ms != null)
            {
                ms.Close();
                ms.Dispose();
                ms = null;
            }
        }


    }

    class ActiveTokenInfoCollection
    {
        List<int> sharedLookaheads;
        TokenInfoCollection tokenInfoCollection;
        int snapAllTokenCount = 0;
        int calculateToken32BitsByteCount = 0;
        int newLookaheadId = 0;
        bool reuseMode;
        SymbolHash symbolHash = new SymbolHash();

        public ActiveTokenInfoCollection(TokenInfoCollection tokenInfoCollection, LRStyle lrStyle)
        {

            this.tokenInfoCollection = tokenInfoCollection;
            if (tokenInfoCollection.CollectionState == TokenInfoCollectionState.Open)
            {
                throw new NotSupportedException();
            }
            //---------------------------
            //prepare first
            this.sharedLookaheads = new List<int>(1000 * 1000);
            //---------------------------            
            this.snapAllTokenCount = tokenInfoCollection.SnapAllTokenCount;
            int slot32Count = snapAllTokenCount / 32;
            if ((snapAllTokenCount % 32) > 0)
            {
                slot32Count++;
            }
            this.calculateToken32BitsByteCount = slot32Count;

            for (int i = 0; i < slot32Count; ++i)
            {
                this.sharedLookaheads.Add(0);

            }
            this.newLookaheadId = this.sharedLookaheads.Count;
        }
        public int Calculated32BitCount
        {
            get
            {
                return this.calculateToken32BitsByteCount;
            }
        }
        public void ReuseLastestTicket(int lastestId)
        {

            if (lastestId + calculateToken32BitsByteCount == this.newLookaheadId)
            {
                reuseMode = true;
            }

        }

        /// <summary>
        /// create new ticket         
        /// </summary>
        /// <returns></returns>
        public int GetNewLookaheadId()
        {
            //get new lookahead id
            List<int> tmplist = this.sharedLookaheads;
            if (this.reuseMode)
            {
                reuseMode = false;
                int nn = this.newLookaheadId - this.calculateToken32BitsByteCount;
                switch (this.calculateToken32BitsByteCount)
                {
                    case 1:
                        {
                            tmplist[nn] = 0;
                        }
                        break;
                    case 2:
                        {
                            tmplist[nn] = 0;
                            tmplist[nn + 1] = 0;
                        }
                        break;
                    case 3:
                        {
                            tmplist[nn] = 0;
                            tmplist[nn + 1] = 0;
                            tmplist[nn + 2] = 0;
                        }
                        break;
                    case 4:
                        {
                            tmplist[nn] = 0;
                            tmplist[nn + 1] = 0;
                            tmplist[nn + 2] = 0;
                            tmplist[nn + 3] = 0;

                        }
                        break;
                    case 5:
                        {
                            ////return nn;
                            tmplist[nn + 0] = 0;
                            tmplist[nn + 1] = 0;
                            tmplist[nn + 2] = 0;
                            tmplist[nn + 3] = 0;
                            tmplist[nn + 4] = 0;
                        }
                        break;
                    default:
                        {
                            for (int i = 0; i < nn; ++i)
                            {
                                tmplist[nn + i] = 0;
                            }
                        }
                        break;
                }
                return nn;
            }

            int new_lookahead_id = this.newLookaheadId;
            int j = this.calculateToken32BitsByteCount;


            switch (j)
            {
                case 1:
                    {
                        tmplist.Add(0);

                    }
                    break;
                case 2:
                    {

                        tmplist.Add(0);
                        tmplist.Add(0);
                    }
                    break;
                case 3:
                    {

                        tmplist.Add(0);
                        tmplist.Add(0);
                        tmplist.Add(0);
                    }
                    break;
                case 4:
                    {

                        tmplist.Add(0);
                        tmplist.Add(0);
                        tmplist.Add(0);
                        tmplist.Add(0);
                    }
                    break;
                case 5:
                    {
                        tmplist.Add(0);
                        tmplist.Add(0);
                        tmplist.Add(0);
                        tmplist.Add(0);
                        tmplist.Add(0);
                    }
                    break;
                default:
                    {
                        for (int i = this.calculateToken32BitsByteCount - 1; i > -1; --i)
                        {

                            tmplist.Add(0);
                        }
                    }
                    break;
            }


            this.newLookaheadId += j;
            return new_lookahead_id;
        }
        public void AddTokenInfo(int ticketId, TokenDefinition tkinfo)
        {

            int tokenNumber = tkinfo.TokenInfoNumber;
            //1 slot => 32 bits  
            int tokeninfoNumber = tkinfo.TokenInfoNumber;
            int slotNum = tokeninfoNumber >> 5;//tokenInfo.TokenInfoNumber/32;
            int remainerForSingleSlot = tokeninfoNumber - (slotNum << 5); //*32

            int prevValue = this.sharedLookaheads[ticketId + slotNum];
            this.sharedLookaheads[ticketId + slotNum] = (prevValue | (int)(1 << remainerForSingleSlot));
            //store into that slot             
        }
        public void AddCahceBitTokenInfo(int ticketId, int[] cacheBits)
        {
            switch (cacheBits.Length)
            {
                case 0:
                    break;
                case 1:
                    sharedLookaheads[ticketId + 0] |= cacheBits[0];
                    break;
                case 2:
                    sharedLookaheads[ticketId + 0] |= cacheBits[0];
                    sharedLookaheads[ticketId + 1] |= cacheBits[1];
                    break;
                case 3:
                    sharedLookaheads[ticketId + 0] |= cacheBits[0];
                    sharedLookaheads[ticketId + 1] |= cacheBits[1];
                    sharedLookaheads[ticketId + 2] |= cacheBits[2];
                    break;
                case 4:
                    sharedLookaheads[ticketId + 0] |= cacheBits[0];
                    sharedLookaheads[ticketId + 1] |= cacheBits[1];
                    sharedLookaheads[ticketId + 2] |= cacheBits[2];
                    sharedLookaheads[ticketId + 3] |= cacheBits[3];
                    break;
                case 5:
                    sharedLookaheads[ticketId + 0] |= cacheBits[0];
                    sharedLookaheads[ticketId + 1] |= cacheBits[1];
                    sharedLookaheads[ticketId + 2] |= cacheBits[2];
                    sharedLookaheads[ticketId + 3] |= cacheBits[3];
                    sharedLookaheads[ticketId + 4] |= cacheBits[4];
                    break;
                default:
                    {
                        //loop
                        for (int i = cacheBits.Length - 1; i >= 0; --i)
                        {
                            sharedLookaheads[ticketId + i] |= cacheBits[i];
                        }
                    }
                    break;
            }

        }

        public void MergeTickets(int ticketA, int ticketB)
        {
            //bitwise OR combine ticketB(source) to ticketA (target)
            List<int> temp = this.sharedLookaheads;
            for (int i = this.calculateToken32BitsByteCount - 1; i > -1; --i)
            {
                temp[i + ticketA] |= temp[i + ticketB];
            }
        }
        public bool IsDiff(int ticketA, int ticketB)
        {

            List<int> temp = this.sharedLookaheads;
            for (int i = this.calculateToken32BitsByteCount - 1; i > -1; --i)
            {
                if (temp[i + ticketA] != temp[i + ticketB])
                {
                    return true;
                }
            }
            return false;
        }

        struct TicketCheckPair
        {
            public readonly int ticket1;
            public readonly int ticket2;
            public TicketCheckPair(int t1, int t2)
            {
                ticket1 = t1;
                ticket2 = t2;
            }
#if DEBUG
            public override string ToString()
            {
                return (ticket1 / 5) + ":" + (ticket2 / 5);
            }
#endif
        }

        Dictionary<TicketCheckPair, bool> prevCheckPairs = new Dictionary<TicketCheckPair, bool>();
        public bool IsCompatibleLookAhead(int ticket1, int ticket2)
        {

            if (ticket1 == ticket2)
            {
                return true;
            }

            //Console.WriteLine(ticket1 + ":" + ticket2);

            TicketCheckPair checkPair = new TicketCheckPair(ticket1, ticket2);
            bool checkResult;
            if (!prevCheckPairs.TryGetValue(checkPair, out checkResult))
            {
                List<int> temp = this.sharedLookaheads;
                for (int i = this.calculateToken32BitsByteCount - 1; i > -1; --i)
                {
                    if (temp[i + ticket1] != temp[i + ticket2])
                    {
                        //cache result
                        prevCheckPairs.Add(checkPair, false);
                        return false;
                    }
                }
                //cache result
                prevCheckPairs.Add(checkPair, true);
                return true;
            }
            //----------------
            //confirm?
            {
                return checkResult;
                //// return checkResult; 
                //List<int> temp = this.sharedLookaheads;
                //for (int i = this.calculateToken32BitsByteCount - 1; i > -1; --i)
                //{
                //    if (temp[i + ticket1] != temp[i + ticket2])
                //    {
                //        //cache result
                //        // prevCheckPairs.Add(checkPair, false);
                //        if (checkResult != false)
                //        {
                //            throw new NotSupportedException();
                //        }
                //        return false;
                //    }
                //}
                ////cache result
                //// prevCheckPairs.Add(checkPair, true);
                //if (checkResult != true)
                //{
                //    throw new NotSupportedException();
                //}
                //return true;
            }
            //----------------
            return checkResult;
        }
        public void PrintLookaheadTokens(int ticket, StringBuilder stbuilder)
        {

            foreach (TokenDefinition tkinfo in this.GetTokenInfoIter(ticket))
            {
                stbuilder.Append(tkinfo.ToString());
                stbuilder.Append('/');
            }

        }
        public IEnumerable<TokenDefinition> GetTokenInfoIter(int ticket)
        {

            int startpos = ticket;
            int maxtoken = this.snapAllTokenCount;

            int nindex = 0;
            for (int i = 0; i < calculateToken32BitsByteCount; i++)
            {

                int bits = this.sharedLookaheads[startpos];
                //32 bits
                if (nindex + 32 < maxtoken)
                {
                    for (int n = 0; n < 32; ++n)
                    {
                        if ((bits & (1 << n)) != 0)
                        {
                            yield return tokenInfoCollection.GetTokenInfoByIndex(nindex);
                        }
                        nindex++;
                    }
                }
                else
                {
                    for (int n = 0; n < 32; ++n)
                    {
                        if (((bits & (1 << n)) != 0) && nindex < maxtoken)
                        {
                            yield return tokenInfoCollection.GetTokenInfoByIndex(nindex);
                        }
                        nindex++;
                    }
                }
                startpos++;
            }
        }

        public int CalculateContentHash(int ticketId)
        {

            List<int> tmp = this.sharedLookaheads;
            switch (this.calculateToken32BitsByteCount)
            {
                case 1:
                    {
                        return symbolHash.GetHash(tmp[ticketId]);
                    }
                case 2:
                    {
                        return symbolHash.GetHash(tmp[ticketId], tmp[ticketId + 1]);
                    }
                case 3:
                    {
                        return symbolHash.GetHash(tmp[ticketId], tmp[ticketId + 1], tmp[ticketId + 2]);
                    }
                case 4:
                    {
                        return symbolHash.GetHash(tmp[ticketId], tmp[ticketId + 1], tmp[ticketId + 2], tmp[ticketId + 3]);
                    }
                case 5:
                    {
                        return symbolHash.GetHash(tmp[ticketId], tmp[ticketId + 1], tmp[ticketId + 2],
                            tmp[ticketId + 3], tmp[ticketId + 4]);
                    }
                default:
                    {

                        int[] inputBuffer = new int[this.calculateToken32BitsByteCount];
                        this.sharedLookaheads.CopyTo(ticketId, inputBuffer, 0, this.calculateToken32BitsByteCount);
                        return symbolHash.GetHash(inputBuffer);
                    }
            }
        }



    }



    class LRItem
    {

        /// <summary>
        /// store prev item, in case of prev step
        /// </summary>
        LRItem prevStepItem;
        /// <summary>
        /// in case of expand from LRItem 
        /// </summary>
        LRItem expandFromLRItem;


        bool hasSubLALRItems;
        bool isLALRLookaheadChanged;
        readonly LRItemSet owner;
        readonly int myLookAheadTicketId = 0;
        readonly int myLookaheadTicketHashCode;


#if DEBUG
        public int dbugId;
        static int dbugTotalId = 0;
#endif
        public LRItem(
            LRItemSet owner,
            LRItem expandFromLRItem,
            SymbolSequence originalSq,
            int importLookaheadTicket,
            int lkTicketHashCode)
        {
            //in expansion case
            dbugSetupId();
            this.owner = owner;
            //1. 
            this.OriginalSeq = originalSq;
            //2.
            this.myLookAheadTicketId = importLookaheadTicket;
            //3. 
            this.myLookaheadTicketHashCode = lkTicketHashCode;
            //4.
            //this.myLRItmHash = ((originalSq.SeqNumber << 16) + lkTicketHashCode);
            //5.  
            this.expandFromLRItem = expandFromLRItem;
            this.IsEnd = false;// this.OriginalSeq.Count == 0;

        }
        private LRItem(
            LRItemSet owner,
            LRItem prevStepItem,
            SymbolSequence originalSq,
            int dotpos,
            int importLookaheadTicket,
            int lkTicketHashCode)
        {


            //create from LRItem from existing lookahead 
            dbugSetupId();
            this.owner = owner;
            //1.
            this.OriginalSeq = originalSq;
            //2.
            this.myLookAheadTicketId = importLookaheadTicket;
            //3.
            this.myLookaheadTicketHashCode = lkTicketHashCode;
            //4.

            //5.
            this.prevStepItem = prevStepItem;
            this.DotPos = dotpos;
            this.IsKernelItem = dotpos != 0;
            this.IsEnd = (this.OriginalSeq.RightSideCount == dotpos);



        }

        public LRItem ExpandFromItem
        {
            get
            {
                return this.expandFromLRItem;
            }
        }

        public int DotPos
        {
            get;
            private set;
        }
        public SymbolSequence OriginalSeq
        {
            get;
            private set;
        }
        public bool IsEnd
        {
            get;
            private set;
        }
        public bool IsKernelItem
        {
            get;
            private set;
        }
        public LRItem PrevItem
        {
            get
            {
                return this.prevStepItem;
            }
        }
        public bool HasSubLALRItems
        {
            get
            {
                return this.hasSubLALRItems;
            }
        }
        public void MergeSubLALRItem(int anotherLookaheadTicket, ActiveTokenInfoCollection tks)
        {
            this.hasSubLALRItems = true;
            if (anotherLookaheadTicket != this.myLookAheadTicketId)
            {
                if (tks.IsDiff(this.myLookAheadTicketId, anotherLookaheadTicket))
                {
                    this.isLALRLookaheadChanged = true;
                    tks.MergeTickets(this.myLookAheadTicketId, anotherLookaheadTicket);
                }
            }
        }
        public bool IsLALRLookaheadChanged
        {
            get
            {
                return this.isLALRLookaheadChanged;
            }
            set
            {
                this.isLALRLookaheadChanged = value;
            }
        }
        [System.Diagnostics.Conditional("DEBUG")]
        void dbugSetupId()
        {
#if DEBUG
            this.dbugId = dbugTotalId++;
            //if (this.dbugId == 769)
            //{
            //}
            //if (this.dbugId == 365651)
            //{

            //}
#endif
        }


        public IEnumerable<TokenDefinition> GetReductionHintIter(ActiveTokenInfoCollection tks, LRStyle lrstyle)
        {
            if (lrstyle == LRStyle.LR0)
            {
                //reduction hint of LR0 -> all possible case                 
                foreach (TokenDefinition tkinfo in this.OriginalSeq.LeftSideNT.GetAllPossibleFollowerTokens())
                {
                    yield return tkinfo;
                }
            }
            else
            {
                //reduction hint of LR1 and LALR -> lookahead symbol of item
                foreach (TokenDefinition tkinfo in tks.GetTokenInfoIter(this.myLookAheadTicketId))
                {
                    yield return tkinfo;
                }
            }
        }


        public static LRItem CreateRootItem(
           ActiveTokenInfoCollection tks,
            SymbolSequence originalSq,
            int importLookaheadTicket, int lookaheadContentHash)
        {
            LRItem rootItem = new LRItem(null, null,
                originalSq, 0, importLookaheadTicket,
                lookaheadContentHash);

            rootItem.IsKernelItem = true;
            return rootItem;
        }

#if DEBUG
        public AlphaPart dbugAlphaPart
        {
            get
            {
                return new AlphaPart(this);
            }
        }
        public ISymbolDefinition dbugB_Part
        {
            //[A ---> alpha-part dot B-part beta-remaining-part ,lookahead]
            get
            {
                return this.dbugGetEntering_B_Symbol();
            }
        }
        public dbugBetaRemainingPart dbugBetaPart
        {
            get
            {
                return new dbugBetaRemainingPart(this);
            }
        }
        public ISymbolDefinition dbugGetEntering_B_Symbol()
        {
            //entering B symbol
            //: just immediate follower symbol of dot
            //dragon book page 245
            if (this.IsEnd)
            {
                return null;
            }
            //==================
            return this.OriginalSeq[this.DotPos];
        }
#endif

        internal bool GetEntering_B_NT_Symbol(out NTDefinition onlyNT)
        {
            //entering B symbol
            //: just immediate follower symbol of dot
            //dragon book page 245
            if (!this.IsEnd)
            {
                ISymbolDefinition s = this.OriginalSeq[this.DotPos];
                if (s.IsNT)
                {
                    onlyNT = (NTDefinition)s;
                    return true;
                }
            }
            onlyNT = null;
            return false;
        }
        /// <summary>
        /// create next step LR item from current item 
        /// </summary>
        /// <param name="jumpOverSymbol"></param>
        /// <returns></returns>
        public LRItem CreateNextStepLRItem(LRItemSet owner, out ISymbolDefinition jumpOverSymbol)
        {
            //---------------------
            if (this.IsEnd)
            {
                jumpOverSymbol = null;
                return null;
            }
            //---------------------
            jumpOverSymbol = this.OriginalSeq.GetExpectedSymbol(this.DotPos);

            return new LRItem(owner, this, this.OriginalSeq,
                this.DotPos + 1,
                this.myLookAheadTicketId,
                this.myLookaheadTicketHashCode);
        }
        public int LookaheadTicketId
        {
            get
            {
                return this.myLookAheadTicketId;
            }
        }
        public override string ToString()
        {

            //find 'dot' position
            string[] o_splits = this.OriginalSeq.ToString().Split(' ');
            StringBuilder stbuilder = new StringBuilder();
            int j = o_splits.Length;
            stbuilder.Append(this.OriginalSeq.LeftSideNT.Name);
            stbuilder.Append("---> ");

            int dotpos = this.DotPos;
            for (int i = 0; i < j; ++i)
            {
                if (i == dotpos)
                {
                    //add 'dot'
                    stbuilder.Append(" @ ");
                }
                stbuilder.Append(o_splits[i]);
                if (i < j)
                {
                    stbuilder.Append(' ');
                }
            }
            if (dotpos == j)
            {
                stbuilder.Append(" @ ");
            }

            stbuilder.Append(',');


            //check lookahead
            this.owner.PrintLookaheadTicket(this.myLookAheadTicketId, stbuilder);
            return stbuilder.ToString();
        }

        static int[] PrepareTokenInfoCache(TokenDefinition[] tkinfos, int slotCount)
        {
            int j = tkinfos.Length;
            int[] tkbitCache = new int[slotCount];

            for (int i = tkinfos.Length - 1; i >= 0; --i)
            {
                TokenDefinition tkinfo = tkinfos[i];
                int tokenNumber = tkinfo.TokenInfoNumber;
                //1 slot => 32 bits  
                int tokeninfoNumber = tkinfo.TokenInfoNumber;
                int slotNum = tokeninfoNumber >> 5;//tokenInfo.TokenInfoNumber/32;
                int remainerForSingleSlot = tokeninfoNumber - (slotNum << 5); //*32
                //int prevValue = tkbitCache[slotNum];
                tkbitCache[slotNum] |= (int)(1 << remainerForSingleSlot);
            }

            return tkbitCache;
        }
        public int GetLookaheadsTicketForLR1(ActiveTokenInfoCollection tks, out bool isSharedTicketId)
        {

            if (this.DotPos < this.OriginalSeq.RightSideCount - 1)
            {

                //prepare space for new data set ***
                int newTicket = tks.GetNewLookaheadId();

                //not finished,
                //find followers for current_s
                //the followers are first items for nextS
                ISymbolDefinition nextS = this.OriginalSeq[this.DotPos + 1];
                if (nextS.IsNT)
                {
                    NTDefinition nextSNt = (NTDefinition)nextS;
                    int[] cacheBitFirstTks = nextSNt.CacheBitFirstTokens;
                    if (cacheBitFirstTks == null)
                    {
                        nextSNt.CacheBitFirstTokens = cacheBitFirstTks = PrepareTokenInfoCache(nextSNt.GetAllFirstTokens(), tks.Calculated32BitCount);

                    }
                    tks.AddCahceBitTokenInfo(newTicket, cacheBitFirstTks);
                    //for (int i = tkinfos.Length - 1; i > -1; --i)
                    //{
                    //    tks.AddTokenInfo(newTicket, tkinfos[i]);
                    //}
                }
                else
                {
                    tks.AddTokenInfo(newTicket, (TokenDefinition)nextS);
                }
                isSharedTicketId = false;

                return newTicket;
            }
            else
            {
                //use existing ticket
                isSharedTicketId = true;
                return this.myLookAheadTicketId;
            }
        }

        /// <summary>         
        /// check if another item is compat with this item
        /// </summary>
        /// <param name="anotherLRItem"></param>
        /// <param name="alsoCheckLookahead"></param>
        /// <param name="tks"></param>
        /// <returns></returns>
        public bool IsEquivalentSequenceAndLookahead(LRItem anotherLRItem)
        {
            return
                this.myLookaheadTicketHashCode == anotherLRItem.myLookaheadTicketHashCode
                && this.OriginalSeq == anotherLRItem.OriginalSeq
                && this.DotPos == anotherLRItem.DotPos
                && this.owner.IsCompatibleLookahead(this.myLookAheadTicketId, anotherLRItem.myLookAheadTicketId);
        }

        public int LookaheadContentHashCode
        {
            get { return this.myLookaheadTicketHashCode; }
        }
        public LRItemSet Owner
        {
            get { return this.owner; }
        }
        /// <summary>
        /// compare only sq and dotpos, not compare lookahead 
        /// </summary>
        /// <param name="anotherLRItem"></param>
        /// <returns></returns>
        public bool IsEquivalentSequence(LRItem anotherLRItem)
        {
            return this.OriginalSeq == anotherLRItem.OriginalSeq
               && this.DotPos == anotherLRItem.DotPos;
        }

    }


    class ProtoLRItemSet
    {

        readonly List<LRItem> onlyKernelItems;
        readonly ISymbolDefinition jumpOverSymbol;
        readonly LRItemSet startFromItemSet;
        int lrItemSetHashCode;
        bool isArranged;
#if DEBUG
        static int dbugTotalId;
        public int dbugId;
#endif
        public ProtoLRItemSet(LRItemSet startFromItemSet, ISymbolDefinition jumpOverSymbol)
        {

#if DEBUG
            this.dbugId = dbugTotalId++;
            //if (this.dbugId == 31426)
            //{
            //}
            //Console.WriteLine(this.dbugId);

#endif
            this.onlyKernelItems = new List<LRItem>();
            this.startFromItemSet = startFromItemSet;
            this.jumpOverSymbol = jumpOverSymbol;
            this.lrItemSetHashCode = 0;
        }
        public ISymbolDefinition JumpOverSymbol
        {
            get
            {
                return this.jumpOverSymbol;
            }
        }
        public void AddKernelItem(LRItem item)
        {
#if DEBUG
            //if (this.dbugId == 31426)
            //{
            //}
#endif
            isArranged = false;
            this.onlyKernelItems.Add(item);
        }
        public LRItemSet StartFromItemSet
        {
            get
            {
                return this.startFromItemSet;
            }
        }
        public List<LRItem> GetKernelItems()
        {
            return this.onlyKernelItems;
        }
        public List<LRItem> GetArrangedKernelItems()
        {
            if (!isArranged)
            {
                this.Md5Hash = LRItemSetSortAndHash.SortAndHash(this.onlyKernelItems);
                isArranged = true;
            }
            return onlyKernelItems;
        }
        internal byte[] Md5Hash
        {
            get;
            private set;
        }
        public override string ToString()
        {
            return "from " + this.startFromItemSet.ToString() + " ,jump:" + jumpOverSymbol.ToString();
        }
        /// <summary>
        /// make proto tobe real LR itemset
        /// </summary>
        /// <param name="itemSetNumber"></param>
        /// <returns></returns>
        public LRItemSet CreateRealLRItemSet(int itemSetNumber)
        {

            return new LRItemSet(
                this.startFromItemSet,
                itemSetNumber,
                this.jumpOverSymbol,
                this.lrItemSetHashCode,
                onlyKernelItems);
        }
    }

    public enum LRItemTodoKind : byte
    {
        Empty,
        Unknown,
        Err,
        Shift,
        Reduce,
        Goto,
        Accept,
        UnresolvedSwitch,
        ResolveSwitch,
        //---------------------------------------
        /// <summary>
        /// shift - reduce conflict need custom resolve at runtime ?
        /// </summary>
        ConflictSR,
        /// <summary>
        /// reduce -reduce conflict need custom resolve at runtime ?
        /// </summary>
        ConflictRR
    }



    public struct LRItemTodo
    {
        //------------------------------       
        readonly int stateNumber;//4 
        readonly LRItemTodoKind itemKind;//1

        readonly ushort shiftSeqNumber; //2
        readonly short sampleUserExpectedSymbolPos;//2

        public static readonly LRItemTodo Empty = new LRItemTodo();
#if DEBUG
        static int dbugTotalId;
        public readonly int dbugId; //4
        //int dbugid2;
        //static int dbug_shift_id_total;
#endif

        private LRItemTodo(int nextItemNumberOrReduceToSqNumber, LRItemTodoKind itemKind)
        {
            //may be shift or goto


            this.stateNumber = nextItemNumberOrReduceToSqNumber;
            this.itemKind = itemKind;
            this.shiftSeqNumber = 0;
            this.sampleUserExpectedSymbolPos = 0;

#if DEBUG
            this.dbugId = dbugTotalId++;

            //dbugid2 = 0;
            //if (itemKind == LRItemTodoKind.Shift)
            //{
            //    dbugid2 = dbug_shift_id_total++;
            //}
            //else if (itemKind == LRItemTodoKind.Reduce)
            //{
            //    dbugid2 = dbug_shift_id_total++;
            //}

            dbugCheckId();
#endif
        }

        private LRItemTodo(int nextItemNumberOrReduceToSqNumber, LRItemTodoKind itemKind, int shiftSeqNumber, int expectedSymbolPos)
        {
#if DEBUG
            this.dbugId = dbugTotalId++;

            if (shiftSeqNumber >= ushort.MaxValue)
            {
                throw new NotSupportedException();
            }


#endif
            if (expectedSymbolPos >= short.MaxValue)
            {
                throw new NotSupportedException();
            }

            this.sampleUserExpectedSymbolPos = (short)expectedSymbolPos;
            this.stateNumber = nextItemNumberOrReduceToSqNumber;
            this.itemKind = itemKind;
            this.shiftSeqNumber = (ushort)shiftSeqNumber;

#if DEBUG

            //dbugid2 = 0;
            //if (itemKind == LRItemTodoKind.Shift)
            //{
            //    dbugid2 = dbug_shift_id_total++;
            //}
            //else if (itemKind == LRItemTodoKind.Reduce)
            //{
            //    dbugid2 = dbug_shift_id_total++;
            //} 
            dbugCheckId();
#endif
        }
#if DEBUG
        public void dbugCheckId()
        {

            //if (this.dbugId == 44301)
            //{

            //}

        }
#endif

        public bool IsEmpty()
        {
            return itemKind == LRItemTodoKind.Empty;
        }
        //===============================================
        public static LRItemTodo CreateAcceptTask()
        {
            return new LRItemTodo(0, LRItemTodoKind.Accept);
        }
        public static LRItemTodo CreateReduceTask(int stateNumber)
        {
            return new LRItemTodo(stateNumber, LRItemTodoKind.Reduce);
        }
        public static LRItemTodo CreateShiftTask(int stateNumber, int fromOrgSeqNumber, int sampleDotPos)
        {
            return new LRItemTodo(stateNumber, LRItemTodoKind.Shift, fromOrgSeqNumber, sampleDotPos);
        }
        public static LRItemTodo CreateGotoTask(int stateNumber)
        {
            return new LRItemTodo(stateNumber, LRItemTodoKind.Goto);
        }

        public static LRItemTodo CreateErrorTask(int stateNumber)
        {
            return new LRItemTodo(stateNumber, LRItemTodoKind.Err);
        }
        //---------
        //my extension
        public static LRItemTodo CreateUnresolvedSwitch(int switchRecordNumber)
        {
            //create unknown switch
            return new LRItemTodo(switchRecordNumber, LRItemTodoKind.UnresolvedSwitch);
        }
        public static LRItemTodo CreateResolvedSwitch(int switchRecordNumber)
        {
            return new LRItemTodo(switchRecordNumber, LRItemTodoKind.ResolveSwitch);
        }
        //---------

        //for internal use, 
        internal static LRItemTodo InternalCreateLRItem(int stateNumber, LRItemTodoKind kind, int fromOrgSeqNumber, int samplDotPos)
        {
            return new LRItemTodo(stateNumber, kind, fromOrgSeqNumber, samplDotPos);
        }
        /// <summary>
        /// crate reduce-reduce conflict handler
        /// </summary>
        /// <param name="stateNumber"></param>
        /// <returns></returns>
        public static LRItemTodo CreateRRConflictHandlerTask(int stateNumber)
        {
            return new LRItemTodo(stateNumber, LRItemTodoKind.ConflictRR);
        }
        /// <summary>
        /// create shift-reduce conflict handler
        /// </summary>
        /// <param name="stateNumber"></param>
        /// <returns></returns>
        public static LRItemTodo CreateSRConflictHandlerTask(int stateNumber)
        {
            return new LRItemTodo(stateNumber, LRItemTodoKind.ConflictSR);
        }
        //===============================================

        public LRItemTodoKind ItemKind
        {
            get
            {
                return this.itemKind;
            }
        }
        public int NextItemSetNumber
        {
            get
            {
                return this.stateNumber;
            }

        }
        public int ReduceToSequenceNumber
        {
            get
            {
                return stateNumber;
            }
        }
        public int StateNumber
        {
            get
            {
                return this.stateNumber;
            }
        }
        public int SwitchRecordNumber
        {
            get { return this.stateNumber; }
        }
        public override string ToString()
        {
            StringBuilder stBuilder = new StringBuilder();
            switch (this.itemKind)
            {
                default:
                    {
                        throw new NotSupportedException();
                    }
                case LRItemTodoKind.Empty:
                    return "";
                case LRItemTodoKind.ConflictRR:
                    {
                        stBuilder.Append("RR");
                        stBuilder.Append(this.stateNumber);
                    }
                    break;
                case LRItemTodoKind.Shift:
                    {
                        stBuilder.Append("s");
                        stBuilder.Append(this.stateNumber);
                    }
                    break;
                case LRItemTodoKind.Reduce:
                    {
                        stBuilder.Append("r");
                        stBuilder.Append(this.stateNumber);
                    }
                    break;
                case LRItemTodoKind.Goto:
                    {
                        stBuilder.Append("g");
                        stBuilder.Append(this.stateNumber);
                    }
                    break;
                case LRItemTodoKind.Err:
                    {
                        stBuilder.Append("e");
                    }
                    break;
                case LRItemTodoKind.Accept:
                    {
                        stBuilder.Append("acc");
                    }
                    break;
                case LRItemTodoKind.UnresolvedSwitch:
                    {
                        stBuilder.Append("sw_u");
                        stBuilder.Append(this.stateNumber);
                    }
                    break;
                case LRItemTodoKind.ResolveSwitch:
                    {
                        stBuilder.Append("sw");
                        stBuilder.Append(this.stateNumber);
                    }
                    break;
            }
            return stBuilder.ToString();
        }

        internal int SampleUserExpectedSymbolPos
        {
            get { return this.sampleUserExpectedSymbolPos; }
        }

        internal int OriginalSeqNumberForShift
        {
            //for shift command 
            get { return shiftSeqNumber; }
        }
    }

    struct DevLRItemTodoMoreInfo
    {
        public readonly bool isNotEmpty;
        public readonly LRItem prevItem;
        public readonly LRItemSet nextItemSet;

        public readonly static DevLRItemTodoMoreInfo Empty = new DevLRItemTodoMoreInfo();

        public DevLRItemTodoMoreInfo(LRItem prevItem, LRItemSet nextItemSet)
        {
            this.prevItem = prevItem;
            this.nextItemSet = nextItemSet;
            this.isNotEmpty = true;
        }
        public bool IsEmpty() { return !this.isNotEmpty; }

        public LRItemSet NextItemSet
        {
            get
            {
                return this.nextItemSet;

            }
        }
        public int NextItemSetNumber
        {
            get
            {
                return this.nextItemSet.ItemSetNumber;
            }
        }
        public SymbolSequence ReductionToSeq
        {
            get
            {
                return this.prevItem.OriginalSeq;
            }
        }
        public int ReductionStateNumber
        {
            get
            {
                return this.prevItem.OriginalSeq.TotalSeqNumber;
            }
        }
        public override string ToString()
        {
            return this.prevItem.ToString();
        }
    }








    static class LRItemSetSortAndHash
    {
        static System.Security.Cryptography.MD5 md5Hash = System.Security.Cryptography.MD5.Create();
        static System.IO.MemoryStream ms = new System.IO.MemoryStream();
        static System.IO.BinaryWriter binWriter = new System.IO.BinaryWriter(ms);
        static byte[] tempBuffer = new byte[1024 * 100];

        static System.IO.BinaryWriter GetTempBinWriter()
        {
            return binWriter;
        }
        static byte[] GetMd5Hash(byte[] input, int count)
        {

            // Convert the input string to a byte array and compute the hash.
            return md5Hash.ComputeHash(input, 0, count);

        }
        public static bool CompareMd5Hash(byte[] buffer1, byte[] buffer2)
        {
            for (int i = buffer1.Length - 1; i >= 0; --i)
            {
                if (buffer1[i] != buffer2[i])
                {
                    return false;
                }
            }
            return true;
        }
        public static byte[] SortAndHash(List<LRItem> onlyKernelItems)
        {

            int kItemCount = onlyKernelItems.Count;
            if (kItemCount > 0)
            {
                onlyKernelItems.Sort(
                             (i1, i2) =>
                             {
                                 int diff = i1.OriginalSeq.TotalSeqNumber - i2.OriginalSeq.TotalSeqNumber;
                                 if (diff == 0)
                                 {
                                     //same then check lookeahead content hash
                                     return i1.LookaheadContentHashCode.CompareTo(i2.LookaheadContentHashCode);
                                 }
                                 else
                                 {
                                     if (diff < 0)
                                     {
                                         return -1;
                                     }
                                     else
                                     {
                                         return 1;
                                     }
                                 }
                             }
                            );
            }
            //-----------
            //calculate MD5 
            //create md5 input value
            var binWriter = GetTempBinWriter();
            System.IO.MemoryStream ms = (System.IO.MemoryStream)binWriter.BaseStream;
            ms.Position = 0;
            //-----------
            int pos = 0;
            for (int i = onlyKernelItems.Count - 1; i >= 0; --i)
            {
                LRItem item = onlyKernelItems[i];
                binWriter.Write((short)item.OriginalSeq.TotalSeqNumber); //2
                binWriter.Write((short)item.DotPos);//2
                binWriter.Write(item.LookaheadContentHashCode);//4
                pos += (2 + 2 + 4);
            }
            ms.Position = 0;
            ms.Read(tempBuffer, 0, pos);
            //-----------
            binWriter.Flush();
            return GetMd5Hash(tempBuffer, pos);
            //-----------
        }
    }



    //--------------
    /// <summary>
    /// set of LRItem
    /// </summary>
    class LRItemSet
    {
        LRItemSet startFromItemSet;
        /// <summary>         
        /// for easy-to-use, store kernel item here
        /// </summary>
        List<LRItem> onlyKernelItems;
        NonKernelCollection nonKernelItems;
        /// <summary>
        /// all next posible states         
        /// </summary>         
        Dictionary<LRItemSet, int> nextItemSets = new Dictionary<LRItemSet, int>();

        ActiveTokenInfoCollection tks;
        int onlyKernelCount;
        bool isKernelArranged;
        static Stack<NonKernelCollection> nonKernelPool = new Stack<NonKernelCollection>();



#if DEBUG
        public int dbugId = dbugTotalId++;
        static int dbugTotalId;
#endif
        private LRItemSet(ActiveTokenInfoCollection tks,
            ISymbolDefinition jumpOverFromSymbol,
            LRStyle lrStyle,
            int itemsetNumber,
            List<LRItem> onlyKernelItems)
        {

            //create from root only
            this.tks = tks;
            this.ItemSetNumber = itemsetNumber;
            this.onlyKernelItems = onlyKernelItems;
            this.onlyKernelCount = onlyKernelItems.Count;
            this.LRStyle = lrStyle;
            this.JumpOverFromSymbol = jumpOverFromSymbol;

            this.nonKernelItems = GetNonKernel();
            ExpandLRItemSet(this);
        }
        public LRItemSet(LRItemSet startFromItemSet, int setnumber,
            ISymbolDefinition jumpOverFromSymbol, int kernelItemsHash,
            List<LRItem> onlyKernelItems)
        {
            //create from proto LR
            this.tks = startFromItemSet.tks;
            this.startFromItemSet = startFromItemSet;
            this.KernelItemsHash = kernelItemsHash;

            this.LRStyle = startFromItemSet.LRStyle;
            this.ItemSetNumber = setnumber;

            this.JumpOverFromSymbol = jumpOverFromSymbol;
            this.onlyKernelItems = onlyKernelItems;
            this.onlyKernelCount = onlyKernelItems.Count;


            startFromItemSet.AddPossibleNextItemSet(this);

            this.nonKernelItems = GetNonKernel();
            ExpandLRItemSet(this);
        }
        static NonKernelCollection GetNonKernel()
        {
            if (nonKernelPool.Count > 0)
            {
                return nonKernelPool.Pop();
            }
            else
            {
                return new NonKernelCollection();
            }
        }
        static void ReleaseNonkernel(NonKernelCollection nonkernel)
        {
            nonkernel.Clear();
            nonKernelPool.Push(nonkernel);
        }
        public LRItemSet StartFromItemSet
        {
            get
            {
                return this.startFromItemSet;
            }
        }
        public bool IsCompatibleLookahead(int ticket1, int ticket2)
        {
            return this.tks.IsCompatibleLookAhead(ticket1, ticket2);
        }
        public static LRItemSet CreateRootItemSet0(ActiveTokenInfoCollection _tks, NTDefinition augmentedG, LRStyle lrStyle)
        {

            //TokenInfo use_eof = (lrStyle == LRStyle.LR0) ? null : TokenInfo._eof;
            TokenDefinition use_eof = TokenDefinition._eof;
            int myLookAheadTicketId = 0;
            if (use_eof != null)
            {
                myLookAheadTicketId = _tks.GetNewLookaheadId();
                _tks.AddTokenInfo(myLookAheadTicketId, use_eof);
            }

            SymbolSequence symbolSeq = augmentedG.GetSequence(0);


            //hash for ticket, for compare the lookup of 2 lr items
            var rootItem = LRItem.CreateRootItem(
                _tks,
                symbolSeq,
                myLookAheadTicketId,
                _tks.CalculateContentHash(myLookAheadTicketId));

            List<LRItem> kernels = new List<LRItem>();
            kernels.Add(rootItem);

            return new LRItemSet(_tks, use_eof, lrStyle, 0, kernels);

        }
        public int KernelItemsHash
        {
            get;
            private set;
        }
        public LRStyle LRStyle
        {
            get;
            private set;
        }
        public byte[] Md5Hash { get; private set; }
        void AddNonKernel(LRItem item)
        {
            //this method is private, use when expansion
            this.nonKernelItems.AddNonKernel(item);
        }
        public int ItemSetNumber
        {
            get;
            set;
        }
        public string ItemSetName
        {
            get
            {
                return this.ItemSetNumber.ToString();
            }
        }

        public ISymbolDefinition JumpOverFromSymbol
        {
            get;
            private set;
        }
        public bool IsExpand
        {
            get;
            private set;
        }
        public void ClearNonKernelCollection()
        {

            ReleaseNonkernel(this.nonKernelItems);
            this.nonKernelItems = null;
        }
        public IEnumerable<LRItem> GetOnlyKernelItemIterForward()
        {
            int j = onlyKernelItems.Count;
            for (int i = 0; i < j; ++i)
            {
                yield return onlyKernelItems[i];
            }
        }

        public IEnumerable<LRItem> GetAllItemIterForward()
        {
            int j = onlyKernelItems.Count;
            for (int i = 0; i < j; ++i)
            {
                yield return onlyKernelItems[i];
            }
            foreach (LRItem nonKernel in this.nonKernelItems.GetItemIterForward())
            {
                yield return nonKernel;
            }

        }

        public void AddPossibleNextItemSet(LRItemSet itemset)
        {

            if (!this.nextItemSets.ContainsKey(itemset))
            {

                nextItemSets.Add(itemset, 0);
            }
        }
        public IEnumerable<LRItemSet> GetNextItemSetIter()
        {
            foreach (LRItemSet itemset in this.nextItemSets.Keys)
            {
                yield return itemset;
            }
        }

        public override string ToString()
        {
            StringBuilder stBuilder = new StringBuilder();
            stBuilder.Append("I:");
            stBuilder.Append(this.ItemSetName);
            stBuilder.Append(" from ");
            stBuilder.Append(this.JumpOverFromSymbol);
            return stBuilder.ToString();
        }
        internal void PrintLookaheadTicket(int ticketId, StringBuilder stbuilder)
        {
            //for debug
            this.tks.PrintLookaheadTokens(ticketId, stbuilder);
        }
        /// <summary>
        /// same sq and same lookahead
        /// </summary>
        /// <param name="sqNumber"></param>
        /// <param name="lookaheadTk"></param>
        /// <returns></returns>
        LRItem ContainsCompatibleNonKernelSeq(SymbolSequence ss, int lookaheadTicketId, int lookaheadContentHash)
        {
            //LRItem existing;
            return this.nonKernelItems.ContainsCompatibleSeq(ss, lookaheadTicketId, lookaheadContentHash);

        }


#if DEBUG
        static int dbugNSort;
#endif



        public bool ContainsEquivalentKernels(ProtoLRItemSet proto)
        {

            List<LRItem> protoKernelItems = proto.GetArrangedKernelItems();// proto.GetKernelItems();
            //1. same amount of item
            if (protoKernelItems.Count != this.onlyKernelCount)
            {
                return false;
            }

            switch (this.LRStyle)
            {
                case LRStyle.LR0:
                default:
                    {
                        //LR0 & LALR -> not check lookahead
                        for (int n = protoKernelItems.Count - 1; n > -1; --n)
                        {
                            if (this.FindEquivalentKernelOnly(protoKernelItems[n]) < 0)
                            {
                                return false;
                            }
                        }

                        return true;
                    }
                case LRStyle.LR1:
                    {
                        //arrange 

                        if (!isKernelArranged)
                        {
                            //should eq one by one ?
                            this.Md5Hash = LRItemSetSortAndHash.SortAndHash(onlyKernelItems);
                            isKernelArranged = true;
                        }
                        //------------------------------------------------------
                        bool foundResult = false;
                        bool result = false;
                        //check hash and hash


                        bool md5Check = LRItemSetSortAndHash.CompareMd5Hash(proto.Md5Hash, this.Md5Hash);
                        return md5Check;


                        //for (int n = protoKernelItems.Count - 1; n > -1; --n)
                        //{
                        //    //***after sort, compare one by one***
                        //    LRItem myLRItem = onlyKernelItems[n];
                        //    LRItem protoLRItem = protoKernelItems[n];
                        //    if (!myLRItem.IsEquivalentSequenceAndLookahead(protoLRItem))
                        //    {
                        //        //return false;
                        //        foundResult = true;
                        //        result = false;
                        //        break;
                        //    }
                        //}
                        //if (!foundResult)
                        //{
                        //    result = true;
                        //    foundResult = true;
                        //}

                        //if (md5Check != result)
                        //{

                        //}

                        ////---------------------------------------------------/
                        ////classic check
                        //for (int n = protoKernelItems.Count - 1; n > -1; --n)
                        //{
                        //    if (this.FindEquivalentKernelAndLookAheads(protoKernelItems[n]) < 0)
                        //    {
                        //        if (result != false)
                        //        {
                        //        }
                        //        return false;
                        //    }
                        //}
                        //if (result != true)
                        //{
                        //}
                        //return true;
                        ////---------------------------------------------------/

                        return result;
                    }
            }
        }

        public void AddLALR1LookAheads(ProtoLRItemSet proto, ActiveTokenInfoCollection tks)
        {
            List<LRItem> protoKernels = proto.GetKernelItems();
            List<LRItem> thisKernels = this.onlyKernelItems;
            int j = protoKernels.Count;
            for (int i = 0; i < j; ++i)
            {
                LRItem protoKernel = protoKernels[i];
                int pos = FindEquivalentKernelOnly(protoKernel);
                //merge lookahead 
                thisKernels[pos].MergeSubLALRItem(protoKernel.LookaheadTicketId, tks);
            }

            //desitibute down
            if (this.nonKernelItems != null)
            {
                foreach (LRItem nonKernel in this.nonKernelItems.GetItemIterForward())
                {
                    LRItem expandFrom = nonKernel.ExpandFromItem;
                    if (expandFrom == null)
                    {
                        continue;
                    }
                    //merge 
                    if (expandFrom.IsLALRLookaheadChanged)
                    {
                        nonKernel.MergeSubLALRItem(expandFrom.LookaheadTicketId, tks);
                    }
                }
            }
            //-----------------------------------------------------
            j = thisKernels.Count;
            for (int i = 0; i < j; ++i)
            {
                thisKernels[i].IsLALRLookaheadChanged = false;
            }


            //-----------------------------------------------------

            //below here , we use chain for preventing recursive
            //-----------------------------------------------------

            List<LRItemSet> updateChain = new List<LRItemSet>();
            foreach (LRItemSet next in this.nextItemSets.Keys)
            {
                if (next == this)
                {
                    continue;
                }

                next.UpdateLALRLookaheadPropagation(tks, updateChain);
            }
            //-----------------------------------------------------
            foreach (LRItemSet itemset in updateChain)
            {
                itemset.updating = true;
            }
        }
        bool updating = false;
        void UpdateLALRLookaheadPropagation(ActiveTokenInfoCollection tks, List<LRItemSet> updateChain)
        {

            if (this.updating)
            {
                return;
            }
            this.updating = true;
            updateChain.Add(this);

            List<LRItem> thisKernels = this.onlyKernelItems;
            int j = thisKernels.Count;
            for (int i = 0; i < j; ++i)
            {
                LRItem kernel = thisKernels[i];
                LRItem prevItem = kernel.PrevItem;
                kernel.MergeSubLALRItem(prevItem.LookaheadTicketId, tks);
            }


            if (this.nonKernelItems != null)
            {
                foreach (LRItem nonKernel in this.nonKernelItems.GetItemIterForward())
                {

                    LRItem expandFrom = nonKernel.ExpandFromItem;
                    if (expandFrom == null)
                    {
                        continue;
                    }
                    //merge 
                    if (expandFrom.IsLALRLookaheadChanged)
                    {
                        nonKernel.MergeSubLALRItem(expandFrom.LookaheadTicketId, tks);
                    }
                }
            }

            //-----------------------------------------------------
            j = thisKernels.Count;
            for (int i = 0; i < j; ++i)
            {
                thisKernels[i].IsLALRLookaheadChanged = false;
            }
            //-----------------------------------------------------

            foreach (LRItemSet next in this.nextItemSets.Keys)
            {
                if (next == this)
                {
                    continue;
                }
                next.UpdateLALRLookaheadPropagation(tks, updateChain);
            }


        }
        ///// <summary>
        ///// check if LRItemSet has kernel that compat and LR item  
        ///// </summary>
        ///// <param name="lrItem"></param>
        ///// <returns></returns>
        //int FindEquivalentKernelAndLookAheads(LRItem lrItem)
        //{
        //    //if not sort, then must use this method
        //    for (int i = onlyKernelItems.Count - 1; i > -1; --i)
        //    {
        //        if (onlyKernelItems[i].IsEquivalentSequenceAndLookahead(lrItem))
        //        {
        //            return i;
        //        }
        //    }
        //    return -1;
        //}
        int FindEquivalentKernelOnly(LRItem lrItem)
        {

            for (int i = onlyKernelItems.Count - 1; i > -1; --i)
            {
                if (onlyKernelItems[i].IsEquivalentSequence(lrItem))
                {
                    return i;
                }
            }
            return -1;
        }

#if DEBUG
        static int dbugCounter;
#endif


        static Stack<List<LRItem>> lrItemListPool = new Stack<List<LRItem>>();
        static List<LRItem> GetLRItemList()
        {
            if (lrItemListPool.Count > 0)
            {
                //Console.WriteLine(lrItemListPool.Count);
                return lrItemListPool.Pop();
            }
            else
            {
                return new List<LRItem>();
            }
        }
        static void ReleaseLRItemList(List<LRItem> itemList)
        {
            itemList.Clear();
            lrItemListPool.Push(itemList);
        }

        /// <summary>
        /// closure process
        /// </summary>
        /// <param name="lrItemSet"></param>
        /// <param name="dicGrammars"></param>
        static void ExpandLRItemSet(LRItemSet currentItemSet)
        {

            List<LRItem> kernels = currentItemSet.onlyKernelItems;
            ActiveTokenInfoCollection tks = currentItemSet.tks;

#if DEBUG
            //Console.WriteLine(dbugCounter++);
#endif
            //--------------------------------------------------------
            int j = kernels.Count;
            List<LRItem> myset1 = GetLRItemList();
            List<LRItem> myset2 = GetLRItemList();
            List<LRItem> set1 = myset1;// new List<LRItem>();
            List<LRItem> set2 = myset2; //new List<LRItem>();
            List<LRItem> inputSet = null;
            List<LRItem> newOutput = null;

            for (int i = 0; i < j; ++i)
            {


                LRItem itm = kernels[i];

                if (itm.IsEnd)
                {

                    continue;
                }
                set1.Clear();
                set2.Clear();
                //--------------------
                inputSet = set1;//swap
                newOutput = set2;//swap
                inputSet.Add(itm);
                do
                {

                    int m = inputSet.Count;

                    for (int n = 0; n < m; ++n)
                    {
                        ExpandLRItem(tks,
                            inputSet[n],
                            currentItemSet,
                            newOutput);
                    }
                    //swap
                    List<LRItem> tmp = inputSet;
                    inputSet = newOutput;
                    newOutput = tmp;
                    tmp.Clear();

                } while (inputSet.Count > 0);

            }
            currentItemSet.IsExpand = true;

            ReleaseLRItemList(myset1);
            ReleaseLRItemList(myset2);

        }

        /// <summary>
        /// expand each LR item  
        /// </summary>
        /// <param name="originalItem"></param>
        /// <param name="currentItemSet"></param>
        /// <param name="newExpandList"></param>
        static void ExpandLRItem(
           ActiveTokenInfoCollection tks,
           LRItem originalItem,
           LRItemSet currentItemSet,
           List<LRItem> newExpandList)
        {
            //1. ask for b symbol
            //b symbol is nt so-> it can expand more
            //so find it out from grammar
            NTDefinition ntB;
            if (!originalItem.GetEntering_B_NT_Symbol(out ntB))
            {
                return;
            }

            //expansion process  
            //1. extract its lookaheads
            //bool isSharedTicketId;
            int lkTicketId = 0;
            int lkContentHash = 0;
            bool isSharedTicketId = false;
            if (currentItemSet.LRStyle == LRStyle.LR0)
            {
                //LR0 not use lookahead *** 
                //isSharedTicketId = false;
            }
            else
            {
                //LR 1,LALR use lookahead 
                //LALR not check lookahead when collection, use merge process instead
                //LR1 ticket created once and not be merged

                lkTicketId = originalItem.GetLookaheadsTicketForLR1(tks, out isSharedTicketId);
                lkContentHash = tks.CalculateContentHash(lkTicketId);
            }
            //------------------------------

            int j = ntB.SeqCount;

            switch (currentItemSet.LRStyle)
            {
                default: throw new NotSupportedException();
                case LRStyle.LR1:
                    {

                        bool used = false;
                        for (int i = ntB.SeqCount - 1; i >= 0; --i)
                        {
                            SymbolSequence subsq = ntB.GetSequence(i);

                            //1. non kernel has dotpos = 0 , so exclude it , not in this calcualtion
                            //lrItemHash = (subsq.SeqNumber << 16) + lkContentHash; 
                            LRItem found = currentItemSet.ContainsCompatibleNonKernelSeq(subsq, lkTicketId, lkContentHash);
                            if (found == null)
                            {

                                LRItem newNonkernelItem = new LRItem(
                                    currentItemSet,
                                    originalItem,
                                    subsq,
                                    lkTicketId,
                                    lkContentHash);

                                currentItemSet.AddNonKernel(newNonkernelItem);
                                //---------------------  
                                newExpandList.Add(newNonkernelItem);
                                //---------------------  
                                used = true;
                            }
                            else
                            {
                            }
                        }
                        //------------------------------------------------  
                        if (!used && !isSharedTicketId)
                        {
                            //LR1 can reuse ticket 
                            tks.ReuseLastestTicket(lkTicketId);
                        }
                    }
                    break;
                case LRStyle.LR0:
                    {
                        for (int i = 0; i < j; ++i)
                        {
                            SymbolSequence subsq = ntB.GetSequence(i);
                            LRItem found = currentItemSet.ContainsCompatibleNonKernelSeq(subsq, lkTicketId, lkContentHash);
                            if (found == null)
                            {
                                LRItem newNonkernelItem = new LRItem(
                                    currentItemSet,
                                    originalItem,
                                    subsq,
                                    lkTicketId, lkContentHash);
                                currentItemSet.AddNonKernel(newNonkernelItem);
                                //---------------------  
                                newExpandList.Add(newNonkernelItem);
                                //---------------------  
                            }
                        }

                    }
                    break;
                case LRStyle.LALR:
                    {
                        for (int i = 0; i < j; ++i)
                        {
                            SymbolSequence subsq = ntB.GetSequence(i);
                            //lrItemHash = (subsq.SeqNumber << 16) + lkContentHash;
                            LRItem found = currentItemSet.ContainsCompatibleNonKernelSeq(subsq, lkTicketId, lkContentHash);
                            if (found == null)
                            {

                                LRItem newNonkernelItem = new LRItem(
                                    currentItemSet,
                                    originalItem,
                                    subsq,
                                    lkTicketId, lkContentHash);
                                currentItemSet.AddNonKernel(newNonkernelItem);
                                //---------------------  
                                newExpandList.Add(newNonkernelItem);
                                //---------------------  
                            }
                            else
                            {

                                //expand LALR not case lookahead ?
                                found.MergeSubLALRItem(lkTicketId, tks);
                            }
                        }
                    }
                    break;
            }
        }
        public static LRItem GetFirstKernelItemForSample(LRItemSet itemSet)
        {
            return itemSet.onlyKernelItems[0];

        }
    }

    public enum LRStyle
    {
        LR0,
        LR1,
        LALR
    }


    public class TestLRParsingTableRow
    {
        Dictionary<ISymbolDefinition, LRItemTodo> todoDic = new Dictionary<ISymbolDefinition, LRItemTodo>();
        internal TestLRParsingTableRow()
        {
        }

        public void AddShiftTask(ISymbolDefinition symbol, int stateNumber)
        {
            todoDic.Add(symbol, LRItemTodo.CreateShiftTask(stateNumber, 0, 0));
        }
        public void AddReductionTask(ISymbolDefinition symbol, int stateNumber)
        {
            todoDic.Add(symbol, LRItemTodo.CreateReduceTask(stateNumber));
        }
        public void AddGotoTask(ISymbolDefinition symbol, int stateNumber)
        {
            todoDic.Add(symbol, LRItemTodo.CreateGotoTask(stateNumber));
        }
        public void AddAcceptTask(ISymbolDefinition symbol)
        {
            todoDic.Add(symbol, LRItemTodo.CreateAcceptTask());
        }
        public void AddErrorTask(ISymbolDefinition symbol, int stateNumber)
        {
            todoDic.Add(symbol, LRItemTodo.CreateErrorTask(stateNumber));
        }
        public LRItemTodo GetTodo(ISymbolDefinition symbolDef)
        {
            LRItemTodo todo;
            todoDic.TryGetValue(symbolDef, out todo);
            return todo;
        }
    }
    public class TestLRParsingTable
    {
        List<TestLRParsingTableRow> testRows = new List<TestLRParsingTableRow>();
        public TestLRParsingTable()
        {
        }
        public TestLRParsingTableRow StartNewTableRow()
        {
            TestLRParsingTableRow newRow = new TestLRParsingTableRow();
            testRows.Add(newRow);
            return newRow;
        }
        public void CheckLRParsingTable(LRParsingTable tableToCheck, StringBuilder reporter)
        {
            ColumnBasedTable<NTDefinition, LRItemTodo> ntTable = tableToCheck.ntTable;
            ColumnBasedTable<TokenDefinition, LRItemTodo> tkTable = tableToCheck.tokenTable;

            int rowCount = ntTable.RowCount;
            if (rowCount != testRows.Count)
            {
                reporter.Append("row count not match!");
                return;
            }

            for (int r = 0; r < rowCount; ++r)
            {
                TestLRParsingTableRow row = testRows[r];
                int j = ntTable.columns.Count;
                for (int i = 0; i < j; ++i)
                {
                    LRItemTodo todo = ntTable.GetCell(r, i);
                    NTDefinition ntdef = ntTable.columns[i].columnHeader;
                    LRItemTodo expectedTodo = row.GetTodo(ntdef);

                    if (todo.StateNumber != expectedTodo.StateNumber)
                    {
                        reporter.Append("row " + r + " on " + ntdef.ToString() + " state number not match");
                    }
                    else if (todo.ItemKind == expectedTodo.ItemKind)
                    {
                        reporter.Append("row " + r + " on " + ntdef.ToString() + " item kind number not match");
                    }
                    else
                    {//ok

                    }
                }

                j = tkTable.columns.Count;
                for (int i = 0; i < j; ++i)
                {
                    LRItemTodo todo = tkTable.GetCell(r, i);
                    TokenDefinition tkdef = tkTable.columns[i].columnHeader;
                    LRItemTodo expectedTodo = row.GetTodo(tkdef);
                    if (todo.StateNumber != expectedTodo.StateNumber)
                    {
                        reporter.Append("row " + r + " on " + tkdef.ToString() + " state number not match");
                    }
                    else if (todo.ItemKind == expectedTodo.ItemKind)
                    {
                        reporter.Append("row " + r + " on " + tkdef.ToString() + " item kind number not match");
                    }
                    else
                    {//ok

                    }
                }
            }
        }
    }
    class LRItemSetCollection
    {
        List<LRItemSet> myLRItemSets = new List<LRItemSet>();
        Dictionary<ISymbolDefinition, List<LRItemSet>> myLRItemSetDic = new Dictionary<ISymbolDefinition, List<LRItemSet>>();
        int totalNumber;

        public LRItemSetCollection(LRStyle myLRStyle)
        {
            this.LRStyle = myLRStyle;
        }
        public LRStyle LRStyle
        {
            get;
            private set;
        }


        public void AddItemSet(LRItemSet itemSet)
        {
            this.myLRItemSets.Add(itemSet);
            List<LRItemSet> existingSubset;
            if (!myLRItemSetDic.TryGetValue(itemSet.JumpOverFromSymbol, out existingSubset))
            {
                existingSubset = new List<LRItemSet>();
                myLRItemSetDic.Add(itemSet.JumpOverFromSymbol, existingSubset);
            }
            existingSubset.Add(itemSet);
            this.totalNumber++;
        }
        public void Clear()
        {
            this.totalNumber = 0;
            this.myLRItemSetDic.Clear();
            this.myLRItemSets.Clear();

        }
        public int Count
        {
            get
            {
                return myLRItemSets.Count;
            }
        }
        public LRItemSet GetItemSet(int index)
        {
            return this.myLRItemSets[index];
        }

        public LRItemSet FindCompatibleItemSet(ProtoLRItemSet proto)
        {
            //check if we already have lrItemSet that has the same kernel
            //with the proto
            List<LRItemSet> existingSet;
            if (this.myLRItemSetDic.TryGetValue(proto.JumpOverSymbol, out existingSet))
            {
                //if (existingSet.Count > 30)
                //{
                //}
                //Console.WriteLine("set " + existingSet.Count);

                for (int i = existingSet.Count - 1; i > -1; --i)
                {
                    LRItemSet lrItemSet = existingSet[i];
                    if (lrItemSet.ContainsEquivalentKernels(proto))
                    {
                        return lrItemSet;
                        //if (proto.StartFromItemSet != lrItemSet.JumpOverFromSymbol)
                        //{
                        //    return lrItemSet;
                        //}
                        //else
                        //{
                        //    return lrItemSet;
                        //}
                    }
                }
            }
            return null;

        }
    }


    //--------------------
    // reference LR parser
    //--------------------
    public delegate void LRParserSwitchHandler(ParserSwitchContext context);



    class StateStack
    {

        int[] states = new int[32];
        int index = -1;
        public StateStack()
        {

        }
        public void Clear()
        {
            index = -1;
        }
        public void PushNull()
        {
            states[++index] = 0;
        }
        public void Push(int todo)
        {
#if DEBUG
            //if (todo != null && todo.dbugId == 1601)
            //{
            //}
#endif
            //note that return value is peek value , not pop value
            states[++index] = todo;
        }
        public int PopAndPeek()
        {
            return states[--index];
        }

        /// <summary>
        /// ignore pop
        /// </summary>
        /// <param name="count"></param>
        public int Pop(int count)
        {
            //ignor pop           
            //states.RemoveRange((index -= count) + 1, count);
            //assign and remove ***

            index -= count;
            return states[index];
        }
        public int Count
        {
            get { return index + 1; }
        }
        public virtual LRItemTodo FindLatestGoto()
        {
            throw new NotSupportedException();
            return LRItemTodo.Empty;
        }
    }

    class StateStackDev
    {
        LRItemTodo[] todoStates = new LRItemTodo[32];
        int[] states = new int[32];
        int index = -1;

        public StateStackDev()
        {

        }
        public void Clear()
        {

            index = -1;

        }
        public void PushNull()
        {
            todoStates[index + 1] = LRItemTodo.Empty;
            states[++index] = 0;
        }
        public void Push(int todo, LRItemTodo lrItemTodo)
        {
#if DEBUG
            //if (todo != null && todo.dbugId == 1601)
            //{
            //}
#endif
            todoStates[index + 1] = lrItemTodo;
            states[++index] = todo;
        }
        public int PopAndPeek()
        {

            //note that return value is peek value , not pop value
            return states[--index];
        }

        /// <summary>
        /// ignore pop
        /// </summary>
        /// <param name="count"></param>
        public int Pop(int count)
        {
            //ignor pop           
            //states.RemoveRange((index -= count) + 1, count);
            //assign and remove ***
            //peekState = (index >= 0) ? states[index] : 0;
            index -= count;
            return states[index];
        }
        public int Count
        {
            get { return index + 1; }
        }

        public virtual LRItemTodo FindLatestGoto()
        {
            throw new NotSupportedException();
            return LRItemTodo.Empty;
        }
    }

    class SimpleLexeme
    {
        string lexeme;
        public SimpleLexeme(string lexeme)
        {
            this.lexeme = lexeme;
        }
        public override string ToString()
        {
            return lexeme;
        }

    }


    class ParseNodeStack
    {
        int index = -1;
        ParseNode[] data = new ParseNode[32];
#if DEBUG
        static int dbugTotalId = 0;
        public readonly int dbugId = dbugTotalId++;
#endif
        public ParseNodeStack()
        {

        }
        public ParseNode Peek()
        {
            return data[index];
        }
        public void Clear()
        {
            index = -1;
        }
        public int Count
        {
            get { return index + 1; }
        }
        public void Push(ParseNode d)
        {
            if (d == null)
            {

            }
            data[++index] = d;
        }
        public ParseNode Pop()
        {
            return data[index--];
        }

        public void Pop(out ParseNode t0, out ParseNode t1)
        {
            t0 = data[index - 1];
            t1 = data[index];
            index -= 2;
        }
        public void Pop(out ParseNode t0, out ParseNode t1, out ParseNode t2)
        {
            t0 = data[index - 2];
            t1 = data[index - 1];
            t2 = data[index];
            index -= 3;
        }
        public void Peek(out ParseNode t0, out ParseNode t1, out ParseNode t2)
        {
            t0 = data[index - 2];
            t1 = data[index - 1];
            t2 = data[index];
        }
        static void StoreValue(ref ParseNode p, ref PNode nn)
        {
            if (p._kind == ParseNodeKind.Tk)
            {
                Token tk = (Token)p;
                ////token
                nn.startAt = tk.startAt; //char start at  
                switch (tk.TkInfo.UserTokenKind)
                {
                    case BasicTokenKind.Identifier:
                    case BasicTokenKind.LiteralNumber:
                    case BasicTokenKind.LiteralString:
                        {
                            //assign simple lexeme value
                            nn.otherInfo = new SimpleLexeme(tk.GetLexeme());
                            //nn.otherInfo = tk.TokenCharacterCount;
                        }
                        break;
                    default:
                        {
                            nn.tkDef = tk.TkInfo;
                        }
                        break;
                }

            }
            else
            {
                nn.ntNode = p;
            }
            p = null;
        }
        public NTn1 PopReverseIntoObject(NTn1 p)
        {
            //p.n1 = GetProperValue(data[index--]);
            StoreValue(ref data[index--], ref p.n1);
            return p;
        }
        public NTn2 PopReverseIntoObject(NTn2 p)
        {
            //p.n1 = data[index - 1];
            //p.n2 = data[index];

            //p.n1 = GetProperValue(data[index - 1]);
            //p.n2 = GetProperValue(data[index]);

            StoreValue(ref data[index - 1], ref p.n1);
            StoreValue(ref data[index], ref p.n2);

            index -= 2;
            return p;
        }
        public NTn3 PopReverseIntoObject(NTn3 p)
        {
            //p.n1 = data[index - 2];
            //p.n2 = data[index - 1];
            //p.n3 = data[index];

            //p.n1 = GetProperValue(data[index - 2]);
            //p.n2 = GetProperValue(data[index - 1]);
            //p.n3 = GetProperValue(data[index]);

            StoreValue(ref data[index - 2], ref p.n1);
            StoreValue(ref data[index - 1], ref p.n2);
            StoreValue(ref data[index], ref p.n3);

            index -= 3;
            return p;
        }
        public NTn4 PopReverseIntoObject(NTn4 p)
        {
            //p.n1 = data[index - 3];
            //p.n2 = data[index - 2];
            //p.n3 = data[index - 1];
            //p.n4 = data[index];

            //p.n1 = GetProperValue(data[index - 3]);
            //p.n2 = GetProperValue(data[index - 2]);
            //p.n3 = GetProperValue(data[index - 1]);
            //p.n4 = GetProperValue(data[index]);

            StoreValue(ref data[index - 3], ref p.n1);
            StoreValue(ref data[index - 2], ref p.n2);
            StoreValue(ref data[index - 1], ref p.n3);
            StoreValue(ref data[index], ref p.n4);


            index -= 4;
            return p;
        }
        public NTn5 PopReverseIntoObject(NTn5 p)
        {

            //p.n1 = data[index - 4];
            //p.n2 = data[index - 3];
            //p.n3 = data[index - 2];
            //p.n4 = data[index - 1];
            //p.n5 = data[index];

            //p.n1 = GetProperValue(data[index - 4]);
            //p.n2 = GetProperValue(data[index - 3]);
            //p.n3 = GetProperValue(data[index - 2]);
            //p.n4 = GetProperValue(data[index - 1]);
            //p.n5 = GetProperValue(data[index]);

            StoreValue(ref data[index - 4], ref p.n1);
            StoreValue(ref data[index - 3], ref p.n2);
            StoreValue(ref data[index - 2], ref p.n3);
            StoreValue(ref data[index - 1], ref p.n4);
            StoreValue(ref data[index], ref p.n5);
            index -= 5;
            return p;
        }
        public NTn6 PopReverseIntoObject(NTn6 p)
        {
            //p.n1 = data[index - 5];
            //p.n2 = data[index - 4];
            //p.n3 = data[index - 3];
            //p.n4 = data[index - 2];
            //p.n5 = data[index - 1];
            //p.n6 = data[index];

            //p.n1 = GetProperValue(data[index - 5]);
            //p.n2 = GetProperValue(data[index - 4]);
            //p.n3 = GetProperValue(data[index - 3]);
            //p.n4 = GetProperValue(data[index - 2]);
            //p.n5 = GetProperValue(data[index - 1]);
            //p.n6 = GetProperValue(data[index]);


            StoreValue(ref data[index - 5], ref p.n1);
            StoreValue(ref data[index - 4], ref p.n2);
            StoreValue(ref data[index - 3], ref p.n3);
            StoreValue(ref data[index - 2], ref p.n4);
            StoreValue(ref data[index - 1], ref p.n5);
            StoreValue(ref data[index], ref p.n6);

            index -= 6;
            return p;
        }
        public NTn7 PopReverseIntoObject(NTn7 p)
        {
            //p.n1 = data[index - 6];
            //p.n2 = data[index - 5];
            //p.n3 = data[index - 4];
            //p.n4 = data[index - 3];
            //p.n5 = data[index - 2];
            //p.n6 = data[index - 1];
            //p.n7 = data[index];

            //p.n1 = GetProperValue(data[index - 6]);
            //p.n2 = GetProperValue(data[index - 5]);
            //p.n3 = GetProperValue(data[index - 4]);
            //p.n4 = GetProperValue(data[index - 3]);
            //p.n5 = GetProperValue(data[index - 2]);
            //p.n6 = GetProperValue(data[index - 1]);
            //p.n7 = GetProperValue(data[index]);


            StoreValue(ref data[index - 6], ref p.n1);
            StoreValue(ref data[index - 5], ref p.n2);
            StoreValue(ref data[index - 4], ref p.n3);
            StoreValue(ref data[index - 3], ref p.n4);
            StoreValue(ref data[index - 2], ref p.n5);
            StoreValue(ref data[index - 1], ref p.n6);
            StoreValue(ref data[index], ref p.n7);

            index -= 7;
            return p;
        }
        public NTn8 PopReverseIntoObject(NTn8 p)
        {

            //p.n1 = GetProperValue(data[index - 7]);
            //p.n2 = GetProperValue(data[index - 6]);
            //p.n3 = GetProperValue(data[index - 5]);
            //p.n4 = GetProperValue(data[index - 4]);
            //p.n5 = GetProperValue(data[index - 3]);
            //p.n6 = GetProperValue(data[index - 2]);
            //p.n7 = GetProperValue(data[index - 1]);
            //p.n8 = GetProperValue(data[index]);

            StoreValue(ref data[index - 7], ref p.n1);
            StoreValue(ref data[index - 6], ref p.n2);
            StoreValue(ref data[index - 5], ref p.n3);
            StoreValue(ref data[index - 4], ref p.n4);
            StoreValue(ref data[index - 3], ref p.n5);
            StoreValue(ref data[index - 2], ref p.n6);
            StoreValue(ref data[index - 1], ref p.n7);
            StoreValue(ref data[index], ref p.n8);
            //p.n1 = data[index - 7];
            //p.n2 = data[index - 6];
            //p.n3 = data[index - 5];
            //p.n4 = data[index - 4];
            //p.n5 = data[index - 3];
            //p.n6 = data[index - 2];
            //p.n7 = data[index - 1];
            //p.n8 = data[index];

            index -= 8;
            return p;
        }
        public NTn9 PopReverseIntoObject(NTn9 p)
        {

            //p.n1 = data[index - 8];
            //p.n2 = data[index - 7];
            //p.n3 = data[index - 6];
            //p.n4 = data[index - 5];
            //p.n5 = data[index - 4];
            //p.n6 = data[index - 3];
            //p.n7 = data[index - 2];
            //p.n8 = data[index - 1];
            //p.n9 = data[index];

            //p.n1 = GetProperValue(data[index - 8]);
            //p.n2 = GetProperValue(data[index - 7]);
            //p.n3 = GetProperValue(data[index - 6]);
            //p.n4 = GetProperValue(data[index - 5]);
            //p.n5 = GetProperValue(data[index - 4]);
            //p.n6 = GetProperValue(data[index - 3]);
            //p.n7 = GetProperValue(data[index - 2]);
            //p.n8 = GetProperValue(data[index - 1]);
            //p.n9 = GetProperValue(data[index]);


            StoreValue(ref data[index - 8], ref p.n1);
            StoreValue(ref data[index - 7], ref p.n2);
            StoreValue(ref data[index - 6], ref p.n3);
            StoreValue(ref data[index - 5], ref p.n4);
            StoreValue(ref data[index - 4], ref p.n5);
            StoreValue(ref data[index - 3], ref p.n6);
            StoreValue(ref data[index - 2], ref p.n7);
            StoreValue(ref data[index - 1], ref p.n8);
            StoreValue(ref data[index], ref p.n9);

            index -= 9;
            return p;
        }
        public NTn10 PopReverseIntoObject(NTn10 p)
        {
            //p.n1 = data[index - 9];
            //p.n2 = data[index - 8];
            //p.n3 = data[index - 7];
            //p.n4 = data[index - 6];
            //p.n5 = data[index - 5];
            //p.n6 = data[index - 4];
            //p.n7 = data[index - 3];
            //p.n8 = data[index - 2];
            //p.n9 = data[index - 1];
            //p.n10 = data[index];
            StoreValue(ref data[index - 9], ref p.n1);
            StoreValue(ref data[index - 8], ref p.n2);
            StoreValue(ref data[index - 7], ref p.n3);
            StoreValue(ref data[index - 6], ref p.n4);
            StoreValue(ref data[index - 5], ref p.n5);
            StoreValue(ref data[index - 4], ref p.n6);
            StoreValue(ref data[index - 3], ref p.n7);
            StoreValue(ref data[index - 2], ref p.n8);
            StoreValue(ref data[index - 1], ref p.n9);
            StoreValue(ref data[index], ref p.n10);

            //p.n1 = GetProperValue(data[index - 9]);
            //p.n2 = GetProperValue(data[index - 8]);
            //p.n3 = GetProperValue(data[index - 7]);
            //p.n4 = GetProperValue(data[index - 6]);
            //p.n5 = GetProperValue(data[index - 5]);
            //p.n6 = GetProperValue(data[index - 4]);
            //p.n7 = GetProperValue(data[index - 3]);
            //p.n8 = GetProperValue(data[index - 2]);
            //p.n9 = GetProperValue(data[index - 1]);
            //p.n10 = GetProperValue(data[index]);


            index -= 10;
            return p;
        }
        public NTn11 PopReverseIntoObject(NTn11 p)
        {
            //p.n1 = data[index - 10];
            //p.n2 = data[index - 9];
            //p.n3 = data[index - 8];
            //p.n4 = data[index - 7];
            //p.n5 = data[index - 6];
            //p.n6 = data[index - 5];
            //p.n7 = data[index - 4];
            //p.n8 = data[index - 3];
            //p.n9 = data[index - 2];
            //p.n10 = data[index - 1];
            //p.n11 = data[index];

            //p.n1 = GetProperValue(data[index - 10]);
            //p.n2 = GetProperValue(data[index - 9]);
            //p.n3 = GetProperValue(data[index - 8]);
            //p.n4 = GetProperValue(data[index - 7]);
            //p.n5 = GetProperValue(data[index - 6]);
            //p.n6 = GetProperValue(data[index - 5]);
            //p.n7 = GetProperValue(data[index - 4]);
            //p.n8 = GetProperValue(data[index - 3]);
            //p.n9 = GetProperValue(data[index - 2]);
            //p.n10 = GetProperValue(data[index - 1]);
            //p.n11 = GetProperValue(data[index]);


            StoreValue(ref data[index - 10], ref p.n1);
            StoreValue(ref data[index - 9], ref p.n2);
            StoreValue(ref data[index - 8], ref p.n3);
            StoreValue(ref data[index - 7], ref p.n4);
            StoreValue(ref data[index - 6], ref p.n5);
            StoreValue(ref data[index - 5], ref p.n6);
            StoreValue(ref data[index - 4], ref p.n7);
            StoreValue(ref data[index - 3], ref p.n8);
            StoreValue(ref data[index - 2], ref p.n9);
            StoreValue(ref data[index - 1], ref p.n10);
            StoreValue(ref data[index], ref p.n11);

            index -= 11;
            return p;
        }
        public NTn12 PopReverseIntoObject(NTn12 p)
        {

            //p.n1 = data[index - 11];
            //p.n2 = data[index - 10];
            //p.n3 = data[index - 9];
            //p.n4 = data[index - 8];
            //p.n5 = data[index - 7];
            //p.n6 = data[index - 6];
            //p.n7 = data[index - 5];
            //p.n8 = data[index - 4];
            //p.n9 = data[index - 3];
            //p.n10 = data[index - 2];
            //p.n11 = data[index - 1];
            //p.n12 = data[index];

            //p.n1 = GetProperValue(data[index - 11]);
            //p.n2 = GetProperValue(data[index - 10]);
            //p.n3 = GetProperValue(data[index - 9]);
            //p.n4 = GetProperValue(data[index - 8]);
            //p.n5 = GetProperValue(data[index - 7]);
            //p.n6 = GetProperValue(data[index - 6]);
            //p.n7 = GetProperValue(data[index - 5]);
            //p.n8 = GetProperValue(data[index - 4]);
            //p.n9 = GetProperValue(data[index - 3]);
            //p.n10 = GetProperValue(data[index - 2]);
            //p.n11 = GetProperValue(data[index - 1]);
            //p.n12 = GetProperValue(data[index]);

            StoreValue(ref data[index - 11], ref p.n1);
            StoreValue(ref data[index - 10], ref p.n2);
            StoreValue(ref data[index - 9], ref p.n3);
            StoreValue(ref data[index - 8], ref p.n4);
            StoreValue(ref data[index - 7], ref p.n5);
            StoreValue(ref data[index - 6], ref p.n6);
            StoreValue(ref data[index - 5], ref p.n7);
            StoreValue(ref data[index - 4], ref p.n8);
            StoreValue(ref data[index - 3], ref p.n9);
            StoreValue(ref data[index - 2], ref p.n10);
            StoreValue(ref data[index - 1], ref p.n11);
            StoreValue(ref data[index], ref p.n12);


            index -= 12;
            return p;
        }
        public NTn13 PopReverseIntoObject(NTn13 p)
        {

            //p.n1 = data[index - 12];
            //p.n2 = data[index - 11];
            //p.n3 = data[index - 10];
            //p.n4 = data[index - 9];
            //p.n5 = data[index - 8];
            //p.n6 = data[index - 7];
            //p.n7 = data[index - 6];
            //p.n8 = data[index - 5];
            //p.n9 = data[index - 4];
            //p.n10 = data[index - 3];
            //p.n11 = data[index - 2];
            //p.n12 = data[index - 1];
            //p.n13 = data[index];

            //p.n1 = GetProperValue(data[index - 12]);
            //p.n2 = GetProperValue(data[index - 11]);
            //p.n3 = GetProperValue(data[index - 10]);
            //p.n4 = GetProperValue(data[index - 9]);
            //p.n5 = GetProperValue(data[index - 8]);
            //p.n6 = GetProperValue(data[index - 7]);
            //p.n7 = GetProperValue(data[index - 6]);
            //p.n8 = GetProperValue(data[index - 5]);
            //p.n9 = GetProperValue(data[index - 4]);
            //p.n10 = GetProperValue(data[index - 3]);
            //p.n11 = GetProperValue(data[index - 2]);
            //p.n12 = GetProperValue(data[index - 1]);
            //p.n13 = GetProperValue(data[index]);


            StoreValue(ref data[index - 12], ref p.n1);
            StoreValue(ref data[index - 11], ref p.n2);
            StoreValue(ref data[index - 10], ref p.n3);
            StoreValue(ref data[index - 9], ref p.n4);
            StoreValue(ref data[index - 8], ref p.n5);
            StoreValue(ref data[index - 7], ref p.n6);
            StoreValue(ref data[index - 6], ref p.n7);
            StoreValue(ref data[index - 5], ref p.n8);
            StoreValue(ref data[index - 4], ref p.n9);
            StoreValue(ref data[index - 3], ref p.n10);
            StoreValue(ref data[index - 2], ref p.n11);
            StoreValue(ref data[index - 1], ref p.n12);
            StoreValue(ref data[index], ref p.n13);

            index -= 13;
            return p;
        }
        public NTnN PopReverseIntoObject(NTnN parentNode)
        {
            PNode[] childNodes = parentNode.nodes;
            int count = childNodes.Length;
            int pos = 0;
            for (int i = count - 1; i >= 0; --i)
            {
                StoreValue(ref data[index], ref childNodes[pos++]);
            }
            index -= count;
            return parentNode;
        }
        public void Clear(int count)
        {

            switch (count)
            {
                case 1:
                    {
                        data[index] = null;
                        index--;
                    }
                    break;
                case 2:
                    {

                        data[index - 1] = data[index] = null;
                        index -= 2;
                    }
                    break;
                case 3:
                    {
                        data[index - 2] = data[index - 1] = data[index] = null;
                        index -= 3;
                    }
                    break;
                case 4:
                    {
                        data[index - 3] = data[index - 2] = data[index - 1] = data[index] = null;
                        index -= 4;
                        break;
                    }
                case 5:
                    {
                        data[index - 4] = data[index - 3] = data[index - 2] = data[index - 1] = data[index] = null;
                        index -= 5;
                        break;
                    }
                case 6:
                    {
                        data[index - 5] = data[index - 4] = data[index - 3] = data[index - 2] = data[index - 1] = data[index] = null;
                        index -= 6;
                        break;
                    }
                case 7:
                    {
                        data[index - 6] = data[index - 5] = data[index - 4] = data[index - 3] = data[index - 2] = data[index - 1] = data[index] = null;
                        index -= 7;
                        break;
                    }
                case 8:
                    {
                        data[index - 7] = data[index - 6] = data[index - 5] = data[index - 4] = data[index - 3] = data[index - 2] = data[index - 1] = data[index] = null;
                        index -= 8;
                        break;
                    }
                case 9:
                    {
                        data[index - 8] = data[index - 7] = data[index - 6] = data[index - 5] = data[index - 4] = data[index - 3] = data[index - 2] = data[index - 1] = data[index] = null;
                        index -= 9;
                        break;
                    }
                case 10:
                    {
                        data[index - 9] = data[index - 8] = data[index - 7] = data[index - 6] = data[index - 5] = data[index - 4] = data[index - 3] = data[index - 2] = data[index - 1] = data[index] = null;
                        index -= 10;
                    }
                    break;
                case 11:
                    {
                        data[index - 10] = data[index - 8] = data[index - 7] = data[index - 6] = data[index - 5] = data[index - 4] = data[index - 3] = data[index - 2] = data[index - 1] = data[index] = null;
                        index -= 11;
                    }
                    break;
                case 12:
                    {
                        data[index - 11] = data[index - 10] = data[index - 8] = data[index - 7] = data[index - 6] = data[index - 5] = data[index - 4] = data[index - 3] = data[index - 2] = data[index - 1] = data[index] = null;
                        index -= 12;
                    }
                    break;
                default:
                    {
                        //from 13
                        for (int i = count; i >= 0; --i)
                        {
                            data[index - i] = null;
                        }
                        index -= count;
                        break;
                    }
            }
        }


    }



    class NonKernelCollection
    {

        //reusable non-kernel collection 
        Dictionary<int, List<LRItem>> sqToLRs = new Dictionary<int, List<LRItem>>();

#if DEBUG
        static int dbugTotalId;
        public readonly int dbugId = dbugTotalId++;
#endif
        public NonKernelCollection()
        {

            //#if DEBUG
            //            Console.WriteLine("nk_collection:" + dbugId);
            //#endif
        }
        public void AddNonKernel(LRItem item)
        {
            int key = item.OriginalSeq.TotalSeqNumber;

            List<LRItem> exitingList;
            if (sqToLRs.TryGetValue(key, out exitingList))
            {
                exitingList.Add(item);
            }
            else
            {
                exitingList = new List<LRItem>();
                sqToLRs.Add(key, exitingList);
                exitingList.Add(item);
            }
            ////separate non kernel into groups
            //LRItem foundItem;
            //if (!this.oneSqToOneLR.TryGetValue(key, out foundItem))
            //{
            //    //check here first
            //    this.oneSqToOneLR.Add(key, item);
            //}
            //else
            //{
            //    //if found item from this sq number ***
            //    this.oneSqToOneLR.Remove(key);
            //    if (this.oneSqToMultipleLR == null)
            //    {
            //        this.oneSqToMultipleLR = new Dictionary<int, List<LRItem>>();
            //    }
            //    List<LRItem> list = new List<LRItem>();
            //    list.Add(item);
            //    list.Add(foundItem);
            //    this.oneSqToMultipleLR.Add(key, list);
            //}

        }

        public LRItem ContainsCompatibleSeq(SymbolSequence ss, int lookaheadTicketId, int lookaheadContentHash)
        {
            //LRItem existing;
            int key = ss.TotalSeqNumber;
            List<LRItem> found;
            if (sqToLRs.TryGetValue(key, out found))
            {

                for (int i = found.Count - 1; i >= 0; --i)
                {
                    LRItem item = found[i];
                    if (item.LookaheadContentHashCode == lookaheadContentHash)
                    {
                        return item;
                    }

                    //if (item.OriginalSeq == ss &&
                    //   item.DotPos == 0 &&
                    if (item.LookaheadContentHashCode == lookaheadContentHash &&
                       item.Owner.IsCompatibleLookahead(item.LookaheadTicketId, lookaheadTicketId))
                    {
                        return item;
                    }
                    //if (item.OriginalSeq == ss &&
                    //    item.DotPos == 0 &&
                    //    item.LookaheadContentHashCode == lookaheadContentHash &&
                    //    item.Owner.IsCompatibleLookahead(item.LookaheadTicketId, lookaheadTicketId))
                    //{
                    //    return item;
                    //}
                    //if (item.IsEquivalentNonKernelSequenceAndLookahead(ss, lookaheadTicketId, lookaheadContentHash))
                    //{
                    //    return item;
                    //}
                }
            }

            //if (oneSqToMultipleLR != null)
            //{ 
            //} 
            //LRItem lr;
            //if (oneSqToOneLR.TryGetValue(key, out lr))
            //{
            //    if (lr.IsEquivalentSequenceAndLookahead(ss, lookaheadTicketId, lookaheadContentHash))
            //    {
            //        return lr;
            //    }
            //}
            return null;
        }
        public IEnumerable<LRItem> GetItemIterForward()
        {
            foreach (var list in sqToLRs.Values)
            {
                int j = list.Count;
                for (int i = 0; i < j; ++i)
                {
                    yield return list[i];
                }
            }
        }
        public void Clear()
        {
            sqToLRs.Clear();
        }
    }


}

