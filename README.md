# Project Issues Suite API

Project Issues Suite (PIS) is designed to be a web application to be used by engineers to upload and view videos primarily regarding how certain bugs or issues are found for other projects.
This is the backend of the whole app.

## Getting Started

### Software Dependencies

- [.NET Core 2.1 (version 2.1.6, sdk 2.1.5)](https://dotnet.microsoft.com/download/dotnet-core/2.1)
- NoSQL with CosmosDB/DocumentDB on MongoDB API

### Nuget Packages

- Microsoft.AspNetCore.App 2.1.5
- Microsoft.Azure.DocumentDB.Core 2.1.2
- NLog.Web.AspNetCore 4.7.0 (for dev environment)

### Installation

Clone this repository:

```c#
git clone https://github.com/jeffvhuang/Project-Issues-Suite-API
cd Project-Issues-Suite-API
```

### Helpful for Development

For Development, the environment variables can be configured to use local emulators instead of the production data when attempting to view or manipulate documents or blobs. Access credentials are the same across all emulators.

The following emulators are used. Download and follow guides on usage via Microsoft's official documentation:

- [Azure Cosmos DB Emulator](https://docs.microsoft.com/en-us/azure/cosmos-db/local-emulator)
- [Azure Storage Emulator](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-emulator)

If intending to use the emulators, make sure to comment and uncomment the appropriate variables in appSettings.json, appSettings.Development.json and appSettings.Production.json files

## Build and Test

1. Build the solution using Visual Studio, or on the command line with `dotnet build`.
2. Run the project. If using command line, run `dotnet run`.
3. Use an HTTP client like Postman or Fiddler to make requests to `http://localhost:44329`.
4. Check the available endpoints with the [swaggerUI in the opened browser](http://localhost:44329/index.html). Otherwise check the swagger OPEN API Specification at [/swagger/v1/swagger.json](http://localhost:44329/swagger/v1/swagger.json).

To run the **tests**: In Visual Studio via the taskbar go to Test > Run > All Tests.

## Production

Subscription to the appropriate resource group on Microsoft Azure is required if changes need to be made to either the PIS API App Service, Azure Cosmos DB account or Storage account.
To simply deploy to production, only access to the PIS Project on TFS is required.

TFS CI/CD pipeline is already set up for all successful release branches to automatically deploy to Azure App Service. The Gitflow workflow design should be followed when making changes to the app:

1. Make pull requests from feature branches into develop branch via TFS.
2. Once changes are merged into develop branch and a release is ready to be created, fetch all committed changes of develop branch onto local machine.
3. Create a release branch `git branch release/vx.x.x` where x should be an integer (eg. `git branch release/v1.9.0`).
4. Push the new branch to remote `git push origin <release_branch_name>`.
5. If the release build is successful on TFS, the release should automatically be deployed to production.
