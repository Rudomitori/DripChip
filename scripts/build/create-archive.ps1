$archiveName = $args[0]
$imageName = "drip-chip"

$repositoryRootPath = Join-Path $PSScriptRoot "..\.."
$repositoryRootPath = Resolve-Path $repositoryRootPath
Write-Output "Repositori root path: ${repositoryRootPath}"

$tempWorkDirPath = Join-Path ([System.IO.Path]::GetTempPath()) ([Guid]::NewGuid())
Write-Output "Temp work dir path: ${tempWorkDirPath}"

$_ = [System.IO.Directory]::CreateDirectory($tempWorkDirPath)

git clone $repositoryRootPath (Join-Path $tempWorkDirPath "Sources")

$buildFolder = Join-Path $tempWorkDirPath "Build"
$_ = [System.IO.Directory]::CreateDirectory($buildFolder)
Write-Output "Build folder: ${buildFolder}"

docker build -t $imageName `
    -f (Join-Path $repositoryRootPath "src/DripChip.WebApi/Dockerfile") `
    (Join-Path $repositoryRootPath "src")

docker save $imageName --output (Join-Path $buildFolder "${imageName}.tar")

docker compose `
    -f (Join-Path $repositoryRootPath "scripts/tests/docker-compose.base.yml") `
    -f (Join-Path $repositoryRootPath "scripts/tests/docker-compose.build.yml") `
    -f (Join-Path $repositoryRootPath "scripts/postgres/docker-compose.yml") `
    -f (Join-Path $repositoryRootPath "scripts/backend/docker-compose.base.yml") `
    -f (Join-Path $repositoryRootPath "scripts/backend/docker-compose.build.yml") `
    config > (Join-Path $buildFolder "docker-compose.yml")

Copy-Item (Join-Path $repositoryRootPath "scripts/build/assets/*")`
    -Destination ${buildFolder}

Compress-Archive `
    -Path (Join-Path $tempWorkDirPath "*") `
    -CompressionLevel "Fastest" `
    -DestinationPath "${archiveName}.zip" `
    -Force
Remove-Item -Path $tempWorkDirPath -Recurse -Force