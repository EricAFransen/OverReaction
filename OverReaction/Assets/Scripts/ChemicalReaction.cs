using UnityEngine;
using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;


public class ChemicalReaction
{
    /// <summary>
    /// Reactants = 1d array, ith element represents the how many of the ith species it needs to react
    /// Products = 1d array, ith element reprsents how many of the ith species it produces.
    /// numSpecies = Should be set to a system's species array that tracks 
    /// </summary>
    public int[] reactants, products, numSpecies;
    public int rate;
    public double time;
    readonly float ReactionTime = 10.0f;

    public ChemicalReaction(int[] givenReactants, int[] givenProducts, int givenRate, int[] givenNumSpecies)
    {
        reactants = givenReactants;
        products = givenProducts;
        rate = givenRate;
        numSpecies = givenNumSpecies;
        time = ReactionTime;
    }

    //semi-random chemical reaction
    public ChemicalReaction(System.Random rand, int givenRate, int[] givenNumSpecies)
    {
        rate = givenRate;
        numSpecies = givenNumSpecies;
        time = ReactionTime;
        List<int> l = new List<int>();
        for (int i = 0; i < givenNumSpecies.Length; i++)
        {
            l.Add(i);
        }

        int f = rand.Next() % 3;
        reactants = new int[givenNumSpecies.Length];
        products = new int[givenNumSpecies.Length];
        int x;

        //2x -> y + z
        if (f == 0)
        {
            x = rand.Next() % l.Count;
            reactants[l[x]] = 2;
            l.RemoveAt(x);
            x = rand.Next() % l.Count;
            products[l[x]] = 1;
            l.RemoveAt(x);
            x = rand.Next() % l.Count;
            products[l[x]] = 1;
            l.RemoveAt(x);
        }

        //x + y -> 2z
        else if (f == 1)
        {
            x = rand.Next() % l.Count;
            reactants[l[x]] = 1;
            l.RemoveAt(x);
            x = rand.Next() % l.Count;
            reactants[l[x]] = 1;
            l.RemoveAt(x);
            x = rand.Next() % l.Count;
            products[l[x]] = 2;
            l.RemoveAt(x);
        }

        //x + y -> w + z
        else if (f == 2)
        {
            x = rand.Next() % l.Count;
            reactants[l[x]] = 1;
            l.RemoveAt(x);
            x = rand.Next() % l.Count;
            reactants[l[x]] = 1;
            l.RemoveAt(x);
            x = rand.Next() % l.Count;
            products[l[x]] = 1;
            l.RemoveAt(x);
            x = rand.Next() % l.Count;
            products[l[x]] = 1;
            l.RemoveAt(x);
        }
    }


    /// <summary>
    /// Calculates the rate expression for the reaction.
    /// </summary>
    /// <returns>The rate at which this reaction occurs, equal to |reactant 1|*|reactant 2|*...*k</returns>
    public double RateExpression()
    {
        int retVal = rate;
        int totalSpecies = 0;
        for (int i = 0; i < reactants.Length; i++)
        {
            if (reactants[i] > 0)
            {
                for(int j = 0; j < reactants[i]; j++)
                {
                    //If there are multiple of the same molecule, 
                    //then the molecules already counted in the rate cannot becount again
                    retVal *= (numSpecies[i] - j);
                }
            }

            //Keep track of the total species (aka volume)
            totalSpecies += numSpecies[i];
        }

        return (double)retVal; // (double) totalSpecies;
    }

    public string Equation()
    {
        string ret = "";

        ret += "" + reactants[0] + "Red + ";
        ret += "" + reactants[1] + "Blue + ";
        ret += "" + reactants[2] + "x1 + ";
        ret += "" + reactants[3] + "x2 + ";
        ret += "" + reactants[4] + "x3";
        ret += " -" + rate + "> ";

        ret += "" + products[0] + "Red + ";
        ret += "" + products[1] + "Blue + ";
        ret += "" + products[2] + "x1 + ";
        ret += "" + products[3] + "x2 + ";
        ret += "" + products[4] + "x3";

        return ret;
    }
    public string EquationMinimal()
    {
        List<int> r = new List<int>();
        List<int> p = new List<int>();
        for (int i = 0; i < products.Length; i++)
        {
            if (reactants[i] > 0)
            {
                r.Add(i);
            }
            if (products[i] > 0)
            {
                p.Add(i);
            }
        }

        string ret = "";
        int rc = r.Count;
        if (r.Contains(0))
        {
            ret += "" + reactants[0] + "Red";
            if (rc-- > 1) ret += " + ";
        }

        if (r.Contains(1))
        {
            ret += "" + reactants[1] + "Blue";
            if (rc-- > 1) ret += " + ";
        }

        if (r.Contains(2))
        { 
            ret += "" + reactants[2] + "x1";
            if (rc-- > 1) ret += " + ";

        }

        if (r.Contains(3))
        {
            ret += "" + reactants[3] + "x2";
            if (rc-- > 1) ret += " + ";
        }

        if (r.Contains(4))
        {
            ret += "" + reactants[4] + "x3";
        }

        ret += " -" + rate + "> ";


        int pc = p.Count;
        if (p.Contains(0))
        {
            ret += "" + products[0] + "Red";
            if (pc-- > 1) ret += " + ";
        }

        if (p.Contains(1))
        {
            ret += "" + products[1] + "Blue";
            if (pc-- > 1) ret += " + ";
        }

        if (p.Contains(2))
        {
            ret += "" + products[2] + "x1";
            if (pc-- > 1) ret += " + ";

        }

        if (p.Contains(3))
        {
            ret += "" + products[3] + "x2";
            if (pc-- > 1) ret += " + ";
        }

        if (p.Contains(4))
        {
            ret += "" + products[4] + "x3";
        }

        return ret;
    }

