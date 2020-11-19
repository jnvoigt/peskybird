CREATE TABLE if not exists Quotes (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  quote VARCHAR(280),
  server uniqueidentifier,
  user VARCHAR(32),
  time DATETIME(128)
)