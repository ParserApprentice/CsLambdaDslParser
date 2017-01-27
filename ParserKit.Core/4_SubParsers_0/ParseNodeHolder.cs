//MIT, 2015-2017, ParserApprentice
using System; 

namespace Parser.ParserKit
{

    public abstract class ParseNodeHolder
    {
        const int INIT_STACKSIZE = 2048 * 2;
        object[] astStack = null;
        int currentIndex;
        int currentLim;
        int currentChildAstIndex;

        public readonly int parseNodeHolderId = ++parseNodeHolderIdTotal;
        static int parseNodeHolderIdTotal;
#if DEBUG
        //Stack<int> dbugAddSteps = new Stack<int>();
        //public int dbugCounting;
        //public int dbugAddStep;
        static int dbugTotalId;
        public readonly int dbugId = dbugTotalId++;
#endif
        object _contextAst;
        LR.ParseNodeStack stack;

        ParseNode context_owner;
        NTAstNode ntAstNode;

        public ParseNodeHolder()
        {
            astStack = new object[INIT_STACKSIZE];

            currentLim = INIT_STACKSIZE;
            currentIndex = 0;
        }
        internal LR.ParseNodeStack ParseNodeStack
        {
            get { return stack; }
            set { stack = value; }
        }
        internal int CurrrentRightSymbolCount
        {
            get;
            set;
        }
        internal SubParser CurrentSubParser { get; set; }

        internal bool CancelNextParseTree { get; set; }

        public void DontBuildNextParseTree()
        {
            CancelNextParseTree = true;
        }
        internal void ResetBuildNextTree()
        {
            CancelNextParseTree = false;
        }
        public ParserReporter Reporter
        {
            get;
            internal set;
        }
        public ParseNode GetChildNode(int index)
        {
            return null;
        }

        public ParseNode ContextOwner
        {
            get { return context_owner; }
            set { context_owner = value; }
        }
        public NTAstNode GetContextNtAstNode()
        {
            if (ntAstNode != null)
            {
                return ntAstNode;
            }
            else
            {
                if (_contextAst == null)
                {
                    return null;
                }
                else
                {
                    return ntAstNode = new NTAstNode(_contextAst);
                }
            }
            return ntAstNode;
        }
        public object ContextAst
        {
            get { return _contextAst; }
            set
            {
                _contextAst = value;
                ntAstNode = null;//reset
            }
        }

        public void A(object astNode)
        {
            _contextAst = astNode;
        }

        /// <summary>
        /// return context's ast
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T A<T>()
        {
            return (T)_contextAst;
        }
        /// <summary>
        /// return lastest ast from subitem
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T B<T>()
        {
            return (T)GetSubItemAst();
        }
        protected virtual object GetSubItemAst()
        {
            return null;
        }

        public SymbolSequence CurrentContextSequence
        {
            get;
            set;
        }

#if DEBUG
        public override string ToString()
        {
            if (this.ContextOwner != null)
            {
                return ContextOwner.ToString() + " /**/ " + CurrentContextSequence.ToString();
            }
            else
            {
                return CurrentContextSequence.ToString();
            }
        }
#endif

        /// <summary>
        /// during parsing only,
        /// </summary>
        internal void SetParsingContextNt(ParseNode nt)
        {
            ContextOwner = nt;
        }
        //-----------------------------------------------------
        public object GetContextAst(int index)
        {
            return astStack[index];
        }
        public void AddContextAst(object obj)
        {
            if (currentIndex >= currentLim)
            {
                //expand
                object[] newArray = new object[astStack.Length + INIT_STACKSIZE];
                Array.Copy(astStack, newArray, astStack.Length); //move to new buffer
                astStack = newArray;
                currentLim += INIT_STACKSIZE;
            }
            astStack[currentIndex++] = obj;
        }
        public void Pop(int num)
        {
            currentIndex -= num;
        }
        public object Pop()
        {
            return astStack[--currentIndex];
        }
        public int CurrentAstIndex
        {
            get { return currentIndex; }
        }
        public int ContextChildCount
        {
            get;
            set;
        }
        public int CurrentChildAstIndex
        {
            get { return currentChildAstIndex; }
            set { currentChildAstIndex = value; }
        }
        public void PeekStack(out ParseNode p1, out ParseNode p2, out ParseNode p3)
        {
            if (this.CurrrentRightSymbolCount == 3)
            {
                stack.Peek(out p1, out p2, out p3);
            }
            else
            {
                throw new NotFiniteNumberException("not correct");
            }
        }
    }

}