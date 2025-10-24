grammar Query;

// --- Parser rules ---

query
    : expr EOF
    ;

expr
    : binaryOp // TODO: Better name for operator
    | unaryOp
    ;

binaryOp
    : (AND | OR) LPAREN expr (COMMA expr)+ RPAREN
    ;

unaryOp
    : (EQUALS | CONTAINS) LPAREN nameToken COMMA valueToken RPAREN
    ;

// --- Lexer rules ---

// Operators (case-insensitive)
AND         : [Aa][Nn][Dd];
OR          : [Oo][Rr];
EQUALS      : [Ee][Qq][Uu][Aa][Ll][Ss];
CONTAINS    : [Cc][Oo][Nn][Tt][Aa][Ii][Nn][Ss];

// Name tokens: letters and digits
nameToken
    : NAME
    ;

NAME
    : [a-zA-Z][a-zA-Z0-9]*
    ;

// Value tokens: string or number
valueToken
    : STRING
    | INTEGER
    | DOUBLE
    ;

// Strings: double quotes, allowing escaped quotes
STRING
    : '"' ( '\\' . | ~["\\] )* '"'
    ;

// Numbers
INTEGER
    : '-'? DIGIT+
    ;
    
DOUBLE
    : '-'? DIGIT+ ('.' DIGIT+)?
    ;

fragment DIGIT : [0-9];

// Punctuation
LPAREN : '(';
RPAREN : ')';
COMMA  : ',';

// Whitespace
WS
    : [ \t\r\n]+ -> skip
    ;
