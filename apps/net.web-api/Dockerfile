FROM mcr.microsoft.com/dotnet/sdk:6.0.400 AS build-env
WORKDIR /app

# Copy everything else and build
COPY . ./
RUN dotnet publish -r linux-x64 --self-contained true -c Release -o adworks.media-web-api/out adworks.media-web-api/adworks.media-web-api.csproj

RUN mkdir p ~/adworks

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime:6.0.8
WORKDIR /app

COPY --from=build-env /app/adworks.media-web-api/out .
ENV ASPNETCORE_URLS http://+:5000;
EXPOSE 5000
ENTRYPOINT ["dotnet", "adworks.media-web-api.dll", "--ASPNETCORE_ENVIRONMENT=prod"]
