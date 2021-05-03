using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;

namespace lab5
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string input = string.Empty;
            string line;
 
            try
            {
                StreamReader file =
                new StreamReader(@"D:\KPI\Теорія синтаксичного аналізу\lab5\input.txt");
                Console.WriteLine("\nВходящий код на языке PASCAL:\n");
                for (int i=0; i<2; i++)
                {
                    line = file.ReadLine();
                    Console.WriteLine(line);
                }
                while ((line = file.ReadLine()) != null)
                {
                    input += line + " ";
                    Console.WriteLine(line);
                }
                Console.WriteLine();
                file.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            //input = "    begin x := 5; y := 6.5; begin z := 7; z := z+1; end; t := z*2; end.";

            {
                try
                {
                    Parser interpreter = new Parser(input);
                    Node node = interpreter.Parse();

                    Console.WriteLine();
                    Console.WriteLine(string.Format("Синтаксическое дерево:{0}{1}", Environment.NewLine + Environment.NewLine, node.Accept(new GraphBuilder(), GraphBuilder.InitialData)));
                    Console.WriteLine();
                    Console.WriteLine(string.Format("Значения переменных:{0}{1}", Environment.NewLine + Environment.NewLine, node.Accept(new ValueBuilder(), null)));
                    Console.WriteLine();
                }
                catch (InvalidSyntaxException ex)
                {
                    Console.WriteLine(ex.Message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
    }

    internal class Token // класс символа
    {
        public TokenType Type { get; private set; } // тип символа
        public string Value { get; private set; } // значение символа

        public Token(TokenType type, string value) // конструктор
        {
            this.Type = type;
            this.Value = value;
        }

        internal static Token None()
        {
            return new Token(TokenType.None, "");
        }

        public override string ToString()
        {
            return this.Value;
        }
    }

    internal enum TokenType // типы символов
    {
        None, // пробел
        Plus, // +
        Minus, // -
        Multiply, // *
        Divide, // дедение
        Number, // число
        LeftParenthesis, //левая круглая скобка
        RightParenthesis, //правая круглая скобка
        Variable, //переменная
        Assignment, // операция присваивания
        End, // конец программы
        Begin, // начало программы
        Dot, // точка
        Semi // точка запятой
    }

    internal interface INode // интерфейс Node
    {
        object Accept(INodeVisitor visitor, object options);
    }

    internal interface INodeVisitor  // интерфейс Node Visitor
    {
        object VisitNum(Token num, object options);
        object VisitUnaryOp(Token op, INode node, object options);
        object VisitBinOp(Token op, INode left, INode right, object options);
        object VisitAssignStatement(Variable variable, Token op, Node expr, object options);
        object VisitCompoundStatement(List<Node> statements, object options);
        object VisitVariable(Token variable, object options);
    }

    internal abstract class Node : INode // узел синтаксического графа
    {
        abstract public object Accept(INodeVisitor visitor, object options);
    }


    internal class Num : Node // число
    {
        internal Token Token { get; private set; }

        public Num(Token token)
        {
            this.Token = token;
        }

        override public object Accept(INodeVisitor visitor, object options)
        {
            return visitor.VisitNum(this.Token, options);
        }
    }

    internal class Variable : Node // переменная
    {
        internal Token Token { get; private set; }

        public Variable(Token token)
        {
            this.Token = token;
        }

        override public object Accept(INodeVisitor visitor, object options)
        {
            return visitor.VisitVariable(this.Token, options);
        }

        public override string ToString()
        {
            return this.Token.ToString();
        }
    }

    internal class UnaryOp : Node // унарная операция
    {
        internal Token Op { get; private set; }
        internal Node Node { get; private set; }

        public UnaryOp(Token op, Node node)
        {
            this.Op = op;
            this.Node = node;
        }

        override public object Accept(INodeVisitor visitor, object options)
        {
            return visitor.VisitUnaryOp(this.Op, this.Node, options);
        }
    }

    internal class BinOp : Node // бинарная операция
    {
        internal Token Op { get; private set; }
        internal Node Left { get; private set; }
        internal Node Right { get; private set; }

        public BinOp(Token op, Node left, Node right)
        {
            this.Op = op;
            this.Left = left;
            this.Right = right;
        }

        override public object Accept(INodeVisitor visitor, object options)
        {
            return visitor.VisitBinOp(this.Op, this.Left, this.Right, options);
        }
    }

    internal class NoOp : Node // неопредеоенный символ
    {
        public override object Accept(INodeVisitor visitor, object options)
        {
            throw new NotImplementedException(); // вывести уведомление об ошибке
        }
    }

    internal class AssignStatement : Node // операция присваивания
    {
        private Variable variable;
        private Node expr;
        private Token assignToken;

        public AssignStatement(Variable variable, Token assignToken, Node expr)
        {
            this.variable = variable;
            this.expr = expr;
            this.assignToken = assignToken;
        }

        public override object Accept(INodeVisitor visitor, object options)
        {
            return visitor.VisitAssignStatement(variable, assignToken, expr, options);
        }
    }

    internal class CompoundStatement : Node // вложенная структура
    {
        private List<Node> statementList;

        public CompoundStatement(List<Node> statementList)
        {
            this.statementList = new List<Node>();

            foreach (Node node in statementList)
            {
                if (node is NoOp)
                    continue;

                this.statementList.Add(node);
            }
        }

        public override object Accept(INodeVisitor visitor, object options)
        {
            return visitor.VisitCompoundStatement(this.statementList, options);
        }
    }

    [Serializable]
    internal class InvalidSyntaxException : Exception // класс синтаксической ошибки
    {
        public InvalidSyntaxException() { }

        public InvalidSyntaxException(string message) : base(message) { }

        public InvalidSyntaxException(string message, Exception innerException) : base(message, innerException) { }
        protected InvalidSyntaxException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}