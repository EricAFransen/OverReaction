using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

public interface IParserOption
{
    int ID();
    ICard TryParse(int ID, int[] values);
}

public class InvalidCardException : Exception
{
    public string message { get; private set; }

    public InvalidCardException()
    {
        this.message = "";
    }

    public InvalidCardException(string message)
    {
        this.message = message;
    }
}

public class CardFactory
{
    private Dictionary<int, IParserOption> Parsers;

    public CardFactory()
    {
        Parsers = new Dictionary<int, IParserOption>();
    }

    public void AddParser(IParserOption option)
    {
        Parsers.Add(option.ID(), option);
    }

    public void AutoAddParsers()
    {
        // Get the assembly that contains IParserOption
        Assembly assembly = Assembly.GetAssembly(typeof(IParserOption));
        // Create an array of all the types in the assembly
        Type[] AllTypes = assembly.GetTypes();
        // Create a list for the types that inherit IParserOption
        List<Type> ParserTypes = new List<Type>();
        // Get the type of IParserOption
        Type ParserType = typeof(IParserOption);

        // Iterate through all of the types
        foreach(Type type in AllTypes)
        {
            if (
                // Make sure type inherits ParserType
                ParserType.IsAssignableFrom(type) &&
                // Make sure type isn't an interface
                !type.IsInterface &&
                // Make sure type isn't abstract
                !type.IsAbstract)
                // Add the type to the list of ParserTypes
                ParserTypes.Add(type);
        }

        // Iterate through each Parser Type
        foreach(Type type in ParserTypes)
        {
            // Create an instance of each option
            IParserOption parserOption = (IParserOption) Activator.CreateInstance(type);
            // Add the option to the factory
            AddParser(parserOption);
        }
    }

    public ICard ParseLine(string Line)
    {
        Line = Line.Replace("\r", "");
        string[] splitLine = Line.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        int[] values = Array.ConvertAll<string, int>(splitLine, Convert.ToInt32);
        int ID = values[0];
        if(Parsers.ContainsKey(ID))
        {
            return Parsers[ID].TryParse(ID, values);
        } else
        {
            throw new InvalidCardException("Card factory did not contain a parser for ID " + ID);
        }
    }

    public List<ICard> ParseDeck(string source, bool IsFile)
    {
        if (IsFile)
        {
            using (System.IO.StreamReader sr = new System.IO.StreamReader(source, System.Text.Encoding.UTF8))
            {
                source = sr.ReadToEnd();
            }
        }

        string[] splitSource = source.Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        List<ICard> cards = new List<ICard>();
        for(int i = 0; i < splitSource.Length; i++)
        {
            try
            {
                cards.Add(ParseLine(splitSource[i]));
            } catch (InvalidCardException e)
            {
                Debug.LogError("Error parsing line " + i + " of the deck.\n" +
                    "Line: " + splitSource[i] + "\n" +
                    "Error: " + e.message);
            }
        }
        return cards;
    }
}
