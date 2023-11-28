# SuperUnit .NET Testing Library

SuperUnit is a Unit Testing language. Currently supports all .NET Languages ( C#, F#, and VB ), however only integration with C# has been extensively tested.

## How To Use

To get started, create a new file in your favourite code editor, by convention ending in '.superunit'. A typical SuperUnit file starts by importing all necessary assemblies (the System assembly is included by default). Optionally, you can import namespaces, allowing you use `Exception` instead of `System.Exception`, for example. Note the double backslashes in the path: the SuperUnit compiler supports string escape sequences, so two backslashes are needed for every one.

```csharp
using "C:\\...\\MyPackage.dll";

using namespace System;
using namespace System.Numerics;
```

At this point, you can optionally declare a test group. This has no effect on the test itself. However, having test groups improves the formatting of the test result output.

```
test_group MyExampleTestGroup;
```

All tests start with the `do` keyword, followed by the class name (preceded by any necessary namespace names), an arrow, then the method name. Test cases are represented as a set of parameters passed to the method, and an expected result. For most tests, this takes the form of a return value, indicated by the `expect` keyword. By convention, cases each have their own line and are indented, however the interpreter is not whitespace sensitive so this isn't compulsory.

```csharp
do Mathf->Sum:
    case (5, 2) expect 7,
    case (7, 1) expect 8;

:: To reduce ambiguities, you can statically type method invocations
do Mathf->Sum(float, float):
    case (6.6f, 2.1f) expect 8.7f,
    case (2.7f, 1f) expect 3.7f;
```

In some cases, it may be that you are expecting an error to be thrown. In this case, the `throws` keyword can be used, followed by the __type__ of the error thrown. If an error of the type specified is thrown, the test is marked as passing. If the method call does not throw an error, or the error is not of the specified type, then the test is marked as failing. Note that this is checking for an exact match; in the below test, if a hypothetical Sqrt function threw `InvalidOperationException` for negative values, the test would be marked as failing, even though `InvalidOperationException` derives from `Exception`.

```csharp
do Mathf->Sqrt(int):
    case (-1) throws Exception, :: This would fail
    case (-1) throws InvalidOperationException; :: This would pass
```

Object creation is supported via constructors using the `new` keyword, and are able to be used as both method parameters and expected return values.

```csharp
do IntWrapper->Wrap:
    case (5) expect new IntWrapper(5),
    case (-5) expect new IntWrapper(-5);

do IntWrapper->UnWrap:
    case (new IntWrapper(5)) expect 5,
    case (new IntWrapper(-5)) expect 5;
```

If the tested method is not a static method, you can use the `on` keyword to specify the target of the method. It is important to note that this object is regenerated for each test case, so any side effects of method calls will be lost.

```csharp
test_group Vector2s;

do int->CompareTo:
    on 5:
        case (3) expect -1,
        case (5) expect 0,
        case (7) expect 1,
    on 10:
        case (10)

do Vector2->Magnitude:
    on new Vector2(3f, 4f):
        case () expect 5f,
    
    :: Type inference in this context is supported
    on new (30f, 40f):
        case () expect 50f;
```

## Sample Unit Test

```csharp
:: sample_tests.superunit

using "C:\...\MyPackage.dll";

using namespace System;
using namespace System.Numerics;

test_group MyExampleTestGroup;

do Mathf->Sum(int, int):
    case (5, 2) expect 7,
    case (7, 1) expect 8;

do Mathf->Sum(float, float):
    case (6.6f, 2.1f) expect 8.7f,
    case (2.7f, 1f) expect 3.7f;

do Mathf->Sqrt(int):
    case (4) expect 2,
    case (-1) throws Exception;

test_group Vector2Class;

do Vector2->Add:
    on new (5f, 5f):
        case (new Vector2(2f, 2f)) expect new Vector2(7f, 7f),
        case (new Vector2(1f, 1f)) expect new Vector2(6f, 6f);
```
## Running Your Test

In order to run your test, first navigate to the containing folder in the command prompt.

```
C:\Users\Steve> cd C:\Users\Steve\MyCSharpProject
```

Next, use the `superunit` command, followed by the name of the test file.

```
C:\Users\Steve\MyCSharpProject> superunit my_test.superunit
```

## Other Language Features

### Raw Strings

Raw strings, denoted by an ampersand '&', tells the interpreter to ignore escape sequences, such as '\\\\' or '\\n'.

```csharp
:: Both lines yield an identical result
using "C:\\...\\MyPackage.dll";
using &"C:\...\MyPackage.dll";
```

### Numeric Postfixes

In .NET, there are many different types for both integers and floats. By default, the SuperUnit parser will default to Int32 and Double, consistent with .NET compilers, however most .NET numeric types are supported.

| Type                  | Postfix | Example  |
|-----------------------|---------|----------|
| `short` (`Int16`)     | `s`     | `42s`    |
| `int` (`Int32`)       | `i`     | `51i`    |
| `uint` (`UInt32`)     | `u`     | `13u`    |
| `long` (`Int64`)      | `l`     | `71l`    |
| `byte` (`Byte`)       | `b`     | `85b`    |
| `float` (`Single`)    | `f`     | `5.2f`   |
| `double` (`Double`)   | `d`     | `1.95d`  |
| `decimal` (`Decimal`) | `m`     | `2.712m` |

## Library Limitations
- Null and nullable types are not yet supported due to issues regarding nullable reference types in C#
