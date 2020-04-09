using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;
using FearValleyNetwork;
using Scripts.DBMS;

public class Server : MonoBehaviour
{
    private const int MAX_USER = 2; 
    private const int PORT = 50000;
    private const int BYTE_SIZE = 1024; // standard packet size
    
    private byte reliableChannel;
    private int hostId;
    private bool isStarted = false;
    private byte error; // general error byte, see documentation
    private int waitingConnections = 2;

    private DatabaseController dbCont;
    private List<Troop> allTroops = new List<Troop>();
    private Dictionary<int, int> magics = new Dictionary<int, int>();

    #region Monobehavior
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        Init();
    }

    private void Update()
    {
        UpdateMessagePump();
    }
    #endregion

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

        isStarted = true;
        Debug.Log($"Started server on port {PORT}");

        // Clear out Army table for database
        dbCont = new DatabaseController();
        dbCont.Update("DELETE FROM Army;");
        dbCont.Update("DELETE FROM Magic;");
        dbCont.CloseDB();

        
    }                                         
    public void Shutdown()
    {
        isStarted = false;
#pragma warning disable CS0618 // Type or member is obsolete
        NetworkTransport.Shutdown();
#pragma warning restore CS0618 // Type or member is obsolete
    }

    public void UpdateMessagePump()
    {
        if (!isStarted)
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
        switch(type)
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
    }

    #region OnData
    private void OnData(int connId, int channelId, int recHostId, NetMsg msg)
    {
        Debug.Log($"Recieved message of type {msg.OperationCode}");
        switch (msg.OperationCode)
        {
            case (byte) NetOP.Operation.None:
                Debug.Log("Unexpected NETOP code");
                break;
            case (byte) NetOP.Operation.AddTroop:
                Debug.Log("NETOP: Add Troop to DB");
                Net_AddTroop(connId, channelId, recHostId, (Net_AddTroop)msg);
                break;
            case (byte) NetOP.Operation.FinishBuild:
                Debug.Log($"NETOP: Player {connId} is finished");
                Net_FinishBuild(connId, channelId, recHostId, (Net_FinishBuild)msg);
                break;
            case (byte) NetOP.Operation.MOVE:
                Debug.Log("NETOP: Move troop");
                Net_MOVE(connId, channelId, recHostId, (Net_MOVE)msg);
                break;
			case (byte) NetOP.Operation.ATTACK:
				Debug.Log("NETOP: Attack troop");
				Net_ATTACK(connId, channelId, recHostId, (Net_ATTACK)msg);
				break;
            case (byte) NetOP.Operation.RETREAT:
                Debug.Log("NETOP: Attack troop");
                Net_RETREAT(connId, channelId, recHostId, (Net_RETREAT)msg);
                break;
            case (byte) NetOP.Operation.EndTurn:
                Debug.Log($"NETOP: End P{connId} turn");
                Net_EndTurn(connId, channelId, recHostId, (Net_EndTurn)msg);
                break;
        }
    }

    private void Net_EndTurn(int connId, int channelId, int recHostId, Net_EndTurn msg)
    {
        Debug.Log("Toggling controls for all players");
        ToggleControls(1);
        ToggleControls(2);
    }

    private void Net_RETREAT(int connId, int channelId, int recHostId, Net_RETREAT msg)
    {
        Debug.Log($"Attack {msg.TroopID} with damage");
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
    private void Net_ATTACK(int connId, int channelId, int recHostId, Net_ATTACK msg)
    {
        Debug.Log($"Attack {msg.TroopID} with damage");
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
        dbCont = new DatabaseController();
        allTroops = dbCont.GetAllTroops();
        magics = dbCont.ReadMagicFromDB();
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
        foreach (KeyValuePair<int,int> entry in magics)
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
