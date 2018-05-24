/*
 * Copyright (C) 2014 WarEmu
 *	http://WarEmu.com
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Common;
using FrameWork;
using WorldServer.Services.World;
using WorldServer.World.Objects.PublicQuests;

namespace WorldServer
{
    /* Quest: Grimmenhagen Burning
     * (needs verifing)
     * The quests requires you to kill 3 Tourchbearers and free 3 villagers from their burning
     * house by opening a door.
     * 
     * Whats working:
     * Because we cannot open doors. This will simply spawn a villager when a Tourchbearer dies.
     * You then click on the Villager and he will run away, thanking you for freeing him. However
     * you should 'free' him once you open the door to his house.
     * 
     * Todo:
     * - Support with doors
     */
    [GeneralScript(false, "", 46, 0)]
    public class FatherSigwaldQuestScript : AGeneralScript
    {
        private Object Obj;
        private int Quote = 0;
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;

            Obj.EvtInterface.AddEvent(SpawnBaddies, 9000, 5);
        }

        public void SpawnBaddies()
        {
            Creature_proto Proto = CreatureService.GetCreatureProto((uint)43);

            if (Proto == null)
                return;

            if (Quote == 0)
                Obj.Say("Hold your ground, hold your ground! ", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);
            else if (Quote == 1)
                Obj.Say("I see in your eyes the same fear that would take the heart of me.", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);
            else if (Quote == 2)
                Obj.Say("A day may come when the courage of men fails, when we forsake our friends and break all bonds of fellowship, but it is not this day.", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);
            else if (Quote == 3)
                Obj.Say("An hour of wolves and shattered shields, when the age of men comes crashing down!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);
            else if (Quote == 4)
                Obj.Say("But it is not this day! This day we fight!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);

            Quote++;

            Random rand = new Random();

            for (int i = 0; i < 3; i++)
            {
                Creature_spawn Spawn = new Creature_spawn();
                Spawn.Guid = (uint)CreatureService.GenerateCreatureSpawnGUID();
                Spawn.BuildFromProto(Proto);
                Spawn.WorldO = Obj.Heading;
                Spawn.WorldX = (int)(Obj.WorldPosition.X + 150 - 300 * rand.NextDouble());
                Spawn.WorldY = (int)(Obj.WorldPosition.Y + 150 - 300 * rand.NextDouble());
                Spawn.WorldZ = Obj.WorldPosition.Z;
                Spawn.ZoneId = Obj.Zone.ZoneId;
                Creature c = Obj.Region.CreateCreature(Spawn);
                //c.GetCreature().MvtInterface.WalkTo(Obj.WorldPosition.X, Obj.WorldPosition.Y, Obj.WorldPosition.Z, MovementInterface.CREATURE_SPEED);
                c.EvtInterface.AddEvent(c.Destroy, 20000, 1);
            }
            
        }

        public override void OnDie(Object Obj)
        {
            if (!(Obj is PQuestCreature))
                return;

            PQuestCreature Crea = Obj as PQuestCreature;

            Crea.Objective.Quest.Failed();

            Crea.Say("You fools!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);

            base.OnDie(Obj);
        }
    }
}
