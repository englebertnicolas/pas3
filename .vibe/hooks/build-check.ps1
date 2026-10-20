# Post-agent build gate: runs after each completed turn.
# Prints a structured "deny" response (injected back as a retry user message)
# when the solution no longer compiles.
$ErrorActionPreference = 'Continue'

$output = & dotnet build PAS3.slnx --nologo -v q 2>&1
if ($LASTEXITCODE -eq 0) { exit 0 }

$tail = ($output | Select-Object -Last 25 | Out-String).Trim()
$reason = "The solution no longer compiles after this turn. Fix the build before proceeding. Last build output lines:`n$tail"

$response = @{
    decision = 'deny'
    reason   = $reason
} | ConvertTo-Json -Compress
[Console]::Out.Write($response)
exit 0
