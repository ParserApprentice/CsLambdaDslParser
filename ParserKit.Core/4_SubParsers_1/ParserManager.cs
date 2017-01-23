//MIT 2015-2017, ParserApprentice 
using System;
using System.Text;
using System.Collections.Generic;
using System.Reflection;

using Parser.ParserKit.LR;
using Parser.ParserKit.SubParsers;

namespace Parser.ParserKit
{
  
    public class ParserManager
    {
        Dictionary<string, SubParser> subParsers = new Dictionary<string, SubParser>();
        TokenInfoCollection tkInfoCollection;
        bool _useFastParseMode;
        bool _breakOnShift;
        bool _breakOnReduce;

        public ParserManager(TokenInfoCollection tkInfoCollection)
        {
            this.tkInfoCollection = tkInfoCollection;
        }

        public T Setup<T>(T subParser)
           where T : SubParser
        {

            subParser.TryUseTableDataCache = UseCache;//set use cache or not before call Setup()
            subParser.Setup(tkInfoCollection);
            subParsers.Add(subParser.RootNtName, subParser);
            subParser.SetParserSwitchHandler(ParserSwitchHandler);

            return subParser;
        }
        
        public bool UseCache
        {
            get;
            set;
        }
#if DEBUG
        static int dbugTotalCount = 0;
#endif
        void ParserSwitchHandler(ParserSwitchContext sw)
        {

            SwitchDetail swDetail = sw.SwitchDetail;
#if DEBUG
            dbugTotalCount++;
            //if (dbugTotalCount == 2)
            //{
            //}
            if (swDetail == null)
            {
                throw new NotSupportedException();
            }
#endif
            //find a proper parser 

            SwitchPair swPair = null;
            int choiceCount = swDetail.Count;
            SubParser selectedSubParser = null;
            switch (choiceCount)
            {
                case 0: throw new NotSupportedException();
                case 1:
                    {
                        //only one  
                        swPair = swDetail.GetSwPair(0);
                        selectedSubParser = swPair.resolvedSubParser;
                    }
                    break;
                default:
                    {
                        swPair = GetSubParser(swDetail, sw.Reader);
                        selectedSubParser = swPair.resolvedSubParser;
                    }
                    break;
            }

            //SwitchChoicesForRow dic = sw.LookFor.choices;
            //
            //if (sw.LookFor.fromResolvedSwitch && sw.LookFor.KnownParserNameIndex != 0)
            //{
            //    SwitchChoice selectedChoice = dic.GetChoiceByParserNameIndex(sw.LookFor.KnownParserNameIndex);
            //    if (selectedChoice == null || selectedChoice.resolvedSubParser == null)
            //    {
            //        throw new NotSupportedException();
            //        //no switch avaliable here 
            //        //return;
            //    }
            //    selectedSubParser = selectedChoice.resolvedSubParser;
            //}
            //else
            //{
            //    switch (dic.Count)
            //    {
            //        case 0:
            //            new NotSupportedException();
            //            break;
            //        case 1:
            //            {

            //                //only 1
            //                SwitchChoice selectedChoice = dic.GetChoiceByIndex(0);
            //                if (selectedChoice != null)
            //                {
            //                    selectedSubParser = selectedChoice.resolvedSubParser;
            //                    if (!selectedSubParser.StartWith(sw.Reader.CurrentToken))
            //                    {
            //                        return;
            //                    }
            //                }

            //            } break;
            //        default:
            //            {
            //                selectedSubParser = GetSubParser(dic.GetSwitchTable(), sw.Reader);
            //            } break;
            //    }
            //}
            ////-------------------------------------------

            //int beforeEnterIndex = sw.Reader.InputIndex;
            //if (selectedSubParser == null)
            //{
            //    //no other parser 
            //    throw new NotSupportedException();
            //}
            ////-------------------------------------------  
            //selectedSubParser.Parse(sw);
            //if (sw.SwitchBackParseResult.resultKind != ParseResultKind.Error)
            //{

            //}
            //else
            //{
            //    //switch back from error 
            //    throw new NotSupportedException();
            //}
#if DEBUG
            if (selectedSubParser == null)
            {
                throw new NotSupportedException();
            }
#endif
            selectedSubParser.Parse(sw);
            if (sw.SwitchBackParseResult.resultKind != ParseResultKind.Error)
            {
                sw.SwitchBackState = swPair.switchBackState;
            }
            else
            {
                //switch back from error 
                throw new NotSupportedException();
            }
        }

