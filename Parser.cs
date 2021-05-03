using System;
using System.Collections.Generic;
using System.Text;

namespace lab5
{
    internal class Parser // лексический анализатор
    {
        private Token curToken; // текущий символ
        private int curPos; // текущая позиция символа
        private int charCount;
        private char curChar;
        public string Text { get; private set; } // текст прогаммы

        public Parser(string text) // конструктор
        {
            this.Text = string.IsNullOrEmpty(text) ? string.Empty : text;
            this.charCount = this.Text.Length;
            this.curToken = Token.None();

            this.curPos = -1;
            this.Advance();
        }

        internal Node Parse()
        {
            this.NextToken();
            Node node = this.GrabCompoundStatement();
            this.ExpectToken(TokenType.Dot); // программа должна заканчиваться точкой

            return node;
        }

        private Node GrabCompoundStatement() // определение и проверка вложенных структур внутри begin и end
        {
            this.EatToken(TokenType.Begin);
            List<Node> statementList = this.GrabStatementList();
            this.EatToken(TokenType.End); 

            return new CompoundStatement(statementList);
        }

        private List<Node> GrabStatementList() // список операция и лексем внутри бложенной структуры
        {
            List<Node> statementList = new List<Node>();
            Node statement = this.GrabStatement();
            statementList.Add(statement);

            while (this.curToken.Type == TokenType.Semi)
            {
                this.EatToken(TokenType.Semi);
                statement = this.GrabStatement();
                statementList.Add(statement);
            }

            return statementList;
        }

        private Node GrabStatement()
        {
            Node statement;

            if (this.curToken.Type == TokenType.Begin)
            {
                statement = this.GrabCompoundStatement();
            }
            else if (this.curToken.Type == TokenType.Variable)
            {
                statement = this.GrabAssignStatement();
            }
            else
            {
                statement = new NoOp();
            }

            return statement;
        }

        private Node GrabAssignStatement() //определение операции присваивания
        {
            Token varToken = this.EatToken(TokenType.Variable);
            Token assignToken = this.EatToken(TokenType.Assignment);
            Node expr = this.GrabExpr();

            return new AssignStatement(new Variable(varToken), assignToken, expr);
        }

        private Token ExpectToken(TokenType tokenType) // определение ожидаемой лексемы
        {
            if (this.curToken.Type == tokenType)
            {
                return this.curToken;
            }
            else
            {
                throw new InvalidSyntaxException(string.Format("Неверный синтаксис на {0} позиции. Ожидалось {1} вместо {2}.", this.curPos, tokenType, this.curToken.Type.ToString()));
            }
        }

        private Token EatToken(TokenType tokenType) // опеределение действительной лексемы
        {
            if (this.curToken.Type == tokenType)
            {
                Token token = this.curToken;
                this.NextToken();
                return token;
            }
            else
            {
                throw new InvalidSyntaxException(string.Format("Неверный синтаксис на {0} позиции. Ожидалось {1} вместо {2}.", this.curPos, tokenType, this.curToken.Type.ToString()));
            }
        }

        private Node GrabExpr()  // определение операций сложение и вычитание
        {
            Node left = this.GrabTerm();

            while (this.curToken.Type == TokenType.Plus
                || this.curToken.Type == TokenType.Minus)
            {
                Token op = this.curToken;
                this.NextToken();
                Node right = this.GrabTerm();
                left = new BinOp(op, left, right);
            }

            return left;
        }

        private Node GrabTerm() // определение операций умножение и деление
        {
            Node left = this.GrabFactor();

            while (this.curToken.Type == TokenType.Multiply
                || this.curToken.Type == TokenType.Divide)
            {
                Token op = this.curToken;
                this.NextToken();
                Node right = this.GrabFactor();
                left = new BinOp(op, left, right);
            }

            return left;
        }

        private Node GrabFactor() // определение множителей в операции умножение (делимое/делитель в операции деление)
        {
            if (this.curToken.Type == TokenType.Plus  // опередление лексемы "унарная операция"
                || this.curToken.Type == TokenType.Minus)
            {
                Node node = this.GrabUnaryExpr();
                return node;
            }
            else if (this.curToken.Type == TokenType.LeftParenthesis) // если найдено выражение в скобках
            {
                Node node = this.GrabBracketExpr(); 
                return node;
            }
            else if (this.curToken.Type == TokenType.Variable)  // опередление лексемы "переменная"
            {
                Node node = this.GrabVariable();
                return node;
            }
            else
            {
                Token token = this.ExpectToken(TokenType.Number);
                this.NextToken();
                return new Num(token);
            }
        }

        private Node GrabVariable() // определение лексемы "переменная"
        {
            Token token = this.ExpectToken(TokenType.Variable);
            this.NextToken();

            return new Variable(token);
        }

        private Node GrabUnaryExpr() // определение лексемы "унарная операция"
        {
            Token op;

            if (this.curToken.Type == TokenType.Plus)
            {
                op = this.ExpectToken(TokenType.Plus);
            }
            else
            {
                op = this.ExpectToken(TokenType.Minus);
            }

            this.NextToken();

            if (this.curToken.Type == TokenType.Plus
                || this.curToken.Type == TokenType.Minus)
            {
                Node expr = this.GrabUnaryExpr();
                return new UnaryOp(op, expr);
            }
            else
            {
                Node expr = this.GrabFactor();
                return new UnaryOp(op, expr);
            }
        }

