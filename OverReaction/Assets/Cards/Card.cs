using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class to represent cards in the game
/// </summary>
public class Card
{
    public int Cost { get; protected set; }
    public int Duration { get; protected set; }

    public Card(int cost, int duration)
    {
        Cost = cost;
        Duration = duration;
    }

    /// <summary>
    /// Attempts to parse a set of string values into a Default card
    /// Returns the card if successful, returns null otherwise.
    /// Uses the format: $Cost,$Duration
    /// </summary>
    public static Card TryParse(string[] values)
    {
        // Get a combined string for errors
        string line = string.Join(",", values);
        // Keeps track of the current index
        int cur = 0;
        // cost and the duration of the card we're creating
        int cost, duration;
        Debug.LogWarning("Default card encountered in line: " + line);
        // Check the length of the array, make sure we have enough values
        if (values.Length < 2)
        {
            Debug.LogError("Not enough arguments to parse default card in line: " + line);
            return null;
        }
        // Holds the currently parsed value
        int val;
        // Try to parse the cost
        if (!int.TryParse(values[cur++], out val))
        {
            Debug.LogError("Could not parse Cost for default card in line: " + line);
            return null;
        }
        // set the cost
        cost = val;
        // Try to parse the duration
        if (!int.TryParse(values[cur++], out val))
        {
            Debug.LogError("Could not parse Duration for default card in line: " + line);
            return null;
        }
        // Set the duration
        duration = val;
        // Return a new card
        return new Card(cost, duration);
    }

    /// <summary>
    /// Method used to play the card on the simulation
    /// </summary>
    public virtual void Play(Simulation sim)
    {
        // This is where we'll subtract the cost from the player's balance if we used it
    }

    /// <summary>
    /// Method used to generate a string to display on a card
    /// </summary>
    public override string ToString()
    {
        return "Default card with \ncost: " + Cost + "\nand duration: " + Duration;
    }

    /// <summary>
    /// Method used to generate a string of CSV that represents this card
    /// </summary>
    public virtual string ToDataString()
    {
        return "0," + Cost + "," + Duration + ",";
    }
}

public class ReactionCard : Card
{
    // The reaction used by the card
    public ChemicalReaction Reaction { get; private set; }

    /// <summary>
    /// Creates a reaction with the given cost, duration, and reaction
    /// </summary>
    public ReactionCard(int cost, int duration, ChemicalReaction reaction) : base(cost, duration)
    {
        Reaction = reaction;
    }

    /// <summary>
    /// Attempts to parse a set of string values into a Reaction card
    /// Returns the card if successful, returns null otherwise.
    /// Uses the format: $Cost,$Duration,$Rate,$NumSpecies,$Reactants,$Products
    /// Note: $NumSpecies is the number of reactants and products, not an array
    /// Note: $Reactants and $Products are both lists of integers
    /// </summary>
    public static new Card TryParse(string[] values)
    {
        // Combine the string for debugging
        string line = string.Join(",", values);
        // hold the current line
        int cur = 0;
        // variables to hold the current parsed value, cost, duration, rate, and number of species
        int val, cost, duration, rate, numSpecies;
        // Make sure we have enough to read what we have so far
        if (values.Length < 4)
        {
            Debug.LogError("Not enough arguments to parse ReactionCard at line: " + line);
            return null;
        }
        // Try to read the cost
        if (!int.TryParse(values[cur++], out val))
        {
            Debug.LogError("Could not parse Cost for ReactionCard at line: " + line);
            return null;
        }
        cost = val;
        // Try to read the Rate
        if (!int.TryParse(values[cur++], out val))
        {
            Debug.LogError("Could not parse Rate for ReactionCard at line: " + line);
            return null;
        }
        rate = val;
        // Try to read the duration
        if (!int.TryParse(values[cur++], out val))
        {
            Debug.LogError("Could not parse Duration for ReactionCard at line: " + line);
            return null;
        }
        duration = val;
        // Try to read the number of species
        if (!int.TryParse(values[cur++], out val))
        {
            Debug.LogError("Could not parse NumSpecies for ReactionCard at line: " + line);
            return null;
        }
        numSpecies = val;
        // Create arrays for the reactants, products, and liveSpecies for the reaction
        int[] reactants = new int[numSpecies];
        int[] products = new int[numSpecies];
        int[] liveSpecies = new int[numSpecies];
        // Make sure we have enough parameters for all the reactants and products
        if (cur + numSpecies > values.Length)
        {
            Debug.LogError("Not enough arguments to parse ReactionCard at line: " + line);
            return null;
        }
        // Read through the reactants
        for(int i = 0; i < numSpecies; i++)
        {
            // Set the live species to 0 initially
            liveSpecies[i] = 0;
            // Try to reaed the reactant
            if (!int.TryParse(values[cur++], out val))
            {
                Debug.LogError("Could not parse a reactant for ReactionCard at line: " + line);
                return null;
            }
            reactants[i] = val;
        }
        // Read through the products
        for (int i = 0; i < numSpecies; i++)
        {
            // Try to read the product
            if (!int.TryParse(values[cur++], out val))
            {
                Debug.LogError("Could not parse a product for ReactionCard at line: " + line);
                return null;
            }
            products[i] = val;
        }
        // Make a new reaction
        ChemicalReaction reaction = new ChemicalReaction(reactants, products, rate, liveSpecies);
        // Return the finished card
        return new ReactionCard(cost, duration, reaction);
    }

