using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using UnityEngine;

public class Client : MonoBehaviour
{
    public string clientName;
    public bool isHost;

    private bool socketReady;
    private TcpClient socket;
    private NetworkStream stream;
    private StreamWriter writer;
    private StreamReader reader;

    public delegate void TryMov(int x1, int y1, int x2, int y2);

    public event TryMov OnTryMove;


    public List<GameClient> players = new List<GameClient>();

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void ConnectToServer(string host, int port)
    {
        if (socketReady) return;

        try
        {
            socket = new TcpClient(host, port);
            stream = socket.GetStream();
            writer = new StreamWriter(stream);
            reader = new StreamReader(stream);

            socketReady = true;
        }
        catch (Exception e)
        {
            Debug.Log("Socket error" + e.Message);
        }
    }

    // Socket - communication between client and server
    void Update()
    {
        if (socketReady && stream.DataAvailable)
        {
            string data = reader.ReadLine();
            if (data != null)
            {
                OnIncomingData(data);
            }
        }
    }

    // Sending messaged to the server
    public void Send(string data)
    {
        if (!socketReady)
            return;

        writer.WriteLine(data);
        writer.Flush();
    }

    // Read message from the server
    private void OnIncomingData(string data)
    {
        Debug.Log("Client: " + data);
        var aData = data.Split('|');

        switch (aData[0])
        {
            case "SWHO":
                for (int i = 1; i < aData.Length - 1; i++)
                {
                    UserConnected(aData[i], false);
                }

                Send("CWHO|" + clientName + "|" + ((isHost) ? 1 : 0).ToString());
                break;
            case "SCNN":
                UserConnected(aData[1], false);
                break;
            // Msg from our game
            case "SMOV":
                OnTryMove(int.Parse(aData[1]), int.Parse(aData[2]), int.Parse(aData[3]), int.Parse(aData[4]));
                break;
        }
    }

    private void UserConnected(string name, bool host)
    {
        GameClient c = new GameClient {Name = name, IsHost = host};

        players.Add(c);

        if (players.Count == 2)
            GameManager.Instance.StartGame();
    }

    private void OnApplicationQuit()
    {
        CloseSocket();
    }

    private void OnDisable()
    {
        CloseSocket();
    }

    private void CloseSocket()
    {
        if (!socketReady)
            return;

        writer.Close();
        reader.Close();
        socket.Close();
        socketReady = false;
    }
}

public class GameClient
{
    public string Name { get; set; }
    public bool IsHost { get; set; }
}