# PAS.Policies.Api

## PolicyDbContext migrations

To generate migration for PolicyDbContext for version X.0.0:
- Open a PowerShell prompt in the `src/Policies/Api` folder.
- Execute the following command (with the expected version):
```
dotnet ef migrations add V_1_0_0 --output-dir Persistence/Migrations
```
