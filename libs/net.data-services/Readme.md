cd libs/net.data-services

dotnet-ef --startup-project ../../apps/net.database-setup migrations add initial-creation

dotnet-ef --startup-project ../../apps/net.database-setup database update
