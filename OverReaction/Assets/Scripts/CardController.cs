using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardController : MonoBehaviour
{
    public static AbstractCardController Controller { get; private set; }
    public bool IsInitialized { get; private set; }
    public GameSparksManager SparksManager;

    private double LastTime;

    private void Start()
    {
        IsInitialized = false;
        Controller = null;
        LastTime = Time.time;
        Init(GameSparksManager.instance.iAmHost);
    }

    public void Init(bool IsHost)
    {
        if(IsHost)
        {
            Controller = new HostCardController();

        } else
        {
            Controller = new ClientCardController();
        }
        SparksManager = GameSparksManager.instance;
        Controller.SparksManager = SparksManager;
        Controller.Start();
        IsInitialized = true;
    }

    public void Update()
    {
        double TimeElapsed = Time.time - LastTime;
        LastTime = Time.time;
        Controller.Update(TimeElapsed);
    }

    public int GetNumberOfCards()
    {
        return Controller.GetNumberOfCards();
    }

    public string GetCardInfo(int ID)
    {
        return Controller.GetCardInfo(ID);
    }

    public void PlayCard(int index) {
        Controller.PlayCard(index);
    }

    public void PlayCard(string peerID, int ID)
    {
        Controller.PlayCard(peerID, ID);
    }

    public virtual void AddEffect(Effect effect)
    {
        Controller.AddEffect(effect);
    }

    public IControllerExtension GetExtension(System.Type type)
    {
        return Controller.GetExtension(type);
    }
}

public abstract class AbstractCardController
{
    public GameSparksManager SparksManager;

    public abstract void Start();
    public abstract int GetNumberOfCards();
    public abstract string GetCardInfo(int ID);
    public abstract void PlayCard(int ID);
    public abstract void PlayCard(string peerID, int index);
    public abstract void AddEffect(Effect effect);
    public abstract void Update(double TimeElapsed);
    public abstract void setDeck(Dictionary<int, string> newDeck);
    public virtual IControllerExtension GetExtension(System.Type type)
    {
        Debug.LogWarning("Tried to get a ControllerExtension from an invalid CardController");
        return null;
    }
}

public class HostCardController : AbstractCardController {
    // The PreFab card to initialize cards with. Passed to Factory
    public GameObject BaseCard;
    // The card factory used to create cards
    public CardParser Factory;

    // TEMP REPRESENTATION SO WE DONT HAVE TO USE FILES
    private string deckFileFormatted = "1,1,1,20,5,1,0,1,0,0,0,0,0,1,1,\n" +
        "1,1,1,20,5,0,1,0,0,1,0,0,1,1,0,\n" +
        "1,1,1,20,5,1,1,0,0,0,0,0,0,2,0,\n" +
        "1,1,1,20,5,0,0,1,1,0,1,1,0,0,0,\n" +
        "1,1,1,20,5,1,1,0,0,0,0,0,1,0,1,\n" +
        "1,1,1,20,5,0,0,0,1,1,1,0,1,0,0,\n" +
        "1,1,1,20,5,0,0,1,1,0,0,1,0,0,1,\n" +
        "1,1,1,20,5,0,1,0,1,0,1,0,1,0,0,\n" +
        "1,1,1,20,5,0,0,0,1,1,0,1,1,0,0,\n" +
        "1,1,1,20,5,0,1,1,0,0,0,0,0,1,1,\n" +
        "1,1,1,20,5,0,0,0,1,1,1,1,0,0,0,\n" +
        "1,1,1,20,5,0,1,1,1,0,1,0,0,0,2,\n" +
        "1,1,1,20,5,0,0,1,1,1,3,0,0,0,0,\n" +
        "1,1,1,20,5,0,0,1,1,1,3,0,0,0,0,\n" +
        "3,1,5,0,1,1,\n" +
        "3,1,5,1,2,1,\n" +
        "3,1,10,2,1,2,\n";

    // The simulation for this game
    private Simulation simulation;
    // The list of active effects
    private List<Effect> Effects;
    // The list of effects to remove
    private List<Effect> EffectsToRemove;
    // The list of ControllerExtensions used for added functionality
    private List<IControllerExtension> Extensions;

    public string FileName = "BasicDeck.txt";

    // The path to get to the Decks folder
    private string Path = "..\\OverReaction\\Assets\\Decks\\";
    // Used to get random numbers
    private System.Random Rand;
    // The master list of cards in the game
    private List<Card> Cards;
    // A copy of each player's Deck mapped to thier peer id
    private Dictionary<string, Dictionary<int, Card>> Decks;

