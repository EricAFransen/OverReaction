using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interface used to define the bare minimum a card is required to have
/// </summary>
public interface ICard
{
    /// <summary>
    /// Method used by the card controller to perform this card's actions
    /// </summary>
    void Play(Simulation sim);
    /// <summary>
    /// Method used to get the name of the card
    /// </summary>
    /// <returns>The name of the card</returns>
    string GetName();
    /// <summary>
    /// Method used to get the description of the card
    /// </summary>
    /// <returns>The name of the card</returns>
    string GetDescription();
    /// <summary>
    /// Method used to get the CSV representation of this card
    /// </summary>
    /// <returns>The CSV representation of this card</returns>
    string GetCSV();
}

public class CardInfo
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string CSV { get; private set; }
    
    public CardInfo(ICard card)
    {
        Name = card.GetName();
        Description = card.GetDescription();
        CSV = card.GetCSV();
    }

    public CardInfo(string name, string desc, string csv)
    {
        Name = name;
        Description = desc;
        CSV = csv;
    }
}

/// <summary>
/// The abstract card forming the basis of all regular cards in game
/// </summary>
public abstract class Card : ICard
{
    /// <summary>
    /// The cost of this card
    /// </summary>
    protected int Cost;
    /// <summary>
    /// The duration of this card
    /// </summary>
    protected double Duration;

    /// <summary>
    /// Method used by the card controller to play the card
    /// </summary>
    public abstract void Play(Simulation sim);
    /// <summary>
    /// Method used to get the name of the card
    /// </summary>
    /// <returns></returns>
    public abstract string GetName();
    /// <summary>
    /// Method used to get the description of the card
    /// </summary>
    /// <returns>The name of the card</returns>
    public abstract string GetDescription();
    /// <summary>
    /// Method used to get the CSV representation of this card
    /// </summary>
    /// <returns>The CSV representation of this card</returns>
    public abstract string GetCSV();
}

/// <summary>
/// The card which adds reactions to the simulation
/// </summary>
public class ReactionCard : Card
{
    /// <summary>
    /// The reaction we're adding to the simulation
    /// </summary>
    private ChemicalReaction Reaction;


    public ReactionCard(int cost, int duration, ChemicalReaction reaction)
    {
        Cost = cost;
        Duration = duration;
        Reaction = reaction;
    }

    public override void Play(Simulation sim)
    {
        Reaction.numSpecies = sim.getLiveSpecies();
        sim.AddReaction(Reaction);
        sim.AddToHand(this);
    }

    public override string GetName()
    {
        return "Reaction Card";
    }

    public override string GetDescription()
    {
        return "Cost: " + Cost + "\nDuration: " + Duration + "\nReaction: " + Reaction.EquationMinimal();
    }

    public override string GetCSV()
    {
        // Find the number of species
        int numSpecies = Reaction.products.Length;
        // Grab the reactants and products
        int[] reactants = Reaction.reactants;
        int[] products = Reaction.products;
        // Generate CSV for the ID, Cost, Rate, Duration, and number of species
        string result = "1," + Cost + "," + Reaction.rate + "," + Duration + "," + numSpecies + ",";
        // Iterate through the reactants
        for (int i = 0; i < numSpecies; i++)
        {
            // Print the reactant
            result += reactants[i] + ",";
        }
        // Iterate through the products
        for (int i = 0; i < numSpecies; i++)
        {
            // Print the product
            result += products[i] + ",";
        }
        // return the final string
        return result;
    }

    public class ReactionParser : IParserOption
    {
        public ICard TryParse(int ID, int[] values)
        {
            int cur = 1;
            // variables to hold the current parsed value, cost, duration, rate, and number of species
            int cost, duration, rate, numSpecies;
            // Make sure we have enough to read what we have so far
            if (values.Length < 4)
            {
                throw new InvalidCardException("Not enough arguments to parse ReactionCard");
            }
            cost = values[cur++];
            rate = values[cur++];
            duration = values[cur++];
            numSpecies = values[cur++];
            // Create arrays for the reactants, products, and liveSpecies for the reaction
            int[] reactants = new int[numSpecies];
            int[] products = new int[numSpecies];
            int[] liveSpecies = new int[numSpecies];
            // Make sure we have enough parameters for all the reactants and products
            if (cur + numSpecies > values.Length)
            {
                throw new InvalidCardException("Not enough arguments to parse reactants and/or products of ReactionCard");
            }
            // Read through the reactants
            for (int i = 0; i < numSpecies; i++)
            {
                // Set the live species to 0 initially
                liveSpecies[i] = 0;
                reactants[i] = values[cur++];
            }
            // Read through the products
            for (int i = 0; i < numSpecies; i++)
            {
                products[i] = values[cur++];
            }
            // Make a new reaction
            ChemicalReaction reaction = new ChemicalReaction(reactants, products, rate, liveSpecies);
            // Return the finished card
            return new ReactionCard(cost, duration, reaction);
        }
        public int ID()
        {
            return 1;
        }
    }
}

