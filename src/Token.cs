namespace CryoLang;

public enum TokenType {
    EOF,
    NEWLINE,

    IDENT,
    NUMBER,
    VERSION,
    PATH,

    CRYOLANG,
    IMPORT,
    MACRO,
    FUNCTION,
    RETURN,
    PUBLIC,
    PRIVATE,
    IF,
    ELSE,
    WHILE,

    CONSTANT,
    VARIABLE,

    TYPE,

    LPAREN,     // (
    RPAREN,     // )
    LBRACE,     // {
    RBRACE,     // }
    COMMA,      // ,
    SEMICOLON,  // ;
    ARROW,      // ->
    QUESTION,   // ?
    COLON,      // :

    PLUS,
    MINUS,
    STAR,
    SLASH,
    MOD,
    PLUS_PLUS,
    EQUALS,
    EQUAL_EQUAL,
    BANG_EQUAL,
    LESS,
    LESS_EQUAL,
    GREATER,
    GREATER_EQUAL
}

public class Token
{
    public TokenType Type;
    public string Value;
    public int Line;
    public int Column;

    public Token(){
        Type = TokenType.EOF;
        Value = "";
        Line = 0;
        Column = 0;
    }

    public Token(TokenType type, string value, int line = 0, int column = 0)
    {
        Type = type;
        Value = value;
        Line = line;
        Column = column;
    }
}