    /// <summary>
    /// Copies the current live species from the simulation, applies it to the reaction,
    /// and then adds the reaction to the simulation.
    /// </summary>
    public override void Play(Simulation sim)
    {
        // Get the liveSpecies from the simulation
        int[] liveSpecies = sim.getLiveSpecies();
        // Set the liveSpecies from the simulation to the reaction
        Reaction.numSpecies = liveSpecies;
        // Add the reaction to the simulation
        sim.AddReaction(Reaction);
        // Add the reaction to the currently active reactions
        sim.AddToHand(this);
    }

    /// <summary>
    /// Generates the string to display on a card
    /// </summary>
    public override string ToString()
    {
        return "Cost: " + Cost + "\nDuration: " + Duration + "\nReaction: " + Reaction.EquationMinimal();
    }

    /// <summary>
    /// Generates a line of CSV in the form:
    /// $ID,$Cost,$Duration,$Rate,$NumSpecies,$Reactants,$Products
    /// </summary>
    public override string ToDataString()
    {
        // Find the number of species
        int numSpecies = Reaction.products.Length;
        // Grab the reactants and products
        int[] reactants = Reaction.reactants;
        int[] products = Reaction.products;
        // Generate CSV for the ID, Cost, Rate, Duration, and number of species
        string result = "1," + Cost + "," + Reaction.rate + "," + Duration + "," + numSpecies + ",";
        // Iterate through the reactants
        for(int i = 0; i < numSpecies; i++)
        {
            // Print the reactant
            result += reactants[i] + ",";
        }
        // Iterate through the products
        for(int i = 0; i < numSpecies; i++)
        {
            // Print the product
            result += products[i] + ",";
        }
        // return the final string
        return result;
    }
}

public class RateModifierCard : Card
{
    // Store the rate as a numerator and denominator, to make parsing and reading easier
    public int Numerator { get; private set; }
    public int Denominator { get; private set; }

    public RateModifierCard(int cost, int duration, int numerator, int denominator) : base(cost, duration)
    {
        Cost = cost;
        Duration = duration;
    }

