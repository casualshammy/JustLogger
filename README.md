# JustLogger
Very simple logger I am using in my projects

![GitHub Workflow Status](https://img.shields.io/github/workflow/status/casualshammy/JustLogger/publish-nuget)
![Nuget](https://img.shields.io/nuget/v/JustLogger)
![Nuget](https://img.shields.io/nuget/dt/JustLogger)
## Loggers
There are 2 (3) loggers: `ConsoleLogger`, `FileLogger` and `CompositeLogger`. First and second are self-describing, the third is special logger that can be used like 'container' for multiple loggers
## Examples
```csharp
using var consoleLogger = new ConsoleLogger(); // colourized console logger
consoleLogger.Info("This is info text");
consoleLogger.Warn("This is warning text");
consoleLogger.Error("This is error text");

using var fileLogger = new FileLogger("log.txt", 1000, null); // write buffer = 1000 ms
// or
using var fileLogger = new FileLogger(() => $"{DateTimeOffset.UtcNow}.txt", 1000, null); // filename is produced by function
consoleLogger.Info("Writing to file...");

using var compLogger = new CompositeLogger(consoleLogger, fileLogger);
compLogger.Info("This text will be written to both console and file");

var namedLogger = compLogger["module name"];
namedLogger.Info("Hello world!"); // will produce '[module name] Hello world!' string
```
