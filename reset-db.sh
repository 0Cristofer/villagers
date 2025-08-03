#!/bin/bash

# Villagers - Database Reset Script

echo "🗑️ Resetting Villagers databases..."

# Change to /tmp to avoid permission issues
cd /tmp

# Drop databases
echo "Dropping databases..."
sudo -u postgres dropdb villagers_api 2>/dev/null || echo "ℹ️ Database villagers_api doesn't exist"
sudo -u postgres dropdb villagers_game 2>/dev/null || echo "ℹ️ Database villagers_game doesn't exist"

# Drop user
echo "Dropping user..."
sudo -u postgres dropuser villagers_user 2>/dev/null || echo "ℹ️ User villagers_user doesn't exist"

echo "✅ Database cleanup complete!"
echo ""
echo "🔧 To recreate databases, run: ./setup-db.sh"