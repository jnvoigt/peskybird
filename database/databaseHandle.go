package database

import (
	"database/sql"
	"fmt"
	"log"
	"time"
)

type Handle struct {
	db *sql.DB
}

func NewHandle(db *sql.DB) *Handle {
	handle := new(Handle)
	handle.db = db
	return handle
}

func (me *Handle) Migration() error {
	setupMap := GetMigrationScripts()
	currentMigrations, _ := me.getExistingMigrations()

	for _, entry := range setupMap {
		if !currentMigrations[entry.scriptName] {
			err2 := me.executeMigration(entry)
			if err2 != nil {
				return err2
			}

		}
	}

	return nil
}

func (me *Handle) executeMigration(entry MigrationScript) error {
	log.Print(fmt.Sprintf("run setup script: %v", entry.scriptName))
	statement, _ := me.db.Prepare(entry.script)
	defer statement.Close()

	_, err := statement.Exec()
	if err != nil {
		return err
	}

	statement, _ = me.db.Prepare(addMigrationSql)
	defer statement.Close()

	_, err = statement.Exec(entry.scriptName, time.Now())
	if err != nil {
		return err
	}
	return nil
}

func (me *Handle) AddQuote(quote, server, user string) error {
	stmt, _ := me.db.Prepare(addQuoteSql)
	_, err := stmt.Exec(quote, server, user, time.Now())
	stmt.Close()
	if err != nil {
		return err
	}

	return nil
}

func (me *Handle) GetQuotes(guildId string) ([]Quote, error) {
	stmt, _ := me.db.Prepare(getQuotesSql)
	rows, err := stmt.Query(guildId)
	stmt.Close()

	if err != nil {
		return nil, err
	}

	result := make([]Quote, 0, 10)
	for rows.Next() {
		var rowValue Quote
		rows.Scan(&rowValue.Id, &rowValue.Quote, &rowValue.Server, &rowValue.User, &rowValue.Time)
		result = append(result, rowValue)
	}

	return result, nil
}

func (me *Handle) getExistingMigrations() (map[string]bool, error) {

	statement, _ := me.db.Prepare(sqliteCheckForMigrationsSql)
	rows, _ := statement.Query()
	defer rows.Close()
	defer statement.Close()

	if !rows.Next() {
		statement, _ = me.db.Prepare(setupMigrationsSql)
		statement.Exec()
		statement.Close()
		return make(map[string]bool), nil
	}

	statement, _ = me.db.Prepare(getMigrationsSql)
	rows, _ = statement.Query()
	defer rows.Close()
	defer statement.Close()
	scriptMap := make(map[string]bool)

	for rows.Next() {
		scriptName := ""
		rows.Scan(&scriptName)
		scriptMap[scriptName] = true
	}

	return scriptMap, nil
}
