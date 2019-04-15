const electron = require('electron');

const app = electron.app;
const protocol = electron.protocol;
const BrowserWindow = electron.BrowserWindow

const path = require('path');
const url = require('url');

let mainWindow;

function createWindow () {
    mainWindow = new BrowserWindow();
    mainWindow.maximize();

    mainWindow.loadURL('file:///index.html');

    mainWindow.on('closed', function () {
        mainWindow = null
    })
}

app.on('ready', function () {
    protocol.unregisterProtocol('file');
    protocol.registerFileProtocol('file', (request, callback) => {
        const url = request.url.substr(8);
        const part = 'ActiveQueryBuilder/Handler/resources';
        const index = url.indexOf(part);

        if(index === -1)
          return callback(url);

        const newUrl = url.substr(index + part.length);
        callback(`${__dirname}${newUrl}.svg`);
    });

    createWindow();
});

app.on('window-all-closed', function () {
  if (process.platform !== 'darwin') {
    app.quit()
  }
});

app.on('activate', function () {
  if (mainWindow === null) {
    createWindow()
  }
});