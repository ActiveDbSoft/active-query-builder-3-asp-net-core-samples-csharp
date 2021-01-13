function JqxGridDataExplorer() {
    DataExplorer.apply(this, arguments);
    this._setLinkToDataExplorerSite('https://www.jqwidgets.com');
}

JqxGridDataExplorer.prototype = Object.create(DataExplorer.prototype);
JqxGridDataExplorer.prototype.constructor = JqxGridDataExplorer;

JqxGridDataExplorer.prototype.create = function (columns) {
    var self = this;
    this.isInit = true;

    this._source = {
        type: 'POST',
        contentType: 'application/json',
        datatype: 'json',
        beforeSend: this.beforeSend,        
        url: this.dataUrl,
        formatData: function (data) {
            data.params = self.getParams();
            data.instanceId = self.instanceId;
            return JSON.stringify(data);
        },
        sort: function () {
            $("#jqxgrid").jqxGrid('updatebounddata');
        },
        datafields: columns.map(function (c) {
            return { name: c.Name }
        }),
        totalrecords: this._recordsCount
    };

    this._dataAdapter = new $.jqx.dataAdapter(this._source);

    if ($("#jqxgrid").length)
        $("#jqxgrid").jqxGrid('destroy');

    this.container.append('<div id="jqxgrid"></div>');

    $("#jqxgrid").jqxGrid({
        width: '100%',
        source: this._dataAdapter,
        pageable: true,
        sortable: true,
        virtualmode: true,
        rendergridrows: function () {
            return self._dataAdapter.loadedData;
        },
        columns: columns
    });
}

JqxGridDataExplorer.prototype.update = function () {
    this._dataAdapter.dataBind();
}

JqxGridDataExplorer.prototype.clear = function () {
    $("#jqxgrid").jqxGrid('clear');
}

JqxGridDataExplorer.prototype._updateRecordsCount = function (count) {
    this._recordsCount = count;

    if (this._source)
        this._source.totalrecords = count;
}