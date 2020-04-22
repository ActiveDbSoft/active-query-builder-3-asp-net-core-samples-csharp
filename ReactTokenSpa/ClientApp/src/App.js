import React, { Component } from 'react';
import AQB from './aqb.client';

export default class App extends Component {
  componentDidMount() {
    this.name = 'React';

    window.AQB = AQB;

    AQB.Web.beforeSend = this.beforeSend;
    AQB.Web.UI.QueryBuilder(this.name, this.querybuilder);
    AQB.Web.UI.ObjectTreeView(this.name, this.treeview);
    AQB.Web.UI.SubQueryNavigationBar(this.name, this.navbar);
    AQB.Web.UI.Canvas(this.name, this.canvas);
    AQB.Web.UI.StatusBar(this.name, this.statusbar);
    AQB.Web.UI.Grid(this.name, this.grid);
    AQB.Web.UI.SqlEditor(this.name, this.sql);

    AQB.Web.UI.startApplication(this.name, this.createQueryBuilder);
  }

  beforeSend = (xhr) => {
      // Add token the request header to identify the client and find the right QueryBuilder instance on the server.
      xhr.setRequestHeader('query-builder-token', this.getToken());
  }

  createQueryBuilder = (onSuccess, onError) => {
    const me = this;
    this.checkToken(function () {
      me.createQbOnServer(onSuccess, onError);
    });
  }

  checkToken = (callback) => {
      // Send a request to check for the token validity.
      fetch('/QueryBuilder/CheckToken?token=' + this.getToken()).then(r => r.text()).then(token => {
        // Save new token to the local storage to use in further requests.
        if (token)
          this.saveToken(token);
        callback();
      });
  }

  createQbOnServer = (onSuccess, onError) => {    
    fetch('/QueryBuilder/CreateQueryBuilder?name=' + this.name, 
    {
      headers: {'query-builder-token': this.getToken() }
    })
    .then(r => onSuccess())
    .catch(e => onError());
  }
  
  getToken = () => {
      return localStorage.getItem('queryBuilderToken');
  }

  saveToken = (token) => {
      localStorage.setItem('queryBuilderToken', token);
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
