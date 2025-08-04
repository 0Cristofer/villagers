#!/bin/bash

# Villagers - Start Game Server
# Usage: ./start-gameserver.sh [port]
# Example: ./start-gameserver.sh 5035

PORT=$1

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

# Start Game Server
if [ -n "$PORT" ]; then
    # Custom port specified
    if ! [[ "$PORT" =~ ^[0-9]+$ ]] || [ "$PORT" -lt 1024 ] || [ "$PORT" -gt 65535 ]; then
        echo "❌ Invalid port: $PORT. Port must be a number between 1024 and 65535."
        exit 1
    fi
    
    echo "🚀 Starting Game Server on https://localhost:$PORT..."
    echo "📡 Game Server will register with API on startup"
    
    # Use --no-launch-profile and manually set all the needed environment variables
    export ASPNETCORE_ENVIRONMENT="Development"
    export ASPNETCORE_URLS="https://localhost:$PORT;http://localhost:$((PORT-1))"
    export Server__Endpoint="https://localhost:$PORT"
    dotnet run --no-launch-profile
else
    # Use default launch profile
    echo "🚀 Starting Game Server on https://localhost:5034..."
    echo "📡 Game Server will register with API on startup"
    dotnet run --launch-profile "Game Server (HTTPS)"
fi