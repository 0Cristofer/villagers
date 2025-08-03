#!/bin/bash

# Villagers - Start Game Server

echo "🎯 Starting Villagers Game Server..."

# Build Game Server
echo "🔨 Building Game Server..."
cd game-server
dotnet build
if [ $? -ne 0 ]; then
    echo "❌ Game Server build failed!"
    exit 1
fi

echo "✅ Game Server build completed!"
echo ""

# Start Game Server (will register with API)
echo "🚀 Starting Game Server on https://localhost:5034..."
echo "📡 Game Server will register with API on startup"
dotnet run --launch-profile "Game Server (HTTPS)"