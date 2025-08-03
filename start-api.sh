#!/bin/bash

# Villagers - Start API Server

echo "⚡ Starting Villagers API Server..."

# Build API
echo "🔨 Building API..."
cd api
dotnet build
if [ $? -ne 0 ]; then
    echo "❌ API build failed!"
    exit 1
fi

echo "✅ API build completed!"
echo ""

# Start API server
echo "🚀 Starting API server on https://localhost:3002..."
dotnet run --launch-profile "Villagers API (HTTPS)"