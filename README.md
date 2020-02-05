# hands-on-clustering

## Prerequisites and setup

Prerequisites: 
    dotnet core 3.1 SDK (check: dotnet --list-sdks)
    VS Code editor
    Ionide VS Code plugin

1) Setup VS Code

Make sure that in Settings, Ionide F# is set to "FSharp.useSdkScripts": true

2) Install paket for dependency management

dotnet new tool-manifest
dotnet tool install paket
dotnet tool restore
dotnet paket init

3) Specify the dependencies

Change paket.dependencies:

```
source https://api.nuget.org/v3/index.json

storage: packages
framework: netcore3.0, netstandard2.0, netstandard2.1
nuget XPlot.GoogleCharts
```

4) Install dependencies

dotnet paket install

5) Go to workshop.fsx