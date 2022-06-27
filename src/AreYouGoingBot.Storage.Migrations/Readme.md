# EF Hints

## Install dotnet-ef

`dotnet tool install --global dotnet-ef`

## Add migration

```
cd src/AreYouGoingBot.Storage.Migrations
dotnet ef migrations add migration_name --project AreYouGoingBot.Storage.Migrations.csproj`
```

## Apply migration to your db

`dotnet ef database update`