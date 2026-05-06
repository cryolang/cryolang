namespace CryoLang;

public class Parser
{
    private readonly List<Token> tokens;
    private int position = 0;

    private Token Peek(int offset = 0) => position + offset < tokens.Count ? tokens[position + offset] : tokens[^1];
    private Token Advance() => tokens[position++];
    private bool Match(TokenType type) => Peek().Type == type;
    private bool IsAtEnd() => Match(TokenType.EOF);

    private Token Consume(TokenType type)
    {
        if (Match(type))
            return Advance();

        throw new Exception($"Expected {type}, got {Peek().Type} at {Peek().Line}:{Peek().Column}");
    }

    private void SkipNewlines()
    {
        while (Match(TokenType.NEWLINE))
            Advance();
    }

    public ProgramNode Parse()
    {
        SkipNewlines();
        Consume(TokenType.CRYOLANG);
        string version = Consume(TokenType.VERSION).Value;
        ConsumeLineEnd();

        ProgramNode program = new() { LanguageVersion = version };

        while (!IsAtEnd())
        {
            SkipNewlines();

            if (IsAtEnd())
                break;

            if (Match(TokenType.IMPORT))
                program.Imports.Add(ParseImport());
            else if (Match(TokenType.MACRO))
                program.Macros.Add(ParseMacro());
            else
                program.Functions.Add(ParseFunction());
        }

        return program;
    }

    private void ConsumeLineEnd()
    {
        if (Match(TokenType.SEMICOLON))
            Advance();

        if (Match(TokenType.NEWLINE))
            SkipNewlines();
        else if (!IsAtEnd())
            throw new Exception($"Expected end of line at {Peek().Line}:{Peek().Column}");
    }

    private ImportNode ParseImport()
    {
        Consume(TokenType.IMPORT);
        Token pathToken = Match(TokenType.PATH) ? Advance() : Consume(TokenType.IDENT);
        ConsumeLineEnd();

        return new ImportNode
        {
            Path = pathToken.Value,
            IsLocal = pathToken.Type == TokenType.PATH || pathToken.Value.StartsWith("./") || pathToken.Value.StartsWith("../")
        };
    }

    private MacroNode ParseMacro()
    {
        Consume(TokenType.MACRO);
        string name = Consume(TokenType.IDENT).Value;
        List<string> valueParts = [];

        while (!Match(TokenType.NEWLINE) && !Match(TokenType.EOF))
            valueParts.Add(Advance().Value);

        ConsumeLineEnd();

        return new MacroNode
        {
            Name = name,
            Value = string.Join(" ", valueParts)
        };
    }

    private FunctionNode ParseFunction()
    {
        string visibility = "private";
        if (Match(TokenType.PUBLIC) || Match(TokenType.PRIVATE))
            visibility = Advance().Value;

        Consume(TokenType.FUNCTION);

        string name = Consume(TokenType.IDENT).Value;

        Consume(TokenType.LPAREN);

        var parameters = new List<ParameterNode>();

        if (!Match(TokenType.RPAREN))
        {
            do
            {
                bool isConst = true;
                if (Match(TokenType.CONSTANT))
                {
                    Advance();
                    isConst = true;
                }
                else if (Match(TokenType.VARIABLE))
                {
                    Advance();
                    isConst = false;
                }

                string type = Consume(TokenType.TYPE).Value;
                string parameterName = Consume(TokenType.IDENT).Value;

                parameters.Add(new ParameterNode
                {
                    IsConst = isConst,
                    Type = type,
                    Name = parameterName
                });

                if (!Match(TokenType.COMMA))
                    break;

                Consume(TokenType.COMMA);
            } while (true);
        }

        Consume(TokenType.RPAREN);

        string returnType = "void";
        if (Match(TokenType.ARROW))
        {
            Advance();
            returnType = Consume(TokenType.TYPE).Value;
        }

        var body = ParseBlock();

        return new FunctionNode
        {
            Visibility = visibility,
            Name = name,
            ReturnType = returnType,
            Parameters = parameters,
            Body = body
        };
    }

    private List<StatementNode> ParseBlock()
    {
        Consume(TokenType.LBRACE);
        SkipNewlines();

        var body = new List<StatementNode>();

        while (!Match(TokenType.RBRACE) && !IsAtEnd())
        {
            body.Add(ParseStatement());
            SkipNewlines();
        }

        Consume(TokenType.RBRACE);
        return body;
    }

    private List<StatementNode> ParseStatementBody()
    {
        SkipNewlines();
        if (Match(TokenType.LBRACE))
            return ParseBlock();

        return [ParseStatement()];
    }

    private StatementNode ParseStatement()
    {
        SkipNewlines();

        if (Match(TokenType.RETURN))
            return ParseReturn();
        if (Match(TokenType.CONSTANT) || Match(TokenType.VARIABLE))
            return ParseVariable();
        if (Match(TokenType.IF))
            return ParseIf();
        if (Match(TokenType.WHILE))
            return ParseWhile();

        return ParseExpressionStatement();
    }

