//MIT, 2015-2017, ParserApprentice
using System;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;

namespace Parser.ParserKit
{
    public abstract class UserLangGrammarSheet
    {

        protected SymbolResolutionInfo symResolutionInfo = new SymbolResolutionInfo();
        protected UserNTCollection allUserNTs = new UserNTCollection();
        //------------------------------------------------------ 
        protected Dictionary<string, NTDefinition> dicCoreNTs = new Dictionary<string, NTDefinition>();
        //------------------------------------------------------ 

        protected TokenInfoCollection tokenInfoCollection = new TokenInfoCollection();
        Dictionary<string, NTDefinition> nts = new Dictionary<string, NTDefinition>();


        public UserLangGrammarSheet()
        {
        }
        static void ResolveUserNTDefs(UserNTCollection tempList)
        {

            foreach (UserNTDefinition nt in tempList.GetUserNTIterForward())
            {

                foreach (UserSymbolSequence sq in nt.GetAllOriginalSeqIterForward())
                {

                    int symbolCount = sq.RightCount;
                    for (int a = 0; a < symbolCount; ++a)
                    {

                        UserExpectedSymbol s = sq[a];
                        if (s.SymbolKind == UserExpectedSymbolKind.UnknownNT)
                        {
                            UserNTDefinition foundNT = tempList.Find(s.SymbolString);
                            if (foundNT != null)
                            {
                                s.SetResolveNT(foundNT);
                            }
                            else
                            {

                            }
                        }
                        else
                        {
                            if (s.ResolvedNt == null)
                            {
                            }
                        }
                    }
                }

            }
        }

        NTDefinition CreateNTDefinition(UserNTDefinition userNT)
        {
            NTDefinition existing;
            if (this.nts.TryGetValue(userNT.Name, out existing))
            {
                throw new NotSupportedException();
            }

            NTDefinition nt = userNT.MarkedAsUnknownNT ?
                symResolutionInfo.CreateUnknownNT(userNT.Name) :
                new NTDefinition(userNT.Name, NTDefintionKind.NormalNT);


            if (userNT.NTPrecedence > 0)
            {
                symResolutionInfo.AddSymbolPrecedenceAndAssoc(nt,
                  userNT.NTPrecedence, userNT.IsRightAssociative);
            }

            //-----------------------------------------------------------------------------               
            nts.Add(nt.Name, nt);
            nt.UserNT = userNT;
            userNT.GenNT = nt;
            //---------------------------------------------------------------------------

            return nt;
        }
        static void CreateBodySequence(NTDefinition nt, SymbolResolutionInfo symbolResInfo)
        {

            //--------------------------------------------
            UserNTDefinition userNT = nt.UserNT;
            if (userNT.SeqCount == 0)
            {
                //user nt with seq ==0 => empty epsilon, except when it is unknown nt
                if (nt.NTKind == NTDefintionKind.UnknownNT)
                {
                    nt.HasEmptyEpsilonForm = false;
                }
                else
                {
                    nt.HasEmptyEpsilonForm = true;
                }
                nt.LoadSequences(new SymbolSequence[0]);
            }
            else
            {

                //single user seq  may be produce more then 1 seq
                //eg. when it containts 'optional' symbol -> it will
                //produce more than 1 seq
                int userSqCount = userNT.SeqCount;
                List<SymbolSequence> newSeqs = new List<SymbolSequence>(userNT.SeqCount);
                foreach (UserSymbolSequence ss in userNT.GetAllPossibleSeqIterForward())
                {
                    //recursive
                    CreateSymbolSequence(ss, newSeqs, symbolResInfo);
                }


                nt.LoadSequences(newSeqs.ToArray());
            }
        }

        class SeqLine
        {
            List<int> positions;
            int maxNumber = 0;
            public SeqLine()
            {
                this.positions = new List<int>();
            }
            public SeqLine(SeqLine originalSource)
            {
                this.positions = new List<int>(originalSource.positions);
                this.maxNumber = originalSource.maxNumber;
            }
            public void AddEmpty()
            {
                this.positions.Add(-1);
            }
            public void AddPosition()
            {
                this.positions.Add(this.maxNumber++);
            }
            public int[] ToArray()
            {
                return this.positions.ToArray();
            }
        }
        class PreparingSeqs
        {
            List<ISymbolDefinition> defaultSeq;
            List<List<ISymbolDefinition>> moreSeqs;
            SeqLine defaultSeqPositions;
            List<SeqLine> moreSeqPositions;

