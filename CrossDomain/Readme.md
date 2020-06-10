# Active Query Builder Cross-domain request demo
​
This demo is dedicated to configuring the component to run the backend on a separate host. 
​
Please consider setting up CORS on the server in order to operate properly.
To enable CORS for Active Query Builder in a .NET Core project, add the following line **as the first line** to the *Configure* method of the `Startup.cs` file:
```
  app.UseCors(
  	b => b.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin() 
  );
```
​
## Running the project regularly
​
To run this demo, run two web servers:
​
- Server A: ASP.NET server running this demo project.
- Server B: Simple HTTP server returning the `./FrontEnd/index.html` page.
   This might be any static HTTP server, for example https://www.npmjs.com/package/static-server
​
    In the `index.html` file, specify the *host address of the server A* via the **AQB.Web.host** property.
​
Run the browser and open the `/index.html` page from server B.
​
​
## Running the back-end in a Docker container
​
To run Active Query Builder in a docker container, do the following.
​
1. Configure the component via the configuration  file (see below) 
2. Run the following commands to create a docker image and run the container on the port 8080:
```	
    $ docker build -t crossdomain .
	$ docker run -d -p 8080:80 --name crossdomaindemo crossdomain
```
​
3. To the attach the front-end to it, change host in the `\FrontEnd\index.html` by setting  the **AQB.Web.host** property to `http://localhost:8080` and run it as a static web server.
​
That's all. Run the browser and open the `/index.html` page from the static HTTP server.
​
## Configuring the component
​
### Configuring the source of information about the database schema
​
The source of information about database schema must be configured via the `ActiveQueryBuilder.json` file.
​
Find the `aspQueryBuilder` node and define the XML with pre-loaded metadata information or setup live connection to the database.
​
Sample of the JSON file metadata specification: 
​
    "aspQueryBuilder": {
        "common": {
            "httpCompression": "true",
            "persistentConnection": "false"
        },
        "syntaxProvider": {
            "type": "ActiveQueryBuilder.Core.MSSQLSyntaxProvider, ActiveQueryBuilder.Core"
        },
        "metadataSource": {
            "xml": "C:\\Work\\ActiveQueryBuilderASPNET_3.0\\src\\Samples\\Sample databases\\AdventureWorks.xml"
        }
​
*Note* that the path to the XML file must be **absolute**.
​
Sample of the live database connection specification:
​
	"aspQueryBuilder": {
        "common": {
            "httpCompression": "true",
            "persistentConnection": "false"
        },
        "syntaxProvider": {
            "type": "ActiveQueryBuilder.Core.MSSQLSyntaxProvider, ActiveQueryBuilder.Core"
        },
        "metadataProvider": {
            "type": "ActiveQueryBuilder.Core.MSSQLMetadataProvider, ActiveQueryBuilder.MSSQLMetadataProvider"
        },
        "metadataSource": {
            "dbConnection": {
                "type": "System.Data.SqlClient.SqlConnection, System.Data",
                "connectionString": "data source=.\\sqlexpress; integrated security=true"
            }
