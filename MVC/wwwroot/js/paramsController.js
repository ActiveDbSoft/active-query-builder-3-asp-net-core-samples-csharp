function ParamsController(container) {
    this.container = container;

    var table = $('<table class="table table-striped">');
    var thead = $('<thead><tr><th scope="col">Parameter</th><th scope="col">Value</th></tr></thead>');
    this._tbody = $('<tbody>');

    table.append(thead);
    table.append(this._tbody);

    this.container.append(table);
}

ParamsController.prototype.create = function (params) {
    for (var i = 0; i < params.length; i++) {
        var tr = $('<tr>');
        var name = $('<td>' + params[i].FullName + '</td>');
        var value = $('<td><input type="text" class="' + params[i].Name + '" /></td>');
        tr.append(name).append(value);
        this._tbody.append(tr);
    }

    this.container.show();
};

ParamsController.prototype.clear = function () {
    this.container.hide();
    this._tbody.empty();
}