# Technical Overview

## Global architecture

From your source code to the result of the executed program, several steps happen :
* Lexer
  It attempts to detect special known keywords, identifier, special character.
* Parser & Optimizer
  It will then search for logical series to determine the Abstract Syntax Tree (AST) and detect syntax errors.
* Interpreter or Compiler
  Depending of the user's settings, the engine will then interpret the AST, or generate C# from it, build and run it.

![Architecture](/Docs/Architecture.png)

## Lexer

The goal of the Lexer is to translate the source code from its textual form to an array of token while removing all the unecessary spaces and tabs.

A token is an object in memory that contains several informations :
* Type of token
  Is it a keyword? An identifier? A number? An operator? ...
* The value
  Useful for identifiers, numbers and strings, it provides literally what there is in the source code.
* Line, Column & Length
  It corresponds to the location of the token in the original source code.

![Lexer](/Docs/Lexer.png)

## Parser

The parser will then use the list of token to find some logical series to determine what is there exactly in the code.
Let's take the variable declaration statement as an example. Here is a simplified grammar for the example that match a variable declaration :

```
'VARIABLE' Identifier ('=' Expression)?
```

What tell us this grammar is exactly what the parser is doing :
* If we have strickly the keyword `VARIABLE`
* Followed by an `Identifier`
* Then we probably have a variable declaration where the variable name is the `Identifier`
* Optionally, if it is followed by a `=` symbol and an `Expression`
* Then the default value of the variable is the `Expression`

## Optimizer

The optimizer is here to solve several problems.

### Performances

It is its main goal, speed-up the code execution by producing a code faster to interpret by the interpreter.

### Prevent Stack Overflows

In the case of recursive methods calls, stack overflows can arrive quickly as the interpreter runs statements and expressions recursively.

To prevent it, the optimizer inline calls, which means taking the content of the method called, and put it at the place where it is called.

![Optimizer](/Docs/Optimizer.png)

Inlining method calls also contribute to improve execution speed.

## BaZic Interpreter

The interpreter works recursively. For each statements, it first interpret the expressions it contains, then the statement itself.

When the interpreter works with a not-optimized algorithm, it can pause it, step into the next statement, and resume.

## C# Generator and Compiler

This is used when the user wants to get an executable (.exe) file.

As C# is an object-oriented language and, more important, not procedural (in contradiction with BaZic), the generated C# is provided with a set of helper designed to insure that the C# source code has exactly the same behavior than the BaZic one.