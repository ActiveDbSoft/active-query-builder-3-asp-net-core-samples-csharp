function DataExplorer(container, dataUrl, recordsCountUrl, instanceId, beforeSend) {
    this.container = container;
    this.dataUrl = dataUrl;
    this.recordsCountUrl = recordsCountUrl;
    this.instanceId = instanceId;
    this.beforeSend = beforeSend;

    this._linkToDataExplorerSite = $('<a class="link-to-grid-site"></a>');
    this.container.append(this._linkToDataExplorerSite);
}

DataExplorer.prototype.getParams = function () {
    var result = [];
    var params = getUniqueQueryParams();

    for (var i = 0; i < params.length; i++) {
        result.push({
            name: params[i].FullName,
            value: $('input.' + params[i].Name).val()
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
        data: JSON.stringify({ parameters: this.getParams(), instanceId: this.instanceId }),
        beforeSend: this.beforeSend,
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