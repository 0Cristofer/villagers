#!/bin/bash

# Villagers - Start Frontend

echo "🌐 Starting Villagers Frontend..."

# Install dependencies and start frontend
echo "📦 Installing frontend dependencies..."
cd frontend
npm install --silent
if [ $? -ne 0 ]; then
    echo "❌ Frontend dependency installation failed!"
    exit 1
fi

echo "✅ Frontend dependencies installed!"
echo ""

# Start frontend with Vite
echo "🚀 Starting React frontend on https://localhost:3000..."
npm run dev