//MIT 2015-2017, ParserApprentice 
using System;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;

namespace Parser.ParserKit
{

    public enum ParseEventKind
    {
        Shift,
        Reduce,
        Conflict,
        Error
    }

    public enum UserExpectedSymbolKind
    {
        UnknownNT,
        Terminal,
        Nonterminal,
        End
    }

    public class UserNTCollection
    {
        Dictionary<string, UserNTDefinition> dic = new Dictionary<string, UserNTDefinition>();
        public UserNTCollection()
        {
        }
        public void AddNT(UserNTDefinition uNT)
        {
            dic.Add(uNT.Name, uNT);
        }
        public int Count
        {
            get
            {
                return this.dic.Count;
            }
        }
        public UserNTDefinition Find(string name)
        {
            UserNTDefinition found;
            dic.TryGetValue(name, out found);
            return found;
        }
        public IEnumerable<UserNTDefinition> GetUserNTIterForward()
        {
            foreach (UserNTDefinition uNT in dic.Values)
            {
                yield return uNT;
            }
        }
    }


    /// <summary>
    /// user's symbol definition
    /// </summary>
    public abstract class USymbol
    {

    }

    public class UserNTDefinition : USymbol
    {

        List<UserSymbolSequence> originalSymbolSqs = new List<UserSymbolSequence>();
        List<UserSymbolSequence> allPossibleSequences = new List<UserSymbolSequence>();

#if DEBUG
        static int dbugTotalId = 0;
        public int dbugId;
#endif

        public UserNTDefinition()
        {
            setupId();
        }
        public UserNTDefinition(string ntname)
        {
            setupId();
            this.Name = ntname;
        }
        public int UserSeqCount
        {
            get { return originalSymbolSqs.Count; }
        }
        public UserNTDefinition BaseOnUserNt
        {
            get;
            set;
        }

        [System.Diagnostics.Conditional("DEBUG")]
        void setupId()
        {
#if DEBUG
            dbugId = dbugTotalId++;

#endif
        }

        public int NTPrecedence
        {
            get;
            set;
        }
        /// <summary>
        /// for LR
        /// </summary>
        public bool IsRightAssociative
        {
            get;
            set;
        }
        public string Name
        {
            get;
            private set;
        }

        public delegate void DefT<T>(T t);
        public T _1<T>(T t)
        {
            return t;
        }
        public T _1<T>(T t, DefT<T> t2)
        {
            return t;
        }
        public IEnumerable<UserSymbolSequence> GetAllOriginalSeqIterForward()
        {
            int j = this.originalSymbolSqs.Count;
            for (int i = 0; i < j; ++i)
            {
                yield return originalSymbolSqs[i];
            }

        }
        public IEnumerable<UserSymbolSequence> GetAllPossibleSeqIterForward()
        {
            int j = this.allPossibleSequences.Count;
            for (int i = 0; i < j; ++i)
            {
                yield return this.allPossibleSequences[i];
            }
        }
        public bool IsAutoGen
        {
            get;
            set;
        }
        public NTDefinition GenNT
        {
            get;
            set;
        }

        public void AddSymbolSequence(UserSymbolSequence sq)
        {

            this.originalSymbolSqs.Add(sq);
            if (sq.Precedence == 0)
            {

                sq.Precedence = this.NTPrecedence;
            }
        }

        public void CollectAllPossibleSequences()
        {
            if (this.IsCompleteSelectSqs)
            {
                return;
            }
            this.allPossibleSequences.AddRange(this.originalSymbolSqs);
            this.IsCompleteSelectSqs = true;
        }
        public override string ToString()
        {
            return this.Name;
        }


        public int SeqCount
        {
            get
            {
                return this.allPossibleSequences.Count;
            }
        }
        internal bool MarkedAsUnknownNT
        {
            get;
            set;
        }
        internal bool IsCompleteSelectSqs
        {
            get;
            set;
        }

