/*
* Name: Hasnain Mazhar
* Instructor: Even Olds
* Class: CPTS 321 
* Homework 10
* Date: 03/31/2017
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SpreadsheetEngine
{
    //abstract base class that handles just one cell
    public abstract class Cell : INotifyPropertyChanged
    {
        public readonly int columnIndex;
        public readonly int rowIndex;
        protected string cellText;
        protected string value;
        protected uint BGColor;
        public List<Cell> listOfCells;
        //protected uint BGColor;
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        //protected abstract void NotifyPropertyChanged([CallerMemberName] String PropertyName = "");
        //constructor that initializes row index and column index
        public Cell(int row, int col)
        {
            columnIndex = col;
            rowIndex = row;
            listOfCells = new List<Cell>();
            PropertyChanged = null;
            cellText = null;
            value = null;
            BGColor = 0xFFFFFFFF;
        }
        //get column index
        public int getColumn
        {
            get
            { return columnIndex; }
        }
        //get row index
        public int getRow
        {
            get { return rowIndex; }
        }
        //evaluated value of a cell
        public string cellValue
        {
            get { return value; }
        }
        //text inside the cell
        public string newCellText
        {
            get { return cellText; }
            set
            {

                if (cellText == value)
                {
                    return;
                }
                //call OnPropertyChange if changing the value of cell
                cellText = value;
                OnPropertyChanged("cellText");
            }
        }
        public uint BgColor
        {
            get { return BGColor; }
            set
            {
                if (BGColor == value)
                {
                    return;
                }
                BGColor = value;
                OnPropertyChanged("BGColor");
            }
        }
        // Create the OnPropertyChanged method to raise the event
        public void OnPropertyChanged(string text = "")
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(text));
            }
        }
        public void OnVariableChange(object sender, PropertyChangedEventArgs e)
        {
            Cell caller = (Cell)sender;

            if (!caller.value.StartsWith("!"))
            {
                caller.PropertyChanged -= this.OnVariableChange;
                OnPropertyChanged("cellText");
            }
        }

        public void addReferences(Cell c)
        {
            listOfCells.Add(c);
        }
    }


}
