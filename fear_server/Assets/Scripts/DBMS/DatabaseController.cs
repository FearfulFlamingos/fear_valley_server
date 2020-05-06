using System.Collections.Generic;
using UnityEngine;
using System.Data;
using Mono.Data.Sqlite;

namespace Scripts.DBMS
{
    /// <inheritdoc cref="IDatabaseController"/>
    public class DatabaseController : IDatabaseController
    {
        private readonly string dbpath = "URI=file:" + Application.persistentDataPath + "/fearful_data.sqlite";
        public static IDbConnection dbcon;

        /// <summary>
        /// Creates a new database connection and opens it.
        /// </summary>
        public DatabaseController()
        {
            Debug.Log(dbpath);
            if (!System.IO.File.Exists(Application.persistentDataPath + "/fearful_data.sqlite"))
            {
                Debug.Log("Creating database");
                CreateDatabase();
            }
            else
            {
                dbcon = new SqliteConnection(dbpath);
            }
            OpenDB();
        }

        // Creates a new database if a file isn't found in persistent storage.
        private void CreateDatabase()
        {
            SqliteConnection.CreateFile(Application.persistentDataPath + "/fearful_data.sqlite");
            dbcon = new SqliteConnection("URI=file:" + Application.persistentDataPath + "/fearful_data.sqlite;Version=3;");
            dbcon.Open();
            Create("CREATE TABLE Armor (" +
                    "id integer PRIMARY KEY," +
                    "armor text NOT NULL," +
                    "bonus float," +
                    "stealth float" +
                    ", cost float)");
            Update("INSERT INTO Armor (id, armor, bonus, stealth, cost) VALUES('1', 'LightMundaneArmor', '3.0', '0.0', '20.0');");
            Update("INSERT INTO Armor (id, armor, bonus, stealth, cost) VALUES('2', 'LightMagicalArmor', '3.0', '1.0', '40.0');");
            Update("INSERT INTO Armor (id, armor, bonus, stealth, cost) VALUES('3', 'HeavyMundaneArmor', '5.0', '-2.0', '30.0');");
            Update("INSERT INTO Armor (id, armor, bonus, stealth, cost) VALUES('4', 'HeavyMagicalArmor', '5.0', '-1.0', '50.0');");
            Update("INSERT INTO Armor (id, armor, bonus, stealth, cost) VALUES('5', 'Unarmored', '0.0', '2.0', '0.0');");

            Create("CREATE TABLE Army(" +
                "id integer PRIMARY KEY," +
                "teamNumber INTEGER," +
                "class TEXT," +
                "armor text," +
                "shield text," +
                "weapon TEXT," +
                "currentHealth INTEGER," +
                "isLeader BOOLEAN" +
                ", pos_x FLOAT, pox_z FLOAT)");
            Update("INSERT INTO Army (id, teamNumber, class, armor, shield, weapon, currentHealth, isLeader) VALUES ('1','1','Peasant','Light mundane armor','','Unarmed','10','false');");

            Create("CREATE TABLE Magic (" +
                   "teamNum INTEGER," +
                   "spellNum INTEGER)");

            Create("CREATE TABLE Troop (" +
                   "id integer PRIMARY KEY," +
                   "class text NOT NULL," +
                   "cost float," +
                   "health float," +
                   "attack float," +
                   "damage float," +
                   "movement float," +
                   "perception float, magicattack float, magicDamage float)");

            Update("INSERT INTO Troop(id, class, cost, health, attack, damage, movement, perception, magicattack, magicDamage) VALUES('1', 'Peasant', '10.0', '6.0', '4.0', '0.0', '4.0', '10.0', '', '');");
            Update("INSERT INTO Troop(id, class, cost, health, attack, damage, movement, perception, magicattack, magicDamage) VALUES('2', 'TrainedWarrior', '50.0', '16.0', '6.0', '2.0', '6.0', '12.0', '', '');");
            Update("INSERT INTO Troop(id, class, cost, health, attack, damage, movement, perception, magicattack, magicDamage) VALUES('3', 'MagicUser', '100.0', '10.0', '2.0', '-1.0', '4.0', '12.0', '5.0', '0.0');");

            Create("CREATE TABLE Weapon (" +
                   "id integer PRIMARY KEY," +
                   "name text NOT NULL," +
                   "cost float," +
                   "damage float," +
                   "attack float," +
                   "range float" +
                   ", AOE float)");

            Update("INSERT INTO Weapon(id, name, cost, damage, attack, range, AOE) VALUES('1', 'Unarmed', '0.0', '1.0', '1.0', '3.0', '1.0');");
            Update("INSERT INTO Weapon(id, name, cost, damage, attack, range, AOE) VALUES('2', 'Polearm', '10.0', '6.0', '1.0', '3.0', '1.0');");
            Update("INSERT INTO Weapon(id, name, cost, damage, attack, range, AOE) VALUES('3', 'TwoHandedWeapon', '20.0', '10.0', '1.0', '3.0', '1.0');");
            Update("INSERT INTO Weapon(id, name, cost, damage, attack, range, AOE) VALUES('4', 'OneHandedWeapon', '15.0', '8.0', '1.0', '3.0', '1.0');");
            Update("INSERT INTO Weapon(id, name, cost, damage, attack, range, AOE) VALUES('5', 'RangedAttack', '25.0', '6.0', '2.0', '8.0', '1.0');");
            Update("INSERT INTO Weapon(id, name, cost, damage, attack, range, AOE) VALUES('6', 'MagicalExplosion', '10.0', '12.0', '3.0', '10.0', '4.0');");
            CloseDB();
        }

