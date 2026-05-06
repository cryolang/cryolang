namespace CryoLang;

public abstract class StatementNode { }
public abstract class ExpressionNode { }

public class ProgramNode
{
    public required string LanguageVersion;
    public List<ImportNode> Imports = [];
    public List<MacroNode> Macros = [];
    public List<FunctionNode> Functions = [];
}

public class ImportNode
{
    public required string Path;
    public bool IsLocal;
}

public class MacroNode
{
    public required string Name;
    public required string Value;
}

public class FunctionNode
{
    public string Visibility = "private";
    public required string Name;
    public required string ReturnType;
    public List<ParameterNode> Parameters = [];
    public List<StatementNode> Body = [];
}

public class ReturnNode : StatementNode
{
    public ExpressionNode? Expression;
}

public class IfNode : StatementNode
{
    public required ExpressionNode Condition;
    public List<StatementNode> ThenBody = [];
    public List<StatementNode> ElseBody = [];
}

public class WhileNode : StatementNode
{
    public required ExpressionNode Condition;
    public List<StatementNode> Body = [];
}

public class ExpressionStatementNode : StatementNode
{
    public required ExpressionNode Expression;
}

public class NumberNode : ExpressionNode
{
    public required string Value;
}

public class IdentifierNode : ExpressionNode
{
    public required string Name;
}

public class BinaryExpressionNode : ExpressionNode
{
    public required ExpressionNode Left;
    public required string Operator;
    public required ExpressionNode Right;
}

public class UnaryExpressionNode : ExpressionNode
{
    public required string Operator;
    public required ExpressionNode Operand;
}

public class PostfixExpressionNode : ExpressionNode
{
    public required ExpressionNode Operand;
    public required string Operator;
}

public class TernaryExpressionNode : ExpressionNode
{
    public required ExpressionNode Condition;
    public required ExpressionNode WhenTrue;
    public required ExpressionNode WhenFalse;
}

public class ParameterNode
{
    public bool IsConst;
    public required string Type;
    public required string Name;
}

public class VariableNode : StatementNode
{
    public bool IsConst;
    public required string Type;
    public required string Name;
    public required ExpressionNode Value;
}
