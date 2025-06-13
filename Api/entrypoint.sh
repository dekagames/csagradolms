#!/bin/bash
set -e

echo "Executando migrations..."
until dotnet ef database update; do
    echo "Falha ao executar migrations. Tentando novamente em 5 segundos..."
    sleep 5
done

echo "Migrations aplicadas com sucesso!"
exec dotnet WebApi.dll