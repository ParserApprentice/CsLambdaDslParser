//MIT 2015-2017, ParserApprentice 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Parser.AsmInfrastructures
{


    public class AsmIndentTextWriter
    {

        StringBuilder stBuilder;
        int indentLevel;
        const string defaultTabString = "\t";
        string newLine = "\r\n";

        public AsmIndentTextWriter(StringBuilder stBuilder)
        {
            this.stBuilder = stBuilder;
        }
        public StringBuilder InnterStringBuilder
        {
            get
            {
                return this.stBuilder;
            }
        }
        public void Append(string str)
        {
            stBuilder.Append(str);
        }
        public void Append(char c)
        {
            stBuilder.Append(c);
        }

        public void AppendDoubleColonWrap(string str)
        {
            stBuilder.Append('"');
            stBuilder.Append(str);
            stBuilder.Append('"');
        }

        public void OutputTabs()
        {
            for (int i = 0; i < indentLevel; i++)
            {
                stBuilder.Append(defaultTabString);
            }
        }

        public void CloseLine()
        {
            stBuilder.Append(newLine);
            OutputTabs();
        }

        public void CloseLine(string str)
        {

            stBuilder.Append(str);
            stBuilder.Append(newLine);
            OutputTabs();
        }

        public void CloseLineFinal(string str)
        {
            stBuilder.Append(str);

        }

        public void CloseLine(char c)
        {

            stBuilder.Append(c);
            stBuilder.Append(newLine);
            OutputTabs();
        }

        public void CloseLineNoTab(string str)
        {
            stBuilder.Append(str);
            stBuilder.Append(newLine);
        }
        public void CloseLineNoTab()
        {
            stBuilder.Append(newLine);
        }
        public void CloseLineNoTab(char c)
        {
            stBuilder.Append(c);
            stBuilder.Append(newLine);
        }
        public string NewLine
        {
            get
            {
                return newLine;
            }
            set
            {
                newLine = value;
            }
        }
        public int IndentLevel
        {
            get
            {
                return indentLevel;
            }
            set
            {
                indentLevel = value;
            }
        }


    }

}