        internal void FilteroutDuplicateSequences()
        {
            int j = this.allPossibleSequences.Count;

            //separate sq into groups if it has common
            Dictionary<UserSymbolSequence, int> dic = new Dictionary<UserSymbolSequence, int>();
            List<UserSymbolSequence> filter1 = new List<UserSymbolSequence>();
            for (int i = 0; i < j; ++i)
            {
                if (!dic.ContainsKey(allPossibleSequences[i]))
                {
                    filter1.Add(allPossibleSequences[i]);
                    dic.Add(allPossibleSequences[i], i);
                }
            }
        }
        internal object OwnerSubParser
        {
            get;
            set;
        }
        public static UserNTDefinition operator +(UserNTDefinition unt, NtDefAssignSet assignSet)
        {
            assignSet.AssignDataToNt(unt);
            return unt;
        }

        internal bool IsOneOf { get; set; }
        internal bool IsClosed { get; set; }
    }



    public class UserSymbolSequence
    {

        List<UserExpectedSymbol> rightside_expectedGmParts;
        ParserNotifyDel parserNotify;//shift, err etc
        SeqReductionDel seqReductionDel; //for reduction only 
        bool targetReductionDelIsLambda;

        bool isRightAssoc;
        UserNTDefinition ownerNT;
#if DEBUG
        static int dbugTotalId = 0;
        public int dbugId;
#endif

        public UserSymbolSequence(UserNTDefinition ownerNT)
        {
            setupId();
            this.ownerNT = ownerNT;
            this.rightside_expectedGmParts = new List<UserExpectedSymbol>();
        }
        public UserSymbolSequence(UserNTDefinition ownerNT, UserExpectedSymbol s)
        {
            setupId();
            this.ownerNT = ownerNT;
            this.rightside_expectedGmParts = new List<UserExpectedSymbol>() { s };
        }

        [System.Diagnostics.Conditional("DEBUG")]
        void setupId()
        {
#if DEBUG
            dbugId = dbugTotalId++;
#endif
        }
        public bool CreatedFromListOfNt
        {
            get;
            set;
        }

        public int Precedence
        {
            get;
            set;
        }
        public bool IsRightAssociative
        {
            get
            {
                return this.isRightAssoc;
            }
            set
            {
                this.isRightAssoc = value;
                if (value)
                {
                    int j = rightside_expectedGmParts.Count;
                    for (int i = 0; i < j; ++i)
                    {
                        rightside_expectedGmParts[i].IsRightAssoc = true;
                    }
                }
            }
        }

        public void AppendLast(UserExpectedSymbol symbol)
        {
            this.rightside_expectedGmParts.Add(symbol);
        }
        public UserExpectedSymbol this[int index]
        {
            get
            {
                return this.rightside_expectedGmParts[index];
            }
        }
        public int RightCount
        {
            get
            {
                return this.rightside_expectedGmParts.Count;
            }
        }
        public override string ToString()
        {
            StringBuilder stbuilder = new StringBuilder();
            int j = rightside_expectedGmParts.Count;
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
        public List<UserExpectedSymbol> GetRightSideSymbols()
        {
            return this.rightside_expectedGmParts;
        }
        //----------------------------------------------------------------- 
        public void SetParserNotifyDel(ParseEventKind parserEventKind, ParserNotifyDel del)
        {
            switch (parserEventKind)
            {
                case ParseEventKind.Reduce:
                    throw new NotSupportedException("please use SetParserReductionNotifyDel");

                default:
                    //set to all user expected symbol
                    this.parserNotify = del;
                    break;
            }
        }
        public void SetParserReductionNotifyDel(SeqReductionDel seqReductionDel)
        {
            this.seqReductionDel = seqReductionDel;
            this.targetReductionDelIsLambda = seqReductionDel.Method.Name.Contains("<");
        }
        public void ClearParserReductionNotifyDel()
        {
            this.seqReductionDel = null;
            this.targetReductionDelIsLambda = false;
        }
        internal SeqReductionDel GetUserSeqReductionDel()
        {
            return seqReductionDel;
        }
        [System.Diagnostics.DebuggerNonUserCode]
        [System.Diagnostics.DebuggerStepThrough]
        internal void Notify(ParseEventKind eventKind, ParseNodeHolder holder)
        {
            switch (eventKind)
            {
                case ParseEventKind.Reduce:
                    {
                        // bool parsingPhase = (reporter != null); //for parsing notify break point only *** 
                        if (this.seqReductionDel != null)
                        {
                            //reporter.ParseNodeHolder.SetParsingContextNt(reporter.Result);
                            this.seqReductionDel(holder);
                        }
                    }
                    break;
                default:
                    {
                        //eg shift, reduce, err 
                        if (this.parserNotify != null)
                        {
                            this.parserNotify(holder);
                        }
                    }
                    break;
            }

        }

        public string SeqName
        {
            get;
            set;
        }



        internal string GetNewLeftSideSymbolInfo()
        {
            return this.ownerNT.ToString();
        }


    }

