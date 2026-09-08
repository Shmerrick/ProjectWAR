using System.Collections.Generic;
using FrameWork;
using GameData;

namespace WorldServer.Services.World
{
    /// <summary>
    /// Play as Skaven: the four monster forms a player can take at an Excavated Skaven Device.
    ///
    /// Shipped to live in Game Update 1.4.0 (2 November 2011) and maintained through 1.4.5; see
    /// <c>docs/patch-notes/1.4.0.md</c>. The 1.4.0 notes describe the mechanic:
    ///
    ///   "Players that assist in fighting back Thanquol's Incursion during a contested zone battle
    ///    will be able to help take over Skaven troops... Play as Skaven will be unlocked for all
    ///    players 15 minutes following activation of Skaven by participants in 'Thanquol's
    ///    Incursion.'"
    ///
    ///   "There will be a limited number of Skaven troops available within a lake. These slots are
    ///    on a first-come, first-serve basis and limited to a set amount per battle. The available
    ///    Skaven classes are: Gutter Runner, Engineer, Packmaster, and Rat Ogre."
    ///
    ///   "While controlling a Skaven troop, the player takes on the form, abilities, and stats of
    ///    the Skaven. Access to inventory or character screens are disabled."
    ///
    /// The three kits below are read off the wire, not inferred. In each of the official
    /// "CONTROL A ..." captures the server grants a form by re-sending F_CHARACTER_INFO subcode 1
    /// once per ability, each packet three bytes longer than the last; diffing consecutive packets
    /// yields the granted set exactly. Gutter Runner is confirmed by two independent captures.
    ///
    /// An earlier reading of <c>effects.csv</c>'s "[Start] 4820 - 4889 -- Skaven Play-As-Monster"
    /// block grouped the kits by entry ordering and got three things wrong, which the captures
    /// correct: 24853 Running with the Pack is shared by ALL forms rather than being Pack Master's,
    /// Death Globe belongs to the Warlock Engineer, and Residual Charge, Warp Lightning and Warp
    /// Energy Grenade are not granted at all despite sitting inside the Engineer's stretch of that
    /// block. Treat the effect block as a hint only.
    /// </summary>
    public static class SkavenFormService
    {
        public enum SkavenForm : byte
        {
            None = 0,
            WarlockEngineer = 1,
            GutterRunner = 2,
            RatOgre = 3,
            PackMaster = 4
        }

        public sealed class FormDefinition
        {
            public SkavenForm Form;

            /// <summary>Menu text, verbatim from the captured F_INTERACT_RESPONSE.</summary>
            public string MenuText;

            /// <summary>
            /// The ability id the live server's interact menu carried for this option. Recorded for
            /// fidelity; this server does not route selection through the quest system.
            /// </summary>
            public ushort LiveMenuId;

            /// <summary>Abilities granted, exactly as captured. Null for a form with no capture.</summary>
            public ushort[] Abilities;

            /// <summary>False where no capture shows the kit, so it must not be guessed.</summary>
            public bool KitIsEvidenced;
        }

        /// <summary>Granted by every captured form; not specific to any one of them.</summary>
        public const ushort RunningWithThePack = 24853;

        private static readonly Dictionary<SkavenForm, FormDefinition> Forms =
            new Dictionary<SkavenForm, FormDefinition>
            {
                {
                    SkavenForm.GutterRunner, new FormDefinition
                    {
                        Form = SkavenForm.GutterRunner,
                        MenuText = "Control a Gutter Runner",
                        LiveMenuId = 53055,
                        KitIsEvidenced = true,
                        // CONTROL A GUTTER RUNNER + play as a gutter runner; both agree.
                        Abilities = new ushort[]
                        {
                            24822, // Spin Slash
                            24823, // Leap
                            24824, // Snare Net
                            24825, // Gutter Run
                            24826, // Sabotage
                            24852, // Spy
                            RunningWithThePack
                        }
                    }
                },
                {
                    SkavenForm.WarlockEngineer, new FormDefinition
                    {
                        Form = SkavenForm.WarlockEngineer,
                        MenuText = "Control a Warlock Engineer",
                        LiveMenuId = 53056,
                        KitIsEvidenced = true,
                        Abilities = new ushort[]
                        {
                            24802, // Stored Warp-Energy
                            24805, // Death Globe
                            24806, // Warpfire Thrower
                            24807, // Repair
                            24808, // Doomrocket
                            24809, // Warp-Energy Condenser
                            24818, // Warp-Energy Accumulator
                            RunningWithThePack
                        }
                    }
                },
                {
                    SkavenForm.RatOgre, new FormDefinition
                    {
                        Form = SkavenForm.RatOgre,
                        MenuText = "Control a Rat Ogre",
                        LiveMenuId = 53057,
                        KitIsEvidenced = true,
                        Abilities = new ushort[]
                        {
                            24830, // Frenzy
                            24832, // Savage Assault
                            24833, // Roar
                            24834, // Charge
                            24835, // Hurl
                            24836, // Bash
                            RunningWithThePack
                        }
                    }
                },
                {
                    // Offered by the live menu and named in the 1.4.5 notes, but no capture shows
                    // anyone playing one, so its kit is unknown and is deliberately left empty
                    // rather than guessed from the effect block.
                    SkavenForm.PackMaster, new FormDefinition
                    {
                        Form = SkavenForm.PackMaster,
                        MenuText = "Control a Packmaster",
                        LiveMenuId = 53058,
                        KitIsEvidenced = false,
                        Abilities = null
                    }
                }
            };

