function ReactGridDataExplorer() {
    DataExplorer.apply(this, arguments);
    this._setLinkToDataExplorerSite('http://adazzle.github.io/react-data-grid');

    this.container.append('<div id="reactgrid"></div>');
}

ReactGridDataExplorer.prototype = Object.create(DataExplorer.prototype);
ReactGridDataExplorer.prototype.constructor = ReactGridDataExplorer;

ReactGridDataExplorer.prototype.create = function (columns) {
    var self = this;
    this.isInit = true;

    ReactDOM.unmountComponentAtNode(document.getElementById('reactgrid'));

    getData(init, 0);

    function getData(callback, pageNum, sortField, sortOrder) {
        $.ajax({
            url: self.dataUrl,
            type: 'POST',
            contentType: 'application/json;',
            dataType: 'json',
            data: JSON.stringify({
                pagenum: pageNum,
                pagesize: 10,
                sortdatafield: sortField,
                sortorder: sortOrder,
                params: self.getParams()
            }),
            success: callback
        });
    }

    function init(data) {
        var Grid = React.createClass({
            getInitialState() {
                this._columns = columns.map(function (c) {
                    c.sortable = true;
                    c.width = 300;
                    return c;
                });

                return { rows: data, page: 0 };
            },

            sort(field, order) {
                getData(function (data) {
                    this.setState({ rows: data });
                }.bind(this),
                    this.state.page,
                    field,
                    order !== 'NONE' ? order : undefined);

                this.setState({ field: field, order: order });
            },

            page(page) {
                getData(function (data) {
                    this.setState({ rows: data });
                }.bind(this),
                    page,
                    this.state.field,
                    this.state.order !== 'NONE' ? this.state.order : undefined);

                this.setState({ page: page });
            },

            updateRows() {
                getData(function (data) {
                    this.setState({ rows: data });
                }.bind(this),
                    this.state.page,
                    this.state.field,
                    this.state.order !== 'NONE' ? this.state.order : undefined);
            },

            prevPage() {
                this.page(this.state.page - 1);
            },

            nextPage() {
                this.page(this.state.page + 1);
            },

            rowGetter(i) {
                return this.state.rows[i];
            },

            render() {
                return React.createElement('div',
                    null,
                    [
                        React.createElement(ReactDataGrid,
                            {
                                onGridSort: this.sort,
                                columns: this._columns,
                                rowGetter: this.rowGetter,
                                rowsCount: this.state.rows.length,
                                minHeight: 500
                            }),
                        React.createElement('span', { onClick: this.prevPage }, ['prev ']),
                        React.createElement('span', { onClick: this.nextPage }, [' next'])
                    ]);
            }
        });

        var gridElem = React.createElement(Grid);
        window.reactGrid = ReactDOM.render(gridElem, document.getElementById('reactgrid'));
    }
}

ReactGridDataExplorer.prototype.update = function() {
    reactGrid.updateRows();
}

ReactGridDataExplorer.prototype.clear = function () {
    ReactDOM.unmountComponentAtNode(document.getElementById('reactgrid'));
}

ReactGridDataExplorer.prototype._updateRecordsCount = function(count) {
    this._recordsCount = count;
}