            bool hasMoreThanOne;
#if DEBUG
            int dbugId;
            static int dbugTotalId;
#endif
            public PreparingSeqs()
            {
                defaultSeq = new List<ISymbolDefinition>();
                defaultSeqPositions = new SeqLine();
                moreSeqs = null;
                hasMoreThanOne = false;
#if DEBUG
                this.dbugId = dbugTotalId++;
#endif
            }
            public void AddSymbol(ISymbolDefinition symbol, int originalPosition)
            {
                if (symbol == null)
                {

                }

                if (!hasMoreThanOne)
                {
                    defaultSeq.Add(symbol);
                    defaultSeqPositions.AddPosition();
                }
                else
                {

                    for (int i = moreSeqs.Count - 1; i > -1; --i)
                    {
                        moreSeqs[i].Add(symbol);
                        moreSeqPositions[i].AddPosition();
                    }
                }
            }
            public void Bifurcate(ISymbolDefinition sym1, int originalPos)
            {
                if (!hasMoreThanOne)
                {

                    moreSeqs = new List<List<ISymbolDefinition>>();
                    moreSeqPositions = new List<SeqLine>();
                    //move to more seq
                    moreSeqs.Add(defaultSeq);
                    moreSeqPositions.Add(defaultSeqPositions);
                    defaultSeq = null;
                    defaultSeqPositions = null;
                    hasMoreThanOne = true;
                }
                //-------------------------------
                //copy it into two set
                int j = moreSeqs.Count;
                for (int i = 0; i < j; ++i)
                {
                    List<ISymbolDefinition> list1 = moreSeqs[i];
                    SeqLine list1_positions = moreSeqPositions[i];

                    List<ISymbolDefinition> list2 = new List<ISymbolDefinition>(list1);
                    SeqLine list2_positions = new SeqLine(list1_positions);

                    if (sym1 != null)
                    {
                        list1.Add(sym1);
                        list1_positions.AddPosition();
                        list2_positions.AddEmpty();
                    }

                    this.moreSeqs.Add(list2);
                    this.moreSeqPositions.Add(list2_positions);
                }
                //-------------------------------

            }

#if DEBUG
            static int dbug_step = 0;
#endif
            public void GenerateSymbolSequences(UserSymbolSequence user_sequence, bool underConflict, List<SymbolSequence> output)
            {
                if (!hasMoreThanOne)
                {
                    var ss = new SymbolSequence(
                        defaultSeq.ToArray(),
                        defaultSeqPositions.ToArray(),
                        user_sequence);
                    ss.CreatedUnderConflict = underConflict;
                    output.Add(ss);

                }
                else
                {

#if DEBUG
                    dbug_step++;
#endif
                    int j = moreSeqs.Count;
                    for (int i = 0; i < j; ++i)
                    {
                        ISymbolDefinition[] symDefs = this.moreSeqs[i].ToArray();
                        if (symDefs.Length > 0)
                        {
                            var ss = new SymbolSequence(
                                symDefs,
                                this.moreSeqPositions[i].ToArray(),
                                user_sequence);
                            ss.CreatedUnderConflict = underConflict;
                            ss.FromBifurcation = true;
                            output.Add(ss);
                        }
                    }
                }
            }
        }

