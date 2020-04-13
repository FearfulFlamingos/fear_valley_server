using System.Collections.Generic;
using System.Data;

namespace Scripts.DBMS
{
    public interface IDatabaseController
    {
        void AddMagicToDB(int team, int value);
        void AddTroopToDB(int teamNum, string troop, string weapon, string armor, float posX, float posZ, bool isLeader = false);
        void CloseDB();
        IDataReader Create(string sql);
        List<Troop> GetAllTroops();
        void OpenDB();
        Dictionary<int, int> GetMagic();
        void Update(string sql);
        void ClearPreviousGameData();
    }
}