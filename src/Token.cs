namespace CryoLang;

public enum TokenType {
    EOF,

    IDENT,
    NUMBER,

    FUNCTION,
    RETURN,
    PUBLIC,
    PRIVATE,

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

    PLUS,
    MINUS,
    STAR,
    SLASH,
    MOD,
    EQUALS
}

public class Token
{
    public TokenType Type;
    public string Value;

    public Token(){
        Type = TokenType.EOF;
        Value = "";
    }

    public Token(TokenType type, string value)
    {
        Type = type;
        Value = value;
    }
}