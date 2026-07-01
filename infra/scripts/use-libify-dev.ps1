# Perfil AWS libify-dev — execute: . .\infra\scripts\use-libify-dev.ps1
$env:AWS_PROFILE = "libify-dev"
$env:AWS_REGION = "sa-east-1"
$env:AWS_DEFAULT_REGION = "sa-east-1"
Write-Host "AWS_PROFILE=$env:AWS_PROFILE | AWS_REGION=$env:AWS_REGION"
