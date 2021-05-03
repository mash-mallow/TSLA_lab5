using System;
using System.Collections.Generic;
using System.Text;

namespace lab5
{
    internal class ValueBuilder : INodeVisitor // поиск переменных и их значений
    {
        Dictionary<string, decimal> varLookup = new Dictionary<string, decimal>();

        public object VisitBinOp(Token op, INode left, INode right, object options) // вычисление результата выполнения бинарных операций
        {
            switch (op.Type)
            {
                case TokenType.Plus:
                    return (decimal)left.Accept(this, options) + (decimal)right.Accept(this, options); // сложение
                case TokenType.Minus:
                    return (decimal)left.Accept(this, options) - (decimal)right.Accept(this, options); // вычитание
                case TokenType.Multiply:
                    return (decimal)left.Accept(this, options) * (decimal)right.Accept(this, options); // умножение
                case TokenType.Divide: 
                    return (decimal)left.Accept(this, options) / (decimal)right.Accept(this, options); // деление
                default:
                    throw new Exception(string.Format("Операция типа {0} не может быть произведена.", op.Type.ToString()));
            }
        }

        public object VisitNum(Token num, object options)  
        {
            return decimal.Parse(num.Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture);
        }

        public object VisitVariable(Token variable, object options)
        {
            string varName = variable.Value;

            if (this.varLookup.ContainsKey(varName))
            {
                return this.varLookup[varName];
            }
            else
            {
                throw new Exception(string.Format("Переменной {0} не существует.", varName));
            }
        }

        public object VisitAssignStatement(Variable variable, Token op, Node expr, object options)  // результат присваивания переменной значения
        {
            string varName = variable.ToString();
            decimal value = (decimal)expr.Accept(this, options);
            this.varLookup[varName] = value;

            return value;
        }

        public object VisitCompoundStatement(List<Node> statements, object options)  // вложенные структуры
        {
            foreach (Node statement in statements)
            {
                statement.Accept(this, options);
            }

            StringBuilder sb = new StringBuilder();

            foreach (String key in this.varLookup.Keys)
            {
                sb.AppendLine(string.Format("{0} = {1}", key, this.varLookup[key]));
            }

            return sb.ToString(0, sb.Length - 1);
        }

        public object VisitUnaryOp(Token op, INode node, object options) // вычисление результата выполнения унарных операций
        {
            switch (op.Type)
            {
                case TokenType.Plus:
                    return (decimal)node.Accept(this, options);
                case TokenType.Minus:
                    return -(decimal)node.Accept(this, options);
                default:
                    throw new Exception(string.Format("Операция типа {0} не может быть произведена.", op.Type.ToString()));
            }
        }
    }
}