    /// <summary>
    /// Tries to parse a line of CSV into a RateModifierCard using the form:
    /// $Cost,$Duration,$Numerator,$Denominator
    /// </summary>
    public static new Card TryParse(string[] values)
    {
        // Re-combine the line
        string line = string.Join(",", values);
        // Keep track of the current index
        int cur = 0;
        // Save the currently parsed value, cost, duration, numerator, and denominator
        int val, cost, duration, numerator, denominator;
        // Make sure we have enough parameters
        if (values.Length < 4)
        {
            Debug.LogError("Not enough arguments to parse a RateModifierCard at line: " + line);
            return null;
        }
        // Read the Cost
        if (!int.TryParse(values[cur++], out val))
        {
            Debug.LogError("Could not parse Cost for a RateModifierCard at line: " + line);
            return null;
        }
        cost = val;
        // Read the duration
        if (!int.TryParse(values[cur++], out val))
        {
            Debug.LogError("Could not parse Duration for a RateModifierCard at line: " + line);
            return null;
        }
        duration = val;
        // Read the numerator
        if (!int.TryParse(values[cur++], out val))
        {
            Debug.LogError("Could not parse Numerator for a RateModifierCard at line: " + line);
            return null;
        }
        numerator = val;
        // Read the denominator
        if (!int.TryParse(values[cur++], out val))
        {
            Debug.LogError("Could not parse Denominator for a RateModifierCard at line: " + line);
            return null;
        }
        denominator = val;
        // Return a new card
        return new RateModifierCard(cost, duration, numerator, denominator);
    }

    /// <summary>
    /// Multiplies the rate on the simulation by Numerator / Denominator
    /// </summary>
    /// <param name="sim"></param>
    public override void Play(Simulation sim)
    {
        // Make sure we aren't dividing by zero
        if (Denominator == 0)
        {
            Debug.LogError("RateModifier card has a denominator of 0");
            return;
        }
        // Calculate the modifier
        double rateModifier = Numerator / Denominator;
        // Set the rate in the simulation
        //int t = sim.TriggerModifier(1, rateModifier);
        Debug.Log("THIS SHOULD BE DECREMENTED");
        //Debug.Log(t);
        Debug.Log("Played a rate modifier card");
    }

    /// <summary>
    /// Gets the string to display on a card
    /// </summary>
    public override string ToString()
    {
        return "Cost: " + Cost + "\nNumerator: " + Numerator + "\nDenominator: " + Denominator;
    }

    /// <summary>
    /// Converts the card to a line of CSV in the form:
    /// $ID,$Cost,$Duration,$Numerator,$Denominator,
    /// </summary>
    /// <returns></returns>
    public override string ToDataString()
    {
        return "2," + Cost + "," + Duration + "," + Numerator + "," + Denominator + ",";
    }
}

public class TotalRateModiferCard : RateModifierCard
{
    public TotalRateModiferCard(int cost, int duration, int numerator, int denominator) : base(cost, duration, numerator, denominator)
    {
    }

    /// <summary>
    /// Updates all active reaction rates by Numerator/Denomenator
    /// </summary>
    /// <param name="sim">Simulation that is tracking the reactions</param>
    public override void Play(Simulation sim)
    {
        // Make sure we aren't dividing by zero
        if (Denominator == 0)
        {
            Debug.LogError("RateModifier card has a denominator of 0");
            return;
        }
        // Calculate the modifier
        double rateModifier = Numerator / Denominator;
        // Set the rate in the simulation
        int t = sim.SetUpModifier(GeneralModifierCard.ModifierType.TotalRate, rateModifier);
        Debug.Log(t);
        Debug.Log("Played a rate modifier card");
    }

    public override string ToDataString()
    {
        return "3," + Cost + "," + Duration + "," + Numerator + "," + Denominator + ",";
    }
}

public class ModifierCard : Card
{
    // Figures out what kind of modifier this is
    public enum ModifierType { TotalRate = 0, ReactionRate = 1, Time = 2, Reactants = 3, Products = 4 }
    // Keeps track of the kind of modifier
    public ModifierType Type { get; private set; }
    // Numerator and denominator, easier to parse than a double or float
    public int Numerator, Denominator;

    public ModifierCard(int cost, int duration, ModifierType type, int numerator, int denominator) : base(cost, duration)
    {
        Type = type;
        Numerator = numerator;
        Denominator = denominator;
    }

