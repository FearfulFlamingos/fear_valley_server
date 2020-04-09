using System.Collections.Generic;
using UnityEngine;
using System.Data;
using Mono.Data.Sqlite;

namespace Scripts.DBMS
{
    /// <summary>
    /// Class makes database interactions simpler.
    /// </summary>
    public class DatabaseController
    {
        //public static DatabaseController DBInstance { private set; get; }

        private readonly string dbpath = "URI=file:" + Application.dataPath + "/Data/fearful_data.sqlite";
        private IDbConnection dbcon;

        /// <summary>
        /// Constructor for class. Opens a connection to our database.
        /// </summary>
        public DatabaseController()
        {
            // connection created in instance
            dbcon = new SqliteConnection(dbpath);
            dbcon.Open();
        }

        public void OpenDB()
        {
            dbcon.Open();
        }
        #region Basic Commands
        /// <summary>
        /// runs CREATE command. Used for "CREATE TABLE", etc.
        /// </summary>
        /// <param name="sql">Query to be executed.</param>
        /// <returns>IDataReader object with read() method to iterate through.</returns>
        public IDataReader Create(string sql)
        {
            IDbCommand dbcmd = dbcon.CreateCommand();
            dbcmd.CommandText = sql;
            return dbcmd.ExecuteReader();

        }
        /// <summary>
        /// Internal function to build more complicated, repeated queries.
        /// </summary>
        /// <param name="sql">Query to be executed.</param>
        /// <returns>IDataReader object with read() method.</returns>
        private IDataReader Read(string sql)
        {
            IDbCommand dbcmd = dbcon.CreateCommand();
            dbcmd.CommandText = sql;
            return dbcmd.ExecuteReader();
        }

        /// <summary>
        /// Runs an UPDATE query. Use for DELETE and UPDATE.
        /// </summary>
        /// <param name="sql">Query to be executed.</param>
        public void Update(string sql)
        {
            IDbCommand cmd = dbcon.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Closes the database connection. 
        /// </summary>
        public void CloseDB()
        {
            dbcon.Close();
        }
        #endregion

        #region Complex Queries
        /// <summary>
        /// Adds a troop to the database. Assumes the client has enough money.
        /// </summary>
        /// <remarks>
        /// The important thing to notice here is that all characters have a unique ID. That means that the client needs to check
        /// both dictionaries for the TroopID when deleting.
        /// </remarks>
        /// <param name="teamNum">Player connection id</param>
        /// <param name="troop">Class.</param>
        /// <param name="weapon">Name of weapon.</param>
        /// <param name="armor">Name of armor.</param>
        /// <param name="posX">Relative X position on board.</param>
        /// <param name="posZ">Relative Z position on board.</param>
        /// <param name="isLeader">Leader (default false).</param>
        public void AddTroopToDB(int teamNum, string troop, string weapon, string armor, float posX, float posZ, bool isLeader = false)
        {
            Update("INSERT INTO Army " +
                "(teamNumber,class,armor,weapon,isLeader,pos_x,pox_z) " +
                "VALUES (" +
                $"{teamNum}," +
                $"'{troop}'," +
                $"'{armor}'," +
                $"'{weapon}'," +
                $"'{isLeader}'," +
                $"{posX}," +
                $"{posZ});");
        }
        /// <summary>
        /// Queries the DB and pulls all relevant info from tables.
        /// </summary>
        /// <returns>List of Troop objects for easy use</returns>
        public List<Troop> GetAllTroops()
        {
            List<Troop> allTroops = new List<Troop>();
            IDataReader troops = Read("SELECT " +
                "Ay.id, Ay.teamNumber, Ay.class, Ay.weapon, Ay.isLeader, Ay.pos_x, Ay.pox_z, " +//6 
                "T.health, T.attack, T.damage, T.movement, T.perception, T.magicattack, " +//12 
                "W.damage, W.range, W.AOE, " +//15
                "Ar.bonus, Ar.stealth " +//17
                "" +
                "FROM Army Ay " +
                "INNER JOIN Troop T ON Ay.class = T.class " +
                "INNER JOIN Weapon W ON Ay.weapon = W.name " +
                "INNER JOIN Armor Ar ON Ay.armor = Ar.armor;");
            while (troops.Read())
            {
                Troop t = new Troop()
                {
                    TroopID = troops.GetInt32(0),
                    TeamNum = troops.GetInt32(1), 
                    TroopType = troops.GetString(2), 
                    Armor = (int)troops.GetDouble(16) + 10, 
                    TroopAtkBonus = (int)troops.GetDouble(8), 
                    WeaponRange = (int)troops.GetDouble(14), 
                    WeaponDamage = (int)troops.GetDouble(13), //WeapDmg
                    TroopDamageBonus = (int)troops.GetDouble(9),
                    Health = (int)troops.GetDouble(7), 
                    Movement = (int)troops.GetDouble(10), 
                    Leader = false,//troops.GetBoolean(4),
                    XPos = troops.GetFloat(5),
                    ZPos = troops.GetFloat(6)

            };
                allTroops.Add(t);
            }

            //Debug.Log($"DB Records = {allTroops.Count}");
            return allTroops;
        }

        /// <summary>
        /// Adds the team Magic budget into the database. This should only be called once, ideally after the "Finish Build" button is clicked.
        /// </summary>
        /// <param name="team">Connection number to associate this with.</param>
        /// <param name="value">Amount of magic charges that the team purchased.</param>
        public void AddMagicToDB(int team, int value)
        {
            Update("INSERT INTO Magic " +
                "VALUES (" +
                $"{team}, " +
                $"{value});");
        }

        /// <summary>
        /// Reads the Magic table in its entirety and returns it as a dictionary.
        /// </summary>
        /// <returns>Dictionary object of ConnectionID to Magic Charge amount.</returns>
        public Dictionary<int, int> ReadMagicFromDB()
        {
            Dictionary<int, int> magic = new Dictionary<int, int>();
            IDataReader magicInDB = Read("SELECT * FROM Magic;");
            while (magicInDB.Read())
            {
                magic.Add(
                    magicInDB.GetInt32(0),
                    magicInDB.GetInt32(1));
            }

            return magic;
        }
        #endregion
    }
}