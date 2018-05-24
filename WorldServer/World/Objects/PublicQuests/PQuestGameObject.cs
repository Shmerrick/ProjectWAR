using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorldServer.World.Objects.PublicQuests
{
    public class PQuestGameObject : GameObject
    {
        public PQuestObjective Objective;

        public PQuestGameObject(GameObject_spawn spawn, PQuestObjective objective)
        {
            this.Spawn = spawn;
            Name = spawn.Proto.Name;
            this.Objective = objective;
            this.Respawn = 0;
        }

        public override void RezUnit()
        {
            GameObject go = Region.CreateGameObject(Spawn);
            go.Respawn = 0;
            Destroy();
        }
    }
}
