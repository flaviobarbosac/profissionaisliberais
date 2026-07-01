#Requires -Version 5.1
<#
.SYNOPSIS
  Cria/atualiza registros DNS dev no Cloudflare (api.dev, aspire.dev).
.DESCRIPTION
  Requer API Token com permissao Zone:DNS:Edit.
  https://dash.cloudflare.com/profile/api-tokens

  $env:CLOUDFLARE_API_TOKEN = "..."
  .\infra\scripts\cloudflare-dns-dev.ps1

  -Proxied $true  = nuvem laranja (HTTPS via Cloudflare -> EC2 HTTP:80, SSL Flexible)
  -Proxied $false = DNS only (Caddy emite Let's Encrypt direto)
#>
param(
    [string]$ZoneName = "libify.com.br",
    [string]$Ec2Ip = "54.232.84.74",
    [switch]$Proxied = $true,
    [string]$ApiToken = $env:CLOUDFLARE_API_TOKEN
)

$ErrorActionPreference = "Stop"

if (-not $ApiToken) {
    Write-Host "Defina CLOUDFLARE_API_TOKEN." -ForegroundColor Red
    exit 1
}

$headers = @{
    Authorization = "Bearer $ApiToken"
    "Content-Type" = "application/json"
}

function Invoke-CfApi {
    param([string]$Method, [string]$Uri, [object]$Body = $null)
    $params = @{ Method = $Method; Uri = $Uri; Headers = $headers }
    if ($Body) { $params.Body = ($Body | ConvertTo-Json -Compress) }
    return Invoke-RestMethod @params
}
$zones = Invoke-CfApi GET "https://api.cloudflare.com/client/v4/zones?name=$ZoneName"
if (-not $zones.result.Count) { throw "Zone nao encontrada: $ZoneName" }
$zoneId = $zones.result[0].id
Write-Host "Zone ID: $zoneId"

function Set-CfRecord {
    param([string]$Name, [string]$Type, [string]$Content)
    $fqdn = "$Name.$ZoneName"
    $existing = Invoke-CfApi GET "https://api.cloudflare.com/client/v4/zones/$zoneId/dns_records?name=$fqdn&type=$Type"
    $body = @{
        type    = $Type
        name    = $Name
        content = $Content
        proxied = [bool]$Proxied
        ttl     = 1
    }
    if ($existing.result.Count -gt 0) {
        $id = $existing.result[0].id
        Write-Host "Atualizando $fqdn -> $Content (proxied=$Proxied)" -ForegroundColor Yellow
        Invoke-CfApi PUT "https://api.cloudflare.com/client/v4/zones/$zoneId/dns_records/$id" $body | Out-Null
    } else {
        Write-Host "Criando $fqdn -> $Content (proxied=$Proxied)" -ForegroundColor Green
        Invoke-CfApi POST "https://api.cloudflare.com/client/v4/zones/$zoneId/dns_records" $body | Out-Null
    }
}

Set-CfRecord -Name "api.dev" -Type "A" -Content $Ec2Ip
Set-CfRecord -Name "aspire.dev" -Type "A" -Content $Ec2Ip

Write-Host "Ajustando SSL/TLS para Flexible ..." -ForegroundColor Cyan
Invoke-CfApi PATCH "https://api.cloudflare.com/client/v4/zones/$zoneId/settings/ssl" @{ value = "flexible" } | Out-Null

Write-Host "`nRegistros aplicados. Teste em ~1 min:" -ForegroundColor Green
Write-Host "  curl https://api.dev.libify.com.br/health"
