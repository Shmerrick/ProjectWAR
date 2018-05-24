using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorldServer.API
{
    public class ApiPacket : Packet
    {

        private ushort _sequenceID;

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
            //TODO: Check buffer size
            _size = size;
            _offset = 0;


            buf.Peek(_data, size + 1);

            OP = (Opcodes)ReadByte();
        }

        public void LoadFrame(int size)
        {
            //TODO: Check buffer size
            _size = size;
            _offset = 0;
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
