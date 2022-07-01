import React, { Component } from 'react';
import AQB from '../../../aqb.client';

export default class QueryBuilderLayout extends Component {
    componentDidMount() {
        // Instance identifier string to bind to the QueryBuilder component on the server side.
        // See the ReactQueryBuilderController.cs code for details.
        var name = 'React';

        window.AQB = AQB;
        AQB.Web.UI.QueryBuilder(name, this.querybuilder);
        AQB.Web.UI.ObjectTreeView(name, this.treeview);
        AQB.Web.UI.SubQueryNavigationBar(name, this.navbar);
        AQB.Web.UI.Canvas(name, this.canvas);
        AQB.Web.UI.StatusBar(name, this.statusbar);
        AQB.Web.UI.Grid(name, this.grid);
        AQB.Web.UI.SqlEditor(name, this.sql);

        AQB.Web.UI.autoInit();
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