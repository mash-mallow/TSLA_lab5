using System;
using System.Collections.Generic;
using System.Text;

namespace lab5
{
    internal class GraphBuilder : INodeVisitor // класс-конструктор графа
    {
        private static string ReplaceLastChar(string str, char rep = ' ')
        {
            if (!string.IsNullOrEmpty(str))
            {
                return str.Substring(0, str.Length - 1) + rep.ToString();
            }
            else
            {
                return "";
            }
        }

        const Char SPACE = ' ';
        const Char L_TURN = '┌';
        const Char M_TURN = '├';
        const Char R_TURN = '└';
        const Char V_PIPE = '│';
        const string TAB = "    ";
        const string H_PIPE = "──";

        internal static readonly LegacyData InitialData = new LegacyData { LegacyIndent = TAB, LegacyOrientation = BranchOrientation.Right };

        private StringBuilder sb;

        public GraphBuilder() // инициализация графа
        {
            this.sb = new StringBuilder();
            this.sb.AppendLine("(Root)");
        }
        internal enum BranchOrientation
        {
            Mid, Left, Right
        }

        internal class LegacyData
        {
            public BranchOrientation LegacyOrientation { get; internal set; }
            public string LegacyIndent { get; internal set; }
        }

        public object VisitBinOp(Token op, INode left, INode right, object options) // добавление ветки для бинарной операции
        {
            LegacyData legacyData = (LegacyData)options;

            BranchOrientation legacyOrientation = legacyData.LegacyOrientation;
            string legacyIndent = legacyData.LegacyIndent;

            if (legacyOrientation == BranchOrientation.Left)
            {
                left.Accept(this, new LegacyData
                {
                    LegacyIndent = ReplaceLastChar(legacyIndent, SPACE) + TAB + L_TURN,
                    LegacyOrientation = BranchOrientation.Left
                });
            }
            else
            {
                left.Accept(this, new LegacyData
                {
                    LegacyIndent = ReplaceLastChar(legacyIndent, V_PIPE) + TAB + V_PIPE,
                    LegacyOrientation = BranchOrientation.Left
                });
            }

            if (legacyOrientation == BranchOrientation.Left)
            {
                sb.AppendLine(ReplaceLastChar(legacyIndent, L_TURN) + H_PIPE + " (" + op.ToString() + ")");
            }
            else
            {
                sb.AppendLine(ReplaceLastChar(legacyIndent, R_TURN) + H_PIPE + " (" + op.ToString() + ")");
            }

            if (legacyOrientation == BranchOrientation.Right)
            {
                right.Accept(this, new LegacyData
                {
                    LegacyIndent = ReplaceLastChar(legacyIndent, SPACE) + TAB + R_TURN,
                    LegacyOrientation = BranchOrientation.Right
                });
            }
            else
            {
                right.Accept(this, new LegacyData
                {
                    LegacyIndent = ReplaceLastChar(legacyIndent, V_PIPE) + TAB + V_PIPE,
                    LegacyOrientation = BranchOrientation.Right
                });
            }

            return this.sb.ToString();
        }

        public object VisitNum(Token num, object options) // добавление ветки для числа
        {
            LegacyData legacyData = (LegacyData)options;

            BranchOrientation legacyOrientation = legacyData.LegacyOrientation;
            string legacyIndent = legacyData.LegacyIndent;

            if (legacyOrientation == BranchOrientation.Left)
            {
                sb.AppendLine(ReplaceLastChar(legacyIndent, L_TURN) + H_PIPE + "  " + num.ToString());
            }
            else
            {
                sb.AppendLine(ReplaceLastChar(legacyIndent, R_TURN) + H_PIPE + "  " + num.ToString());
            }

            return this.sb.ToString();
        }

        public object VisitUnaryOp(Token op, INode node, object options) // добавление ветрки для унарной операции
        {
            LegacyData legacyData = (LegacyData)options;

            BranchOrientation legacyOrientation = legacyData.LegacyOrientation;
            string legacyIndent = legacyData.LegacyIndent;

            if (legacyOrientation == BranchOrientation.Left)
            {
                sb.AppendLine(ReplaceLastChar(legacyIndent, L_TURN) + H_PIPE + " (" + op.ToString() + ")");
            }
            else
            {
                sb.AppendLine(ReplaceLastChar(legacyIndent, R_TURN) + H_PIPE + " (" + op.ToString() + ")");
            }

            if (legacyOrientation == BranchOrientation.Right)
            {
                node.Accept(this, new LegacyData
                {
                    LegacyIndent = ReplaceLastChar(legacyIndent, SPACE) + TAB + R_TURN,
                    LegacyOrientation = BranchOrientation.Right
                });
            }
            else
            {
                node.Accept(this, new LegacyData
                {
                    LegacyIndent = ReplaceLastChar(legacyIndent, V_PIPE) + TAB + R_TURN,
                    LegacyOrientation = BranchOrientation.Right
                });
            }

