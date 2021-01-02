const express = require('express');
const mysql = require('mysql');
const fetch = require('node-fetch');

process.env.PORT = "8081";
process.env.QB_API_HOST = "[::1]:26430";
process.env.MYSQL_HOST = "localhost";
process.env.MYSQL_USER = "root";
process.env.MYSQL_DATABASE = "adventureworks";
process.env.MYSQL_PASSWORD = "3903312";

const QB_API_HOST = process.env.QB_API_HOST;

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

const app = express();
app.use(express.urlencoded());
app.use(express.json());

app.post('/getData', (req, res) => {
    const request = { ...req.body };

    const token = request.Token;
    if (token == null) {
        res.status(404).send('Not found');
        return;
    }    
        
    const { Params: params, ...getSqlRequest } = request;
    fetch('http://' + QB_API_HOST + '/getSql', {
        method: 'POST',
        body: JSON.stringify(getSqlRequest),
        headers: new fetch.Headers({'content-type': 'application/json'})
    })
        .catch(r => {
            console.log(r);
        })
        .then(res => res.text())
        .then(sql => {
            dbConnection.connect();

            dbConnection.query(sql, params, (error, results, fields) => {

                console.log(results);
                console.log(fields);

                if (error != null)
                    res.status(500).send(JSON.stringify({ error: error.sqlMessage }))     
                else
                    res.status(200).send(JSON.stringify(prepareData(results, fields)));

                dbConnection.end();
            });
        });
});

const prepareData = (data, fields) => {
    const result = [];

    for (let dataRow of data) {
        const row = {};

        for (let i = 0; i < dataRow.length; i++) {
            row[fields[i].name] = dataRow[i];
        }

        result.push(row);
    }

    return result;
}

const server = app.listen(process.env.PORT);
server.on('close', () => {
    if (dbConnection.state === 'connected')
        dbConnection.end();
})
