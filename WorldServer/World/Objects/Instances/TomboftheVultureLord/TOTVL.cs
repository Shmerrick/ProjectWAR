using System;
using System.Threading;
using Common;
using FrameWork;
using WorldServer.Services.World;
using WorldServer.World.Interfaces;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Objects.Instances.TomboftheVultureLord
{
    class Pendulum : GameObject
    {
        byte Vfxstart;
        byte lastswing;
        byte updatestate;
        bool swinging = false;
        bool active = false;
        byte swings = 0;

        public bool damageleft = false;
        public bool damageright = false;

        public Pendulum(GameObject_proto proto, byte vfxstart, int WX, int WY, int WZ)
        {
            GameObject_spawn sp = new GameObject_spawn();
            sp.ZoneId = 179;
            sp.DisplayID = 7394;
            sp.WorldX = WX;
            sp.WorldY = WY;
            sp.WorldZ = WZ;
            sp.WorldO = 2048;
            sp.Proto = proto;
            VfxState = vfxstart;
            Spawn = sp;
            Name = proto.Name;
            Vfxstart = vfxstart;

            if (Vfxstart == 0)
                lastswing = 5;
            else
                lastswing = 6;

        }
        public void stop()
        {
            active = false;
        }

        public void start()
        {
            active = true;
            new Thread(swing).Start();
        }

        public void swing()
        {
            while (active)
            {
                Thread.Sleep(500);
                updatestate++;
                if (updatestate == 0)
                    updatestate = 1;


                if (swinging)
                    if (damageleft)
                    {
                        damageright = true;
                        damageleft = false;
                    }
                    else
                    {
                        damageright = false;
                        damageleft = true;
                    }

                if (updatestate % 2 == 0)
                {
                    swings++;
                    // Log.Info("Pendulum swings", ""+ swings);
                }
                if (swings == 7)
                {
                    swinging = false;
                    damageleft = false;
                    damageright = false;


                    //Log.Success("Pendulum stop", "");
                }
                if (updatestate % 16 != 0)
                    continue;

                swinging = true;
                swings = 1;
                if (lastswing == 5)
                {
                    damageleft = true;
                    UpdateVfxState(4);
                    VfxState = 4;
                    lastswing = 4;

                }
                else
                {
                    damageright = true;
                    UpdateVfxState(5);
                    VfxState = 5;
                    lastswing = 5;
                }

            }
            UpdateVfxState(Vfxstart);
            VfxState = Vfxstart;
        }

        // 0 = right swing fast stopping
        // 1 = left to right 1 swing
        // 2 = stop rechts fast stooping
        // 3 = right to left 1 swing 
        // 4 = start links stop rechts 7 schwünge
        // 5 = start rechts stop links 7 schwünge
        // 6 = start links stop rechts 10 schwünge laggy :-(





    }

    class Switch : GameObject
    {


    }
    class Firetrap : GameObject
    {
        byte counter = 0;
        public Boolean on = false;

        public Firetrap(GameObject_proto proto, int WX, int WY, int WZ)
        {


            GameObject_spawn sp = new GameObject_spawn();
            sp.ZoneId = 179;
            sp.DisplayID = proto.DisplayID;
            sp.WorldX = WX;
            sp.WorldY = WY;
            sp.WorldZ = WZ;
            sp.WorldO = 2048;
            sp.Proto = proto;
            VfxState = 1;
            sp.Unks[0] = 0;
            sp.Unks[1] = 1;
            sp.Unks[2] = 15266;
            sp.Unks[3] = 769;
            sp.Unks[4] = 5;
            sp.Unks[5] = 47790;
            Spawn = sp;
            Name = proto.Name;
        }

        public void FiretrapON()
        {
            UpdateVfxState(0);
            VfxState = 0;
            on = true;
        }
        public void FiretrapOFF()
        {
            UpdateVfxState(2);
            VfxState = 2;
            counter = 0;
            on = false;
        }
        public void Updates()
        {
            if (!on)
                return;

            if (counter > 3)
            {
                UpdateVfxState(1);
                VfxState = 1;
            }
            else
                counter++;
            lock (ObjectsInRange)
            {
                foreach (Object obj in ObjectsInRange)
                {
                    if (obj.IsPlayer() && Get2DDistanceToObject(obj) < 7)
                    {
                        Player plr = obj.GetPlayer();
                        plr.ReceiveDamage(this, int.MaxValue);

                        PacketOut damageOut = new PacketOut((byte)Opcodes.F_CAST_PLAYER_EFFECT, 24);

                        damageOut.WriteUInt16(Oid);
                        damageOut.WriteUInt16(plr.Oid);
                        damageOut.WriteUInt16(23584); // Terminate

                        damageOut.WriteByte(0);
                        damageOut.WriteByte(0); // DAMAGE EVENT
                        damageOut.WriteByte(7);

                        damageOut.WriteZigZag(-30000);
                        damageOut.WriteByte(0);

                        plr.DispatchPacketUnreliable(damageOut, true, this);
                    }
                }
            }
            //checkdamage
        }


    }
    class DartTrap : GameObject
    {
        public byte Vfxstart;

        public DartTrap(GameObject_proto proto, byte vfxstart, int WX, int WY, int WZ)
        {
            GameObject_spawn sp = new GameObject_spawn();
            sp.ZoneId = 179;
            sp.DisplayID = 7471;
            sp.WorldX = WX;
            sp.WorldY = WY;
            sp.WorldZ = WZ;
            sp.WorldO = 1012;
            sp.Proto = proto;
            VfxState = vfxstart;
            Spawn = sp;
            Name = proto.Name;
            Vfxstart = vfxstart;
        }

        public void rotateR()
        {
            VfxState++;
            UpdateVfxState(VfxState);
            if (VfxState > 8)
                VfxState = 1;
        }
        public void rotateL()
        {
            VfxState++;
            UpdateVfxState(VfxState);
            if (VfxState > 16)
                VfxState = 9;
        }
        public void Fire()
        {
            AbtInterface.StartCast(this, 9101, 1);
        }



    }
    class Pillar : GameObject
    {


    }


    class TOTVL : Instance
    {

        private GameObject[] _Pendulums = new GameObject[12];
        private GameObject[] _Firetrap = new GameObject[44];
        private GameObject[] _Darttrap = new GameObject[11];

        private EventInterface _evtInterface;

        bool Pendulum_Trap_init = false;
        bool Pendulum_Left_active = false;
#pragma warning disable IDE0052 // Удалить непрочитанные закрытые члены
        bool Pendulum_Right_active = false;
#pragma warning restore IDE0052 // Удалить непрочитанные закрытые члены
        bool Firetrap_active = true;
        bool Darttrap_active = true;


        public TOTVL(ushort zoneid, ushort id, byte realm, Instance_Lockouts lockouts) : base(zoneid, id, realm, lockouts)
        {
            _evtInterface = new EventInterface();
            ZoneID = zoneid;
            createPenulums();

            StartFireTrap();
            StartDartTrap();


        }

        public void StopPendulumtrapl()
        {
            for (int i = 0; i < 12; i += 2)
            {
                ((Pendulum)_Pendulums[i]).stop();
            }
            Pendulum_Left_active = false;
        }

        public void StopPendulumtrapr()
        {
            for (int i = 1; i < 12; i += 2)
            {
                ((Pendulum)_Pendulums[i]).stop();
            }
            Pendulum_Right_active = false;
        }

        public void StartPendulumtrapl()
        {
            for (int i = 0; i < 12; i += 2)
            {
                ((Pendulum)_Pendulums[i]).start();
                Thread.Sleep(300);
            }
        }

        public void StartPendulumtrapr()
        {
            for (int i = 1; i < 12; i += 2)
            {
                ((Pendulum)_Pendulums[i]).start();
                Thread.Sleep(300);
            }
        }

        public void StopFireTrap()
        {
            Firetrap_active = false;

        }
        public void StartFireTrap()
        {
            Firetrap_active = true;
            new Thread(Firetrap).Start();

        }



        public void StopDartTrap()
        {
            Darttrap_active = false;

        }
        public void StartDartTrap()
        {
            Darttrap_active = true;
            new Thread(Darttrap).Start();

        }


        public void checkpendulumdamage(Player plr, ushort x, ushort y)
        {
            if (x > 53813 && x < 53911)     // first row
                if (y > 37111 && ((Pendulum)_Pendulums[0]).damageleft)
                    pendulumkill(plr, (Pendulum)_Pendulums[0]);
                else if (y < 37111 && ((Pendulum)_Pendulums[0]).damageright)
                    pendulumkill(plr, (Pendulum)_Pendulums[0]);






            //StopPendulumtrapl();
            if (!Pendulum_Trap_init)
            {
                new Thread(StartPendulumtrapl).Start();
                new Thread(StartPendulumtrapr).Start();
                Pendulum_Trap_init = true;
            }
        }

        public void pendulumkill(Player plr, Pendulum p)
        {
            plr.ReceiveDamage(p, int.MaxValue);

            PacketOut damageOut = new PacketOut((byte)Opcodes.F_CAST_PLAYER_EFFECT, 24);

            damageOut.WriteUInt16(p.Oid);
            damageOut.WriteUInt16(plr.Oid);
            damageOut.WriteUInt16(23584); // Terminate

            damageOut.WriteByte(0);
            damageOut.WriteByte(0); // DAMAGE EVENT
            damageOut.WriteByte(7);

            damageOut.WriteZigZag(-30000);
            damageOut.WriteByte(0);

            plr.DispatchPacketUnreliable(damageOut, true, p);
        }

        public void Darttrap()
        {
            byte counter = 0;

            while (Darttrap_active)
            {


                counter++;
                if (counter == 6)
                {


                    ((DartTrap)_Darttrap[0]).rotateL();
                    ((DartTrap)_Darttrap[1]).rotateR();
                    ((DartTrap)_Darttrap[2]).rotateL();
                    ((DartTrap)_Darttrap[3]).rotateR();
                    ((DartTrap)_Darttrap[4]).rotateR();
                    ((DartTrap)_Darttrap[5]).rotateR();
                    ((DartTrap)_Darttrap[6]).rotateL();
                    ((DartTrap)_Darttrap[7]).rotateL();
                    ((DartTrap)_Darttrap[8]).rotateR();



                    counter = 0;
                }

                Thread.Sleep(500);
                for (int i = 0; i < 8; i++)
                    ((DartTrap)_Darttrap[i]).Fire();

            }
        }

        public void Firetrap()
        {
            // pattern
            byte counter = 0;
            bool[] mainpattern1 = { false, false, false, false, true, true, true, true, true, false, true, true, false, false, false, false, true, true, true, true, true, true, true, true };
            bool[] mainpattern2 = { false, true, true, true, false, false, false, false, true, true, true, true, true, true, true, false, false, false, false, false, true, true, true, true };
            bool[] mainpattern3 = { true, true, true, true, true, true, true, false, false, false, false, false, true, true, true, true, true, true, false, true, false, false, false, false };
            //mainpattern 6 
            //front 3

            while (Firetrap_active)
            {
                if (counter == 0)
                {
                    for (int i = 0; i < 5; i++)
                        ((Firetrap)_Firetrap[i]).FiretrapON();
                    for (int i = 5; i < 10; i++)
                        ((Firetrap)_Firetrap[i]).FiretrapON();

                    for (int i = 10; i < 15; i++)
                        ((Firetrap)_Firetrap[i]).FiretrapOFF();
                    for (int i = 15; i < 20; i++)
                        ((Firetrap)_Firetrap[i]).FiretrapOFF();

                    // mainpattern 1
                    for (int i = 20; i < 44; i++)
                    {
                        if (mainpattern1[i - 20] && !((Firetrap)_Firetrap[i]).on)
                            ((Firetrap)_Firetrap[i]).FiretrapON();
                        else if (!mainpattern1[i - 20] && ((Firetrap)_Firetrap[i]).on)
                            ((Firetrap)_Firetrap[i]).FiretrapOFF();
                    }

                }
                else if (counter == 6)
                {
                    for (int i = 5; i < 10; i++)
                        ((Firetrap)_Firetrap[i]).FiretrapOFF();

                    for (int i = 10; i < 15; i++)
                        ((Firetrap)_Firetrap[i]).FiretrapOFF();
                    // frnt back pattern 2
                }
                else if (counter == 12)
                {
                    for (int i = 0; i < 5; i++)
                        ((Firetrap)_Firetrap[i]).FiretrapOFF();
                    for (int i = 5; i < 10; i++)
                        ((Firetrap)_Firetrap[i]).FiretrapON();

                    for (int i = 10; i < 15; i++)
                        ((Firetrap)_Firetrap[i]).FiretrapON();
                    for (int i = 15; i < 20; i++)
                        ((Firetrap)_Firetrap[i]).FiretrapOFF();

                    // frnt back pattern 3
                    // main 2
                    for (int i = 20; i < 44; i++)
                    {
                        if (mainpattern2[i - 20] && !((Firetrap)_Firetrap[i]).on)
                            ((Firetrap)_Firetrap[i]).FiretrapON();
                        else if (!mainpattern2[i - 20] && ((Firetrap)_Firetrap[i]).on)
                            ((Firetrap)_Firetrap[i]).FiretrapOFF();
                    }
                }
                else if (counter == 18)
                {
                    for (int i = 5; i < 10; i++)
                        ((Firetrap)_Firetrap[i]).FiretrapOFF();

                    for (int i = 15; i < 20; i++)
                        ((Firetrap)_Firetrap[i]).FiretrapON();

                    // frnt back pattern 4

                }
                else if (counter == 24)
                {
                    for (int i = 5; i < 10; i++)
                        ((Firetrap)_Firetrap[i]).FiretrapON();

                    for (int i = 10; i < 15; i++)
                        ((Firetrap)_Firetrap[i]).FiretrapOFF();

                    // frnt back pattern 5
                    // main 3
                    for (int i = 20; i < 44; i++)
                    {
                        if (mainpattern3[i - 20] && !((Firetrap)_Firetrap[i]).on)
                            ((Firetrap)_Firetrap[i]).FiretrapON();
                        else if (!mainpattern3[i - 20] && ((Firetrap)_Firetrap[i]).on)
                            ((Firetrap)_Firetrap[i]).FiretrapOFF();
                    }
                }
                else if (counter == 30)
                {
                    for (int i = 0; i < 5; i++)
                        ((Firetrap)_Firetrap[i]).FiretrapOFF();
                    for (int i = 5; i < 10; i++)
                        ((Firetrap)_Firetrap[i]).FiretrapON();

                    for (int i = 10; i < 15; i++)
                        ((Firetrap)_Firetrap[i]).FiretrapON();
                    for (int i = 15; i < 20; i++)
                        ((Firetrap)_Firetrap[i]).FiretrapOFF();
                    // frnt back pattern 6

                }
                counter++;
                if (counter == 36)
                    counter = 0;
                for (int i = 0; i < 44; i++)
                    ((Firetrap)_Firetrap[i]).Updates();

                Thread.Sleep(500);
            }
            for (int i = 0; i < 44; i++)
                ((Firetrap)_Firetrap[i]).FiretrapOFF();
        }

        public void Doors()
        {

            //259293
            //259294

            //Region.GetGameObject(259293).GetGameObject().UpdateVfxState(1);
            // Region.GetGameObject(259293).GetGameObject().Spawn.
            //Region.GetGameObject(259294).GetGameObject().UpdateVfxState(1);
        }


        public void createPenulums()
        {
            GameObject_proto proto;
            GameObjectService.GameObjectProtos.TryGetValue(98908, out proto);

            Firetrap ft;
            Pendulum go;
            DartTrap dt;

            //left side
            go = new Pendulum(proto, 2, 348784, 282876, 12659);
            _Pendulums[0] = go;
            Region.AddObject(go, ZoneID, true);
            go = new Pendulum(proto, 0, 348664, 282876, 12659);
            _Pendulums[2] = go;
            Region.AddObject(go, ZoneID, true);
            go = new Pendulum(proto, 0, 348544, 282876, 12659);
            _Pendulums[4] = go;
            Region.AddObject(go, ZoneID, true);
            go = new Pendulum(proto, 2, 348424, 282876, 12659);
            _Pendulums[6] = go;
            Region.AddObject(go, ZoneID, true);
            go = new Pendulum(proto, 2, 348304, 282876, 12659);
            _Pendulums[8] = go;
            Region.AddObject(go, ZoneID, true);
            go = new Pendulum(proto, 0, 348184, 282876, 12659);
            _Pendulums[10] = go;
            Region.AddObject(go, ZoneID, true);

            //rightside

            go = new Pendulum(proto, 0, 348784, 280031, 12659);
            _Pendulums[1] = go;
            Region.AddObject(go, ZoneID, true);
            go = new Pendulum(proto, 2, 348664, 280031, 12659);
            _Pendulums[3] = go;
            Region.AddObject(go, ZoneID, true);
            go = new Pendulum(proto, 0, 348544, 280031, 12659);
            _Pendulums[5] = go;
            Region.AddObject(go, ZoneID, true);
            go = new Pendulum(proto, 2, 348424, 280031, 12659);
            _Pendulums[7] = go;
            Region.AddObject(go, ZoneID, true);
            go = new Pendulum(proto, 2, 348304, 280031, 12659);
            _Pendulums[9] = go;
            Region.AddObject(go, ZoneID, true);
            go = new Pendulum(proto, 0, 348184, 280031, 12659);
            _Pendulums[11] = go;
            Region.AddObject(go, ZoneID, true);


            //firetraps
            //frontrow
            GameObjectService.GameObjectProtos.TryGetValue(100490, out proto);
            ft = new Firetrap(proto, 341050, 276799, 13371);
            _Firetrap[0] = ft;
            Region.AddObject(ft, ZoneID, true);
            GameObjectService.GameObjectProtos.TryGetValue(100490, out proto);
            ft = new Firetrap(proto, 341050, 276932, 13371);
            _Firetrap[1] = ft;
            Region.AddObject(ft, ZoneID, true);
            GameObjectService.GameObjectProtos.TryGetValue(100490, out proto);
            ft = new Firetrap(proto, 341050, 277329, 13371);
            _Firetrap[2] = ft;
            Region.AddObject(ft, ZoneID, true);
            GameObjectService.GameObjectProtos.TryGetValue(100490, out proto);
            ft = new Firetrap(proto, 341050, 277197, 13371);
            _Firetrap[3] = ft;
            Region.AddObject(ft, ZoneID, true);
            GameObjectService.GameObjectProtos.TryGetValue(100490, out proto);
            ft = new Firetrap(proto, 341050, 277065, 13371);
            _Firetrap[4] = ft;
            Region.AddObject(ft, ZoneID, true);
            GameObjectService.GameObjectProtos.TryGetValue(100490, out proto);
            ft = new Firetrap(proto, 341182, 276799, 13371);
            _Firetrap[5] = ft;
            Region.AddObject(ft, ZoneID, true);
            GameObjectService.GameObjectProtos.TryGetValue(100490, out proto);
            ft = new Firetrap(proto, 341182, 276932, 13371);
            _Firetrap[6] = ft;
            Region.AddObject(ft, ZoneID, true);
            GameObjectService.GameObjectProtos.TryGetValue(100490, out proto);
            ft = new Firetrap(proto, 341182, 277329, 13371);
            _Firetrap[7] = ft;
            Region.AddObject(ft, ZoneID, true);
            GameObjectService.GameObjectProtos.TryGetValue(100490, out proto);
            ft = new Firetrap(proto, 341182, 277197, 13371);
            _Firetrap[8] = ft;
            Region.AddObject(ft, ZoneID, true);
            GameObjectService.GameObjectProtos.TryGetValue(100490, out proto);
            ft = new Firetrap(proto, 341182, 277065, 13371);
            _Firetrap[9] = ft;
            Region.AddObject(ft, ZoneID, true);

            //backrow

            GameObjectService.GameObjectProtos.TryGetValue(100490, out proto);
            ft = new Firetrap(proto, 342397, 276799, 13371);
            _Firetrap[10] = ft;
            Region.AddObject(ft, ZoneID, true);
            GameObjectService.GameObjectProtos.TryGetValue(100490, out proto);
            ft = new Firetrap(proto, 342397, 276932, 13371);
            _Firetrap[11] = ft;
            Region.AddObject(ft, ZoneID, true);
            GameObjectService.GameObjectProtos.TryGetValue(100490, out proto);
            ft = new Firetrap(proto, 342397, 277329, 13371);
            _Firetrap[12] = ft;
            Region.AddObject(ft, ZoneID, true);
            GameObjectService.GameObjectProtos.TryGetValue(100490, out proto);
            ft = new Firetrap(proto, 342397, 277197, 13371);
            _Firetrap[13] = ft;
            Region.AddObject(ft, ZoneID, true);
            GameObjectService.GameObjectProtos.TryGetValue(100490, out proto);
            ft = new Firetrap(proto, 342397, 277065, 13371);
            _Firetrap[14] = ft;
            Region.AddObject(ft, ZoneID, true);
            GameObjectService.GameObjectProtos.TryGetValue(100490, out proto);
            ft = new Firetrap(proto, 342529, 276799, 13371);
            _Firetrap[15] = ft;
            Region.AddObject(ft, ZoneID, true);
            GameObjectService.GameObjectProtos.TryGetValue(100490, out proto);
            ft = new Firetrap(proto, 342529, 276932, 13371);
            _Firetrap[16] = ft;
            Region.AddObject(ft, ZoneID, true);
            GameObjectService.GameObjectProtos.TryGetValue(100490, out proto);
            ft = new Firetrap(proto, 342529, 277329, 13371);
            _Firetrap[17] = ft;
            Region.AddObject(ft, ZoneID, true);
            GameObjectService.GameObjectProtos.TryGetValue(100490, out proto);
            ft = new Firetrap(proto, 342529, 277197, 13371);
            _Firetrap[18] = ft;
            Region.AddObject(ft, ZoneID, true);
            GameObjectService.GameObjectProtos.TryGetValue(100490, out proto);
            ft = new Firetrap(proto, 342529, 277065, 13371);
            _Firetrap[19] = ft;
            Region.AddObject(ft, ZoneID, true);

            //mainfield

            GameObjectService.GameObjectProtos.TryGetValue(100490, out proto);
            ft = new Firetrap(proto, 341458, 276866, 13406);
            _Firetrap[20] = ft;
            Region.AddObject(ft, ZoneID, true);
            GameObjectService.GameObjectProtos.TryGetValue(100490, out proto);
            ft = new Firetrap(proto, 341458, 277131, 13406);
            _Firetrap[21] = ft;
            Region.AddObject(ft, ZoneID, true);
            GameObjectService.GameObjectProtos.TryGetValue(100490, out proto);
            ft = new Firetrap(proto, 341458, 276999, 13406);
            _Firetrap[22] = ft;
            Region.AddObject(ft, ZoneID, true);
            GameObjectService.GameObjectProtos.TryGetValue(100490, out proto);
            ft = new Firetrap(proto, 341458, 277263, 13406);
            _Firetrap[23] = ft;
            Region.AddObject(ft, ZoneID, true);

            GameObjectService.GameObjectProtos.TryGetValue(100490, out proto);
            ft = new Firetrap(proto, 341590, 276866, 13406);
            _Firetrap[24] = ft;
            Region.AddObject(ft, ZoneID, true);
            GameObjectService.GameObjectProtos.TryGetValue(100490, out proto);
            ft = new Firetrap(proto, 341590, 277131, 13406);
            _Firetrap[25] = ft;
            Region.AddObject(ft, ZoneID, true);
            GameObjectService.GameObjectProtos.TryGetValue(100490, out proto);
            ft = new Firetrap(proto, 341590, 276999, 13406);
            _Firetrap[26] = ft;
            Region.AddObject(ft, ZoneID, true);
            GameObjectService.GameObjectProtos.TryGetValue(100490, out proto);
            ft = new Firetrap(proto, 341590, 277263, 13406);
            _Firetrap[27] = ft;
            Region.AddObject(ft, ZoneID, true);

            GameObjectService.GameObjectProtos.TryGetValue(100490, out proto);
            ft = new Firetrap(proto, 341722, 276866, 13406);
            _Firetrap[28] = ft;
            Region.AddObject(ft, ZoneID, true);
            GameObjectService.GameObjectProtos.TryGetValue(100490, out proto);
            ft = new Firetrap(proto, 341722, 277131, 13406);
            _Firetrap[29] = ft;
            Region.AddObject(ft, ZoneID, true);
            GameObjectService.GameObjectProtos.TryGetValue(100490, out proto);
            ft = new Firetrap(proto, 341722, 276999, 13406);
            _Firetrap[30] = ft;
            Region.AddObject(ft, ZoneID, true);
            GameObjectService.GameObjectProtos.TryGetValue(100490, out proto);
            ft = new Firetrap(proto, 341722, 277263, 13406);
            _Firetrap[31] = ft;
            Region.AddObject(ft, ZoneID, true);

            GameObjectService.GameObjectProtos.TryGetValue(100490, out proto);
            ft = new Firetrap(proto, 341854, 276866, 13406);
            _Firetrap[32] = ft;
            Region.AddObject(ft, ZoneID, true);
            GameObjectService.GameObjectProtos.TryGetValue(100490, out proto);
            ft = new Firetrap(proto, 341854, 277131, 13406);
            _Firetrap[33] = ft;
            Region.AddObject(ft, ZoneID, true);
            GameObjectService.GameObjectProtos.TryGetValue(100490, out proto);
            ft = new Firetrap(proto, 341854, 276999, 13406);
            _Firetrap[34] = ft;
            Region.AddObject(ft, ZoneID, true);
            GameObjectService.GameObjectProtos.TryGetValue(100490, out proto);
            ft = new Firetrap(proto, 341854, 277263, 13406);
            _Firetrap[35] = ft;
            Region.AddObject(ft, ZoneID, true);

            GameObjectService.GameObjectProtos.TryGetValue(100490, out proto);
            ft = new Firetrap(proto, 341986, 276866, 13406);
            _Firetrap[36] = ft;
            Region.AddObject(ft, ZoneID, true);
            GameObjectService.GameObjectProtos.TryGetValue(100490, out proto);
            ft = new Firetrap(proto, 341986, 277131, 13406);
            _Firetrap[37] = ft;
            Region.AddObject(ft, ZoneID, true);
            GameObjectService.GameObjectProtos.TryGetValue(100490, out proto);
            ft = new Firetrap(proto, 341986, 276999, 13406);
            _Firetrap[38] = ft;
            Region.AddObject(ft, ZoneID, true);
            GameObjectService.GameObjectProtos.TryGetValue(100490, out proto);
            ft = new Firetrap(proto, 341986, 277263, 13406);
            _Firetrap[39] = ft;
            Region.AddObject(ft, ZoneID, true);

            GameObjectService.GameObjectProtos.TryGetValue(100490, out proto);
            ft = new Firetrap(proto, 342118, 276866, 13406);
            _Firetrap[40] = ft;
            Region.AddObject(ft, ZoneID, true);
            GameObjectService.GameObjectProtos.TryGetValue(100490, out proto);
            ft = new Firetrap(proto, 342118, 277131, 13406);
            _Firetrap[41] = ft;
            Region.AddObject(ft, ZoneID, true);
            GameObjectService.GameObjectProtos.TryGetValue(100490, out proto);
            ft = new Firetrap(proto, 342118, 276999, 13406);
            _Firetrap[42] = ft;
            Region.AddObject(ft, ZoneID, true);
            GameObjectService.GameObjectProtos.TryGetValue(100490, out proto);
            ft = new Firetrap(proto, 342118, 277263, 13406);
            _Firetrap[43] = ft;
            Region.AddObject(ft, ZoneID, true);



            //Darttrap

            GameObjectService.GameObjectProtos.TryGetValue(100489, out proto);
            dt = new DartTrap(proto, 10, 340385, 281620, 13050);
            _Darttrap[0] = dt;
            Region.AddObject(dt, ZoneID, true);

            GameObjectService.GameObjectProtos.TryGetValue(100489, out proto);
            dt = new DartTrap(proto, 2, 340385, 281290, 13050);
            _Darttrap[1] = dt;
            Region.AddObject(dt, ZoneID, true);

            GameObjectService.GameObjectProtos.TryGetValue(100489, out proto);
            dt = new DartTrap(proto, 11, 339883, 281603, 13085);
            _Darttrap[2] = dt;
            Region.AddObject(dt, ZoneID, true);

            GameObjectService.GameObjectProtos.TryGetValue(100489, out proto);
            dt = new DartTrap(proto, 3, 339883, 281302, 13085);
            _Darttrap[3] = dt;
            Region.AddObject(dt, ZoneID, true);

            GameObjectService.GameObjectProtos.TryGetValue(100489, out proto);
            dt = new DartTrap(proto, 6, 339675, 281456, 13085);
            _Darttrap[4] = dt;
            Region.AddObject(dt, ZoneID, true);

            GameObjectService.GameObjectProtos.TryGetValue(100489, out proto);
            dt = new DartTrap(proto, 9, 339469, 281601, 13085);
            _Darttrap[5] = dt;
            Region.AddObject(dt, ZoneID, true);

            GameObjectService.GameObjectProtos.TryGetValue(100489, out proto);
            dt = new DartTrap(proto, 17, 339469, 281304, 13085);
            _Darttrap[6] = dt;
            Region.AddObject(dt, ZoneID, true);

            GameObjectService.GameObjectProtos.TryGetValue(100489, out proto);
            dt = new DartTrap(proto, 14, 339064, 281596, 13050);
            _Darttrap[7] = dt;
            Region.AddObject(dt, ZoneID, true);

            GameObjectService.GameObjectProtos.TryGetValue(100489, out proto);
            dt = new DartTrap(proto, 6, 339064, 281296, 13050);
            _Darttrap[8] = dt;
            Region.AddObject(dt, ZoneID, true);



            /*

            foreach (var p in GameObjectService.GameObjectSpawns.Where(e => e.Value.ZoneId == 179))
            {
                if (p.Value.Entry == 98908)
                {
                    GameObject go = new GameObject(p.Value);

                    _Objects.Add(go);
                    //Region.AddObject(go, zoneid, true);

                }
            }

    */


        }


    }
}
