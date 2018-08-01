using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace SpreadsheetEngine
{
    public class ExpTree
    {
        //Dictionary that stores a variable and its value
        public static Dictionary<string, double> m_lookup = new Dictionary<string, double>();
        //Node class
        private abstract class Node
        {
            public abstract double Eval();
        }
        //Const node class that evaluates a number to double
        private class ConstNode : Node
        {
            private double value;

            public ConstNode(double newValue) { value = newValue; }

            public override double Eval()
            {
                return value;
            }
        }
        //Operator node class tht evaluates left and right child of the tree 
        //based on the operator and returns the result
        private class OpNode : Node
        {
            private char op;
            private Node m_left, m_right;
            //constructor for OpNode
            public OpNode(char Op, Node left, Node right)
            {
                op = Op;
                m_left = left;
                m_right = right;
            }

            public override double Eval()
            {

                double left = m_left.Eval();
                double right = m_right.Eval();
                switch (op)
                {
                    case '+': return left + right;
                    case '-': return left - right;
                    case '*': return left * right;
                    default:
                        return left / right;
                }
            }
        }
        //variable node class that evalutes a variable into a dictionary and return it
        private class VarNode : Node
        {
            private string m_varName;
            private double m_varValue;



            public VarNode(string varName, double value)
            {
                m_varName = varName;
                m_varValue = value;
                if (m_lookup.ContainsKey(varName))
                {
                    m_lookup[varName] = value;
                }
                else
                {
                    m_lookup.Add(varName, value);
                }
            }

            public override double Eval()
            {
                return m_lookup[m_varName];
            }
        }

        private Node root;
        //Implement this constructor to construct the tree from the specific expression
        public ExpTree(string expression)
        {
            root = Compile(expression);
        }
        public ExpTree() { }

        //Sets the specified variable within the ExpTree variable dictionary
        public void SetVar(string varName, double varValue)
        {
            if (m_lookup.ContainsKey(varName))
            {
                m_lookup[varName] = varValue;
            }
            else
            {
                m_lookup.Add(varName, varValue);
            }
        }

        public string[] getVar()
        {
            return m_lookup.Keys.ToArray();
        }
        //parse the string and return if the string is variable or constant
        private static Node BuildSimple(string term)
        {
            double num;
            if (double.TryParse(term, out num))
            {
                return new ConstNode(num);
            }
            return new VarNode(term, num);
        }

        //main function that compiles the expression
        private static Node Compile(string exp)
        {
            //take out all the empty spaces in the expression
            exp = exp.Replace(" ", "");
            //check for parantheses
            if (exp[0] == '(')
            {
                int counter = 1;
                for (int i = 1; i < exp.Length; i++)
                {
                    if (exp[i] == ')')
                    {
                        counter--;
                        if (counter == 0)
                        {
                            if (i == exp.Length - 1)
                            {
                                return Compile(exp.Substring(1, i - 1));
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    if (exp[i] == '(')
                    {
                        counter++;
                    }
                }
            }

            int index = GetLowOpIndex(exp);
            if (index != -1)
            {
                return new OpNode(
                    exp[index],
                    Compile(exp.Substring(0, index)),
                    Compile(exp.Substring(index + 1)));
            }
            //HW5
            //for (int i = exp.Length - 1; i >= 0; i--)
            //{
            //    switch (exp[i])
            //    {
            //        case '+':
            //        case '-':
            //        case '*':
            //        case '/':
            //            return new OpNode(exp[i],
            //                Compile(exp.Substring(0, i)),
            //                Compile(exp.Substring(i + 1)));
            //    }
            //}
            return BuildSimple(exp);

        }
        //Function that checks for the lowest presedence operator 
        private static int GetLowOpIndex(string exp)
        {
            //3+4*5+6
            //3*4+5*6
            int parenCounter = 0;
            int index = -1;
            //go throught the expression from left to right
            for (int i = exp.Length - 1; i >= 0; i--)
            {
                switch (exp[i])
                {
                    case ')':
                        parenCounter--;
                        break;

                    case '(':
                        parenCounter++;
                        break;

                    case '+':
                    case '-':
                        if (parenCounter == 0)
                            return i;
                        break;
                    case '*':
                    case '/':
                        if (parenCounter == 0 && index == -1)
                            index = i;
                        break;
                }
            }
            return index;
        }

        //Implement this member function with no parameters that evaluates the expression to a
        //double value
        public double Eval()
        {
            if (root != null)
            {
                return root.Eval();
            }
            else
            {
                return double.NaN;
            }
        }

    }
}