    public static new Card TryParse(string[] values)
    {
        // combine the string for debugging
        string line = string.Join(",", values);
        // Check the length of the array
        if (values.Length < 5)
        {
            Debug.LogError("Not enough parameters for a ModifierCard at line: " + line);
            return null;
        }
        // Keep track of various parameters
        int val, cost, duration, numerator, denominator;
        ModifierType type;
        // Keep track of the current index
        int cur = 0;
        // Try to read the Cost
        if (!int.TryParse(values[cur++], out val))
        {
            Debug.LogError("Could not parse Cost for a ModifierCard at line: " + line);
            return null;
        }
        cost = val;
        // Try to read the duration
        if (!int.TryParse(values[cur++], out val))
        {
            Debug.LogError("Could not parse Duration for a ModifierCard at line: " + line);
            return null;
        }
        duration = val;
        // Try to read the ModifierType
        if (!int.TryParse(values[cur++], out val))
        {
            Debug.LogError("Could not parse ModifierType for a ModifierCard at line: " + line);
            return null;
        }
        type = (ModifierType)val;
        // Try to read the Numerator
        if (!int.TryParse(values[cur++], out val))
        {
            Debug.LogError("Could not parse Numerator for a ModifierCard at line: " + line);
            return null;
        }
        numerator = val;
        // Try to read the Denominator
        if (!int.TryParse(values[cur++], out val))
        {
            Debug.LogError("Could not parse Denominator for a ModifierCard at line: " + line);
            return null;
        }
        denominator = val;
        // Return a new modifier card
        return new ModifierCard(cost, duration, type, numerator, denominator);
    }

    public override void Play(Simulation sim)
    {
        // Make sure we aren't dividing by zero
        if(Denominator == 0)
        {
            Debug.LogError("ModifierCard had a denominator of 0!");
            return;
        }
        // Calculate the modifier
        double modifier = Numerator / Denominator;
        int t;
        switch (Type)
        {
            // Different methods for each type
            case ModifierType.TotalRate:
                //int t = sim.TriggerModifier(1, modifier);
                t = sim.SetUpModifier(GeneralModifierCard.ModifierType.TotalRate, modifier);
                Debug.Log("Played a modifierCard with type TotalRate with a final value of " + modifier);
                break;
            case ModifierType.ReactionRate:
                t = sim.SetUpModifier(GeneralModifierCard.ModifierType.ReactionRate, modifier);
                Debug.Log("Played a modifierCard with type ReactionRate with a final value of " + modifier);
                break;
            case ModifierType.Time:
                t = sim.SetUpModifier(GeneralModifierCard.ModifierType.Time, modifier);
                Debug.Log("Played a modifierCard with type Time with a final value of " + modifier);
                break;
            case ModifierType.Reactants:
                t = sim.SetUpModifier(GeneralModifierCard.ModifierType.Reactants, modifier);
                Debug.Log("Playved a modifierCard with type Reactants with a final value of " + modifier);
                break;
            case ModifierType.Products:
                t = sim.SetUpModifier(GeneralModifierCard.ModifierType.Products, modifier);
                Debug.Log("Played a modifierCard with type Products with a final value of " + modifier);
                break;
            default:
                Debug.LogWarning("Played a modifierCard with an unhandled type! The value was " + modifier);
                break;
        }
    }

    public override string ToString()
    {
        string result = "";
        if (Denominator == 0) return "Divide by zero modifier";
        double modifier = Numerator / Denominator;
        // Different text for each type
        switch(Type)
        {
            case ModifierType.TotalRate:
                result = "Total Rate";
                break;
            case ModifierType.ReactionRate:
                result = "Reaction Rate";
                break;
            case ModifierType.Time:
                result = "Time";
                break;
            case ModifierType.Reactants:
                result = "Reactant";
                break;
            case ModifierType.Products:
                result = "Product";
                break;
            default:
                result = "Unhandled";
                break;
        }
        result += " Modifer Card\nCost: " + Cost + "\nDuration: " + Duration + "\nModifier: " + modifier;
        return result;
    }
}
