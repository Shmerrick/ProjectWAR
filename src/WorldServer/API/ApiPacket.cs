using FrameWork;

namespace WorldServer.API
{
    public class ApiPacket : Packet
    {
        public ushort _sequenceID;

        public ushort SequenceID
        {
            get { return _sequenceID; }
        }

        public ushort _sessionID;
        public ushort _unk1;
        public byte _unk2;

        public Opcodes OP;

        public ApiPacket(Opcodes op, int size = 0xFFFFF)
        {
            OP = op;
            _size = size;
            WriteUInt32(0); //length
            WriteByte((byte)op);
        }

        public void LoadFrame(int size, CircularBuffer buf)
        {
            _size = size;
            _offset = 0;

            if (size + 1 > _data.Length || size + 1 > buf.Data.Length)
            {
                Log.Error("ApiPacket", $"Frame size {size} exceeds buffer length");
                return;
            }

            if (!buf.Peek(_data, size + 1))
            {
                Log.Error("ApiPacket", $"Failed to peek {size + 1} bytes from buffer");
                return;
            }

            OP = (Opcodes)ReadByte();
        }

        public void LoadFrame(int size)
        {
            _size = size;
            _offset = 0;

            if (size + 5 > _data.Length)
            {
                Log.Error("ApiPacket", $"Frame size {size} exceeds internal buffer length");
                return;
            }

            ReadUInt32();
            OP = (Opcodes)ReadByte();
        }

        public void FinishPacket()
        {
            int offset = Offset;
            Offset = 0;
            WriteUInt32((ushort)(offset - 5));
            Offset = offset;
        }

        public override string ToString()
        {
            return "ControlPacket OP=" + OP.ToString() + " Size=" + _size;
        }
    }
}