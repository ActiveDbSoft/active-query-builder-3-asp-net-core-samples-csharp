import React, { Component } from 'react';
import AQB from './aqb.client';

export default class App extends Component {
  componentDidMount() {
    this.name = 'React';

    window.AQB = AQB;

    AQB.Web.UI.QueryBuilder(this.name, this.querybuilder);
    AQB.Web.UI.ObjectTreeView(this.name, this.treeview);
    AQB.Web.UI.SubQueryNavigationBar(this.name, this.navbar);
    AQB.Web.UI.Canvas(this.name, this.canvas);
    AQB.Web.UI.StatusBar(this.name, this.statusbar);
    AQB.Web.UI.Grid(this.name, this.grid);
    AQB.Web.UI.SqlEditor(this.name, this.sql);

    AQB.Web.UI.startApplication('/QueryBuilder/CreateQueryBuilder', this.name);
  }

  render() {
    return (
      <div>
        <div id="qb" ref={el => this.querybuilder = el}></div>
        <div className="qb-ui-layout">
          <div className="qb-ui-layout__top">
            <div className="qb-ui-layout__left">
              <div className="qb-ui-structure-tabs">
                <div className="qb-ui-structure-tabs__tab">
                  <input type="radio" id="tree-tab" name="qb-tabs" defaultChecked />
                  <label htmlFor="tree-tab">Database</label>
                  <div className="qb-ui-structure-tabs__content">
                    <div ref={el => this.treeview = el}></div>
                  </div>
                </div>
              </div>
            </div>
            <div className="qb-ui-layout__right">
              <div ref={el => this.navbar = el}></div>
              <div ref={el => this.canvas = el}></div>
              <div ref={el => this.statusbar = el}></div>
              <div ref={el => this.grid = el}></div>
            </div>
          </div>
          <div className="qb-ui-layout__bottom">
            <div ref={el => this.sql = el}></div>
          </div>
        </div>
      </div>
    )
  }
}
