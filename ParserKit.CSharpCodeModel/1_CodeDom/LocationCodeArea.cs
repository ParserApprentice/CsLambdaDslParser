//MIT, 2015-2017, ParserApprentice
 
 

namespace Parser.CodeDom
{   
    public struct LocationCodeArea
    {
        public static readonly LocationCodeArea Empty = new LocationCodeArea();

        int locFlags;
        int beginLineNumber;
        int endLineNumber;
        short beginColumnNumber;
        short endColumnNumber;


        public LocationCodeArea(int beginLineNumber, short beginColumnNumber,
            int endLineNumber, short endColumnNumber)
        {
            this.locFlags = 1;
            this.beginLineNumber = beginLineNumber;
            this.beginColumnNumber = beginColumnNumber;
            this.endLineNumber = endLineNumber;
            this.endColumnNumber = endColumnNumber;
        }
        public bool IsEmpty
        {
            get
            {
                return locFlags == 0;
            }
        }


        public LocationCodeArea Combine(LocationCodeArea another)
        {
            if (another.IsEmpty)
            {

                return this;
            }
            else
            {
                if (this.IsEmpty)
                {
                    return another;
                }


                int finalBeginLineNum = this.beginLineNumber;
                short finalBeginCol = this.beginColumnNumber;

                int finalEndLineNum = this.endLineNumber;
                short finalEndCol = this.endColumnNumber;



                if (another.beginLineNumber < finalBeginLineNum)
                {

                    finalBeginLineNum = another.beginLineNumber;
                    finalBeginCol = another.beginColumnNumber;
                }
                else if (another.beginLineNumber == finalBeginLineNum)
                {

                    if (another.beginColumnNumber < finalBeginCol)
                    {
                        finalBeginCol = another.beginColumnNumber;
                    }
                }
                else
                {


                }


                if (another.endLineNumber > finalEndLineNum)
                {
                    finalEndLineNum = another.endLineNumber;
                    finalEndCol = another.endColumnNumber;
                }
                else if (another.endLineNumber == finalEndLineNum)
                {
                    if (another.endColumnNumber > finalEndCol)
                    {
                        finalEndCol = another.endColumnNumber;
                    }
                }
                else
                {


                }
                return new LocationCodeArea(finalBeginLineNum, finalBeginCol, finalEndLineNum, finalEndCol);
            }


        }


        public override string ToString()
        {
            return GetSourceLocationString();
        }

        public string GetSourceLocationString()
        {
            return "(" + beginLineNumber + "," + beginColumnNumber + ") , (" +
               endLineNumber + "," + endColumnNumber + ")";
        }

        public int BeginLineNumber
        {
            get
            {
                return beginLineNumber;
            }
        }

        public int BeginColumnNumber
        {
            get
            {
                return beginColumnNumber;
            }
        } 
        public int EndLineNumber
        {
            get
            {
                return endLineNumber;
            }
        }
        public int EndColumnNumber
        {
            get
            {
                return endColumnNumber;
            }
        }
    }

}