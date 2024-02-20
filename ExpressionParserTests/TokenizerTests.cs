using ExpressionParser;

namespace ExpressionParserTests
{
    [TestClass]
    public class TokenizerTest
    {
        [TestMethod]
        public void TestValidInput()
        {
            string[] testCases =
            {
                "123",
                "1+2*3/4^5",
                "1,234",
                "2438.523",

                "sin(1)",
                "cos(t)",
                "tan(1+2)",
                "log(1)",
                "ln(4)",

                "e",
                "pi",
                "t",

                "|+-*/^()",
            };

            foreach (string testCase in testCases)
            {
                Tokenizer tokenizer = new Tokenizer(testCase);
                while (tokenizer.Peek().type != TokenType.EOF)
                {
                    Token token = tokenizer.Pop();
                    Assert.AreNotEqual(token.type, TokenType.Unknown, 
                        $"Incorrect token type for '{token.text}'");
                }
            }
        }

        [TestMethod]
        public void TestInvalidInput()
        {
            string[] testCases =
            {
                "!@#$%&?[]{}",
            };

            foreach (string testCase in testCases)
            {
                Tokenizer tokenizer = new Tokenizer(testCase);
                while (tokenizer.Peek().type != TokenType.EOF)
                {
                    Token token = tokenizer.Pop();
                    Assert.AreEqual(token.type, TokenType.Unknown,
                        $"Token type should be unknown '{token.text}'");
                }
            }
        }
    }

    public class ParserTestCase
    {
        public readonly string input;
        public readonly double expected;

        public ParserTestCase(string input, double expected)
        {
            this.input = input;
            this.expected = expected;
        }
    }

    [TestClass]
    public class ParserTests
    {
        [TestMethod]
        public void TestValidInput()
        {
            ParserTestCase[] testCases =
            {
                new ParserTestCase("1", 1),
                new ParserTestCase("1 + 23.74", 24.74),
                new ParserTestCase("4,5 * 3.0", 13.5),
                new ParserTestCase("1 - 2", -1),
                new ParserTestCase("5 * 2", 10),
                new ParserTestCase("e", Math.E),
                new ParserTestCase("pi", Math.PI),
                new ParserTestCase("sin(pi)", 0),
                new ParserTestCase("sin((180 * pi) / 180)", 0),
                new ParserTestCase("cos(pi)", -1),
                new ParserTestCase("cos(-pi)", -1),
                new ParserTestCase("cos(pi/2)", 0),
                new ParserTestCase("tan(0)", 0),
                new ParserTestCase("-1", -1),
                new ParserTestCase("--1", 1),
                new ParserTestCase("---1", -1),
                new ParserTestCase("1 + 2 - 3 * 4 / 5", 0.6),
                new ParserTestCase("4 / 5 * -3 + 2 + 1", 0.6),
            };

            foreach (ParserTestCase testCase in testCases)
            {
                Expression expr = Parser.ParseExpr(testCase.input);
                double actual = Parser.EvalExpr(expr);

                Assert.IsTrue(Math.Abs(testCase.expected - actual) < 0.00001, 
                    $"TestCase '{testCase.input}' failed. Expcted {testCase.expected}, got {actual}");
            }
        }

        [TestMethod]
        public void TestInvalidInput()
        {
            string[] testCases =
            {
                "$",
                "1.234.432",
                "1,429.309",
                "1 + ",
                "* 1",
                "(1 + 2",
                "|1 + 4",
                "((((1)) + (((2 * (3)) + 3)))",
            };

            foreach (string testCase in testCases)
            {
                Assert.ThrowsException<ParserException>(() => Parser.ParseExpr(testCase), $"Input was '{testCase}'");
            }
        }
    }
}