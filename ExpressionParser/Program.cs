using System.Diagnostics;

namespace ExpressionParser
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] inputs = {
                "1",
                "1+2",
                "(1+3)",
                "(((1)+((4))))",
                "1 - 2 + 3",
                "(1 - 2) + 3",
                "1.2",
                "3,4",
                "e + pi",
                "sin(pi * 180)",
                "cos((pi / pi) * 180)",
                "tan(e ^ 1)",
                "log(1)",
                "ln(1)",
                "exp(1)",
                "sqrt(1)",
                "abs(1)",
                "|2 + 4|",
                "1 + 2 * 3",
                "2 * 3 + 1",
                "- 1 + 2",
                "1 + - 2",
                "- - 1",
                "+ + 1",
                "3t * 8",
                "8 + 3t",
                "8 + -3t",
                "4 + 7t^2",
                "4 + (2+7t^2+3)",
                "&",
            };

            Context ctx = new Context();
            ctx.SetVariable("t", 2);

            foreach (string input in inputs)
            {
                try
                {
                    Expression expr = Parser.ParseExpr(input);
                    double value = Parser.EvalExpr(expr, ctx);
                    Console.WriteLine($"{input,-20} = {expr.Print(),-30} = {string.Format("{0:0.##}", value)}");
                }
                catch (ParserException ex)
                {
                    Console.WriteLine();
                    Console.WriteLine(input);
                    Console.WriteLine($"{"".PadLeft(ex.token.begin)}^");
                    Console.WriteLine($"{"".PadLeft(ex.token.begin)}{ex.Message}");
                    Console.WriteLine();
                }
            }

            //string input = "3t + 7";

            //try
            //{
            //    Node ast = Parser.ParseExpr(input);
            //    Console.WriteLine($"Expression: {ast.Print()}");

            //    for (int i = 0; i < 10; i++)
            //    {
            //        ctx.SetVariable("t", i);
            //        Console.WriteLine($"t={i} => {ast.Print()} = {ast.Evaluate(ctx)}");
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.Message);
            //}
        }
    }
}