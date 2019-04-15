import AQB from '../../aqb.client';

// Instance identifier string to bind to the QueryBiulder component on the server side.
// See the WebpackQueryBuilderController.cs code for details.
var name = 'Webpack';

window.AQB = AQB;
var qb = document.getElementById('qb');
var treeview = document.getElementById('treeview');
var navbar = document.getElementById('navbar');
var canvas = document.getElementById('canvas');
var statusbar = document.getElementById('statusbar');
var grid = document.getElementById('grid');
var sql = document.getElementById('sql');

AQB.Web.UI.QueryBuilder(name, qb);
AQB.Web.UI.ObjectTreeView(name, treeview);
AQB.Web.UI.SubQueryNavigationBar(name, navbar);
AQB.Web.UI.Canvas(name, canvas);
AQB.Web.UI.StatusBar(name, statusbar);
AQB.Web.UI.Grid(name, grid);
AQB.Web.UI.SqlEditor(name, sql);

AQB.Web.UI.startApplication();