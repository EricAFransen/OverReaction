using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Overreaction
{
    class ChemicalReaction
    {
        /// <summary>
        /// Reactamts = 1d array, ith element represents the how many of the ith species it needs to react
        /// Products = 1d array, ith element reprsents how many of the ith species it produces.
        /// numSpecies = Should be set to a system's species array that tracks 
        /// </summary>
        int[] reactants, products, numSpecies;
        int rate;

        public ChemicalReaction(int[] givenReactants, int[] givenProducts, int givenRate, int[] givenNumSpecies)
        {
            Reactants = givenReactants;
            products = givenProducts;
            rate = givenRate;
            numSpecies = givenNumSpecies;
        }

        /// <summary>
        /// Calculates the rate expression for the reaction.
        /// </summary>
        /// <returns>The rate at which this reaction occurs, equal to |reactant 1|*|reactant 2|*...*k</returns>
        public int RateExpression()
        {
            int retVal = rate;
            for (int i = 0; i < reactants.Length; i++)
            {
                if (reactants[i] > 0)
                {
                    retVal *= Math.Pow(numSpecies[i], reactants[i]);
                }
            }
            return retVal;
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

        public static void InitReactions(List<ChemicalReaction> reactions, int[] species)
        {
            //Red + Blue -> x1;
            reactions.Add(new ChemicalReaction(new int[] { 1, 1, 0, 0, 0 }, new int[] { 0, 0, 1, 0, 0 }, 1, species));

            //x1 -> Red + Blue
            reactions.Add(new ChemicalReaction(new int[] { 0, 0, 1, 0, 0 }, new int[] { 1, 1, 0, 0, 0 }, 1, species));

            //x1 -> Red + x2;
            reactions.Add(new ChemicalReaction(new int[] { 0, 0, 1, 0, 0 }, new int[] { 1, 0, 0, 1, 0 }, 1, species));
        }

        public int[] Reactants { get => reactants; set => reactants = value; }
        public int Rate { get => rate; set => rate = value; }
        public int[] Products { get => products; set => products = value; }
    }
}
