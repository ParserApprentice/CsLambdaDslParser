//MIT, 2015-2017, ParserApprentice
using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using Parser.ParserKit.LR;
namespace Parser.ParserKit.SubParsers
{

    class ParserDataBinaryCache
    {
        byte[] _md5Stamp;
        List<IntermeidateTokenDef> tokenDefs;
        List<IntermediateNt> nts;

        internal List<SubParsers.SwitchDetail> swDetails;
        internal List<List<LRItemTodo>> todoListForTks;
        internal List<List<LRItemTodo>> todoListForNts;


        static System.Security.Cryptography.MD5 md5Hash = System.Security.Cryptography.MD5.Create();
        const int END_MARKER = 12345;

        static byte[] CalculateMd5Stamp(Stream fs)
        {
            long tmppos = fs.Position;
            fs.Position = 0;
            byte[] data = new byte[(int)tmppos];
            fs.Read(data, 0, (int)tmppos);
            //-----------------------------------
            byte[] md5HashValue = md5Hash.ComputeHash(data);
            //-----------------------------------
            fs.Position = tmppos;//reset
            //----------------------------------- 
            return md5HashValue;

        }
        public static void SaveToBinary(LRParsingTable parsingTable, string filename)
        {
            if (parsingTable == null)
            {
                throw new NotSupportedException();
            }
            ColumnBasedTable<TokenDefinition, LRItemTodo> tkTable = parsingTable.tokenTable;
            ColumnBasedTable<NTDefinition, LRItemTodo> ntTable = parsingTable.ntTable;


            int rowCount = ntTable.RowCount;
            int ntColumnCount = ntTable.columns.Count;
            int tkColumnCount = tkTable.columns.Count;


            using (FileStream fs = new FileStream(filename, System.IO.FileMode.Create))
            using (BinaryWriter writer = new BinaryWriter(fs, Encoding.UTF8))
            {

                WriteParserTableHeader(parsingTable, writer);

                //--------------------------------------------
                //calculate md5 ***
                writer.Flush();
                byte[] md5Stamp = CalculateMd5Stamp(fs);
                writer.Write((int)md5Stamp.Length);
                writer.Write(md5Stamp);
                //-------------------------------------------
                //switch record
                //--------------------------------------------
                //unresolve switch table
                List<SubParsers.SwitchDetail> swDetails = parsingTable.swDetailRecords;
                //--------------------------------------------
                //record sw detail 
                Dictionary<string, int> dic = CreateStringTable(swDetails);
                //write string table
                writer.Write(dic.Count);
                foreach (string str in dic.Keys)
                {
                    writer.Write(str);
                }
                //--------------------------------------------
                {
                    int swCount = swDetails.Count;
                    writer.Write(swCount);
                    for (int i = 1; i < swCount; ++i)
                    {
                        SubParsers.SwitchDetail swDetail = swDetails[i];
                        //start at 1
                        int n = swDetail.Count;
                        writer.Write(n);
                        for (int m = 0; m < n; ++m)
                        {
                            SubParsers.SwitchPair p = swDetail.GetSwPair(m);
                            int found;
                            if (!dic.TryGetValue(p.symbolName, out found))
                            {
                                throw new NotSupportedException();
                            }
                            writer.Write(found);
                            writer.Write(p.switchBackState);
                        }
                    }
                }

                //8. 
                writer.Write(rowCount);
                //write column by column


                //tk
                for (int c = 0; c < tkColumnCount; ++c)
                {
                    var cells = tkTable.columns[c].cells;
                    //write down all cell                                                 
                    for (int r = 0; r < rowCount; ++r)
                    {
                        LRItemTodo todo = cells[r];
                        switch (todo.ItemKind)
                        {
                            case LRItemTodoKind.Shift:
                                writer.Write((todo.StateNumber << 8) | ((int)todo.ItemKind & 0xFF));
                                writer.Write((todo.OriginalSeqNumberForShift << 8) | (todo.SampleUserExpectedSymbolPos & 0xFF));
                                break;
                            default:
                                writer.Write((todo.StateNumber << 8) | ((int)todo.ItemKind & 0xFF));
                                break;
                        }
                    }
                    writer.Write(END_MARKER);//end col marker
                }

                for (int c = 0; c < ntColumnCount; ++c)
                {
                    var cells = ntTable.columns[c].cells;

                    //write down all cell                                                 
                    for (int r = 0; r < rowCount; ++r)
                    {

                        LRItemTodo todo = cells[r];
                        switch (todo.ItemKind)
                        {
                            case LRItemTodoKind.Shift:
                                writer.Write((todo.StateNumber << 8) | ((int)todo.ItemKind & 0xFF));
                                writer.Write((todo.OriginalSeqNumberForShift << 8) | (todo.SampleUserExpectedSymbolPos & 0xFF));
                                break;
                            default:
                                writer.Write((todo.StateNumber << 8) | ((int)todo.ItemKind & 0xFF));
                                break;
                        }
                    }
                    writer.Write(END_MARKER);//end col marker
                }

                writer.Flush();
                writer.Close();
                fs.Close();
            }
        }


