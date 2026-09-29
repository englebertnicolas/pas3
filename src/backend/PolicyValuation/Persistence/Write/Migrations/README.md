# Assembly PAS.PolicyValuation.Persistence

## DbContext migration

To generate migration for ValuationDbContext:
- Open a PowerShell prompt in the `src/PolicyValuation/Persistence` folder.
- Execute the following command (with the expected version):
```
dotnet ef migrations add V_1_0_0 --context ValuationDbContext --output-dir Write/Migrations
```
