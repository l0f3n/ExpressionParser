using System.Xml.Linq;

namespace ExpressionParser
{

    internal class ParserException : Exception
    {
        public Token token;

        public ParserException(string message, Token token) : base(message) 
        {
            this.token = token;
        }
    }

    internal class Parser
    {
        public static Expression ParseExpr(string input)
        {
            Parser parser = new Parser(input);
            return parser.Parse();
        }

        public static double EvalExpr(Expression expr, Context ctx)
        {
            return expr.Eval(ctx);
        }

        public static double EvalExpr(Expression expr)
        {
            return EvalExpr(expr, new Context {});
        }

        private readonly Tokenizer tokenizer;

        private Parser(string input)
        {
            tokenizer = new Tokenizer(input);
        }

        private Expression Parse()
        {
            Expression expr = ParseExpression(Precedence.PLUS);

            Expect(TokenType.EOF);

            return expr;
        }

        private Token Expect(TokenType type)
        {
            Token token = tokenizer.Pop();
            if (token.type != type)
            {
                throw new ParserException($"Unexpected token '{token.text}' ({token.type}), expected {type}", token);
            }

            return token;
        }

        private Expression ParseExpression()
        {
            return ParseExpression(Precedence.MIN);
        }

        private Expression ParseExpression(Precedence prec)
        {
            if (prec >= Precedence.MAX)
            {
                return ParsePrimary();
            }

            Expression lhs = ParseExpression(prec + 1);

            Token token = tokenizer.Peek();
            if (token.type == TokenType.BinaryOperation && token.precedence == prec)
            {
                Expect(TokenType.BinaryOperation);
                return new BinaryOperation(token.text, token.binaryOperation, lhs, ParseExpression(prec));
            } 
            else if (token.type == TokenType.Variable && Precedence.MULT == prec)
            {
                // Implicit multiplication for expressions like 7x => 7 * x or 2t^3 => 2*(t^3)
                return new BinaryOperation("*", (x, y) => x * y, lhs, ParseExpression(prec));
            }

            return lhs;
        }

        private Expression ParsePrimary()
        {
            Token token;
            Expression expr;

            switch(tokenizer.Peek().type)
            {
                case TokenType.Constant:
                    token = Expect(TokenType.Constant);

                    return new Constant(token.constant);

                case TokenType.Variable:
                    token = Expect(TokenType.Variable);

                    return new Variable(token.text);

                case TokenType.UnaryOperation:
                    token = Expect(TokenType.UnaryOperation);

                    Expect(TokenType.LParen);
                    expr = ParseExpression();
                    Expect(TokenType.RParen);

                    return new UnaryOperation(token.text, token.unaryOperation, expr);

                case TokenType.LParen:
                    Expect(TokenType.LParen);
                    expr = ParseExpression();
                    Expect(TokenType.RParen);

                    return expr;

                case TokenType.Pipe:
                    Expect(TokenType.Pipe);
                    expr = ParseExpression();
                    Expect(TokenType.Pipe);

                    return new UnaryOperation("abs", Math.Abs, expr);

                case TokenType.BinaryOperation:
                    // Handles expressions like -3 or 1 + -2 or +1 + +1
                    token = Expect(TokenType.BinaryOperation);
                    switch (token.text)
                    {
                        case "+":
                        case "-":
                            return new UnaryOperation(token.text, x => token.binaryOperation(0, x), ParseExpression(Precedence.MAX));
                    }

                    throw new ParserException($"Misplaced binary operator '{token.text}', expected either '+' or '-'", token);
            }

            token = tokenizer.Peek();
            throw new ParserException($"Unexpected token '{token.text}'", token);
        }
    }

    internal abstract class Expression
    {
        public abstract double Eval(Context ctx);

        public abstract string Print();
    }

    internal class UnaryOperation : Expression
    {
        private readonly string name;
        private readonly UnaryOperator unaryOperator;

        private readonly Expression expr;

        public UnaryOperation(string name, UnaryOperator unaryOperator, Expression expr)
        {
            this.name = name;
            this.unaryOperator = unaryOperator;
            this.expr = expr;
        }

        public override double Eval(Context ctx)
        {
            return unaryOperator(expr.Eval(ctx));
        }

        public override string Print()
        {
            return $"{name}({expr.Print()})";
        }
    }

    internal class BinaryOperation : Expression
    {
        private readonly string op;
        private readonly BinaryOperator binaryOperator;

        private readonly Expression lhs;
        private readonly Expression rhs;

        public BinaryOperation(string op, BinaryOperator binaryOperator, Expression lhs, Expression rhs)
        {
            this.op = op;
            this.binaryOperator = binaryOperator;
            this.lhs = lhs;
            this.rhs = rhs;
        }

        public override double Eval(Context ctx)
        {
            return binaryOperator(lhs.Eval(ctx), rhs.Eval(ctx));
        }

        public override string Print()
        {
            return "(" + lhs.Print() + "" + op + "" + rhs.Print() + ")";
        }
    }

    internal class Variable : Expression
    {
        private readonly string name;

        public Variable(string name)
        {
            this.name = name;
        }

        public override double Eval(Context ctx)
        {
            return ctx.GetVariable(name);
        }

        public override string Print()
        {
            return name;
        }
    }

    internal class Constant : Expression
    {
        private readonly double value;

        public Constant(double value)
        {
            this.value = value;
        }

        public override double Eval(Context ctx)
        {
            return value;
        }

        public override string Print()
        {
            return string.Format("{0:0.##}", value);
        }
    }

    public class Context
    {
        private Dictionary<string, double> Variables = new();

        public void SetVariable(string name, double value)
        {
            if (HasVariable(name))
            {
                Variables[name] = value;
            }
            else
            {
                Variables.Add(name, value);
            }
        }

        public double GetVariable(string name)
        {
            try
            {
                return Variables[name];
            }
            catch
            {
                throw new Exception($"ContextError: Undefined variable '{name}'");
            }
        }

        public bool HasVariable(string name)
        {
            return Variables.ContainsKey(name);
        }
    }
}