        static void WriteParserTableHeader(LRParsingTable parsingTable, BinaryWriter writer)
        {
            ColumnBasedTable<TokenDefinition, LRItemTodo> tkTable = parsingTable.tokenTable;
            ColumnBasedTable<NTDefinition, LRItemTodo> ntTable = parsingTable.ntTable;


            int rowCount = ntTable.RowCount;
            int ntColumnCount = ntTable.columns.Count;
            int tkColumnCount = tkTable.columns.Count;
            //table columns tk and nt defintion
            writer.Write(tkColumnCount); //tkcount
            for (int c = 0; c < tkColumnCount; ++c)
            {
                TokenDefinition tk = tkTable.columns[c].columnHeader;
                //1. id
                writer.Write(tk.TokenInfoNumber); //int 
                //2. symbol string
                writer.Write(tk.SymbolName);//string
                //2. presentation
                writer.Write(tk.PresentationString ?? "");//string
            }
            //--------------------------------------------
            //nt 
            writer.Write(ntColumnCount);
            for (int c = 0; c < ntColumnCount; ++c)
            {
                NTDefinition nt = ntTable.columns[c].columnHeader;
                //1. id
                writer.Write(nt.NtId); //int
                //2. nt name
                writer.Write(nt.Name);
                //3. seq count
                int seqCount = nt.SeqCount;
                writer.Write(seqCount);
                for (int s = 0; s < seqCount; ++s)
                {
                    SymbolSequence ss = nt.GetSequence(s);
                    //4.  seq number
                    writer.Write(ss.TotalSeqNumber);

                    int expSymCount = ss.RightSideCount;
                    //5. exp symbol count
                    writer.Write(expSymCount);
                    for (int sc = 0; sc < expSymCount; ++sc)
                    {

                        UserExpectedSymbol usymbol = ss.GetOriginalUserExpectedSymbol(sc);
                        //6. exp symbol kind
                        writer.Write((int)usymbol.SymbolKind);
                        switch (usymbol.SymbolKind)
                        {
                            case UserExpectedSymbolKind.Nonterminal:
                                {
                                    //6.1
                                    writer.Write(usymbol.SymbolString);
                                    writer.Write(usymbol.ResolvedNt.NtId);
                                }
                                break;
                            case UserExpectedSymbolKind.Terminal:
                                {
                                    //6.2 
                                    writer.Write(usymbol.SymbolString);
                                    writer.Write(usymbol.ResolvedTokenDefinition.TokenInfoNumber);
                                }
                                break;
                            case UserExpectedSymbolKind.UnknownNT:
                                {
                                    //6.3
                                    writer.Write(usymbol.SymbolString);
                                    writer.Write(0);
                                } break;
                            default:
                                throw new NotSupportedException();
                        }
                    }
                }
            }


            writer.Write(END_MARKER);//write end header marker
        }
        public bool SuccessLoaded
        {
            get;
            private set;
        }

        internal int TableRowCount
        {
            get;
            private set;
        }
        public bool CompareWithTable(LRParsingTable compareToTable)
        {
            //md5 check header with  compare table
            byte[] comp_md5HashValue = null;
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms))
            {

                //write header only 
                //and test with md5 ***
                WriteParserTableHeader(compareToTable, writer);
                writer.Flush();


                ms.Position = 0;
                byte[] headerBuffer = ms.ToArray();

                // Convert the input string to a byte array and compute the hash.                
                comp_md5HashValue = md5Hash.ComputeHash(headerBuffer, 0, headerBuffer.Length);
                ms.Close();
            }

