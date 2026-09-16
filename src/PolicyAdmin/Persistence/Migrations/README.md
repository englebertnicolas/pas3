# Assembly PAS.PolicyAdmin.Persistence

## DbContext migration

To generate migration for PolicyAdminDbContext:
- Open a PowerShell prompt in the `src/PolicyAdmin/Persistence` folder.
- Execute the following command (with the expected version):
```
dotnet ef migrations add V_1_0_0 --context PolicyAdminDbContext
```
