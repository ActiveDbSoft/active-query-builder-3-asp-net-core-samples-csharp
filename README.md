# ASP.NET Core C# Demo Projects for [Active Query Builder ASP.NET Edition](https://www.activequerybuilder.com/product_asp.html)

##### Demo projects for Classic ASP.NET reside in a separate repository: [Classic ASP.NET Demo projects repository](https://github.com/ActiveDbSoft/active-query-builder-3-asp-net-samples-csharp).
#
## What is Active Query Builder?
Active Query Builder is a Visual SQL Query Builder and SQL parser component suite for ASP.NET (WebForms, MVC and Core 2.0). 
##### Details:
- [Active Query Builder website](https://www.activequerybuilder.com/),
- [Active Query Builder ASP.NET Edition details page](https://www.activequerybuilder.com/product_asp.html),
- [Live demo website](http://aspquerybuilder.net/).

## How do I get Active Query Builder?
- [Download the trial version](https://www.activequerybuilder.com/trequest.html?request=asp) from the product web site
- Get it by installing the Active Query Builder ASP.NET Edition NuGet package for [MVC](https://www.nuget.org/packages/ActiveQueryBuilder.Web.MVC).

## What's in this repository?
The demo projects in this repository illustrate various aspects of the component's functionality from basic usage scenarios to advanced features. They are also included the trial and full versions of Active Query Builder.
A brief description of each project can be found below.

##### Prerequisites:
- Visual Studio 2017 or higher,
- .NET Standard 2.0 and higher,
- .NET Framework 4.6.2 or higher or ASP.NET Core 2.1 or higher.

##### Dependencies:
- [Active Query Builder Core](https://www.nuget.org/packages/ActiveQueryBuilder.Core/),
- [Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json/) assembly 6.0.4 or higher.

## How to get these demo projects up and running?

1. Clone this repository to your PC.
2. Open the "**Samples.sln**" solution to review the *WebForms and MVC demo projects* 
3. Run the project.

The necessary packages will be installed automatically. In case of any problems with the packages, open the "Tools" - "NuGet Package Manager" - "**Package Manager Console**" menu item and install them by running the following command:
     ```Install-Package ActiveQueryBuilder.Web.MVC```

## Have a question or want to leave feedback?

Welcome to the [Active Query Builder Help Center](https://support.activequerybuilder.com/hc/)!
There you will find:
- End-user's Guide,
- Getting Started guides,
- Knowledge Base,
- Community Forum.

## Contents

All samples are arranged by several demo projects according to the development environment:
- MVC demo project
- Client-side JavaScript rendering project 
- Custom server-side objects storage demo project

The Client-side JavaScript rendering project additionally includes the **React**, **Webpack** and **Electron** demo projects. Their Readme files can be found in appropriate subfolders inside the "JavaScript/Sctipts" folder.

Projects which require a database connection must be properly configured according to the database server being used. 
- Specify the right connection string according to used DBConnection object.
- Choose the syntax and metadata providers which suit your database server and connection method. Read details in this article: [What are the Syntax and Metadata Providers for?](https://support.activequerybuilder.com/hc/en-us/articles/115001063445-What-are-the-Syntax-and-Metadata-providers-for-).

### Getting Started projects

There are some typical basic demos for each of the environments: 
- **The Offline project** which doesn't establish a connection to the database, but reads metadata from the pre-generated XML file (usually from the Northwind database). 
It is ideal for quick acquaintance with the component.
- **The OLE DB project** which must be configured to connect to a database. 
It lets quickly test the component with your database. 
- **The Query Results project** gives an idea how Active Query Builder can be integrated into the data browsing facility. It also illustrates the usage of the **CriteriaBuilder control** along with a result data grid.
- **The Edit Sub-Query Text project** explains how to Edit the Sub-query text and preview sub-query results as if it was a standalone query.

### Designing a user-friendly query building environment
Defining friendly names for database objects and fields, adding meaningful objects, customizing database schema tree.

##### Alternate Names
*Active Query Builder lets substitute unreadable names for user-friendly aliases.*
The Alternate Names feature allows for substitution of unintelligible names of database objects and fields for friendly aliases. End-users see the friendly names in the visual query building interface as well as in the query text when editing it manually. The component gets the query with real names back only when you need to execute the query against a database server. 
Read more about this feature in the Knowledge Base: [Defining friendly names for database objects and fields](https://support.activequerybuilder.com/hc/en-us/articles/115001063565-User-friendly-database-object-and-field-names).


##### Virtual Objects And Fields
*Add objects which act like views and calculated or lookup fields to your database schema.*
This demo illustrates the ability to add queries as new objects to the Database Schema Tree that act like ordinary database views. Users see virtual objects as ordinary database objects and can use them in their queries without the need to understand their complexity. The same is true for virtual fields: you can add new fields and put complex SQL expressions or correlated sub-queries as their expressions. The users will see them as ordinary fields even in the SQL query text. They will be expanded to corresponding SQL expressions only when you need to execute the query against a database server.
Read more about this feature in the Knowledge Base: [Adding virtual objects and calculated fields to the database schema](https://support.activequerybuilder.com/hc/en-us/articles/115001055269-Virtual-objects-and-calculated-fields).


##### Custom Metadata Structure
*Customize your Database Schema View the way you like: group objects by subject area, define folders with favourite objects, etc.*
This demo gives an idea how to build a custom structure of nodes in the Database Schema Tree: Creation of folders for particular objects (Favorites), folders which contain objects according to a mask, etc. 
Read more about this feature: [Customizing the Database Schema Tree structure](https://support.activequerybuilder.com/hc/en-us/articles/115001055289-Customizing-the-Database-Schema-Tree-structure).

##### Toggling Usage of Alternate Names
*Users can see and edit SQL text with both alternate and real object names.*
Advanced SQL writers may wish to turn displaying of friendly aliases off to see the real names in the visual query building interface. This demo project gives the sample for this operation. 


### User-defined Queries and Fields

##### Reusable (User-defined) Queries
*Users can save their queries and use them as data sources in subsequent queries.*
The ability to work save queries to the special repository and then use them in subsequent queries the same way as ordinary views is provided by default. The programmer just has to take care of saving and restoring them between work sessions.

##### User-defined Fields
*Let advanced users create own calculated fields.*
This demo introduces the possibility to define calculated fields by end-users.

### User Interface Customizations

##### Custom Expression Editor
*Define own editor to deal with complex SQL expressions.*
This demo adds a button to the in-place editor of Expression and Criteria cells of the Query Columns List and gives the sample of creating a custom editor to edit the cell content in a popup window. 

##### SQL Syntax Highlighting
*Highlight SQL syntax using a third-party SQL text editor.*
This demo provides the sample of integration of a third-party SQL text editor to enable SQL syntax highlighting.


##### jQueryUI Theming
*Apply any jQueryUI skin to Active Query Builder UI.*
This demo illustrates run-time switching and appliance of jQueryUI themes.


##### Work without the Design Pane
*Build queries without the design pane by dragging fields right to the Query Columns List.*
This demo implements a visual query building interface without the Design Area. Users build queries by dragging fields from the Database Schema Tree to the Query Columns List and define query column properties, such as grouping, ordering, etc. The appropriate database objects are automatically added to the query, get linked with each other and removed when they aren't needed.


### Metadata Loading

##### Load Metadata Demo
*Four ways to fill the Metadata Container programmatically.*
This demo illustrates various ways of loading metadata information to Active Query Builder. 
Metadata handling is described here: [Metadata handling and filtration](https://support.activequerybuilder.com/hc/en-us/sections/115000316525-AQB-for-NET-Metadata-handling-and-filtration).


##### Switch Database Connections
*Switch between different database connections at runtime.*
This demo gives an idea how to get connected to different databases and reload the Database Schema Tree on switching between connections.

### SQL Query Analysis and Modification

##### Query Analysis
*Explore the internal query object model, get summary information about the query.*
This demo contains the code example of a pass through the entire internal query object model.


##### Query Modification
*Modify SQL queries programmatically.*
This demo explains how one can analyze and modify user queries to correct user errors or to limit the data returned by them.

##### Query Creation
*Create SQL queries programmatically from scratch.*
This demo provides code samples of the programmatic creation of various SQL queries.


### Advanced Programming Tasks

##### Redefining the server-side objects storage
*Get rid of the ASP.NET session storage mechanism to maintain the user work sessions.*
The default behaviour of saving the component state within the ASP.NET session can be changed by redefining the storage provider with a few simple steps. Enables the component usage in web farms.
Read more about this feature in the Knowledge Base: [Configuring session state server to use Active Query Builder in a web farm](https://support.activequerybuilder.com/hc/en-us/articles/115001064365-Configuring-session-state-server-to-use-Active-Query-Builder-in-a-web-farm).

##### Handle Query Building Events
*Performing specific actions in the process of building a SQL query.*
This demo gives an idea how to perform custom actions in response to some user actions. The full list of available JavaScript events can be found in this article: [Active Query Builder JavaScript API](https://support.activequerybuilder.com/hc/en-us/articles/115001064225-JavaScript-API-of-Active-Query-Builder-ASP-NET-edition).

##### Handle The Reusable Queries Events
*Performing specific actions in the process of working with Reusable (user-defined) Queries.*
This demo illustrates how to handle addition, renaming and deletion of Reusable Queries, deny or correct user actions.

## License
The source code of the demo projects in this repository is covered by the [MIT license](https://en.wikipedia.org/wiki/MIT_License).
