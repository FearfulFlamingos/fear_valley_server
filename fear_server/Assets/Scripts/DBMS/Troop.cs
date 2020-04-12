namespace Scripts.DBMS
{
    /// <summary>
    /// Troop is a holder class between the network Server and the database.
    /// </summary>
    /// <remarks>
    /// The troop is the basic unit of the game, it contains all of the information needed for the client to
    /// rebuild the enemy's troops on thier machine.
    /// </remarks>
    public class Troop
    {
        /// <summary>Which client created the troop.</summary>
        public int TeamNum { get; set; }
        /// <summary>The kind of troop.</summary><remarks>E.G. Peasant, Trained Warrior, Magic User.</remarks>
        public string TroopType { get; set; }
        /// <summary>What level of armor the troop is wearing.</summary>
        public int Armor { get; set; }
        /// <summary>The bonus to the attack roll the troop makes.</summary>
        public int TroopAtkBonus { get; set; }
        /// <summary>The range at which a troop can attack.</summary>
        public int WeaponRange { get; set; }
        /// <summary>The size of the damage die that the troop uses.</summary>
        public int WeaponDamage { get; set; }
        /// <summary>How much health the troop has.</summary>
        public int Health { get; set; }
        /// <summary>The distance that the troop can move on thier turn.</summary>
        public int Movement { get; set; }
        /// <summary>Whether or not the troop is a leader.</summary><remarks>This is unused.</remarks>
        public bool Leader { get; set; }
        /// <summary>The x position that the troop was placed at, according to the client.</summary>
        public float XPos { get; set; }
        /// <summary>The z position that the troop was placed at, according to the client.</summary>
        public float ZPos { get; set; }
        /// <summary>The ID number of the troop, unique across all troops.</summary>
        public int TroopID { get; set; }
        /// <summary>The damage bonus a troop adds to thier damage roll.</summary>
        public int TroopDamageBonus { get; set; }
        
        /// <summary>
        /// Information about the troop. Useful for debugging.
        /// </summary>
        /// <returns>Human readable description of the troop.</returns>
        public override string ToString()
        {
            return $"\n<Troop>\nTeam={TeamNum}\nClass={TroopType}" +
                $"\nArmor={Armor}\nWMod={TroopAtkBonus}" +
                $"\nWDmg={WeaponDamage}\nHealth={Health}" +
                $"\nleader={Leader}\nXPos={XPos}\nZPos={ZPos}";
        }
    }
}