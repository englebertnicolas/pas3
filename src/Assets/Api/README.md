# PAS.Assets.Api

## AssetDbContext migrations

To generate migration for AssetDbContext:
- Open a PowerShell prompt in the `src/Assets/Api` folder.
- Execute the following command (with the expected version):
```
dotnet ef migrations add V_1_0_0 --output-dir Persistence/Migrations
```
