import React from 'react';
import AQB from './aqb.client';

export default class CriteriaBuilder extends React.Component {
    componentDidMount() {
        this.name = this.props.name;
        
        window.AQB = AQB;

        AQB.Web.UI.CriteriaBuilder(this.name, this.criteriabuilder);
        AQB.Web.onCriteriaBuilderReady(this.subscribeToCriteriaBuilderChanges);
    };

    subscribeToCriteriaBuilderChanges = (cb) => {
        cb.loadColumns();
        cb.on(cb.Events.CriteriaBuilderChanged, () => this.onCriteriaBuilderChanged(cb));
    }

    onCriteriaBuilderChanged = (cb) => {
        cb.transformSql((sql) => {
            if (this.props.onSqlChanged)
                this.props.onSqlChanged(sql);
        });
    }

    componentWillUnmount() {
        AQB.Web.CriteriaBuilder.dispose();
    };
    
      render() {
        return (
            <div>
                <div>
                    <div id="qb" ref={el => this.querybuilder = el}></div>
                    <div id="cb" ref={el => this.criteriabuilder = el}></div>
                    <div style={{visibility: "hidden", position: "absolute"}} className="qb-ui-layout__bottom">
                        <div ref={el => this.sql = el}></div>
                    </div>
                </div>
            </div>
        );
    }
}
