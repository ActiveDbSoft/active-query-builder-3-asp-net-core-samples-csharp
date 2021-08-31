const fetch = require('node-fetch');
const FormData = require('form-data');

module.exports = class QueryBuilderApi {
    constructor(host) {
        this.host = host;
    }

    getUrl(endpoint) {
        return 'http://' + this.host + '/' + endpoint;
    }

    post(endpoint, body, cookies) { 
        return fetch(this.getUrl(endpoint), {
            method: 'POST',
            body: JSON.stringify(body),
            headers: new fetch.Headers({
                'content-type': 'application/json',
                'host': 'localhost:17835',
                'cookie': cookies
            })
        });
    }

    create(cookies, instanceId) {
        return this.post('create', { instanceId }, cookies);
    }

    getQuery(cookies, instanceId, start, end, sorting) { 
        return this.post('getQuery', { instanceId, start, end, sorting }, cookies);
    }

    getQueryCount(cookies, instanceId) {
        return this.post('getQueryCount', { instanceId }, cookies);
    }
}