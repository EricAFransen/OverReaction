All of the cards are stored in CSV, with the ID being the first value.
Unless stated otherwise, all values are integers.
Current list of card types:
Default card, Reaction card, Rate Modifier Card, General Modifier Card

Card format breakdown:

Default Card, ID = 0
$ID,$Cost,$Duration

Reaction Card, ID = 1
$ID,$Cost,$Duration,$Rate,$NumSpecies,$Reactants,$Products
Note: Reactants and Products are both arrays with $NumSpecies elements

Rate Modifier Card, ID = 2
$ID,$Cost,$Duration,$Numerator,$Denominator

General Modifier Card, ID = 3
$ID,$Cost,$Duration,$Type,$Numerator,$Denominator
Note: Type is an enumerated int
0 = TotalRate, 1 = ReactionRate, 2 = Time, 3 = Reactant, 4 = Product