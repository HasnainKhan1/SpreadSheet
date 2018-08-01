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
using System.Xml.Serialization;
using System.Xml;
using System.IO;

namespace SpreadsheetEngine
{
    public class Spreadsheet
    {

        public SpreadsheetCell[,] SS_Array;
        private Stack<MultiCmd> Undos;
        private Stack<MultiCmd> Redos;
        public int NumberOfRows { get; }
        public int NumberOfColumns { get; }
        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangedEventHandler CmdPropertyChanged;
        //The spreadsheet object serve as a container for a 2D array of cells
        public class SpreadsheetCell : Cell
        {
            public SpreadsheetCell(int row, int col) : base(row, col)
            {}
            public string Value
            {
                get { return value; }
                set { this.value = value; }
            }
        }

        //a constructor for the spreadsheet class that takes a number of rows and columns. 
        public Spreadsheet(int newRow, int newColumn)
        {
            NumberOfRows = newRow;
            NumberOfColumns = newColumn;
            SS_Array = new SpreadsheetCell[NumberOfRows, NumberOfColumns];
            Undos = new Stack<MultiCmd>();
            Redos = new Stack<MultiCmd>();
            //go through the 2d array and subscribe to property change event handler
            for (int i = 0; i < NumberOfRows; i++)
            {
                for(int j = 0; j < NumberOfColumns; j++)
                {
                    SS_Array[i, j] = new SpreadsheetCell(i, j);
                    SS_Array[i, j].PropertyChanged += CellPropertyChanged;
                }
            }
        }
        // GetCell function that takes a row and column index and returns the cell at that location
        //or null if there is no such cell
        public Cell GetCell(int row, int col)
        {
            if(SS_Array[row, col].cellValue != null)
            {
                return SS_Array[row, col];
            }
            return null;
        }

        // CellPropertyChanged event in the spreadsheet class. This will serve as a way for the
        //outside world(like UI stuff) to subscribe to a single event that lets them know when any
        //property for any cell in the worksheet has changed.
        private void CellPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SpreadsheetCell changedCell = sender as SpreadsheetCell;
            if (e.PropertyName != null)
            {
                if (changedCell.newCellText == null)
                {
                    changedCell.Value = null;
                }
                else if (!(changedCell.newCellText.StartsWith("=")))
                {
                    changedCell.Value = changedCell.newCellText;
                }
                else
                {
                    UpdateCellValue(changedCell);
                }
            }
        }

        //function that evaluates the expression inside the cell and returns the value as double
        private void UpdateCellValue(SpreadsheetCell c)
        {
            //get the expression inside the cell
            string expression = c.newCellText;
            string cellName = Number2String(c.getColumn + 1, true);
            cellName += (c.getRow+1).ToString();
            string vari = "";
            //parse the string in the exptree class taking out the = sign in the front of exp
            ExpTree tree = new ExpTree(expression.Substring(1));
            //delete thr reference cells
            c.listOfCells.Clear();
            //get all the variables inside the dictionary
            string[] variables = tree.getVar();
            foreach (string variable in variables)
            {
                if (variable.Length > 2)
                {
                    vari = variable;
                    break;
                }
                //all the columns are marked from A to Z so we have to convert that to int
                int col = Convert.ToInt32(variable[0] - 65);
                // remaining part is the name of the cell we need to copy a value from
                //to convert that
                int row = Convert.ToInt32(variable.Substring(1)) - 1;
                //add it to the reference cell list
                c.addReferences(SS_Array[row, col]);
                if (string.IsNullOrEmpty(SS_Array[row,col].newCellText))
                {
                    c.newCellText = "0";
                    c.Value = "0";
                    break;
                }                
                if (variable == cellName)
                {
                    vari = variable;
                    break;
                }
                tree.SetVar(variable, double.Parse(SS_Array[row, col].cellValue));
                SS_Array[row, col].PropertyChanged += c.OnVariableChange;
            }
            if (vari == cellName)
            {
                c.newCellText = "!(Self/Circular reference)";
                c.Value = "!(Self/Circular reference)";
            }
            else if (vari.Length > 3)
            {
                c.newCellText = "!(Bad reference)";
                c.Value = "!(Bad reference)";
            }
            else
            {
                //evaluate the expression
                double newVal = tree.Eval();
                c.Value = newVal.ToString();
            }
        }

