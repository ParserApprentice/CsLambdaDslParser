//MIT 2015-2017, ParserApprentice 
using System;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using Parser.ParserKit.LR;

namespace Parser.ParserKit.SubParsers
{

    enum SyncCmdName
    {
        Unknown,
        Match,
        BeginSkip,
        EndSkip,
        First

    }
    struct SeqSyncCmd
    {
        public readonly SyncCmdName cmdName;
        public readonly TokenDefinition tk1;
        public readonly TokenDefinition tk2;
        public SeqSyncCmd(SyncCmdName cmdName, TokenDefinition tk1)
        {
            this.cmdName = cmdName;
            this.tk1 = tk1;
            this.tk2 = null;
        }
        public SeqSyncCmd(SyncCmdName cmdName, TokenDefinition tk1, TokenDefinition tk2)
        {
            this.cmdName = cmdName;
            this.tk1 = tk1;
            this.tk2 = tk2;
        }
    }
    struct SyncSequence
    {
        internal readonly SeqSyncCmd[] cmds;
        public SyncSequence(SeqSyncCmd[] cmds)
        {
            this.cmds = cmds;
        }
    }
    enum SyncSymbolKind
    {
        Unknown,
        Shift,
        SwitchBack,
        Ignor,
        First,
        Follow
    }
    class SyncSymbol
    {
        internal readonly SyncSymbolKind kind;
        internal readonly TokenDefinition tk1;
        internal readonly TokenDefinition tk2;
        internal SyncSymbol(TokenDefinition tk1, SyncSymbolKind kind)
        {
            this.tk1 = tk1;
            this.kind = kind;
        }
        internal SyncSymbol(TokenDefinition tk1, TokenDefinition tk2, SyncSymbolKind kind)
        {
            this.tk1 = tk1;
            this.tk2 = tk2;
            this.kind = kind;
        }
    }

    //public class SyncParser
    //{
    //    TokenInfoCollection tkInfoCollection;
    //    List<UserNTDefinition> _syncNts;
    //    MiniGrammarSheet _miniGrammarSheet;
    //    NTDefinition _augmentedNTDefinition;

    //    LRParsingTable _parsingTable;
    //    LRParser _actualLRParser;

    //    internal SyncParser() { }
    //    internal void Setup(TokenInfoCollection tkInfoCollection, List<UserNTDefinition> _syncNts)
    //    {
    //        this.tkInfoCollection = tkInfoCollection;
    //        this._syncNts = _syncNts;
    //        //---------------------------------------
    //        _miniGrammarSheet = new MiniGrammarSheet();
    //        _miniGrammarSheet.LoadTokenInfo(tkInfoCollection);
    //        _miniGrammarSheet.LoadUserNts(_syncNts);
    //        //--------------------------------------- 
    //        _augmentedNTDefinition = _miniGrammarSheet.PrepareUserGrammarForAnyLR(_syncNts[0]);
    //        _parsingTable = _miniGrammarSheet.CreateLR1Table(_augmentedNTDefinition);
    //        _parsingTable.MakeParsingTable();
    //        _actualLRParser = LRParsing.CreateRunner(_parsingTable);
    //    }
    //    public LRParsingTable ParsingTable { get { return _parsingTable; } }

    //}


}