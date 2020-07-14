package database

func GetSetupMap() map[string]string {
	return map[string]string{
		"quotes": setupQuotesSql,
	}
}
