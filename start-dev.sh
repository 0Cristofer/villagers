#!/bin/bash

# Villagers - Development Environment Launcher
# 
# This script starts all services together. For individual control, use:
# - ./start-api.sh      (Start API server only)
# - ./start-gameserver.sh (Start Game Server only) 
# - ./start-frontend.sh (Start Frontend only)

echo "🎮 Starting Villagers Development Environment..."

# Kill any existing processes
echo "🔄 Stopping existing servers..."
pkill -f "dotnet run" 2>/dev/null || true
pkill -f "vite" 2>/dev/null || true
sleep 2

# Start services in correct order using individual scripts
echo "⚡ Starting Lambda API..."
./start-api.sh &
API_PID=$!
echo "Lambda API started with PID: $API_PID"

# Wait for API to be ready
echo "  Waiting for API to initialize..."
sleep 5

echo "🎯 Starting Game Server..."
./start-gameserver.sh &
GAME_SERVER_PID=$!
echo "Game Server started with PID: $GAME_SERVER_PID"

# Wait for game server to start and register
echo "  Waiting for Game Server to register with API..."
sleep 3

echo "🌐 Starting Frontend..."
./start-frontend.sh &
FRONTEND_PID=$!
echo "Frontend started with PID: $FRONTEND_PID"

echo ""
echo "✅ Development environment is ready!"
echo "📱 Frontend: https://localhost:3000"
echo "⚡ Lambda API: https://localhost:3002 (Swagger: https://localhost:3002/swagger)"
echo "🎯 Game Server: https://localhost:5034"
echo ""
echo "🏗️  Architecture & Startup Order:"
echo "   1. ⚡ API starts first (world registry service)"
echo "   2. 🎯 Game Server starts and registers with API"
echo "   3. 📱 Frontend connects to both services"
echo "   Flow: Client → API (Auth) + Client ← SignalR ← Game Server (Real-time)"
echo ""
echo "💡 Individual services:"
echo "   ./start-api.sh      - Start API only"
echo "   ./start-gameserver.sh - Start Game Server only"
echo "   ./start-frontend.sh - Start Frontend only"
echo "   ./stop-dev.sh       - Stop all services"
echo ""

# Wait for user to stop
wait