    private VariableNode ParseVariable()
    {
        bool isConst = false;

        if (Match(TokenType.CONSTANT))
        {
            isConst = true;
            Advance();
        }
        else if (Match(TokenType.VARIABLE))
        {
            Advance();
        }
        else
        {
            throw new Exception("Expected const or var");
        }

        string type = Consume(TokenType.TYPE).Value;
        string name = Consume(TokenType.IDENT).Value;

        ExpressionNode value = new NumberNode { Value = "0" };

        if (Match(TokenType.EQUALS))
        {
            Advance();
            value = ParseExpression();
        }

        Consume(TokenType.SEMICOLON);

        return new VariableNode
        {
            Type = type,
            Name = name,
            Value = value,
            IsConst = isConst
        };
    }

    private ReturnNode ParseReturn()
    {
        Consume(TokenType.RETURN);

        ExpressionNode? expr = null;
        if (!Match(TokenType.SEMICOLON))
            expr = ParseExpression();

        Consume(TokenType.SEMICOLON);

        return new ReturnNode { Expression = expr };
    }

    private IfNode ParseIf()
    {
        Consume(TokenType.IF);
        Consume(TokenType.LPAREN);
        var condition = ParseExpression();
        Consume(TokenType.RPAREN);

        var thenBody = ParseStatementBody();
        var elseBody = new List<StatementNode>();

        SkipNewlines();
        if (Match(TokenType.ELSE))
        {
            Advance();
            elseBody = ParseStatementBody();
        }

        return new IfNode
        {
            Condition = condition,
            ThenBody = thenBody,
            ElseBody = elseBody
        };
    }

    private WhileNode ParseWhile()
    {
        Consume(TokenType.WHILE);
        Consume(TokenType.LPAREN);
        var condition = ParseExpression();
        Consume(TokenType.RPAREN);

        return new WhileNode
        {
            Condition = condition,
            Body = ParseStatementBody()
        };
    }

    private ExpressionStatementNode ParseExpressionStatement()
    {
        var expr = ParseExpression();
        Consume(TokenType.SEMICOLON);
        return new ExpressionStatementNode { Expression = expr };
    }

    private ExpressionNode ParseExpression() => ParseTernary();

    private ExpressionNode ParseTernary()
    {
        var condition = ParseComparison();

        if (!Match(TokenType.QUESTION))
            return condition;

        Advance();
        var whenTrue = ParseExpression();
        Consume(TokenType.COLON);
        var whenFalse = ParseExpression();

        return new TernaryExpressionNode
        {
            Condition = condition,
            WhenTrue = whenTrue,
            WhenFalse = whenFalse
        };
    }

    private ExpressionNode ParseComparison()
    {
        var expr = ParseTerm();

        while (Match(TokenType.EQUAL_EQUAL) || Match(TokenType.BANG_EQUAL) || Match(TokenType.LESS) ||
               Match(TokenType.LESS_EQUAL) || Match(TokenType.GREATER) || Match(TokenType.GREATER_EQUAL))
        {
            string op = Advance().Value;
            var right = ParseTerm();
            expr = new BinaryExpressionNode { Left = expr, Operator = op, Right = right };
        }

        return expr;
    }

    private ExpressionNode ParseTerm()
    {
        var expr = ParseFactor();

        while (Match(TokenType.PLUS) || Match(TokenType.MINUS))
        {
            string op = Advance().Value;
            var right = ParseFactor();
            expr = new BinaryExpressionNode { Left = expr, Operator = op, Right = right };
        }

        return expr;
    }

    private ExpressionNode ParseFactor()
    {
        var expr = ParseUnary();

        while (Match(TokenType.STAR) || Match(TokenType.SLASH) || Match(TokenType.MOD))
        {
            string op = Advance().Value;
            var right = ParseUnary();
            expr = new BinaryExpressionNode { Left = expr, Operator = op, Right = right };
        }

        return expr;
    }

    private ExpressionNode ParseUnary()
    {
        if (Match(TokenType.MINUS) || Match(TokenType.PLUS_PLUS))
        {
            string op = Advance().Value;
            return new UnaryExpressionNode { Operator = op, Operand = ParseUnary() };
        }

        return ParsePostfix();
    }

    private ExpressionNode ParsePostfix()
    {
        var expr = ParsePrimary();

        while (Match(TokenType.PLUS_PLUS))
            expr = new PostfixExpressionNode { Operand = expr, Operator = Advance().Value };

        return expr;
    }

    private ExpressionNode ParsePrimary()
    {
        if (Match(TokenType.NUMBER) || Match(TokenType.VERSION))
            return new NumberNode { Value = Advance().Value };

        if (Match(TokenType.IDENT))
            return new IdentifierNode { Name = Advance().Value };

        if (Match(TokenType.LPAREN))
        {
            Advance();
            var expr = ParseExpression();
            Consume(TokenType.RPAREN);
            return expr;
        }

        throw new Exception($"Expected expression, got {Peek().Type} at {Peek().Line}:{Peek().Column}");
    }

    public Parser(List<Token> tokens)
    {
        this.tokens = tokens;
    }
}
