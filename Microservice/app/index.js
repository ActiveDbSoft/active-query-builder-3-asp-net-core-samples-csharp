const express = require('express');
const multipart = require('connect-multiparty');
const mysql = require('mysql');
const QueryBuilderApi = require('./queryBuilderApi');

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

const api = new QueryBuilderApi(QB_API_HOST);

const app = express();
app.use(express.json());

const multipartMiddleware = multipart();
app.post('/createQueryBuilder', multipartMiddleware, (req, res) => {    
    api.create(req.headers.cookie, req.body.name)
    .then(res => res.json().then(json => ({
        headers: res.headers,
        json
      })))
    .then(({ headers, json: { succeeded } }) => {
        res.set('Set-Cookie', headers.get('Set-Cookie'));
        res.status(200).send(JSON.stringify({ succeeded }));
    });
});

app.post('/getData', async (req, res) => {
    const { instanceId, start, end, sorting, params } = req.body;
    const cookies = req.headers.cookie;

    const { query } = await (await api.getQuery(cookies, instanceId, start, end, sorting)).json();
    const { query: countQuery } = await (await api.getQueryCount(cookies, instanceId)).json();

    const dbConnection = createConnection();
    dbConnection.connect();

    dbConnection.query(query, prepareParams(params), (error, results) => {
        if (error != null) {
            dbConnection.end();
            res.status(500).send(JSON.stringify({ error: error.sqlMessage }));
        }            
        else {
            dbConnection.query(countQuery, prepareParams(params), (countError, countResults) => {                
                if (countError != null)
                    res.status(500).send(JSON.stringify({ error: countError.sqlMessage }));
                else {
                    const totalCount = extractRecordsCount(countResults);
                    res.status(200).send(JSON.stringify({ data: results, totalCount }));
                }

                dbConnection.end();
            });
        }        
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
        result[param.name.replace(':', '')] = param.value;

    return result;
}

app.listen(process.env.PORT);
