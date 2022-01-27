import React from 'react';
import { Container, AppBar, Tab, Tabs, Box, Dialog, DialogTitle, DialogContent, Typography, 
    DialogActions, Grid, Button } from '@material-ui/core';    
import { DataGrid as MuiDataGrid } from '@material-ui/data-grid';
import QueryBuilder from './QueryBuilder';
import CriteriaBuilder from './CriteriaBuilder';
import Api from './Api';
import { getParamEditor } from './ParamEditors';

const queryBuilderInstanceid = 'MicroserviceDemo';
const api = new Api(queryBuilderInstanceid);

function TabPanel(props) {
    const { children, value, index, ...other } = props;

    return (
        <div
            role="tabpanel"
            hidden={value !== index}
            id={`simple-tabpanel-${index}`}
            {...other}
        >
            <Box p={3}>
                {children}
            </Box>
        </div>
    );
}

const App = () => {
    const [activeTab, setActiveTab] = React.useState(0);
    const [query, setQuery] = React.useState(null);
    const [gridQuery, setGridQuery] = React.useState(null);
    const [params, setParams] = React.useState([]);

    const resetParamsWrapper = {};

    const onSqlChanged = (sql) => {
        setQuery(sql);
    };

    const isParamsEqual = (first, second) => {
        if (first.length != second.length)
            return false;

        return first.every(fi => second.some(si => si.Name === fi.name && si.DataType === fi.dataType));
    }

    const onParamsChanged = (newParams) => {
        if (!isParamsEqual(params, newParams))
            setParams(newParams.map(p => ({ name: p.Name, dataType: p.DataType })));
    };

    const onCriteriaBuilderSqlChanged = (sql) => {
        setGridQuery(sql);
    };

    const handleTabChange = (event, newTab) => {
        setActiveTab(newTab);
        
        if (newTab == 1) {
            const wasSame = query == gridQuery;
            setGridQuery(query);

            if (wasSame && typeof resetParamsWrapper.resetParams === 'function')
                resetParamsWrapper.resetParams();
        }            
    };

    return (
        <div>
            <Container maxWidth="lg">
                <AppBar position="static">
                    <Tabs value={activeTab} onChange={handleTabChange}>
                        <Tab label="Query Builder" />
                        <Tab label="Query Results" />
                    </Tabs>
                </AppBar>
                <TabPanel value={activeTab} index={0}>
                    <QueryBuilderTab onParamsChanged={onParamsChanged} onSqlChanged={onSqlChanged} />
                </TabPanel>
                <TabPanel value={activeTab} index={1}>
                    <QueryResultsTab active={activeTab == 1}
                        resetParamsWrapper={resetParamsWrapper} 
                        onSqlChanged={onCriteriaBuilderSqlChanged} 
                        query={gridQuery} params={params} />
                </TabPanel>
            </Container>
        </div>
    );
}

const QueryBuilderTab = ({ onSqlChanged, onParamsChanged }) => {
    return <QueryBuilder onSqlChanged={onSqlChanged} onParamsChanged={onParamsChanged} name={queryBuilderInstanceid} />;
}

const QueryResultsTab = ({ active, query, onSqlChanged, resetParamsWrapper, params }) => {
    const [paramsString, setParamsString] = React.useState(null);

    const showParams = (params) => {
        if (params == null || params.length == 0) {
            setParamsString(null);
            return;
        }

        setParamsString("Applied parameter values: " + params.map(p => `${p.name} = "${p.value}"`).join(", "));
    };
    
    return <>
        { paramsString != null && <Typography style={{paddingBottom: 10}} variant="body1">{paramsString}</Typography> }
        <CriteriaBuilder onSqlChanged={onSqlChanged} name={queryBuilderInstanceid} />
        <DataGrid active={active} resetParamsWrapper={resetParamsWrapper} 
            query={query} params={params} showParams={showParams} />
    </>;
}

