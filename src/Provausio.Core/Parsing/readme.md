##Creating a stream
The reader uses streams instead of assuming that you want to load everything into memory. This way, if you want to open a file and read through it, you can, and if you want to read an instance of `System.String` or load the file directly into memory, then you're still going to have to get it into some form of Stream before using the reader, and so on. For this example, we will assume that we are reading from a file.

###Create the stream

```csharp
var stream = new StreamReader("c:\sample.txt").BaseStream;
```

##Initializing the parser
In order to allow custom implementations of a parser, the reader requires an instance of `ITextParser`. The default implementation we will be using is an adapter that wraps an instance of `Microsoft.VisualBasic.FileIO.TextFieldParser`. This is a good csv parser that can account for quoted fields. This adapter is located in `Provausio.Infrastrcuture` because `Provausio.Core` is a portable library and cannot reference it directly. Instead, we will use dependency injection with the adapter.

```csharp
var parser = new DefaultTextFieldParser(stream);
```

## Defining a mapper
Depending on your situation, you will need to choose a mapper to use with the reader. The mapper will read each line in the CSV file and map it to an object. A few mappers are available for use, but any custom adapter can be used by implementing `IStringArrayMapper<T>`

For these examples, we will be using the following sample object

```csharp
public class Person
{
	public string FirstName { get; set; }
	public string LastName { get; set; }
	public int Age { get; set; }
}
```
And we will assume that the source file (sample.txt) looks like this:

    FirstName,LastName,Age
    john,smith,21
    jane,smith,22
    jim,smith, 23

###CustomMapper
`Provausio.Core.Parsing.Csv.Mappers.CustomMapper` allows you to define a specific position in the csv to be mapped to a specific property on the target object. The `Define(...)` method implements a builder pattern for fluid syntax. 

```csharp
var mapper = new CustomMapper<TestClass>()
	.Define(0, s => s.FirstName)
	.Define(1, s => s.LastName)
	.Define(2, s => s.Age);
```

If you were to break each line into an array of values (not including the headers), it would look like this

    [0] john
    [1] smith
    [2] 21

The mapper definition, effectively maps each index to a specific property on the target object. Optionally, you may provide a regex pattern that will be used to validate the source data  before it tries to map it to the object. To do so, just include the pattern as a final argument to the `Define(...)` method. This behavior is repeated throughout each mapper implementation:

```csharp
var mapper = new CustomMapper<TestClass>()
	.Define(0, s => s.FirstName, "[a-zA-Z]+")
	.Define(1, s => s.LastName, "[a-zA-Z]+")
	.Define(2, s => s.Age, "^[1-9][0-9]?$|^100$");
```

### ArrayPropertyMapper
`Provausio.Core.Parsing.Csv.Mappers.ArrayPropertyMapper` allows you to use a decorated DTO to define positions instead of explicitly defining them like you do with `CustomMapper`. To use this, simply decorate each property on your target object with `[ArrayProperty]`

```csharp
public class UnvalidatedTestClass
{
	[ArrayProperty(Index = 0)]
	public string FirstName { get; set; }

	[ArrayProperty(Index = 1)]
	public string LastName { get; set; }

	[ArrayProperty(Index = 2)]
	public int Age { get; set; }
}
```

And then initialize the mapper, specifying the type as a generic parameter

```csharp
var mapper = new ArrayPropertyMapper<UnvalidatedClass>();
```

To implement validation, simply include the value as a property on the decorator on your class:

```csharp
public class ValidatedTestClass
{
	[ArrayProperty(Index = 0, ValidationPattern = "[a-zA-Z]+")]
	public string FirstName { get; set; }

	[ArrayProperty(Index = 1, ValidationPattern = "[a-zA-Z]+")]
	public string LastName { get; set; }

	[ArrayProperty(Index = 2, ValidationPattern = "^[1-9][0-9]?$|^100$")]
	public int Age { get; set; }
}
```

###HeaderMapper
`Provausio.Core.Parsing.Csv.Mappers.HeaderMapper` allows you to provide headers directly from the file to be used as maps. It works much like `CustomMapper` in that you define the header to property mapping through the `Define(...)` method. 

```csharp
var mapper = new HeaderMapper<TestClass>()
	.Define("FirstName", t => t.FirstName)
	.Define("LastName", t => t.LastName)
	.Define("Age", t => t.Age);
```
Instead of specifying an explicit index, as was done with `CustomMapper`, you specify the name of the header (or column name) in the file. So if the head is "fname" in the file, but the property on the target object is "FirstName", then you would define it as `.Define("fname", t => t.FirstName);`

As is with `CustomMapper`, you may also specify a regex pattern to validate source data before mapping:

```csharp
var mapper = new HeaderMapper<TestClass>()
	.Define("FirstName", t => t.FirstName, "[a-zA-Z]+")
	.Define("LastName", t => t.LastName, "[a-zA-Z]+")
	.Define("Age", t => t.Age, "^[1-9][0-9]?$|^100$");
```
	
##Initializing the Reader
Once you've chosen a mapper, you will instantiate the reader. To do so, you will also need to set a few items

 - The parser
 - The mapper
 - Whether or not the file contains quoted fields
 - Whether or not the first line of the file contains headers
 - A list of delimiters 

```csharp 
var reader = new DelimitedStringReader(parser, mapper, false, true, ",");
```

##Using the reader
The reader has one important field, and one important method that you will use, for the most part:

 - `CurrentLine`: This is the last mapped object that was read from the stream
 - `ReadNext()`:  This will read and map the next line in the stream. Returns bool if there is more data to be read.
 
To read a file from top to bottom is quite simple:

```csharp
var results = new List<User>();    
while (reader.ReadNext())
{
	results.Add(reader.CurrentLine);
}
```


##Complete Example

```csharp
var sr = new StreamReader("c:\sample.txt");
var parser = new DefaultTextFieldParser(sr.BaseStream);
var mapper = new HeaderMapper<User>()
	.Define("FirstName", t => t.FirstName)
	.Define("LastName", t => t.LastName)
	.Define("Age", t => t.Age);
	
var reader = new DelimitedStringReader(parser, mapper, false, true, ",");

var results = new List<User>();
while(reader.ReadNext())
	results.Add(reader.CurrentLine);
```

## Using the fluid interface

```csharp
var sr = new StreamReader("c:\sample.txt");
var parser = new DefaultTextFieldParser(sr.BaseStream);
	
var reader = new DelimitedStringReader(parser, false, true, ",")
    .UseMapper<HeaderMapper<User>>()
    .Define("FirstName", t => t.FirstName)
    .Define("LastName", t => t.LastName)
    .Define("Age", t => t.Age)
    .GetReader();

var results = new List<User>();
while(reader.ReadNext())
	results.Add(reader.CurrentLine);
```