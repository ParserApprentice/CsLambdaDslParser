//MIT, 2015-2017, ParserApprentice
using System;
using System.Text;
using System.Collections.Generic;

namespace Parser.ParserKit
{

    class NTDependencyBusCollection
    {

        Dictionary<NTDefinition, int> visitedNTs = new Dictionary<NTDefinition, int>();
        List<NTDependencyBus> cylcicsBus = new List<NTDependencyBus>();
        List<NTReductionChain> reductionChains = new List<NTReductionChain>();

        public NTDependencyBusCollection()
        {

        }
        public void AnalysisDeps(List<NTDefinition> nts)
        {
            List<NTDependencyBus> tobeSortBusList = new List<NTDependencyBus>();
            List<NTDependencyBus> leafNT_NoCyclics = new List<NTDependencyBus>();
            List<NTDependencyBus> branchNt_NoCyclics = new List<NTDependencyBus>();

            int j = nts.Count;
            for (int i = 0; i < j; ++i)
            {
                NTDefinition nt = nts[i];
                NTDependencyBus newbus = this.CreateNewBus(nt);
                nt.CheckDependency(newbus);
                newbus.EliminateSimpleLeftRecursion();
                if (newbus.CyclicCount > 0)
                {
                    //has cyclic
                    newbus.MarkAllCyclicNtInAllChains();
                    this.cylcicsBus.Add(newbus);
                    tobeSortBusList.Add(newbus);
                    if (!visitedNTs.ContainsKey(newbus.StartNT))
                    {
                        visitedNTs.Add(newbus.StartNT, 0);
                    }
                }
                else
                {

                    if (newbus.IsLeafNt)
                    {

                        //not need to sort leafNT
                        leafNT_NoCyclics.Add(newbus);
                    }
                    else
                    {
                        tobeSortBusList.Add(newbus);
                        branchNt_NoCyclics.Add(newbus);
                    }
                }
            }

            //--------------------------------------
            tobeSortBusList.Sort((bus1, bus2) =>
            {
                return bus1.MaxLevelCount.CompareTo(bus2.MaxLevelCount);
            });
            //--------------------------------------
            //asssign NT depth level
            //--------------------------------------

            j = tobeSortBusList.Count;
            for (int i = tobeSortBusList.Count - 1; i > -1; --i)
            {
                NTDependencyBus bus = tobeSortBusList[i];
                if (bus.StartNT.ReductionChain == null)
                {

                    NTReductionChain reductionChain = new NTReductionChain(i + 1);
                    reductionChains.Add(reductionChain);
                    bus.AssignReductionChain(reductionChain);
                }
            }

            //--------------------------------------
        }
        NTDependencyBus CreateNewBus(NTDefinition forNT)
        {
            NTDependencyBus bus = new NTDependencyBus(forNT, this);
            return bus;
        }

        public bool HasVisit(NTDefinition nt)
        {
            return visitedNTs.ContainsKey(nt);
        }
        public IEnumerable<NTDependencyBus> GetCyclicBusIter()
        {
            foreach (NTDependencyBus bus in this.cylcicsBus)
            {
                yield return bus;
            }
        }
    }
    enum NTDependencyChainType
    {
        Unknown,
        SimpleLeftRecursion,
        CyclicRecursionStartEnd,
        CyclicRecursionIntermediateEnd,
        CyclicRecursionEndPart
    }
    class NTCyclicDependencyChain
    {
        List<NTDefinition> ntlist = null;
        public NTCyclicDependencyChain(List<NTDefinition> nts, NTDefinition lastNT)
        {
            //copy 
            ntlist = new List<NTDefinition>(nts);
            ntlist.Add(lastNT);

            AnalysisChainType();
        }
        void AnalysisChainType()
        {
            int count = ntlist.Count;
            if (count > 1)
            {

                //search back to front
                if (ntlist[count - 2] != ntlist[count - 1])
                {
                    if (ntlist[0] == ntlist[count - 1])
                    {
                        this.ChainType = NTDependencyChainType.CyclicRecursionStartEnd;
                    }
                    else
                    {
                        if (ntlist[count - 1].HasSomeCyclicForm)
                        {
                            this.ChainType = NTDependencyChainType.CyclicRecursionEndPart;
                        }
                        else
                        {
                            this.ChainType = NTDependencyChainType.CyclicRecursionIntermediateEnd;
                        }
                    }

                }
                else
                {

                    this.ChainType = NTDependencyChainType.SimpleLeftRecursion;
                }
            }
            else
            {
                this.ChainType = NTDependencyChainType.Unknown;
            }

        }
        public NTDependencyChainType ChainType
        {
            get;
            private set;
        }
        public override string ToString()
        {
            return ChainType.ToString() + ":" + this.ntlist.Count;
        }
        public IEnumerable<NTDefinition> GetNtIterForward()
        {
            int j = ntlist.Count;
            for (int i = 0; i < j; ++i)
            {
                yield return this.ntlist[i];
            }
        }
        public IEnumerable<NTDefinition> GetNtIterBackward()
        {

            for (int i = ntlist.Count - 1; i > -1; --i)
            {
                yield return this.ntlist[i];
            }
        }
    }


