using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace FiveHundred
{
    class Card : IComparable<Card>
    {
        private Suit suit;
        private Value value;      // Ace = 1, Jack = 11, Queen = 12, King = 13.
        private Image img = new Image();

        public Card(Suit s, Value v)
        {
            suit = s;
            value = v;

            img.Source = new BitmapImage(new Uri(determinePath()));
        }

        public Suit Suit
        {
            get { return this.suit; }
            set { this.suit = value; }
        }

        public Value Value
        {
            get { return this.value; }
            set { this.value = value; }
        }

        public ImageSource ImgSource
        {
            get { return this.img.Source; }
        }

        private string determinePath()
        {
            string path = "";

            switch (suit)
            {
                case Suit.Spades:
                    path = string.Format("ms-appx:///Playing Cards/Spades/S{0}.png", (int)this.value);
                    break;
                case Suit.Clubs:
                    path = string.Format("ms-appx:///Playing Cards/Clubs/C{0}.png", (int)this.value);
                    break;
                case Suit.Diamonds:
                    path = string.Format("ms-appx:///Playing Cards/Diamonds/D{0}.png", (int)this.value);
                    break;
                case Suit.Hearts:
                    path = string.Format("ms-appx:///Playing Cards/Hearts/H{0}.png", (int)this.value);
                    break;
                case Suit.Joker:
                    path = string.Format("ms-appx:///Playing Cards/Jokers/Joker_{0}.png", (int)this.value);
                    break;
            }

            return path;
        }

        public override string ToString()
        {
            if (this.suit == Suit.Joker)
            {
                return ("Joker " + (int)this.value);
            }
            return (this.value + " of " + this.suit);
        }

        /// Returns 1 if this card is 'bigger' than the other card, 0 for same, and -1 for smaller.
        /// this goes from 2-10, Jack-King, then Ace, in order of
        /// Spades, Clubs, Diamonds, Hearts, with Joker being absolute top card.
        int IComparable<Card>.CompareTo(Card other)
        {
            if (this.suit > other.suit)
            {
                return 1;
            }
            else if (this.suit == other.suit)
            {
                if (this.value > other.value) return 1;
                else if (this.value == other.value) return 0;
                else return -1;
            }
            else
            {
                return -1;
            }
        }
    }
}
