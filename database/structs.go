package database

import "time"

type Quote struct {
	Id     string
	Quote  string
	Server string
	User   string
	Time   time.Time
}
