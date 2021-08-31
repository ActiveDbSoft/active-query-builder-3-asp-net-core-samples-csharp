import React from 'react';
import { DatePicker, DateTimePicker, TimePicker } from '@material-ui/pickers';
import { TextField, FormControlLabel, Checkbox } from '@material-ui/core';

const numberTypes = ['Byte', 'SByte', 'Int16', 'Int32', 'Int64', 'UInt16', 'UInt32', 'UInt64', 
    'Double', 'Currency'];

export const getParamEditor = (name, dataType, value, onChange) => {
    const wrap = (editor) => wrapEditor(editor, { name, value, onChange });

    if (dataType == 'DateTime')
        return wrap(<DateTimePicker autoOk />);
    else if (dataType == 'Time')
        return wrap(<TimePicker autoOk />);
    else if (dataType == 'Date')
        return wrap(<DatePicker autoOk />);
    else if (dataType == 'Boolean')
        return wrap(<BooleanEditor />);
    else if (numberTypes.includes(dataType))
        return wrap(<TextField type="number" />);
    else 
        return wrap(<TextField />);
}

const BooleanEditor = (label, value, onChange) => {
    return <FormControlLabel
        control={
          <Checkbox
            checked={value}
            onChange={onChange}
          />
        }
        label={label}
      />;
}

const wrapEditor = (editor, props) => {
    return <EditorWrapper {...{ ...props, fullWidth: true }}>
        {editor}
    </EditorWrapper>;
}

const EditorWrapper = ({ children, ...rest }) => {
    return React.cloneElement(children, { ...rest });
}
