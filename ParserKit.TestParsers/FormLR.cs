//MIT 2015-2017, ParserApprentice 
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using System.Windows.Forms;
using Parser.ParserKit;
using Parser.ParserKit.LR;
using Parser.ParserKit.Lexers;
using Parser.MyCs;

namespace ParserKit.TestParsers
{

    public partial class FormLR : Form
    {
        public FormLR()
        {
            InitializeComponent();
        }
        private void button16_Click(object sender, EventArgs e)
        {
            Lang01 lang1 = new Lang01();
            lang1.TestLR_CS1B();
        }
        private void button17_Click(object sender, EventArgs e)
        {
            Lang01 lang1 = new Lang01();
            lang1.TestLR_CS2B();
        }
        private void button18_Click(object sender, EventArgs e)
        {
            Lang01 lang1 = new Lang01();
            lang1.TestLR_CS3B();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Lang01 lang1 = new Lang01();
            lang1.TestLR_SimpleArgsB2();
        }



        void TestA(object a, object b)
        {
        }






        SampleTestFileInfo currentSelectedSampleFile = null;

        class SampleTestFileInfo
        {


            public SampleTestFileInfo(string fullname)
            {
                this.Fullname = fullname;
                this.OnlyFileName = System.IO.Path.GetFileName(this.Fullname);
            }
            public string OnlyFileName
            {
                get;
                private set;
            }
            public string Fullname
            {
                get;
                private set;
            }
            public override string ToString()
            {
                return this.OnlyFileName;
            }
        }

        private void FormLR_Load(object sender, EventArgs e)
        {
            //load test file if exists
            string[] filenames = System.IO.Directory.GetFiles(@"..\..\..\TestCase");
            int j = filenames.Length;
            this.listBox1.Items.Clear();
            for (int i = 0; i < j; ++i)
            {
                this.listBox1.Items.Add(new SampleTestFileInfo(filenames[i]));
            }

            this.listBox1.Click += (s, e2) =>
            {
                currentSelectedSampleFile = this.listBox1.SelectedItem as SampleTestFileInfo;
                if (currentSelectedSampleFile != null)
                {


                    this.richTextBox1.Text = System.IO.File.ReadAllText(currentSelectedSampleFile.Fullname);
                }

            };
        }



        private void button23_Click(object sender, EventArgs e)
        {
            //MyLexer lexer2 = new MyLexer();
            //lexer2.AddTokenDefs(new TokenDef[]{
            //    new TokenDef("inta"){IsKeyword = true},
            //    new TokenDef("("),
            //    new TokenDef(")"),
            //    new TokenDef("+"),
            //    new TokenDef("++"),
            //});
            ////lexer2.SetWhitespaceChars(" \t\r\n".ToCharArray());
            ////lexer2.SetTerminalTokens(" +-*./\0\"'\t\r\n{}()".ToCharArray());
            //lexer2.SetWhitespaceChars(" ".ToCharArray());

            ////set terminal for iden mode (to stop from iden mode)
            //lexer2.SetTerminalTokens(" \r\n(){}[].".ToCharArray());
            //lexer2.MakeTable();

            ////string teststr = ".12\t\r\n abstract public1 12345 okok\0";
            ////string teststr = "\"ok\"123456 'a' 'b' 'c' \0";
            //string teststr = "(inta)+++\0";
            //lexer2.SetLexEventListener(lexEvent =>
            //{
            //    if (lexEvent.fromLexState == LexToDoState.Keyword)
            //    {

            //    }
            //    Console.WriteLine(lexEvent.GetLexString());
            //});


            //lexer2.Lex(teststr.ToCharArray());
        }


        private void button25_Click(object sender, EventArgs e)
        {
            Lang01 lang1 = new Lang01();
            lang1.TestLR0_3_4();
        }

        private void button26_Click(object sender, EventArgs e)
        {
            Lang01 lang1 = new Lang01();
            lang1.TestLR0_3_5();
        }



        private void button27_Click(object sender, EventArgs e)
        {
            Lang01 lang1 = new Lang01();
            lang1.TestLR0_3_6();
        }

        private void button28_Click(object sender, EventArgs e)
        {
            Lang01 lang1 = new Lang01();
            lang1.TestLR_SimpleArgsB();
        }

        private void button29_Click(object sender, EventArgs e)
        {
            Lang01 lang1 = new Lang01();
            lang1.TestLR_SimpleArgsB3();
        }

        private void button30_Click(object sender, EventArgs e)
        {
            Lang01 lang1 = new Lang01();
            lang1.TestLR0_3_5_2();
        }

        private void button31_Click(object sender, EventArgs e)
        {
            Lang01 lang1 = new Lang01();
            lang1.TestLR_SimpleArgsB4();
        }

        private void button32_Click(object sender, EventArgs e)
        {
            Lang01 lang1 = new Lang01();
            lang1.TestLR_SimpleArgsB5();
        }

        private void button33_Click(object sender, EventArgs e)
        {

            Lang01 lang1 = new Lang01();
            lang1.TestLR_SimpleArgsB6();
        }

        private void button34_Click(object sender, EventArgs e)
        {
            Lang01 lang1 = new Lang01();
            lang1.TestLR_SimpleArgsB7();
        }

        private void button35_Click(object sender, EventArgs e)
        {
            Lang01 lang1 = new Lang01();
            lang1.TestLR_SimpleArgsB8();
        }

        private void button36_Click(object sender, EventArgs e)
        {
            Lang01 lang1 = new Lang01();
            lang1.TestLR_SimpleArgsB9();
        }

