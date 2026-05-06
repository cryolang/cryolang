using Spectre.Console;
namespace CryoLang;

public class Program {
    public static void PrintAst(FunctionNode fn)
    {
        var root = new Tree($"[yellow]Function: {fn.Name}[/]");

        root.AddNode($"ReturnType: {fn.ReturnType}");

        var paramNode = root.AddNode("Parameters");
        foreach (var p in fn.Parameters)
        {
            paramNode.AddNode($"{(p.IsConst ? "constant" : "variable")} {p.Type} {p.Name}");
        }

        var bodyNode = root.AddNode("Body");

        foreach (var stmt in fn.Body)
        {
            if (stmt is ReturnNode r)
            {
                var retNode = bodyNode.AddNode("Return");

                if (r.Expression is NumberNode n)
                {
                    retNode.AddNode($"Number: {n.Value}");
                }
            }
        }

        AnsiConsole.Write(root);
    }
    
    public static void Main(string[] args)
    {
        string raw = File.ReadAllText("code.cryo");
        var lexer = new Lexer(raw);
        var tokens = lexer.Tokenize();

        foreach (var token in tokens)
            Console.WriteLine($"{token.Type} {token.Value}");

        var parser = new Parser(tokens);
        var ast = parser.Parse();

        PrintAst(ast);
    }
}