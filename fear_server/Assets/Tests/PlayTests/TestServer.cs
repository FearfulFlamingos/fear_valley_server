using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using NSubstitute;
using Scripts.DBMS;
using Scripts.Networking;
using FearValleyNetwork;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.Networking;

namespace PlayTests
{
    public class TestClient
    {
        GameObject serverObj;

        [OneTimeSetUp]
        public void CreateServerGameObject()
        {
            Time.timeScale = 20f;
            serverObj = new GameObject("Server");
            serverObj.AddComponent<MonoServer>();
        }

        [SetUp]
        public void CreateServer()
        {
            MonoServer.Instance = new Server();
        }

        [UnityTest]
        public IEnumerator TestNet_AddTroop()
        {
            // Arrange
            IDatabaseController databaseController = Substitute.For<IDatabaseController>();
            MonoServer.Instance.dbCont = databaseController;
            
            Net_AddTroop message = new Net_AddTroop()
            {
                TroopType = "Peasant",
                WeaponType = "Unarmed",
                ArmorType = "Unarmored",
                XPosRelative = 1f,
                ZPosRelative = 1f
            };
            byte[] buffer = new byte[1024];
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream ms = new MemoryStream(buffer);
            formatter.Serialize(ms, message);

            // Act
            MonoServer.Instance.CheckMessageType(0, 1, 0, buffer, NetworkEventType.DataEvent);
            yield return null;

            // Assert
            databaseController.Received(1).AddTroopToDB(Arg.Any<int>(), 
                Arg.Any<string>(), 
                Arg.Any<string>(), 
                Arg.Any<string>(), 
                Arg.Any<float>(), 
                Arg.Any<float>());

            yield return null;
        }


        [UnityTest]
        public IEnumerator TestNet_EndTurn()
        {
            Net_EndTurn message = new Net_EndTurn();
            byte[] buffer = new byte[1024];
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream ms = new MemoryStream(buffer);
            formatter.Serialize(ms, message);

            // Act
            MonoServer.Instance.CheckMessageType(0, 1, 0, buffer, NetworkEventType.DataEvent);
            yield return null;

            // Assert
            Net_EndTurn actual = (Net_EndTurn)MonoServer.Instance.LastRecieved;
            Assert.AreEqual((byte)NetOP.Operation.EndTurn, actual.OperationCode);

            yield return null;
        }


        [UnityTest]
        public IEnumerator TestNet_RETREATFromConnection1()
        {
            Net_RETREAT message = new Net_RETREAT()
            {
                TroopID = 1
            };
            byte[] buffer = new byte[1024];
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream ms = new MemoryStream(buffer);
            formatter.Serialize(ms, message);

            // Act
            MonoServer.Instance.CheckMessageType(0, 1, 0, buffer, NetworkEventType.DataEvent);
            yield return null;

            // Assert
            Net_RETREAT actual = (Net_RETREAT)MonoServer.Instance.LastSentToClient;
            Assert.AreEqual((byte)NetOP.Operation.RETREAT, actual.OperationCode);
            Assert.AreEqual(2, MonoServer.Instance.LastClient);
            yield return null;
        }

        [UnityTest]
        public IEnumerator TestNet_RETREATFromConnection2()
        {
            Net_RETREAT message = new Net_RETREAT()
            {
                TroopID = 1
            };
            byte[] buffer = new byte[1024];
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream ms = new MemoryStream(buffer);
            formatter.Serialize(ms, message);

            // Act
            MonoServer.Instance.CheckMessageType(0, 2, 0, buffer, NetworkEventType.DataEvent);
            yield return null;

            // Assert
            Net_RETREAT actual = (Net_RETREAT)MonoServer.Instance.LastSentToClient;
            Assert.AreEqual((byte)NetOP.Operation.RETREAT, actual.OperationCode);
            Assert.AreEqual(1, MonoServer.Instance.LastClient);
            yield return null;
        }

        [UnityTest]
        public IEnumerator TestNet_RETREATFromConnection1ToKillEnemy()
        {
            Net_RETREAT message = new Net_RETREAT()
            {
                TroopID = 1,
                ForceEnemyToRetreat = true
            };
            byte[] buffer = new byte[1024];
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream ms = new MemoryStream(buffer);
            formatter.Serialize(ms, message);

            // Act
            MonoServer.Instance.CheckMessageType(0, 1, 0, buffer, NetworkEventType.DataEvent);
            yield return null;

            // Assert
            Net_RETREAT actual = (Net_RETREAT)MonoServer.Instance.LastSentToClient;
            Assert.AreEqual((byte)NetOP.Operation.RETREAT, actual.OperationCode);
            Assert.AreEqual(2, MonoServer.Instance.LastClient);
            yield return null;
        }

