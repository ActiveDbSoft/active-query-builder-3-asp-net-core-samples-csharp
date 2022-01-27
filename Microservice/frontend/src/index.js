import React from 'react';
import ReactDOM from 'react-dom';
import App from './App';
import { MuiPickersUtilsProvider } from '@material-ui/pickers';
import DateFnsUtils from '@date-io/date-fns';

ReactDOM.render(
    <React.StrictMode>
        <MuiPickersUtilsProvider utils={DateFnsUtils}>
            <App />
        </MuiPickersUtilsProvider>
    </React.StrictMode>,
    document.getElementById('root')
);
