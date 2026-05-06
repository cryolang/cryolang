namespace CryoLang;

public class Parser
{
    private List<Token> tokens;
    private int position = 0;

    // Helper functions to make the parsing clean.
    private Token Peek()
    {
        return tokens[position];
    }

    private Token Advance()
    {
        return tokens[position++];
    }

    private bool Match(TokenType type)
    {
        return Peek().Type == type;
    }

    private Token Consume(TokenType type)
    {
        if (Match(type))
            return Advance();

        throw new Exception($"Expected {type}, got {Peek().Type}");
    }

    public FunctionNode Parse()
    {
        return ParseFunction();
    }

    private FunctionNode ParseFunction()
    {
        // optional public/private
        if (Match(TokenType.PUBLIC) || Match(TokenType.PRIVATE))
            Advance();

        Consume(TokenType.FUNCTION);

        string name = Consume(TokenType.IDENT).Value;

        Consume(TokenType.LPAREN);

        var parameters = new List<ParameterNode>();

        if (!Match(TokenType.RPAREN))
        {
            do
            {
                string mut = "";
                if(Match(TokenType.CONSTANT)){
                    mut = Consume(TokenType.CONSTANT).Value;
                } else {
                    mut = Consume(TokenType.VARIABLE).Value;
                }

                string type = Consume(TokenType.TYPE).Value;
                string _name = Consume(TokenType.IDENT).Value;

                parameters.Add(new ParameterNode
                {
                    IsConst = (mut == "const"),
                    Type = type,
                    Name = _name
                });

                if (!Match(TokenType.COMMA))
                    break;

                Consume(TokenType.COMMA);

            } while (true);
        }

        Consume(TokenType.RPAREN);

        Consume(TokenType.ARROW);
        string returnType = Consume(TokenType.TYPE).Value;

        Consume(TokenType.LBRACE);

        var body = new List<object>();

        while (!Match(TokenType.RBRACE))
        {
            body.Add(ParseStatement());
        }

        Consume(TokenType.RBRACE);

        return new FunctionNode
        {
            Name = name,
            ReturnType = returnType,
            Parameters = parameters,
            Body = body
        };
    }

    private object ParseStatement()
    {
        if (Match(TokenType.RETURN))
            return ParseReturn();
        else if(Match(TokenType.CONSTANT) || Match(TokenType.VARIABLE))
            return ParseVariable();

        throw new Exception("Unknown statement");
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

        ExpressionNode value;

        // OPTIONAL initializer
        if (Match(TokenType.EQUALS))
        {
            Advance(); // consume '='
            // value = ParseExpression();
        }
        else
        {
            // default initialization = 0
            value = new NumberNode { Value = "0" };
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

        var expr = ParseExpression();

        Consume(TokenType.SEMICOLON);

        return new ReturnNode
        {
            Expression = expr
        };
    }

    private object ParseExpression()
    {
        var token = Consume(TokenType.NUMBER);

        return new NumberNode
        {
            Value = token.Value
        };
    }

    public Parser(List<Token> tokens)
    {
        this.tokens = tokens;
    }
}