function JsGridDataExplorer() {
    DataExplorer.apply(this, arguments);
    this._setLinkToDataExplorerSite('http://js-grid.com');

    this._grid = $('<div id="jsgrid"></div>');
        this.container.append(this._grid);
}

JsGridDataExplorer.prototype = Object.create(DataExplorer.prototype);
JsGridDataExplorer.prototype.constructor = JsGridDataExplorer;

JsGridDataExplorer.prototype.create = function(columns) {
    var self = this;
    this.isInit = true;

    this._grid.jsGrid({
        width: "100%",
        height: "400px",
        sorting: true,
        paging: true,
        pageLoading: true,
        pageSize: 10,
        autoload: true,
        fields: columns,
        controller: {
            loadData: function(filter) {
                var d = $.Deferred();

                $.ajax({
                    url: self.dataUrl,
                    type: 'POST',
                    contentType: 'application/json;',
                    dataType: 'json',
                    data: JSON.stringify({
                        pagenum: filter.pageIndex - 1,
                        pagesize: filter.pageSize,
                        sortdatafield: filter.sortField,
                        sortorder: filter.sortOrder,
                        params: self.getParams()
                    })
                }).done(function(res) {
                    d.resolve({
                        data: res,
                        itemsCount: self._recordsCount
                    });
                });

                return d.promise();
            }
        }
    });

    $('jsgrid-header-cell').click(function() {
        var field = this.innerText;
        self._grid.jsGrid("sort", field);
    });
};

JsGridDataExplorer.prototype.update = function() {
    this._grid.jsGrid();
};

JsGridDataExplorer.prototype.clear = function() {
    this._grid.jsGrid('destroy');
};

JsGridDataExplorer.prototype._updateRecordsCount = function (count) {
    this._recordsCount = count;
};