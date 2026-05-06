namespace CryoLang;

public abstract class StatementNode { }
public abstract class ExpressionNode { }

public class FunctionNode
{
    public string Name;
    public string ReturnType;
    public List<ParameterNode> Parameters = new();
    public List<object> Body = new();
}

public class ReturnNode : StatementNode
{
    public object Expression;
}

public class NumberNode : ExpressionNode
{
    public string Value;
}

public class ParameterNode
{
    public bool IsConst;
    public string Type;
    public string Name;
}

public class VariableNode : StatementNode
{
    public bool IsConst;
    public string Type;
    public string Name;
    public ExpressionNode Value;
}