    public static void InitReactions(List<ChemicalReaction> reactions, int[] species)
    {
        //Red + Blue -> x1;
        reactions.Add(new ChemicalReaction(new int[] { 1, 1, 0, 0, 0 }, new int[] { 0, 0, 1, 0, 0 }, 1, species));

        //x1 -> Red + Blue
        reactions.Add(new ChemicalReaction(new int[] { 0, 0, 1, 0, 0 }, new int[] { 1, 1, 0, 0, 0 }, 1, species));

        //x1 -> Red + x2;
        reactions.Add(new ChemicalReaction(new int[] { 0, 0, 1, 0, 0 }, new int[] { 1, 0, 0, 1, 0 }, 1, species));
    }

    public void passTime(double t)
    {
        //Debug.Log(t + " is passed in");
        //Debug.Log(time + " before");
        time -= t;
        //Debug.Log(time + " second remain");
    }

    public void addBonusTime(double t)
    {
        time += t;
    }

    public bool isExpired()
    {
        //if (0 > time) Debug.Log("Is expired");

        return (0 > time);
    }

    public void multRate(double m)
    {
        rate += rate;
    }

    /* //storage in case unity updates to C# version 7
    public int[] Reactants { get => reactants; set => reactants = value; }
    public int Rate { get => rate; set => rate = value; }
    public int[] Products { get => products; set => products = value; }
    */

    public List<ChemicalReaction> getDeck(int[] species)
    {
        List<ChemicalReaction> list = new List<ChemicalReaction>();
        /**/
        list.Add(new ChemicalReaction(new int[] { 1, 0, 1, 0, 0 }, new int[] { 0, 0, 0, 1, 1 }, 1, species));
        list.Add(new ChemicalReaction(new int[] { 0, 1, 0, 0, 1 }, new int[] { 0, 0, 1, 1, 0 }, 1, species));
        list.Add(new ChemicalReaction(new int[] { 1, 1, 0, 0, 0 }, new int[] { 0, 0, 0, 2, 0 }, 1, species));
        list.Add(new ChemicalReaction(new int[] { 0, 0, 1, 1, 0 }, new int[] { 1, 1, 0, 0, 0 }, 1, species));
        list.Add(new ChemicalReaction(new int[] { 1, 1, 0, 0, 0 }, new int[] { 0, 0, 1, 0, 1 }, 1, species));
        list.Add(new ChemicalReaction(new int[] { 0, 0, 0, 1, 1 }, new int[] { 1, 0, 1, 0, 0 }, 1, species));
        
        list.Add(new ChemicalReaction(new int[] { 0, 0, 1, 1, 0 }, new int[] { 0, 1, 0, 0, 1 }, 1, species));
        list.Add(new ChemicalReaction(new int[] { 0, 1, 0, 1, 0 }, new int[] { 1, 0, 1, 0, 0 }, 1, species));
        list.Add(new ChemicalReaction(new int[] { 0, 0, 0, 1, 1 }, new int[] { 0, 1, 1, 0, 0 }, 1, species));
        list.Add(new ChemicalReaction(new int[] { 0, 1, 1, 0, 0 }, new int[] { 0, 0, 0, 1, 1 }, 1, species));
        list.Add(new ChemicalReaction(new int[] { 0, 0, 0, 1, 1 }, new int[] { 1, 1, 0, 0, 0 }, 1, species));
        list.Add(new ChemicalReaction(new int[] { 0, 1, 1, 1, 0 }, new int[] { 1, 0, 0, 0, 2 }, 1, species));
        list.Add(new ChemicalReaction(new int[] { 0, 0, 1, 1, 1 }, new int[] { 3, 0, 0, 0, 0 }, 1, species));
        list.Add(new ChemicalReaction(new int[] { 0, 0, 1, 1, 1 }, new int[] { 3, 0, 0, 0, 0 }, 1, species));
        

        list.Add(new ChemicalReaction(new int[] { 1, 1, 0, 0, 0 }, new int[] { 0, 0, 0, 0, 0 }, 1, species));
        list.Add(new ChemicalReaction(new int[] { 0, 0, 0, 0, 0 }, new int[] { 1, 0, 0, 0, 0 }, 1, species));
        list.Add(new ChemicalReaction(new int[] { 0, 0, 0, 0, 0 }, new int[] { 0, 1, 0, 0, 0 }, 1, species));
        list.Add(new ChemicalReaction(new int[] { 1, 0, 0, 0, 0 }, new int[] { 0, 0, 0, 0, 0 }, 1, species));
        list.Add(new ChemicalReaction(new int[] { 0, 1, 0, 0, 0 }, new int[] { 0, 0, 0, 0, 0 }, 1, species));

        return list;
    }

    public bool Equals(ChemicalReaction obj)
    {
        if((obj == null) || !this.GetType().Equals(obj.GetType()))
        {
            return false;
        }
        else
        {
            ChemicalReaction r = (ChemicalReaction) obj;

            bool shareReactants = true;
            bool shareProducts = true;
            for(int i = 0; i < reactants.Length; i++)
            {
                if (this.products[i] != r.products[i])
                    shareProducts = false;

                if (this.reactants[i] != r.reactants[i])
                    shareReactants = false;
            }

            return ((r.rate == this.rate) && shareReactants && shareProducts);
        }
    }
}

