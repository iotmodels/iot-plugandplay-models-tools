
# Microsoft IoT Plug and Play Model Resolution Client

## :exclamation: WARNING: This project is under heavy active development and should not be depended on until further notice

## Overview

The model resolution client `ResolverClient` provides functionality for retrieving Digital Twin Definition Language (`DTDL`) models from a device model repository such as the *IoT Plug and Play Device Model Repository* [https://github.com/Azure/iot-plugandplay-models](https://github.com/Azure/iot-plugandplay-models).

## Usage

The client is available in the C# project `Azure.Iot.ModelsRepository` as a `netstandard2.0` library.

> Note. The package is not yet available on NuGet.org.

### Default settings

The following code block shows the basic usage of the `ResolverClient` using default parameters:

```csharp
using Azure.Iot.ModelsRepository;

ResolverClient client = new ResolverClient();
Dictionary<string, string> models = await client.ResolveAsync("dtmi:com:example:Thermostat;1");
```

Without any options the resolver will use the default repository `devicemodels.azure.com`.

The resolver can be customized to use a different repository, local or remote:

```csharp
using Azure.Iot.ModelsRepository;

ResolverClient client = new ResolverClient("https://raw.githubusercontent.com/Azure/iot-plugandplay-models/main");
Dictionary<string, string> models = await client.ResolveAsync("dtmi:com:example:Thermostat;1");
```

To configure the repository from a local folder use an absolute path:

```csharp
using Azure.Iot.ModelsRepository;

ResolverClient client = new ResolverClient("/LocalModelRepo/");
Dictionary<string, string> models = await client.ResolveAsync("dtmi:com:example:Thermostat;1");
```

### DependencyResolutionOption

If the root interface has dependencies with external interfaces, via `extends` or `@component` as described [here](https://github.com/Azure/iot-plugandplay-models-tools/wiki/Resolution-Convention#expanded-dependencies), the client can be configured with the following `DependencyResolutionOption`:

|DependencyResolutionOption|Description|
|--------------------------|-----------|
|Disabled|Do not process external dependencies|
|Enabled|Enable external dependencies|
|TryFromExpanded|Try to get external dependencies using [.expanded.json](https://github.com/Azure/iot-plugandplay-models-tools/wiki/Resolution-Convention#expanded-dependencies)|

The next code block shows how to configure the resolver with a custom `DependencyResolutionOption`

```csharp
using Azure.Iot.ModelsRepository;

ResolverClient rc = new ResolverClient(new ResolverClientOptions(DependencyResolutionOption.Enabled));
Dictionary<string, string> models = await client.ResolveAsync("dtmi:com:example:TemperatureController;1");
```

### Logging

To support traceability and diagnostics, the `ResolverClient` implements the [AzureEventSourceListener](https://docs.microsoft.com/dotnet/api/azure.core.diagnostics.azureeventsourcelistener?view=azure-dotnet)

The following shows an example of how to configure an event source to show logs in the debug output

```csharp
using Azure.Core.Diagnostics;
using System.Diagnostics.Tracing;

using AzureEventSourceListener listener = AzureEventSourceListener.CreateTraceLogger(EventLevel.Verbose);
ResolverClient rc = new ResolverClient();
```

## Integration with the DigitalTwins Model Parser

The `ResolverClient` is designed to work independently of the Digital Twins `ModelParser`.

There are two options to integrate with the parser:

### Resolve before parsing

```csharp
using Azure.Iot.ModelsRepository;
using Azure.Iot.ModelsRepository.Extensions;
using Microsoft.Azure.DigitalTwins.Parser;
using System;
using System.Linq;
using System.Threading.Tasks;

string dtmi = "dtmi:com:example:TemperatureController;1";
ResolverClient rc = new ResolverClient();
var models = await rc.ResolveAsync(dtmi);
ModelParser parser = new ModelParser();
var parseResult = await parser.ParseAsync(models.Values.ToArray());
Console.WriteLine($"{dtmi} resolved in {models.Count} interfaces with {parseResult.Count} entities.");
```

### Resolve while parsing

The parser calls a `DtmiResolver` callback when it finds an unknown `@Id`. To configure the callback to be used from the parser, you can use the sister package
`Azure.Iot.ModelsRepository.Extensions`:

```csharp
using Azure.Iot.ModelsRepository;
using Azure.Iot.ModelsRepository.Extensions;
using Microsoft.Azure.DigitalTwins.Parser;
using System;
using System.Linq;
using System.Threading.Tasks;

string dtmi = "dtmi:com:example:TemperatureController;1";
ResolverClient rc = new ResolverClient(new ResolverClientOptions(DependencyResolutionOption.Enabled));
var models = await rc.ResolveAsync(dtmi);
ModelParser parser = new ModelParser();
parser.DtmiResolver = rc.ParserDtmiResolver;
var parseResult = await parser.ParseAsync(models.Values.Take(1).ToArray());
Console.WriteLine($"{dtmi} resolved in {models.Count} interfaces with {parseResult.Count} entities.");
```

## Error Handling

When the `ResolverClient` hits a problem resolving `DTMI`'s a `ResolverException` will be thrown which summarizes the issue. The `ResolverException` may contain an inner exception with additional details as to why the exception occured.

This snippet from the `CLI` shows a way to use `ResolverException`.

```csharp
try
{
    result = await new ResolverClient().ResolveAsync(dtmi);
}
catch (ResolverException resolverEx)
{
    logger.LogError(resolverEx.Message);
}
```

## Device Model Repository Client

This solution includes a CLI project `Azure.Iot.ModelsRepository.CLI` to interact with local and remote repositories. 

### Install dmr-client

The tool is distributed as source code and requires `dotnet sdk 3.1` to build and install.

#### Linux/Bash

```bash
curl -L https://aka.ms/install-dmr-client-linux | bash
```

#### Windows/Powershell

```powershell
iwr https://aka.ms/install-dmr-client-windows -UseBasicParsing | iex
```

### dmr-client Usage

```text
dmr-client:
  Microsoft IoT Plug and Play Device Models Repository CLI v0.0.17.0

Usage:
  dmr-client [options] [command]

Options:
  --version         Show version information
  -?, -h, --help    Show help and usage information

Commands:
  export      Retrieve a model and its dependencies by dtmi or model file using the target repository for model
              resolution.
  validate    Validates a model using the DTDL model parser & resolver. The target repository is used for model
              resolution.
  import      Validates a local model file then adds it to the local repository.

  
```

## Examples

### dmr-client export

```bash
# Retrieves an interface from the default repo by DTMI

> dmr-client export --dtmi "dtmi:com:example:Thermostat;1"
> dmr-client export --dtmi "dtmi:com:example:Thermostat;1" -o thermostat.json
```

>Note: The quotes are required to avoid the shell to split the param in the `;`

```bash
# Retrieves an interface from a custom  repo by DTMI

> dmr-client export --dtmi "dtmi:com:example:Thermostat;1" --repo https://raw.githubusercontent.com/Azure/iot-plugandplay-models/main
```

### dmr-client import

```bash
# Adds an external file to the `dtmi` folder structure in the current working directory

> dmr-client import --model-file "MyThermostat.json" --local-repo .

# Creates the path `.dtmi/com/example/thermostat-1.json`
```

### dmr-client validate

```bash
# Validates a DTDLv2 model using the Digital Twins Parser and default model repository for resolution.

> dmr-client validate --model-file file.json
```

```bash
# Validates a DTDLv2 model using the Digital Twins Parser and custom repository endpoint for resolution.

> dmr-client validate --model-file ./my/model/file.json --repo "https://mycustom.domain"
```
