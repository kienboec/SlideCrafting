$FilePath = ".\slideCrafting.sh"
(Get-Content -Raw -Path $FilePath) -replace '\n','\r\n' | Set-Content -Path $FilePath

$FilePath = ".\run.sh"
(Get-Content -Raw -Path $FilePath) -replace '\n','\r\n' | Set-Content -Path $FilePath

$FilePath = ".\updateTemplate.sh"
(Get-Content -Raw -Path $FilePath) -replace '\n','\r\n' | Set-Content -Path $FilePath

$FilePath = ".\watch.sh"
(Get-Content -Raw -Path $FilePath) -replace '\n','\r\n' | Set-Content -Path $FilePath
