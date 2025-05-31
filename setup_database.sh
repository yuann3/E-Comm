#!/bin/bash

# Setup MSSQL Database for E-Comm Application
# This script will run the database creation script on MSSQL Server 2022 running in Docker

echo "Setting up MSSQL Database for E-Comm Application..."

# Database connection parameters
SERVER="localhost,1433"
USERNAME="sa"
PASSWORD="LYy95858sg@"
SQL_SCRIPT="Requirement/scripted_db.sql"

# Check if the SQL script exists
if [ ! -f "$SQL_SCRIPT" ]; then
    echo "Error: SQL script not found at $SQL_SCRIPT"
    exit 1
fi

echo "Connecting to MSSQL Server at $SERVER..."

if command -v sqlcmd &> /dev/null; then
    echo "Running database setup script..."
    sqlcmd -S "$SERVER" -U "$USERNAME" -P "$PASSWORD" -C -i "$SQL_SCRIPT"
    
    if [ $? -eq 0 ]; then
        echo "Database setup completed successfully!"
    else
        echo "Error: Database setup failed!"
        exit 1
    fi
else
    echo "sqlcmd not found. Please install SQL Server command line tools or run the script manually."
    echo ""
    echo "To install sqlcmd on macOS:"
    echo "brew install microsoft/mssql-release/mssql-tools18"
    echo ""
    echo "Or run the script manually using a SQL client with these connection details:"
    echo "Server: $SERVER"
    echo "Username: $USERNAME"
    echo "Password: $PASSWORD"
    echo "Script: $SQL_SCRIPT"
fi

echo "Done!" 