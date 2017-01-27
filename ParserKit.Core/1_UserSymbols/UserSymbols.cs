//MIT, 2015-2017, ParserApprentice
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

    public class TopUserNTDefinition : UserNTDefinition
    {
        internal TopUserNTDefinition()
        {
            //intened to be internal ctor
            //use need to create this type by calling top() method of reflection subparser
            //
        }
        public static TopUserNTDefinition operator *(TopUserNTDefinition topNt, NtDefAssignSet ntDefAssignSet)
        {
            ntDefAssignSet.AssignDataToNt(topNt);
            return topNt;
        }
    }
    public class UserNTDefinition : USymbol
    {

        List<UserSymbolSequence> originalSymbolSqs = new List<UserSymbolSequence>();
        List<UserSymbolSequence> allPossibleSequences = new List<UserSymbolSequence>();

#if DEBUG
        static int dbugTotalId = 0;
        public int dbugId;
#endif

        internal UserNTDefinition()
        {
            setupId();
            //we can set the name later
        }
        public UserNTDefinition(string ntname)
        {
            setupId();
            this.SetName(ntname);
        }

        public virtual List<UserNTDefinition> GetLateNts()
        {
            return this.lateCreatedUserNts;
        }
        public virtual int UserSeqCount
        {
            get { return originalSymbolSqs.Count; }
        }
        public virtual UserNTDefinition BaseOnUserNt
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

        public virtual int NTPrecedence
        {
            get;
            set;
        }
        /// <summary>
        /// for LR
        /// </summary>
        public virtual bool IsRightAssociative
        {
            get;
            set;
        }
        string _name;
        public virtual string Name
        {
            get { return _name; }
        }
        internal virtual void SetName(string name)
        {
            this._name = name;
        }
        public virtual IEnumerable<UserSymbolSequence> GetAllOriginalSeqIterForward()
        {
            int j = this.originalSymbolSqs.Count;
            for (int i = 0; i < j; ++i)
            {
                yield return originalSymbolSqs[i];
            }

        }
        public virtual IEnumerable<UserSymbolSequence> GetAllPossibleSeqIterForward()
        {
            int j = this.allPossibleSequences.Count;
            for (int i = 0; i < j; ++i)
            {
                yield return this.allPossibleSequences[i];
            }
        }
        public virtual bool IsAutoGen
        {
            get;
            set;
        }

        NTDefinition _genNT;
        public virtual NTDefinition GenNT
        {
            get { return this._genNT; }
            set
            {
                if (value == null)
                {
                }
                this._genNT = value;
            }
        }

        public virtual void AddSymbolSequence(UserSymbolSequence sq)
        {

            this.originalSymbolSqs.Add(sq);
            if (sq.Precedence == 0)
            {

                sq.Precedence = this.NTPrecedence;
            }
        }

        public virtual void CollectAllPossibleSequences()
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


        public virtual int SeqCount
        {
            get
            {
                return this.allPossibleSequences.Count;
            }
        }
        internal virtual bool MarkedAsUnknownNT
        {
            get;
            set;
        }
        internal virtual bool IsCompleteSelectSqs
        {
            get;
            set;
        }

        internal virtual void FilteroutDuplicateSequences()
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
        List<UserNTDefinition> lateCreatedUserNts;
        internal virtual void AddLateCreatedUserNt(UserNTDefinition lateUserNt)
        {
            if (lateCreatedUserNts == null)
            {
                lateCreatedUserNts = new List<UserNTDefinition>();
            }
            lateCreatedUserNts.Add(lateUserNt);
        }

        internal virtual object OwnerSubParser
        {
            get;
            set;
        }

        internal virtual bool IsOneOf
        {
            get;
            set;
        }

        internal virtual bool IsClosed
        {
            get;
            set;
        }

        //---------------
        public static UserNTDefinition CreateProxyUserNtDefinition(System.Reflection.FieldInfo fieldInfo, string name)
        {
            //this is special version of  UserNTDefinition
            return new ProxyUserNTDefinition(fieldInfo, name);
        }
        public static implicit operator UserNTDefinition(NtDefAssignSet assignSet)
        {
            var newNt = new UserNTDefinition();
            //the name of this nt will be set later
            assignSet.AssignDataToNt(newNt);
            return newNt;
        }

    }

    class ProxyUserNTDefinition : UserNTDefinition
    {
        System.Reflection.FieldInfo fieldInfo;
        UserNTDefinition unt;
        public ProxyUserNTDefinition(System.Reflection.FieldInfo fieldInfo, string name)
        {
            this.fieldInfo = fieldInfo;
            base.SetName(name);
        }
        public override string Name
        {
            get
            {
                if (unt != null)
                {
                    return unt.Name;
                }
                else
                {
                    return base.Name;
                }
            }
        }
        internal override void SetName(string name)
        {
            //should not visit here
            throw new NotSupportedException();
        }
        public void SetActualImplementation(UserNTDefinition unt)
        {
            if (unt == null)
            {
                throw new NotSupportedException();
            }
            if (this.unt != null)
            {
                throw new NotSupportedException();
            }
            if (unt is ProxyUserNTDefinition)
            {
                if (unt != this)
                {
                    throw new NotSupportedException();
                }
                else
                {
                    return;
                }
            }
            else
            {
                this.unt = unt;
            }
        }
        internal override void AddLateCreatedUserNt(UserNTDefinition lateUserNt)
        {
            unt.AddLateCreatedUserNt(lateUserNt);
        }
        public override UserNTDefinition BaseOnUserNt
        {
            get
            {
                if (unt != null)
                {
                    return unt.BaseOnUserNt;
                }
                else
                {
                    return base.BaseOnUserNt;
                }
            }
            set
            {
                unt.BaseOnUserNt = value;
            }
        }
        public override bool IsRightAssociative
        {
            get
            {
                return unt.IsRightAssociative;
            }
            set
            {
                unt.IsRightAssociative = value;
            }
        }
        public override int NTPrecedence
        {
            get
            {
                if (unt != null)
                {
                    return unt.NTPrecedence;
                }
                else
                {
                    return base.NTPrecedence;
                }
            }
            set
            {
                unt.NTPrecedence = value;
            }
        }
        public override int UserSeqCount
        {
            get
            {
                if (unt == null)
                {
                    return base.UserSeqCount;
                }
                else
                {
                    return unt.UserSeqCount;
                }
            }
        }
        public override List<UserNTDefinition> GetLateNts()
        {
            if (unt != null)
            {
                return unt.GetLateNts();
            }
            else
            {
                return base.GetLateNts();
            }
        }
        public override void AddSymbolSequence(UserSymbolSequence sq)
        {
            unt.AddSymbolSequence(sq);
        }
        public override void CollectAllPossibleSequences()
        {
            if (unt != null)
            {
                unt.CollectAllPossibleSequences();
            }
            else
            {
                base.CollectAllPossibleSequences();
            }
        }
        internal override void FilteroutDuplicateSequences()
        {
            if (unt != null)
            {
                unt.FilteroutDuplicateSequences();
            }
            else
            {
                base.FilteroutDuplicateSequences();
            }

        }
        public override NTDefinition GenNT
        {
            get
            {
                if (unt != null)
                {
                    return unt.GenNT;
                }
                else
                {
                    return base.GenNT;
                }
            }
            set
            {
                if (unt != null)
                {
                    unt.GenNT = value;
                }
                else
                {
                    base.GenNT = value;
                }
            }
        }
        public override IEnumerable<UserSymbolSequence> GetAllOriginalSeqIterForward()
        {
            if (unt != null)
            {
                return unt.GetAllOriginalSeqIterForward();
            }
            else
            {
                return base.GetAllOriginalSeqIterForward();
            }
        }
        public override IEnumerable<UserSymbolSequence> GetAllPossibleSeqIterForward()
        {
            if (unt != null)
            {
                return unt.GetAllPossibleSeqIterForward();
            }
            else
            {
                return base.GetAllPossibleSeqIterForward();
            }
        }


        public override bool IsAutoGen
        {
            get
            {
                if (unt == null)
                {
                    return base.IsAutoGen;
                }
                else
                {
                    return unt.IsAutoGen;
                }
            }
            set
            {
                unt.IsAutoGen = value;
            }
        }
        internal override bool IsClosed
        {
            get
            {
                return unt.IsClosed;
            }
            set
            {
                unt.IsClosed = value;
            }
        }
        internal override bool IsCompleteSelectSqs
        {
            get
            {
                if (unt != null)
                {
                    return unt.IsCompleteSelectSqs;
                }
                else
                {
                    return base.IsCompleteSelectSqs;
                }
            }
            set
            {
                if (unt != null)
                {
                    unt.IsCompleteSelectSqs = value;
                }
                else
                {
                    base.IsCompleteSelectSqs = value;
                }
            }
        }
        internal override bool IsOneOf
        {
            get
            {
                return unt.IsOneOf;
            }
            set
            {
                unt.IsOneOf = value;
            }
        }
        internal override bool MarkedAsUnknownNT
        {
            get
            {
                if (unt != null)
                {
                    return unt.MarkedAsUnknownNT;
                }
                else
                {
                    return base.MarkedAsUnknownNT;
                }
            }
            set
            {
                if (unt != null)
                {
                    unt.MarkedAsUnknownNT = value;
                }
                else
                {
                    base.MarkedAsUnknownNT = value;
                }

            }
        }

        internal override object OwnerSubParser
        {
            get
            {
                if (unt == null)
                {
                    return base.OwnerSubParser;
                }
                else
                {
                    return unt.OwnerSubParser;
                }
            }
            set
            {
                if (unt == null)
                {
                    base.OwnerSubParser = value;
                }
                else
                {
                    unt.OwnerSubParser = value;
                }
            }
        }
        public override int SeqCount
        {
            get
            {
                if (unt != null)
                {
                    return unt.SeqCount;
                }
                else
                {
                    return base.SeqCount;
                }
            }
        }
    }





    public class UserSymbolSequence
    {

        List<UserExpectedSymbol> rightside_expectedGmParts;
        ParserNotifyDel parserNotify;//shift, err etc
        SeqReductionDel seqReductionDel; //for reduction only 
        bool targetReductionDelIsLambda;

        bool isRightAssoc;
        UserNTDefinition ownerNT;
        bool isSetup;

#if DEBUG
        static int dbugTotalId = 0;
        public int dbugId;
#endif
        Action<object> lateSetupDel;
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
        public void SetLateSetupDel(Action<object> lateSetupDel)
        {
            this.lateSetupDel = lateSetupDel;
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
            EndOfFileSymbol.SymbolString = "$";
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
            this.SymbolString = userNTName;
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
            this.SymbolString = nt.Name;
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
            this.SymbolString = tokenInfo.PresentationString;
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
            this.SymbolString = tokenInfo.PresentationString;
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
        string _symbolString;
        public string SymbolString
        {
            get
            {
                if (_symbolString == null)
                {
                    if (this.resolvedUserNt != null)
                    {
                        return this.resolvedUserNt.Name;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return _symbolString;
                }
            }
            private set
            {
                _symbolString = value;
            }
        }

        public override string ToString()
        {
            return this.SymbolString;
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
            if (nt == null)
            {

            }
            if (this.resolvedUserNt != nt)
            {

            }

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

    /// <summary>
    /// non-terminal (Nt) definition assignment set
    /// </summary>
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
#if DEBUG
                if (symDel == null)
                {

                }
#endif
                object f_result = symDel(default(T));
#if DEBUG
                if (f_result == null)
                {
                    //can't be null

                }
#endif
                if (f_result is USymbol)
                {

                    USymbol usymbol = (USymbol)f_result;
                    var seqShiftDelMap = new SeqShiftDelMap(getBuilder, symDel);
                    var symbolWithStepInfo = new SubParsers.SymbolWithStepInfo(usymbol,
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

                if (this.Precedence > 0)
                {
                    int prec = this.Precedence;
                    for (int i = newss.RightCount - 1; i >= 0; --i)
                    {
                        UserExpectedSymbol ues = newss[i];
                        if (ues.SymbolKind == UserExpectedSymbolKind.Nonterminal)
                        {
                            UserNTDefinition unt2 = ues.ResolvedUserNtDef;
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



        /// <summary>
        /// set precedence
        /// </summary>
        /// <returns></returns>
        public NtDefAssignSet<T> set_prec(int value)
        {
            //TODO: review here again ***
            this.Precedence = value;

            return this;
        }
        public int Precedence { get; private set; }
    }




}