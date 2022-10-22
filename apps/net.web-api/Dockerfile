FROM mcr.microsoft.com/dotnet/sdk:6.0.402 AS build-env
WORKDIR /app

RUN apt-get update && apt-get install -y \
    software-properties-common \
    npm
RUN npm install npm@latest -g && \
    npm install n -g && \
    n latest

# Copy everything else and build
COPY . ./
RUN npm install

RUN dotnet publish -r linux-x64 --self-contained true -c Release -o apps/net.web-api/out apps/net.web-api/Eworks.AdworksWebApi.csproj

RUN mkdir p ~/adworks

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime:6.0.10
WORKDIR /app

COPY --from=build-env /app/apps/net.web-api/out .
ENV ASPNETCORE_URLS http://+:5000;
EXPOSE 5000
ENTRYPOINT ["dotnet", "Eworks.AdworksWebApi.dll", "--ASPNETCORE_ENVIRONMENT=prod"]
