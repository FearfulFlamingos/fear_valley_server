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

            // Assert

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