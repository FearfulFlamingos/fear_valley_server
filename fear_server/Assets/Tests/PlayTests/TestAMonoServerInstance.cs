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

namespace PlayTests
{
    public class TestAMonoServerInstance : MonoBehaviour
    {

        [UnityTest]
        public IEnumerator TestMonoServerStart()
        {
            // Arrange
            GameObject serverObj = new GameObject("server");
            serverObj.AddComponent<MonoServer>();
            yield return new WaitForSeconds(3);
            // Act
            // Assert
            Assert.IsTrue(MonoServer.Instance.IsStarted);
            yield return null;
            MonoServer.Instance.Shutdown();
            Destroy(serverObj);
        }
    }
}