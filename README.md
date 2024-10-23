# RUB ICDD Platform
Contains a .NET Core web application for validating and manipulating ICDD data containers. 
For using the project the respective library [IIB.ICDD](https://github.com/philhag/IIB.ICDD) has to be used from GitHub or [NuGet](https://www.nuget.org/packages/ICDDToolkitCore/). The IIB.ICDD framework provides functions in C# .NET 6 to open, create, validate, edit, export containers defined by ISO 21597-1:2020. In ISO 21597-1, a specification is given for a generic container format that stores documents using various formats and structures, along with a means of linking otherwise disconnected data within those documents (including individual parts). These documents can have any syntax and semantics. The container format includes a header file and optional link files that define relationships by including references to the documents, or to elements within them. The header file uniquely identifies the container and its contractual or collaborative intention. This information is defined using the RDF, RDFS, and OWL semantic web standards. Querying these containers with SPARQL as well as using SHACL for validation is provided by this framework.

## License

This software is licensed under [MIT License](/LICENSE).

## Requirements
Requires [.NET 5 SDK](https://docs.microsoft.com/en-us/dotnet/core/dotnet-five).

## Usage
### Client build command (.NET CLI)

```npm install --save-dev webpack webpack-cli```  
```npm run build```

requires npm and node to be installed and is integrated into the build pipeline

### Database update/initialise command (.NET CLI):

```dotnet tool install --global dotnet-ef```

```dotnet ef migrations add InitialiseDB```  
see https://docs.microsoft.com/en-us/ef/core/cli/dotnet#dotnet-ef-migrations-add  
optionally use [Package Manager Console (Powershell)](https://docs.microsoft.com/en-us/ef/core/cli/powershell#add-migration)


```dotnet ef database update```  
see https://docs.microsoft.com/en-us/ef/core/cli/dotnet#dotnet-ef-database-update  
optionally use [Package Manager Console (Powershell)](https://docs.microsoft.com/en-us/ef/core/cli/powershell#update-database)



