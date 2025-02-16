using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;

// Clase que se adjunta a un GameObject en Unity para iniciar el servidor WebSocket.
public class BasicWebSocketServer : MonoBehaviour
{
    // Instancia del servidor WebSocket.
    private WebSocketServer wss;
    private static Dictionary<string, string> clients = new Dictionary<string, string>();
    private static string[] colors =  { "#FF5733", "#33FF57", "#3357FF", "#F5B041", "#9B59B6" };
    public static int clientCounter = 1;

    // Se ejecuta al iniciar la escena.
    void Start()
    {
        if (wss == null || !wss.IsListening)
        {
            // Crear un servidor WebSocket que escucha en el puerto 7777.
            wss = new WebSocketServer(7777);

            // Añadir un servicio en la ruta "/" que utiliza el comportamiento EchoBehavior.
            wss.AddWebSocketService<EchoBehavior>("/");

            // Iniciar el servidor.
            wss.Start();

            Debug.Log("Servidor WebSocket iniciado en ws://127.0.0.1:7777/");
        }
    }

    // Se ejecuta cuando el objeto se destruye (por ejemplo, al cerrar la aplicación o cambiar de escena).
    void OnDestroy()
    {
        // Si el servidor está activo, se detiene de forma limpia.
        if (wss != null)
        {
            wss.Stop();
            wss = null;
            Debug.Log("Servidor WebSocket detenido.");
        }
    }

    public static Dictionary<string, string> GetClients() => clients;
    public static string[] GetColors() => colors;
    public static int GetNextClientId() => clientCounter++;
}

// Comportamiento básico del servicio WebSocket: simplemente devuelve el mensaje recibido.
public class EchoBehavior : WebSocketBehavior
{
    private string clientId;
    private string clientColor;
    private string logFilePath = "chat_history.txt";

    protected override void OnOpen()
    {
        var clients = BasicWebSocketServer.GetClients();
        var colors = BasicWebSocketServer.GetColors();
        int clientNum = BasicWebSocketServer.GetNextClientId();

        clientId = "Usuario" + clientNum;
        clientColor = colors[(clientNum - 1) % colors.Length];
        clients[ID] = clientId;
        SendToAll("<color=" + clientColor + ">" + clientId + " se ha conectado.</color>");
        SaveMessageToFile(clientId + " se ha conectado.");
    }
    
    // Se invoca cuando se recibe un mensaje desde un cliente.
    protected override void OnMessage(MessageEventArgs e)
    {
        SendToAll("<color=" + clientColor + "><b>" + clientId + ":</b></color> " + e.Data);
        SaveMessageToFile(clientId + ": " + e.Data);
    }

    protected override void OnClose(CloseEventArgs e)
    {
        var clients = BasicWebSocketServer.GetClients();
        if (clients.ContainsKey(ID))
        {
            SendToAll("<color=" + clientColor + ">" + clientId + " se ha desconectado.</color>");
            SaveMessageToFile(clientId + " se ha desconectado.");
            clients.Remove(ID);
        }
    }

    private void SendToAll(String message) {
        Sessions.Broadcast(message);
    }

    private void SaveMessageToFile(string message)
    {
        try
        {
            File.AppendAllText(logFilePath, message + "\n");
        }
        catch (Exception ex)
        {
            Debug.LogError("Error al guardar el mensaje en el historial: " + ex.Message);
        }
    }
}