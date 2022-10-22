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

RUN dotnet publish -r linux-x64 --self-contained -c Release -o apps/net.media-processor/out apps/net.media-processor/Eworks.AdworksMediaProcessor.csproj

RUN mkdir p ~/adworks

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime:6.0.10
WORKDIR /app

COPY --from=build-env /app/apps/net.media-processor/out .

ENTRYPOINT ["dotnet", "Eworks.AdworksMediaProcessor.dll", "--environment=prod"]
