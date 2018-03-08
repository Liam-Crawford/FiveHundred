using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiveHundred
{
    class PlayerAI : Player
    {
        private List<Card> cardsPlayedSoFar = new List<Card>();
        //private int amountWillingToGoTo;
        
        Random r = new Random();

        public PlayerAI(string n) : base(n)
        {
        }

        public Card processTurn(List<Card> cardsInPlay, Player currentHighestBidder, Player currentlyWinning, Suit trumpSuit, Card currentHighestCard)
        {
            Card leadCard = null;
            if (cardsInPlay.Count > 0) leadCard = cardsInPlay[0];
            Card thisCard = null;

            // If our partner is winning the bid and there are 3 cards already in play
            // then we can throw away a low card as our partner has already won.
            if (currentlyWinning == this.Partner)
            {
                if (cardsInPlay.Count == 3)
                {
                    thisCard = findThrowAwayCard(leadCard.Suit, trumpSuit);
                } // There is some logic for what to do when your partner has lead and is currently winning.
                else if (cardsInPlay.Count == 2)
                {
                    if (cardsInPlay[0].Value == Value.Ace) thisCard = findThrowAwayCard(leadCard.Suit, trumpSuit);
                    else if (cardsInPlay[0].Suit == trumpSuit && this.Partner.HasLeadTrumps) thisCard = findHighTrumpOrThrowAway(trumpSuit, currentHighestCard);
                    else thisCard = normalMove(leadCard, thisCard, currentHighestCard, trumpSuit);
                }
            }
            else if (cardsInPlay.Count == 0)
            {
                // Lead something.
                foreach (Card c in this.Hand)
                {
                    if (c.Value == Value.Ace && c.Suit != trumpSuit) thisCard = c;
                }
                // If we have no Aces (or no more aces), and we haven't lead trumps yet to see
                // what our partner has, lead our lowest trump.
                if (thisCard == null)
                {
                    if (!this.HasLeadTrumps && currentHighestBidder == this)
                    {
                        thisCard = findLowestInSuit(trumpSuit);
                        if (thisCard != null) this.HasLeadTrumps = true;
                    }
                    else if (hasTrumpsLeft(trumpSuit)) thisCard = findRandomNonTrumpCard(trumpSuit);
                }
                
                // If thisCard is still null, just play a random card.
                if (thisCard == null) thisCard = this.Hand.ElementAt(r.Next(this.Hand.Count));
            }
            else // Our partner isn't winning, and we aren't leading.
            {
                thisCard = normalMove(leadCard, thisCard, currentHighestCard, trumpSuit);
            }

            if (thisCard == null) throw new NullReferenceException();
            this.Hand.Remove(thisCard);
            return thisCard;
        }

        private bool hasTrumpsLeft(Suit trumpSuit)
        {
            int trumps = 0;
            foreach (Card c in this.Hand)
            {
                if (c.Suit == trumpSuit) trumps++;
            }
            if (trumps > 0 && trumps < this.Hand.Count) return true;
            return false;
        }

        private Card findRandomNonTrumpCard(Suit trumpSuit)
        {
            Card thisCard = thisCard = this.Hand.ElementAt(r.Next(this.Hand.Count));
            while (thisCard.Suit == trumpSuit)
            {
                thisCard = this.Hand.ElementAt(r.Next(this.Hand.Count));
            }
            return thisCard;
        }

        private Card findHighTrumpOrThrowAway(Suit trumpSuit, Card currentHighest)
        {
            Card thisCard = null;
            foreach (Card c in this.Hand)
            {
                // Find our highest trump.
                if (c.Suit == trumpSuit && thisCard == null) thisCard = c;
                else if (c.Suit == trumpSuit && c.Value > thisCard.Value) thisCard = c;
            }
            if (thisCard == null || thisCard.Value < currentHighest.Value)
            {
                // Our highest trump is still lower than the current highest so throw away.
                foreach (Card c in this.Hand)
                {
                    if (c.Suit != trumpSuit)    // Make sure the throw away card isn't a trump!
                    {
                        if (thisCard == null || thisCard.Suit == trumpSuit) thisCard = c; // If our card is currently a trump, choose the next non trump card.
                        else if (thisCard.Value > c.Value && c.Suit != trumpSuit) thisCard = c; // Keep swapping cards until we get our lowest non trump card.
                    }   
                }
            }
            return thisCard;
        }

        private Card findTrumpOrThrowAway(Suit trumpSuit)
        {
            Card thisCard = null;
            foreach (Card c in this.Hand)
            {
                // Find our lowest trump.
                if (c.Suit == trumpSuit && thisCard == null) thisCard = c;
                else if (c.Suit == trumpSuit && c.Value < thisCard.Value) thisCard = c;
            }
            if (thisCard == null)
            {
                foreach (Card c in this.Hand)
                {
                    // We don't have a trump so throw away something else low.
                    if (thisCard == null) thisCard = c;
                    else if (thisCard.Value > c.Value) thisCard = c;
                }
            }
            return thisCard;
        }

        private Card findLowestInSuit(Suit s)
        {
            // We have to follow suit and don't have a card that can win, so throw away the lowest
            // of this suit.
            Card thisCard = null;
            foreach (Card c in this.Hand)
            {
                if (thisCard == null)
                {
                    if (c.Suit == s) thisCard = c;
                }
                else if (c.Value < thisCard.Value && c.Suit == s) thisCard = c;
            }
            return thisCard;
        }

        private Card normalMove(Card leadCard, Card thisCard, Card currentHighestCard, Suit trumpSuit)
        {
            // Do we have any cards of the same suit as the lead card?
            foreach (Card c in this.Hand)
            {
                if (c.Suit == leadCard.Suit && thisCard == null) thisCard = c;
                else if (c.Suit == leadCard.Suit && c.Value > thisCard.Value) thisCard = c;
            }

            // We have the same suit as lead, and it isn't trumps.
            if (thisCard != null && thisCard.Suit != trumpSuit)
            {
                // Our suit is the same as the current highest cards suit.
                if (thisCard.Suit == currentHighestCard.Suit)
                {
                    // Our card is lower value than the current highest card, so throw away our lowest in this suit.
                    if (thisCard.Value < currentHighestCard.Value) thisCard = findLowestInSuit(thisCard.Suit);
                    else
                    {
                        // Our card has higher value than the current highest, so use it.
                    }
                } // The current highest card is in a suit we can't play (trumps) so throw away.
                else if (thisCard.Suit != currentHighestCard.Suit)
                {
                    thisCard = findLowestInSuit(thisCard.Suit);
                }
            } // We have the same suit as lead, and it is trumps.
            else if (thisCard != null && thisCard.Suit == trumpSuit)
            {
                // current highest card is also trumps.
                if (thisCard.Suit == currentHighestCard.Suit)
                {
                    // Our highest in this suit is lower than the current highest so throw away.
                    if (thisCard.Value < currentHighestCard.Value) thisCard = findLowestInSuit(thisCard.Suit);
                    else
                    {
                        // Our highest is higher, so use it.
                    }
                }
            } // We don't have a card in the suit that was lead, and currently no trumps have been played.
            else if (thisCard == null && currentHighestCard.Suit != trumpSuit)
            {
                thisCard = findTrumpOrThrowAway(trumpSuit);
            } // We don't have a card in the suit that was lead, and the current highest card is a trump.
            else if (thisCard == null && currentHighestCard.Suit == trumpSuit)
            {
                thisCard = findHighTrumpOrThrowAway(trumpSuit, currentHighestCard);
            }

            return thisCard;
        }

        /// <summary>
        /// This is a special case used when we are the last to play, and our partner is already winning the trick.
        /// In this case we just throw away a low card. More advanced AI might choose to throw away a card in another suit
        /// in order to short suit themselves.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="trumpSuit"></param>
        /// <returns></returns>
        private Card findThrowAwayCard(Suit s, Suit trumpSuit)
        {
            Card throwAway = null;
            foreach (Card c in this.Hand)
            {
                if (c.Suit == s)
                {
                    if (throwAway == null) throwAway = c;
                    else if (c.Value < throwAway.Value) throwAway = c;
                }
            }

            // If we've come this far and throwAway is still null, then we don't have any of the suit that was lead.
            // We should find another non trump card to throw out.
            if (throwAway == null)
            {
                foreach (Card c in this.Hand)
                {
                    if (c.Suit != trumpSuit)
                    {
                        if (throwAway == null) throwAway = c;
                        else if (c.Value < throwAway.Value) throwAway = c;
                    }
                }
            }

            // If throwAway is still null, we only have trumps left. Throw out our smallest one.
            if (throwAway == null)
            {
                foreach (Card c in this.Hand)
                {
                    if (throwAway == null) throwAway = c;
                    else if (c.Value < throwAway.Value) throwAway = c;
                }
            }

            return throwAway;
        }

        public bool doBid(Player currentHighestBidder)
        {
            if (currentHighestBidder == this) return true;

            List<Card> spadeCount = new List<Card>();
            Card[] clubCount = new Card[10];
            Card[] diamondCount = new Card[10];
            Card[] heartCount = new Card[10];
            bool hasJoker = false;

            // Check our hand, see if it is worth bidding on.
            if (this.HasBidded != 0)
            {
                foreach (Card c in this.Hand)
                {
                    switch (c.Suit)
                    {
                        case Suit.Spades:
                            spadeCount += 1;
                            break;
                        case Suit.Clubs:
                            clubCount += 1;
                            break;
                        case Suit.Diamonds:
                            diamondCount += 1;
                            break;
                        case Suit.Hearts:
                            heartCount += 1;
                            break;
                        case Suit.Joker:
                            hasJoker = true;
                            break;
                    }
                }
                if (determineWhichSuitToBid(spadeCount, clubCount, diamondCount, heartCount, hasJoker))
                {
                    int amountToBid = 0;
                    switch (this.BiddedSuit)
                    {
                        case Suit.Spades:
                            amountToBid = determineHowHighToBid(this.BiddedSuit, spadeCount, hasJoker);
                            break;
                        case Suit.Clubs:
                            amountToBid = determineHowHighToBid(this.BiddedSuit, clubCount, hasJoker);
                            break;
                        case Suit.Diamonds:
                            amountToBid = determineHowHighToBid(this.BiddedSuit, diamondCount, hasJoker);
                            break;
                        case Suit.Hearts:
                            amountToBid = determineHowHighToBid(this.BiddedSuit, heartCount, hasJoker);
                            break;
                    }
                }else
                {
                    this.HasBidded = 0;
                    return false;
                }
            }
            
            if (currentHighestBidder == null)
            {
                this.HasBidded = 1;
                return true;
            } // Someone else has bidded already, check their bid.
            else
            {
                if (this.BiddedValue > currentHighestBidder.BiddedValue)
                {
                    this.HasBidded = 1;
                    return true;
                }
                else if (this.BiddedValue < currentHighestBidder.BiddedValue)
                {
                    this.HasBidded = 0;
                    return false;
                }
                else
                {
                    if (this.BiddedSuit > currentHighestBidder.BiddedSuit)
                    {
                        this.HasBidded = 1;
                        return true;
                    }
                    else if (this.BiddedSuit < currentHighestBidder.BiddedSuit)
                    {
                        this.HasBidded = 0;
                        return false;
                    }
                    else
                    {
                        this.HasBidded = 0;
                        return false;
                    }
                }
            }
        }

        private bool determineWhichSuitToBid(int spades, int clubs, int diamonds, int hearts, bool hasJoker)
        {
            int[] suits = new int[4];
            suits[0] = spades;
            suits[1] = clubs;
            suits[2] = diamonds;
            suits[3] = hearts;
            int highest = 0;
            Suit suitToBid = Suit.Joker;

            for (int i = 0; i < 4; i++)
            {
                if (suits[i] > highest)
                {
                    highest = suits[i];
                    switch (i)
                    {
                        case 0:
                            suitToBid = Suit.Spades;
                            break;
                        case 1:
                            suitToBid = Suit.Clubs;
                            break;
                        case 2:
                            suitToBid = Suit.Diamonds;
                            break;
                        case 3:
                            suitToBid = Suit.Hearts;
                            break;
                    }
                } 
            }

            if (highest > 4)
            {
                this.BiddedSuit = suitToBid;
                return true;
            }
            else if (highest > 3 && hasJoker)
            {
                this.BiddedSuit = suitToBid;
                return true;
            }

            return false;
        }

        private int determineHowHighToBid(Suit s, int amount, bool hasJoker)
        {
            // Add the card values together to get a total?
            // Figure out if we have relevant Jacks.
            this.BiddedValue = Value.Six;
            return 6;
        }

        public void pickKittyCards(List<Card> kitty)
        {
            // todo: have AI pick kitty cards.
        }

        public void resetKnowledge()
        {
            this.cardsPlayedSoFar = new List<Card>();
        }

        public List<Card> CardsPlayedSoFar
        {
            get { return this.cardsPlayedSoFar; }
        }
    }
}
