using UnityEngine;
using WebSocketSharp;
using TMPro;
using UnityEngine.UI;

public class BasicWebSocketClient : MonoBehaviour
{
    // Instancia del cliente WebSocket
    private WebSocket ws;
    public TMP_Text chatDisplay;
    public TMP_InputField inputField;
    public Button sendButton;
    public ScrollRect scrollRect;

    // Se ejecuta al iniciar la escena
    void Start()
    {
        // Crear una instancia del WebSocket apuntando a la URI del servidor
        ws = new WebSocket("ws://127.0.0.1:7777/");

        // Evento OnOpen: se invoca cuando se establece la conexión con el servidor
        ws.OnOpen += (sender, e) =>
        {
            Debug.Log("WebSocket conectado correctamente.");
        };

        // Evento OnMessage: se invoca cuando se recibe un mensaje del servidor
        ws.OnMessage += (sender, e) =>
        {
            Debug.Log("Mensaje recibido: " + e.Data);
            UpdateChat(e.Data);
        };

        // Evento OnError: se invoca cuando ocurre un error en la conexión
        ws.OnError += (sender, e) =>
        {
            Debug.LogError("Error en el WebSocket: " + e.Message);
        };

        // Evento OnClose: se invoca cuando se cierra la conexión con el servidor
        ws.OnClose += (sender, e) =>
        {
            Debug.Log("WebSocket cerrado. Código: " + e.Code + ", Razón: " + e.Reason);
        };

        // Conectar de forma asíncrona al servidor WebSocket
        ws.ConnectAsync();

        sendButton.onClick.AddListener(SendMessage);
        inputField.onSubmit.AddListener(delegate { SendMessage(); });
        chatDisplay.text = "";
    }

    // Método para enviar un mensaje al servidor (puedes llamarlo, por ejemplo, desde un botón en la UI)
    public void SendMessage()
    {
        if (!string.IsNullOrEmpty(inputField.text))
        {
            ws.Send(inputField.text);
            inputField.text = "";
            inputField.ActivateInputField();
        }
    }

    private void UpdateChat(string message)
    {
        chatDisplay.text += "\n" + message;
        LayoutRebuilder.ForceRebuildLayoutImmediate(chatDisplay.rectTransform);
        ScrollToBottom();
    }

    private void ScrollToBottom()
    {
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }
    
    // Se ejecuta cuando el objeto se destruye (por ejemplo, al cambiar de escena o cerrar la aplicación)
    void OnDestroy()
    {
        if (ws != null)
        {
            ws.Close();
            ws = null;
        }
    }
}
