#!/bin/bash

# Villagers - Database Setup Script

echo "🗄️ Setting up Villagers databases..."

# Store the original directory
ORIGINAL_DIR=$(pwd)

# Change to /tmp to avoid permission issues
cd /tmp

# Create user
echo "Creating PostgreSQL user..."
sudo -u postgres createuser villagers_user 2>/dev/null || echo "ℹ️ User villagers_user already exists"

# Set password
echo "Setting user password..."
sudo -u postgres psql -c "ALTER USER villagers_user PASSWORD 'villagers_password';"

# Create databases
echo "Creating databases..."
sudo -u postgres createdb villagers_api -O villagers_user 2>/dev/null || echo "ℹ️ Database villagers_api already exists"
sudo -u postgres createdb villagers_game -O villagers_user 2>/dev/null || echo "ℹ️ Database villagers_game already exists"

# Grant privileges
echo "Granting privileges..."
sudo -u postgres psql -c "GRANT ALL PRIVILEGES ON DATABASE villagers_api TO villagers_user;"
sudo -u postgres psql -c "GRANT ALL PRIVILEGES ON DATABASE villagers_game TO villagers_user;"

echo "✅ Database setup complete!"
echo ""
echo "📋 Created:"
echo "  - User: villagers_user"
echo "  - Databases: villagers_api, villagers_game"
echo ""

# Return to the original directory for migrations
cd "$ORIGINAL_DIR"

# Run EF migrations
echo "🔧 Running Entity Framework migrations..."

echo "Running API migrations..."
cd api && dotnet ef database update
if [ $? -eq 0 ]; then
    echo "✅ API migrations completed"
else
    echo "❌ API migrations failed"
fi

echo "Running Game Server migrations..."
cd ../game-server && dotnet ef database update
if [ $? -eq 0 ]; then
    echo "✅ Game Server migrations completed"
else
    echo "❌ Game Server migrations failed"
fi

cd ..
echo ""
echo "🎉 Database setup and migrations complete!"
echo "Ready to run: ./start-dev.sh"