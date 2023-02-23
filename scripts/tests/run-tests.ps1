docker compose `
    -f (Join-Path $PSScriptRoot "docker-compose.yml") `
    -f (Join-Path $PSScriptRoot "../backend/docker-compose.base.yml") `
    -f (Join-Path $PSScriptRoot "../backend/docker-compose.dev.yml") `
    -f (Join-Path $PSScriptRoot "../postgres/docker-compose.yml") `
    up