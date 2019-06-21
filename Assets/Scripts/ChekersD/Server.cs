using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using System.Net;
using System.IO;

public class Server : MonoBehaviour
{
    // Port which we will use. For IP-address
    public int port = 6321;

    private List<ServerClient> clients;
    private List<ServerClient> disconnectList;

    private TcpListener server;
    private bool serverStarted = true;

    public void Init()
    {
        DontDestroyOnLoad(gameObject);
        clients = new List<ServerClient>();
        disconnectList = new List<ServerClient>();

        try
        {
            server = new TcpListener(IPAddress.Any, port);
            server.Start();

            StartListening();
        }
        catch (Exception e)
        {
            Debug.Log("Socket error: " + e.Message);
        }
    }

    private void Update()
    {
        if (!serverStarted)
        {
            return;
        }

        foreach (var c in clients)
        {
            // Is the clients still connected?
            if (IsConnected(c.Tcp))
            {
                var s = c.Tcp.GetStream();
                if (s.DataAvailable)
                {
                    var reader = new StreamReader(s, true);
                    var data = reader.ReadLine();
                    if (data != null) OnIncomingData(c, data);
                }
            }
            else
            {
                c.Tcp.Close();
                disconnectList.Add(c);
            }
        }

        for (int i = 0; i < disconnectList.Count - 1; i++)
        {
            // Tell our player somebody has dosconected
            clients.Remove(disconnectList[i]);
            disconnectList.RemoveAt(i);
        }
    }

    private void StartListening()
    {
        // Funct which accept Tcp-client
        server.BeginAcceptTcpClient(AcceptTcpClient, server);
    }

    private void AcceptTcpClient(IAsyncResult ar)
    {
        var listener = (TcpListener) ar.AsyncState;

        var allUsers = "";
        foreach (var i in clients)
        {
            allUsers += i.ClientName + '|';
        }

        // Создаем определение для этого самого человека
        var sc = new ServerClient(listener.EndAcceptTcpClient(ar));
        // Add in our List
        clients.Add(sc);

        // Start listening again for other people
        StartListening();

        // Indo which we want send
        Broadcast("SWHO|" + allUsers, clients[clients.Count - 1]);

        //Debug.Log("Somebody has connected!");
    }

    private bool IsConnected(TcpClient c)
    {
        try
        {
            if (c?.Client != null && c.Client.Connected)
            {
                if (c.Client.Poll(0, SelectMode.SelectRead))
                    return c.Client.Receive(new byte[1], SocketFlags.Peek) != 0;
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    // Server Send
    private static void Broadcast(string data, IEnumerable<ServerClient> cl)
    {
        foreach (var sc in cl)
        {
            try
            {
                var writer = new StreamWriter(sc.Tcp.GetStream());
                writer.WriteLine(data);
                writer.Flush();
            }
            catch (Exception e)
            {
                Debug.Log("Write error : " + e.Message);
            }
        }
    }

    // For simple client
    private void Broadcast(string data, ServerClient c)
    {
        // Put our client in List and call first Broadcast funct
        var sc = new List<ServerClient> {c};
        Broadcast(data, sc);
    }


    // Server Read
    private void OnIncomingData(ServerClient c, string data)
    {
        Debug.Log("Server: " + data);
        var aData = data.Split('|');

        switch (aData[0])
        {
            case "CWHO":
                c.ClientName = aData[1];
                c.IsHost = (aData[2] != "0");
                Broadcast("SCNN|" + c.ClientName, clients);
                break;

            // Msg from our game
            case "CMOV":
                Broadcast("SMOV|" + aData[1] + "|" + aData[2] + "|" + aData[3] + "|" + aData[4], clients);
                break;
        }
    }
}

public class ServerClient
{
    public string ClientName { get; set; }
    public TcpClient Tcp { get; set; }
    public bool IsHost { get; set; }

    public ServerClient(TcpClient tcp)
    {
        Tcp = tcp;
    }
}