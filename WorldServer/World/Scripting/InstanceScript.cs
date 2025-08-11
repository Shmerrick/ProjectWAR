using WorldServer.World.Objects;
using WorldServer.World.Objects.Instances;

namespace WorldServer.World.Scripting
{
    /// <summary>
    /// Base class for instance-wide scripts.
    /// </summary>
    public abstract class InstanceScript
    {
        /// <summary>
        /// The instance this script is attached to.
        /// </summary>
        public Instance Instance { get; set; }

        /// <summary>
        /// Called when the instance and this script are first loaded.
        /// </summary>
        public virtual void OnInstanceLoad() { }

        /// <summary>
        /// Called periodically by the instance's update loop.
        /// </summary>
        /// <param name="tick">The server tick.</param>
        public virtual void Update(long tick) { }

        /// <summary>
        /// Called when a player enters the instance.
        /// </summary>
        /// <param name="player">The player who entered.</param>
        public virtual void OnPlayerEnter(Player player) { }

        /// <summary>
        /// Called when a player leaves the instance.
        /// </summary>
        /// <param name="player">The player who left.</param>
        public virtual void OnPlayerLeave(Player player) { }

        /// <summary>
        /// Called when a creature in the instance dies.
        /// </summary>
        /// <param name="creature">The creature that died.</param>
        /// <param name="killer">The unit that killed the creature.</param>
        public virtual void OnCreatureDie(Creature creature, Unit killer) { }

        /// <summary>
        /// Called when a game object in the instance is clicked by a player.
        /// </summary>
        /// <param name="go">The game object that was clicked.</param>
        /// <param name="player">The player who clicked it.</param>
        public virtual void OnGameObjectClick(GameObject go, Player player) { }
    }
}