        /// <summary>         
        /// convert user symbol sequence to real sq
        /// </summary>
        /// <returns></returns>
        static void CreateSymbolSequence(UserSymbolSequence user_sequence, List<SymbolSequence> outputSeqs, SymbolResolutionInfo symbolResInfo)
        {
            List<UserExpectedSymbol> rightSideSymbols = user_sequence.GetRightSideSymbols();

            //convert symbols in each seq to expected symbol
            int symCount = rightSideSymbols.Count;
            if (symCount == 0)
            {
                throw new NotSupportedException();
            }


            PreparingSeqs preSeqs = new PreparingSeqs();
            for (int pos = 0; pos < symCount; ++pos)
            {
                //--------------------  

                //before we create 'body', we must know all symbol
                UserExpectedSymbol userExpectedSymbol = rightSideSymbols[pos];
                switch (userExpectedSymbol.SymbolKind)
                {
                    case UserExpectedSymbolKind.Terminal:
                        {


                            if (userExpectedSymbol.IsOptional)
                            {
                                preSeqs.Bifurcate(userExpectedSymbol.ResolvedTokenDefinition, pos);
                            }
                            else
                            {
                                preSeqs.AddSymbol(userExpectedSymbol.ResolvedTokenDefinition, pos);
                            }
                        } break;
                    case UserExpectedSymbolKind.Nonterminal:
                        {
                            NTDefinition ntSymbol = userExpectedSymbol.ResolvedNt;
                            if (userExpectedSymbol.IsOptional)
                            {
                                preSeqs.Bifurcate(ntSymbol, pos);
                            }
                            else
                            {
                                preSeqs.AddSymbol(ntSymbol, pos);
                            }
                        } break;
                    case UserExpectedSymbolKind.End:
                        {
                            preSeqs.AddSymbol(TokenDefinition._eof, pos);

                        } break;
                    case UserExpectedSymbolKind.UnknownNT:
                        {

                            //report found unknown nt                  
                            //and user should provide how to manage it

                            string leftsideNTName2 = user_sequence.GetNewLeftSideSymbolInfo();
                            //create jump symbol here!
                            //handle unknown
                            symbolResInfo.AddNtSequenceResolveMessage(
                                "create_seq: " + leftsideNTName2 + " found unknown nt " + userExpectedSymbol.ToString());


                            NTDefinition unknownNTSymbol = symbolResInfo.CreateUnknownNT(userExpectedSymbol.SymbolString);
                            if (userExpectedSymbol.IsOptional)
                            {
                                preSeqs.Bifurcate(unknownNTSymbol, pos);
                            }
                            else
                            {
                                preSeqs.AddSymbol(unknownNTSymbol, pos);
                            }




                        } break;
                    default:
                        throw new NotSupportedException();
                }
            }

            preSeqs.GenerateSymbolSequences(user_sequence, false, outputSeqs);

        }
        static void MakeFollowSet(NTDefinition nt, Dictionary<NTDefinition, WaitingFollow> dicNeedRevisit)
        {

            //if (nt.dbugId == 3 || nt.dbugId == 5)
            //{

            //}
            //see about 'first and follow' the Dragon Book page 221
            //each nt contains sequence(s)
            //then find and store set of follow of the nt
            switch (nt.NTKind)
            {
                case NTDefintionKind.RootStartSymbol:
                    nt.AddPossibleFollowerToken(TokenDefinition._eof, null);
                    break;
                case NTDefintionKind.UnknownNT:
                    break;
                default:
                    break;
            }



            //analyze content of user ...
            int sqcount = nt.SeqCount;
            for (int i = 0; i < sqcount; ++i)
            {

                SymbolSequence ss = nt.GetSequence(i);
                int symbolLim = ss.RightSideCount - 1;

                for (int s = 0; s < symbolLim; ++s)
                {

                    ISymbolDefinition thisSymbol = ss.GetExpectedSymbol(s);
                    ISymbolDefinition nextSymbol = ss.GetExpectedSymbol(s + 1);

                    if (thisSymbol.IsNT)
                    {

                        NTDefinition thisNT = (NTDefinition)thisSymbol;

                    TEST_NEXT_TOKEN:
                        if (nextSymbol.IsNT)
                        {
                            NTDefinition nextSymbolNT = (NTDefinition)nextSymbol;


                            //*** first token empty epsilon check 
                            bool foundEmptyEpsilon;
                            thisNT.AddPossibleFollowerTokensExceptEmptyEpsilon(nextSymbolNT, out foundEmptyEpsilon);

                            if (foundEmptyEpsilon)
                            {

                                //if nextSymbol has empty epsilon in its components
                                //we must add first token of next token to
                                //follow
                                s++;

                                if (s + 1 <= symbolLim)
                                {

                                    nextSymbol = ss.GetExpectedSymbol(s + 1);
                                    goto TEST_NEXT_TOKEN;

                                }
                                else
                                {

                                    WaitingFollow waiting;
                                    if (!dicNeedRevisit.TryGetValue(nt, out waiting))
                                    {

                                        waiting = new WaitingFollow(nt, thisNT);
                                        dicNeedRevisit.Add(nt, waiting);
                                    }
                                    else
                                    {

                                        waiting.AddMoreFillTarget(thisNT);
                                    }
                                }
                            }
                        }
                        else
                        {
                            thisNT.AddPossibleFollowerToken((TokenDefinition)nextSymbol, nt);
                        }
                    }

                }

                //---------***

                //for last one 
                //---------
                ISymbolDefinition lastSymbol = ss.GetExpectedSymbol(symbolLim);
                if (lastSymbol.IsNT)
                {

                    //nt : 
                    //follow of left side nt -> are follow of last symbol
                    if (lastSymbol != nt)
                    {
                        WaitingFollow waiting;
                        if (!dicNeedRevisit.TryGetValue(nt, out waiting))
                        {

                            if (nt.NTKind == NTDefintionKind.RootStartSymbol)
                            {
                                ((NTDefinition)lastSymbol).AddPossibleFollowerToken(TokenDefinition._eof, null);

                            }
                            waiting = new WaitingFollow(nt, (NTDefinition)lastSymbol);
                            dicNeedRevisit.Add(nt, waiting);

                        }
                        else
                        {

                            waiting.AddMoreFillTarget((NTDefinition)lastSymbol);
                        }
                    }
                }
            }

        }

