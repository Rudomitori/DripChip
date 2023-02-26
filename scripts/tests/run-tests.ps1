$variant = $args[0]

switch ($variant){
    Build {
        docker compose `
            -f (Join-Path $PSScriptRoot "docker-compose.base.yml") `
            -f (Join-Path $PSScriptRoot "docker-compose.build.yml") `
            -f (Join-Path $PSScriptRoot "../backend/docker-compose.base.yml") `
            -f (Join-Path $PSScriptRoot "../backend/docker-compose.dev.yml") `
            -f (Join-Path $PSScriptRoot "../postgres/docker-compose.yml") `
            up
    }
    Local {
        docker compose `
            -f (Join-Path $PSScriptRoot "docker-compose.base.yml") `
            -f (Join-Path $PSScriptRoot "docker-compose.local.yml") `
            up
    }
}

