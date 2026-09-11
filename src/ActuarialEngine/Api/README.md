# PAS.ActuarialEngine.Api

## ActuDbContext migrations

To generate migration for ActuDbContext:
- Open a PowerShell prompt in the `src/ActuarialEngine/Api` folder.
- Execute the following command (with the expected version):
```
dotnet ef migrations add V_1_0_0 --context ActuDbContext --output-dir Persistence/Migrations
```