        /// <inheritdoc cref="IDatabaseController.OpenDB"/>
        public void OpenDB()
        {
            dbcon.Open();
        }

        #region Basic Commands
        /// <inheritdoc cref="IDatabaseController.Create(string)"/>
        public IDataReader Create(string sql)
        {
            IDbCommand dbcmd = dbcon.CreateCommand();
            dbcmd.CommandText = sql;
            return dbcmd.ExecuteReader();

        }
        // <summary>
        // Internal function to build more complicated, repeated queries.
        // </summary>
        // <param name="sql">Query to be executed.</param>
        // <returns>IDataReader object with read() method.</returns>
        private IDataReader Read(string sql)
        {
            IDbCommand dbcmd = dbcon.CreateCommand();
            dbcmd.CommandText = sql;
            return dbcmd.ExecuteReader();
        }

        /// <inheritdoc cref="IDatabaseController.Update(string)"/>
        public void Update(string sql)
        {
            IDbCommand cmd = dbcon.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
        }

        /// <inheritdoc cref="IDatabaseController.CloseDB"/>
        public void CloseDB()
        {
            dbcon.Dispose();
            dbcon.Close();
        }
        #endregion

        #region Complex Queries

        /// <inheritdoc cref="IDatabaseController.ClearPreviousGameData"/>
        public void ClearPreviousGameData()
        {
            Update("DELETE FROM Army;");
            Update("DELETE FROM Magic;");
        }

        /// <inheritdoc cref="IDatabaseController.AddTroopToDB(int, string, string, string, float, float, bool)"/>
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

        /// <inheritdoc cref="IDatabaseController.GetAllTroops"/>
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
                    WeaponDamage = (int)troops.GetDouble(13),
                    TroopDamageBonus = (int)troops.GetDouble(9),
                    Health = (int)troops.GetDouble(7),
                    Movement = (int)troops.GetDouble(10),
                    Leader = false, // troops.GetBoolean(4),
                    XPos = troops.GetFloat(5),
                    ZPos = troops.GetFloat(6)

                };
                allTroops.Add(t);
            }

            //Debug.Log($"DB Records = {allTroops.Count}");
            return allTroops;
        }

        /// <inheritdoc cref="IDatabaseController.AddMagicToDB(int, int)"/>
        public void AddMagicToDB(int team, int value)
        {
            Update("INSERT INTO Magic " +
                "VALUES (" +
                $"{team}, " +
                $"{value});");
        }

        /// <inheritdoc cref="IDatabaseController.GetMagic"/>
        public Dictionary<int, int> GetMagic()
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