module.exports = {
    entry: './index.js',
    output: {
        filename: 'index.js'
    },
    devServer: {
        proxy: {
            '*': {
				"target": "https://[::1]:44368",
				"secure": false
            }			
        },
		'https': true
    }
};