        [UnityTest]
        public IEnumerator TestNet_RETREATFromConnection2ToKillEnemy()
        {
            Net_RETREAT message = new Net_RETREAT()
            {
                TroopID = 1,
                ForceEnemyToRetreat = true
            };
            byte[] buffer = new byte[1024];
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream ms = new MemoryStream(buffer);
            formatter.Serialize(ms, message);

            // Act
            MonoServer.Instance.CheckMessageType(0, 2, 0, buffer, NetworkEventType.DataEvent);
            yield return null;

            // Assert
            Net_RETREAT actual = (Net_RETREAT)MonoServer.Instance.LastSentToClient;
            Assert.AreEqual((byte)NetOP.Operation.RETREAT, actual.OperationCode);
            Assert.AreEqual(1, MonoServer.Instance.LastClient);
            yield return null;
        }

        [UnityTest]
        public IEnumerator TestNet_RETREATFromUnknownConnection()
        {
            Net_RETREAT message = new Net_RETREAT()
            {
                TroopID = 1
            };
            byte[] buffer = new byte[1024];
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream ms = new MemoryStream(buffer);
            formatter.Serialize(ms, message);

            // Act
            MonoServer.Instance.CheckMessageType(0, 3, 0, buffer, NetworkEventType.DataEvent);
            yield return null;

            // Assert
            yield return null;
        }

        [UnityTest]
        public IEnumerator TestNet_ATTACKFromConnection1()
        {
            // Arrange
            Net_ATTACK message = new Net_ATTACK()
            {
                TroopID = 1
            };
            byte[] buffer = new byte[1024];
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream ms = new MemoryStream(buffer);
            formatter.Serialize(ms, message);

            // Act
            MonoServer.Instance.CheckMessageType(0, 1, 0, buffer, NetworkEventType.DataEvent);
            yield return null;

            // Assert
            Net_ATTACK actual = (Net_ATTACK)MonoServer.Instance.LastSentToClient;
            Assert.AreEqual((byte)NetOP.Operation.ATTACK, actual.OperationCode);
            Assert.AreEqual(2, MonoServer.Instance.LastClient);
            yield return null;
        }

        [UnityTest]
        public IEnumerator TestNet_ATTACKFromConnection2()
        {
            // Arrange
            Net_ATTACK message = new Net_ATTACK()
            {
                TroopID = 1
            };
            byte[] buffer = new byte[1024];
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream ms = new MemoryStream(buffer);
            formatter.Serialize(ms, message);

            // Act
            MonoServer.Instance.CheckMessageType(0, 2, 0, buffer, NetworkEventType.DataEvent);
            yield return null;

            // Assert
            Net_ATTACK actual = (Net_ATTACK)MonoServer.Instance.LastSentToClient;
            Assert.AreEqual((byte)NetOP.Operation.ATTACK, actual.OperationCode);
            Assert.AreEqual(1, MonoServer.Instance.LastClient);
            yield return null;
        }

        [UnityTest]
        public IEnumerator TestNet_ATTACKFromUnknownConnection()
        {
            // Arrange
            Net_ATTACK message = new Net_ATTACK()
            {
                TroopID = 1
            };
            byte[] buffer = new byte[1024];
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream ms = new MemoryStream(buffer);
            formatter.Serialize(ms, message);

            // Act
            MonoServer.Instance.CheckMessageType(0, 3, 0, buffer, NetworkEventType.DataEvent);
            yield return null;

            // Assert
            yield return null;
        }

        [UnityTest]
        public IEnumerator TestNET_FinishBuildWithWaitingConnections()
        {
            // Arrange
            IDatabaseController databaseController = Substitute.For<IDatabaseController>();
            
            MonoServer.Instance.dbCont = databaseController;

            Net_FinishBuild message = new Net_FinishBuild();
            byte[] buffer = new byte[1024];
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream ms = new MemoryStream(buffer);
            formatter.Serialize(ms, message);

            // Act
            MonoServer.Instance.CheckMessageType(0, 1, 0, buffer, NetworkEventType.DataEvent);
            yield return null;

            // Assert
            Net_ChangeScene actual = (Net_ChangeScene)MonoServer.Instance.LastSentToClient;
            databaseController.Received(1).AddMagicToDB(Arg.Any<int>(),Arg.Any<int>());
            Assert.AreEqual((byte)NetOP.Operation.ChangeScene, actual.OperationCode);
            Assert.AreEqual(1, MonoServer.Instance.waitingConnections);
            yield return null;
        }

