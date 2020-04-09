namespace Scripts.DBMS
{
    public class Troop
    {
        public string PRINT()
        {
            return $"\n<Troop>\nTeam={TeamNum}\nClass={TroopType}" +
                $"\nArmor={Armor}\nWMod={TroopAtkBonus}" +
                $"\nWDmg={WeaponDamage}\nHealth={Health}" +
                $"\nleader={Leader}\nXPos={XPos}\nZPos={ZPos}";
        }


        #region Getters and Setters
        // call with:
        //    Troop instance = new Troop(...);
        //    int team = instance.TeamNum;
        //    instance.TeamNum = 8;
        public int TeamNum { get; set; }
        public string TroopType { get; set; }
        public int Armor { get; set; }
        public int TroopAtkBonus { get; set; }
        public int WeaponRange { get; set; }
        public int WeaponDamage { get; set; }
        public int Health { get; set; }
        public int Movement { get; set; }
        public bool Leader { get; set; }
        public float XPos { get; set; }
        public float ZPos { get; set; }
        public int TroopID { get; set; }
        public int TroopDamageBonus { get; set; }
        #endregion
    }
}