        private Node GrabBracketExpr() // определение вложенного выражения в скобках
        {
            this.ExpectToken(TokenType.LeftParenthesis);
            this.NextToken();
            Node node = this.GrabExpr();
            this.ExpectToken(TokenType.RightParenthesis);
            this.NextToken();
            return node;
        }

        private void Advance() // функция перехода на следующий символ
        {
            this.curPos += 1;

            if (this.curPos < this.charCount)
            {
                this.curChar = this.Text[this.curPos];
            }
            else
            {
                this.curChar = char.MinValue;
            }
        }

        private Char Peek() // функция для определения знака = в присваивании
        {
            if (this.curPos + 1 < this.charCount)
            {
                return this.Text[this.curPos + 1];
            }
            else
            {
                return char.MinValue;
            }
        }

        private void NextToken() // функция определения текущего символа
        {
            if (this.curChar == char.MinValue)
            {
                this.curToken = Token.None();
                return;
            }

            if (this.curChar == ' ')
            {
                while (this.curChar != char.MinValue && this.curChar == ' ')
                {
                    this.Advance();
                }

                if (this.curChar == char.MinValue)
                {
                    this.curToken = Token.None();
                    return;
                }
            }

            if (this.curChar == '+')
            {
                this.curToken = new Token(TokenType.Plus, this.curChar.ToString());
                this.Advance();
                return;
            }

            if (this.curChar == '-')
            {
                this.curToken = new Token(TokenType.Minus, this.curChar.ToString());
                this.Advance();
                return;
            }

            if (this.curChar == '*')
            {
                this.curToken = new Token(TokenType.Multiply, this.curChar.ToString());
                this.Advance();
                return;
            }

            if (this.curChar == '/')
            {
                this.curToken = new Token(TokenType.Divide, this.curChar.ToString());
                this.Advance();
                return;
            }

            if (this.curChar == '(')
            {
                this.curToken = new Token(TokenType.LeftParenthesis, this.curChar.ToString());
                this.Advance();
                return;
            }

            if (this.curChar == ')')
            {
                this.curToken = new Token(TokenType.RightParenthesis, this.curChar.ToString());
                this.Advance();
                return;
            }

            if (this.curChar >= '0' && this.curChar <= '9')  // если символ является числом
            {
                string num = string.Empty;
                while (this.curChar >= '0' && this.curChar <= '9')
                {
                    num += this.curChar.ToString();
                    this.Advance();
                }

                if (this.curChar == '.')
                {
                    num += this.curChar.ToString();
                    this.Advance();

                    if (this.curChar >= '0' && this.curChar <= '9')
                    {
                        while (this.curChar >= '0' && this.curChar <= '9')
                        {
                            num += this.curChar.ToString();
                            this.Advance();
                        }
                    }
                    else
                    {
                        throw new InvalidSyntaxException(string.Format("Неверный синтаксис на {0} позиции. Непредвиденный символ {1}", this.curPos, this.curChar));
                    }
                }

                this.curToken = new Token(TokenType.Number, num);
                return;
            }

            if ((this.curChar >= 'a' && this.curChar <= 'z') // если символ является буквой
                || this.curChar >= 'A' && this.curChar <= 'Z')
            {
                string word = string.Empty;
                word += this.curChar;
                this.Advance();

                if ((this.curChar >= 'a' && this.curChar <= 'z')  // поиск слов
                    || (this.curChar >= 'A' && this.curChar <= 'Z')
                    || this.curChar == '_'
                    || (this.curChar >= '0' && this.curChar <= '9'))
                {
                    while ((this.curChar >= 'a' && this.curChar <= 'z')
                        || (this.curChar >= 'A' && this.curChar <= 'Z')
                        || this.curChar == '_'
                        || (this.curChar >= '0' && this.curChar <= '9'))
                    {
                        word += this.curChar.ToString();
                        this.Advance();
                    }
                }

                if (string.Compare(word, "BEGIN", true) == 0) // если слово begin
                {
                    this.curToken = new Token(TokenType.Begin, word);
                }
                else if (string.Compare(word, "END", true) == 0) // если слово end
                {
                    this.curToken = new Token(TokenType.End, word);
                }
                else
                {
                    this.curToken = new Token(TokenType.Variable, word);
                }

                return;
            }

            if (this.curChar == ';')
            {
                this.curToken = new Token(TokenType.Semi, ";");
                this.Advance();
                return;
            }

            if (this.curChar == '.')
            {
                this.curToken = new Token(TokenType.Dot, ".");
                this.Advance();
                return;
            }

            if (this.curChar == ':')
            {
                if (this.Peek() == '=')
                {
                    this.Advance();
                    this.curToken = new Token(TokenType.Assignment, ":=");
                    this.Advance();
                    return;
                }
            }

            throw new InvalidSyntaxException(string.Format("Неверный синтаксис на {0} позиции. Непредвиденный символ {1}", this.curPos, this.curChar));
        }
     
    }
}
