//MIT, 2015-2017, ParserApprentice

using System;
using System.Text;
using System.Collections.Generic;

namespace Parser.ParserKit
{

    public class ColumnBasedTable<T, U>
    {

        internal List<TableColumn<T, U>> columns = new List<TableColumn<T, U>>();
        Dictionary<T, int> columnPositions = new Dictionary<T, int>();
        int nColumns;
        int rowCount;
        int colCount;
        public ColumnBasedTable()
        {
        }
        public void AddColumn(T sym)
        {
            columns.Add(new TableColumn<T, U>(colCount, sym));
            columnPositions.Add(sym, colCount);
            colCount++;
        }

        public void FinishColumnsDefinition()
        {
            this.nColumns = columns.Count;
        }

        public void AppendNewRow(U initData)
        {

            rowCount++;
            //add all cell for newRow to each column
            for (int i = nColumns - 1; i >= 0; --i)
            {
                //create new cell
                columns[i].AddNewBlankCell(initData);
            }
        }
        public int RowCount
        {
            get { return rowCount; }
        }

        public void SetCell(int rowIndex, int colIndex, U celldata)
        {
            this.columns[colIndex].SetTableCell(rowIndex, celldata);
        }
        public U GetCell(int rowIndex, int colIndex)
        {
            return this.columns[colIndex].GetTableCell(rowIndex);
        }

        internal void Clear()
        {
            this.columns.Clear();
            this.columns = null;
            this.columnPositions.Clear();
            this.columnPositions = null;
        }

        internal void UnsafeRefreshRowCount(int rowCount)
        {
            this.rowCount = rowCount;
        }


    }

    public abstract class TableColumnBase
    {
    }

    public class TableColumn<T, U> : TableColumnBase
    {
        public readonly int ColumnIndex;
        public readonly T columnHeader;
        internal List<U> cells = new List<U>();


        public TableColumn(int colIndex, T symbolDefinition)
        {
            this.columnHeader = symbolDefinition;
            this.ColumnIndex = colIndex;
        }
        public void AddNewBlankCell(U initValue)
        {
            cells.Add(initValue);
        }
        public U GetTableCell(int rowIndex)
        {
            return this.cells[rowIndex];
        }
        public void SetTableCell(int rowIndex, U data)
        {
            cells[rowIndex] = data;
        }
#if DEBUG
        public override string ToString()
        {
            return ColumnIndex + " " + columnHeader.ToString();
        }
#endif
    }

}