        static void MakeFollowSetForNts(List<NTDefinition> nts)
        {

            Dictionary<NTDefinition, WaitingFollow> dicNeedRevisit = new Dictionary<NTDefinition, WaitingFollow>();
            foreach (NTDefinition nt in nts)
            {
                MakeFollowSet(nt, dicNeedRevisit);
            }
            //-----
            if (dicNeedRevisit.Count > 0)
            {
                Dictionary<NTDefinition, WaitingFollow> init1 = dicNeedRevisit;
                Dictionary<NTDefinition, WaitingFollow> dicNeedRevisit2 = null;
                do
                {

                    dicNeedRevisit2 = AddFollowerTokens(init1);
                    init1 = dicNeedRevisit2;

                } while (dicNeedRevisit2 != null);
            }
        }
        static Dictionary<NTDefinition, WaitingFollow> AddFollowerTokens(Dictionary<NTDefinition, WaitingFollow> dicNeedRevisit)
        {
            if (dicNeedRevisit.Count > 0)
            {

                //revisit
                List<NTDefinition> changedList = new List<NTDefinition>();
                foreach (WaitingFollow ww in dicNeedRevisit.Values)
                {
                    Dictionary<NTDefinition, int> waitingTargets = ww.fillingTargets;

                    //we add the dic to waiting targets
                    //check if it has more change
                    foreach (NTDefinition waitTarget in waitingTargets.Keys)
                    {

                        //if it has some change then -> we must add to the revisit list again
                        if (AddFollowerTokens(ww.source, waitTarget))
                        {

                            changedList.Add(waitTarget);
                        }
                    }
                }

                Dictionary<NTDefinition, WaitingFollow> dicNeedRevisit2 = null;
                if (changedList.Count > 0)
                {
                    dicNeedRevisit2 = new Dictionary<NTDefinition, WaitingFollow>();

                    //check if changed target can be source
                    //of filling target 
                    int cc = changedList.Count;
                    for (int i = 0; i < cc; ++i)
                    {

                        NTDefinition changedNT = changedList[i];

                        WaitingFollow foundAsSource;
                        if (dicNeedRevisit.TryGetValue(changedNT, out foundAsSource))
                        {
                            //if (foundAsSource.source.Name == "argument")
                            //{
                            //}
                            if (!dicNeedRevisit2.ContainsKey(changedNT))
                            {

                                dicNeedRevisit2.Add(changedNT, foundAsSource);
                            }
                        }
                        else
                        {

                            //if (changedNT.Name == "argument")
                            //{
                            //}
                        }
                    }
                }
                if (dicNeedRevisit2 != null && dicNeedRevisit2.Count > 0)
                {
                    return dicNeedRevisit2;
                }
                else
                {
                    return null;
                }

            }
            else
            {
                return null;
            }
        }
        static bool AddFollowerTokens(NTDefinition fromsource, NTDefinition totarget)
        {

            //ask for all follower
            TokenDefinition[] allFollowers = fromsource.GetAllPossibleFollowerTokens();
            int j = allFollowers.Length;
            bool foundNewlyAdded = false;
            for (int i = 0; i < j; ++i)
            {
                if (totarget.AddPossibleFollowerToken(allFollowers[i], fromsource))
                {
                    foundNewlyAdded = true;
                }
            }
            return foundNewlyAdded;
        }
        /// <summary>
        /// use dependency in resolving step         
        /// </summary>
        /// <param name="nts"></param>
        /// <returns></returns>
        static NTDependencyBusCollection DoDependencyAnalysis(List<NTDefinition> nts)
        {

            //first start root and go deeper 
            NTDependencyBusCollection dependBusCollection = new NTDependencyBusCollection();
            dependBusCollection.AnalysisDeps(nts);
            return dependBusCollection;
        }
        static void SwitchToUserUnderlyingNt(List<NTDefinition> nts)
        {
            for (int i = nts.Count - 1; i >= 0; --i)
            {
                NTDefinition nt = nts[i];
                for (int m = nt.SeqCount - 1; m >= 0; --m)
                {
                    nt.GetSequence(m).SwitchToUserUnderlyingNtPresentation();
                }
            }
        }
        /// <summary>
        /// dependeny analysis and collect first terminals
        /// </summary>
        /// <param name="nts"></param>
        static void CollectFirstTerminal(List<NTDefinition> nts, NTDependencyBusCollection dependBusCollection)
        {
            //and collect first terminal 
            int j = nts.Count;
            //------------------------------------
            List<NTDefinition> nonStaticNts = new List<NTDefinition>();
            for (int i = 0; i < j; ++i)
            {

                //each nt will collect start its start token**
                NTDefinition nt = nts[i];
                if (!nt.CollectFirstTerminalPhase1())
                {
                    nonStaticNts.Add(nt);
                }
            }
            //in LR parser, may has infinite loop
            //because nt may has recursion definition
            //we fix this by using parent-child dependcy analysis
            j = nonStaticNts.Count;


            bool somethingChanged = false;
            do
            {
                //loop until no change
                somethingChanged = false;
                for (int i = 0; i < j; ++i)
                {

                    //if not 'static nt'
                    //if we find new not complete,
                    //then we store it int incompleteNTs
                    var nt = nonStaticNts[i];
                    if (nt.CollectFirstTerminalPhase2())
                    {
                        somethingChanged = true;
                    }
                }

            } while (somethingChanged);

            //--------------------------------
            //phase 3: add only cyclic
            //share start token
            //in this step some nt may changed
            nonStaticNts.Clear();
            j = nts.Count;
            for (int i = 0; i < j; ++i)
            {
                if (!nts[i].IsStaticNT)
                {
                    nonStaticNts.Add(nts[i]);
                }
            }

            foreach (NTDependencyBus bus in dependBusCollection.GetCyclicBusIter())
            {
                bus.CollectShareFirstTokens();
            }
            foreach (NTDependencyBus bus in dependBusCollection.GetCyclicBusIter())
            {
                bus.MarkAllCyclicNtResolved();
            }
            //------------------------------
            //phase 4: recheck again
            j = nonStaticNts.Count;
            for (int i = 0; i < j; ++i)
            {
                //clear non static nt                
                nonStaticNts[i].CollectFirstTerminalPhase3();
            }

        }
        public NTDefinition EndSG(UserNTDefinition rootUnt)
        {
            return PrepareUserGrammarForAnyLR(rootUnt);
        }
        public NTDefinition PrepareUserGrammarForAnyLR(UserNTDefinition userRootNt)
        {


            foreach (TokenDefinition tokendef in this.tokenInfoCollection.GetTokenDefinitionIter())
            {
                if (tokendef.TokenPrecedence > 0)
                {
                    this.symResolutionInfo.AddSymbolPrecedenceAndAssoc(
                        tokendef,
                        tokendef.TokenPrecedence,
                        tokendef.IsRightAssoc);
                }
            }

            //-------------------------------    
            int j = this.allUserNTs.Count;
            //------------------------------- 
            //find first terminal list
            ResolveUserNTDefs(this.allUserNTs);
            //-----------------------------------------------


            //1. first round resolve early user nt definition
            //assign which terminal , nonterminal symbol

            foreach (UserNTDefinition nt in this.allUserNTs.GetUserNTIterForward())
            {
                nt.CollectAllPossibleSequences();
            }
            foreach (UserNTDefinition nt in this.allUserNTs.GetUserNTIterForward())
            {
                nt.FilteroutDuplicateSequences();
            }
            List<NTDefinition> preFinals = new List<NTDefinition>(j);
            foreach (UserNTDefinition nt in this.allUserNTs.GetUserNTIterForward())
            {

                //create final nt from user nt
                preFinals.Add(CreateNTDefinition(nt));
            }

            //-------------------------------------------   
            foreach (NTDefinition nt in preFinals)
            {
                //create 'body'
                CreateBodySequence(nt, this.symResolutionInfo);
                if (nt.SeqCount == 0 && !nt.HasEmptyEpsilonForm)
                {
                }
            }
            //------------------------------------------- 
            List<NTDefinition> finalNts = new List<NTDefinition>(j);
            foreach (NTDefinition nt in preFinals)
            {
                if (nt.SeqCount > 0)
                {
                    finalNts.Add(nt);
                }
                else
                {
                    if (nt.HasEmptyEpsilonForm || nt.NTKind == NTDefintionKind.UnknownNT)
                    {
                        finalNts.Add(nt);
                    }
                    else
                    {
                        this.symResolutionInfo.AddNtSequenceResolveMessage("not found " + nt.ToString());
                    }
                }
            }

            //-------------------------------------------   
            var dependBus = DoDependencyAnalysis(finalNts);
            SwitchToUserUnderlyingNt(finalNts);
            CollectFirstTerminal(finalNts, dependBus);
            //-------------------------------------------   
            //create 'augmented' root nt
            //------------------------------------  

            NTDefinition augmentedNT = CreateAugmentedRootNTDefinition(userRootNt);
            finalNts.Insert(0, augmentedNT);
            //assign number to all user expected symbol
            AssignSqNumber(finalNts);
            //------------------------------------  
            MakeFollowSetForNts(finalNts);
            //------------------------------------  
            foreach (NTDefinition nt in finalNts)
            {
                dicCoreNTs.Add(nt.Name, nt);
            }
            //------------------------------------  
            //error handling state
            foreach (NTDefinition nt in finalNts)
            {
                FindSyncToken(nt, finalNts);
            }



            //------------------------------------  
            return augmentedNT;
        }
        static void FindSyncToken(NTDefinition nt, List<NTDefinition> allNts)
        {
            switch (nt.NTKind)
            {
                case NTDefintionKind.RootStartSymbol:

                    break;
                case NTDefintionKind.UnknownNT:
                    break;
                default:
                    break;
            }

            //List<SeqSync> seqSyncs = new List<SeqSync>();

            //int seqCount = nt.SeqCount;
            //for (int i = 0; i < seqCount; ++i)
            //{
            //    SymbolSequence seq = nt.GetSequence(i);
            //    int rightSideCount = seq.RightSideCount;
            //    seqSyncs.Clear();

            //    for (int n = 0; n < rightSideCount; ++n)
            //    {
            //        ISymbolDefinition symbol = seq[n];
            //        if (!symbol.IsNT)
            //        {
            //            TokenDefinition tk = (TokenDefinition)symbol;
            //            seqSyncs.Add(new SeqSync(tk, n, SyncSymbolKind.Shift));
            //        }
            //        else
            //        {
            //            NTDefinition symNt = (NTDefinition)symbol;
            //            if (symNt.NTKind != NTDefintionKind.UnknownNT)
            //            {
            //                TokenDefinition[] firstNts = symNt.GetAllFirstTokens(); 
            //                TokenDefinition[] followNts = symNt.GetAllPossibleFollowerTokens();
            //            }
            //        }
            //    }

            //    if (seqSyncs.Count > 0)
            //    {
            //        seq.SyncTokens = seqSyncs.ToArray();
            //    }

            //}

        }
        public void BeginSG()
        {
            this.dicCoreNTs = new Dictionary<string, NTDefinition>();
            this.allUserNTs = new UserNTCollection();
            this.symResolutionInfo = new SymbolResolutionInfo();

        }
        public NTDefinition[] PrepareUserGrammarForAnyLR(UserNTDefinition[] multipleRootNts)
        {
            //-------------------------------    
            int j = this.allUserNTs.Count;
            //------------------------------- 
            //find first terminal list
            ResolveUserNTDefs(this.allUserNTs);
            //----------------------------------------------- 

            //1. first round resolve early user nt definition
            //assign which terminal , nonterminal symbol

            foreach (UserNTDefinition nt in this.allUserNTs.GetUserNTIterForward())
            {
                nt.CollectAllPossibleSequences();
            }
            foreach (UserNTDefinition nt in this.allUserNTs.GetUserNTIterForward())
            {
                nt.FilteroutDuplicateSequences();
            }
            List<NTDefinition> preFinals = new List<NTDefinition>(j);
            foreach (UserNTDefinition nt in this.allUserNTs.GetUserNTIterForward())
            {

                //create final nt from user nt
                preFinals.Add(CreateNTDefinition(nt));
            }
            //-------------------------------------------   
            foreach (NTDefinition nt in preFinals)
            {
                //create 'body'
                CreateBodySequence(nt, this.symResolutionInfo);
                if (nt.SeqCount == 0 && !nt.HasEmptyEpsilonForm)
                {
                }
            }
            //------------------------------------------- 
            List<NTDefinition> finalNTs = new List<NTDefinition>(j);
            foreach (NTDefinition nt in preFinals)
            {
                if (nt.SeqCount > 0)
                {
                    finalNTs.Add(nt);
                }
                else
                {
                    if (nt.HasEmptyEpsilonForm)
                    {
                        finalNTs.Add(nt);
                    }
                    else
                    {
                        this.symResolutionInfo.AddNtSequenceResolveMessage("not found " + nt.ToString());
                    }
                }
            }

            //-------------------------------------------   
            var dependBus = DoDependencyAnalysis(finalNTs);
            SwitchToUserUnderlyingNt(finalNTs);
            CollectFirstTerminal(finalNTs, dependBus);
            //-------------------------------------------   
            //create 'augmented' root nt
            //------------------------------------  
            NTDefinition[] multiRoots = new NTDefinition[multipleRootNts.Length];
            int i = 0;
            foreach (UserNTDefinition user_rootnt in multipleRootNts)
            {
                NTDefinition augmentedNT = CreateAugmentedRootNTDefinition(user_rootnt);
                finalNTs.Insert(0, augmentedNT);
                multiRoots[i] = augmentedNT;
                i++;
            }

            AssignSqNumber(finalNTs);
            //------------------------------------  
            MakeFollowSetForNts(finalNTs);
            //------------------------------------  
            foreach (NTDefinition nt in finalNTs)
            {
                dicCoreNTs.Add(nt.Name, nt);
            }
            //----------------
            return multiRoots;
        }
        static void AssignSqNumber(List<NTDefinition> finalNTs)
        {
            int j = finalNTs.Count;
            int sqcountTotal = 0;
            for (int i = 0; i < j; ++i)
            {
                NTDefinition nt = finalNTs[i];
                int sqcount = nt.SeqCount;
                for (int s = 0; s < sqcount; ++s)
                {
                    nt.GetSequence(s).TotalSeqNumber = sqcountTotal++;
                }
            }

        }
        static NTDefinition CreateAugmentedRootNTDefinition(UserNTDefinition userRootNt)
        {
            ISymbolDefinition rootnt = userRootNt.GenNT;
            NTDefinition user_root_nt = (NTDefinition)rootnt;
#if DEBUG
            if (user_root_nt == null)
            {
            }
#endif
            //----------------------------------------------------------------------
            UserNTDefinition augmentUserNt = new UserNTDefinition(user_root_nt.Name + "'");
            UserSymbolSequence augmentedUserSymbolSq = new UserSymbolSequence(augmentUserNt);
            augmentedUserSymbolSq.AppendLast(new UserExpectedSymbol(userRootNt));
            //--------------------------------------------------------------------------------
            NTDefinition augmentedNT = new NTDefinition(user_root_nt.Name + "'", NTDefintionKind.RootStartSymbol);
            augmentedNT.LoadSequences(new[] { new SymbolSequence(new[] { user_root_nt }, new[] { 0 }, augmentedUserSymbolSq) });
            return augmentedNT;
        }



        class WaitingFollow
        {
            public NTDefinition source;
            public Dictionary<NTDefinition, int> fillingTargets = new Dictionary<NTDefinition, int>();

            public WaitingFollow(NTDefinition source, NTDefinition target)
            {
                this.source = source;
                fillingTargets.Add(target, 0);
            }
            public void AddMoreFillTarget(NTDefinition target)
            {
                if (!fillingTargets.ContainsKey(target))
                {
                    fillingTargets.Add(target, 0);
                }

            }
            public override string ToString()
            {
                StringBuilder stbuilder = new StringBuilder();

                stbuilder.Append("source=");
                stbuilder.Append(source.ToString());
                stbuilder.Append("  fill  to ");

                int j = fillingTargets.Count;
                int i = 0;
                foreach (NTDefinition nt in fillingTargets.Keys)
                {
                    stbuilder.Append(nt.ToString());

                    if (i < j - 1)
                    {
                        stbuilder.Append(',');
                    }
                    i++;
                }
                return stbuilder.ToString();

            }
        }
    }

}