        public void PrepareSwitchLink()
        {
            PrepareSwitchLink(false);
        }
        public void PrepareSwitchLink(bool alsoClearIntermediateLRTable)
        {
            //--------------------------
            foreach (SubParser subParser in subParsers.Values)
            {
                //if (subParser.RootNtName == "_compilation_unit")
                //{
                //}
                //SyncParser syncParser = subParser.GetSyncParser();
                //if (syncParser != null)
                //{

                //}
                subParser.PrepareSwitchTable(subParsers);
            }
            foreach (SubParser subParser in subParsers.Values)
            {
                subParser.CompactTable(alsoClearIntermediateLRTable);
            }
        }

        public bool BreakOnShift
        {
            get { return this._breakOnShift; }
            set
            {
                this._breakOnShift = value;
                foreach (SubParser subParser in subParsers.Values)
                {
                    subParser.SetBreakOnShift(value);
                }
            }
        }
        public bool BreakOnReduce
        {
            get { return this._breakOnReduce; }
            set
            {
                this._breakOnReduce = value;
                foreach (SubParser subParser in subParsers.Values)
                {
                    subParser.SetEnableBreakOnReduce(value);
                }
            }
        }


        public bool UseFastParseMode
        {
            //default is parse dev mode
            get { return _useFastParseMode; }
            set
            {
                this._useFastParseMode = value;
                foreach (SubParser subParse in subParsers.Values)
                {
                    subParse.SetFastParseMode(value);
                }
            }
        }
#if DEBUG
        static int dbugGetSubParserCounter;
#endif

        static SubParser GetSubParser(Dictionary<TokenDefinition, SubParser> dic, TokenStreamReader reader)
        {
#if DEBUG
            dbugGetSubParserCounter++;
#endif

            int begin1 = reader.CurrentReadIndex;
            //int missing = 0;
            Token currentToken = reader.CurrentToken;
            do
            {
                SubParser subParser;
                if (dic.TryGetValue(currentToken.TkInfo, out subParser))
                {
                    //found 
                    reader.SetIndex(begin1);
                    return subParser;
                }
                currentToken = reader.ReadNext();
                //missing++;
            } while (currentToken != null);

            reader.SetIndex(begin1);
            return null;
        }

        SwitchPair GetSubParser(SwitchDetail swDetail, TokenStreamReader reader)
        {
#if DEBUG
            dbugGetSubParserCounter++;
#endif
            //find what parser is involved in this ...

            SwitchPair swPair = null;
            int begin1 = reader.CurrentReadIndex;
            //var a = reader.GetToken(begin1);
            int pos = begin1;
            if (begin1 >= (TokenStream.BUFF_LEN - 100))
            {
                reader.EnsureNext(TokenStream.BUFF_LEN);
                begin1 = 1;
                pos = begin1;

            }
            //int missing = 0;
            Token currentToken = reader.CurrentToken;
            Dictionary<TokenDefinition, SwitchPair> dic = swDetail.GetSwitchTable();

            do
            {

                if (dic.TryGetValue(currentToken.TkInfo, out swPair))
                {
                    //found 
                    reader.SetIndex(begin1);
                    return swPair;
                }
                //if (pos >= 1023)
                //{

                //}
                currentToken = reader.GetToken(pos++);
                //missing++;
            } while (currentToken != null);

            //reader.SetIndex(begin1);
            return null;
        }
    }

}