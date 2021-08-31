export default class Api {
    constructor(instanceId){
        this.instanceId = instanceId;
    }

    async post(url, body) {
        return await (await fetch(url, {
            method: 'POST',
            body: JSON.stringify(body),
            headers: {
                'content-type': 'application/json',
            },
            credentials: 'include'
        })).json();
    }

    getData(start, end, sorting, params) {
        return this.post('/getData', { instanceId: this.instanceId, start, end, sorting, params });
    }

    getQueryParameters() {
        return this.post('/getQueryParameters', { instanceId: this.instanceId });
    }
}
