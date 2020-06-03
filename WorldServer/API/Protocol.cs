using FrameWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WorldServer.Managers;
using WorldServer.Services.World;
using WorldServer.World.Objects;

namespace WorldServer.API
{
    public class Protocol
    {
        private Dictionary<Opcodes, FrameDelegate> _handlers = new Dictionary<Opcodes, FrameDelegate>();
        delegate void FrameDelegate(Client client, ApiPacket packet);

        private List<Client> _clients = new List<Client>();

        public Protocol()
        {
            foreach (MethodInfo field in this.GetType().GetMethods())
            {
                var attribs = field.GetCustomAttributes(typeof(ControlHandler), true);
                if (attribs != null && attribs.Length > 0)
                    _handlers[((ControlHandler)attribs[0]).OP] = (FrameDelegate)Delegate.CreateDelegate(typeof(FrameDelegate), this, field);
            }
        }

        public void OnFrame(Client client, ApiPacket packet)
        {
            try
            {
                if (_handlers.ContainsKey(packet.OP))
                {
                    _handlers[packet.OP](client, packet);
                }
            }
            catch (Exception)
            {
            }
        }

        public void RemoveClient(Client client)
        {
            lock (_clients)
                if (_clients.Contains(client))
                    _clients.Remove(client);
        }

        [ControlHandler(Opcodes.CHAR_TELEPORT)]
        public void CHAR_TELEPORT(Client client, ApiPacket packet)
        {
            var charId = packet.ReadUInt32();
            var zoneId = packet.ReadUInt16();
            var x = packet.ReadUInt32();
            var y = packet.ReadUInt32();
            var z = packet.ReadUInt16();

            Player player = null;
            lock (Player._Players)
                player = Player._Players.Where(e => e.CharacterId == charId).FirstOrDefault();
            if (player != null)
                player.Teleport(zoneId, x, y, (ushort)ClientFileMgr.GetHeight((int)zoneId, (int)x, (int)y), player.Heading);
        }


        [ControlHandler(Opcodes.CHAR_SEND_PACKET)]
        public void CHAR_SEND_PACKET(Client client, ApiPacket packet)
        {
            var op = packet.ReadByte();
            var charId = packet.ReadUInt32();
            var data = packet.ReadByteArray();

            Player player = null;
            lock (Player._Players)
                player = Player._Players.Where(e => e.CharacterId == charId).FirstOrDefault();
            if (player != null)
            {
                var Out = new PacketOut(op);
                Out.Write(data, 0, data.Length);
                player.SendPacket(Out);
            }
        }

        public void SendScriptError(Client client, string name, int line, string message)
        {
            var packet = new ApiPacket(Opcodes.EXECUTE_SCRIPT_ERROR);
            packet.WritePascalString(name);
            packet.WriteUInt32((uint)line);
            packet.WriteByteArray(System.Text.ASCIIEncoding.ASCII.GetBytes(message));
            client.SendPacket(packet);
        }

        public void SendScriptException(Client client, string name, string message)
        {
            var packet = new ApiPacket(Opcodes.EXECUTE_SCRIPT_EXCEPTION);
            packet.WritePascalString(name);
            packet.WriteByteArray(System.Text.ASCIIEncoding.ASCII.GetBytes(message));
            client.SendPacket(packet);
        }

        public void SendScriptOK(Client client, string name)
        {
            var packet = new ApiPacket(Opcodes.EXECUTE_SCRIPT_OK);
            packet.WritePascalString(name);
            client.SendPacket(packet);
        }

        public static void ScriptLog(Client client, string name, object data)
        {
            var packet = new ApiPacket(Opcodes.EXECUTE_SCRIPT_PRINT);
            packet.WritePascalString(name);
            packet.WriteByteArray(System.Text.ASCIIEncoding.ASCII.GetBytes(data.ToString()));
            client.SendPacket(packet);
        }

        [ControlHandler(Opcodes.EXECUTE_SCRIPT)]
        [Obsolete]
        public void EXECUTE_SCRIPT(Client client, ApiPacket packet)
        {
            var name = packet.ReadPascalString();
            var script = System.Text.ASCIIEncoding.ASCII.GetString(packet.ReadByteArray());
  
            int lineCount = 16;

            var template = @"using System;
                            using System.Collections.Generic;
                            using System.Linq;
                            using System.Linq.Expressions;
                            using System.Reflection;
                            using System.Text;
                            using System.Threading.Tasks;
                            using Common;
                            using FrameWork;

                            namespace WorldServer
                            {
                                public class ScriptEnv
                                {
		                            public void Execute()
		                            {
					                    [@@CODE@@]
		                            }
                                    private WorldServer.API.Client _client;
                                    private string _name;
                                    public ScriptEnv(WorldServer.API.Client client, string name)
                                    {
                                        _client = client;
                                        _name = name;
                                    }
                                }
                                public void Print(object data)
                                {
                                    WorldServer.API.Protocol.ScriptLog(_client, _name, data);
                                }
                            }";

            string result = template.Replace("[@@CODE@@]", script);
            System.CodeDom.Compiler.ICodeCompiler compiler = new Microsoft.CSharp.CSharpCodeProvider().CreateCompiler();
            System.CodeDom.Compiler.CompilerParameters param = new System.CodeDom.Compiler.CompilerParameters();
            param.GenerateExecutable = false;
            param.GenerateInMemory = true;

            param.IncludeDebugInformation = false;

            param.ReferencedAssemblies.Add("System.Windows.Forms.dll");
            param.ReferencedAssemblies.Add("System.dll");
            param.ReferencedAssemblies.Add("System.Core.dll");
            param.ReferencedAssemblies.Add("System.Data.dll");
            param.ReferencedAssemblies.Add("System.Deployment.dll");
            param.ReferencedAssemblies.Add("System.Drawing.dll");
            param.ReferencedAssemblies.Add("WorldServer.exe");
            param.ReferencedAssemblies.Add("Common.dll");
            param.ReferencedAssemblies.Add("FrameWork.dll");

            System.CodeDom.Compiler.CompilerResults results = compiler.CompileAssemblyFromSource(param, result);

            if (results.Errors.Count != 0)
            {
                if (results.Errors.Count != 0)
                    foreach (System.CodeDom.Compiler.CompilerError error in results.Errors)
                    {
                        SendScriptError(client, name, error.Line - lineCount, error.ErrorText);
                        break;
                    }
            }
            else
            {
                try
                {
                    var obj = results.CompiledAssembly.CreateInstance("WorldServer.ScriptEnv", false, BindingFlags.CreateInstance, null,
                            new object[] { client, name }, null, null);
                    obj.GetType().GetMethod("Execute").Invoke(obj, null);
                    SendScriptOK(client, name);
                }
                catch (Exception e)
                {
                    if(e.InnerException != null)
                        SendScriptException(client, name, e.InnerException.Message);
                    else
                        SendScriptException(client, name, e.Message);
                }
            }
        }

