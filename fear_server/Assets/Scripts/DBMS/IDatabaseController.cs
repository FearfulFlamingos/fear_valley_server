using System.Collections.Generic;
using System.Data;

/// <summary>
/// All of the Database related scripts and functions are here.
/// </summary>
namespace Scripts.DBMS
{
    /// <summary>
    /// The Database controller is the main way to access the single database we created.
    /// It is easily extensible, with method definitions for three out of the four main operations:
    /// <para><see cref="Create(string)"/></para>
    /// <para><see cref="DatabaseController.Read(string)"/></para>
    /// <para><see cref="Update(string)"/></para>
    /// <para>Delete is covered by update, as they are functionally identical.</para>
    /// </summary>
    /// <remarks>
    /// More complex queries exist as methods. These are purpose built for the code that requires them,
    /// since all database interactions are kept in the same file.
    /// </remarks>
    public interface IDatabaseController
    {

        /// <summary>
        /// Adds the team Magic budget into the database. This should only be called once, ideally after the "Finish Build" button is clicked.
        /// </summary>
        /// <param name="team">Connection number to associate this with.</param>
        /// <param name="value">Amount of magic charges that the team purchased.</param>
        void AddMagicToDB(int team, int value);

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
        void AddTroopToDB(int teamNum, string troop, string weapon, string armor, float posX, float posZ, bool isLeader = false);
        
        /// <summary>
        /// Clears out the data from the previous game so no troops are left over.
        /// </summary>
        void ClearPreviousGameData();

        /// <summary>
        /// Closes the database connection. 
        /// </summary>
        void CloseDB();

        /// <summary>
        /// runs CREATE command. Used for "CREATE TABLE", etc.
        /// </summary>
        /// <param name="sql">Query to be executed.</param>
        /// <returns>IDataReader object with read() method to iterate through.</returns>
        IDataReader Create(string sql);

        /// <summary>
        /// Queries the DB and pulls all relevant info from tables.
        /// </summary>
        /// <returns>List of Troop objects for easy use.</returns>
        List<Troop> GetAllTroops();

        /// <summary>
        /// Reads the Magic table in its entirety and returns it as a dictionary.
        /// </summary>
        /// <returns>Dictionary object of ConnectionID to Magic Charge amount.</returns>
        Dictionary<int, int> GetMagic();

        /// <summary>
        /// Opens the Database connection.
        /// </summary>
        void OpenDB();

        /// <summary>
        /// Runs an UPDATE query. Use for DELETE and UPDATE.
        /// </summary>
        /// <param name="sql">Query to be executed.</param>
        void Update(string sql);
    }
}