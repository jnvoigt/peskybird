package database

type MigrationScript struct {
	scriptName string
	script     string
}

// create this automatic via generate
func GetMigrationScripts() []MigrationScript {
	return []MigrationScript{{"20201115_setupQuotes.sql", setupQuotesSql}}
}