        [ControlHandler(Opcodes.SET_IMAGE_NUM)]
        public void SET_IMAGE_NUM(Client client, ApiPacket packet)
        {
            var charId = packet.ReadUInt32();
            var monsterID = packet.ReadUInt16();

            Player player = null;
            lock (Player._Players)
                player = Player._Players.Where(e => e.CharacterId == charId).FirstOrDefault();
            if (player != null)
            {
                var Out = new PacketOut(0x73);
                Out.WriteUInt16(player.Oid);
                Out.WriteUInt16(monsterID);
                Out.Fill(0, 18);
                player.DispatchPacket(Out, true);
            }
        }

        [ControlHandler(Opcodes.CHAR_ITEM_SET_SLOT_MODEL)]
        public void CHAR_ITEM_SET_SLOT_MODEL(Client client, ApiPacket packet)
        {
            var charId = packet.ReadUInt32();
            var slotIndex = packet.ReadUInt16();
            var modelID = packet.ReadUInt16();

            Player player = null;
            lock (Player._Players)
                player = Player._Players.Where(e => e.CharacterId == charId).FirstOrDefault();
            if (player != null)
            {
                var Out = new PacketOut(0xAA);
                var item = player.ItmInterface.GetItemInSlot(slotIndex);
                if (item != null)
                {
                    Out.WriteByte(1);
                    Out.Fill(0, 3);
                    Item.BuildItem(ref Out, item, null,null ,slotIndex, 0, player);
                    var pos = Out.Position;
                    Out.Position = 14;
                    Out.WriteUInt16(modelID);
                    Out.Position = pos;
                    player.SendPacket(Out);

                    Out = new PacketOut(0xBD); //F_PLAYER_INVENTORY
                    Out.WriteUInt16(player.Oid);
                    Out.WriteUInt16(1);
                    Out.WriteUInt16(slotIndex);
                    Out.WriteUInt16(modelID);
                    Out.WriteByte(0);
                    player.DispatchPacket(Out, false);
                }
            }
        }

        [ControlHandler(Opcodes.LOGIN)] //todo: send has instead of password
        public void LOGIN(Client client, ApiPacket packet)
        {
            var user = packet.ReadPascalString();
            var password = packet.ReadPascalString();

            lock (_clients)
                _clients.Add(client);
            SendOK(client);
        }

        [ControlHandler(Opcodes.ZONE_GET_LIST)]
        public void ZONE_GET_LIST(Client client, ApiPacket packet)
        {
            if (ZoneService._Zone_Info == null)
                return;

            var Out = new ApiPacket(Opcodes.ZONE_LIST);

            Out.WriteUInt16((byte)ZoneService._Zone_Info.Count);
            foreach (var zone in ZoneService._Zone_Info)
            {
                Out.WriteUInt16(zone.ZoneId);
                Out.WriteUInt32((uint)(zone.OffX << 12));
                Out.WriteUInt32((uint)(zone.OffY << 12));
                Out.WritePascalString(zone.Name);
            }
            client.SendPacket(Out);
        }

        [ControlHandler(Opcodes.GET_CHARACTER_LIST)] 
        public void GET_CHARACTER_LIST(Client client, ApiPacket packet)
        {
            var Out = new ApiPacket(Opcodes.CHAR_LIST);

            var players = new List<Player>();

            lock (Player._Players)
                players = Player._Players.ToList();

            Out.WriteUInt16((byte)players.Count);
            foreach (var player in players)
            {
                Out.WritePascalString(player.Name);
                Out.WriteUInt32(player.CharacterId);
                Out.WriteUInt16(player.Oid);
                Out.WriteUInt16(player.Zone.ZoneId);
                Out.WriteUInt16((ushort)player.X);
                Out.WriteUInt16((ushort)player.Y);
                Out.WriteUInt16((ushort)player.Z);
                Out.WriteUInt16(player.Heading);
                Out.WriteUInt32((uint)player.WorldPosition.X);
                Out.WriteUInt32((uint)player.WorldPosition.Y);
            }

            client.SendPacket(Out);
        }


        public void SendOK(Client client)
        {
            var packet = new ApiPacket(Opcodes.OK);
            client.SendPacket(packet);
        }

    }
    public class ControlHandler : Attribute
    {
        public Opcodes OP { get; set; }
        public ControlHandler(Opcodes op)
        {
            OP = op;
        }
    }

}
