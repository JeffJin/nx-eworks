FROM mcr.microsoft.com/dotnet/sdk:6.0.400 AS build-env
WORKDIR /app

# Copy everything else and build
COPY . ./
RUN dotnet publish -r linux-x64 --self-contained -c Release -o adworks.media-processor/out adworks.media-processor/adworks.media-processor.csproj 

RUN mkdir p ~/adworks

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime:6.0.8
WORKDIR /app

COPY --from=build-env /app/adworks.media-processor/out .
 
ENTRYPOINT ["dotnet", "adworks.media-processor.dll", "--environment=prod"]
