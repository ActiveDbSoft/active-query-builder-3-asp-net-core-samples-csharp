function JsonDataExplorer() {
    DataExplorer.apply(this, arguments);
    this._setLinkToDataExplorerSite('https://github.com/josdejong/jsoneditor');

    this.container.append('<br />');
    this.container.append('<div id="jsoneditor"></div>');
    this.container.append('<button class="prev">Prev</button>');
    this.container.append('<span>Page:<span class="jsonPage"></span></span>');
    this.container.append('<button class="next">Next</button>');
    this.container.append('<button class="record-count" style="float: right;"></button>');

    $('.next').button().click(function () {
        this._page += 1;
        this._fillJsonEditor();
    }.bind(this));

    $('.prev').button().click(function () {
        this._page -= 1;
        this._fillJsonEditor();
    }.bind(this));
}

JsonDataExplorer.prototype = Object.create(DataExplorer.prototype);
JsonDataExplorer.prototype.constructor = JsonDataExplorer;

JsonDataExplorer.prototype.create = function() {
    this.isInit = true;
    this._page = 0;
    this._fillJsonEditor();
};

JsonDataExplorer.prototype.update = function() {
    this._page = 0;
    this._fillJsonEditor();
};

JsonDataExplorer.prototype.clear = function() {
    this._editor.destroy();
    this._editor = undefined;
};

JsonDataExplorer.prototype._updateRecordsCount = function (count) {
    this._recordsCount = count;
    $('.record-count').button().text('Records count: ' + count);
};

JsonDataExplorer.prototype._fillJsonEditor = function() {
    if (this._page < 0)
        return this._page = 0;

    if (this._page * 10 > this._recordsCount)
        return this._page -= 1;

    var self = this;

    $('.jsonPage').text(this._page + 1);

    if (!this._editor) {
        var container = document.getElementById('jsoneditor');
        this._editor = new JSONEditor(container, { mode: 'code' });
    }

    $.ajax({
        type: 'POST',
        contentType: 'application/json;',
        dataType: 'json',
        data: JSON.stringify({
            pagenum: this._page,
            pagesize: 10,
            params: this.getParams()
        }),
        url: this.dataUrl,
        success: function(data) {
            self._editor.set(data);
        }.bind(this)
    });
};
