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
		command := m.Content[len(me.activator):]

		if command == "help" {
			me.printHelp(s, m)
			return
		}

		if command == "sayHello" {
			_, err := s.ChannelMessageSend(m.ChannelID, "Hello "+m.Author.Username+", Pesky wants a Cooky")
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
	b.WriteString(fmt.Sprintf("all commands are preceeded with '%v' as command activator", me.activator))
	b.WriteString("\n")
	b.WriteString("sayHello")
	b.WriteString("\n")
	b.WriteString("\tchecks if pesky is still alive")
	b.WriteString("\n")
	b.WriteString("quote")
	b.WriteString("\n")
	b.WriteString("\tlists random quote of the current server")
	b.WriteString("\n")
	b.WriteString("addquot <content>")
	b.WriteString("\n")
	b.WriteString("\tadds <content> to the quote list of this server")

	s.ChannelMessageSend(m.ChannelID, b.String())
}
