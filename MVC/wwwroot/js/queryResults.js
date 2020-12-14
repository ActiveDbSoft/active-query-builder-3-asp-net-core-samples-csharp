function QueryResultsDemo(dataExplorer) {
    var paramsController = new ParamsController($('.table-params'));

    $(function () {
        $('#main-tabs').tabs();
        $('[href="#qr"]').click(onOpenQueryResults);
        AQB.Web.onCriteriaBuilderReady(subscribeToChanges);
        AQB.Web.onQueryBuilderReady(createCodeMirrorEditors);
    });

    function onOpenQueryResults() {
        var cb = window.criteriaBuilder;

        onCriteriaBuilderChanged(cb,
            function () {
                var params = getUniqueQueryParams();
                paramsController.clear();
                $('.execute').hide();

                if (params.length) {
                    if (dataExplorer.isInit) {
                        dataExplorer.clear();
                    }
                    paramsController.create(params);
                    $('.execute').show();
                } else {
                    dataExplorer.updateRecordsCount(createDataExplorer);
                }
            });
    };

    function subscribeToChanges(cb) {
        window.criteriaBuilder = cb;
        cb.loadColumns();

        cb.on(cb.Events.CriteriaBuilderChanged,
            function () {
                onCriteriaBuilderChanged(cb, function() {
                    dataExplorer.updateRecordsCount(updateDataExplorer);
                });
            });
    }

    function createDataExplorer() {
        $('.alert-danger').hide();
        var cb = window.criteriaBuilder;

        var columns = cb.Columns.map(function (c) {
            return {
                key: c.ResultName,
                name: c.ResultName,
                text: c.ResultName,
                datafield: c.ResultName
            };
        });

        testQuery(function() {
            dataExplorer.create(columns);
        });
    }

    function updateDataExplorer() {
        $('.alert-danger').hide();
        testQuery(function() {
            dataExplorer.update();
        });
    }

    function testQuery(callback) {
        $.ajax({
            type: 'POST',
            url: dataExplorer.dataUrl,
            dataType: "json",
            contentType: 'application/json',
            data: JSON.stringify({
                pagenum: 0,
                pagesize: 1,
                params: dataExplorer.getParams()
            }),
            success: function (data) {
                if (data.error)
                    showError(data.error);
                else {
                    hideError();
                    callback();
                }
            },
            error: function (xhr, error, text) {
                showError(text);
            }
        });
    }

    $('.execute').click(function () {
        if (dataExplorer.isInit)
            dataExplorer.updateRecordsCount(updateDataExplorer);
        else
            dataExplorer.updateRecordsCount(createDataExplorer);
    });

    function createCodeMirrorEditors(qb) {
        createCodeMirrorEditor(qb);
        createExpressionEditor(qb);
    }

    function createCodeMirrorEditor(qb) {
        var codeMirror = CodeMirror(document.body,
            {
                mode: 'text/x-sql',
                indentWithTabs: true,
                smartIndent: true,
                lineNumbers: true,
                matchBrackets: true
            });

        qb.EditorComponent.setEditor({
            element: codeMirror.display.wrapper,
            getSql: function () {
                return codeMirror.getValue();
            },
            setSql: function (sql) {
                codeMirror.setValue(sql);
            },
            setCursorPosition: function (pos, col, line) {
                this.focus();
                codeMirror.setCursor(line - 1, col - 1, { scroll: true });
            },
            focus: function () {
                codeMirror.focus();
            },
            onChange: function (callback) {
                this.changeCallback = callback;
                codeMirror.on('change', this.changeCallback);
            },
            remove: function () {
                codeMirror.off('change', this.changeCallback);
                this.element.remove();
            }
        });
    }

    function createExpressionEditor(qb) {
        window.codeMirrorForExpression = CodeMirror(document.body, {
            mode: 'text/x-sql',
            indentWithTabs: true,
            smartIndent: true,
            lineNumbers: true,
            matchBrackets: true,
            width: '500px',
            height: '250px'
        });

        $(codeMirrorForExpression.display.wrapper).hide();

        qb.GridComponent.on(AQB.Web.QueryBuilder.GridComponent.Events.GridBeforeCustomEditCell, beforeCustomEditCell);
    }

    function beforeCustomEditCell(data) {
        var cell = data.cell;

        var error = $('<p class="ui-state-error" style="display: none;"></div>');

        var $dialog = $('<div>').dialog({
            modal: true,
            width: 'auto',
            title: 'Custom expression editor',
            buttons: [
            {
                text: "OK",
                click: function () {
                    var newValue = codeMirrorForExpression.getValue();

                    var ifValid = function() {
                        cell.updateValue(newValue);
                        $dialog.dialog("close");
                    };

                    var ifNotValid = function(message) {
                        error.html(message).show();
                    };

                    validate(newValue, ifValid, ifNotValid);
                }
            },
            {
                text: "Cancel",
                click: function () {
                    $dialog.dialog("close");
                }
            }]
        });

        $dialog.append(error, $(codeMirrorForExpression.display.wrapper).show());
        $dialog.parent().css({
            top: '25%',
            left: '30%',
            width: 600
        });

        codeMirrorForExpression.setValue(cell._value || '');
        codeMirrorForExpression.refresh();
    };

    function validate(expression, ifValid, ifNotValid) {
        AQB.Web.QueryBuilder.validateExpression(expression, function (isValid, message) {
            if (isValid)
                ifValid();
            else
                ifNotValid(message);
        });
    }
}

function onCriteriaBuilderChanged(cb, callback) {
    cb.transformSql(function (sql) {
        $('.sql').text(sql);
        callback();
    });
}

function getUniqueQueryParams() {
    var params = AQB.Web.QueryBuilder.queryParams;
    var result = [];

    for (var i = 0, l = params.length; i < l; i++) {
        var param = params[i];

        if (result.find(r => r.FullName === param.FullName) == null)
            result.push(param);
    }

    return result;
}

function showError(statusText) {
    $('.alert-danger').show().text(statusText);
    $("#second-tabs").hide();
}

function hideError() {
    $('.alert-danger').hide();
    $("#second-tabs").show();
}