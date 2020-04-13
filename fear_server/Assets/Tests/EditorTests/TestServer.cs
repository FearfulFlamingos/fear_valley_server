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
    public class TestServer : MonoBehaviour
    {
        GameObject serverObj;
        
        [OneTimeSetUp]
        public void CreateServerGameObject()
        {
            serverObj = new GameObject("Server");
            serverObj.AddComponent<MonoServer>();
        }

        [SetUp]
        public void CreateServer()
        {
            MonoServer.Instance = new Server();
            MonoServer.Instance.Init();
        }

        [Test]
        public void TestInit()
        {
            // Arrange
            DatabaseController databaseController = new DatabaseController();
            Dictionary<int, int> magic = databaseController.GetMagic();
            List<Troop> troops = databaseController.GetAllTroops();
            // Act: Init() is called in setup
            // Assert
            Assert.IsTrue(MonoServer.Instance.IsStarted);
            Assert.AreEqual(0, magic.Count);
            Assert.AreEqual(0, troops.Count);
        }

        [Test]
        public void TestShutdown()
        {
            MonoServer.Instance.Shutdown();
            Assert.IsFalse(MonoServer.Instance.IsStarted);
        }

        [Test]
        public void TestUpdateMessagePumpWhenServerIsntStarted()
        {
            // Arrange
            MonoServer.Instance.IsStarted = false;
            // Act
            MonoServer.Instance.UpdateMessagePump();
            // Assert
            Assert.IsFalse(MonoServer.Instance.IsStarted);

        }

        [Test]
        public void TestUpdateMessagePump()
        {
            // Arrange
            // Act
            MonoServer.Instance.UpdateMessagePump();
            // Assert
            Assert.AreEqual(0, MonoServer.Instance.error);

        }


        [Test]
        public void TestMessageTypeOfNothing()
        {
            MonoServer.Instance.CheckMessageType(0, 0, 0, new byte[1], NetworkEventType.Nothing);
            Assert.AreEqual(NetworkEventType.Nothing, MonoServer.Instance.LastEvent);
        }

        [Test]
        public void TestMessageTypeOfConnection()
        {
            MonoServer.Instance.CheckMessageType(0, 0, 0, new byte[1], NetworkEventType.ConnectEvent);
            Assert.AreEqual(NetworkEventType.ConnectEvent, MonoServer.Instance.LastEvent);
        }

        [Test]
        public void TestMessageTypeOfBroadcast()
        {
            MonoServer.Instance.CheckMessageType(0, 0, 0, new byte[1], NetworkEventType.BroadcastEvent);
            Assert.AreEqual(NetworkEventType.BroadcastEvent, MonoServer.Instance.LastEvent);
        }
        [Test]
        public void TestMessageTypeOfDisconnect()
        {
            MonoServer.Instance.CheckMessageType(0, 0, 0, new byte[1], NetworkEventType.DisconnectEvent);
            Assert.AreEqual(NetworkEventType.DisconnectEvent, MonoServer.Instance.LastEvent);
        }

        [Test]
        public void TestMessageTypeOfData()
        {
            // Arrange
            Net_ToggleControls ntc = new Net_ToggleControls() { OperationCode = 0 };
            byte[] buffer = new byte[1024];
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream ms = new MemoryStream(buffer);
            formatter.Serialize(ms, ntc);

            // Act
            MonoServer.Instance.CheckMessageType(0, 0, 0, buffer, NetworkEventType.DataEvent);
            Net_ToggleControls actual = (Net_ToggleControls) MonoServer.Instance.LastRecieved;

            // Assert
            Assert.AreEqual(ntc.OperationCode, actual.OperationCode);
        }

        [Test]
        public void TestSendToClient()
        {
            // Arrange
            Net_ToggleControls ntc = new Net_ToggleControls();
            // Act
            MonoServer.Instance.SendToClient(0,1,ntc);
            // Assert
            Assert.AreEqual(ntc, (Net_ToggleControls)MonoServer.Instance.LastSentToClient);
        }

        [Test]
        public void TestChangeSceneForClient1()
        {
            // Arrange
            string scene = "Battlefield";

            // Act
            MonoServer.Instance.ChangeScene(scene, 1);

            // Assert
            Assert.AreEqual((byte)NetOP.Operation.ChangeScene, ((Net_ChangeScene)MonoServer.Instance.LastSentToClient).OperationCode);
            Assert.AreEqual(1, MonoServer.Instance.LastClient);
        }

        [Test]
        public void TestChangeSceneForClient2()
        {
            // Arrange
            string scene = "Battlefield";

            // Act
            MonoServer.Instance.ChangeScene(scene, 2);

            // Assert
            Assert.AreEqual((byte)NetOP.Operation.ChangeScene, ((Net_ChangeScene)MonoServer.Instance.LastSentToClient).OperationCode);
            Assert.AreEqual(2, MonoServer.Instance.LastClient);
        }

        [Test]
        public void TestChangeSceneForBothClients()
        {
            // Arrange
            string scene = "Battlefield";

            // Act
            MonoServer.Instance.ChangeScene(scene, 3);

            // Assert
            Assert.AreEqual((byte)NetOP.Operation.ChangeScene, ((Net_ChangeScene)MonoServer.Instance.LastSentToClient).OperationCode);
            Assert.AreEqual(2, MonoServer.Instance.LastClient);
        }

        [Test]
        public void TestPropogateSingleTroopFromConnectionOneWithNoMagic()
        {
            // Arrange
            IDatabaseController databaseController = Substitute.For<IDatabaseController>();
            databaseController.GetAllTroops().Returns(
                new List<Troop>() 
                {
                    new Troop()
                    {
                        TroopID = 1,
                        TeamNum = 1
                    }
                });
            databaseController.GetMagic().Returns(new Dictionary<int, int>());
            MonoServer.Instance.dbCont = databaseController;
            // Act
            MonoServer.Instance.PropogateTroops();
            // Assert
            Net_Propogate actual = (Net_Propogate) MonoServer.Instance.LastSentToClient;
            Assert.AreEqual((byte)NetOP.Operation.PropogateTroop, actual.OperationCode);
            Assert.AreEqual(2, MonoServer.Instance.LastClient);
            Assert.AreEqual(255, actual.ComingFrom);
        }

        [Test]
        public void TestPropogateSingleTroopFromConnectionTwoWithNoMagic()
        {
            // Arrange
            IDatabaseController databaseController = Substitute.For<IDatabaseController>();
            databaseController.GetAllTroops().Returns(
                new List<Troop>()
                {
                    new Troop()
                    {
                        TroopID = 2,
                        TeamNum = 2
                    }
                });
            databaseController.GetMagic().Returns(new Dictionary<int, int>());
            MonoServer.Instance.dbCont = databaseController;
            // Act
            MonoServer.Instance.PropogateTroops();
            // Assert
            Net_Propogate actual = (Net_Propogate)MonoServer.Instance.LastSentToClient;
            Assert.AreEqual((byte)NetOP.Operation.PropogateTroop, actual.OperationCode);
            Assert.AreEqual(2, MonoServer.Instance.LastClient);
            Assert.AreEqual(2, actual.ComingFrom);
        }

        [Test]
        public void TestPropogateMultipleTroopsWithNoMagic()
        {
            // Arrange
            IDatabaseController databaseController = Substitute.For<IDatabaseController>();
            databaseController.GetAllTroops().Returns(
                new List<Troop>()
                {
                    new Troop()
                    {
                        TroopID = 1,
                        TeamNum = 1
                    },
                    new Troop()
                    {
                        TroopID = 2,
                        TeamNum = 2
                    }
                });
            databaseController.GetMagic().Returns(new Dictionary<int, int>());
            MonoServer.Instance.dbCont = databaseController;
            // Act
            MonoServer.Instance.PropogateTroops();
            // Assert
            Net_Propogate actual = (Net_Propogate)MonoServer.Instance.LastSentToClient;
            Assert.AreEqual((byte)NetOP.Operation.PropogateTroop, actual.OperationCode);
            Assert.AreEqual(2, MonoServer.Instance.LastClient);
        }


        [Test]
        public void TestPropogateWithMultipleTroopsAndMagic()
        {
            // Arrange
            IDatabaseController databaseController = Substitute.For<IDatabaseController>();
            databaseController.GetAllTroops().Returns(
                new List<Troop>()
                {
                    new Troop()
                    {
                        TroopID = 1,
                        TeamNum = 1
                    },
                    new Troop()
                    {
                        TroopID = 2,
                        TeamNum = 2
                    }
                });
            databaseController.GetMagic().Returns(
                new Dictionary<int, int>()
                {
                    { 1, 3 },
                    { 2, 1 }
                });
            MonoServer.Instance.dbCont = databaseController;
            // Act
            MonoServer.Instance.PropogateTroops();
            // Assert
            Net_SendMagic actual = (Net_SendMagic)MonoServer.Instance.LastSentToClient;
            Assert.AreEqual((byte)NetOP.Operation.SendMagic, actual.OperationCode);
            Assert.AreEqual(2, MonoServer.Instance.LastClient);
        }

        [Test]
        public void TestToggleControls()
        {
            // Arrange
            // Act
            MonoServer.Instance.ToggleControls(1);
            // Assert
            Net_ToggleControls actual = (Net_ToggleControls)MonoServer.Instance.LastSentToClient;
            Assert.AreEqual((byte)NetOP.Operation.ToggleControls, actual.OperationCode);
            Assert.AreEqual(1, MonoServer.Instance.LastClient);
        }

        [Test]
        public void TestRestartServer()
        {
            // Arrange
            // Act
            MonoServer.Instance.dbCont = Substitute.For<IDatabaseController>();
            MonoServer.Instance.RestartServer();

            // Assert
            Assert.IsTrue(MonoServer.Instance.IsStarted);
            Assert.AreEqual(2, MonoServer.Instance.waitingConnections);

        }

        [TearDown]
        public void Teardown()
        {
            MonoServer.Instance.Shutdown();
            DestroyImmediate(serverObj);
        }
        
    }
}