using SuperUnit.Tests;
using SuperUnit.Tokeniser;
using SuperUnit.Tokeniser.Token;

namespace SuperUnit;

public static class Program
{
    private const string WelcomeMessage = "Thanks for checking out the SuperUnit Testing Framework! See the " +
                                          "documentation at 'https://github.com/DispairingGoose/SuperUnit', or supply a unit test file to begin testing.";
    
    public static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine(WelcomeMessage);
            return 0;
        }
        
        var path = @$"{Environment.CurrentDirectory}\{args[0]}";
        string source;

        try
        {
            source = File.ReadAllText(path);
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine($"File \"{path}\" not found");
            return 1;
        }
        catch (IOException)
        {
            Console.WriteLine($"File \"{path}\" could not be opened");
            return 1;
        }

        var lexer = new Lexer(source);
        Token[] programTokens;

        try
        {
            programTokens = lexer.Lex().ToArray();
        }
        catch (LexerException exception)
        {
            Console.WriteLine(exception.Message);
            return 1;
        }

        if (args.Contains("--printtokens"))
        {
            Console.WriteLine(programTokens.Aggregate("", (s, t) => s + t));
        }

        var parser = new TestParser(programTokens);
        TestGroup[] tests;
        
        try
        {
            tests = parser.Parse().ToArray();
        }
        catch (ParserException exception)
        {
            Console.WriteLine(exception.Message);
            return 1;
        }

        TestPrinter.PrintTests(tests);

        return 0;
    }
}