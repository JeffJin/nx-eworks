cd adworks.data-services

dotnet-ef --startup-project ../adworks.database-setup migrations add InitialCreation

dotnet-ef --startup-project ../adworks.database-setup  database update