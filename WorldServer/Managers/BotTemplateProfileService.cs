using Common;
using FrameWork;
using System;
using System.Collections.Generic;
using WorldServer.World.Objects;

namespace WorldServer.Managers
{
    public static class BotTemplateProfileService
    {
        private static readonly object SyncRoot = new object();
        private static readonly Dictionary<uint, BotTemplateProfile> ProfileCache = new Dictionary<uint, BotTemplateProfile>();
        private static readonly HashSet<uint> MissingProfiles = new HashSet<uint>();

        public static bool TryGetVariantIndex(uint characterId, out byte variantIndex)
        {
            lock (SyncRoot)
            {
                BotTemplateProfile profile = GetOrLoadProfileUnsafe(characterId);
                if (profile != null)
                {
                    variantIndex = profile.VariantIndex;
                    return true;
                }

                variantIndex = 0;
                return false;
            }
        }

        public static byte ResolveVariantIndex(uint characterId, BotRole role)
        {
            lock (SyncRoot)
            {
                BotTemplateProfile profile = GetOrLoadProfileUnsafe(characterId);
                if (profile != null)
                    return profile.VariantIndex;

                return GetFallbackVariantIndex(characterId, role);
            }
        }

        public static void SetVariantIndex(uint characterId, byte variantIndex)
        {
            lock (SyncRoot)
            {
                BotTemplateProfile profile = GetOrLoadProfileUnsafe(characterId);
                if (profile == null)
                {
                    profile = new BotTemplateProfile
                    {
                        CharacterId = characterId,
                        VariantIndex = variantIndex
                    };

                    CharMgr.Database.AddObject(profile);
                }
                else
                {
                    profile.VariantIndex = variantIndex;
                    CharMgr.Database.SaveObject(profile);
                }

                ProfileCache[characterId] = profile;
                MissingProfiles.Remove(characterId);
                CharMgr.Database.ForceSave();
            }
        }

        public static void RemoveVariantIndex(uint characterId)
        {
            lock (SyncRoot)
            {
                BotTemplateProfile profile = GetOrLoadProfileUnsafe(characterId);
                if (profile != null)
                    CharMgr.Database.DeleteObject(profile);

                ProfileCache.Remove(characterId);
                MissingProfiles.Add(characterId);
                CharMgr.Database.ForceSave();
            }
        }

        private static BotTemplateProfile GetOrLoadProfileUnsafe(uint characterId)
        {
            if (ProfileCache.TryGetValue(characterId, out BotTemplateProfile profile))
                return profile;

            if (MissingProfiles.Contains(characterId))
                return null;

            profile = CharMgr.Database.SelectObject<BotTemplateProfile>($"CharacterId={characterId}");
            if (profile == null)
            {
                MissingProfiles.Add(characterId);
                return null;
            }

            ProfileCache[characterId] = profile;
            return profile;
        }

        private static byte GetFallbackVariantIndex(uint characterId, BotRole role)
        {
            Character character = CharMgr.GetCharacter(characterId, false);
            string name = character?.Name;
            if (!string.IsNullOrWhiteSpace(name))
            {
                if (name.EndsWith("_OT", StringComparison.OrdinalIgnoreCase)
                    || name.EndsWith("_M2", StringComparison.OrdinalIgnoreCase))
                    return 1;

                if (name.EndsWith("_H", StringComparison.OrdinalIgnoreCase)
                    || name.EndsWith("_R", StringComparison.OrdinalIgnoreCase)
                    || name.EndsWith("_MT", StringComparison.OrdinalIgnoreCase)
                    || name.EndsWith("_M1", StringComparison.OrdinalIgnoreCase))
                    return 0;
            }

            return BotLoadoutManager.GetDefaultVariantIndex(role);
        }
    }
}
