Param(
  [bool]$issueNew = $false
)

$certificateDnsName = "localhost"
$certificatePassword = "sitecore"
$certificateStore = "Cert:\CurrentUser\My"
$certificateFriendlyName = "Sitecore Commerce Services Development Certificate"
$certificateOutputDirectory = "..\src\Sitecore.Commerce.Engine\wwwroot"

# Allows for issuing a new cert rather than using an existing one - this makes it easy to replace an expired cert for dev
if($issueNew)
{
	Write-Host "Deleting $certificateFriendlyName" -ForegroundColor Yellow
	Get-ChildItem -Path "Cert:\CurrentUser\My" | Where-Object{$_.FriendlyName -eq $certificateFriendlyName} | Remove-Item
	Get-ChildItem -Path "Cert:\CurrentUser\Root" | Where-Object{$_.FriendlyName -eq $certificateFriendlyName} | Remove-Item
    Get-ChildItem -Path "Cert:\CurrentUser\CA" | Where-Object{$_.FriendlyName -eq $certificateFriendlyName} | Remove-Item

    Get-ChildItem -Path "Cert:\LocalMachine\Root" | Where-Object{$_.FriendlyName -eq $certificateFriendlyName} | Remove-Item
}

$certificates = Get-ChildItem `
    -Path $certificateStore `
    -DnsName $certificateDnsName | Where-Object { $_.FriendlyName -eq $certificateFriendlyName }

if ($certificates.Length -eq 0) {
    Write-Host "Issuing new certificate" -ForegroundColor Green

    $certificate = New-SelfSignedCertificate `
        -Subject $certificateDnsName `
        -DnsName $certificateDnsName `
        -KeyAlgorithm RSA `
        -KeyLength 2048 `
        -NotBefore (Get-Date) `
        -NotAfter (Get-Date).AddYears(1) `
        -CertStoreLocation $certificateStore `
        -FriendlyName $certificateFriendlyName `
        -HashAlgorithm SHA256 `
        -KeyUsage DigitalSignature, KeyEncipherment, DataEncipherment `
        -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.1")
}
else {
    Write-Host "Found existing certificate" -ForegroundColor Yellow

    $certificate = $certificates[0]
}

$certificatePath = $certificateStore + "\" + $certificate.Thumbprint
$pfxPassword = ConvertTo-SecureString -String $certificatePassword -Force -AsPlainText
$pfxPath = "$certificateOutputDirectory\$certificateDnsName.pfx"
$cerPath = "$certificateOutputDirectory\$certificateDnsName.cer"

Export-PfxCertificate -Cert $certificatePath -FilePath $pfxPath -Password $pfxPassword
Export-Certificate -Cert $certificatePath -FilePath $cerPath

Import-PfxCertificate -FilePath $pfxPath -CertStoreLocation $certificateStore -Password $pfxPassword -Exportable

$rootCerticatePath = Import-Certificate -FilePath $cerPath -CertStoreLocation Cert:\LocalMachine\Root
$rootCerticatePath.FriendlyName = $certificateFriendlyName
