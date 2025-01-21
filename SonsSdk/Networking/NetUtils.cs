using System.Drawing;
using Bolt;
using RedLoader;
using RedLoader.Utils;

namespace SonsSdk.Networking;

public static class NetUtils
{
    public static bool IsMultiplayer => BoltNetwork.isRunning;
    public static bool IsServer => BoltNetwork.isServer;
    public static bool IsClient => BoltNetwork.isClient;
    public static bool IsDedicatedServer => LoaderEnvironment.IsDedicatedServer;
    
    /// <summary>
    /// Send a chat message to everyone or a specific connection. THIS IS ONLY POSSIBLE IF YOU ARE A SERVER (either dedicated server or host in coop).
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="message"></param>
    /// <param name="color"></param>
    /// <param name="receiver"></param>
    public static void SendChatMessage(NetworkId sender, string message, string color = null, BoltConnection receiver = null)
    {
        if (!BoltNetwork.isRunning || !BoltNetwork.isServer)
        {
            RLog.Msg("Sending messaged is only possible in a multiplayer game and if you are the server");
            return;
        }
        
        var chat = receiver != null ? ChatEvent.Create(receiver) : ChatEvent.Create(GlobalTargets.AllClients);
        
        if(!string.IsNullOrEmpty(color))
            message = $"<color={color}>{message}</color>";
        
        chat.Message = message;
        chat.Sender = sender;
        
        chat.Send();
    }
}