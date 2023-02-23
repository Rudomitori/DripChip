docker image load -i (Join-Path $PSScriptRoot "drip-chip.tar")
docker compose -f (Join-Path $PSScriptRoot "docker-compose.yml") up