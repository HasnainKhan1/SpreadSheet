using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpreadsheetEngine;

namespace expTree
{
    public class Program
    {
        static void Main(string[] args)
        {
            string exp = "A1-12-C1";
            bool decision = true;
            string input = "";
            string variable = "";
            ExpTree exptree = new ExpTree(exp);
            while(decision)
            {
                Console.WriteLine("Menu (current expression = " + exp + ")");
                Console.WriteLine("1 = Enter a new expression");
                Console.WriteLine("2 = Set a variable value");
                Console.WriteLine("3 = Evaluate tree");
                Console.WriteLine("4 = Quit");
                input = Console.ReadLine();
                if(input == "1")
                {
                    Console.WriteLine("Enter a new expression: ");
                    exp = Console.ReadLine();
                    exptree = new ExpTree(exp);
                }
                else if (input == "2")
                {
                    Console.WriteLine("Enter variable name: ");
                    exp = Console.ReadLine();
                    exptree = new ExpTree(exp);
                }

            }
        }
    }
}
