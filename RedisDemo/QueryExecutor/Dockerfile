FROM node:12

WORKDIR /usr/src/app
COPY package.json ./

RUN npm install

COPY index.js ./

CMD ["node", "index.js"]
