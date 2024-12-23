# Playground
Purpose of this repository is to showcase how to use database code generator with some of the examples. Other than that, it serves as a kind of a template for future API projects.

## Preconditions
In order to evaluate the example project user should first install or review CORE_VS_PLUGIN. As the name suggests, it's a plugin for Visual Studio which enables user with commands to generate code.
This example project (Playground) will contain already generated code that servers as an usage example.

## Project structure
- Project structure starts from the plugin libraries first. User can choose between available database plugins (Core.DB.Plugin.MySQL/MSSQL/PostgreSQL) and import it.
- Plugin library needs the shared utilities found within Core.DB.Plugin.Shared library.
- Core.Shared library contains shared code that can be used between multiple API's in the future.
- Core.Auth is not necessary, it's an example library that will contain shared logic for authentication and authorization. It will be implemented later.
- CoreDB serves as an example of a business logic layer, responsible for handling data processing, database interactions, and related operations.
- At last there is Playground API. Example of an API and it's current form represents a project template.

### Core.DB.Plugin.$DATABASE_PROVIDER
- Provides a class that encapsulates most commonly used database operations (CRUD) and contains code specific to the database in use along with it's library (nuget library made by the database provider). This class is called DBTable.cs.
- This library also provides abstract controller (to be used in API's) and encapsulates database access within each request.
- Each plugin offers same classes where only implementation is different due to the different database being used.
- Idea of this plugin is that user can extend it's functions at any time and already existing generated code doesn't have to change, it will automatically have new functions available.

### CoreDB
- Serves as an example of business logic layer with code examples as well.

### Playground API
- Contains also an abstract controller which extends the one provided by the plugin library. It will setup the connection string and provide optional use of custom authentication.
- Contains an example controller which shows how the abstract controller can be used.
- Contains a version controller that can be used to check current version of the API after deployment
- Program.cs contains example of API initialization

# How to use database code generating plugin?

- First of all plugin needs to be installed. After that there will be new commands found in the Visual Studio under Tools. There are options to generate code for specific database. This will open a popup where user must choose it's configuration file that contains info necessary for plugin to run.

- Here is the example of the file
```
{
	"ORM_Location": "path_to_the_location_where_code_will_be_generated_at",
	"ORM_Namespace": "namespace_of_the_generated_classes",
	"ConnectionString": "connection_string_to_the_database"
}
```
- After that user can just click on the command and plugin will handle the rest. At anytime user can execute the same command and it will generate the code again with the latest changes from the database.
- The plugin will load the current state of the database and generate code accordingly.
- *NOTE:* VS plugin will first delete the folder specified in the configuration file, create it again, and then place files within.
