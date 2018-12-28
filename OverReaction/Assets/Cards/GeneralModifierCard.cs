using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class GeneralModifierCard : Card
{
    public enum ModifierType { Empty = -1, TotalRate = 0, ReactionRate = 1, Time = 2, Reactants = 3, Products = 4 }

    public ModifierType Type { get; private set; }

    public int Numerator, Denominator;

    public GeneralModifierCard(int cost, int duration, ModifierType type, int numerator, int denominator) : base(cost, duration)
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
        return new GeneralModifierCard(cost, duration, type, numerator, denominator);
    }

    public override void Play(Simulation sim)
    {
        if (Denominator == 0)
        {
            Debug.LogError("GeneralModifierCard had a denominator of 0!");
            return;
        }
        Camera.main.GetComponent<CardController>().AddEffect(new GeneralModifierEffect(this));
    }

    public override string ToString()
    {
        string result = "";
        if (Denominator == 0) return "Divide by zero modifier";
        double modifier = Numerator / Denominator;
        // Different text for each type
        switch (Type)
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

    public override string ToDataString()
    {
        return "3," + Cost + "," + Duration + "," + Type + "," + Numerator + "," + Denominator + ",";
    }
}

public class GeneralModifierEffect : Effect
{
    public double Modifier { get; private set; }
    public GeneralModifierCard.ModifierType Type { get; private set; }
    private ChemicalReaction reaction;


    public GeneralModifierEffect(GeneralModifierCard card) : base(card.Duration)
    {
        Modifier = card.Numerator / card.Denominator;
        Type = card.Type;
        reaction = null;
    }

    public override void Play()
    {
        CardController controller = Camera.main.GetComponent<CardController>();
        if (Type == GeneralModifierCard.ModifierType.TotalRate)
        {
            // Change the total rate
            return;
        }
        Simulation simulation = Camera.main.GetComponent<Simulation>();
        ReactionSelectionExtension extension = (ReactionSelectionExtension)controller.GetExtension(typeof(ReactionSelectionExtension));
        if (extension == null)
        {
            Debug.LogError("Tried to use a ControllerExtension that did not exist!");
            return;
        }
        /*
        int index = extension.SelectReaction(simulation);
        reaction = simulation.RemoveReaction(index);*/

        int index = extension.SelectReaction(simulation, Type, Modifier);
        return;
        //This is now handled in Simulation methods SetUpModifier() and ApplyModifier()
        /*
        reaction = simulation.RemoveReaction(index);
        if (reaction == null)
        {
            Debug.LogWarning("GeneralModifierCard was given a null reaction. Index was " + index);
            return;
        }
        // Now we decide what to do based on the type of the modifier
        switch (Type)
        {
            case GeneralModifierCard.ModifierType.ReactionRate:
                reaction.rate = (int)(reaction.rate * Modifier);
                break;
            case GeneralModifierCard.ModifierType.Time:
                reaction.time = reaction.time * Modifier;
                break;
            case GeneralModifierCard.ModifierType.Reactants:
                for (int i = 0; i < reaction.reactants.Length; i++)
                    reaction.reactants[i] = (int)(reaction.reactants[i] * Modifier);
                break;
            case GeneralModifierCard.ModifierType.Products:
                for (int i = 0; i < reaction.products.Length; i++)
                    reaction.products[i] = (int)(reaction.products[i] * Modifier);
                break;
            default:
                Debug.LogWarning("Tried to play a GeneralModifierCard with an unhandled type!");
                break;
        }
        simulation.AddReaction(reaction);*/
    }

    public override void Remove()
    {
        if (Type == GeneralModifierCard.ModifierType.TotalRate)
        {
            // Change the total rate back to normal
            return;
        }
        Simulation simulation = Camera.main.GetComponent<Simulation>();
        int index = simulation.GetReactionIndex(reaction);
        if (index < 0) return;
        reaction = simulation.RemoveReaction(index);
        switch (Type)
        {
            case GeneralModifierCard.ModifierType.ReactionRate:
                reaction.rate = (int)(reaction.rate / Modifier);
                break;
            case GeneralModifierCard.ModifierType.Time:
                reaction.time = reaction.time / Modifier;
                break;
            case GeneralModifierCard.ModifierType.Reactants:
                for (int i = 0; i < reaction.reactants.Length; i++)
                    reaction.reactants[i] = (int)(reaction.reactants[i] / Modifier);
                break;
            case GeneralModifierCard.ModifierType.Products:
                for (int i = 0; i < reaction.products.Length; i++)
                    reaction.products[i] = (int)(reaction.products[i] / Modifier);
                break;
        }
    }
}

public class GeneralModifierCardParserOption : ParserOption
{
    public GeneralModifierCardParserOption()
    {
        NextOption = null;
    }

    public override Card TryParse(string[] values, string ID)
    {
        if (ID.Equals("3")) return GeneralModifierCard.TryParse(values);
        if (NextOption != null) return NextOption.TryParse(values, ID);
        return null;
    }
}