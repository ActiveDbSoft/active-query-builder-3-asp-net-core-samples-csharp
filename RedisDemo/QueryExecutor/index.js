const express = require('express');
const mysql = require('mysql');
const fetch = require('node-fetch');

const QB_API_HOST = process.env.QB_API_HOST;

const dbConnection = mysql.createConnection({
    host: process.env.MYSQL_HOST,
    user: process.env.MYSQL_USER,
    database: process.env.MYSQL_DATABASE,
    password: process.env.MYSQL_PASSWORD
  });

const app = express();

app.get('/getData/:token', function(request, response){
    const token = request.params.token;
    if (token == null) {
        response.status(404).send('Not found');
        return;
    }

    fetch(QB_API_HOST + '/QueryBuilder/getSql/' + token)
        .then(res => res.text())
        .then(sql => {
            dbConnection.connect();

            dbConnection.query(sql, (error, results, fields) => {
                if (error != null)
                    response.status(500).send(JSON.stringify({ error: error.sqlMessage }))
                else
                    response.status(200).send(JSON.stringify({ data: results, fields }));

                dbConnection.end();
            });
        });
});

const server = app.listen(process.env.PORT);
server.on('close', () => {
    if (dbConnection.state === 'connected')
        dbConnection.end();
})
