const { createProxyMiddleware } = require('http-proxy-middleware');

module.exports = function(app) {
  app.use(
    '/createQueryBuilder',
    createProxyMiddleware({
      target: 'http://[::1]:5001'
    })
  );

  app.use(
    '/getData',
    createProxyMiddleware({
      target: 'http://[::1]:5001'
    })
  );

  app.use(
    '/getQueryParameters',
    createProxyMiddleware({
      target: 'http://[::1]:5001'
    })
  );

  app.use(
    '/getData',
    createProxyMiddleware({
      target: 'http://[::1]:17835'
    })
  );

  app.use(
    '/ActiveQueryBuilder/Handler/**',
    createProxyMiddleware({
      target: 'http://[::1]:17835'
    })
  ); 
};