        [UnityTest]
        public IEnumerator TestNET_FinishBuildWithNoWaitingConnections()
        {
            // Arrange
            IDatabaseController databaseController = Substitute.For<IDatabaseController>();
            databaseController.GetAllTroops().Returns(new List<Troop>());
            databaseController.GetMagic().Returns(new Dictionary<int, int>());
            MonoServer.Instance.dbCont = databaseController;
            MonoServer.Instance.waitingConnections = 1;

            Net_FinishBuild message = new Net_FinishBuild();
            byte[] buffer = new byte[1024];
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream ms = new MemoryStream(buffer);
            formatter.Serialize(ms, message);

            // Act
            MonoServer.Instance.CheckMessageType(0, 1, 0, buffer, NetworkEventType.DataEvent);
            yield return new WaitForSeconds(1);

            // Assert
            Net_ToggleControls actual = (Net_ToggleControls)MonoServer.Instance.LastSentToClient;
            databaseController.Received(1).AddMagicToDB(Arg.Any<int>(), Arg.Any<int>());
            Assert.AreEqual((byte)NetOP.Operation.ToggleControls, actual.OperationCode);
            Assert.AreEqual(0, MonoServer.Instance.waitingConnections);
            yield return null;
        }

        [UnityTest]
        public IEnumerator TestNet_MOVEFromConnection1()
        {
            // Arrange
            Net_MOVE message = new Net_MOVE()
            {
                TroopID = 1
            };
            byte[] buffer = new byte[1024];
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream ms = new MemoryStream(buffer);
            formatter.Serialize(ms, message);

            // Act
            MonoServer.Instance.CheckMessageType(0, 1, 0, buffer, NetworkEventType.DataEvent);
            yield return null;

            // Assert
            Net_MOVE actual = (Net_MOVE)MonoServer.Instance.LastSentToClient;
            Assert.AreEqual((byte)NetOP.Operation.MOVE, actual.OperationCode);
            Assert.AreEqual(2, MonoServer.Instance.LastClient);
            yield return null;
        }

        [UnityTest]
        public IEnumerator TestNet_MOVEFromConnection2()
        {
            // Arrange
            Net_MOVE message = new Net_MOVE()
            {
                TroopID = 1
            };
            byte[] buffer = new byte[1024];
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream ms = new MemoryStream(buffer);
            formatter.Serialize(ms, message);

            // Act
            MonoServer.Instance.CheckMessageType(0, 2, 0, buffer, NetworkEventType.DataEvent);
            yield return null;

            // Assert
            Net_MOVE actual = (Net_MOVE)MonoServer.Instance.LastSentToClient;
            Assert.AreEqual((byte)NetOP.Operation.MOVE, actual.OperationCode);
            Assert.AreEqual(1, MonoServer.Instance.LastClient);
            yield return null;
        }

        [UnityTest]
        public IEnumerator TestNet_MOVEFromUnknownConnection()
        {
            // Arrange
            Net_MOVE message = new Net_MOVE()
            {
                TroopID = 1
            };
            byte[] buffer = new byte[1024];
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream ms = new MemoryStream(buffer);
            formatter.Serialize(ms, message);

            // Act
            MonoServer.Instance.CheckMessageType(0, 3, 0, buffer, NetworkEventType.DataEvent);
            yield return null;

            // Assert
            yield return null;
        }

        [UnityTest]
        public IEnumerator TestChangeEnemyNameFromConnection1()
        {
            // Arrange
            Net_UpdateEnemyName message = new Net_UpdateEnemyName();
            byte[] buffer = new byte[1024];
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream ms = new MemoryStream(buffer);
            formatter.Serialize(ms, message);

            // Act
            MonoServer.Instance.CheckMessageType(0, 1, 0, buffer, NetworkEventType.DataEvent);
            yield return null;

            // Assert
            yield return null;
        }

        [UnityTest]
        public IEnumerator TestChangeEnemyNameFromConnection2()
        {
            // Arrange
            Net_UpdateEnemyName message = new Net_UpdateEnemyName();
            byte[] buffer = new byte[1024];
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream ms = new MemoryStream(buffer);
            formatter.Serialize(ms, message);

            // Act
            MonoServer.Instance.CheckMessageType(0, 2, 0, buffer, NetworkEventType.DataEvent);
            yield return null;

            // Assert
            yield return null;
        }

        [TearDown]
        public void Teardown()
        {
            MonoServer.Instance = null;
        }

    }
}