        private void button37_Click(object sender, EventArgs e)
        {
            Lang01 lang1 = new Lang01();
            lang1.TestLR_SimpleArgsB10();
        }

        private void button38_Click(object sender, EventArgs e)
        {
            Lang01 lang1 = new Lang01();
            lang1.TestLR_SimpleArgsB11();
        }

        private void button39_Click(object sender, EventArgs e)
        {
            Lang01 lang1 = new Lang01();
            lang1.TestLR_SimpleArgsB12();
        }

        private void button40_Click(object sender, EventArgs e)
        {
            Lang01 lang1 = new Lang01();
            lang1.TestLR_SimpleArgsB13();
        }

        private void button41_Click(object sender, EventArgs e)
        {
            Lang01 lang1 = new Lang01();
            lang1.TestLR_SimpleArgsB14();
        }

        private void button42_Click(object sender, EventArgs e)
        {
            Lang01 lang1 = new Lang01();
            lang1.TestLR_SimpleArgsB15();
            this.Text = lang1.lastestAvg.ToString();
        }

        private void button43_Click(object sender, EventArgs e)
        {
            Lang01 lang1 = new Lang01();
            lang1.TestLexer1();



            //Test1();
            //Test2();

        }
        static void Test1()
        {
            System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();
            //Stack<SymbolSequence> ss = new Stack<SymbolSequence>();

            MyStack ss = new MyStack();

            GC.Collect();
            st.Start();
            for (int i = 10000000; i >= 0; --i)
            {
                ss.Push(null);
                ss.Push(null);
                ss.Push(null);
                ss.Push(null);
                ss.Push(null);

                ss.Pop();
                ss.Pop();
                ss.Pop();
                ss.Pop();
                ss.Pop();

            }
            st.Stop();
            //long a = st.ElapsedMilliseconds;
            Console.WriteLine("my_stack" + st.ElapsedMilliseconds.ToString());
        }
        static void Test2()
        {
            System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();


            Stack<SymbolSequence> ss = new Stack<SymbolSequence>();
            //MyStack ss = new MyStack();

            GC.Collect();

            st.Start();
            for (int i = 10000000; i >= 0; --i)
            {
                ss.Push(null);
                ss.Push(null);
                ss.Push(null);
                ss.Push(null);
                ss.Push(null);

                ss.Pop();
                ss.Pop();
                ss.Pop();
                ss.Pop();
                ss.Pop();

            }
            st.Stop();
            //long a = st.ElapsedMilliseconds;
            Console.WriteLine("system" + st.ElapsedMilliseconds.ToString());
        }
        class MyStack
        {
            SymbolSequence[] arr = new SymbolSequence[32];
            int index = 0;
            public void Push(SymbolSequence ss)
            {
                arr[index++] = ss;

            }
            public void Pop()
            {
                index--;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //generate test case
            StringBuilder stbuilder = new StringBuilder();
            stbuilder.AppendLine("using X;");
            stbuilder.AppendLine("namespace A");
            stbuilder.AppendLine("{");

            int startAt = 100;
            int n = 300;
            for (int i = startAt; i < n; ++i)
            {
                //300 sample
                WriteTestClass(stbuilder, i);
            }
            stbuilder.AppendLine("}");
            //save to file
            System.IO.File.WriteAllText(@"D:\projects\px02\cs_inputtest\cs_02_" + (n - startAt) + ".cs", stbuilder.ToString());

        }
        static void WriteTestClass(StringBuilder stbuilder, int prefixNumber)
        {
            string[] testCase1 = new string[]{

                    "public class B" + prefixNumber,
                    "{",
                        "public void M(){",
                            " var a =1+ 2*3;",
                            " var b = new { x = 20, y = 30 };",
                            " var c = 2;",
                            " if (true) { return; } else { return; }",
                        "}",
                    "}",
            };
            foreach (string s in testCase1)
            {
                stbuilder.AppendLine(s);
            }

        }



        class txtAttribute : Attribute
        {

            public txtAttribute(string grammarString)
            {
                this.GrammarString = grammarString;
            }
            public string GrammarString { get; set; }
        }
        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            string teststr = System.IO.File.ReadAllText(@"D:\projects\px02\cs_inputtest\cs_02_200.cs");
            char[] codeBuffer = teststr.ToCharArray();
            //-------------------------------------------             

            Parser.ParserKit.SubParsers.BreakMode._shouldBreakOnLambda = ShouldStopOnLambda;
            //------------------------------------------- 

            CsParserHelper csParserHelper = new CsParserHelper();
            ParseNode finalNode = csParserHelper.Parse(codeBuffer);
        }
        static bool ShouldStopOnLambda(AstWalker walker, string argName)
        {
            return true;
        }


        static CsParseNodeHolder CreateCsParseNodeHolderForAst()
        {
            CsParseNodeHolder holder = new CsParseNodeHolder();
            holder.ClassWalker = new ClassBuilder();
            holder.NamespaceWalker = new NamespaceBuilder();
            holder.StatementBuilder = new StatementBuilder();
            holder.ArrayTypeWalker = new ArrayTypeBuilder();
            holder.ExpressionWalker = new ExpressionBuilder();
            holder.TypeBuilder = new TypeBuilder();
            return holder;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Lang01 lang1 = new Lang01();
            lang1.TestLR_SimpleArgsB16();
            this.Text = lang1.lastestAvg.ToString();
        }
    }

}