    struct NTDependencyBusVisitHx
    {
        public readonly NTDefinition ntdef;
        public readonly int level;
        public NTDependencyBusVisitHx(int level, NTDefinition ntdef)
        {
            this.level = level;
            this.ntdef = ntdef;
        }
        public override string ToString()
        {
            return new string('>', level) + level + ":" + this.ntdef.ToString();
        }
    }

    struct NTReductionItem
    {
        public readonly NTDefinition ntdef;
        public NTReductionItem(NTDefinition ntdef)
        {
            this.ntdef = ntdef;
        }
        public override string ToString()
        {
            int level = this.ntdef.NTDepthLevel;
            return new string('>', level) + level + ":" + this.ntdef.ToString();
        }
    }

    public class NTReductionChain
    {
        List<NTReductionItem> reductionItems = new List<NTReductionItem>();
        internal NTReductionChain(int chainNumber)
        {
            this.ChainNumber = chainNumber;
        }
        public int ChainNumber
        {
            get;
            private set;
        }
        public void AddReductionItem(NTDefinition nt)
        {
            nt.ReductionChain = this;
            reductionItems.Add(new NTReductionItem(nt));
        }
        public NTDefinition FirstNt
        {
            get
            {
                if (reductionItems.Count > 0)
                {
                    return reductionItems[0].ntdef;
                }
                return null;
            }
        }
        public int ReductionItemCount
        {
            get
            {
                return reductionItems.Count;
            }

        }
    }

    class NTDependencyBus
    {
        List<NTDefinition> visitNTs = new List<NTDefinition>();

        List<NTDependencyBusVisitHx> visitNTHx = new List<NTDependencyBusVisitHx>();
        List<NTCyclicDependencyChain> foundCyclics = new List<NTCyclicDependencyChain>();
        NTDependencyBusCollection owner;
        int currentLevel;
        int maxLevelCount;

#if DEBUG
        List<NTCyclicDependencyChain> dbugPrevCyclics;
#endif

        public NTDependencyBus(NTDefinition nt, NTDependencyBusCollection owner)
        {
            this.owner = owner;
            this.StartNT = nt;
        }
        public NTDefinition StartNT
        {
            get;
            private set;
        }
        public bool AlreadyHasCyclic(NTDefinition nt)
        {
            return this.owner.HasVisit(nt);
        }
        public bool EnterNT(NTDefinition nt)
        {

            int pos = this.visitNTs.IndexOf(nt);
            if (pos > -1)
            {

                return false;
            }
            else
            {
                this.currentLevel++;
                this.visitNTs.Add(nt);
                if (nt != this.StartNT)
                {
                    if (this.currentLevel > this.maxLevelCount)
                    {
                        this.maxLevelCount = this.currentLevel;
                    }
                    this.visitNTHx.Add(new NTDependencyBusVisitHx(this.currentLevel, nt));
                }
                return true;
            }
        }
        public int MaxLevelCount
        {
            get
            {
                return this.maxLevelCount;
            }
        }

        public NTDependencyBus MergeToDependencyBus
        {
            get;
            set;
        }

