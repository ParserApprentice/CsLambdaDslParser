//MIT 2015-2017, ParserApprentice 
using System;
using System.Text;
using System.Collections.Generic;

namespace Parser.ParserKit
{

    /// <summary>
    /// expected symbol  (right hand production)
    /// </summary>
    public class SymbolSequence
    {

        /// <summary>
        /// left-side of production 
        /// </summary>
        NTDefinition leftside_ntDefinition;
        /// <summary>
        /// right-side of production
        /// </summary>        
        readonly ISymbolDefinition[] rightside_expectedGmParts;
        readonly int rightSidePartCount = 0;

        /// <summary>
        /// store symbols in original user sq
        /// index  -> original position, value -> real parse node position
        /// </summary>
        readonly int[] abstractPositionToRealParseNode;
        /// <summary>
        /// index ->real parse node position , value -> original position 
        /// </summary>
        readonly int[] realParseNodeToAbstractPosition;

        //------------------
        /// <summary>
        /// user symbol definition
        /// </summary>
        readonly UserSymbolSequence user_sym_sq;

        readonly ReductionMonitor[] user_exp_monitors;
        readonly SeqReductionDel _userSeqReductionDel;


        //------------------ 
        //sync extension
        SeqSync[] syncSymbols;
        //------------------ 
        public readonly bool CreatedFromListOfNt;
        public readonly byte listNtRdMonitorFlags;

#if DEBUG

        static int dbugTotalId;
        public readonly int dbugId = dbugTotalId++;
#endif
        internal SymbolSequence(ISymbolDefinition[] words,
            int[] abstractPositionToRealParseNode,
            UserSymbolSequence user_sym_sq)
        {

#if DEBUG

            if (user_sym_sq == null)
            {
                throw new NotSupportedException();
            }
#endif
            this.user_sym_sq = user_sym_sq;

            this.CreatedFromListOfNt = user_sym_sq.CreatedFromListOfNt;

            this.rightside_expectedGmParts = words;

            this.abstractPositionToRealParseNode = abstractPositionToRealParseNode;
            int j = abstractPositionToRealParseNode.Length;
            int[] realParserNodeToAbstractPosition = new int[j];

            int rpos = 0;
            for (int i = 0; i < j; ++i)
            {
                int opos = abstractPositionToRealParseNode[i];
                if (opos > -1)
                {
                    realParserNodeToAbstractPosition[rpos] = i;
                    rpos++;
                }
            }
            if (rpos < j)
            {
                for (int i = rpos; i < j; ++i)
                {
                    realParserNodeToAbstractPosition[rpos] = -1;
                    rpos++;
                }

            }
            this.realParseNodeToAbstractPosition = realParserNodeToAbstractPosition;
            this.Precedence = user_sym_sq.Precedence;
            this.IsRightAssociative = user_sym_sq.IsRightAssociative;
            this.rightSidePartCount = this.rightside_expectedGmParts.Length;

            //----------------------------------------------------------------------------------------------------
            ReductionMonitor[] rdMonitors = new ReductionMonitor[rightSidePartCount];
            for (int i = rightSidePartCount - 1; i >= 0; --i)
            {
                if ((rdMonitors[i] = user_sym_sq[realParseNodeToAbstractPosition[i]].ReductionDel) != null)
                {
                    this.HasSomeUserExpectedSymbolMonitor = true;
                }
                else
                {

                }
            }
            //----------------------------------------------------------------------------------------
            this.user_exp_monitors = rdMonitors;
            this.HasReductionListener = (_userSeqReductionDel = user_sym_sq.GetUserSeqReductionDel()) != null;
            if (this.CreatedFromListOfNt)
            {
                //analyze
                switch (rightSidePartCount)
                {
                    default: throw new NotSupportedException();
                    case 1:

                        break;
                    case 2:
                        //do nothing
                        break;
                    case 3:
                        if (rdMonitors[1] != null)
                        {
                            this.listNtRdMonitorFlags |= 1; //01
                        }
                        if (rdMonitors[2] != null)
                        {
                            this.listNtRdMonitorFlags |= 2; //10
                        }
                        break;
                }
            }
            //----------------------------------------------------------------------------------------------------
            //replace null monitor with blank monitor ?
            //for (int i = rightSidePartCount - 1; i >= 0; --i)
            //{
            //    if ((rdMonitors[i] = user_sym_sq[realParseNodeToAbstractPosition[i]].ReductionDel) != null)
            //    {
            //        this.HasSomeUserExpectedSymbolMonitor = true;
            //    }
            //    else
            //    {

            //    }
            //}

            //----------------------------------------------------------------------------------------------------
        }



        public int Precedence
        {
            get;
            private set;
        }
        public bool IsRightAssociative
        {
            get;
            private set;
        }

        internal int TotalSeqNumber
        {
            get;
            set;
        }
        internal bool CreatedUnderConflict
        {
            get;
            set;
        }

        public ISymbolDefinition GetExpectedSymbol(int index)
        {
            return this.rightside_expectedGmParts[index];
        }

        public int RightSideCount
        {
            get
            {
                return this.rightSidePartCount;

            }
        }

        internal bool IsClosed
        {
            get;
            set;
        }
        internal ISymbolDefinition this[int index]
        {
            get
            {
                return this.rightside_expectedGmParts[index];
            }
        }
        public override string ToString()
        {
            StringBuilder stbuilder = new StringBuilder();
            int j = rightside_expectedGmParts.Length;
            for (int i = 0; i < j; ++i)
            {
                stbuilder.Append(rightside_expectedGmParts[i].ToString());
                if (i < j - 1)
                {
                    stbuilder.Append(' ');
                }
            }
            return stbuilder.ToString();
        }

