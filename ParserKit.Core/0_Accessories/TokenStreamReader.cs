//MIT, 2015-2017, ParserApprentice  

namespace Parser.ParserKit
{

    public class TokenStreamReader
    {
        Token cachedTk;
        TokenStream tokenStream;
        //int currentReadPos = -1;
        public TokenStreamReader(TokenStream tokenStream)
        {
            this.tokenStream = tokenStream;
        }

        static int dbugIdCount = 0;
        public Token GetToken(int index)
        {

            return tokenStream.GetToken(index);
        }
        /// <summary>
        /// read next only functional
        /// </summary>
        /// <returns></returns>
        public Token ReadNext()
        {
            //currentReadPos++;
            return cachedTk = this.tokenStream.ReadNextToken();
            //Console.WriteLine((dbugIdCount++) + cachedTk.ToString());
            //return cachedTk;
        }
        public void EnsureNext(int number)
        {
            //ensure for next ...  
            tokenStream.LoadMore();
        }
        public Token CurrentToken
        {
            get
            {

                return this.cachedTk;
            }
        }

        public int CurrentReadIndex
        {
            get
            {
                return tokenStream.CurrentPosInPage;
            }
        }
        public void SetIndex(int index)
        {
            //this.currentReadPos = index;
            tokenStream.SetCurrentPosition(index);
            this.cachedTk = tokenStream.GetToken(index);

        }
        internal TokenStream OriginalTokens
        {
            get { return this.tokenStream; }
        }

#if DEBUG
        string dbugLogFileName;
        System.IO.FileStream dbugFs;
        System.IO.StreamWriter dbugWriter;
        public void dbugInitLogs(string filename)
        {
            if (dbugLogFileName != null)
            {
                return;
            }

            this.dbugLogFileName = filename;
            dbugFs = new System.IO.FileStream(filename, System.IO.FileMode.Create);
            dbugWriter = new System.IO.StreamWriter(dbugFs);
            dbugWriter.AutoFlush = true;
        }
        public System.IO.StreamWriter dbugLogWriter
        {
            get { return this.dbugWriter; }
        }
#endif
    }



}