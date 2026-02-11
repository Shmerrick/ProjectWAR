using FrameWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorldServer.NetWork.Handler
{
    class State2
    {

        public int PID;
        public int data4;
        public int MoveVelocity;
        public int FallTime;
        public int HasEnemyTarget;
        public int FreeFall;
        public int Action;
        public int Strafe;
        public int data32;
        public int Heartbeat;
        public int HasPosition;
        public float Heading;
        public int HasPositionChange;
        public int Grounded;
        public int X;
        public int Y;
        public int data60;
        public int ZoneID;
        public int Z;
        public int data70;
        public int data72;
        public int data76;
        public int data80;
        public int data84;
        public int TargetLos;
        public int data89;
        public int data90;
        public int data91;
        public int data92;
        public int GroundType;
        public int data100;
        public int Floating2;
        public int data102;
        public int data103;
        public int data104;
        public int data105;
        public int data106;
        public int data107;


        public void Read(byte[] data, int size)
        {
            PacketIn pIn = new PacketIn(data, 0, size);
            //pIn.SetBuffer(buffer, size);
            //pIn.ReadByte();
            //pIn.ReadByte();
            //pIn.ReadByte();

            //PID = pIn.GetInt16();
            //data4 = pIn.ReadByte();
            //MoveVelocity = pIn.GetIntClamped(-127, 325);
            //FallTime = pIn.GetIntClamped(-2000, 500);

            // 10 -1 92
            List<string> lst = new List<string>();
            for (int i = 0; i < 15; i++)
            {
                if (i > 10)
                lst.Add(i.ToString() + " " + pIn.ReadByte().ToString().PadLeft(3));
            }

            Log.Error("", string.Join(", ", lst));
            //pIn.Skip(7);
            byte test = (byte)pIn.ReadByte();
            HasEnemyTarget = test & 64;
            //pIn.ReadByte(); //2 or 3 readbyte here seems to give some correct data and would correspond with the original readint(2) of freefall
            //pIn.ReadByte();
            FreeFall = test & 0x1F; //3F pIn.ReadByte() & 48; //BitConverter.ToInt16(pIn.Read(2), 0);
            //Log.Error("freefall", Convert.ToString(test, 2));
            //pIn.ReadByte();
            //pIn.ReadByte();
            //pIn.ReadByte();
            Action = BitConverter.ToInt16(pIn.Read(2), 0); //pIn.ReadByte();
            //pIn.ReadByte();
            //pIn.ReadByte();
            //pIn.ReadByte();
            Strafe = BitConverter.ToInt16(pIn.Read(3), 0); //pIn.ReadByte();

            if (HasEnemyTarget > 0 && data4 == 0)
                data32 = pIn.ReadByte();

            if (data4 == 0)
                Heartbeat = BitConverter.ToInt16(pIn.Read(3), 0); //pIn.ReadByte();

            HasPosition = pIn.ReadByte();

            if (HasPosition > 0)
            {
                Heading = pIn.GetUint16(); // pIn.GetAngleRad(12);
                HasPositionChange = pIn.ReadByte();
                Grounded = pIn.ReadByte();

                if (HasPositionChange == 0)
                {
                    X = pIn.GetInt16();
                    Y = pIn.GetInt16();
                }
            }

            if (HasEnemyTarget > 0 && data4 == 0)
                data60 = pIn.ReadByte();

            if (HasPosition > 0)
            {
                if (HasPositionChange == 0)
                {
                    ZoneID = BitConverter.ToInt16(pIn.Read(9), 0); //pIn.GetInt16(); // (9)
                    Z = pIn.GetInt16();
                }
                else
                {
                    data72 = pIn.GetUint16();
                }
            }

            if (HasPosition > 0 && HasPositionChange > 0)
            {
                data76 = pIn.GetUint16();
                data80 = pIn.GetUint16();
                data84 = BitConverter.ToInt16(pIn.Read(9), 0); //pIn.GetInt16(); // (9)
            }

            if (HasEnemyTarget > 0)
            {
                if (data4 == 0)
                {
                    TargetLos = pIn.ReadByte();
                    data89 = pIn.ReadByte();
                    data90 = pIn.ReadByte();
                    data91 = pIn.ReadByte();
                    data92 = pIn.ReadByte();
                }
            }

            if ((data4 > 0) || (GroundType = BitConverter.ToInt16(pIn.Read(3), 0)) != 0) // (3)
            {
                data100 = pIn.ReadByte();
                Floating2 = pIn.ReadByte();
                data102 = pIn.ReadByte();
                data103 = pIn.ReadByte();
            }

            data104 = pIn.ReadByte();

            if (data4 == 0)
                data105 = pIn.ReadByte();

            data106 = pIn.ReadByte();
            data107 = pIn.ReadByte();
        }

        public override string ToString()
        {
            StringBuilder insb = new StringBuilder();
            insb.Append(" ; PID: ").Append(PID);
            insb.Append(" ; data4: ").Append(data4);
            insb.Append(" ; MoveVelocity: ").Append(MoveVelocity);
            insb.Append(" ; FallTime: ").Append(FallTime);
            insb.Append(" ; HasEnemyTarget: ").Append(HasEnemyTarget);
            insb.Append(" ; FreeFall: ").Append(FreeFall);
            insb.Append(" ; Action: ").Append(Action);
            insb.Append(" ; Strafe: ").Append(Strafe);
            insb.Append(" ; data32: ").Append(data32);
            insb.Append(" ; Heartbeat: ").Append(Heartbeat);
            insb.Append(" ; HasPosition: ").Append(HasPosition);
            insb.Append(" ; Heading: ").Append(Heading);
            insb.Append(" ; HasPositionChange: ").Append(HasPositionChange);
            insb.Append(" ; Grounded: ").Append(Grounded);
            insb.Append(" ; X: ").Append(X);
            insb.Append(" ; Y: ").Append(Y);
            insb.Append(" ; data60: ").Append(data60);
            insb.Append(" ; ZoneID: ").Append(ZoneID);
            insb.Append(" ; Z: ").Append(Z);
            insb.Append(" ; data70: ").Append(data70);
            insb.Append(" ; data72: ").Append(data72);
            insb.Append(" ; data76: ").Append(data76);
            insb.Append(" ; data80: ").Append(data80);
            insb.Append(" ; data84: ").Append(data84);
            insb.Append(" ; TargetLos: ").Append(TargetLos);
            insb.Append(" ; data89: ").Append(data89);
            insb.Append(" ; data90: ").Append(data90);
            insb.Append(" ; data91: ").Append(data91);
            insb.Append(" ; data92: ").Append(data92);
            insb.Append(" ; GroundType: ").Append(GroundType);
            insb.Append(" ; data100: ").Append(data100);
            insb.Append(" ; Floating2: ").Append(Floating2);
            insb.Append(" ; data102: ").Append(data102);
            insb.Append(" ; data103: ").Append(data103);
            insb.Append(" ; data104: ").Append(data104);
            insb.Append(" ; data105: ").Append(data105);
            insb.Append(" ; data106: ").Append(data106);
            insb.Append(" ; data107: ").Append(data107);
            return insb.ToString();
        }

        /*
        public  int Write(BitpInWriter& pIn, char* buffer, int size)
        {
            memset(buffer, 0, size);

            pIn.SetBuffer(buffer, size);

            pIn.WriteInt(PID, 16);
            pIn.WriteBit(data4);
            pIn.WriteIntClamped(MoveVelocity, -127, 325);
            pIn.WriteIntClamped(FallTime, -2000, 500);
            pIn.WriteBit(HasEnemyTarget);
            pIn.WriteInt(FreeFall, 2);
            pIn.WriteInt((int)Action, 3);
            pIn.WriteInt((int)Strafe, 3);

            if (HasEnemyTarget > 0 && data4 == 0)
                pIn.WriteBit(data32);

            if (data4 == 0)
                pIn.WriteInt(Heartbeat, 3);

            pIn.WriteBit(HasPosition);

            if (HasPosition > 0)
            {
                pIn.WriteAngleRad(Heading, 12);
                pIn.WriteBit(HasPositionChange);
                pIn.WriteBit(Grounded);

                if (HasPositionChange == 0)
                {
                    pIn.WriteInt(X, 16);
                    pIn.WriteInt(Y, 16);
                }
            }

            if (HasEnemyTarget > 0 && data4 == 0)
                pIn.WriteBit(data60);

            if (HasPosition > 0)
            {
                if (HasPositionChange == 0)
                {
                    pIn.WriteInt(ZoneID, 9);
                    pIn.WriteInt(Z, 16);
                }
                else
                {
                    pIn.WriteIntSigned(data72, 16);
                }
            }

            if (HasPosition > 0 && HasPositionChange > 0)
            {
                pIn.WriteIntSigned(data76, 16);
                pIn.WriteIntSigned(data80, 16);
                pIn.WriteInt(data84, 9);
            }

            if (HasEnemyTarget > 0)
            {
                if (data4 == 0)
                {
                    pIn.WriteBit(TargetLos);
                    pIn.WriteBit(data89);
                    pIn.WriteBit(data90);
                    pIn.WriteBit(data91);
                    pIn.WriteBit(data92);
                }
            }

            if ((data4 > 0) || GroundType != 0)
            {
                pIn.WriteBit(data100);
                pIn.WriteBit(Floating2);
                pIn.WriteBit(data102);
                pIn.WriteBit(data103);
            }

            pIn.WriteBit(data104);

            if (HasPosition == 0)
                pIn.WriteBit(data105);

            pIn.WriteBit(data106);
            pIn.WriteBit(data107);

            return pIn.getBytesWritten();
        }
        */
    }
}
