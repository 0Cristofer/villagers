#!/bin/bash

# Villagers - Stop Development Servers

echo "🛑 Stopping Villagers development servers..."

# Kill .NET processes (Game Server and Lambda API)
echo "Stopping Game Server and Lambda API..."
pkill -f "dotnet run" 2>/dev/null && echo "✅ .NET services stopped" || echo "ℹ️  No .NET processes found"

# Kill frontend processes
echo "Stopping React frontend (Vite)..."
pkill -f "vite" 2>/dev/null && echo "✅ Frontend stopped" || echo "ℹ️  No frontend process found"

echo "🏁 All development servers stopped!"