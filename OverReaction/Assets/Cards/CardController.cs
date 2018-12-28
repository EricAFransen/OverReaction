using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public interface ICardController
{
    void Init();
    int GetNumberOfCards();
    ICard GetCard(int ID);
    string GetCardName(int ID);
    string GetCardDescription(int ID);
    string GetCardCSV(int ID);
    void PlayCard(int index);
    void PlayCard(string peerID, int index);
    void AddEffect(Effect effect);
    void Update(double TimeElapsed);
    void SetDeck(Dictionary<int, string> newDeck);
}

public class HostCardController : ICardController
{
    public CardFactory Factory { get; private set; }
    private List<ICard> MasterCardList;
    // A mapping of PlayerID to a mapping of DeckIndex to Card
    private Dictionary<string, Dictionary<int, ICard>> Decks;
    private Simulation Sim;
    private List<Effect> Effects;
    private List<Effect> EffectsToRemove;

    private GameSparksManager SparksManager;

    private bool IsInitialized = false;

    public string FileName = "BasicDeck.txt";
    public string Path = "..\\OverReaction\\Assets\\Decks\\";

    public void Init()
    {
        if (!IsInitialized)
        {
            // Get the GameSparksManager instance
            SparksManager = GameSparksManager.instance;
            // Create a new CardFactory
            Factory = new CardFactory();
            // Add all ParserOptions that currently exist
            Factory.AutoAddParsers();
            // Parse the deck
            MasterCardList = Factory.ParseDeck(Path + FileName, true);
            // Initialize the Deck-to-Player map
            Decks = new Dictionary<string, Dictionary<int, ICard>>();
            // Get the simulation instance
            Sim = Camera.main.GetComponent<MainController>().Sim;

            List<RealtimePlayer> Players = SparksManager.getSessionInfo().getPlayers();
            foreach (RealtimePlayer Player in Players)
            {
                Dictionary<int, int> Deck = new Dictionary<int, int>();
                List<int> shuffledDeck = shuffle(MasterCardList.Count);
                for (int i = 0; i < shuffledDeck.Count; i++)
                {
                    Deck.Add(i, shuffledDeck[i]);
                }
            }

            SparksManager.sendInitialDeck(Decks);

            IsInitialized = true;
        }
    }

    public ICard GetCard(int ID) { return MasterCardList[ID]; }
    public string GetCardName(int ID) { return MasterCardList[ID].GetName(); }
    public string GetCardDescription(int ID) { return MasterCardList[ID].GetDescription(); }
    public string GetCardCSV(int ID) { return MasterCardList[ID].GetCSV(); }
    public int GetNumberOfCards() { return MasterCardList.Count; }

    public void PlayCard(int index)
    {
        SparksManager.canPlayCard(index);
    }

    public void PlayCard(string peerID, int index)
    {
        Decks[peerID][index].Play(Sim);
    }

    public void SetDeck(Dictionary<int, string> newDeck)
    {
        // Host doesn't do anything
    }

    public void Update(double timeElapsed)
    {
        UpdateEffects(timeElapsed);
    }

    public void UpdateEffects(double timeElapsed)
    {
        foreach (Effect effect in Effects)
        {
            effect.TimeRemaining -= timeElapsed;
            if (effect.TimeRemaining < 0)
            {
                effect.Remove();
                EffectsToRemove.Add(effect);
            }
        }
        foreach (Effect effect in EffectsToRemove)
        {
            Effects.Remove(effect);
        }
        EffectsToRemove.Clear();
    }

    public void AddEffect(Effect effect)
    {
        effect.Play();
        Effects.Add(effect);
    }


    public static List<int> shuffle(int max)
    {
        System.Random rng = new System.Random(DateTime.Now.Millisecond);

        List<int> deck = new List<int>();
        for(int i = 0; i < max; i++)
        {
            deck.Add(i);
        }
        int n = max;
        while(n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            int value = deck[k];
            deck[k] = deck[n];
            deck[n] = value;
        }
        return deck;
    }
}

public class ClientCardController : ICardController
{
    private Dictionary<int, CardInfo> Cards;

    public void Init()
    {
        
    }

    public void SetDeck(Dictionary<int, string> newDeck)
    {
        Dictionary<int, CardInfo> NewCards = new Dictionary<int, CardInfo>();
        foreach(KeyValuePair<int, string> StringPair in newDeck)
        {
            CardInfo CI = new CardInfo("", StringPair.Value, "");
            NewCards.Add(StringPair.Key, CI);
        }
        Cards = NewCards;
    }

    public void Update(double TimeElapsed)
    {
        // Client doesn't really have to do anything, yet
    }

    public void AddEffect(Effect effect)
    {
        // Client doesn't have to do anything, yet
    }

    public void PlayCard(string peerID, int index)
    {
        // Do nothing because we are not the host
    }

    public void PlayCard(int index)
    {
        Debug.Log("Tried to play a card.");
        GameSparksManager.instance.canPlayCard(index);
    }

    public string GetCardName(int index) { return Cards[index].Name; }
    public string GetCardDescription(int index) { return Cards[index].Description; }
    public string GetCardCSV(int index) { return Cards[index].CSV; }
    public ICard GetCard(int index) { throw new InvalidOperationException(); }
    public int GetNumberOfCards() { return Cards.Count; }
}

public class OfflineCardController : ICardController
{
    public string SourceFolder = "..\\OverReaction\\Assets\\Decks\\";
    public string SourceFile = "BasicDeck.txt";

    private Dictionary<int, ICard> MainDeck;
    private List<int> PlayerDeck;
    private CardFactory Factory;
    private Simulation Sim;
    private List<Effect> ActiveEffects;
    private List<Effect> EffectsToRemove;

    public void Init()
    {
        MainDeck = new Dictionary<int, ICard>();
        PlayerDeck = new List<int>();
        Factory = new CardFactory();
        Factory.AutoAddParsers();
        Sim = Camera.main.GetComponent<MainController>().Sim;
        ActiveEffects = new List<Effect>();
        EffectsToRemove = new List<Effect>();

        List<ICard> cards = Factory.ParseDeck(SourceFolder + SourceFile, true);
        for(int i = 0; i < cards.Count; i++)
        {
            MainDeck.Add(i, cards[i]);
        }

        PlayerDeck = HostCardController.shuffle(cards.Count);
    }

    public void SetDeck(Dictionary<int, string> newDeck)
    {
        // We're playing offline, so do nothing
    }

    public void Update(double timeElapsed)
    {
        UpdateEffects(timeElapsed);
    }

    public void UpdateEffects(double timeElapsed)
    {
        foreach(Effect effect in ActiveEffects)
        {
            effect.TimeRemaining -= timeElapsed;
            if(effect.TimeRemaining < 0)
            {
                EffectsToRemove.Add(effect);
            }
        }
        foreach(Effect effect in EffectsToRemove)
        {
            ActiveEffects.Remove(effect);
            effect.Remove();
        }
    }

    public void AddEffect(Effect effect)
    {
        ActiveEffects.Add(effect);
        effect.Play();
    }

    public void PlayCard(int index)
    {
        MainDeck[index].Play(Sim);
    }

    public void PlayCard(string player, int index)
    {
        // This method is used by the GameSparks manager, so we don't have to do anything here
    }

    public string GetCardCSV(int index) { return MainDeck[index].GetCSV(); }
    public string GetCardDescription(int index) { return MainDeck[index].GetDescription(); }
    public string GetCardName(int index) { return MainDeck[index].GetName(); }
    public ICard GetCard(int index) { return MainDeck[index]; }
    public int GetNumberOfCards() { return MainDeck.Count; }

}
