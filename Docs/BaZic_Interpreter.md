# BaZic interpreter

## Execution mode

### Debug mode without optimized code

In this mode, the BaZic code is interpreter without being edited to be faster, but the debugging feature are fully available. In case of execution error (Exception) or breakpoint, the program will stop/pause, provides the error message and the call stack with the local and global variables values for each call.

### Debug mode with optimized code

In this mode, the BaZic code is refactored before being interpreted to make it faster when there are a significant number of method invocation, conditions and loop. When the code is optimized, the debugging features are limited. In case of execution error (Exception) or breakpoint, the program will stop/pause, provides the error message, but not the call stack and not the variables values.

### Release mode

In this mode, the BaZic code is converted to C# (CSharp) code with a set of additional internal features designed to keep the same behavior than with the BaZic interpretation. It is then built in memory by [Roslyn](https://github.com/dotnet/roslyn) and run. After compilation, the executed code must be faster than in Debug mode and must consume less RAM, but debugging possibilities are very limited. This mode is only recommended in a production environment.

## Using the BaZic interpreter

The `BaZic.Core` and `BaZic.Runtime` assemblies are required.

### Parse a BaZic code

```csharp
void Run(string baZicCode)
{
    BaZic.Runtime.BaZic.Code.Parser.BaZicParser parser = new BaZic.Runtime.BaZic.Code.Parser.BaZicParser();
    BaZic.Runtime.BaZic.Code.Parser.ParserResult result = parser.Parse(baZicCode, optimize: false);

    // Retrieves the syntax tree.
    BaZic.Runtime.BaZic.Code.AbstractSyntaxTree.BaZicProgram program = result.Program;

    if (result.Issues.InnerExceptions.Count > 0)
    {
        // Show the reasons why the parser failed to parse or what are the warning detected.
    }
}
```

### Generate a BaZic code

```csharp
void Run(BaZic.Runtime.BaZic.Code.AbstractSyntaxTree.BaZicProgram program)
{
    BaZic.Runtime.BaZic.Code.BaZicCodeGenerator codeGenerator = new BaZic.Runtime.BaZic.Code.BaZicCodeGenerator();
    string generatedCode = codeGenerator.Generate(program);
}
```

### Interpret a BaZic code

```csharp
async void Run(BaZic.Runtime.BaZic.Code.AbstractSyntaxTree.BaZicProgram program)
{
    using (BaZic.Runtime.BaZic.Runtime.BaZicInterpreter interpreter = new BaZic.Runtime.BaZic.Runtime.BaZicInterpreter(program))
    {
        await interpreter.StartDebugAsync(verbose: true, args: null);

        if (interpreter.Error != null)
        {
            Console.WriteLine("Error : " + interpreter.Error.Exception.Message);
        }
        else
        {
            Console.WriteLine("Program result : " + interpreter.ProgramResult);
        }
    }
}
```
