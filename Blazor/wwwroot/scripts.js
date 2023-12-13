const name = 'Blazor';

window.initializeQueryBuilder = () => {
    AQB.Web.UI.QueryBuilder(name, $('#qb'));
    AQB.Web.UI.ObjectTreeView(name, $('#treeview'));
    AQB.Web.UI.SubQueryNavigationBar(name, $('#navbar'));
    AQB.Web.UI.Canvas(name, $('#canvas'));
    AQB.Web.UI.StatusBar(name, $('#statusbar'));
    AQB.Web.UI.Grid(name, $('#grid'), { orColumnCount: 0 });
    AQB.Web.UI.SqlEditor(name, $('#sql'));

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
