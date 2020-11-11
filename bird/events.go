package bird

import (
	"fmt"
	"github.com/bwmarrin/discordgo"
	"log"
	"math/rand"
	"peskybird/database"
	"regexp"
	"strings"
)

func New(activator string, handle *database.Handle) *Bird {
	bird := new(Bird)
	bird.handle = handle
	bird.activator = activator
	return bird
}

type Bird struct {
	activator string
	handle    *database.Handle
}

func (me *Bird) MessageCreateHandler(s *discordgo.Session, m *discordgo.MessageCreate) {
	if m.Author.Bot {
		return
	}

	if strings.HasPrefix(m.Content, me.activator) {
		command := strings.ToLower(m.Content[len(me.activator):])

		if command == "help" {
			me.printHelp(s, m)
			return
		}

		if command == "sayhello" {
			_, err := s.ChannelMessageSend(m.ChannelID, "Hello "+m.Author.Username+", Pesky wants a Cookie")
			if err != nil {
				log.Fatal(err)
			}

			return
		}

		if command == "quote" {
			quotes, err := me.handle.GetQuotes(m.GuildID)
			if err != nil {
				log.Fatal(err)
			} else if len(quotes) > 0 {
				index := rand.Int31n(int32(len(quotes)))
				s.ChannelMessageSend(m.ChannelID, quotes[index].Quote)
			} else {
				s.ChannelMessageSend(m.ChannelID, "there is nothing to quote!")
			}
			return
		}

		r := regexp.MustCompile("addquote (.*)")
		match := r.FindStringSubmatch(command)
		if len(match) > 0 {
			quote := match[1]
			err := me.handle.AddQuote(quote, m.GuildID, m.Author.ID)
			if err != nil {
				log.Fatal(err)
			} else {
				s.ChannelMessageSend(m.ChannelID, "Quote added")
			}

			return
		}
	}
}

func (me *Bird) printHelp(s *discordgo.Session, m *discordgo.MessageCreate) {
	var b strings.Builder
	b.WriteString("```")
	b.WriteString(fmt.Sprintf("all commands are preceeded with '%v' as command activator", me.activator))
	b.WriteString("\n")
	b.WriteString("sayhello")
	b.WriteString("\n")
	b.WriteString("\t- checks if pesky is still alive")
	b.WriteString("\n")
	b.WriteString("quote")
	b.WriteString("\n")
	b.WriteString("\t- lists random quote of the current server")
	b.WriteString("\n")
	b.WriteString("addquote <content>")
	b.WriteString("\n")
	b.WriteString("\t- adds <content> to the quote list of this server")
	b.WriteString("```")

	s.ChannelMessageSend(m.ChannelID, b.String())
}
