function DataExplorer(container, dataUrl, recordsCountUrl) {
    this.container = container;
    this.dataUrl = dataUrl;
    this.recordsCountUrl = recordsCountUrl;

    this._linkToDataExplorerSite = $('<a class="link-to-grid-site"></a>');
    this.container.append(this._linkToDataExplorerSite);
}

DataExplorer.prototype.getParams = function () {
    var result = [];
    var params = getUniqueQueryParams();

    for (var i = 0; i < params.length; i++) {
        result.push({
            Name: params[i].FullName,
            Value: $('input.' + params[i].Name).val()
        });
    }

    return result;
}

DataExplorer.prototype.updateRecordsCount = function (callback) {
    var self = this;

    $.ajax({
        type: 'POST',
        url: this.recordsCountUrl,
        dataType: 'json',
        contentType: 'application/json',
        data: JSON.stringify(this.getParams()),
        success: function (count) {
            self._updateRecordsCount(count);
            callback();
        },
        error: function (xhr, error, text) {
            showError(text);
        }
    });
}

DataExplorer.prototype._setLinkToDataExplorerSite = function (url) {
    this._linkToDataExplorerSite.attr(url);
    this._linkToDataExplorerSite.text(url);
}