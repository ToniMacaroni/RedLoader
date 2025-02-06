using Bolt;
using Endnight.Extensions;
using HarmonyLib;
using RedLoader;
using RedLoader.Utils;
using Sons.Multiplayer;
using UdpKit;
using UnityEngine;
using Color = System.Drawing.Color;

namespace SonsSdk.Networking;

public static class Packets
{
    private static readonly Dictionary<int, NetRegistration> RegisteredEvents = new();

    /// <summary>
    /// Gets called when a chat message arrives. Returning true means you handled the message and none of the other handlers get called.
    /// If you just want to read the message, return false so the chat event gets sent to all other handlers as well.
    /// </summary>
    public static event Func<ChatEvent, bool> OnChatReceived;

    internal static void Init()
    {
        NetworkingPatches.Patch();
    }

    internal static void Uninit()
    {
    }
    
    public static void Register(NetEvent ev)
    {
        var hash = ev.GetIdHash();
        if (!RegisteredEvents.TryAdd(hash, new(hash, ev.Read)))
        {
            throw new Exception($"Net Event with id '{ev.Id}' already registered");
        }
    }

    public static bool Unregister(NetEvent ev)
    {
        var hash = ev.GetIdHash();
        return RegisteredEvents.Remove(hash);
    }

    // I don't know why I have to clone the packet this way. Seems like some Il2Cpp issue.
    private static UdpPacket ClonePacket(UdpPacket packet)
    {
        var newPacket = new UdpPacket(new byte[packet.Data.Length]);
        newPacket.WriteByteArray(packet.Data);
        return newPacket;
    }

    /// <summary>
    /// Send it to the matching NetEvent. Also relay it to the clients / target BoltConnection if necessary.
    /// </summary>
    /// <param name="packet"></param>
    /// <param name="fromConnection"></param>
    internal static void HandlePacket(UdpPacket packet, BoltConnection fromConnection)
    {
        var targetByte = packet.ReadByte();
        var target = (GlobalTargets)targetByte;
        uint targetConn = 0;

        if (targetByte == 0)
        {
            targetConn = packet.ReadUInt();
        }
        
        var id = packet.ReadInt();
        if (!RegisteredEvents.TryGetValue(id, out var reg))
        {
            return;
        }
        
        if (BoltNetwork.isServer)
        {
            // first read the packet if it also meant for us as a server
            if (target is GlobalTargets.OnlyServer or GlobalTargets.Everyone or GlobalTargets.Others)
            {
                reg.ReadFunc(packet);
            }

            if (target is GlobalTargets.OnlyServer)
            {
                return;
            }

            // handle relaying the source connection
            SendPacket(new EventPacket(ClonePacket(packet), target, targetConn), target is GlobalTargets.Others ? fromConnection.ConnectionId : null);

            return;
        }

        reg.ReadFunc(packet);
    }

    internal static void SendPacket(EventPacket packet, uint? skipConn = null)
    {
        if (!BoltNetwork.isRunning)
            return;
        
        // if we are the client send it to the server
        if (BoltNetwork.isClient)
        {
            BoltNetwork.server._udp.Send(packet.Packet);
            return;
        }
        
        // if we are the server check the target

        if (packet.Targets is GlobalTargets.Everyone or GlobalTargets.Others or GlobalTargets.AllClients)
        {
            BoltNetwork.clients.ForEach((Action<BoltConnection>)(conn =>
            {
                if (skipConn != null && conn.ConnectionId == skipConn.Value)
                {
                    return;
                }

                conn._udp.Send(packet.Packet);
            }));
        }

        if (packet.Targets is GlobalTargets.OnlySelf)
        {
            BoltNetwork.clients.ForEach((Action<BoltConnection>)(conn =>
            {
                if (conn.ConnectionId == packet.TargetConnectionId)
                {
                    conn._udp.Send(packet.Packet);
                }
            }));
        }
    }

    internal static EventPacket GetPacket(int typeId, int size, GlobalTargets targets)
    {
        size += 7;
        var packet = new UdpPacket(new byte[size]);
        packet.WriteUShort(6969);
        packet.WriteByte((byte)targets);
        packet.WriteInt(typeId);
        return new EventPacket(packet, targets, 0);
    }
    