        public void AddUndo(MultiCmd cmd)
        {
            string undo = "Un";
            undo += cmd.text.Substring(2);
            cmd.text = undo;
            Undos.Push(cmd);
        }
        public void ExecUndo()
        {
            if(Undos.Count > 0)
            {
                MultiCmd firstCommand = Undos.Peek();
                firstCommand.UnExec(this);
                AddRedo(Undos.Pop());
            }
        }
        private void AddRedo(MultiCmd cmd)
        {
            string redo = "Re";
            redo += cmd.text.Substring(2);
            cmd.text = redo;
            Redos.Push(cmd);
        }
        public void ExecRedo()
        {
            if(Redos.Count > 0)
            {
                MultiCmd first = Redos.Peek();
                first.Exec(this);
                AddUndo(Redos.Pop());
            }
        }

        public string undoString()
        {
            if(Undos.Count > 0)
            {
                string t = Undos.Peek().text;
                return t;
            }
            else
            {
                return null;
            }
        }
        public string redoString()
        {
            if (Redos.Count > 0)
            {
                string t = Redos.Peek().text;
                return t;
            }
            else
            {
                return null;
            }
        }

        
        //Save it to xml file
        public void WriteXML(StreamWriter file)
        {
            XmlWriter writer = XmlWriter.Create(file);
            writer.WriteStartDocument();
            writer.WriteStartElement("Spreadsheet");
            //go through each cell av save its attributes
            foreach (SpreadsheetCell cell in SS_Array)
            {
                string cellName = Number2String(cell.getColumn + 1, true);
                cellName += cell.getRow.ToString();
                if ((cell.cellValue != null))
                {
                    
                    writer.WriteStartElement("cell");
                    writer.WriteAttributeString("name", cellName);
                    writer.WriteStartElement("bgcolor");
                    writer.WriteValue(cell.BgColor);
                    writer.WriteEndElement();
                    writer.WriteStartElement("text");
                    writer.WriteValue(cell.newCellText);
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                }
            }
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();
        }
        //convert a number to letter
        private string Number2String(int number, bool isCaps)
        {
            char c = (char)((isCaps ? 65 : 97) + (number - 1));
            return c.ToString();
        }

        //load the xml file
        public void readXML(Stream stream)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            int row = 0, col = 0;
            Undos.Clear();
            Redos.Clear();
            using (XmlReader reader = XmlReader.Create(stream, settings))
            {
                while(reader.Read())
                {
                    if (reader.IsStartElement())
                    {
                        switch (reader.Name)
                        {
                            //get the cell name
                            case "cell":
                                string attribute = reader["name"];
                                //all the columns are marked from A to Z so we have to convert that to int
                                col = Convert.ToInt32(attribute[0] - 65);
                                // remaining part is the name of the cell we need to copy a value from
                                //to convert that
                                row = Convert.ToInt32(attribute.Substring(1));
                                break;
                            //get the color
                            case "bgcolor":
                                if (reader.Read())
                                {
                                    string val = reader.Value.Trim();
                                    uint color = Convert.ToUInt32(val);
                                    SS_Array[row, col].BgColor = color;
                                }
                                break;
                            //get the cell value
                            case "text":
                                if (reader.Read())
                                {

                                    string val = reader.Value.Trim();
                                    if(val.StartsWith("="))
                                    {
                                        SS_Array[row, col].newCellText = val;
                                        UpdateCellValue(SS_Array[row, col]);
                                        SS_Array[row, col].Value = val;
                                    }
                                    else
                                    {
                                        SS_Array[row, col].newCellText = val;
                                        SS_Array[row, col].Value = val;
                                    }
                                    
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }
    }
}
