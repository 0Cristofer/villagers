#!/bin/bash

# Villagers - Development Server Startup Script

echo "🎮 Starting Villagers Development Environment..."

# Kill any existing processes
echo "🔄 Stopping existing servers..."
pkill -f "dotnet run" 2>/dev/null || true
pkill -f "react-scripts" 2>/dev/null || true
sleep 2

# Build all services
echo "🔨 Building all services..."

echo "  Building Game Server..."
cd game-server
dotnet build
if [ $? -ne 0 ]; then
    echo "❌ Game Server build failed!"
    exit 1
fi

echo "  Building Lambda API..."
cd ../api
dotnet build
if [ $? -ne 0 ]; then
    echo "❌ Lambda API build failed!"
    exit 1
fi

echo "  Installing frontend dependencies..."
cd ../frontend
npm install --silent
if [ $? -ne 0 ]; then
    echo "❌ Frontend dependency installation failed!"
    exit 1
fi

echo "✅ All builds completed successfully!"
echo ""

# Start Game Server (ECS)
echo "🎯 Starting Game Server (ECS)..."
cd ../game-server
dotnet run &
GAME_SERVER_PID=$!
echo "Game Server started with PID: $GAME_SERVER_PID"

# Wait for game server to start
sleep 3

# Start Lambda API
echo "⚡ Starting Lambda API..."
cd ../api
dotnet run --urls="http://localhost:3001" &
API_PID=$!
echo "Lambda API started with PID: $API_PID"

# Wait for API to start
sleep 3

# Start frontend React app
echo "🌐 Starting React frontend..."
cd ../frontend
npm start &
FRONTEND_PID=$!
echo "Frontend started with PID: $FRONTEND_PID"

echo ""
echo "✅ Development environment is starting up!"
echo "📱 Frontend: http://localhost:3000"
echo "⚡ Lambda API: http://localhost:3001 (Swagger: http://localhost:3001/swagger)"
echo "🎯 Game Server: http://localhost:5033"
echo ""
echo "🏗️  Architecture:"
echo "   Client → Lambda API → Game Server"
echo "   Client ← SignalR ← Game Server"
echo ""
echo "💡 To stop servers: ./stop-dev.sh or Ctrl+C"
echo ""

# Wait for user to stop
wait