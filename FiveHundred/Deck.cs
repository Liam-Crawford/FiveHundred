using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace FiveHundred
{
    class Deck
    {
        private List<Card> cards = new List<Card>();
        private Dictionary<ImageSource, Card> imageLinks = new Dictionary<ImageSource, Card>();
        Random r = new Random();

        /// <summary>
        /// Default constructor makes a new deck of 52 cards.
        /// By default this is in order of Ace, 2-10, Jack-King, 
        /// Spades->Clubs->Diamonds->Hearts->Jokers.
        /// </summary>
        public Deck()
        {
            for (int s = 0; s < 4; s++)
            {
                for (int v = 2; v < 15; v++)
                {
                    cards.Add(new Card((Suit)s, (Value)v));
                }
            }
            cards.Add(new Card(Suit.Joker, Value.Two));
            cards.Add(new Card(Suit.Joker, Value.Three));

            createImageLinks();
        }

        /// <summary>
        /// Deals a random card from the deck.
        /// </summary>
        /// <returns>Card</returns>
        public Card dealFromDeck()
        {
            int newCard = r.Next(cards.Count);
            Card thisCard = null;

            if (cards.Count > 0)
            {
                thisCard = cards.ElementAt(newCard);
            }

            cards.RemoveAt(newCard);

            return thisCard;
        }

        /// <summary>
        /// Always deals the next card from index 0. Because the deck is made
        /// in order, this will deal in order from Ace -> 2 through 10 -> Jack, Queen, King,
        /// first with Spades then Clubs, Diamonds, Hearts, and finally the Jokers.
        /// </summary>
        /// <returns>Card</returns>
        public Card dealInOrder()
        {
            Card thisCard = cards.ElementAt(0);
            cards.Remove(thisCard);
            return thisCard;
        }

        public void shuffle()
        {
            // Something something dark side.
        }

        /// <summary>
        /// Initialises a dictionary with mapping of each card to it's image source.
        /// this is needed because while we have an image inside the card class,
        /// we have no way of working backwards from an image to the card.
        /// </summary>
        private void createImageLinks()
        {
            foreach (Card c in cards)
            {
                imageLinks.Add(c.ImgSource, c);
            }
        }

        /// <summary>
        /// Getter for the dictionary of imagelinks.
        /// </summary>
        public Dictionary<ImageSource, Card> ImageLinks
        {
            get { return this.imageLinks; }
        }

        /// <summary>
        /// Sets up the game for 500 which means taking out one of the Jokers,
        /// all of the 2s and 3s, and the black 4s.
        /// </summary>
        public void setUpFor500()
        {
            List<Card> cardsToRemove = new List<Card>();

            foreach (Card c in cards)
            {
                if (c.Value == Value.Two) cardsToRemove.Add(c);
                else if (c.Value == Value.Three && c.Suit != Suit.Joker) cardsToRemove.Add(c);
                else if (c.Value == Value.Four && c.Suit == Suit.Spades) cardsToRemove.Add(c);
                else if (c.Value == Value.Four && c.Suit == Suit.Clubs) cardsToRemove.Add(c);
            }

            foreach (Card c in cardsToRemove)
            {
                cards.Remove(c);
            }
        }

    }
}
