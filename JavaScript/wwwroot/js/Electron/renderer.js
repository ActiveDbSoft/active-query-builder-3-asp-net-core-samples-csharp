const AQB = require('../../aqb.client.js');

// Instance identifier string to bind to the QueryBuilder component on the server side.
// See the ElectronQueryBuilderController.cs code for details.
var instanceId = 'Electron';

window.AQB = AQB;
var qb = document.getElementById('qb');
var treeview = document.getElementById('treeview');
var navbar = document.getElementById('navbar');
var canvas = document.getElementById('canvas');
var statusbar = document.getElementById('statusbar');
var grid = document.getElementById('grid');
var sql = document.getElementById('sql');

AQB.Web.host = 'https://localhost:44368';

AQB.Web.UI.QueryBuilder(instanceId, qb);
AQB.Web.UI.ObjectTreeView(instanceId, treeview);
AQB.Web.UI.SubQueryNavigationBar(instanceId, navbar);
AQB.Web.UI.Canvas(instanceId, canvas);
AQB.Web.UI.StatusBar(instanceId, statusbar);
AQB.Web.UI.Grid(instanceId, grid);
AQB.Web.UI.SqlEditor(instanceId, sql);

AQB.Web.UI.startApplication('https://localhost:44368/CreateQueryBuilder/CreateQueryBuilder', instanceId);