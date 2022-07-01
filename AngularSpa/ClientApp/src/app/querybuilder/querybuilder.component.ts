import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-querybuilder',
  templateUrl: './querybuilder.component.html'
})
export class QuerybuilderComponent implements OnInit {

  constructor() { }

  ngOnInit() {
    const name = 'Angular';

    const qb = document.getElementById('qb');
    const treeview = document.getElementById('treeview');
    const navbar = document.getElementById('navbar');
    const canvas = document.getElementById('canvas');
    const statusbar = document.getElementById('statusbar');
    const grid = document.getElementById('grid');
    const sql = document.getElementById('sql');

    AQB.Web.UI.QueryBuilder(name, qb, { useDefaultTheme: false });
    AQB.Web.UI.ObjectTreeView(name, treeview);
    AQB.Web.UI.SubQueryNavigationBar(name, navbar);
    AQB.Web.UI.Canvas(name, canvas);
    AQB.Web.UI.StatusBar(name, statusbar);
    AQB.Web.UI.Grid(name, grid, { orColumnCount: 0 });
    AQB.Web.UI.SqlEditor(name, sql);

    AQB.Web.UI.autoInit();
  }

}
