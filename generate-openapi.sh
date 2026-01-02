#!/bin/bash
set -e

echo "🚀 Generando openapi.json..."

# Variables
PORT=5123
PROJECT_DIR="DoliMiddlewareApi"
OUTPUT_FILE="openapi.json"

# Levantar la API en background
cd "$PROJECT_DIR"
dotnet run --urls "http://localhost:$PORT" > /dev/null 2>&1 &
PID=$!

# Esperar a que arranque
echo "⏳ Esperando a que la API arranque..."
sleep 8

# Descargar el OpenAPI spec
echo "📥 Descargando openapi.json..."
curl -s "http://localhost:$PORT/openapi/v1.json" -o "../$OUTPUT_FILE"

# Matar el proceso
kill $PID 2>/dev/null || true

echo "✅ openapi.json generado exitosamente en: $OUTPUT_FILE"
echo ""
echo "📦 Para generar cliente TypeScript:"
echo "   npx @openapitools/openapi-generator-cli generate -i openapi.json -g typescript-fetch -o ./client"
echo ""
echo "📦 Para importar en Postman:"
echo "   File > Import > Selecciona openapi.json"
