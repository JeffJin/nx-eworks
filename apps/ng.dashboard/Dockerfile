FROM mcr.microsoft.com/dotnet/sdk:6.0.402 AS build-env
WORKDIR /app

RUN apt-get update && apt-get install -y \
    software-properties-common \
    npm
RUN npm install npm@latest -g && \
    npm install n -g && \
    n latest

RUN npm install --global yarn
# add `/app/node_modules/.bin` to $PATH
ENV PATH /app/node_modules/.bin:$PATH

COPY ./ /app/
RUN yarn install
RUN nx build ng.dashboard

FROM nginx:latest
COPY apps/ng.dashboard/nginx.conf /etc/nginx/conf.d/default.conf
RUN ls -l
RUN ls -l /usr/share/nginx/html
COPY --from=build-env  /app/dist/apps/ng.dashboard /usr/share/nginx/html

EXPOSE 80
