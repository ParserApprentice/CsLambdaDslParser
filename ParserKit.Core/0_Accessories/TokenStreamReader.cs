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
        DevDebugLogger dbugLogger;
        public void dbugInitLogs(string filename)
        {
            if (dbugLogger != null)
            {
                return;
            }

            this.dbugLogger = new DevDebugLogger(filename);

        }
        public DevDebugLogger dbugLogWriter
        {
            get { return this.dbugLogger; }
        }
#endif
    }

#if DEBUG
    public class DevDebugLogger
    {
        string dbugLogFileName;
        System.IO.FileStream dbugFs;
        System.IO.StreamWriter dbugWriter;

        int lineNo;
        public DevDebugLogger(string outputFilename)
        {
            this.dbugLogFileName = outputFilename;
            dbugFs = new System.IO.FileStream(outputFilename, System.IO.FileMode.Create);
            dbugWriter = new System.IO.StreamWriter(dbugFs);
            dbugWriter.AutoFlush = true;
        }
        public void Write(string data)
        {
            dbugWriter.Write(data);
        }
        public void Write(char data)
        {
            dbugWriter.Write(data);
        }
        public void WriteLine(string data)
        {
            dbugWriter.WriteLine(data);
            lineNo++;
        }
        public void Flush()
        {
            dbugWriter.Flush();
        }
        public void NotifyShiftEvent()
        {
        }
        public void NotifyReduceEvent()
        {
        }
        public void NotifyErrorEvent()
        {
        }
        public void NotifyConflictRR()
        {
        }
    }
#endif

}