# REPL (Read-Eval-Print Loop)

This guide covers the REPL functionality of dotnet-exec, which provides an interactive C# execution environment.

## Interactive Mode

The REPL provides an interactive C# execution environment that allows you to execute C# code line by line, similar to other interactive shells.

### Starting REPL

REPL mode starts automatically when no script is provided:

```sh
# Start REPL mode
dotnet-exec

# Start REPL with specific references
dotnet-exec --reference "nuget:Newtonsoft.Json"

# Start REPL with web framework
dotnet-exec --web

# Start REPL with configuration profile
dotnet-exec --profile myprofile
```

### REPL Commands

Once in REPL mode, you can use several special commands:

| Command | Description | Example |
|---------|-------------|---------|
| `#q` or `#exit` | Exit REPL session | `#q` |
| `#cls` or `#clear` | Clear screen | `#cls` |
| `#help` | Show help information | `#help` |
| `#r <reference>` | Add reference | `#r nuget:CsvHelper` |
| `<expression>?` | Get completions | `Console.?` |
| `<expression>.` | Show members | `DateTime.` |
| `\` (line ending) | Continue on next line | `var x = 1 \` |

### Interactive Features

#### Basic Code Execution

```csharp
> var message = "Hello, World!";
> Console.WriteLine(message);
Hello, World!

> DateTime.Now
[1/15/2024 10:30:45 AM]

> Math.Sqrt(16)
4
```

#### Multi-line Input

Use `\` at the end of a line to continue on the next line:

```csharp
> var numbers = new[] { 1, 2, 3, 4, 5 } \
* .Where(x => x % 2 == 0) \
* .ToArray();
> numbers
int[2] { 2, 4 }
```

#### Auto-completion

End expressions with `?` or `.` to get completions:

```csharp
> Console.?
WriteLine
Write
ReadLine
ReadKey
...

> DateTime.
Now
Today
UtcNow
DaysInMonth
...
```

#### Dynamic Reference Loading

Add references during REPL session:

```csharp
> #r nuget:Newtonsoft.Json
Reference added

> using Newtonsoft.Json;
> JsonConvert.SerializeObject(new { Name = "John", Age = 30 })
"{"Name":"John","Age":30}"

> #r nuget:CsvHelper,30.0.0
Reference added

> #r /path/to/local/library.dll
Reference added
```

### REPL Configuration

#### Starting with Predefined Configuration

```sh
# REPL with web references
dotnet-exec --web

# REPL with specific framework
dotnet-exec --framework Microsoft.AspNetCore.App

# REPL with multiple references
dotnet-exec --reference "nuget:Dapper" --reference "nuget:MySql.Data"
```

#### Using Configuration Profiles

Create a profile for common REPL scenarios:

```sh
# Create web development REPL profile
dotnet-exec config set-profile web-repl \
  --web \
  --reference "nuget:Dapper" \
  --reference "nuget:Microsoft.EntityFrameworkCore.SqlServer" \
  --using "Microsoft.AspNetCore.Mvc" \
  --using "Microsoft.EntityFrameworkCore"

# Start REPL with profile
dotnet-exec --profile web-repl
```

### Advanced REPL Usage

#### State Persistence

Variables and types defined in previous statements are available in subsequent ones:

```csharp
> class Person { public string Name { get; set; } public int Age { get; set; } }
> var john = new Person { Name = "John", Age = 30 };
> john.Name
"John"

> void PrintPerson(Person p) => Console.WriteLine($"{p.Name} is {p.Age} years old");
> PrintPerson(john);
John is 30 years old
```

#### Exception Handling

REPL gracefully handles compilation and runtime errors:

```csharp
> int x = "invalid";
(1,9): error CS0029: Cannot implicitly convert type 'string' to 'int'

> throw new Exception("Test");
System.Exception: Test
   at Submission#1.<<Initialize>>d__0.MoveNext()
```

#### Integration with External Services

```csharp
> #r nuget:System.Net.Http.Json
Reference added

> var client = new HttpClient();
> var response = await client.GetFromJsonAsync<dynamic>("https://api.github.com/users/octocat");
> response.name
"The Octocat"
```

The REPL provides a powerful interactive environment for rapid C# development, testing, and exploration. For information about the internal architecture of dotnet-exec, see the [Architecture Guide](architecture.md).