	// Use this for initialization
	public override void Start () {
        // Initialize Rand
        Rand = new System.Random();
        // Initialize Factory
        Factory = new CardParser(Rand);
        // Add parsers to the factory
        AddParsers();
        // Parse the cards at the given file
        Cards = Factory.ParseDeck(deckFileFormatted, false);
        // Grab the simulation from the main camera
        simulation = Camera.main.GetComponent<Simulation>();
        // Initialize the Effect List
        Effects = new List<Effect>();
        // Initialize the extensions
        Extensions = new List<IControllerExtension>();
        // Add all the valid extensions
        AddExtensions();

        EffectsToRemove = new List<Effect>();
        if (SparksManager.iAmOnline && SparksManager.iAmHost)
        {
            Debug.Log("generating initial decks for each player");
            Decks = new Dictionary<string, Dictionary<int, Card>>();

            List<RealtimePlayer> Players = SparksManager.getSessionInfo().getPlayers();
            foreach (RealtimePlayer Player in Players)
            {
                Dictionary<int, Card> Deck = new Dictionary<int, Card>();

                List<Card> shuffledCards = shuffle(Cards);

                // Add shuffled cards to their dict
                for (int i = 0; i < shuffledCards.Count; i++) {
                    Deck.Add(i, shuffledCards[i]);
                }

                Decks.Add(Player.getPeerID(), Deck);
            }

            SparksManager.sendInitialDeck(Decks);

            foreach (KeyValuePair<string, Dictionary<int, Card>> Deck in Decks)
            {
                string entry = "Player ID: " + Deck.Key + "\n";
                foreach (KeyValuePair<int, Card> Card in Deck.Value)
                {
                    entry += "Index: " + Card.Key + "\n";
                    entry += "Card: " + Card.Value.ToString();
                }
                Debug.Log(entry);
            }
        }
	}

    /// <summary>
    /// Method used to add parsers to the simulation
    /// </summary>
    public void AddParsers()
    {
        Factory.AddParserOption(new GeneralModifierCardParserOption());
    }

    public void AddExtensions()
    {
        // This is where we add the extensions
        Extensions.Add(new ReactionSelectionExtension());
    }

    public override IControllerExtension GetExtension(System.Type type)
    {
        foreach( IControllerExtension Extension in Extensions)
        {
            if (Extension.GetType() == type) return Extension;
        }
        return null;
    }

    /// <summary>
    /// Used to get the number of cards that can be played
    /// </summary>
    /// <returns>The number of cards in play</returns>
    public override int GetNumberOfCards()
    {
        return Cards.Count;
    }

    public List<Card> GetCards()
    {
        return new List<Card>(Cards);
    }

    /// <summary>
    /// Returns the card at the given index
    /// Returns null if the index is out of bounds
    /// </summary>
    public Card GetCard(int index)
    {
        if (index < 0 || index >= Cards.Count) return null;
        return Decks[SparksManager.getRTSession().PeerId + ""][index];
    }

    /// <summary>
    /// Gets the text to display on a card given the index
    /// Returns nothing if the index is out of bounds
    /// </summary>
    public override string GetCardInfo(int index)
    {
        if (index < 0 || index >= Cards.Count) return "";
        return Cards[index].ToString();
    }

    public override void PlayCard(int index) {
        SparksManager.canPlayCard(index);
    }

    /// <summary>
    /// Plays the card with the given index
    /// Does nothing if the index is out of range
    /// </summary>
    public override void PlayCard(string peerID, int index)
    {
        // This function will be called from the gamesparks manager when the server says we are good to play a card
        Decks[peerID][index].Play(simulation);
    } 

    public override void Update(double timeElapsed)
    {
        UpdateEffects(timeElapsed);
        foreach(IControllerExtension Extension in Extensions)
        {
            Extension.Update(timeElapsed);
        }
    }

    private void UpdateEffects(double timeElapsed)
    {
        foreach(Effect effect in Effects)
        {
            effect.TimeRemaining -= timeElapsed;
            if (effect.TimeRemaining < 0)
            {
                effect.Remove();
                EffectsToRemove.Add(effect);
            }
        }
        foreach(Effect effect in EffectsToRemove)
        {
            Effects.Remove(effect);
        }
        EffectsToRemove.Clear();
    }

    public override void setDeck(Dictionary<int, string> newDeck) {
        // We are host we dont need to do anything
    }

    public override void AddEffect(Effect effect)
    {
        effect.Play();
        Effects.Add(effect);
    }

    // Utility function to map the list of cards in a dict to strings
    private Dictionary<int, string> mapCardDeckToString(Dictionary<int, Card> deck) {
        throw new NotImplementedException();
    }

    // Utility function to shuffle a list and return new shuffled list
    private List<Card> shuffle(List<Card> list) {
        // Seed the random off of time
        System.Random rng = new System.Random(DateTime.Now.Millisecond);

        // Creat copy of passed in list
        List<Card> cards = new List<Card>(list);

        int n = cards.Count;
        while (n > 1) {
            n--;
            int k = rng.Next(n + 1);
            Card value = cards[k];
            cards[k] = cards[n];
            cards[n] = value;
        }
        return cards;
    }
}

public class ClientCardController : AbstractCardController
{
    private Dictionary<int, string> Cards;

    public override void Start()
    {
        // Get the cards from the server
    }

    public override int GetNumberOfCards()
    {
        return Cards.Count;
    }

    public override string GetCardInfo(int ID)
    {
        if (ID < 0 || ID >= Cards.Count) return "Index out of bounds";
        return Cards[ID];
    }

    public override void PlayCard(int ID)
    {
        SparksManager.canPlayCard(ID);
    }

    public override void PlayCard(string peerID, int index) {
      // We do nothing, this function should only be called on the host
    }

    public override void AddEffect(Effect effect)
    {
        Debug.LogWarning("Tried to add an effect on the client side!");
    }

    public override void Update(double TimeElapsed)
    {
        
    }

    public override void setDeck(Dictionary<int, string> newDeck) {
        this.Cards = newDeck;
    }
}
