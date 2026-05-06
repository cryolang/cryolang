using Spectre.Console;
namespace CryoLang;

public class Program {
    private const string CompilerLanguageVersion = "1.0.0";

    public static void PrintAst(ProgramNode program)
    {
        var root = new Tree($"[yellow]CryoLang {program.LanguageVersion}[/]");

        var importsNode = root.AddNode("Imports");
        foreach (var import in program.Imports)
            importsNode.AddNode($"{(import.IsLocal ? "local" : "global")}: {import.Path}");

        var macrosNode = root.AddNode("Macros");
        foreach (var macro in program.Macros)
            macrosNode.AddNode($"{macro.Name} = {macro.Value}");

        var functionsNode = root.AddNode("Functions");
        foreach (var fn in program.Functions)
            AddFunction(functionsNode, fn);

        AnsiConsole.Write(root);
    }

    private static void AddFunction(TreeNode parent, FunctionNode fn)
    {
        var root = parent.AddNode($"[yellow]{fn.Visibility} Function: {fn.Name}[/]");
        root.AddNode($"ReturnType: {fn.ReturnType}");

        var paramNode = root.AddNode("Parameters");
        foreach (var p in fn.Parameters)
            paramNode.AddNode($"{(p.IsConst ? "const" : "var")} {p.Type} {p.Name}");

        var bodyNode = root.AddNode("Body");
        AddStatements(bodyNode, fn.Body);
    }

    private static void AddStatements(TreeNode parent, IEnumerable<StatementNode> statements)
    {
        foreach (var stmt in statements)
        {
            switch (stmt)
            {
                case ReturnNode r:
                    parent.AddNode($"Return {FormatExpression(r.Expression)}");
                    break;
                case VariableNode v:
                    parent.AddNode($"{(v.IsConst ? "Const" : "Var")} {v.Type} {v.Name} = {FormatExpression(v.Value)}");
                    break;
                case IfNode i:
                    var ifNode = parent.AddNode($"If {FormatExpression(i.Condition)}");
                    var thenNode = ifNode.AddNode("Then");
                    AddStatements(thenNode, i.ThenBody);
                    if (i.ElseBody.Count > 0)
                    {
                        var elseNode = ifNode.AddNode("Else");
                        AddStatements(elseNode, i.ElseBody);
                    }
                    break;
                case WhileNode w:
                    var whileNode = parent.AddNode($"While {FormatExpression(w.Condition)}");
                    AddStatements(whileNode, w.Body);
                    break;
                case ExpressionStatementNode e:
                    parent.AddNode($"Expression {FormatExpression(e.Expression)}");
                    break;
            }
        }
    }

    private static string FormatExpression(ExpressionNode? expression)
    {
        return expression switch
        {
            null => "",
            NumberNode n => n.Value,
            IdentifierNode i => i.Name,
            BinaryExpressionNode b => $"({FormatExpression(b.Left)} {b.Operator} {FormatExpression(b.Right)})",
            UnaryExpressionNode u => $"({u.Operator}{FormatExpression(u.Operand)})",
            PostfixExpressionNode p => $"({FormatExpression(p.Operand)}{p.Operator})",
            TernaryExpressionNode t => $"({FormatExpression(t.Condition)} ? {FormatExpression(t.WhenTrue)} : {FormatExpression(t.WhenFalse)})",
            _ => expression.GetType().Name
        };
    }
    
    public static void Main(string[] args)
    {
        string path = args.Length > 0 ? args[0] : "code.cryo";
        string raw = File.ReadAllText(path);
        var lexer = new Lexer(raw);
        var tokens = lexer.Tokenize();

        foreach (var token in tokens)
            Console.WriteLine($"{token.Line}:{token.Column} {token.Type} {token.Value}");

        var parser = new Parser(tokens);
        var ast = parser.Parse();

        if (ast.LanguageVersion != CompilerLanguageVersion)
            AnsiConsole.MarkupLine($"[yellow]Warning:[/] source CryoLang version {ast.LanguageVersion} does not match compiler language version {CompilerLanguageVersion}.");

        PrintAst(ast);
    }
}
