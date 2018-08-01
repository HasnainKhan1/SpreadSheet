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

namespace SpreadsheetEngine
{
    //command interface for execution and unexecution methods
    public interface IInvertibleCmd
    {
        void Exec(Object obj);
        void UnExec(Object obj);
    }
    //retore text class to undo or redo a text in spreadsheet
    public class restoreText : IInvertibleCmd
    {
        private int row;
        private int column;
        string undoText, redoText;
        //constructor
        public restoreText(Cell c)
        {
            row = c.rowIndex;
            column = c.columnIndex;
            if (c.newCellText != null)
            {
                undoText = string.Copy(c.newCellText);
            }
            else
            {
                undoText = null;
            }
        }

        //execute function that redo the the cell text
        public void Exec(object ss)
        {
            Spreadsheet redo = (Spreadsheet)ss;
            //get current cell
            Cell redoCell = redo.GetCell(row, column);
            //change the cell text property
            redoCell.newCellText = redoText;
        }
        //Unexec function that undo the cell text
        public void UnExec( object ss)
        {
            Spreadsheet undo = (Spreadsheet)ss;
            Cell undoCell = undo.GetCell(row, column);
            //if redo is null then set the redo text equal to undo text
            if(redoText == null && undoCell.newCellText != null)
            {
                redoText = string.Copy(undoCell.newCellText);
            }
            else
            {
                redoText = null;
            }
            undoCell.newCellText = undoText;
        }
    }
    //class that execute undo and redo of color of a cell
    public class restoreBGColor : IInvertibleCmd
    {
        private int row;
        private int column;
        uint undoColor, redoColor;
        //constructor
        public restoreBGColor(Cell c)
        {
            row = c.rowIndex;
            column = c.columnIndex;
            undoColor = c.BgColor;
        }
        //execute redo for cell color
        public void Exec(object ss)
        {
            Spreadsheet redo = (Spreadsheet)ss;
            //get current cell
            Cell redoCell = redo.GetCell(row, column);
            redoCell.BgColor = redoColor; 
        }
        //execute undo of cell color
        public void UnExec(object ss)
        {
            Spreadsheet undo = (Spreadsheet)ss;
            Cell undoCell = undo.GetCell(row, column);
            redoColor = undoCell.BgColor;
            undoCell.BgColor = undoColor;
        }
    }
    //class that execute multiple undo and redo
    public class MultiCmd : IInvertibleCmd
    {
        public string text
        {
            get;
            set;
        }
        //store commands in the list
        List<IInvertibleCmd> cmds;
        public MultiCmd(string t, List<IInvertibleCmd> newCmds)
        {
            cmds = newCmds;
            text = t;
        }
        public void Exec(object ss)
        {
            foreach(IInvertibleCmd cmd in cmds)
            {
                cmd.Exec(ss);
            }
        }
        public void UnExec(object ss)
        {
            foreach (IInvertibleCmd cmd in cmds)
            {
                cmd.UnExec(ss);
            }
        }
    }
}
