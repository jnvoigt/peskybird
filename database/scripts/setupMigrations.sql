CREATE TABLE if not exists _dbMigrations (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  script VARCHAR(280),
  time DATETIME(128)
)