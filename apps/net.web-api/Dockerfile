FROM mcr.microsoft.com/dotnet/sdk:6.0.400 AS build-env
WORKDIR /app

# Copy everything else and build
COPY . ./
RUN dotnet publish -r linux-x64 --self-contained true -c Release -o net.web-api/out net.web-api/Eworks.AdworksWebApi.csproj

RUN mkdir p ~/adworks

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime:6.0.8
WORKDIR /app

COPY --from=build-env /app/net.web-api/out .
ENV ASPNETCORE_URLS http://+:5000;
EXPOSE 5000
ENTRYPOINT ["dotnet", "adworks.media-web-api.dll", "--ASPNETCORE_ENVIRONMENT=prod"]