        public void AssignReductionChain(NTReductionChain reductionChain)
        {
            NTDefinition nt = this.StartNT;
            if (this.StartNT.NTDepthLevel == 0)
            {
                nt.NTDepthLevel = 1;
                reductionChain.AddReductionItem(nt);
            }

            int j = visitNTHx.Count;
            for (int i = 0; i < j; ++i)
            {
                NTDependencyBusVisitHx visitHx = visitNTHx[i];
                nt = visitHx.ntdef;
                if (nt.NTDepthLevel < visitHx.level)
                {
                    //nt depth level must be assigned befoare add to
                    //reduction chain
                    nt.NTDepthLevel = visitHx.level;
                    reductionChain.AddReductionItem(nt);
                }
            }

        }
        public void ExitNT()
        {
            this.currentLevel--;
            this.visitNTs.RemoveAt(this.visitNTs.Count - 1);
        }
        public override string ToString()
        {
            return this.StartNT.ToString();
        }
        public void SnapCyclicBranch(NTDefinition recurAt)
        {

            this.foundCyclics.Add(new NTCyclicDependencyChain(this.visitNTs, recurAt));
        }
        public void EliminateSimpleLeftRecursion()
        {
            int j = foundCyclics.Count;
            List<NTCyclicDependencyChain> newlist = new List<NTCyclicDependencyChain>(j);
            for (int i = 0; i < j; ++i)
            {
                NTCyclicDependencyChain chain = this.foundCyclics[i];
                switch (chain.ChainType)
                {

                    case NTDependencyChainType.CyclicRecursionEndPart:
                        {

                        } break;
                    case NTDependencyChainType.CyclicRecursionIntermediateEnd:
                    case NTDependencyChainType.CyclicRecursionStartEnd:
                        {
                            newlist.Add(chain);
                        } break;
                }

            }
#if DEBUG
            this.dbugPrevCyclics = foundCyclics;
#endif
            this.foundCyclics = newlist;
        }
        public int CyclicCount
        {
            get
            {

                return this.foundCyclics.Count;
            }
        }

        public bool IsLeafNt
        {
            get
            {
                return this.CyclicCount == 0 && this.visitNTHx.Count == 0;
            }
        }
        public void MarkAllCyclicNtInAllChains()
        {
            this.StartNT.HasSomeCyclicForm = true;
            foreach (NTCyclicDependencyChain chain in this.foundCyclics)
            {
                foreach (NTDefinition nt in chain.GetNtIterForward())
                {
                    nt.HasSomeCyclicForm = true;
                }
            }
        }
        public void CollectShareFirstTokens()
        {
            foreach (NTCyclicDependencyChain chain in this.foundCyclics)
            {
                //iterate bottom -up style
                bool repeat = false;
                Dictionary<TokenDefinition, int> shareFirstTokens = new Dictionary<TokenDefinition, int>();
                do
                {

                    foreach (NTDefinition nt in chain.GetNtIterBackward())
                    {
                        TokenDefinition[] firstTokens = nt.GetAllFirstTokens();
                        for (int i = firstTokens.Length - 1; i >= 0; --i)
                        {
                            if (!shareFirstTokens.ContainsKey(firstTokens[i]))
                            {
                                shareFirstTokens.Add(firstTokens[i], 0);
                            }
                        }
                    }
                    //-----------

                    //send new token into nt list
                    List<NTDefinition> changedNts = new List<NTDefinition>();
                    foreach (NTDefinition nt in chain.GetNtIterBackward())
                    {
                        if (nt.AddFirstTerminalPhase3IfNotExists(shareFirstTokens))
                        {

                            changedNts.Add(nt);
                        }

                    }
                    repeat = changedNts.Count > 0;
                } while (repeat);
                //-----------
            }
        }

        public void MarkAllCyclicNtResolved()
        {
            this.StartNT.CyclicFormResolved = true;
            foreach (NTCyclicDependencyChain chain in this.foundCyclics)
            {
                foreach (NTDefinition nt in chain.GetNtIterForward())
                {
                    nt.CyclicFormResolved = true;
                }
            }
        }
    }
}