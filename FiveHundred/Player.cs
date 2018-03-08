using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiveHundred
{
    class Player
    {
        private string name;
        private List<Card> hand;
        private int points;
        private int tricksWon;
        private int hasBidded = -1;
        private Suit biddedSuit;
        private Value biddedValue;
        private Player partner;
        private bool hasLeadTrumps = false;

        public Player(string n)
        {
            name = n;
            points = 0;
            hand = new List<Card>();
        }

        /// <summary>
        ///  Code to reset player hand to no cards.
        /// </summary>
        public void resetHand()
        {
            this.hand.Clear();
        }

        public bool isLegalMove(List<Card> cardsInPlay, Card thisCard)
        {
            // if we are the lead, we can play anything.
            if (cardsInPlay.Count == 0) return true;

            Card leadCard = cardsInPlay.ElementAt(0);

            // If the card we click is the same suit as the lead card, it is legal.
            if (leadCard.Suit == thisCard.Suit) return true;

            // If we've got this far, we are trying to play a card that doesn't match
            // the lead suit. This is only legal if we don't have any of the lead suit.
            bool hasSuit = false;
            foreach (Card c in this.Hand)
            {
                // If any of the cards in our hand match the lead suit at this point, 
                // the clicked card can not be played.
                if (c.Suit == leadCard.Suit) hasSuit = true;
            }
            if (hasSuit == true) return false;
            else if (hasSuit == false) return true;

            return false;
        }

        public bool isValidBid(Suit s, Value v, Player currentHighestBidder)
        {
            if (currentHighestBidder != null)
            {
                if (currentHighestBidder != this)
                {
                    Suit hs = currentHighestBidder.BiddedSuit;
                    Value hv = currentHighestBidder.BiddedValue;

                    if (v > hv) return true;
                    else if (v == hv)
                    {
                        if (s > hs) return true;
                        else return false;
                    }
                    else return false;
                }
            }

            return true;
        }

        public void assignPoints()
        {
            this.points += this.tricksWon * 10;
        }

        /// <summary>
        /// getter for player hand.
        /// </summary>
        public List<Card> Hand
        {
            get { return this.hand; }
        }

        /// <summary>
        /// Get and set for player name.
        /// </summary>
        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }

        /// <summary>
        /// Get and set for player points.
        /// </summary>
        public int Points
        {
            get { return this.points; }
            set { this.points = value; }
        }

        public Suit BiddedSuit
        {
            get { return this.biddedSuit; }
            set { this.biddedSuit = value; }
        }

        public Value BiddedValue
        {
            get { return this.biddedValue; }
            set { this.biddedValue = value; }
        }

        public int HasBidded
        {
            get { return this.hasBidded; }
            set { this.hasBidded = value; }
        }

        public int TricksWon
        {
            get { return this.tricksWon; }
            set { this.tricksWon = value; }
        }

        public Player Partner
        {
            get { return this.partner; }
            set { this.partner = value; }
        }

        public bool HasLeadTrumps
        {
            get { return this.hasLeadTrumps; }
            set { this.hasLeadTrumps = value; }
        }

        /// <summary>
        /// Override for player name eg. "Player North".
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ("Player " + name);
        }
    }
}