    public class TopUserNtDefinition : UserNTDefinition
    {
        public TopUserNtDefinition(string name)
            : base(name)
        {
        }
        public static TopUserNtDefinition operator +(TopUserNtDefinition unt, NtDefAssignSet assignSet)
        {
            assignSet.AssignDataToNt(unt);
            return unt;
        }
    }


    public class UserExpectedSymbol
    {
        UserNTDefinition resolvedUserNt;
        TokenDefinition tokenInfo;
        ReductionMonitor reductionMonitor;

        public static readonly UserExpectedSymbol EndOfFileSymbol;
        public readonly ParserNotifyDel onStepDel;


        UserExpectedSymbolKind essSymbolKind;

#if DEBUG
        static int dbugTotalId;
        public int dbugId;
#endif
        static UserExpectedSymbol()
        {

            //-----------------------------------------------------
            EndOfFileSymbol = new UserExpectedSymbol();
            EndOfFileSymbol.SymbolKind = UserExpectedSymbolKind.End;
            EndOfFileSymbol.SymbolSting = "$";
            //-----------------------------------------------------
        }

        [Conditional("DEBUG")]
        void dbugSetupDebugId()
        {
#if DEBUG
            dbugId = dbugTotalId++;
            //if (dbugId == 31)
            //{
            //}
#endif
        }

        private UserExpectedSymbol()
        {
            dbugSetupDebugId();
        }


        public UserExpectedSymbol(string userNTName)
        {
            dbugSetupDebugId();
            this.SymbolKind = UserExpectedSymbolKind.UnknownNT;
            this.SymbolSting = userNTName;
        }
        public UserExpectedSymbol(string userNTName, bool isOptional)
            : this(userNTName)
        {
            this.IsOptional = isOptional;
        }
        //------------------------------------------------------------------
        public UserExpectedSymbol(UserNTDefinition nt)
        {
            dbugSetupDebugId();

            if (nt == null)
            {
                throw new NotSupportedException();
            }
            this.resolvedUserNt = nt;
            this.SymbolSting = nt.Name;
            this.SymbolKind = UserExpectedSymbolKind.Nonterminal;
            this.IsAuto = nt.IsAutoGen;
        }
        //------------------------------------------------------------------
        public UserExpectedSymbol(UserNTDefinition nt, bool isOptional)
            : this(nt)
        {
            this.IsOptional = isOptional;
        }
        public UserExpectedSymbol(TokenDefinition tokenInfo, bool isOptional)
        {
            dbugSetupDebugId();
            this.tokenInfo = tokenInfo;
            this.SymbolKind = UserExpectedSymbolKind.Terminal;
            this.IsOptional = isOptional;
            this.SymbolSting = tokenInfo.PresentationString;
        }
        //------------------------------------------------------------------
        public UserExpectedSymbol(UserNTDefinition nt, bool isOptional, ParserNotifyDel onShiftDel)
            : this(nt)
        {
            this.IsOptional = isOptional;
            this.onStepDel = onShiftDel;
        }
        public UserExpectedSymbol(TokenDefinition tokenInfo, bool isOptional, ParserNotifyDel onShiftDel)
        {
            dbugSetupDebugId();
            this.tokenInfo = tokenInfo;
            this.SymbolKind = UserExpectedSymbolKind.Terminal;
            this.IsOptional = isOptional;
            this.SymbolSting = tokenInfo.PresentationString;
            this.onStepDel = onShiftDel;
        }
        //------------------------------------------------------------------
        public UserExpectedSymbolKind SymbolKind
        {
            get
            {
                switch (essSymbolKind)
                {
                    case UserExpectedSymbolKind.Nonterminal:
                        {
                            if (this.resolvedUserNt.MarkedAsUnknownNT)
                            {
                                return UserExpectedSymbolKind.UnknownNT;
                            }

                        }
                        break;
                }
                return essSymbolKind;
            }
            set
            {
                essSymbolKind = value;
            }
        }
        public bool IsAuto
        {
            get;
            private set;
        }
        public bool IsOptional
        {
            get;
            set;
        }
        public string SymbolSting
        {
            get;
            private set;
        }