        /// <summary>
        /// left hand nt_definition
        /// </summary>
        public NTDefinition LeftSideNT
        {
            get
            {
                return this.leftside_ntDefinition;
            }
        }

        public int SeqNumberOnLeftSideNt
        {
            get;
            private set;
        }

        internal void SetLeftSideNt(NTDefinition leftsideNt, int sqNum)
        {
            this.leftside_ntDefinition = leftsideNt;
            this.SeqNumberOnLeftSideNt = sqNum;

        }

        internal void SwitchToUserUnderlyingNtPresentation()
        {

            for (int i = rightside_expectedGmParts.Length - 1; i > -1; --i)
            {
                NTDefinition nt = rightside_expectedGmParts[i] as NTDefinition;
                if (nt != null)
                {

                    var unt = nt.UserNT;
                    if (unt != null)
                    {
                        if (unt.BaseOnUserNt != null)
                        {

                            NTDefinition replacingNT = unt.BaseOnUserNt.GenNT;
                            if (replacingNT != null)
                            {
                                rightside_expectedGmParts[i] = replacingNT;
                            }
                            else
                            {
                                throw new NotSupportedException();
                            }
                        }
                    }
                }
            }
        }

        internal bool FromBifurcation
        {
            get;
            set;
        }
        internal SeqSync[] SyncTokens
        {
            get { return syncSymbols; }
            set { syncSymbols = value; }
        }
        //---------
        //ParserKit  extension
        [System.Diagnostics.DebuggerNonUserCode]
        internal void NotifyEvent(ParseEventKind eventKind, ParseNodeHolder holder)
        {
            holder.CurrentContextSequence = this;
            user_sym_sq.Notify(eventKind, holder);
        }

        //ParserKit  extension
        [System.Diagnostics.DebuggerNonUserCode]
        internal void NotifyReduceEvent(ParseNodeHolder holder)
        {

            //if (_userSeqReductionDel != null)
            //{

            // reporter.ParseNodeHolder.CurrentContextSequence = this;
            holder.CurrentContextSequence = this;
            //ParseNodeHolder holder = reporter.ParseNodeHolder;
            //holder.SetParsingContextNt(reporter.Result);
            this._userSeqReductionDel(holder);

            //if (this.HasSomeUserExpectedSymbolMonitor)
            //{
            //    //create jit method for 
            //    // CreateJitInvokeMethod();
            //    ReductionMonitor[] rdMonitors = this.GetAllUserExpectedSymbolReductionMonitors();
            //    NonTerminalParseNode nt = (NonTerminalParseNode)holder.ContextOwner;


            //    //ParseNode c = nt.FirstChild;
            //    int childCount = holder.ContextChildCount;
            //    int currentAstIndex = holder.CurrentAstIndex - childCount;
            //    for (int i = 0; i < childCount; ++i)
            //    {
            //        ReductionMonitor m = rdMonitors[i];

            //        if (m != null)
            //        {
            //            //holder.ContextOwner = c;
            //            holder.CurrentChildAstIndex = currentAstIndex;
            //            m.NotifyReduction(holder);
            //        }
            //        currentAstIndex++;

            //    }

            //    //while (c != null)
            //    //{
            //    //    ReductionMonitor m = rdMonitors[i];
            //    //    if (m != null)
            //    //    {
            //    //        holder.ContextOwner = c;
            //    //        holder.CurrentChildAstIndex = currentAstIndex;
            //    //        m.NotifyReduction(holder);
            //    //    }
            //    //    //fill component 
            //    //    c = c.NextSibling;
            //    //    i++;
            //    //    currentAstIndex++;
            //    //}

            //    holder.ContextOwner = nt;
            //}

        }


        [System.Diagnostics.DebuggerNonUserCode]
        public void NotifyReductionEvent(ParseNodeHolder h)
        {
            this._userSeqReductionDel(h);
        }
        internal bool HasReductionListener;

        public UserExpectedSymbol GetOriginalUserExpectedSymbol(int realNodePos)
        {

            return user_sym_sq[realParseNodeToAbstractPosition[realNodePos]];
        }
        public ReductionMonitor GetOriginalUserExpectedSymbolReductionMonitor(int realNodePos)
        {
            return this.user_exp_monitors[realNodePos];
        }
        public ReductionMonitor[] GetAllUserExpectedSymbolReductionMonitors()
        {
            return this.user_exp_monitors;
        }

        public bool HasSomeUserExpectedSymbolMonitor
        {
            get;
            private set;
        }

    }



    struct SeqSync
    {
        public readonly SubParsers.SyncSymbolKind symbolKind;
        public readonly int position;
        public readonly ISymbolDefinition symbol;
        public readonly TokenDefinition[] tkset;
        public SeqSync(ISymbolDefinition symbol, int pos, SubParsers.SyncSymbolKind symbolKind)
        {
            this.symbol = symbol;
            this.position = pos;
            this.symbolKind = symbolKind;
            this.tkset = null;
        }
        public SeqSync(TokenDefinition[] tkset, int pos, SubParsers.SyncSymbolKind symbolKind)
        {
            this.symbol = null;
            this.position = pos;
            this.symbolKind = symbolKind;
            this.tkset = tkset;
        }
    }
}