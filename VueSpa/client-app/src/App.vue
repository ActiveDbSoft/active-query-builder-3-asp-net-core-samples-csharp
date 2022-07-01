<template>
  <div>
    <div ref="qb"></div>
    <div class="qb-ui-layout">
      <div class="qb-ui-layout__top">
        <div class="qb-ui-layout__left">
          <div class="qb-ui-structure-tabs">
            <div class="qb-ui-structure-tabs__tab">
              <input type="radio" id="tree-tab" name="qb-tabs" checked />
              <label for="tree-tab">Database</label>
              <div class="qb-ui-structure-tabs__content">
                <div ref="treeview"></div>
              </div>
            </div>
          </div>
        </div>
        <div class="qb-ui-layout__right">
          <div ref="navbar"></div>
          <div ref="canvas"></div>
          <div ref="statusbar"></div>
          <div ref="grid"></div>
        </div>
      </div>
      <div class="qb-ui-layout__bottom">
        <div ref="sql"></div>
      </div>
    </div>
  </div>
</template>

<script>
import AQB from './aqb.client'

export default {
  name: 'App',
  mounted: function () {
    const name = 'Vue';

    window.AQB = AQB;
    AQB.Web.UI.QueryBuilder(name, this.$refs.qb, { useDefaultTheme: false });
    AQB.Web.UI.ObjectTreeView(name, this.$refs.treeview);
    AQB.Web.UI.SubQueryNavigationBar(name, this.$refs.navbar);
    AQB.Web.UI.Canvas(name, this.$refs.canvas);
    AQB.Web.UI.StatusBar(name, this.$refs.statusbar);
    AQB.Web.UI.Grid(name, this.$refs.grid, { orColumnCount: 0 });
    AQB.Web.UI.SqlEditor(name, this.$refs.sql);

    AQB.Web.UI.autoInit();
  }
}
</script>