        public override string ToString()
        {
            return this.SymbolSting;
        }

        internal TokenDefinition ResolvedTokenDefinition
        {
            get
            {
                return this.tokenInfo;
            }
        }
        internal NTDefinition ResolvedNt
        {
            get
            {
                if (this.resolvedUserNt != null)
                {
                    return this.resolvedUserNt.GenNT;
                }
                return null;
            }
        }
        internal bool IsRightAssoc
        {
            get
            {
                if (this.resolvedUserNt != null)
                {
                    return resolvedUserNt.IsRightAssociative;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                if (this.resolvedUserNt != null)
                {
                    this.resolvedUserNt.IsRightAssociative = value;
                }
            }

        }
        public UserNTDefinition ResolvedUserNtDef
        {
            get { return this.resolvedUserNt; }
        }
        internal void SetResolveNT(UserNTDefinition nt)
        {
            if (this.SymbolKind == UserExpectedSymbolKind.UnknownNT)
            {
                this.SymbolKind = UserExpectedSymbolKind.Nonterminal;
                this.resolvedUserNt = nt;
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        internal ReductionMonitor ReductionDel
        {
            get { return reductionMonitor; }
            set
            {
                reductionMonitor = value;
            }
        }
    }


   



    public delegate object UserExpectedSymbolDef<T>(T r);
    public abstract class NtDefAssignSet
    {
        internal abstract void AssignDataToNt(UserNTDefinition unt);
    }

    public class NtDefAssignSet<T> : NtDefAssignSet
    {
        GetWalkerDel<T> getBuilder;
        //------------------------------
        //simple seq
        ParserKit.SubParsers.UserExpectedSymbolShift symbolShiftDel;
        BuilderDel3<T> reductionDel;
        object[] symbols;
        //------------------------------
        NtDefAssignSet<T>[] subAssignSets;

        public NtDefAssignSet(
            GetWalkerDel<T> getBuilder,
            ParserKit.SubParsers.UserExpectedSymbolShift symbolShiftDel,
            BuilderDel3<T> reductionDel,
            UserExpectedSymbolDef<T>[] symbols)
        {
            this.getBuilder = getBuilder;
            this.symbolShiftDel = symbolShiftDel;
            this.reductionDel = reductionDel;


            int j = symbols.Length;
            List<object> firstLevelSymbols = new List<object>();
            for (int i = 0; i < j; ++i)
            {
                UserExpectedSymbolDef<T> symDel = symbols[i];
                object f_result = symbols[i](default(T));
                if (f_result is USymbol)
                {

                    USymbol usymbol = (USymbol)f_result;
                    SeqShiftDelMap seqShiftDelMap = new SeqShiftDelMap(getBuilder, symDel);
                    SubParsers.SymbolWithStepInfo symbolWithStepInfo = new SubParsers.SymbolWithStepInfo(usymbol,
                        new SubParsers.UserExpectedSymbolShift(seqShiftDelMap.Invoke));
                    firstLevelSymbols.Add(symbolWithStepInfo);

                }
                else
                {



                }
            }

            this.symbols = firstLevelSymbols.ToArray();

        }

        public NtDefAssignSet(GetWalkerDel<T> getBuilder, NtDefAssignSet<T>[] subAssignSets)
        {
            this.getBuilder = getBuilder;
            this.subAssignSets = subAssignSets;
        }


        internal override void AssignDataToNt(UserNTDefinition unt)
        {
            if (subAssignSets != null)
            {
                //more than 1 seq
                if (unt.IsClosed)
                {
                    throw new NotSupportedException();
                }

                unt.IsOneOf = true;
                int j = subAssignSets.Length;
                for (int i = 0; i < j; ++i)
                {
                    subAssignSets[i].AssignDataToNt(unt);
                }
                unt.IsClosed = true;
            }
            else
            {
                if (unt.IsClosed)
                {
                    throw new NotSupportedException();
                }

                UserSymbolSequence newss = UserNTSubParserExtension.CreateUserSymbolSeq(unt, symbols);
                if (symbolShiftDel != null)
                {
                    ShiftMap shMap = new ShiftMap(symbolShiftDel);
                    newss.SetParserNotifyDel(ParseEventKind.Shift, shMap.Invoke);
                }
                if (reductionDel != null)
                {
                    var map = new SeqReductionDelMap(getBuilder, reductionDel);
                    newss.SetParserReductionNotifyDel(map.Invoke);
                }

                if (!unt.IsOneOf)
                {
                    unt.IsClosed = true;
                }
            }
        }
        class ShiftMap
        {
            SubParsers.UserExpectedSymbolShift shiftDel;
            public ShiftMap(SubParsers.UserExpectedSymbolShift shiftDel)
            {
                this.shiftDel = shiftDel;
            }
            public void Invoke(ParseNodeHolder h)
            {
                shiftDel(h);
            }
        }

        //--------------------------------------------
        [System.Diagnostics.DebuggerNonUserCode]
        [System.Diagnostics.DebuggerStepThrough]
        class SeqReductionDelMap
        {

            ParserKit.SubParsers.ReductionDef subItemFill;
            BuilderDel3<T> builderDel;
            GetWalkerDel<T> getBuilder;
            int cacheHolderId = 0;
            public SeqReductionDelMap(GetWalkerDel<T> getBuilder, BuilderDel3<T> builderDel)
            {
                this.getBuilder = getBuilder;
                this.builderDel = builderDel;
            }
            public void Invoke(ParseNodeHolder pnHolder)
            {
                if (cacheHolderId != pnHolder.parseNodeHolderId)
                {
                    //create new
                    cacheHolderId = pnHolder.parseNodeHolderId;
                    subItemFill = builderDel(getBuilder(pnHolder));
                }
                subItemFill();
            }
        }
        [System.Diagnostics.DebuggerNonUserCode]
        [System.Diagnostics.DebuggerStepThrough]
        class SeqShiftDelMap
        {

            UserExpectedSymbolDef<T> builderDel;
            GetWalkerDel<T> getBuilder;
            int cacheHolderId = 0;
            T cachedBuilder;
            public SeqShiftDelMap(GetWalkerDel<T> getBuilder, UserExpectedSymbolDef<T> builderDel)
            {
                this.getBuilder = getBuilder;
                this.builderDel = builderDel;
            }
            public object Invoke(ParseNodeHolder pnHolder)
            {
                if (cacheHolderId != pnHolder.parseNodeHolderId)
                {
                    //create new
                    cacheHolderId = pnHolder.parseNodeHolderId;
                    cachedBuilder = getBuilder(pnHolder);
                }
                return (object)builderDel(cachedBuilder);
            }
        }
    }




}