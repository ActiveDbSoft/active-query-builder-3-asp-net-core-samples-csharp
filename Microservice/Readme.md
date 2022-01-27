# Scalable stateless multi-container Redis-backed demo for Active Query Builder ASP.NET
​
This demo allows for building and executing SQL queries for an unlimited number of users simultaneously with load balancing across several workers. 
​
The demo consists of the following services:
​
- MySQL database server with a demo AdventureWorks DB.
- Redis for centralized storage of the Query Builders' client state.
- One or more Query Builder services (ASP.NET Core application).
- One or more Query Executor services (**can be of any architecture**; here it's a Node.js application).
- Nginx as a static web server and a reverse proxy for routing requests to the Query Builder and Query Executor services.
​
## How to start
​
Run the following command to build the demo.
​
    docker-compose up --scale api=N 
​
where `N` is the number of Query Builder service instances involved. 
You can also play with the number of Query Executor services.
​
## Client Configuration
​
**To let users simultaneously build different queries in different browser tabs**, set the `allowMultipleInstancesPerUser` flag to True in the `nginx/www/index.html` file.
​
## Server configuration
​
You can configure the Query Builder service via a configuration file located in the `/QueryBuilderApi/ActiveQueryBuilder.json` file.
​
Query Builder service doesn't have a live connection to the database. Instead, it uses the pre-generated XML file with all the necessary information about the database schema.
​
You can build your configuration and metadata XML files using the Configuration Wizard Tool (Windows desktop GUI application) in the installation package or installed separately from [the download page](https://www.activequerybuilder.com/download.html). 
​
## Principle of work
​
1. When the user opens the web page, Nginx returns the static content, and JavaScript initializes the QueryBuilder client. Then a query to initialize the server-side instance is being sent. Nginx routes this query to the Query Builder application, which requests the Redis storage to get the state. The application creates an instance of the QueryBuilder object and returns the necessary data to complete the client's component initialization. 
​
2. This procedure repeats every time the user changes the query. There could be several Query Builder workers, and the load is distributed by the Docker using the round-robin DNS technique.
​
3. When the user requests the data, the client's request is routed by Nginx to the Query Executor application, which gets the SQL query from the Query Builder, not from the client, for safety reasons. The Query Executor also handles requests to get the total number of rows calculated using a separate query requested from the Query Builder app. 
​
Tokens stored in the client's LocalStorage are used as keys to get access to the client state in the Redis storage. They are passed via the `query-builder-token` http header. Redis key consists of the token plus the instanceId which uniquely identifies a QueryBuilder instance within the user session. If the  `instanceId` value is a constant, the user will see the same state as the previous tab when opening a new browser tab. If it is generated anew each time, the user will start building a new (empty) request when a new window is opened.
​
The Redis classes RedisQueryBuilderProvider and RedisQueryTransformerProvider are used to store the QueryBuilder and QueryTransformer states. In order to prevent race condition (when two queries access the same state within a short period of time and in parallel change this state, which leads to incorrect behavior of the application) the `instanceId:token-key` locks are used (by means of the RedLockNet library).
​
To demonstrate load balancing, the query builder application gives the HTTP header `Processed-By-Id` in each response, this is a unique integer application identifier which you can see on the Network tab in the developer tools.
​
## QueryBuilder app API description
​
    GET /QueryBuilder/CheckToken?token={token}&instanceId={instanceId}
​
Returns an empty string if there is a component state in the Redis repository with the `token:instanceId` key. If there is no value for that key, it generates and returns a new token.
​
    GET /QueryBuilder/CreateQueryBuilder?name={instanceId}
​
Creates a new QueryBuilder instance with the given `instanceId`.
Requires HTTP header with a token value in the `query-builder-token` key.
​
    POST /QueryBuilder/getSql 
​
Returns the SQL query text with the requested sorting and pagination applied.
Accepts a request body of the form: `{ "pagenum": <int>, "pagesize": <int>, "sortdatafield": <string>, "sortorder": <string>, "instanceid": <string> }`.
Requires HTTP header with a token value in the `query-builder-token` key.
​
    POST /QueryBuilder/getRecordCountSql 
​
Returns the SQL query text to query the total records number for the current query result dataset. 
Example: `Select count(*) as recCount ...`
Accepts a request body of the form: `{ "instanceid": <string> }`.
Requires HTTP header with a token value in the `query-builder-token` key.
​
​
## Описание API приложения query executor:
​
    POST /getData 
​
Returns the query result data in the form: `{[{"field1": "value1", "field2": "value2", ...}, {"field1": "value1", "field2": "value2", ...}, {"field1": "value1", "field2": "value2", ...}, ...]}`.
Accepts a request body of the form: `{ "pagenum": <int>, "pagesize": <int>, "sortdatafield": <string>, "sortorder": <string>, "instanceid": <string>, "params": [{"name": <string>, "value": <string>}, ...] }`.
Requires HTTP header with a token value in the `query-builder-token` key.
​
    POST /getRecordsCount 
​
Returns the number of lines in the resulting dataset.
Accepts a request body of the form: `{ "instanceid": <string>, "parameters": [{"name": <string>, "value": <string>}, …] }`.
Requires HTTP header with a token value in the `query-builder-token` key.