package main

import (
	"database/sql"
	"github.com/bwmarrin/discordgo"
	"github.com/joho/godotenv"
	_ "github.com/mattn/go-sqlite3"
	"log"
	"os"
	"os/signal"
	"peskybird/bird"
	"peskybird/database"
	"syscall"
)

func main() {
	err := godotenv.Load()
	if err != nil {
		log.Fatal("Error loading .env file")
		return
	}

	db, err := prepareDataBase()
	if err != nil {
		log.Fatal("Error loading .env file")
		return
	}

	handle := database.NewHandle(db)
	log.Print("Pesky bird migration started")
	err = handle.Migration()

	if err != nil {
		log.Fatal(err)
		return
	}

	log.Print("Pesky bird started")
	err = runBot(handle)
	if err != nil {
		log.Fatal(err)
		return
	}
}

func runBot(handle *database.Handle) error {
	peskyToken := os.Getenv("PESKY_TOKEN")
	activator := os.Getenv("PESKY_ACTIVATOR")

	log.Printf("started with activator '%v'", activator)
	pesky := bird.New(activator, handle)

	discord, err := discordgo.New("Bot " + peskyToken)
	if err != nil {
		log.Fatal("Error creating discord client")
		return err
	}

	discord.AddHandler(pesky.MessageCreateHandler)

	if err := discord.Open(); err != nil {
		log.Fatal("Error while opening Discord")
		return err
	}

	defer discord.Close()

	sc := make(chan os.Signal, 1)
	signal.Notify(sc, syscall.SIGINT, syscall.SIGTERM, syscall.SIGSEGV, syscall.SIGHUP)
	<-sc
	log.Print("shut down bird")
	return nil
}

func prepareDataBase() (*sql.DB, error) {
	db, err := sql.Open("sqlite3", "file:pesky.sqlite")

	if err != nil {
		log.Fatal("Error loading database")
		return nil, err
	}

	return db, nil
}
