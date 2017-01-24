//MIT, 2015-2017, ParserApprentice
using System;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using Parser.ParserKit.LR;

namespace Parser.ParserKit.SubParsers
{

    public delegate bool ShouldBreakOnLambda(AstWalker walker, string argName);

    public static class BreakMode
    {
        public static bool _letsEnterMethodBody = false;
        public static ShouldBreakOnLambda _shouldBreakOnLambda;
    }

     
    public static class SubParserCache
    {
        public static void SaveAsBinaryFile(SubParser subparser, string filename)
        {
            ParserDataBinaryCache.SaveToBinary(subparser.InternalParsingTable, filename);
        }
        public static void SaveAsTextFile(SubParser subparser, string filename)
        {
            LRTableReaderWriter.SaveAsTextFile(subparser.InternalParsingTable, filename);
        }
        public static void SaveAsHtmlFile(SubParser subparser, string filename)
        {
            LRTableReaderWriter.SaveAsHtmlFile(subparser.InternalParsingTable, filename);
        }
    }

    public class SymbolWithStepInfo
    {
        internal readonly object symbol;
        internal ReductionMonitor reductionDel;
        internal ParserNotifyDel shiftDel;

        //public SymbolWithStepInfo(object symbol, ReductionFillSubItem reductionDel)
        //{
        //    /*symbol must be one of (UserNTDefinition,TokenDefinition,OptSymbol,ListSymbol,OneOfSymbol)*/
        //    this.symbol = symbol;
        //    this.reductionDel = new ReductionMonitor(reductionDel);
        //}
        //public SymbolWithStepInfo(object symbol, SeqReductionBreakable reductionDel)
        //{
        //    this.symbol = symbol;
        //    this.reductionDel = new ReductionMonitor(reductionDel);
        //}
        //public SymbolWithStepInfo(object symbol, ReductionFillSubItemBreakable reductionDel)
        //{
        //    /*symbol must be one of (UserNTDefinition,TokenDefinition,OptSymbol,ListSymbol,OneOfSymbol)*/
        //    this.symbol = symbol;
        //    this.reductionDel = new ReductionMonitor(reductionDel);
        //}
        public SymbolWithStepInfo(object symbol, UserExpectedSymbolShift shiftDel)
        {
            /*symbol must be one of (UserNTDefinition,TokenDefinition,OptSymbol,ListSymbol,OneOfSymbol)*/
            this.symbol = symbol;
            var holder = new UserSymbolShiftHolder(shiftDel);
            this.shiftDel = holder.Invoke;
        }

        [System.Diagnostics.DebuggerNonUserCode]
        [System.Diagnostics.DebuggerStepThrough]
        class UserSymbolShiftHolder
        {
            UserExpectedSymbolShift uSymbolShiftDel;
            public UserSymbolShiftHolder(UserExpectedSymbolShift uSymbolShiftDel)
            {
                this.uSymbolShiftDel = uSymbolShiftDel;
            }
            public void Invoke(ParseNodeHolder report)
            {
                uSymbolShiftDel(report);
            }
        }
    }

    public class OptSymbol : USymbol
    {
        public readonly object ss;
        public OptSymbol(object ss)
        {
#if DEBUG
            if (ss == null)
            {
                throw new NotSupportedException();
            }
#endif
            this.ss = ss;
        }
    }
    public class ListSymbol : USymbol
    {
        public readonly object ss;
        public readonly object sep;
        public ListSymbol(object ss)
        {
#if DEBUG
            if (ss == null)
            {
                throw new NotSupportedException();
            }
#endif
            this.ss = ss;
            this.sep = null;
        }
        public ListSymbol(object ss, TokenDefinition sep)
        {
#if DEBUG
            if (ss == null || sep == null)
            {
                throw new NotSupportedException();
            }
#endif
            this.ss = ss;
            this.sep = sep;
        }
    }
    public class OneOfSymbol : USymbol
    {
        public readonly object[] symbols;
        public OneOfSymbol(object[] symbols)
        {
            this.symbols = symbols;
        }
    }


    public class MiniGrammarSheet : UserLangGrammarSheet
    {

        public void LoadTokenInfo(TokenInfoCollection tokenInfoCollection)
        {
            this.tokenInfoCollection = tokenInfoCollection;
        }
        public void LoadUserNts(List<UserNTDefinition> uNts)
        {
            foreach (UserNTDefinition unt in uNts)
            {
                this.allUserNTs.AddNT(unt);
            }
        }
        Dictionary<NTDefinition, NTDefinition> GetNtDic()
        {

            Dictionary<NTDefinition, NTDefinition> ntdic = new Dictionary<NTDefinition, NTDefinition>();
            foreach (NTDefinition nt in this.dicCoreNTs.Values)
            {
                ntdic.Add(nt, nt);
            }
            return ntdic;
        }
        public LRParsingTable CreateLR1Table(NTDefinition augmentedNT)
        {
            LRParsingTable parsingTable = new LRParsingTable(
                LRStyle.LR1,
                augmentedNT,
                this.GetNtDic(),
                this.symResolutionInfo,
                this.tokenInfoCollection);

            return parsingTable;
        }
        public LRParsingTable CreateLALR1Table(NTDefinition augmentedNT)
        {
            LRParsingTable parsingTable = new LRParsingTable(
                LRStyle.LALR,
                augmentedNT,
                this.GetNtDic(),
                this.symResolutionInfo,
                this.tokenInfoCollection);

            return parsingTable;
        }
        public LRParsingTable CreateLR0Table(NTDefinition augmentedNT)
        {
            LRParsingTable parsingTable = new LRParsingTable(
                LRStyle.LR0,
                augmentedNT,
                this.GetNtDic(),
                this.symResolutionInfo,
                this.tokenInfoCollection);

            return parsingTable;
        }
    }




}