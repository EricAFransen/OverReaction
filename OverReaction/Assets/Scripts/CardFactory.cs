using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardFactory
{
    private System.Random Rand;

    //Different options for creating cards
    public CardFactory(System.Random rand)
    {
        Rand = rand;      
    }

    public Card CreateCard(int cost, int duration)
    {
        return new Card(cost, duration);
    }

    public Card CreateCard(int cost, int duration, ChemicalReaction reaction)
    {
        return new ReactionCard(cost, duration, reaction);
    }

    public Card CreateCard(int cost, int duration, int numerator, int denominator)
    {
        return new RateModifierCard(cost, duration, numerator, denominator);
    }

    // Todo: Make this actually random
    public Card RandCard()
    {
        int i = Rand.Next() % 5;
        return CreateCard(i, 1, 1, 1);
    }
}

public class CardParser : CardFactory
{
    // Class to hold different options for parsing
    private ParserOption parserOption;
    public CardParser(System.Random rand) : base(rand)
    {
        parserOption = new ParserOption();
    }
    /// <summary>
    /// Parses a line of CSV, reads the first value to find an ID, and then tries to pass it to another parser
    /// Returns the result of that parser if it succeeds, otherwise it will return null
    /// </summary>
    public virtual Card ParseLine(string line)
    {
        string[] split = line.Split(",".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
        string ID = split[0];
        string[] values = new string[split.Length - 1];
        for(int i = 1; i < split.Length; i++)
        {
            values[i - 1] = split[i];
        }
        switch(ID)
        {
            case "0":
                return Card.TryParse(values);
            case "1":
                return ReactionCard.TryParse(values);
            case "2":
                return RateModifierCard.TryParse(values);
            default:
                // Try other parsers
                return parserOption.TryParse(values, ID);
        }
    }
    /// <summary>
    /// Parses either a file or a string of CSV
    /// Will try to open a file if IsFile is true, otherwise will treat the source as CSV
    /// Breaks up each newline into a card
    /// </summary>
    public virtual List<Card> ParseDeck(string source, bool IsFile)
    {
        string deck = "";
        if (IsFile)
        {
            using (System.IO.StreamReader sr = new System.IO.StreamReader(source, System.Text.Encoding.UTF8))
            {
                deck = sr.ReadToEnd();
            }
        } else
        {
            deck = source;
        }
        List<Card> result = new List<Card>();
        string[] lines = deck.Split("\n".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
        foreach (string line in lines)
        {
            Card card = ParseLine(line);
            if(card != null)
                result.Add(card);
        }
        return result;
    }

    /// <summary>
    /// Function to add parser options to the Parser
    /// </summary>
    public void AddParserOption(ParserOption pa)
    {
        parserOption.SetNext(pa);
    }
}

/// <summary>
/// Basic class to add an option to the parser
/// Is designed to be overriden
/// </summary>
public class ParserOption
{
    /// <summary>
    /// Stores the next option in the sequence
    /// </summary>
    public ParserOption NextOption { get; protected set; }
    /// <summary>
    /// Initializes the parser option, sets the next option to null
    /// </summary>
    public ParserOption()
    {
        NextOption = null;
    }
    /// <summary>
    /// Tries to parse the line. Because this is a default one, it doesn't really do anything,
    /// Checks if the next option is null, and passes the line over if it exists.
    /// </summary>
    public virtual Card TryParse(string[] values, string ID)
    {
        if (NextOption == null) return null;
        return NextOption.TryParse(values, ID);
    }
    /// <summary>
    /// Sets the next parser option. If we already have the option, we send the new option
    /// over to the next one.
    /// </summary>
    public void SetNext(ParserOption option)
    {
        if (NextOption == null) NextOption = option;
        else NextOption.SetNext(option);
    }
}

