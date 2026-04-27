# PowerShell script to create transfer record
$headers = @{
    'Content-Type' = 'application/json'
}

$body = @{
    AssetId = 16
    AssetTag = "AST-0016"
    FromDivisionId = 1
    FromDivision = "Information Technology"
    CurrentHolderId = 65
    CurrentHolder = "IT Employee"
    AssetRequestId = 4
    Reason = "my chair is under maintenance"
    ToDivisionId = 1
    ToDivision = "Information Technology"
    TargetUserId = 65
    TargetUser = "emp_it"
    TransferDate = "2026-04-25T06:17:08.572Z"
    Status = "PendingOwnerApproval"
    CreatedBy = 65
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri 'http://localhost:5000/api/transfers' -Method POST -Headers $headers -Body $body
    Write-Host "Transfer created successfully:"
    Write-Host $response
} catch {
    Write-Host "Error creating transfer:"
    Write-Host $_.Exception.Message
    Write-Host $_.Exception.Response
}
