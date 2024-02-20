[assembly: System.Runtime.CompilerServices.InternalsVisibleToAttribute("ExpressionParserTests")]

namespace ExpressionParser
{
    internal class Tokenizer
    {
        private readonly string input;
        private int index;
        private int current;

        private Token[] tokens;

        public Tokenizer(string input)
        {
            this.input = input;

            index = 0;
            current = 0;
            tokens = Array.Empty<Token>();
        }

        public Token Peek()
        {
            return Peek(0);
        }

        public Token Peek(int n)
        {
            while (tokens.Length <= current + n && index <= input.Length)
            {
                tokens = tokens.Append(Next()).ToArray();
            }

            return tokens[Math.Min(current + n, tokens.Length - 1)];
        }

        public Token Pop()
        {
            Token token = Peek();

            if (token.type != TokenType.EOF)
            {
                current += 1;
            }

            return token;
        }

        private Token Next()
        {
            while (index < input.Length && Char.IsWhiteSpace(input[index]))
            {
                index += 1;
            }

            if (index >= input.Length)
            {
                return new Token(TokenType.EOF, input.Length, input.Length);
            }

            switch (input[index])
            {
                case '(':
                    return new Token(TokenType.LParen, index, index, input[index++].ToString());
                case ')':
                    return new Token(TokenType.RParen, index, index, input[index++].ToString());
                case '|':
                    return new Token(TokenType.Pipe, index, index, input[index++].ToString());

                case '+':
                    return new Token(TokenType.BinaryOperation, index, index, input[index++].ToString(), (x, y) => x + y, Precedence.PLUS);
                case '-':
                    return new Token(TokenType.BinaryOperation, index, index, input[index++].ToString(), (x, y) => x - y, Precedence.MINUS);
                case '*':
                    return new Token(TokenType.BinaryOperation, index, index, input[index++].ToString(), (x, y) => x * y, Precedence.MULT);
                case '/':
                    return new Token(TokenType.BinaryOperation, index, index, input[index++].ToString(), (x, y) => x / y, Precedence.DIV);
                case '^':
                    return new Token(TokenType.BinaryOperation, index, index, input[index++].ToString(), Math.Pow, Precedence.EXPONENT);
            }

            if (Char.IsNumber(input[index]))
            {
                int begin = index;

                while (index < input.Length && Char.IsNumber(input[index]))
                {
                    index += 1;
                }

                if (index < input.Length && (input[index] == '.' || input[index] == ','))
                {
                    index += 1;
                }

                while (index < input.Length && Char.IsNumber(input[index]))
                {
                    index += 1;
                }

                int end = index - 1;
                string word = input[begin..index];
                double value = double.Parse(input[begin..index].Replace('.', ','));

                return new Token(TokenType.Constant, begin, end, word, value);
            }

            if (Char.IsLetter(input[index]))
            {
                int begin = index;

                while (index < input.Length && Char.IsLetter(input[index]))
                {
                    index += 1;
                }

                string word = input[begin..index];

                int end = index - 1;

                return word switch
                {
                    "sin"  => new Token(TokenType.UnaryOperation, begin, end, word, Math.Sin),
                    "cos"  => new Token(TokenType.UnaryOperation, begin, end, word, Math.Cos),
                    "tan"  => new Token(TokenType.UnaryOperation, begin, end, word, Math.Tan),
                    "log"  => new Token(TokenType.UnaryOperation, begin, end, word, Math.Log10),
                    "ln"   => new Token(TokenType.UnaryOperation, begin, end, word, x => Math.Log(x)),
                    "exp"  => new Token(TokenType.UnaryOperation, begin, end, word, Math.Exp),
                    "sqrt" => new Token(TokenType.UnaryOperation, begin, end, word, Math.Sqrt),
                    "abs"  => new Token(TokenType.UnaryOperation, begin, end, word, Math.Abs),

                    "pi" => new Token(TokenType.Constant, begin, end, word, Math.PI),
                    "e" =>  new Token(TokenType.Constant, begin, end, word, Math.E),

                    "t" => new Token(TokenType.Variable, begin, end, word),

                    _ => new Token(TokenType.Unknown, begin, end, word),
                };
            }

            return new Token(TokenType.Unknown, index, index, input[index++].ToString());
        }
    }

    internal class Token
    {
        public TokenType type;
        public int begin;
        public int end;
        public string text = "";

        public double constant = -69;

        public UnaryOperator unaryOperation = _ => -69;

        public BinaryOperator binaryOperation = (_, _) => -69;
        public Precedence precedence = Precedence.MIN;

        public Token(TokenType type, int begin, int end)
        {
            this.type = type;
            this.begin = begin;
            this.end = end;
        }

        public Token(TokenType type, int begin, int end, string text) : this(type, begin, end)
        {
            this.text = text;
        }

        public Token(TokenType type, int begin, int end, string text, double constant) : this(type, begin, end, text)
        {
            this.constant = constant;
        }

        public Token(TokenType type, int begin, int end, string text, UnaryOperator unaryOperation) : this(type, begin, end, text)
        {
            this.unaryOperation = unaryOperation;
        }

        public Token(TokenType type, int begin, int end, string text, BinaryOperator binaryOperation, Precedence precedence) : this(type, begin, end, text)
        {
            this.binaryOperation = binaryOperation;
            this.precedence = precedence;
        }

        public override string ToString()
        {
            return $"Token<{type}, '{text}', {begin}-{end}>";
        }
    }

    public delegate double UnaryOperator(double x);
    public delegate double BinaryOperator(double x, double y);

    internal enum TokenType
    {
        Unknown,
        EOF,

        Constant,
        Variable,
        UnaryOperation,
        BinaryOperation,

        LParen,
        RParen,
        Pipe,
    }

    internal enum Precedence
    {
        MIN,

        PLUS,
        MINUS,
        MULT,
        DIV,
        EXPONENT,

        MAX,
    }
}