    internal static EventPacket GetPacket(int typeId, int size, uint targetConnectionId)
    {
        size += 11;
        var packet = new UdpPacket(new byte[size]);
        packet.WriteUShort(6969);
        packet.WriteByte(0);
        packet.WriteUInt(targetConnectionId);
        packet.WriteInt(typeId);
        return new EventPacket(packet, GlobalTargets.OnlySelf, targetConnectionId);
    }
    
    public abstract class NetEvent
    {
        public abstract string Id { get; }

        private int _cachedIdHash = -1;
        
        public abstract void Read(UdpPacket packet);

        protected EventPacket NewPacket(int size, GlobalTargets targets) 
            => GetPacket(GetIdHash(), size, targets);

        protected EventPacket NewPacket(int size, uint targetConnection) 
            => GetPacket(GetIdHash(), size, targetConnection);

        protected EventPacket NewPacket(int size, BoltConnection targetConnection)
            => NewPacket(size, targetConnection.ConnectionId);

        protected void Send(EventPacket packet)
        {
            SendPacket(packet);
        }

        protected bool TryRelay<T>(UdpPacket packet) where T : MonoBehaviour, IPacketReader
        {
            var entity = packet.ReadBoltEntity();
            if (!entity)
            {
                RLog.Error("Relaying failed: Entity not found");
                return false;
            }

            if (!entity.TryGetComponent<T>(out var comp))
            {
                RLog.Error($"Packet relaying failed: Component {typeof(T).Name} not found on object");
                return false;
            }
            
            comp.ReadPacket(packet);
            return true;
        }

        public int GetIdHash()
        {
            if (_cachedIdHash == -1)
                _cachedIdHash = LoaderUtils.HashString(Id);
            return _cachedIdHash;
        }
    }

    public record EventPacket(UdpPacket Packet, GlobalTargets Targets, uint TargetConnectionId);
    
    private class NetRegistration
    {
        public int IdHash;
        public readonly Action<UdpPacket> ReadFunc;

        public NetRegistration(int idHash, Action<UdpPacket> readFunc)
        {
            IdHash = idHash;
            ReadFunc = readFunc;
        }
    }
    
    public interface IPacketReader
    {
        public void ReadPacket(UdpPacket packet);
    }

    private class NetworkingPatches
    {
        [HarmonyPatch(typeof(BoltConnection), nameof(BoltConnection.PacketReceived), typeof(UdpPacket))]
        [HarmonyPrefix]
        private static bool PacketReceived(BoltConnection __instance, UdpPacket udpPacket)
        {
            // 6 = 9696 short + target byte + net event id int
            if (udpPacket.Size < 7 || udpPacket.ReadUShort() != 6969)
            {
                udpPacket.Position = 0;
                return true;
            }

            HandlePacket(udpPacket, __instance);

            return false;
        }

        [HarmonyPatch(typeof(BoltConnection), nameof(BoltConnection.PacketDelivered), typeof(Packet))]
        [HarmonyPrefix]
        private static bool PacketDelivered(BoltConnection __instance, Packet packet)
        {
            if (packet == null)
            {
                return false;
            }
    
            if (packet.UdpPacket == null)
            {
                return true;
            }
    
            if (packet.UdpPacket.Size < 2 || packet.UdpPacket.ReadUShort() != 6969)
            {
                packet.UdpPacket.Position = 0;
                return true;
            }

            return false;
        }
        
        [HarmonyPatch(typeof(CoopServerCallbacks), nameof(CoopServerCallbacks.OnEvent), typeof(ChatEvent))]
        [HarmonyPrefix]
        public static bool ChatEventPatch(CoopServerCallbacks __instance, ChatEvent evnt)
        {
            if (OnChatReceived != null && OnChatReceived.Invoke(evnt))
            {
                return false;
            }

            return true;
        }
        
        public static void Patch()
        {
            SdkEntryPoint.Harmony.PatchAll(typeof(NetworkingPatches));
        }
    }
}