            for (int i = comp_md5HashValue.Length - 1; i >= 0; --i)
            {
                if (this._md5Stamp[i] != comp_md5HashValue[i])
                {
                    //not match 
                    return false;
                }
            }
            return true;
        }
        public void LoadFromBinary(string filename)
        {

            SuccessLoaded = false;

            try
            {
                if (!File.Exists(filename))
                {
                    return;
                }

                using (FileStream fs = new FileStream(filename, FileMode.Open))
                using (BinaryReader reader = new BinaryReader(fs, Encoding.UTF8))
                {

                    tokenDefs = new List<IntermeidateTokenDef>();
                    nts = new List<IntermediateNt>();

                    //table columns tk and nt defintion
                    int tkcolCount = reader.ReadInt32();
                    for (int i = 0; i < tkcolCount; ++i)
                    {
                        IntermeidateTokenDef tokendef = new IntermeidateTokenDef();
                        tokendef.tokenInfoNumber = reader.ReadInt32();
                        tokendef.symbolName = reader.ReadString();
                        tokendef.presentationString = reader.ReadString();
                        tokenDefs.Add(tokendef);
                    }
                    //--------------------------------------------
                    //nt 
                    int ntCount = reader.ReadInt32();
                    for (int i = 0; i < ntCount; ++i)
                    {
                        IntermediateNt ntdef = new IntermediateNt();
                        ntdef.ntId = reader.ReadInt32();
                        ntdef.symbolName = reader.ReadString();
                        nts.Add(ntdef);
                        //---------
                        //seq 
                        int seqCount = reader.ReadInt32();
                        for (int s = 0; s < seqCount; ++s)
                        {

                            //seq number
                            IntermediateSeq seq = new IntermediateSeq();
                            seq.Number = reader.ReadInt32();
                            ntdef.seqs.Add(seq);

                            //ss component count
                            int expSymCount = reader.ReadInt32();

                            for (int sc = 0; sc < expSymCount; sc++)
                            {
                                IntermediateSymbol sym = new IntermediateSymbol();
                                sym.SymbolKind = reader.ReadInt32();
                                sym.symbolName = reader.ReadString();
                                sym.symbolId = reader.ReadInt32();

                                seq.AddSymbol(sym);
                            }
                        }
                    }
                    int endHeader = reader.ReadInt32();
                    if (endHeader == END_MARKER)
                    {
                        //--------------------------------------------
                        //read md5 stamp 16 bytes
                        int hashSize = reader.ReadInt32();
                        _md5Stamp = reader.ReadBytes(16);
                        //--------------------------------------------

                        //unresolve switch table
                        swDetails = new List<SubParsers.SwitchDetail>();
                        swDetails.Add(null); //1st record is null
                        //-------------------------------------------- 
                        //record sw detail 
                        List<string> strTable = ReadStringTable(reader);
                        int swRecordCount = reader.ReadInt32();//all record count include index 0
                        for (int rr = 1; rr < swRecordCount; ++rr)
                        {
                            //start rr at 1
                            SubParsers.SwitchDetail swDetail = new SubParsers.SwitchDetail(rr);
                            swDetails.Add(swDetail);
                            int pairCount = reader.ReadInt32();
                            for (int p = 0; p < pairCount; ++p)
                            {
                                //target name index                             
                                swDetail.AddChoice(new SubParsers.SwitchPair(strTable[reader.ReadInt32()], reader.ReadInt32()));
                            }
                        }
                        //--------------------------------------------
                        //lr parsing table data  
                        //-------------------------------------------- 
                        todoListForTks = new List<List<LRItemTodo>>(tkcolCount);
                        todoListForNts = new List<List<LRItemTodo>>(ntCount);
                        int rowCount = reader.ReadInt32();
                        this.TableRowCount = rowCount;

                        bool load_OK = true;
                        for (int c = 0; c < tkcolCount; ++c)
                        {
                            List<LRItemTodo> list = new List<LRItemTodo>(rowCount);
                            todoListForTks.Add(list);
                            for (int r = 0; r < rowCount; ++r)
                            {

                                int todo1 = reader.ReadInt32();
                                int todo2 = 0;
                                switch ((LRItemTodoKind)(todo1 & 0xFF))
                                {
                                    case LRItemTodoKind.Shift:
                                        todo2 = reader.ReadInt32();
                                        break;
                                }

                                list.Add(LRItemTodo.InternalCreateLRItem(todo1 >> 8,
                                        (LRItemTodoKind)(todo1 & 0xFF),
                                        todo2 >> 8,
                                        todo2 & 0xFF));
                            }
                            int endMarker = reader.ReadInt32();
                            if (endMarker != END_MARKER)
                            {
                                break;
                            }
                        }

                        for (int c = 0; c < ntCount; ++c)
                        {
                            List<LRItemTodo> list = new List<LRItemTodo>(rowCount);
                            todoListForNts.Add(list);
                            for (int r = 0; r < rowCount; ++r)
                            {
                                int todo1 = reader.ReadInt32();
                                int todo2 = 0;
                                switch ((LRItemTodoKind)(todo1 & 0xFF))
                                {
                                    case LRItemTodoKind.Shift:
                                        todo2 = reader.ReadInt32();
                                        break; 
                                } 
                                list.Add(LRItemTodo.InternalCreateLRItem(todo1 >> 8,
                                        (LRItemTodoKind)(todo1 & 0xFF),
                                        todo2 >> 8,
                                        todo2 & 0xFF));
                            }
                            int endMarker = reader.ReadInt32();
                            if (endMarker != END_MARKER)
                            {
                                break;
                            }
                        } 
                        if (load_OK)
                        {
                            SuccessLoaded = true;
                        } 
                    }
                    else
                    {

                    }

                    reader.Close();
                    fs.Close();
                }
            }
            catch (Exception ex)
            {
            }

            if (!SuccessLoaded)
            {
                throw new NotSupportedException();
            }

        }
        static List<string> ReadStringTable(BinaryReader reader)
        {
            int strCount = reader.ReadInt32();
            List<string> list = new List<string>(strCount);
            for (int i = 0; i < strCount; ++i)
            {
                list.Add(reader.ReadString());
            }
            return list;
        } 

        class IntermeidateTokenDef
        {
            public int tokenInfoNumber;
            public string symbolName;
            public string presentationString;
#if DEBUG
            public override string ToString()
            {
                return symbolName;
            }
#endif
        }
        class IntermediateNt
        {
            public int ntId;
            public string symbolName;
            public List<IntermediateSeq> seqs = new List<IntermediateSeq>();
#if DEBUG
            public override string ToString()
            {
                return symbolName;
            }
#endif
        }
        class IntermediateSeq
        {
            List<IntermediateSymbol> ssList = new List<IntermediateSymbol>();
            public int Number;
            public void AddSymbol(IntermediateSymbol sym)
            {
                ssList.Add(sym);
            }
#if DEBUG
            public override string ToString()
            {
                StringBuilder stbuilder = new StringBuilder();
                int j = ssList.Count;
                for (int i = 0; i < j; ++i)
                {
                    stbuilder.Append(ssList[i].ToString());
                    if (i < j - 1)
                    {
                        stbuilder.Append(' ');
                    }
                }

                return stbuilder.ToString();
            }
#endif
        }
        class IntermediateSymbol
        {
            public string symbolName;
            public int SymbolKind;
            public int symbolId;

#if DEBUG
            public override string ToString()
            {
                return symbolName;
            }
#endif
        }


        static Dictionary<string, int> CreateStringTable(List<SubParsers.SwitchDetail> swDetails)
        {
            Dictionary<string, int> dic = new Dictionary<string, int>();
            dic.Add("", 0);
            int j = swDetails.Count;
            for (int i = 1; i < j; ++i)
            {
                SubParsers.SwitchDetail swDetail = swDetails[i];
                //start at 1
                int n = swDetail.Count;
                for (int m = 0; m < n; ++m)
                {
                    SubParsers.SwitchPair p = swDetail.GetSwPair(m);
                    int found;
                    if (!dic.TryGetValue(p.symbolName, out found))
                    {
                        found = dic.Count;
                        dic.Add(p.symbolName, found);
                    }
                }
            }

            return dic;
        }
    }

    public static class LRTableReaderWriter
    {

        internal static ParserDataBinaryCache LoadTableDataFromBinaryFile(string filename)
        {
            ParserDataBinaryCache cache = new ParserDataBinaryCache();
            cache.LoadFromBinary(filename);
            return cache;
        }

        public static void SaveAsBinaryFile(LRParsingTable parsingTable, string filename)
        {
            ParserDataBinaryCache.SaveToBinary(parsingTable, filename);
        }

      
        
        public static void SaveAsTextFile(LRParsingTable parsingTable, string filename)
        {

            ColumnBasedTable<TokenDefinition, LRItemTodo> tkTable = parsingTable.tokenTable;
            ColumnBasedTable<NTDefinition, LRItemTodo> ntTable = parsingTable.ntTable;


            int rowCount = ntTable.RowCount;
            int ntColumnCount = ntTable.columns.Count;
            int tkColumnCount = tkTable.columns.Count;

            StringBuilder stbuilder = new StringBuilder();
            for (int r = 0; r < rowCount; ++r)
            {
                //each col
                for (int c = 0; c < tkColumnCount; ++c)
                {
                    LRItemTodo todo = tkTable.GetCell(r, c);
                    if (todo.IsEmpty()) continue;
                    //----------------------------
                    TokenDefinition tkdef = tkTable.columns[c].columnHeader;
                    stbuilder.Append("t" + tkdef.TokenInfoNumber);
                    stbuilder.Append(':');
                    WriteTodoAsText(stbuilder, todo);
                    stbuilder.Append(',');
                }
                stbuilder.Append(',');
                for (int c = 0; c < ntColumnCount; ++c)
                {
                    LRItemTodo todo = ntTable.GetCell(r, c);
                    if (todo.IsEmpty()) continue;
                    //----------------------------
                    NTDefinition ntdef = ntTable.columns[c].columnHeader;
                    stbuilder.Append("nt" + ntdef.NtId);
                    stbuilder.Append(':');
                    WriteTodoAsText(stbuilder, todo);
                    stbuilder.Append(',');
                }

                if (r < rowCount - 1)
                {
                    stbuilder.AppendLine();
                }
            }

            using (System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Create))
            {
                byte[] output = Encoding.UTF8.GetBytes(stbuilder.ToString());
                fs.Write(output, 0, output.Length);
                fs.Close();
            }
        }
        public static void SaveAsHtmlFile(LRParsingTable parsingTable, string filename)
        {

            var ntTable = parsingTable.ntTable;
            var tkTable = parsingTable.tokenTable;

            int rowCount = ntTable.RowCount;
            int ntColumnCount = ntTable.columns.Count;
            int tkColumnCount = tkTable.columns.Count;

            StringBuilder stbuilder = new StringBuilder();
            //header
            stbuilder.Append("<html><head></head><body>");
            stbuilder.Append("<table>");
            //first row for column name
            //--------------------------------------------------------------------------

            {
                stbuilder.Append("<tr>");
                for (int c = 0; c < tkColumnCount; ++c)
                {
                    TokenDefinition tk = tkTable.columns[c].columnHeader;
                    stbuilder.Append("<td>");
                    stbuilder.Append(tk.SymbolName);
                    stbuilder.Append("</td>");
                }

                for (int c = 0; c < ntColumnCount; ++c)
                {
                    NTDefinition nt = ntTable.columns[c].columnHeader;
                    stbuilder.Append("<td>");
                    stbuilder.Append(nt.SymbolName);
                    stbuilder.Append("</td>");
                }
                stbuilder.Append("</tr>");
            }
            //--------------------------------------------------------------------------

            for (int r = 0; r < rowCount; ++r)
            {
                stbuilder.Append("<tr>");
                for (int c = 0; c < tkColumnCount; ++c)
                {

                    stbuilder.Append("<td>");
                    WriteTodoAsText(stbuilder, tkTable.GetCell(r, c));
                    stbuilder.Append("</td>");
                }
                for (int c = 0; c < ntColumnCount; ++c)
                {

                    stbuilder.Append("<td>");
                    WriteTodoAsText(stbuilder, ntTable.GetCell(r, c));
                    stbuilder.Append("</td>");
                }


                stbuilder.Append("</tr>");
            }
            stbuilder.Append("</table></body></html>");

            using (System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Create))
            {
                byte[] output = Encoding.UTF8.GetBytes(stbuilder.ToString());
                fs.Write(output, 0, output.Length);
                fs.Close();
            }
        }

        //--------------------------------------------------------------------------------
        static void WriteTodoAsText(StringBuilder stbuilder, LRItemTodo todo)
        {

            switch (todo.ItemKind)
            {
                case LRItemTodoKind.Accept:
                    {
                        stbuilder.Append("a" + todo.StateNumber);
                    } break;
                case LRItemTodoKind.Err:
                    {
                        stbuilder.Append("e" + todo.StateNumber);
                    } break;
                case LRItemTodoKind.Goto:
                    {
                        stbuilder.Append("g" + todo.StateNumber);
                    } break;
                case LRItemTodoKind.Reduce:
                    {
                        stbuilder.Append("r" + todo.StateNumber);
                    } break;
                case LRItemTodoKind.Shift:
                    {
                        stbuilder.Append("s" + todo.StateNumber);
                    } break;
                case LRItemTodoKind.UnresolvedSwitch:
                    {
                        stbuilder.Append("u_sw" + todo.StateNumber);
                    } break;
                case LRItemTodoKind.ResolveSwitch:
                    {
                        stbuilder.Append("sw" + todo.StateNumber);
                    } break;
                case LRItemTodoKind.Empty:
                    {

                    } break;
                default:
                    {
                        stbuilder.Append("u" + todo.StateNumber);
                        throw new NotSupportedException();
                    } break;
            }
        }

    }


    
}