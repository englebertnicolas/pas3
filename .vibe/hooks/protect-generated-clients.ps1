# Denies edits to Kiota-generated Client projects.
# Reads the pre_tool hook JSON from stdin; prints a structured deny response
# when tool_input.file_path points inside a src/backend/<Context>/Client/ tree.
$raw = [Console]::In.ReadToEnd()
if ([string]::IsNullOrWhiteSpace($raw)) { exit 0 }

try { $hook = $raw | ConvertFrom-Json } catch { exit 0 }
$path = $hook.tool_input.file_path
if ([string]::IsNullOrWhiteSpace($path)) { exit 0 }

$normalized = $path -replace '\\', '/'
if ($normalized -match '(?i)src/backend/[^/]+/Client/') {
    $response = @{
        decision = 'deny'
        reason   = 'This file belongs to a Kiota-generated Client project. Never edit it by hand - regenerate the client from the API OpenAPI document instead.'
    } | ConvertTo-Json -Compress
    [Console]::Out.Write($response)
}

exit 0