            return this.sb.ToString();
        }

        public object VisitAssignStatement(Variable variable, Token op, Node expr, object options) // добавление ветки операции присваивания
        {
            LegacyData legacyData = (LegacyData)options;

            BranchOrientation legacyOrientation = legacyData.LegacyOrientation;
            string legacyIndent = legacyData.LegacyIndent;

            if (legacyOrientation == BranchOrientation.Left)
            {
                variable.Accept(this, new LegacyData
                {
                    LegacyIndent = ReplaceLastChar(legacyIndent, SPACE) + TAB + L_TURN,
                    LegacyOrientation = BranchOrientation.Left
                });
            }
            else
            {
                variable.Accept(this, new LegacyData
                {
                    LegacyIndent = ReplaceLastChar(legacyIndent, V_PIPE) + TAB + V_PIPE,
                    LegacyOrientation = BranchOrientation.Left
                });
            }

            if (legacyOrientation == BranchOrientation.Left)
            {
                sb.AppendLine(ReplaceLastChar(legacyIndent, L_TURN) + H_PIPE + " (" + op.ToString() + ")");
            }
            else if (legacyOrientation == BranchOrientation.Mid)
            {
                sb.AppendLine(ReplaceLastChar(legacyIndent, M_TURN) + H_PIPE + " (" + op.ToString() + ")");
            }
            else
            {
                sb.AppendLine(ReplaceLastChar(legacyIndent, R_TURN) + H_PIPE + " (" + op.ToString() + ")");
            }

            if (legacyOrientation == BranchOrientation.Right)
            {
                expr.Accept(this, new LegacyData
                {
                    LegacyIndent = ReplaceLastChar(legacyIndent, SPACE) + TAB + R_TURN,
                    LegacyOrientation = BranchOrientation.Right
                });
            }
            else
            {
                expr.Accept(this, new LegacyData
                {
                    LegacyIndent = ReplaceLastChar(legacyIndent, V_PIPE) + TAB + V_PIPE,
                    LegacyOrientation = BranchOrientation.Right
                });
            }

            return this.sb.ToString();
        }

        public object VisitCompoundStatement(List<Node> statements, object options) // добавление дочерних веток - структур внутри begin та end
        {
            LegacyData legacyData = (LegacyData)options;

            BranchOrientation legacyOrientation = legacyData.LegacyOrientation;
            string legacyIndent = legacyData.LegacyIndent;

            if (legacyOrientation == BranchOrientation.Left)
            {
                sb.AppendLine(ReplaceLastChar(legacyIndent, L_TURN) + H_PIPE + " (Child)");
            }
            else if (legacyOrientation == BranchOrientation.Mid)
            {
                sb.AppendLine(ReplaceLastChar(legacyIndent, M_TURN) + H_PIPE + " (Child)");
            }
            else
            {
                sb.AppendLine(ReplaceLastChar(legacyIndent, R_TURN) + H_PIPE + " (Child)");
            }

            string childIndent = legacyIndent;
            if (legacyOrientation == BranchOrientation.Right)
            {
                childIndent = ReplaceLastChar(childIndent, SPACE) + TAB;
            }
            else
            {
                childIndent = ReplaceLastChar(childIndent, V_PIPE) + TAB;
            }

            for (int i = 0; i < statements.Count; i++)
            {
                Node statement = statements[i];

                if (i < statements.Count - 1)
                {
                    statement.Accept(this, new LegacyData
                    {
                        LegacyIndent = childIndent,
                        LegacyOrientation = BranchOrientation.Mid
                    });
                }
                else
                {
                    statement.Accept(this, new LegacyData
                    {
                        LegacyIndent = childIndent,
                        LegacyOrientation = BranchOrientation.Right
                    });
                }
            }

            return this.sb.ToString();
        }

        public object VisitVariable(Token variable, object options) // добавление ветки для переменной
        {
            LegacyData legacyData = (LegacyData)options;

            BranchOrientation legacyOrientation = legacyData.LegacyOrientation;
            string legacyIndent = legacyData.LegacyIndent;

            if (legacyOrientation == BranchOrientation.Left)
            {
                sb.AppendLine(ReplaceLastChar(legacyIndent, L_TURN) + H_PIPE + "  " + variable.ToString());
            }
            else
            {
                sb.AppendLine(ReplaceLastChar(legacyIndent, R_TURN) + H_PIPE + "  " + variable.ToString());
            }

            return this.sb.ToString();
        }
    }
}
