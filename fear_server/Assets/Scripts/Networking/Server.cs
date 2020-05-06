using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;
using FearValleyNetwork;
using Scripts.DBMS;

/// <summary>
/// All networking related scripts and functions are here.
/// </summary>
namespace Scripts.Networking
{
    /// <summary>
    /// The server runs constantly during the game and checks every frame for new network events from clients.
    /// </summary>
    public class Server
    {
        private const int MAX_USER = 2;
        private const int PORT = 50000;
        private const int BYTE_SIZE = 1024; // standard packet size

        private byte reliableChannel;
        private int hostId;
        public bool IsStarted { set; get; }
        public byte error; // general error byte, see documentation
        public int waitingConnections = 2;

        public IDatabaseController dbCont;
        private List<Troop> allTroops = new List<Troop>();
        private Dictionary<int, int> magics = new Dictionary<int, int>();
        
        /// <summary>Used for testing.</summary>
        public NetworkEventType LastEvent { set; get; }
        
        /// <summary>Used for testing.</summary>
        public NetMsg LastRecieved { set; get; }
        
        /// <summary>Used for testing.</summary>
        public NetMsg LastSentToClient { set; get; }
        
        /// <summary>Used for testing.</summary>
        public int LastClient { set; get; }

        /// <summary>
        /// Start the server.
        /// </summary>
        public void Init()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            NetworkTransport.Init();
#pragma warning restore CS0618 // Type or member is obsolete

#pragma warning disable CS0618 // Type or member is obsolete
            ConnectionConfig cc = new ConnectionConfig();
#pragma warning restore CS0618 // Type or member is obsolete
            reliableChannel = cc.AddChannel(QosType.Reliable); // other channels available
                                                               // need a QosType.ReliableFragmented if data needs to be bigger than 1024 bytes

#pragma warning disable CS0618 // Type or member is obsolete
            HostTopology topo = new HostTopology(cc, MAX_USER); // "map" of channels
#pragma warning restore CS0618 // Type or member is obsolete

            // Server only code
#pragma warning disable CS0618 // Type or member is obsolete
            hostId = NetworkTransport.AddHost(topo, PORT);
#pragma warning restore CS0618 // Type or member is obsolete

            IsStarted = true;
            Debug.Log($"Started server on port {PORT}");

            // Clear out Army table for database
            if (dbCont == null)
                dbCont = new DatabaseController();
            dbCont.ClearPreviousGameData();
            dbCont.CloseDB();
        }


        /// <summary>
        /// Shut the server down.
        /// </summary>
        public void Shutdown()
        {
            IsStarted = false;
#pragma warning disable CS0618 // Type or member is obsolete
            NetworkTransport.Shutdown();
#pragma warning restore CS0618 // Type or member is obsolete
        }

        /// <summary>
        /// Check for new network events.
        /// </summary>
        public void UpdateMessagePump()
        {
            if (!IsStarted)
                return;

            int recHostId; // From web or standalone?
            int connectionId; // Which user?
            int channelId; // Which "lane"  is message coming through?

            byte[] recievedBuffer = new byte[BYTE_SIZE];
            int dataSize; // length of byte[] that data fills

#pragma warning disable CS0618 // Type or member is obsolete
            NetworkEventType type = NetworkTransport.Receive(out recHostId,
#pragma warning restore CS0618 // Type or member is obsolete
            out connectionId,
                out channelId,
                recievedBuffer,
                BYTE_SIZE,
                out dataSize,
                out error);
            CheckMessageType(recHostId, connectionId, channelId, recievedBuffer, type);
        }

        /// <summary>
        /// Checks the network event type of the incoming net message and acts appropriately.
        /// </summary>
        /// <remarks>
        /// Public due to testing, could easily have been a part of 
        /// <see cref="UpdateMessagePump"/>
        /// </remarks>
        /// <param name="recHostId">Which client is it coming from</param>
        /// <param name="connectionId">Which connection is being used.</param>
        /// <param name="channelId">Which channel is being used.</param>
        /// <param name="recievedBuffer">Data sent with message (if applicable).</param>
        /// <param name="type">Type of network event.</param>
        public void CheckMessageType(int recHostId, int connectionId, int channelId, byte[] recievedBuffer, NetworkEventType type)
        {
            switch (type)
            {
                case NetworkEventType.Nothing:
                    break;
                case NetworkEventType.ConnectEvent:
                    Debug.Log($"User {connectionId} has connected through host {hostId}");
                    break;
                case NetworkEventType.DataEvent:
                    Debug.Log("Data recieved");
                    BinaryFormatter formatter = new BinaryFormatter();
                    MemoryStream ms = new MemoryStream(recievedBuffer);
                    NetMsg msg = (NetMsg)formatter.Deserialize(ms);
                    OnData(connectionId, channelId, recHostId, msg);
                    break;
                case NetworkEventType.DisconnectEvent:
                    Debug.Log($"User {connectionId} has disconnected");
                    waitingConnections++;
                    break;

                default:
                case NetworkEventType.BroadcastEvent:
                    Debug.Log("Unexpected network event type");
                    break;
            }
            LastEvent = type;
        }

