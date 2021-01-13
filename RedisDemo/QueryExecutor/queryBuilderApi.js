const fetch = require('node-fetch');

module.exports = class QueryBuilderApi {
    constructor(host) {
        this.host = host;
    }

    getUrl(endpoint) {
        return 'http://' + this.host + '/' + endpoint;
    }

    post(endpoint, body, token) { 
        return fetch(this.getUrl(endpoint), {
            method: 'POST',
            body: JSON.stringify(body),
            headers: new fetch.Headers({
                'content-type': 'application/json',
                'query-builder-token': token,
            })
        });
    }

    getSql(body, token) { 
        return this.post('getSql', body, token);
    }

    getRecordCountSql(token, instanceId) {
        return this.post('getRecordCountSql', { instanceId }, token);
    }
}