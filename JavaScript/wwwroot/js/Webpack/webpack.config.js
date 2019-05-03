module.exports = {
    entry: './index.js',
    output: {
        filename: 'index.js'
    },
    devServer: {
        proxy: {
            '*': {
                target: 'http://[::1]:1067'
            }
        }
    }
};