        #region OnData
        /* Summary:
         *   Check the operation code of the network message. 
         */
        private void OnData(int connId, int channelId, int recHostId, NetMsg msg)
        {
            Debug.Log($"Recieved message of type {msg.OperationCode}");
            switch (msg.OperationCode)
            {
                case (byte)NetOP.Operation.None:
                    Debug.Log("Unexpected NETOP code");
                    break;
                case (byte)NetOP.Operation.AddTroop:
                    Debug.Log("NETOP: Add Troop to DB");
                    Net_AddTroop(connId, channelId, recHostId, (Net_AddTroop)msg);
                    break;
                case (byte)NetOP.Operation.FinishBuild:
                    Debug.Log($"NETOP: Player {connId} is finished");
                    Net_FinishBuild(connId, channelId, recHostId, (Net_FinishBuild)msg);
                    break;
                case (byte)NetOP.Operation.MOVE:
                    Debug.Log("NETOP: Move troop");
                    Net_MOVE(connId, channelId, recHostId, (Net_MOVE)msg);
                    break;
                case (byte)NetOP.Operation.ATTACK:
                    Debug.Log("NETOP: Attack troop");
                    Net_ATTACK(connId, channelId, recHostId, (Net_ATTACK)msg);
                    break;
                case (byte)NetOP.Operation.RETREAT:
                    Debug.Log("NETOP: Attack troop");
                    Net_RETREAT(connId, channelId, recHostId, (Net_RETREAT)msg);
                    break;
                case (byte)NetOP.Operation.EndTurn:
                    Debug.Log($"NETOP: End P{connId} turn");
                    Net_EndTurn(connId, channelId, recHostId, (Net_EndTurn)msg);
                    break;
                case (byte)NetOP.Operation.UpdateEnemyName:
                    Debug.Log("NETOP: Change Enemy Name");
                    Net_ChangeEnemyName(connId, channelId, recHostId, (Net_UpdateEnemyName)msg);
                    break;
            }
            LastRecieved = msg;
        }

        // change the enemy name for the other client
        private void Net_ChangeEnemyName(int connId, int channelId, int recHostId, Net_UpdateEnemyName msg)
        {
            switch(connId)
            {
                case 1:
                    SendToClient(0, 2, msg);
                    break;
                case 2:
                    SendToClient(0, 1, msg);
                    break;
            }
        }

        // end the clients turn
        private void Net_EndTurn(int connId, int channelId, int recHostId, Net_EndTurn msg)
        {
            Debug.Log("Toggling controls for all players");
            ToggleControls(1);
            ToggleControls(2);
        }

        // send a retreat request from one client to the other.
        private void Net_RETREAT(int connId, int channelId, int recHostId, Net_RETREAT msg)
        {
            switch (msg.ForceEnemyToRetreat)
            {
                case true:
                    Debug.Log($"Connection {connId} says that Troop {msg.TroopID} on team {msg.TeamNum} is dead");
                    msg.TeamNum = 1;
                    if (connId == 1)
                        SendToClient(0, 2, msg);
                    else
                        SendToClient(0, 1, msg);
                    break;
                case false:
                    Debug.Log($"Connection {connId} sent retreat command for Troop {msg.TroopID} on team {msg.TeamNum}");
                    msg.TeamNum = 2;
                    if (connId == 1)
                        SendToClient(0, 2, msg);
                    else
                        SendToClient(0, 1, msg);
                    break;
            }

        }

        // Let the otehr client know that they have been attacked.
        private void Net_ATTACK(int connId, int channelId, int recHostId, Net_ATTACK msg)
        {
            Debug.Log($"Attack {msg.TroopID} with {msg.DamageTaken} damage");
            switch (connId)
            {
                case 1:
                    SendToClient(0, 2, msg);
                    break;
                case 2:
                    SendToClient(0, 1, msg);
                    break;
                default:
                    Debug.Log("Unknown connectionID, disconnected?");
                    break;
            }
        }

        // when a client finishs thier army build, change the scene
        private void Net_FinishBuild(int connId, int channelId, int recHostId, Net_FinishBuild msg)
        {
            // Add magic amount to DB
            dbCont.OpenDB();
            dbCont.AddMagicToDB(connId, msg.MagicBought);
            dbCont.CloseDB();

            //Debug.Log($"{waitingConnections} left");
            ChangeScene("TempLoadingScene", connId);
            //ToggleControls(connId);
            Debug.Log($"Changed P{connId} scene to temp");
            waitingConnections--;
            Debug.Log($"{waitingConnections} connection(s) remaining");

            if (waitingConnections == 0)
            {
                ChangeScene("Battlefield", 3);
                PropogateTroops();
                ToggleControls(1);
            }
        }