        /// <summary>
        /// The "Controlled &lt;form&gt;" ability for a realm, or 0 if there is none.
        ///
        /// The live server applies this to the player as a buff when a form is taken. The
        /// "CONTROL A GUTTER RUNNER" capture carries it plainly -- F_INIT_EFFECTS (0xD7) with
        /// entry 0x611E little-endian, 24862 "Destruction Controlled Gutter Runner", zero
        /// duration, on the player's own Oid. All eight resolve to effect 4860, "Skaven PaM -
        /// FORM OF... A SKAVEN!".
        ///
        /// CORRECTION. An earlier version of this comment said each of these abilities carries a
        /// component with operation 51, career ability-set replacement, and that applying the buff
        /// therefore makes the client swap the bar from its own data. That is not true and was
        /// never checked: in the client's own records all eight have empty ComponentData and no
        /// rows at all in mythic_bin_abilitycomponentlink. They have no components. Whatever
        /// performs the transformation, it is not an op-51 component on these entries, and this
        /// server does not currently reproduce it -- taking a form grants the abilities and applies
        /// the buff, and the bar is not swapped or restored. See docs/SKAVEN_PLAY_AS_MONSTER.md.
        ///
        /// Granting the form's abilities cannot do it either: F_CHARACTER_INFO subcode 1 is
        /// cumulative, so it adds actions but never replaces or removes them.
        /// </summary>
        public static ushort GetControlAbility(SkavenForm form, Realms realm)
        {
            bool order = realm == Realms.REALMS_REALM_ORDER;

            switch (form)
            {
                case SkavenForm.WarlockEngineer: return order ? (ushort)24857 : (ushort)24861;
                case SkavenForm.GutterRunner:    return order ? (ushort)24858 : (ushort)24862;
                case SkavenForm.RatOgre:         return order ? (ushort)24859 : (ushort)24863;
                case SkavenForm.PackMaster:      return order ? (ushort)24860 : (ushort)24864;
                default: return 0;
            }
        }

        /// <summary>Every control ability, for clearing whichever one is applied.</summary>
        public static readonly ushort[] AllControlAbilities =
        {
            24857, 24858, 24859, 24860, 24861, 24862, 24863, 24864
        };

        /// <summary>Prototype of the Excavated Skaven Device, where a form is taken.</summary>
        public const uint DeviceEntry = 98811;

        public static IEnumerable<FormDefinition> AllForms
        {
            get
            {
                yield return Forms[SkavenForm.GutterRunner];
                yield return Forms[SkavenForm.WarlockEngineer];
                yield return Forms[SkavenForm.RatOgre];
                yield return Forms[SkavenForm.PackMaster];
            }
        }

        public static FormDefinition GetForm(SkavenForm form)
        {
            FormDefinition definition;
            return Forms.TryGetValue(form, out definition) ? definition : null;
        }

        /// <summary>Resolves a menu index (0-3, in the captured order) to its form.</summary>
        public static FormDefinition FromMenuIndex(int index)
        {
            switch (index)
            {
                case 0: return Forms[SkavenForm.GutterRunner];
                case 1: return Forms[SkavenForm.WarlockEngineer];
                case 2: return Forms[SkavenForm.RatOgre];
                case 3: return Forms[SkavenForm.PackMaster];
                default: return null;
            }
        }

        public static bool IsSkavenFormAbility(ushort entry)
        {
            foreach (KeyValuePair<SkavenForm, FormDefinition> pair in Forms)
            {
                ushort[] abilities = pair.Value.Abilities;
                if (abilities == null)
                    continue;

                for (int i = 0; i < abilities.Length; ++i)
                {
                    if (abilities[i] == entry)
                        return true;
                }
            }

            return false;
        }
    }
}
