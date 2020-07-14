package bird

import (
	"github.com/bwmarrin/discordgo"
	"log"
	rand2 "math/rand"
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

		if command == "sayHello" {
			_, err := s.ChannelMessageSend(m.ChannelID, "Hello "+m.Author.Username+", Pesky wants Cooky")
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
				index := rand2.Int31n(int32(len(quotes)))
				s.ChannelMessageSend(m.ChannelID, quotes[index].Quote)
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