public class RateModifierCard : Card
{
    public enum ModifierType { Unhandled = -1, TotalRate = 0, ReactionRate = 1, ReactionTime = 2, ReactionProducts = 3, ReactionReactants = 4 }

    private ModifierType Type;
    private int Numerator;
    private int Denominator;

    public RateModifierCard(int cost, int duration, ModifierType type, int numerator, int denominator)
    {
        Cost = cost;
        Duration = duration; ;
        Type = type;
        Numerator = numerator;
        Denominator = denominator;
    }

    public override void Play(Simulation sim)
    {
        double Modifier = Numerator / Denominator;
        Camera.main.GetComponent<MainController>().CardController.AddEffect(new RateModifierEffect(Duration, Modifier, Type));
        
    }

    public override string GetName()
    {
        string typeString;
        switch(Type)
        {
            case ModifierType.TotalRate:
                typeString = "Total Reaction Rate ";
                break;
            case ModifierType.ReactionRate:
                typeString = "Reaction Rate ";
                break;
            case ModifierType.ReactionTime:
                typeString = "Reaction Time ";
                break;
            case ModifierType.ReactionProducts:
                typeString = "Reaction Product ";
                break;
            case ModifierType.ReactionReactants:
                typeString = "Reaction Reactant ";
                break;
            default:
                typeString = "Unhandled ";
                break;
        }
        return typeString + "Modifier Card";
    }

    public override string GetDescription()
    {
        return "Type: " + Type + "\nCost: " + Cost + "\nMultiplier: " + Numerator / Denominator;
    }

    public override string GetCSV()
    {
        return "2," + Cost + "," + Duration + "," + Type + "," + Numerator + "," + Denominator + ",";
    }
}

public class RateModifierParser : IParserOption
{
    public int ID() { return 2; }
    public ICard TryParse(int ID, int[] values)
    {
        int cost, duration, numerator, denominator;
        int cur = 1; // Setting the start index to 1 to skip re-reading the ID
        RateModifierCard.ModifierType type;
        if (values.Length < 5)
            throw new InvalidCardException("Not enough arguments to parse a RateModifier Card");
        cost = values[cur++];
        duration = values[cur++];
        type = (RateModifierCard.ModifierType)values[cur++];
        numerator = values[cur++];
        denominator = values[cur++];
        return new RateModifierCard(cost, duration, type, numerator, denominator);
    }
}

public class RateModifierEffect : Effect
{
    public double Modifier;
    public RateModifierCard.ModifierType Type;
    private ChemicalReaction Reaction;

    public RateModifierEffect(double duration, double modifier, RateModifierCard.ModifierType type) : base(duration)
    {
        Modifier = modifier;
        Type = type;
        Reaction = null;
    }

    public override void Play()
    {
        if (Type == RateModifierCard.ModifierType.TotalRate)
        {
            throw new System.NotImplementedException();
        }
        Debug.LogWarning("Old selection code used. Might be a good idea to rewrite.");
        Simulation sim = Camera.main.GetComponent<MainController>().Sim;
        int index = sim.SetUpModifier(Type, Modifier);
        Reaction = sim.GetReactions()[index];
    }

    public override void Remove()
    {
        if (Type == RateModifierCard.ModifierType.TotalRate)
        {
            throw new System.NotImplementedException();
        }
        Simulation sim = Camera.main.GetComponent<MainController>().Sim;
        int index = sim.GetReactionIndex(Reaction);
        if (index < 0) return;
        Reaction = sim.RemoveReaction(index);
        switch (Type)
        {
            case RateModifierCard.ModifierType.ReactionRate:
                Reaction.rate = (int)(Reaction.rate / Modifier);
                break;
            case RateModifierCard.ModifierType.ReactionTime:
                Reaction.time = Reaction.time / Modifier;
                break;
            case RateModifierCard.ModifierType.ReactionProducts:
                for (int i = 0; i < Reaction.products.Length; i++)
                {
                    Reaction.products[i] = (int)(Reaction.products[i] / Modifier);
                }
                break;
            case RateModifierCard.ModifierType.ReactionReactants:
                for (int i = 0; i < Reaction.reactants.Length; i++)
                {
                    Reaction.reactants[i] = (int)(Reaction.reactants[i] / Modifier);
                }
                break;
            default:
                throw new System.NotImplementedException();
        }
    }
}