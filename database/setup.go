package database

type MigrationScript struct {
	scriptName string
	script     string
}

func GetMigrationScripts() []MigrationScript {
	return []MigrationScript{{"setupQuotesSql.sql", setupQuotesSql}}
}