        // move a troop on both clients screens
        private void Net_MOVE(int connId, int channelId, int recHostId, Net_MOVE msg)
        {
            Debug.Log($"Moved {msg.TroopID} to <{msg.NewX},{msg.NewZ}");
            switch (connId)
            {
                case 1:
                    SendToClient(0, 2, msg);
                    break;
                case 2:
                    SendToClient(0, 1, msg);
                    break;
                default:
                    Debug.Log("Unknown connectionID, disconnected?");
                    break;
            }
        }

        // Add a troop to the database
        private void Net_AddTroop(int connId, int channelId, int recHostId, Net_AddTroop msg)
        {
            dbCont.OpenDB();
            dbCont.AddTroopToDB(
                connId,
                msg.TroopType,
                msg.WeaponType,
                msg.ArmorType,
                msg.XPosRelative,
                msg.ZPosRelative);
            dbCont.CloseDB();
        }
        #endregion

        #region Send
        /// <summary>
        /// Sends a message to the client over the network.
        /// </summary>
        /// <param name="recHost">Recieving host socket type, typically 0.</param>
        /// <param name="connId">Connection to client.</param>
        /// <param name="msg">Any child of NetMsg, carries a payload.</param>
        public void SendToClient(int recHost, int connId, NetMsg msg)
        {
            // hold data to send
            byte[] buffer = new byte[BYTE_SIZE];

            // serialize to byte[]
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream ms = new MemoryStream(buffer);
            formatter.Serialize(ms, msg);

            //int test = 0;
            //for (int i = 0; i < BYTE_SIZE;i++)
            //{
            //    if (buffer[i] != new byte())
            //        test++;
            //}
            //Debug.Log($"Transfer = {test} bytes");

#pragma warning disable CS0618 // Type or member is obsolete
            NetworkTransport.Send(hostId,
#pragma warning restore CS0618 // Type or member is obsolete
            connId,
                reliableChannel,
                buffer,
                BYTE_SIZE,
                out error);
            if (error != 0)
                Debug.Log(error);

            LastSentToClient = msg;
            LastClient = connId;
        }

        /// <summary>
        /// Changes the scene on the client.
        /// </summary>
        /// <param name="scene">Name of the scene to switch to.</param>
        public void ChangeScene(string scene, int whichConn)
        {
            Net_ChangeScene cs = new Net_ChangeScene();
            cs.SceneName = scene;

            switch (whichConn)
            {
                case 1:
                    SendToClient(0, 1, cs);
                    break;
                case 2:
                    SendToClient(0, 2, cs);
                    break;
                case 3:
                    SendToClient(0, 1, cs);
                    SendToClient(0, 2, cs);
                    break;
            }

        }

        /// <summary>
        /// Reads DB for troops and sends info to clients.
        /// </summary>
        public void PropogateTroops()
        {
            Debug.Log("Sending troops");
            dbCont.OpenDB();
            allTroops = dbCont.GetAllTroops();
            magics = dbCont.GetMagic();
            dbCont.CloseDB();

            foreach (Troop t in allTroops)
            {
                Net_Propogate np = new Net_Propogate
                {
                    Prefab = t.TroopType,
                    TroopID = t.TroopID,
                    TeamNum = t.TeamNum,
                    AbsoluteXPos = t.XPos,
                    AbsoluteZPos = t.ZPos,
                    AtkBonus = t.TroopAtkBonus,
                    AtkRange = t.WeaponRange,
                    DamageBonus = t.TroopDamageBonus,
                    Health = t.Health,
                    Movement = t.Movement,
                    MaxAttackVal = t.WeaponDamage,
                    DefenseMod = t.Armor
                };
                Debug.Log($"DBG>{np.Prefab}");

                if (np.TeamNum == 1)
                {
                    // Send to P1 as part of team
                    np.ComingFrom = 1;
                    SendToClient(0, 1, np);

                    // Send to P2 as enemy
                    np.ComingFrom = 255;
                    SendToClient(0, 2, np);
                }
                else
                {
                    // Send to P1 as enemy
                    np.ComingFrom = 255;
                    SendToClient(0, 1, np);

                    // Send to P2 as part of team
                    np.ComingFrom = 2;
                    SendToClient(0, 2, np);
                }

            }

            Net_SendMagic sm = new Net_SendMagic();

            // send magic amount to client
            foreach (KeyValuePair<int, int> entry in magics)
            {
                sm.MagicAmount = entry.Value;
                SendToClient(0, entry.Key, sm);
            }
        }

        /// <summary>
        /// Toggles whether the controls are enabled or disabled for the client.
        /// </summary>
        public void ToggleControls(int connId) //TODO: Add payload?
        {
            Net_ToggleControls tc = new Net_ToggleControls();
            SendToClient(0, connId, tc);
        }

        #endregion

        #region TEST FUNCTIONS
        /// <summary>
        /// These are test functions that are mapped to buttons in the server scene.
        /// </summary>
        public void RestartServer()
        {
            waitingConnections = 2;
            Debug.Log($"Reset waiting connections to {waitingConnections}");
            Shutdown();
            Debug.Log("Shut down server");
            Debug.Log("Restarting server...");
            Init();
        }

        #endregion

    }
}