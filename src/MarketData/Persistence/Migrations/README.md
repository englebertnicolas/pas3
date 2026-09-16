# Assembly PAS.MarketData.Persistence

## DbContext migration

To generate migration for MarketDataDbContext:
- Open a PowerShell prompt in the `src/MarketData/Persistence` folder.
- Execute the following command (with the expected version):
```
dotnet ef migrations add V_1_0_0 --context MarketDataDbContext
```
