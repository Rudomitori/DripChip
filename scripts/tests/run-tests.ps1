$variant = $args[0]

switch ($variant){
    Build {
        docker compose `
            -f (Join-Path $PSScriptRoot "docker-compose.base.yml") `
            -f (Join-Path $PSScriptRoot "docker-compose.build.yml") `
            -f (Join-Path $PSScriptRoot "../backend/docker-compose.base.yml") `
            -f (Join-Path $PSScriptRoot "../backend/docker-compose.build.yml") `
            -f (Join-Path $PSScriptRoot "../postgres/docker-compose.yml") `
            up
    }
    Local {
        docker compose `
            -f (Join-Path $PSScriptRoot "docker-compose.base.yml") `
            -f (Join-Path $PSScriptRoot "docker-compose.local.yml") `
            up
    }
    Archive {
        $archiveName = "Ivan Ivanov"
        $createArchive = Resolve-Path (Join-Path $PSScriptRoot "../archive/create-archive.ps1")
        
        Invoke-Expression "${createArchive} '${archiveName}'"
        Expand-Archive -Path "${archiveName}.zip" -DestinationPath $archiveName

        Invoke-Expression "& '$(Resolve-Path (Join-Path $archiveName "Build/run-tests.ps1"))'"
    }
}

