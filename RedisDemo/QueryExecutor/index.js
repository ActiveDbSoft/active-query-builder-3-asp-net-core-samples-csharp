const express = require('express');
const mysql = require('mysql');
const QueryBuilderApi = require('./queryBuilderApi');

/*process.env.PORT = "8081";
process.env.QB_API_HOST = "[::1]:26430";
process.env.MYSQL_HOST = "localhost";
process.env.MYSQL_USER = "root";
process.env.MYSQL_DATABASE = "adventureworks";
process.env.MYSQL_PASSWORD = "3903312";
*/

const QB_API_HOST = process.env.QB_API_HOST;

const createConnection = () => {
    const dbConnection = mysql.createConnection({
        host: process.env.MYSQL_HOST,
        user: process.env.MYSQL_USER,
        database: process.env.MYSQL_DATABASE,
        password: process.env.MYSQL_PASSWORD
    });

    dbConnection.config.queryFormat = function (query, values) {
        if (!values) 
            return query;
    
        return query.replace(/\:(\w+)/g, function (txt, key) {
          if (values.hasOwnProperty(key)) {
            return this.escape(values[key]);
          }
          return txt;
        }.bind(this));
    };

    return dbConnection;
}

const getToken = (req) => req.header('query-builder-token');
const api = new QueryBuilderApi(QB_API_HOST);

const app = express();
app.use(express.json());

app.post('/getData', (req, res) => {
    const request = { ...req.body };
    const token = getToken(req);
        
    const { Params: params, ...getSqlRequest } = request;

    api.getSql(getSqlRequest, token)
        .then(res => res.text())
        .then(sql => {
            const dbConnection = createConnection();
            dbConnection.connect();

            dbConnection.query(sql, prepareParams(params), (error, results, fields) => {
                if (error != null)
                    res.status(500).send(JSON.stringify({ error: error.sqlMessage }))     
                else
                    res.status(200).send(JSON.stringify(results));

                dbConnection.end();
            });
        });
});

app.post('/getRecordsCount', (req, res) => {
    const { parameters, instanceId } = req.body;
    const token = getToken(req);

    api.getRecordCountSql(token, instanceId)
        .then(res => res.text())        
        .then(sql => {
            const dbConnection = createConnection();
            dbConnection.connect();

            dbConnection.query(sql, prepareParams(parameters), (error, results, fields) => {
                if (error != null)
                    res.status(500).send(JSON.stringify({ error: error.sqlMessage }))     
                else
                    res.status(200).send(extractRecordsCount(results).toString());

                dbConnection.end();
            });
        });
});

const extractRecordsCount = (results) => {
    const record = results[0];

    return record[Object.keys(record)[0]];
}

const prepareParams = (params) => {
    if (params == null)
        return {};

    const result = {};
    for (let param of params)
        result[param.name] = param.value;

    return result;
}

const server = app.listen(process.env.PORT);
server.on('close', () => {
    if (dbConnection.state === 'connected')
        dbConnection.end();
})
