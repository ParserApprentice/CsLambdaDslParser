//MIT 2015-2017, ParserApprentice  
using System;
using System.Text;
using System.Collections.Generic;
using Parser.ParserKit.LR;

namespace Parser.ParserKit.SubParsers
{   
    public class SwitchDetail
    {
        List<SwitchPair> swPairs = new List<SwitchPair>();
#if DEBUG
        public readonly int dbugId = dbugTotalId++;
        static int dbugTotalId;
#endif
        public SwitchDetail(int number)
        {
            this.Number = number;
            //if (this.dbugId == 391)
            //{ 
            //}
        }
        public int Number
        {
            get;
            private set;
        }
        public SwitchPair GetSwPair(int index)
        {
            return swPairs[index];
        }
        public int Count
        {
            get
            {
                return swPairs.Count;
            }
        }
        public void AddChoice(ISymbolDefinition symbol, int swBackState)
        {
            swPairs.Add(new SwitchPair(symbol.SymbolName, swBackState));
        }
        public void AddChoice(SwitchPair pair)
        {
            swPairs.Add(pair);
        }
        public bool IsResolved
        {
            get;
            set;
        }

        //--------------------------------------------------------------------------
        Dictionary<TokenDefinition, SwitchPair> mergeDic;
        //--------------------------------------------------------------------------
        public void PrepareSyncTable()
        {
            mergeDic = new Dictionary<TokenDefinition, SwitchPair>();
            for (int i = swPairs.Count - 1; i >= 0; --i)
            {
                SwitchPair swPair = swPairs[i];
                SubParser subparser = swPair.resolvedSubParser;
                if (subparser == null)
                {
                    continue;
                }
                List<SubParsers.SyncSequence> synLists = subparser.GetSyncSeqs();
                if (synLists != null)
                {
                    int n = synLists.Count;
                    for (int m = 0; m < n; ++m)
                    {
                        SubParsers.SeqSyncCmd[] cmds = synLists[m].cmds;
                        //more than 1 cmd
                        //int p = cmds.Length;
                        SubParsers.SeqSyncCmd cmd = cmds[0];//get only first 

                        //TokenDefinition tk1 = cmd.tk1;
                        switch (cmd.cmdName)
                        {
                            case SubParsers.SyncCmdName.First:
                                {
                                    //if (cmd.tk1 == currentTokenInfo)
                                    //{
                                    //    reader.SetIndex(begin1);
                                    //    ReleaseFreeListForSwitchChoice(newlist);
                                    //    return subparser;
                                    //}
                                    //else
                                    //{
                                    //    newlist.RemoveAt(i);
                                    //}

                                    mergeDic.Add(cmd.tk1, swPair);

                                } break;
                            default:
                                {
                                    mergeDic.Add(cmd.tk1, swPair);
                                    //fisrt exist 
                                    //if not then read next
                                    //if (cmd.tk1 == currentTokenInfo)
                                    //{
                                    //    reader.SetIndex(begin1);
                                    //    ReleaseFreeListForSwitchChoice(newlist);
                                    //    return subparser;
                                    //}
                                } break;
                        }
                    }
                }
            }
        }
        public Dictionary<TokenDefinition, SwitchPair> GetSwitchTable()
        {

            return mergeDic;
        }
#if DEBUG
        public override string ToString()
        {
            StringBuilder stbuilder = new StringBuilder();
            stbuilder.Append("c=" + swPairs.Count);
            stbuilder.Append(' ');

            int j = swPairs.Count;
            for (int i = 0; i < j; ++i)
            {
                stbuilder.Append(swPairs[i].ToString());
                stbuilder.Append(',');
            }
            return stbuilder.ToString();
        }
#endif
    }

    public class SwitchPair
    {
        public readonly string symbolName;
        public readonly int switchBackState;

        //---------------------------------
        //late resolve sub parser ***
        public SubParser resolvedSubParser;

        public SwitchPair(string symbolName, int switchBackState)
        {
            this.switchBackState = switchBackState;
            this.symbolName = symbolName;
        }
        public override string ToString()
        {
            return symbolName + ":" + switchBackState;
        }

    }



}