using System.Text;
using Antlr4.Runtime;

namespace Issueneter.Application.Parser.Parser;

internal class FilterErrorListener : BaseErrorListener
{
    private bool _hasErrors;
    private readonly StringBuilder _errorReport = new();
    
    public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine,
        string msg, RecognitionException e)
    {
        _hasErrors = true;
        _errorReport.AppendLine($"Syntax error at line {line} character {charPositionInLine}: {msg}");
    }

    public bool HasErrors(out string error)
    {
        if (_hasErrors)
        {
            error = _errorReport.ToString();
            return true;
        }

        error = string.Empty;
        return false;
    }
}