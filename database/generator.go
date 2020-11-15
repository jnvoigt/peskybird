package database

// migration
//go:generate textFileToGoConst -in scripts/setupMigrations.sql
//go:generate textFileToGoConst -in scripts/sqliteCheckForMigrations.sql
//go:generate textFileToGoConst -in scripts/getMigrations.sql
//go:generate textFileToGoConst -in scripts/addMigration.sql
//go:generate textFileToGoConst -in scripts/20201115_setupQuotes.sql

// standard operations
//go:generate textFileToGoConst -in scripts/addQuote.sql
//go:generate textFileToGoConst -in scripts/getQuotes.sql
