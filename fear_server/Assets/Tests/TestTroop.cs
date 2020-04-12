using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FearValleyNetwork;
using Scripts.DBMS;
using NUnit.Framework;
using NSubstitute;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.TestTools;

namespace Assets.Tests
{
    public class TestTroop : MonoBehaviour
    {
        GameObject serverObj;

        [Test]
        public void TestToString()
        {
            // Arrange
            Troop troop = new Troop()
            {
                TeamNum = 1,
                TroopType = "Peasant",
                Armor = 10,
                TroopAtkBonus = 0,
                WeaponRange = 1,
                WeaponDamage = 6,
                Health = 10,
                Movement = 5,
                Leader = false,
                XPos = 1,
                ZPos = 1,
                TroopID = 1,
                TroopDamageBonus = 0
            };

            // Act
            string expected = $"\n<Troop>\nTeam=1\nClass=Peasant" +
                $"\nArmor=10\nWMod=0" +
                $"\nWDmg=6\nHealth=10" +
                $"\nleader=False\nXPos=1\nZPos=1";
            string actual = troop.ToString();

            // Assert
            Assert.AreEqual(expected, actual);

        }
    }
}