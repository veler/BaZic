# BaZic

The BaZic is a `procedural object-oriented semi-dynamic typed` programming language `interpreted or compiled`, compatible with most of the .NET Framework 4.7.1 APIs.

A few notion of C# or Visual Basic are recommended but are not required.

# Frequently asked questions (FAQ)

## Can I create a Class and do inheritance?

No. BaZic is object-oriented in the way that you can use the .NET Framework types and objects.

## Can I still instantiate a .NET Framework class?

Yes. By using the keyword ```NEW``` followed by the full type name and constructor arguments.

## Can I use static .NET Framework class and methods?

Yes. Simply by using then the same way than in .Net : in a statement or expression.

## Can I use generic .NET Framework class?

No. It is not supported (yet?).

## Is there an equivalent of REF and OUT keywords that we can find in C#?

No. It is not supported (yet?).

## What is a BaZic program entry point?

The entry point of a BaZic program must be a synchronous declared function called ```Main``` and taking a single array argument ```args[]```.
The ```args[]``` argument can receive an array of .NET Framework objects.

## Does a BaZic's function can be called even if the program is not running?

Yes. Like in the case of a compiled DLL, you can call any method marked as ```EXTERN```.

## Does a BaZic program can have a UI (user interface)?

Yes. A BaZic program is allowed to get one single window for the user interface. The UI is described thanks to [XAML](https://www.microsoft.com/en-us/download/details.aspx?id=19600) code.

## Does a BaZic program can return a value?

Yes. If the program does not have a UI defined, the value returned by the entry point method will be considered as the program's result. If the program has a UI, the value returned by the function binded to the ```Closed``` event of the program's ```Window``` will be considered as the program's result.

## Do the language keywords are case-sensitive.

The language operators like ```FUNCTION``` or ```ASYNC``` are **not** case-sensitive.
The identifiers **are** case-sensitive.

Therefore, in the following example, two (2) variables are created. ```Foo``` and ```FOO```.

```
VARIABLE Foo
Variable FOO
```

## How can I run a BaZic program?

See the [BaZic interpreter](https://github.com/veler/BaZic/blob/master/BaZic_Interpreter.md) documentation.

# Syntax

The information below describes the grammar of the language.

Understanding the grammar syntax :

* `*` : zero or more.
* `+` : one or more.
* `?` : one or zero.
* `|` : or.

## Statements

### Function declaration

```
'EXTERN'? ('ASYNC' | 'EVENT')? 'FUNCTION' Identifier '(' Parameter_List ')'
    Statement_List
'END' 'FUNCTION'
```

### Assignment or Expression statement

```
Expression ('=' Expression)?
```

## Breakpoint

```
'BREAKPOINT'
```

Note : Breakpoints are ignored when the BaZic code is compiled.

### Break statement

```
'BREAK'
```

**Note** : this can be used only in a loop.

### Return statement

```
'RETURN' Expression?
```

### Throw statement

```
'THROW' Expression
```

### Try-Catch block

```
'TRY'
    Statement_List
'CATCH'?
    Statement_List
'END' 'TRY'
```

### Condition statement

```
'IF' Expression 'THEN'
    Statement_List
'ELSE'?
    Statement_List
'END' 'IF'
```

### Loop statement

```
'DO' ('WHILE' Expression)?
    Statement_List
'LOOP' ('WHILE' Expression)?
```

**Note** : at least one `'WHILE' Expression` must be placed at the top or bottom of the statement.

### Variable or binding declaration

```
('VARIABLE' | 'BIND') Identifier '[]'? ('=' Expression)?
```

## Expressions

### Expression list

```
Expression? ( ',' Expression)*
```

### Parameter list

```
Parameter_Declaration? ( ',' Parameter_Declaration)*
```

### Parameter declaration

```
Identifier ('[]')?
```

### Conditional OR

```
Conditional_And_Expression ('OR' Conditional_And_Expression)*
```

### Conditional AND

```
Negative_Expression ('AND' Negative_Expression)*
```

### Negative expression

```
('NOT')? Equality_Expression
```

### Equality expression

```
Relational_Expression ('=' Relational_Expression)*
```

### Relational expression

```
Additive_Expression (('<' | '>' | '<=' | '>=') Additive_Expression)*
```

### Additive expression

```
Multiplicative_Expression (('+' | '-') Multiplicative_Expression)*
```

### Multiplicative expression

```
Unary_Expression (('*' | '/' | '%') Unary_Expression)*
```

### Unary expression

```
Primary_Expression
| 'AWAIT' Unary_Expression
```

### Primary expression

```
Primary_Expression_Start Bracket_Expression* ((Member_Access | Method_Invocation) Bracket_Expression* )*
```

### Primary expression start

```
Primitive_Value
| Identifier
| 'EXCEPTION'
| '(' Expression ')'
| 'NEW' (Bracket_Expression | Namespace_Or_Type_Name Method_Invocation)
```

**Note** : The `EXCEPTION` keyword is designed to retrieves a thrown `System.Exception` in a `TRY-CATCH` statement. It must be use only in the `CATCH` block.

### Primitive expression

```
'NULL'
| 'TRUE'
| 'FALSE'
| String
| (+|-)?Integer.Integer
| (+|-)?Integer
```

### Static Property or Method invocation expression

```
Namespace_Or_Type_Name (Member_Access | Method_Invocation)
```

### Namespace or type name or variable reference

```
Identifier Member_Access*
```

### Member access

```
'.' Identifier
```

### Method invocation expression

```
'(' Expression_List ')'
```

### Bracket expression

```
'[' Expression_List ']'
```

# Sample code

## Program without UI

### Calculation expression

```
EXTERN FUNCTION Main(args[])
    RETURN (100 * 1.5) + 2 * (4 / 11.3)
    # The result must be 150.707964602
END FUNCTION
```

### Simple recursivity

```
VARIABLE initialValue = 100

EXTERN FUNCTION Main(args[])
    RETURN FirstMethod(initialValue) # This must return 0.
END FUNCTION

FUNCTION FirstMethod(num)
    IF num > 1 THEN
        RETURN FirstMethod(num - 1)
    END IF
    RETURN num
END FUNCTION
```

### Selection sort algorithm

```
EXTERN FUNCTION Main(args[])
    VARIABLE arr[] = NEW [64, 34, 25, 12, 22, 11, 90, 123]

    VARIABLE i = 0
    VARIABLE n = arr.Count
    DO WHILE i < n - 1
        VARIABLE j = 0
        DO WHILE j < n - i - 1
            IF arr[j] > arr[j + 1] THEN
                # swap temp and arr[i]
                VARIABLE temp = arr[j]
                arr[j] = arr[j + 1]
                arr[j + 1] = temp
            END IF
            j = j + 1
        LOOP
        i = i + 1
    LOOP

    RETURN arr
    # The result must be :
    # 11, 12, 22, 25, 34, 64, 90, 123
END FUNCTION
```

### Fibonacci series

```
EXTERN FUNCTION Main(args[])
    VARIABLE result = Fibonacci(9)
    System.Console.WriteLine("Fib 9 = " + result)
    RETURN result
END FUNCTION

FUNCTION Fibonacci(n)
    # To return the first Fibonacci number
    IF n = 0 THEN
        RETURN 0
    END IF

    VARIABLE a = 0
    VARIABLE b = 1
    VARIABLE c = 0
    VARIABLE i = 2

    DO WHILE i <= n
        c = a + b
        a = b
        b = c
        i = i + 1
    LOOP

    RETURN b
END FUNCTION
```

### Fibonacci series with recursivity

```
EXTERN FUNCTION Main(args[])
    VARIABLE result = Fibonacci(9)
    System.Console.WriteLine("Fib 9 = " + result)
    RETURN result
END FUNCTION

FUNCTION Fibonacci(n)
    IF n <= 1 THEN
        RETURN n
    ELSE
        RETURN Fibonacci(n - 1) + Fibonacci(n - 2)
    END IF
END FUNCTION
```

### Concurrent programming

```
# Expected result :
# Method1 blocks the thread until the end of its execution.
# var1 will be equal to "Hello".
# The first call to MethodAsync will take 3 sec to run but will not block the main thread as the ASYNC keyword is used but no AWAIT.
# var2 will be equal to a Task<object>. After 3 sec, if we access to the property "Result" of var2, this last one will be equal to "Hello Async".
# The second call to MethodAsync will take 1 sec to run and will block the main thread as the AWAIT keyword is used.
# var3 will be equal to "Hello Await Async".

# In BaZic, the program wait that all the unwaited asynchronous call are done before considering the program finished.
# Therefore, this program will takes approximately 3 sec to run.

EXTERN FUNCTION Main(args[])
    VARIABLE var1 = Method1("Hello")
    VARIABLE var2 = MethodAsync("Hello Async", 3.0)
    VARIABLE var3 = AWAIT MethodAsync("Hello Await Async", 1.0)
    RETURN "END OF MAIN METHOD"
END FUNCTION

FUNCTION Method1(value)
    RETURN value
END FUNCTION

ASYNC FUNCTION MethodAsync(value, timeToWait)
    VARIABLE var1 = AWAIT System.Threading.Tasks.Task.Delay(System.TimeSpan.FromSeconds(timeToWait))
    RETURN value
END FUNCTION
```

### Complete syntax demonstration

```
# BaZically, this program does nothing interesting...

VARIABLE myVar[] = NEW ["value1", "val2"]

EXTERN FUNCTION Main(args[])
    MyFunction(1, 2, NULL)
END FUNCTION


FUNCTION MyFunction(arg1, arg2, arg3[])
    DO
        VARIABLE x = 1 + 2 * (3 + 4 + 5)
        x = myVar[0]
        x = "This is a
										               string
on several lines with spaces and tabs" + x.ToString()
        x = 1.ToString()
        myVar["hey"] = "ho" # An array variable can use any object as an index.
        BREAK
    LOOP WHILE myVar = arg1 OR (arg1 = arg2 AND arg2 = myVar[0])
    arg2 = NEW System.DateTime()
    RETURN RecursivityFunction(100)
END FUNCTION

ASYNC FUNCTION RecursivityFunction(num)
    IF num > 1 THEN
        num = AWAIT RecursivityFunction(num – 1)
        TRY
            num.ToString() # this is a comment
        CATCH
            THROW NEW System.Exception(EXCEPTION.Message)
        END TRY
    ELSE
        IF NOT num = 1 THEN
            # another comment
            BREAKPOINT
        END IF
    END IF

    RETURN num
END FUNCTION
```

## Program with UI

The `BaZic code`, that provides the logic :

```
BIND ListBox1_ItemsSource[] = NEW ["Value 1", "Value 2"]
BIND TextBox1_Text = "Value to add"

EXTERN FUNCTION Main(args[])
END FUNCTION

EVENT FUNCTION Window1_Closed()
    RETURN "Result of Window.Close"
END FUNCTION

EVENT FUNCTION Button1_Click()
    ListBox1_ItemsSource.Add(TextBox1_Text)
END FUNCTION
```

The `XAML code`, that provides a representation of the user interface

```XML
<Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" Name="Window1">
    <StackPanel>
        <TextBox Name="TextBox1"/>
        <Button Name="Button1" Content="Add a value"/>
        <ListBox Name="ListBox1"/>
    </StackPanel>
</Window>
```

### Explanations

#### Understanding the BIND syntax

In `XAML`, controls such as `Button` or `TextBox` have properties.
A `TextBox` has a property called `Text` that is designed to gets or sets the text typed by the user.

With the syntax `BIND`, we can create a global variable that will automatically get or set the property of the defined control. The name of the variable is used to identify which control and property must be binded.
Therefore, a `BIND` name must always have the following syntax : `ControlNameInTheXaml_PropertyName`.
The default value of the `BIND` statement will be the default value of the property when the user interface is loading.

#### Understanding the EVENT FUNCTION syntax

In `XAML`, controls such as `Button` or `TextBox` have events.
A `Button` has a event called `Click` that calls the linked function when the user, obviously, clicks on the button.

With the syntax `EVENT FUNCTION`, we can link a function to an event. The name of the function is used to identify which control and event must be linked.
Therefore, a `EVENT FUNCTION` name must always have the following syntax : `ControlNameInTheXaml_EventName`.
Those kind of function must **never** take any argument.
An event function **cannot* be `ASYNC`.

##### Special case with the Closed event of the Window

The value returned by the function binded to the ```Closed``` event of the program's ```Window``` will be considered as the program's result.