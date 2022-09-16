FROM node:lts AS compile-image

WORKDIR /app

# add `/app/node_modules/.bin` to $PATH
ENV PATH /app/node_modules/.bin:$PATH

COPY ./ /app/
RUN yarn install
RUN yarn build

FROM nginx:latest
COPY nginx.conf /etc/nginx/conf.d/default.conf

COPY --from=compile-image /app/dist/adworks-ng /usr/share/nginx/html

EXPOSE 80
