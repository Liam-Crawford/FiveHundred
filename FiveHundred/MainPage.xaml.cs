using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace FiveHundred
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Player[] players = new Player[4];
        private Deck deck;
        private List<Card> kitty = new List<Card>();
        private List<Card> cardsInPlay = new List<Card>();

        private Image backCover = new Image();
        private Image[] playerHandImages = new Image[10];

        private bool isKittyShown = false;
        private bool isPlayersTurn = false;
        private bool didBidderMakeCall = false;
        private bool isInvalidBid;
        private string winningTeam;

        private Card currentlySelectedCard = null;
        private Card currentlySelectedKittyCard = null;
        private Image currentlySelectedImage = null;
        private Image currentlySelectedKittyImage = null;

        private Player currentDealer;
        private Player currentHighestBidder;
        private Player winningCurrentTrick;
        private Card currentHighestCard;
        private Suit trumpSuit;
        private int trumpValue;

        public MainPage()
        {
            this.InitializeComponent();
            setUpGame();        
        }

        public void setUpGame()
        {
            // Initialise the variables we need to start the game.
            backCover.Source = new BitmapImage(new Uri("ms-appx:///Playing Cards/Back Covers/Emerald.png"));
            makeDeck();                 // Make the deck to use in this game.
            createPlayers();            // Create the 4 players, 3 of which are AI.
            dealHands();                // Deal 10 cards to each player, and 3 to the kitty.
            setUpPlayerHandImages();    // Makes an array of Image objects.
            showPlayerHand();           // Assign the images corresponding to the cards in the Players hand to the UI.
            setupKitty();               // Gives the 3 kitty cards a back cover image.
            setupBiddingOptions();      // Creates the bidding options for the UI ComboBoxes.
            setTeamPoints();            // Ensure the points in the top left corner are set to 0 as this is the start of a game.
            currentDealer = players[0]; // Start with the North player for dealing.
            currentDealerBlock.Text = ("Current dealer: " + currentDealer.Name);

            // Once everything is setup, we start the bidding phase.
            biddingPhase();
        }

        private void newGame()
        {
            Debug.WriteLine("New Game started");
            currentDealer = players[3];
            foreach (Player p in players)
            {
                p.Points = 0;
            }
            setTeamPoints();

            playNewRound();
        }

        private void playNewRound()
        {
            northBid.Visibility = Visibility.Collapsed;
            northBid.Text = "";
            eastBid.Visibility = Visibility.Collapsed;
            eastBid.Text = "";
            southBid.Visibility = Visibility.Collapsed;
            southBid.Text = "";
            westBid.Visibility = Visibility.Collapsed;
            westBid.Text = "";
            currentCallBlock.Visibility = Visibility.Collapsed;
            bidSuitOptions.SelectedIndex = 0;
            bidValueOptions.SelectedIndex = 0;
            team1TricksBlock.Text = "Team 1 tricks: 0";
            team2TricksBlock.Text = "Team 2 tricks: 0";
            foreach (Player p in players)
            {
                p.resetHand();
                p.HasBidded = -1;
                p.TricksWon = 0;
                p.HasLeadTrumps = false;
                if (p.Name.Equals("North") || p.Name.Equals("East") || p.Name.Equals("West"))
                {
                    PlayerAI ai = (PlayerAI)p;
                    ai.CardsPlayedSoFar.Clear();
                }
            }
            makeDeck();
            dealHands();
            showPlayerHand();
            foreach (Image c in playerHandImages)
            {
                c.Visibility = Visibility.Visible;
            }
            setupKitty();

            determineDealer();
            currentDealerBlock.Text = ("Current dealer: " + currentDealer.Name);

            currentlySelectedCard = null;
            currentlySelectedImage = null;
            currentlySelectedKittyCard = null;
            currentlySelectedKittyImage = null;

            biddingPhase();
        }

        private void determineDealer()
        {
            switch (currentDealer.Name)
            {
                case "North":
                    currentDealer = players[1];
                    break;
                case "East":
                    currentDealer = players[2];
                    break;
                case "South":
                    currentDealer = players[3];
                    break;
                case "West":
                    currentDealer = players[0];
                    break;
            }
        }

        // Create the suits and values for the bidding comboboxes.
        private void setupBiddingOptions()
        {
            bidSuitOptions.Items.Add("Spades");
            bidSuitOptions.Items.Add("Clubs");
            bidSuitOptions.Items.Add("Diamonds");
            bidSuitOptions.Items.Add("Hearts");
            bidSuitOptions.Items.Add("No Trumps");
            bidSuitOptions.SelectedIndex = 0;

            bidValueOptions.Items.Add(6);
            bidValueOptions.Items.Add(7);
            bidValueOptions.Items.Add(8);
            bidValueOptions.Items.Add(9);
            bidValueOptions.Items.Add(10);
            bidValueOptions.SelectedIndex = 0;
        }

        private void createPlayers()
        {
            players[0] = new PlayerAI("North");
            players[1] = new PlayerAI("East");
            players[2] = new Player("South");
            players[3] = new PlayerAI("West");

            players[0].Partner = players[2];
            players[1].Partner = players[3];
            players[2].Partner = players[0];
            players[3].Partner = players[1];
        }

        private Player[] definePlayerOrder(string name)
        {
            Player[] playerOrder = new Player[4];
            switch (name) {
                case "North":
                    playerOrder[0] = players[0];
                    playerOrder[1] = players[1];
                    playerOrder[2] = players[2];
                    playerOrder[3] = players[3];
                    break;
                case "East":
                    playerOrder[0] = players[1];
                    playerOrder[1] = players[2];
                    playerOrder[2] = players[3];
                    playerOrder[3] = players[0];
                    break;
                case "South":
                    playerOrder[0] = players[2];
                    playerOrder[1] = players[3];
                    playerOrder[2] = players[0];
                    playerOrder[3] = players[1];
                    break;
                case "West":
                    playerOrder[0] = players[3];
                    playerOrder[1] = players[0];
                    playerOrder[2] = players[1];
                    playerOrder[3] = players[2];
                    break;
            }
            return playerOrder;
        }

        // Deals to all the players and the kitty.
        private void dealHands()
        {
            foreach (Player p in players)
            {
                p.resetHand();
            }

            for (int i = 0; i < 10; i++)
            {
                foreach (Player p in players)
                {
                    p.Hand.Add(deck.dealFromDeck());
                }
            }
            kitty.Clear();
            for (int i = 0; i < 3; i++)
            {
                kitty.Add(deck.dealFromDeck());
            }
        }

        // Makes a new deck and sets up for playing 500.
        private void makeDeck()
        {
            deck = new Deck();
            deck.setUpFor500();
        }

        // Creates an array of the image holders for the Players Hand. 
        private void setUpPlayerHandImages()
        {
            playerHandImages[0] = card1;
            playerHandImages[1] = card2;
            playerHandImages[2] = card3;
            playerHandImages[3] = card4;
            playerHandImages[4] = card5;
            playerHandImages[5] = card6;
            playerHandImages[6] = card7;
            playerHandImages[7] = card8;
            playerHandImages[8] = card9;
            playerHandImages[9] = card10;
        }

        // Assigns the image of each card the player has to the image holders (playerHandGrid).
        private void showPlayerHand()
        {
            // Todo.. Better logic for displaying cards
            players[2].Hand.Sort();
            for (int i = 0; i < players[2].Hand.Count; i++)
            {
                playerHandImages[i].Source = players[2].Hand.ElementAt(i).ImgSource;
            }
            for (int i = players[2].Hand.Count; i < 10; i++)
            {
                playerHandImages[i].Visibility = Visibility.Collapsed;
            }

        }

        // Just gives the kitty cards the back cover image.
        private void setupKitty()
        {
            cardK1.Source = backCover.Source;
            cardK2.Source = backCover.Source;
            cardK3.Source = backCover.Source;
            kittyGrid.Visibility = Visibility.Visible;
        }
        
        // Shows the actual kitty cards.
        private void showKittyCards()
        {
            cardK1.Source = kitty.ElementAt(0).ImgSource;
            cardK2.Source = kitty.ElementAt(1).ImgSource;
            cardK3.Source = kitty.ElementAt(2).ImgSource;
            isKittyShown = true;
            acceptKittyCardsButton.Visibility = Visibility.Visible;
        }

        private void setBidText(string t, Player p)
        {
            switch (p.Name)
            {
                case "North":
                    setBidParameters(northBid, t);
                    break;
                case "East":
                    setBidParameters(eastBid, t);
                    break;
                case "South":
                    setBidParameters(southBid, t);
                    break;
                case "West":
                    setBidParameters(westBid, t);
                    break;
            }
        }

        private void setBidParameters(TextBlock x, string t)
        {
            x.Text = t;
            if (x.Visibility != Visibility.Visible) x.Visibility = Visibility.Visible;
        }

        private string findBidder()
        {
            switch (currentDealer.Name)
            {
                case ("North"):
                    return "East";
                case ("East"):
                    return "South";
                case ("South"):
                    return "West";
                case ("West"):
                    return "North";
            }

            // Default case, should never have to execute.
            return "North";
        }

        /// <summary>
        /// Logic for the bidding phase.
        /// </summary>
        private async void biddingPhase()
        {
            currentHighestBidder = null;
            Player[] playerOrder = definePlayerOrder(findBidder());
            while (!isBiddingOver())
            {
                foreach (Player p in playerOrder)
                {
                    if (p.Name.Equals("South"))
                    {
                        if (p.HasBidded != 0)
                        {
                            if (currentHighestBidder != players[2])
                            {
                                Debug.WriteLine("Player bid");
                                minimumNextBid();

                                bidButton.Visibility = Visibility.Visible;
                                passButton.Visibility = Visibility.Visible;

                                isInvalidBid = true;
                                while(isInvalidBid) await Task.WhenAny(whenClicked(passButton), whenClicked(acceptBidButton));
                            }
                        }
                    }
                    else
                    {
                        PlayerAI ai = (PlayerAI)p;
                        if (ai.HasBidded == 0) { }
                        else if (ai.doBid(currentHighestBidder))
                        {
                            currentHighestBidder = ai;
                            string s;

                            if (ai.BiddedSuit == Suit.Joker)
                            {
                                s = "No Trumps";
                                setBidText(string.Format("Current Bid {0} {1}", (int)ai.BiddedValue, s), ai);
                            }
                            else setBidText(string.Format("Current Bid {0} {1}", (int)ai.BiddedValue, ai.BiddedSuit), ai);
                        }
                        else
                        {
                            setBidText("Pass", ai);
                            ai.HasBidded = 0;
                        }
                    }
                    if (!p.Name.Equals("East") || p.HasBidded != 0) await Task.Delay(500);
                } // End of foreach loop.
            } // End of While loop.

            startGame();
        }

        /// <summary>
        /// Returns true if the bidding is over, and performs some tasks
        /// related to the winning bid.
        /// </summary>
        /// <returns></returns>
        private bool isBiddingOver()
        {
            // Check if 3 players have passed, meaning 1 player has won the bid.
            int count = 0;
            foreach (Player p in players)
            {
                if (p.HasBidded == 0) count++;
                else if (p.HasBidded == -1) count = 0;
            }
            Debug.WriteLine(string.Format("Bid Count: {0}", count));
            if (count == 3)
            {
                // Find the player with the winning bid
                foreach (Player p in players)
                {
                    if (p.HasBidded == 1)
                    {
                        trumpSuit = p.BiddedSuit;
                        trumpValue = (int)p.BiddedValue;

                        currentCallBlock.Text = string.Format("{0}: {1} {2}", p.Name, trumpValue, trumpSuit);
                        currentCallBlock.Visibility = Visibility.Visible;

                        // If the winning bid is from the human player, do the kitty phase.
                        if (p.Name.Equals("South")) showKittyCards();
                        else
                        {
                            // Otherwise have the AI process the kitty.
                            PlayerAI ai = (PlayerAI)p;
                            ai.pickKittyCards(this.kitty);
                        }

                        return true;
                    }
                }
            } // If all 4 players pass, start again.
            else if (count == 4)
            {
                resetBiddingPhase();
                count = 0;
            }
            return false;
        }

        private void resetBiddingPhase()
        {
            northBid.Visibility = Visibility.Collapsed;
            northBid.Text = "";
            eastBid.Visibility = Visibility.Collapsed;
            eastBid.Text = "";
            southBid.Visibility = Visibility.Collapsed;
            southBid.Text = "";
            westBid.Visibility = Visibility.Collapsed;
            westBid.Text = "";
            currentCallBlock.Visibility = Visibility.Collapsed;
            bidSuitOptions.SelectedIndex = 0;
            bidValueOptions.SelectedIndex = 0;
            foreach (Player p in players)
            {
                p.resetHand();
                p.HasBidded = -1;
            }
            makeDeck();
            dealHands();
            showPlayerHand();
            foreach (Image c in playerHandImages)
            {
                c.Visibility = Visibility.Visible;
            }
            setupKitty();

            currentDealerBlock.Text = ("Current dealer: " + currentDealer.Name);
            currentlySelectedCard = null;
            currentlySelectedImage = null;
            currentlySelectedKittyCard = null;
            currentlySelectedKittyImage = null;
        }

        private void minimumNextBid()
        {
            if (currentHighestBidder != null)
            {
                Suit s = currentHighestBidder.BiddedSuit;
                Value v = currentHighestBidder.BiddedValue;
                string ss = "";

                if (s < Suit.Joker)
                {
                    s = s + 1;
                    if (s == Suit.Joker)
                    {
                        ss = "No Trumps";
                    }
                    else ss = s.ToString();
                }
                else if (s == Suit.Joker) v = v + 1;

                setBidText(string.Format("Minimum next bid is {0} {1}", (int)v, ss), players[2]);
            }
            else setBidText(string.Format("Minimum next bid is 6 Spades"), players[2]);
        }

        /// <summary>
        /// Logic for playing each trick in the game. 
        /// </summary>
        private async void startGame()
        {
            if (isKittyShown)
            {
                while (isKittyShown)
                {
                    await Task.WhenAny(whenClicked(acceptKittyCardsButton));
                }
            }
            else
            {
                isKittyShown = false;

                kittyGrid.Visibility = Visibility.Collapsed;
                acceptKittyCardsButton.Visibility = Visibility.Collapsed;
            }
            nextTrickButton.Content = "Next Trick";

            // Need to define the player order based on who won the bid. We can use 'currentHighestBidder'
            // for this as once this method is called, it will contain a reference to the winning player.
            Player[] playerOrder = definePlayerOrder(currentHighestBidder.Name);
            redefineForTrumps();
            showPlayerHand();
            setTrickAreaVisibility(Visibility.Visible);

            for (int thisTrick = 1; thisTrick < 11; thisTrick++)
            {

                foreach (Player p in playerOrder)
                {
                    if (p.Name.Equals("South"))
                    {
                        // Do player turn
                        isPlayersTurn = true;

                        // Wait for the player to click a valid card.
                        while (isPlayersTurn)
                        {
                            await Task.WhenAny(whenImageClicked(card1), whenImageClicked(card2), whenImageClicked(card3), whenImageClicked(card4), whenImageClicked(card5),
                            whenImageClicked(card6), whenImageClicked(card7), whenImageClicked(card8), whenImageClicked(card9), whenImageClicked(card10));
                        }
                        
                        this.trickCardSouth.Source = currentlySelectedCard.ImgSource;
                    }
                    else
                    {
                        // Do AI turn.
                        PlayerAI ai = (PlayerAI)p;
                        Card c = ai.processTurn(this.cardsInPlay, currentHighestBidder, winningCurrentTrick, trumpSuit, currentHighestCard);
                        if (isCurrentHighestCard(c))
                        {
                            winningCurrentTrick = ai;
                            currentHighestCard = c;
                        }
                        this.cardsInPlay.Add(c);
                        setAITrickCard(ai, c);
                    }
                    if (cardsInPlay.Count < 4) await Task.Delay(500);

                }
                playerOrder = definePlayerOrder(determineTrickWinner().Name);
                team1TricksBlock.Text = ("Team 1 tricks: " + (players[0].TricksWon + players[2].TricksWon).ToString());
                team2TricksBlock.Text = ("Team 2 tricks: " + (players[1].TricksWon + players[3].TricksWon).ToString());
                if (thisTrick == 10) nextTrickButton.Content = "Finish Round";
                nextTrickButton.Visibility = Visibility.Visible;
                addCardKnowledge();
                this.cardsInPlay.Clear();
                this.winningCurrentTrick = null;
                this.currentHighestCard = null;
                await Task.WhenAny(whenClicked(nextTrickButton));
            }
            currentCallBlock.Visibility = Visibility.Collapsed;

            if (!addWinningPoints()) playNewRound();
            else
            {
                finishGame();
                await Task.WhenAny(whenClicked(newGameButton), whenClicked(quitButton));
                setGameVisibility(Visibility.Visible);
                newGame();
            }

        }

        private void addCardKnowledge()
        {
            PlayerAI ai1 = (PlayerAI)players[0];
            PlayerAI ai2 = (PlayerAI)players[1];
            PlayerAI ai3 = (PlayerAI)players[3];
            foreach (Card c in cardsInPlay)
            {
                ai1.CardsPlayedSoFar.Add(c);
                ai2.CardsPlayedSoFar.Add(c);
                ai3.CardsPlayedSoFar.Add(c);
            }
        }

        private bool isCurrentHighestCard(Card thisCard)
        {
            if (cardsInPlay.Count == 0) return true;
            bool thisCardisHighest = true;
            foreach (Card c in cardsInPlay)
            {
                if (c.Suit == this.trumpSuit && thisCard.Suit != this.trumpSuit) thisCardisHighest = false;
                else if (c.Suit != this.trumpSuit && thisCard.Suit == this.trumpSuit) thisCardisHighest = true;
                else if (c.Suit == this.trumpSuit && thisCard.Suit == trumpSuit)
                {
                    if (c.Value > thisCard.Value) thisCardisHighest = false;
                    else thisCardisHighest = true;
                }
                else if (c.Suit == thisCard.Suit)
                {
                    if (c.Value > thisCard.Value) thisCardisHighest = false;
                    else thisCardisHighest = true;
                }
            }

            return thisCardisHighest;
        }

        private Player determineTrickWinner()
        {
            Card highestCard = cardsInPlay[0];
            for (int i = 1; i < 4; i++)
            {
                if (cardsInPlay[i].Suit == highestCard.Suit)
                {
                    if (cardsInPlay[i].Value > highestCard.Value) highestCard = cardsInPlay[i];
                }
                else if (cardsInPlay[i].Suit == this.trumpSuit && highestCard.Suit != this.trumpSuit)
                    highestCard = cardsInPlay[i];
            }
            if (highestCard.ImgSource == trickCardNorth.Source)
            {
                return applyWinningTrick(players[0]);
            }
            else if (highestCard.ImgSource == trickCardEast.Source)
            {
                return applyWinningTrick(players[1]);
            }
            else if (highestCard.ImgSource == trickCardSouth.Source)
            {
                return applyWinningTrick(players[2]);
            }
            else if (highestCard.ImgSource == trickCardWest.Source)
            {
                return applyWinningTrick(players[3]);
            }

            return players[0];
        }

        private Player applyWinningTrick(Player p)
        {
            p.TricksWon += 1;
            return p;
        }

        private bool addWinningPoints()
        {
            int points = 0;
            switch (this.trumpSuit)
            {
                case Suit.Spades:
                    points = 40 + determinePoints();
                    break;
                case Suit.Clubs:
                    points = 60 + determinePoints();
                    break;
                case Suit.Diamonds:
                    points = 80 + determinePoints();
                    break;
                case Suit.Hearts:
                    points = 100 + determinePoints();
                    break;
                case Suit.Joker:
                    points = 120 + determinePoints();
                    break;
            }
            if (hasEnoughTricks())
            {
                currentHighestBidder.Points += points;
                didBidderMakeCall = true;
            }
            else
            {
                currentHighestBidder.Points -= points;
                didBidderMakeCall = false;
            }

            if (setTeamPoints()) return true;
            else return false;
        }

        private bool hasEnoughTricks()
        {
            int tricks = 0;
            switch (currentHighestBidder.Name)
            {
                case "North":
                    tricks += determineTrickPoints(players[0], players[2], players[1], players[3]);
                    break;
                case "East":
                    tricks += determineTrickPoints(players[1], players[3], players[0], players[2]);
                    break;
                case "South":
                    tricks += determineTrickPoints(players[0], players[2], players[1], players[3]);
                    break;
                case "West":
                    tricks += determineTrickPoints(players[1], players[3], players[0], players[2]);
                    break;
            }
            if (tricks >= this.trumpValue) return true;
            return false;
        }

        private int determineTrickPoints(Player t1, Player t2, Player p1, Player p2)
        {
            int tricks = 0;
            tricks = t1.TricksWon + t2.TricksWon;
            p1.assignPoints();
            p2.assignPoints();
            return tricks;
        }

        private bool setTeamPoints()
        {
            int team1Points = 0;
            int team2Points = 0;

            foreach (Player p in players)
            {
                switch (p.Name)
                {
                    case "North":
                        team1Points += p.Points;
                        break;
                    case "East":
                        team2Points += p.Points;
                        break;
                    case "South":
                        team1Points += p.Points;
                        break;
                    case "West":
                        team2Points += p.Points;
                        break;
                }
            }

            team1PointsBlock.Text = team1Points.ToString();
            team2PointsBlock.Text = team2Points.ToString();
            if (team1Points >= 500)
            {
                if ((currentHighestBidder.Name.Equals("North") || currentHighestBidder.Name.Equals("South")) && didBidderMakeCall)
                {
                    winningTeam = "Team 1";
                    return true;
                }
            }
            if (team2Points >= 500)
            {
                if ((currentHighestBidder.Name.Equals("East") || currentHighestBidder.Name.Equals("West")) && didBidderMakeCall)
                {
                    winningTeam = "Team 2";
                    return true;
                }
            }
            return false;
        }

        private int determinePoints()
        {
            int points = 0;
            switch (this.trumpValue)
            {
                case 7:
                    points = 100;
                    break;
                case 8:
                    points = 200;
                    break;
                case 9:
                    points = 300;
                    break;
                case 10:
                    points = 400;
                    break;
            }

            return points;
        }

        private void redefineForTrumps()
        {
            Suit offSuit = Suit.Joker;
            switch (trumpSuit)
            {
                case Suit.Spades:
                    offSuit = Suit.Clubs;
                    break;
                case Suit.Clubs:
                    offSuit = Suit.Spades;
                    break;
                case Suit.Diamonds:
                    offSuit = Suit.Hearts;
                    break;
                case Suit.Hearts:
                    offSuit = Suit.Diamonds;
                    break;
            }
            foreach (Player p in players)
            {
                foreach (Card c in p.Hand)
                {
                    if (c.Suit == Suit.Joker)
                    {
                        c.Suit = trumpSuit;
                        c.Value = Value.Joker;
                    }
                    else if (c.Value == Value.Jack && c.Suit == trumpSuit) c.Value = Value.TrumpJack;
                    else if (c.Value == Value.Jack && c.Suit == offSuit)
                    {
                        c.Suit = trumpSuit;
                        c.Value = Value.OffJack;
                    }
                }
            }
        }

        private void setTrickAreaVisibility(Visibility v)
        {
            trickCardNorth.Visibility = v;
            trickCardEast.Visibility = v;
            trickCardSouth.Visibility = v;
            trickCardWest.Visibility = v;
            team1TricksBlock.Visibility = v;
            team2TricksBlock.Visibility = v;
        }

        private void setAITrickCard(PlayerAI ai, Card c)
        {
            switch (ai.Name)
            {
                case "North":
                    trickCardNorth.Source = c.ImgSource;
                    break;
                case "East":
                    trickCardEast.Source = c.ImgSource;
                    break;
                case "West":
                    trickCardWest.Source = c.ImgSource;
                    break;
            }
        }

        private void finishGame()
        {
            setGameVisibility(Visibility.Collapsed);

            gameWinnerBlock.Text = (winningTeam + " WINS!");
            finishGameGrid.Visibility = Visibility.Visible;
        }

        private void setGameVisibility(Visibility v)
        {
            currentDealerBlock.Visibility = v;
            pointsGrid.Visibility = v;
            northGrid.Visibility = v;
            EastGrid.Visibility = v;
            southGrid.Visibility = v;
            westGrid.Visibility = v;
            playerHandGrid.Visibility = v;
            trumpsGrid.Visibility = v;
        }

        //--------------------------//--------------------------//--------------------------//
        //---Click Handlers below---//---Click Handlers below---//---Click Handlers below---//
        //--------------------------//--------------------------//--------------------------//

        private void passButton_Click(object sender, RoutedEventArgs e)
        {
            bidButton.Visibility = Visibility.Collapsed;
            passButton.Visibility = Visibility.Collapsed;
            isInvalidBid = false;
            southBid.Text = "Pass";

            players[2].HasBidded = 0;
        }

        /// <summary>
        /// Shows bidding grid.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bidButton_Click(object sender, RoutedEventArgs e)
        {
            bidButton.Visibility = Visibility.Collapsed;
            passButton.Visibility = Visibility.Collapsed;

            chooseBidGrid.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Cancels bidding and collapses bidding grid.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cancelBidButton_Click(object sender, RoutedEventArgs e)
        {
            chooseBidGrid.Visibility = Visibility.Collapsed;

            bidButton.Visibility = Visibility.Visible;
            passButton.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// What happens when bid is accepted.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void acceptBidButton_Click(object sender, RoutedEventArgs e)
        {
            // Check that some options have actually been selected, and pull them into this method.
            if (bidSuitOptions.SelectionBoxItem == null || bidValueOptions.SelectionBoxItem == null) return;
            string biddedSuit = (string)bidSuitOptions.SelectionBoxItem;
            Value biddedValue = (Value)bidValueOptions.SelectionBoxItem;

            // Parse the suit.
            Suit s = Suit.Joker;
            switch (biddedSuit)
            {
                case "Spades":
                    s = Suit.Spades;
                    break;
                case "Clubs":
                    s = Suit.Clubs;
                    break;
                case "Diamonds":
                    s = Suit.Diamonds;
                    break;
                case "Hearts":
                    s = Suit.Hearts;
                    break;
            }

            // Check that the bid is valid, notify player if it isn't.
            if (!players[2].isValidBid(s, biddedValue, currentHighestBidder))
            {
                setBidText("Invalid Bid", players[2]);
                return;
            }
            isInvalidBid = false;
            
            // Set variables related to accepting a valid bid.
            players[2].BiddedSuit = s;
            players[2].BiddedValue = biddedValue;
            players[2].HasBidded = 1;
            currentHighestBidder = players[2];
            setBidText(string.Format("Current Bid {0} {1}", (int)biddedValue, biddedSuit), players[2]);
            chooseBidGrid.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// The player has selected the cards they want from the kitty and are now ready to start the game.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void acceptKittyCardsButton_Click(object sender, RoutedEventArgs e)
        {
            isKittyShown = false;

            kittyGrid.Visibility = Visibility.Collapsed;
            acceptKittyCardsButton.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Logic for determining what happens when a player clicks a card in their hand.
        /// Calls cardClickHelper if the kitty is shown, otherwise plays a card for the 
        /// current trick.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void playerCardClicked(object sender, RoutedEventArgs e)
        {
            // Logic applying to Bidding phase.
            if (isKittyShown)
            {
                cardClickHelper(sender, ref currentlySelectedKittyCard, ref currentlySelectedCard, ref currentlySelectedKittyImage, ref currentlySelectedImage);
            }
            // Logic applying to Trick phase.
            else if (isPlayersTurn)
            {
                Image thisImage = (Image)sender;
                Card thisCard = deck.ImageLinks[thisImage.Source];

                if (players[2].isLegalMove(cardsInPlay, thisCard))
                {
                    if (isCurrentHighestCard(thisCard))
                    {
                        winningCurrentTrick = players[2];
                        currentHighestCard = thisCard;
                    }
                    this.cardsInPlay.Add(thisCard);
                    players[2].Hand.Remove(thisCard);
                    this.currentlySelectedCard = thisCard;

                    isPlayersTurn = false;

                    showPlayerHand();
                }
                else return;
            }
            return;
        }

        /// <summary>
        /// Checks if the kitty is shown, calls cardClickHelper if it is with the correct params.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void kittyCardClicked(object sender, RoutedEventArgs e)
        {
            // If the player can't actually see the cards in the kitty, do nothing.
            if (!isKittyShown) return;

            cardClickHelper(sender, ref currentlySelectedCard, ref currentlySelectedKittyCard, ref currentlySelectedImage, ref currentlySelectedKittyImage);
        }

        /// <summary>
        /// Logic for what to do when a card is clicked, the steps are largely the same
        /// for a card in the players hand or the kitty, only minor differences.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="otherCard"></param>
        /// <param name="thisOtherCard"></param>
        /// <param name="otherImage"></param>
        /// <param name="thisOtherImage"></param>
        private void cardClickHelper(object sender, ref Card otherCard, ref Card thisOtherCard, ref Image otherImage, ref Image thisOtherImage)
        {
            Card thisCard;
            Border b;

            Image thisImage = (Image)sender;
            thisCard = deck.ImageLinks[thisImage.Source];

            // If a card from the other hand is selected
            if (otherCard != null)
            {
                // Swap cards, different depending on which hand was selected.
                if (otherCard == currentlySelectedCard)
                {
                    kitty.Insert(kitty.IndexOf(thisCard), otherCard);
                    kitty.Remove(thisCard);
                    players[2].Hand.Insert(players[2].Hand.IndexOf(otherCard), thisCard);
                    players[2].Hand.Remove(otherCard);
                }
                else if (otherCard == currentlySelectedKittyCard)
                {
                    players[2].Hand.Insert(players[2].Hand.IndexOf(thisCard), otherCard);
                    players[2].Hand.Remove(thisCard);
                    kitty.Insert(kitty.IndexOf(otherCard), thisCard);
                    kitty.Remove(otherCard);
                }

                // Set the border of the other card to nothing.
                b = (Border)otherImage.Parent;
                b.BorderThickness = new Thickness(0);

                // Reset all currently selected cards.
                currentlySelectedKittyCard = null;
                currentlySelectedKittyImage = null;
                currentlySelectedCard = null;
                currentlySelectedImage = null;

                showKittyCards();
                showPlayerHand();

                return;
            }

            // No card in the other hand is selected, but is one in the current hand already selected?
            if (thisCard != thisOtherCard && thisOtherCard != null)
            {
                // If so, remove the border from the other card in this hand.
                b = (Border)thisOtherImage.Parent;
                b.BorderThickness = new Thickness(0);
            }

            // Create a new border for this card.
            b = (Border)thisImage.Parent;
            Thickness currentThickness = b.BorderThickness;
            Thickness newThickness = new Thickness(4);

            // Check if this card already has a border.
            if (currentThickness != newThickness)
            {
                // If not, apply the border to indicate it is now selected.
                b.BorderThickness = newThickness;
                thisOtherCard = thisCard;
                thisOtherImage = thisImage;
            }
            else
            {
                // If so, remove the border and reset the selected card relating to this hand.
                b.BorderThickness = new Thickness(0);
                thisOtherCard = null;
                thisOtherImage = null;
            }
        }

        /// <summary>
        /// I don't fully understand what is happening here, the code came from:
        /// http://stackoverflow.com/questions/21835667/how-do-i-pause-a-loop-in-c-sharp-to-wait-for-a-user-interaction
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        private Task whenClicked(Button button)
        {
            var tcs = new TaskCompletionSource<bool>();
            RoutedEventHandler handler = null;
            handler = (s, e) =>
            {
                tcs.TrySetResult(true);
                button.Click -= handler;
            };

            button.Click += handler;

            return tcs.Task;
        }

        private Task whenImageClicked(Image image)
        {
            var tcs = new TaskCompletionSource<bool>();
            PointerEventHandler handler = null;
            handler = (s, e) =>
            {
                tcs.TrySetResult(true);
                image.PointerPressed -= handler;
            };

            image.PointerPressed += handler;

            return tcs.Task;
        }

        private void nextTrickButton_Click(object sender, RoutedEventArgs e)
        {
            trickCardNorth.Source = null;
            trickCardEast.Source = null;
            trickCardSouth.Source = null;
            trickCardWest.Source = null;

            nextTrickButton.Visibility = Visibility.Collapsed;
        }

        private void newGameButton_Click(object sender, RoutedEventArgs e)
        {
            finishGameGrid.Visibility = Visibility.Collapsed;
        }

        private void quitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
        }
    }
}
