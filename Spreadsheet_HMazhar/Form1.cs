/*
* Name: Hasnain Mazhar
* Instructor: Even Olds
* Class: CPTS 321 
* Homework 10
* Date: 03/31/2017
*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SpreadsheetEngine;
using System.IO;

namespace Spreadsheet_HMazhar
{
    public partial class Form1 : Form
    {
        private Spreadsheet ss;
        public Form1()
        {

            InitializeComponent();
            
        }
        private void Spreadsheet_Load(Object sender, EventArgs e)
        {
            createNewDataGrid();
            //initialize the spreadsheet
            ss = new Spreadsheet(50, 26);

            //Step 7 point 3
            //Subscribe to the spreadsheet’s CellPropertyChanged event. Implement this so that when a cell’s
            //Value changes it gets updated in the cell in the DataGridView.
            for (int i = 0; i < ss.NumberOfRows; i++)
            {
                for (int j = 0; j < ss.NumberOfColumns; j++)
                {
                    ss.SS_Array[i, j].PropertyChanged += Cell_PropertyChanged;
                }
            }

            ss.PropertyChanged += OnMultiCommandChanged;
            //ss.CmdPropertyChanged += Cell_PropertyChanged;

            undoToolStripMenuItem.Enabled = false;
            redoToolStripMenuItem.Enabled = false;
        }
        //Function that that updates the cell value and update it in DataGridView
        private void Cell_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Spreadsheet.SpreadsheetCell cell = sender as Spreadsheet.SpreadsheetCell;
            dataGridView1.Rows[cell.rowIndex].Cells[cell.columnIndex].Value = cell.cellValue;

            dataGridView1.Rows[cell.rowIndex].Cells[cell.columnIndex].Style.BackColor =
                        Color.FromArgb((int)cell.BgColor);
        }
        private void dataGridView1_CellBeginEdit(Object sender, DataGridViewCellCancelEventArgs e)
        {
            int row = e.RowIndex;
            int column = e.ColumnIndex;
            dataGridView1.Rows[row].Cells[column].Value = ss.SS_Array[row, column].newCellText;
        }

        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            int row = e.RowIndex;
            int column = e.ColumnIndex;
            dataGridView1.Rows[row].Cells[column].Value = ss.SS_Array[row, column].cellValue;
        }
        //update the cell value in the dataGridView1
        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            var cellVal = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;

            string newVal = String.Empty;

            if (cellVal != null)
                newVal += cellVal.ToString();

            Spreadsheet.SpreadsheetCell cell = ss.SS_Array[e.RowIndex, e.ColumnIndex];
            List<IInvertibleCmd> cmds = new List<IInvertibleCmd>();
            if (cell.newCellText == null || (cell.cellValue != newVal && cell.newCellText != newVal))
            {
                cell.newCellText = newVal;
                cmds.Add(new restoreText(cell));
            }
            if (cmds.Count > 0)
            {
                ss.AddUndo(new MultiCmd("Undo cell text", cmds));
                undoToolStripMenuItem.Enabled = true;
                undoToolStripMenuItem.Text = "Redo cell text";
            }
        }
        

        private void chooseBackgroundColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataGridViewSelectedCellCollection selectedCells = dataGridView1.SelectedCells;
            ColorDialog dialog = chooseBackgroundColorToolStripMenuItem.Tag as ColorDialog;

            if (dialog == null)
            {
                dialog = new ColorDialog();
                chooseBackgroundColorToolStripMenuItem.Tag = dialog;
            }

            dialog.FullOpen = true;

            if (selectedCells.Count > 1)
            {
                dialog.Color = Color.White;
            }
            else if (selectedCells.Count > 0)
            {
                dialog.Color = selectedCells[0].Style.BackColor;
            }

            List<IInvertibleCmd> cmds = new List<IInvertibleCmd>();
            if (selectedCells.Count == 0 || dialog.ShowDialog() == DialogResult.OK)
            {
                foreach (DataGridViewCell cell in selectedCells)
                {
                    Cell temp = ss.GetCell(cell.RowIndex, cell.ColumnIndex);
                    cmds.Add(new restoreBGColor(temp));
                    temp.BgColor = (uint)dialog.Color.ToArgb();

                }
            }

            string cmdMessage = string.Format("Undo changing cell{0} background color{1}",
                (dataGridView1.SelectedCells.Count > 1) ? "s" : "",
                (dataGridView1.SelectedCells.Count > 1) ? "s" : "");

            if (cmds.Count > 0)
            {
                ss.AddUndo(new MultiCmd(cmdMessage, cmds));
                undoToolStripMenuItem.Enabled = true;
                undoToolStripMenuItem.Text = cmdMessage;
            }

        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ss.ExecUndo();
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ss.ExecRedo();
        }
        private void OnMultiCommandChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is Spreadsheet)
            {
                // cast sender to spreadsheet and get undo/redo text
                Spreadsheet ss = (Spreadsheet)sender;
                string undoText = ss.undoString();
                string redoText = ss.redoString();

                // set menu item text appropriateley
                if (undoText != null)
                {
                    undoToolStripMenuItem.Text = undoText;
                    undoToolStripMenuItem.Enabled = true;
                }
                else
                {
                    undoToolStripMenuItem.Text = "Undo";
                    undoToolStripMenuItem.Enabled = false;
                }

                if (redoText != null)
                {
                    redoToolStripMenuItem.Text = redoText;
                    redoToolStripMenuItem.Enabled = true;
                }
                else
                {
                    redoToolStripMenuItem.Text = "Redo";
                    redoToolStripMenuItem.Enabled = false;
                }
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.IO.StreamWriter file;
            //save file dialog box
            SaveFileDialog saveSS = new SaveFileDialog();
            // Filter files by extension
            saveSS.Filter = "XML files(.xml)|*.xml|all Files(*.*)|*.*";
            saveSS.FilterIndex = 2;
            saveSS.RestoreDirectory = true;
            //show save file dialog box
            if (saveSS.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    file = new System.IO.StreamWriter(saveSS.FileName.ToString());
                    //save object to the file
                    ss.WriteXML(file);
                    //close file
                    file.Close();
                }
                //trows the exception if it can't save
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            createNewDataGrid();
            Stream file = null;
            //save file dialog box
            OpenFileDialog loadFile = new OpenFileDialog();
            // Filter files by extension
            loadFile.Filter = "XML files(.xml)|*.xml|all Files(*.*)|*.*";
            loadFile.FilterIndex = 2;
            loadFile.RestoreDirectory = true;
            //show save file dialog box
            if (loadFile.ShowDialog() == DialogResult.OK)
            {
                //if file opens
                if ((file = loadFile.OpenFile()) != null)
                {
                    try
                    {                        
                        ss.readXML(file);
                        //close file
                        file.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }

        private void createNewDataGrid()
        {
            //
            DataGridViewCell layout = new DataGridViewTextBoxCell();
            dataGridView1.Columns.Clear();
            //loop through an add columns from A to Z (26)
            for (char c = 'A'; c <= 'Z'; c++)
            {
                DataGridViewColumn col = new DataGridViewColumn(layout);
                col.HeaderText = c.ToString();
                col.Name = c.ToString();
                dataGridView1.Columns.Add(col);
            }

            //rows 1 through 50
            //loop through to add 50 rows
            for (int i = 1; i <= 50; i++)
            {
                DataGridViewRow row = new DataGridViewRow();
                row.HeaderCell.Value = i.ToString();
                dataGridView1.Rows.Add(row);
            }
        }
    }
}

