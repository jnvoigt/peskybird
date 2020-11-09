package database

import (
	"database/sql"
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

func (me *Handle) Setup() {
	setupMap := GetSetupMap()

	for key := range setupMap {
		log.Print("run setup script: %v", key)

		stmtString := setupMap[key]
		statement, _ := me.db.Prepare(stmtString)
		statement.Exec()
		defer statement.Close()
	}
}

func (me *Handle) AddQuote(quote, server, user string) error {
	stmt, _ := me.db.Prepare(addQuoteSql)
	_, err := stmt.Exec(quote, server, user, time.Now())
	defer stmt.Close()
	if err != nil {
		return err
	}

	return nil
}

func (me *Handle) GetQuotes(guildId string) ([]Quote, error) {
	stmt, _ := me.db.Prepare(getQuotesSql)
	rows, err := stmt.Query(guildId)
	defer stmt.Close()

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
