# ArgumentParser
A simple and lightweight utility for parsing command line arguments.

## Terms
- Argument: A string out of the args collection provided to the program when starting.
- Parameter: A description of the value given to the program. (Thus, arguments are the content of parameters)

## Style
ArgumentParser expects the following format:\
`<'key' 'value'> <'flag'> <'required'> <'optional'>`

## Parameters
- All parameters are declared to the parser with a string identifier that is used for accessing the values after parsing the arguments.
- All parameters can have any number of aliases, as long as they are distinct (the identifier is also treated as an alias).

### Named
#### Mapping
- Named arguments are expected before any required or optional ones. The ordering of flags and named parameters however, does not matter.
- Named arguments must consist of a key-value pair, where the key matches an alias of a named parameter and the value will be parsed to the desired data type.
- Named arguments are always optional.
#### Declaring
- Named parameters can be specified with a custom parse function and a default value.

### Flag
#### Mapping
- Flag arguments are expected before any required or optional ones. The ordering of flags and named parameters however, does not matter.
- Flag arguments must consist of a single string matching an alias of a flag parameter. The parameter value is `true`, if the flag is present and `false`, if it is not.
- Flag arguments are always optional.
#### Declaring
- Flag parameters can only be specified with any number of aliases.

### Required
#### Mapping
- Required parameters are positional.
- Required arguments are expected after any flags and named parameters.
- The first argument that does not match any flag or named parameter will be mapped to the first required parameter.
#### Declaring
- Required parameters can be specified with a custom parse function.
- Required parameters have only one alias - the identifier.

### Optional
#### Mapping
- Optional parameters are positional.
- Optional arguments are expected last.
#### Declaring
- Optional parameters can be specified with a custom parse function.
- Optional parameters have only one alias - the identifier.

## Reading Values
The parameter values can be accessed on the parser object either using the generic `Parser.Value<T>(string name)` function, with the alias as the `name` or the indexer Function `Parser[string key]` and the generic `Parameter.Vaue<T>()` on the returned parameter.

## Example
```C#
static void Main(string[] args)
{
      Parser p = new Parser(
          Parameter.Flag("-v", "--verbose"),
          Parameter.Named("-l", LogLevel.Error, arg => Enum.Parse(typeof(LogLevel), arg), "--loglevel"),
          Parameter.Required("name", arg => arg),
          Parameter.Optional("port", 8080, arg => int.Parse(arg))
      );
      // use this, if you want to allow undefined arguments
      List<string> unmapped = p.Map(args);
      // use this, if you want to allow only defined arguments
      bool success = p.MapStrict(args, out string helpText);
      if (!success) Console.WriteLine(helpText);

      Console.WriteLine($"Verbose output is {(p.Value<bool>("-v") ? "enabled" : "disabled")}");
      Console.WriteLine($"Log level is {p["-l"].Value<LogLevel>()}");
      Console.WriteLine($"Name is {p.Value<string>("name")}");
      Console.WriteLine($"Port is {p.Value<int>("port")}");

}
```
### Setting up the parser
You can specify the parameters in any order. However, it is best practice to specify the positional parameters last.
```C#
          Parameter.Flag("-v", "--verbose"),
```
A flag parameter is specified with the identifier `"-v"` and one additional alias `"--verbose"`. You could also specify more (or less, but at least one) aliases.
```C#
          Parameter.Named("-l", LogLevel.Error, arg => Enum.Parse(typeof(LogLevel), arg), "--loglevel"),
```
A named parameter is specified with the identifier `"-l"`, the default value of `LogLevel.Error`, a lambda function using the `Enum.Parse(...)` method and one additional alias.
```C#
          Parameter.Required("name", arg => arg),
```
A required parameter is specified with the identifier `"name"` and an identity parse function. In this case you could also omit the parse function, as all parameters default the parse function to the identity, if omitted.
```C#
          Parameter.Optional("port", 8080, arg => int.Parse(arg))
```
An optional parameter is specified with the identifier `"port"`, the default value of `8080` and a lambda function using the `int.Parse(...)` method.

### Mapping the arguments
```C#
      // use this, if you want to allow undefined arguments
      List<string> unmapped = p.Map(args);
```
The permissive method maps all arguments to the parameters defined on the parser. If more arguments are provided than there are parameters, the additional arguments are returned as a list. If required parameters are specified, but not enough arguments provided to map all of them, an `InvalidOperationException` is thrown.
```C#
      // use this, if you want to allow only defined arguments
      bool success = p.MapStrict(args, out string helpText);
      if (!success) Console.WriteLine(helpText);
```
The strict method maps all arguments to the parameters defined on the parser. If more arguments are provided than there are parameters, the method returns false and provides a `helpText` string that can be displayed to the user. If required parameters are specified, but not enough arguments provided to map all of them, an `InvalidOperationException` is thrown.

### Accessing the values
```C#
      Console.WriteLine($"Verbose output is {(p.Value<bool>("-v") ? "enabled" : "disabled")}");
      Console.WriteLine($"Log level is {p["-l"].Value<LogLevel>()}");
      Console.WriteLine($"Name is {p.Value<string>("name")}");
      Console.WriteLine($"Port is {p.Value<int>("port")}");
```
The example shows the two possible ways to access the parameter values and how to output them to the console. Note, that you must specify the type for the generic functions. In most scenarios, the C# compiler can not infer the type for the parameter values.