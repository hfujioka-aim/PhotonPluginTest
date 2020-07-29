using System;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using Photon.Hive.Plugin;

namespace Photon.Pun
{
    static class IPluginHostExtensions
    {
        public static bool TryRegisterType<T>(this IPluginHost host, byte code, Func<T, byte[]> serializeFunction, Func<byte[], T> deserializeFunction)
        {
            return host.TryRegisterType(
                typeof(T), code,
                obj => obj is T v ? serializeFunction(v) : throw new InvalidCastException(),
                buf => deserializeFunction(buf)
            );
        }

        public static bool RegisterPUNTypes(this IPluginHost host)
        {
            var pun = new PunTypes(host);
            return pun.Register();
        }
    }

    internal static class BitConverterExtensions
    {
        public static T Read<T>(this byte[] buf, ref int i)
            where T: unmanaged
        {
            var s = buf.AsSpan(i, Unsafe.SizeOf<T>());
            i += Unsafe.SizeOf<T>();
            if (BitConverter.IsLittleEndian) {
                s.Reverse();
            }
            return MemoryMarshal.Read<T>(s);
        }

        public static void Write<T>(this byte[] buf, T value, ref int i)
            where T : unmanaged
        {
            var s = buf.AsSpan(i, Unsafe.SizeOf<T>());
            i += Unsafe.SizeOf<T>();
            MemoryMarshal.Write(s, ref value);
            if (BitConverter.IsLittleEndian) {
                s.Reverse();
            }
        }
    }

    /// <summary>
    /// Assets/Photon/PhotonUnityNetworking/Code/CustomTypes.cs
    /// </summary>
    class PunTypes
    {
        public IPluginHost PluginHost { get; }

        public PunTypes(IPluginHost host)
        {
            this.PluginHost = host;
        }

        public bool Register()
        {
            var result = true;
            result &= this.PluginHost.TryRegisterType((byte)'W', this.SerializeVector2, this.DeserializeVector2);
            result &= this.PluginHost.TryRegisterType((byte)'V', this.SerializeVector3, this.DeserializeVector3);
            result &= this.PluginHost.TryRegisterType((byte)'Q', this.SerializeQuaternion, this.DeserializeQuaternion);
            result &= this.PluginHost.TryRegisterType((byte)'P', this.SerializePhotonPlayer, this.DeserializePhotonPlayer);
            return result;
        }

        private byte[] SerializeVector2(Vector2 vec2)
        {
            var buf = new byte[sizeof(float) * 2];
            var i = 0;
            buf.Write(vec2.X, ref i);
            buf.Write(vec2.Y, ref i);
            return buf;
        }

        private Vector2 DeserializeVector2(byte[] buf)
        {
            var i = 0;
            var x = buf.Read<float>(ref i);
            var y = buf.Read<float>(ref i);
            return new Vector2(x, y);
        }

        private byte[] SerializeVector3(Vector3 vec3)
        {
            var buf = new byte[sizeof(float) * 3];
            var i = 0;
            buf.Write(vec3.X, ref i);
            buf.Write(vec3.Y, ref i);
            buf.Write(vec3.Z, ref i);
            return buf;
        }

        private Vector3 DeserializeVector3(byte[] buf)
        {
            var i = 0;
            var x = buf.Read<float>(ref i);
            var y = buf.Read<float>(ref i);
            var z = buf.Read<float>(ref i);
            return new Vector3(x, y, z);
        }

        private byte[] SerializeQuaternion(Quaternion q)
        {
            var buf = new byte[sizeof(float) * 4];
            var i = 0;
            buf.Write(q.W, ref i);
            buf.Write(q.X, ref i);
            buf.Write(q.Y, ref i);
            buf.Write(q.Z, ref i);
            return buf;
        }

        private Quaternion DeserializeQuaternion(byte[] buf)
        {
            var i = 0;
            var w = buf.Read<float>(ref i);
            var x = buf.Read<float>(ref i);
            var y = buf.Read<float>(ref i);
            var z = buf.Read<float>(ref i);
            return new Quaternion(x, y, z, w);
        }

        private byte[] SerializePhotonPlayer(IActor player)
        {
            var buf = new byte[sizeof(int)];
            var i = 0;
            buf.Write(player.ActorNr, ref i);
            return buf;
        }

        private IActor DeserializePhotonPlayer(byte[] buf)
        {
            var i = 0;
            var actorNr = buf.Read<int>(ref i);
            return this.PluginHost.GameActors.FirstOrDefault(e => e.ActorNr == actorNr);
        }
    }
}