const DataGrid = ({ active, query, resetParamsWrapper, params, showParams }) => {
    const [pageSize, setPageSize] = React.useState(5);
    const [page, setPage] = React.useState(1);
    const [rowCount, setRowCount] = React.useState(10);
    const [sorting, setSorting] = React.useState([]);
    const [rows, setRows] = React.useState([]);
    const [columns, setColumns] = React.useState([]);
    const [loading, setLoading] = React.useState(false);
    const [paramsDialogOpen, setParamsDialogOpen] = React.useState(false);
    const [filledParams, setFilledParams] = React.useState([]);
    const [isParamsFilled, setIsParamsFilled] = React.useState(true);

    const resetParams = () => {
        setFilledParams([]);
        setIsParamsFilled(params == null || params.length == 0);
    };

    resetParamsWrapper.resetParams = resetParams;

    const updatePage = async (page) => {
        setPage(page);
    };

    const updatePageSize = async (size) => {
        setPageSize(size);
    };

    const updateSorting = async (sortModel) => {
        setSorting(sortModel);
    };

    const onParamsFilled = (params) => {
        setParamsDialogOpen(false);
        setFilledParams(params);
        setIsParamsFilled(true);        
    }

    React.useEffect(() => {
        if (showParams)
            showParams(filledParams);

    }, [filledParams]);

    React.useEffect(() => {
        resetParams();        
    }, [active, query, params]);

    React.useEffect(() => {
        setPage(1);
    }, [query]);

    React.useEffect(() => {
        if (!active || isParamsFilled)
            return;

        setParamsDialogOpen(true);
    }, [active, isParamsFilled]);

    React.useEffect(() => {
        const fetchData = async () => {
            if (!active || query == null || !isParamsFilled)
                return;

            const start = (page - 1) * pageSize;
            const end = start + pageSize;

            setLoading(true);
            try {
                const { data, totalCount } = await api.getData(start, end, sorting, filledParams);

                setRowCount(totalCount);
                setColumns(getColumns(data));
                setRows(addUniqueIds(data));
            }
            finally {
                setLoading(false);
            }
        }

        fetchData();
    }, [query, page, pageSize, sorting, isParamsFilled]);

    return <div style={{ height: 600, width: '100%' }}>
        <ParametersDialog onParamsFilled={onParamsFilled} open={paramsDialogOpen} params={params} handleClose={() => setParamsDialogOpen(false)} />
        <MuiDataGrid rows={rows} columns={columns} pageSize={pageSize} page={page} rowCount={rowCount}
            rowsPerPageOptions={[5, 10, 50, 100]} onSortModelChange={async (e) => updateSorting(e.sortModel)}
            paginationMode="server" sortingMode="server" onPageChange={async (e) => updatePage(e.page)}
            onPageSizeChange={async (e) => updatePageSize(e.pageSize)} loading={loading} />
    </div>;
}

const ParametersDialog = ({ params, open, handleClose, onParamsFilled }) => {
    const [filledParams, setFilledParams] = React.useState([]);

    const findFilled = (name) => filledParams.find(p => p.name == name);
    const onParamValueChanged = (param, e) => {
        const value = typeof e.toISOString === 'function' ? e.toISOString() : e.target.value;
        const existing = findFilled(param.name);

        if (existing == null)
            setFilledParams([{ name: param.name, value }, ...filledParams]);
        else {
            existing.value = value;
            setFilledParams([...filledParams]);
        }
    }

    return (
        <Dialog maxWidth="sm" fullWidth={true} open={open} onClose={handleClose}>
            <DialogTitle>Query Parameters</DialogTitle>
            <DialogContent>
                <Grid container spacing={2}>
                    {params.map((p) => getParamsFormItem(p.name, p.dataType, findFilled(p.name)?.value ?? null, 
                        (e) => onParamValueChanged(p, e)))}
                </Grid>                
            </DialogContent>
            <DialogActions>
                <Button onClick={handleClose} color="secondary">
                    Cancel
                </Button>
                <Button onClick={() => onParamsFilled(filledParams)} color="primary">
                    OK
                </Button>
            </DialogActions>
        </Dialog>);
}

const getParamsFormItem = (name, dataType, value, onChanged) => {
    return <>
        <Grid item xs={4}>
            <Typography variant="body1" display="block" gutterBottom>
                {name}
            </Typography>
        </Grid>
        <Grid item xs={8}>
            {getParamEditor(name, dataType, value, onChanged)}
        </Grid>
    </>;
}

const getColumns = (data) => {
    if (data == null || data.length == 0)
        return [];

    const columns = [];
    const row = data[0];

    for (let field in row)
        columns.push({ field, headerName: field, width: 180 });

    return columns;
}

const addUniqueIds = (data) => {
    for (let i = 0; i < data.length; i++)
        if (data[i].id == null)
            data[i].id = i;

    return data;
}

export default App;
