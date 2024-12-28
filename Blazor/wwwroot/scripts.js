
let sessionId = localStorage.getItem('qbSessionId');

if (!sessionId) {
    sessionId = Math.random().toString(36).substring(2, 9);

    localStorage.setItem('qbSessionId', sessionId);
}

class Helpers {
    static dotNetHelper;

    static setDotNetHelper(value) {
        Helpers.dotNetHelper = value;
    }

    static async sqlUpdated() {
        await Helpers.dotNetHelper.invokeMethodAsync('OnSqlUpdated');
    }
}

window.Helpers = Helpers;

window.initializeQueryBuilder = () => {
    AQB.Web.UI.QueryBuilder(sessionId, $('#qb'));
    AQB.Web.UI.ObjectTreeView(sessionId, $('#treeview'));
    AQB.Web.UI.SubQueryNavigationBar(sessionId, $('#navbar'));
    AQB.Web.UI.Canvas(sessionId, $('#canvas'));
    AQB.Web.UI.StatusBar(sessionId, $('#statusbar'));
    AQB.Web.UI.Grid(sessionId, $('#grid'), { orColumnCount: 0 });
    AQB.Web.UI.SqlEditor(sessionId, $('#sql'));

    AQB.Web.onQueryBuilderReady(function (qb) {
        qb.on(qb.Events.SqlChanged, function () {
            window.Helpers.sqlUpdated();
        });
    });

    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/query-builder/handler")
        .build();

    connection.start().then(() => {
        AQB.Web.UI.useSignalR(connection);
    });
}

window.disposeQueryBuilder = () => {
    AQB.Web.QueryBuilder.dispose();
}

window.getSessionId = () => {
    return sessionId;
}