using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FearValleyNetwork;
using Scripts.DBMS;
using Scripts.Networking;
using NUnit.Framework;
using NSubstitute;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.TestTools;

namespace EditorTests
{
    public class TestDatabaseController : MonoBehaviour
    {
        DatabaseController db;
        
        [SetUp]
        public void Setup()
        {
            db = new DatabaseController();
        }


        [Test]
        public void TestClearPreviousGameData()
        {
            // Arrange
            // Fake data insert
            db.AddTroopToDB(
                1,
                "Peasant",
                "Unarmed",
                "Unarmored",
                1.0f,
                1.0f
                );
            // Act
            db.ClearPreviousGameData();
        }

        [Test]
        public void TestAddTroopToDB()
        {
            // Arrange
            db.ClearPreviousGameData();
            db.AddTroopToDB(1, "Peasant", "Unarmed", "Unarmored", 1f, 2f);

            // Assert
            List<Troop> troops = db.GetAllTroops();
            Assert.AreEqual(1, troops.Count);
        }

        [Test]
        public void TestGetAllTroops()
        {
            // Arrange
            db.ClearPreviousGameData();
            db.AddTroopToDB(1, "Peasant", "Unarmed", "Unarmored", 1f, 2f);
            db.AddTroopToDB(1, "Peasant", "Unarmed", "Unarmored", 1f, 2f);
            db.AddTroopToDB(1, "Peasant", "Unarmed", "Unarmored", 1f, 2f);
            // Act
            List<Troop> troops = db.GetAllTroops();

            // Assert
            Assert.AreEqual(3, troops.Count);
        }

        [Test]
        public void TestAddMagic()
        {
            // Arrange
            db.ClearPreviousGameData();

            // Act
            db.AddMagicToDB(1, 3);
            // Assert
        }

        [Test]
        public void TestReadMagic()
        {
            // Arrange
            db.ClearPreviousGameData();
            db.AddMagicToDB(1, 3);
            db.AddMagicToDB(2, 5);
            // Act
            Dictionary<int, int> magic = db.GetMagic();

            // Assert
            Assert.AreEqual(2, magic.Keys.Count);

        }                

        [TearDown]
        public void TearDown()
        {
            db.CloseDB();
        }

    }
}

/* Tests To Write:
 *   CTor() => no file exists
 *   CTor() => File exists
 *   OpenDB()
 *   Create()
 *   Update()
 *   CloseDB()
 *   ClearPreviousGameData()
 *   AddTroopToDB()
 *   GetAllTroops()
 *   AddMagicToDB()
 *   GetMagic()
 */