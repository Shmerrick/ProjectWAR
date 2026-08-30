using Common;
using FrameWork;
using GameData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml;
using WorldServer.Managers;
using WorldServer.Services.World;
using WorldServer.World.Objects;
using WorldItem = WorldServer.World.Objects.Item;

namespace WorldServer.API
{
    public sealed class BotEditorHttpServer
    {
        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        // Icon data — loaded once from icons.xml + itemdata.csv on Start()
        private static readonly string _iconBaseDir = @"C:\Users\Admin\Downloads\myps\interface\default\eatemplate_icons";
        private static readonly Dictionary<uint, ushort> _itemIconIds = new Dictionary<uint, ushort>();
        private static readonly Dictionary<ushort, string> _iconFileNames = new Dictionary<ushort, string>();

        private readonly HttpListener _listener = new HttpListener();
        private readonly string _prefix;
        private Thread _listenerThread;
        private volatile bool _running;

        public BotEditorHttpServer(string address, int port)
        {
            _prefix = $"http://{address}:{port}/";
        }

        public void Start()
        {
            if (_running)
                return;

            LoadIconData();

            _listener.Prefixes.Add(_prefix);
            _listener.Start();
            _running = true;

            _listenerThread = new Thread(ListenLoop)
            {
                IsBackground = true,
                Name = "BotEditorHttpServer"
            };
            _listenerThread.Start();

            Log.Success("BotEditorAPI", "Bot editor API started " + _prefix);
        }

        private static void LoadIconData()
        {
            _itemIconIds.Clear();
            _iconFileNames.Clear();

            // 1. Parse icons.xml: iconId → texture filename (strip "Textures/" prefix, keep basename)
            string iconsXml = Path.Combine(_iconBaseDir, "source", "icons.xml");
            try
            {
                if (File.Exists(iconsXml))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(iconsXml);
                    foreach (XmlNode node in doc.SelectNodes("/Interface/Assets/Icon"))
                    {
                        string idStr = node.Attributes?["id"]?.Value;
                        string tex = node.Attributes?["texture"]?.Value;
                        if (idStr == null || tex == null) continue;
                        if (!ushort.TryParse(idStr, out ushort iconId)) continue;
                        // texture is like "Textures/Itm_ge_something.dds" — keep only filename
                        string fileName = Path.GetFileName(tex);
                        if (!string.IsNullOrEmpty(fileName))
                            _iconFileNames[iconId] = fileName;
                    }
                    Log.Success("BotEditorAPI", $"Loaded {_iconFileNames.Count} icon filename mappings from icons.xml.");
                }
                else
                {
                    Log.Notice("BotEditorAPI", $"icons.xml not found at {iconsXml}");
                }
            }
            catch (Exception e)
            {
                Log.Error("BotEditorAPI", $"Failed to load icons.xml: {e.Message}");
            }

            // 2. Parse itemdata.csv: item entry → iconId (covers vanilla entries ~1-65604)
            string csvPath = Path.Combine(_iconBaseDir, "..", "..", "..", "data", "gamedata", "itemdata.csv");
            try
            {
                csvPath = Path.GetFullPath(csvPath);
                if (File.Exists(csvPath))
                {
                    bool header = true;
                    foreach (string line in File.ReadLines(csvPath))
                    {
                        if (header) { header = false; continue; }
                        string[] parts = line.Split(',');
                        if (parts.Length < 2) continue;
                        if (!uint.TryParse(parts[0], out uint entry)) continue;
                        if (!ushort.TryParse(parts[1], out ushort iconId)) continue;
                        _itemIconIds[entry] = iconId;
                    }
                    Log.Success("BotEditorAPI", $"Loaded {_itemIconIds.Count} item icon IDs from itemdata.csv.");
                }
                else
                {
                    Log.Notice("BotEditorAPI", $"itemdata.csv not found at {csvPath}");
                }
            }
            catch (Exception e)
            {
                Log.Error("BotEditorAPI", $"Failed to load itemdata.csv: {e.Message}");
            }
        }

        public void Stop()
        {
            _running = false;

            try
            {
                _listener.Stop();
                _listener.Close();
            }
            catch (Exception)
            {
            }
        }

        private void ListenLoop()
        {
            while (_running)
            {
                HttpListenerContext context;
                try
                {
                    context = _listener.GetContext();
                }
                catch (HttpListenerException)
                {
                    if (!_running)
                        return;

                    continue;
                }
                catch (ObjectDisposedException)
                {
                    return;
                }

                ThreadPool.QueueUserWorkItem(HandleContext, context);
            }
        }

        private void HandleContext(object state)
        {
            HttpListenerContext context = (HttpListenerContext)state;
            HttpListenerResponse response = context.Response;

            try
            {
                AddCorsHeaders(response);

                if (string.Equals(context.Request.HttpMethod, "OPTIONS", StringComparison.OrdinalIgnoreCase))
                {
                    response.StatusCode = (int)HttpStatusCode.NoContent;
                    return;
                }

                string[] segments = context.Request.Url.AbsolutePath
                    .Trim('/')
                    .Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                if (segments.Length < 3
                    || !segments[0].Equals("api", StringComparison.OrdinalIgnoreCase)
                    || !segments[1].Equals("bot-editor", StringComparison.OrdinalIgnoreCase))
                {
                    WriteError(response, HttpStatusCode.NotFound, "Route not found.");
                    return;
                }

                if (segments[2].Equals("health", StringComparison.OrdinalIgnoreCase))
                {
                    if (!string.Equals(context.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
                    {
                        WriteError(response, HttpStatusCode.MethodNotAllowed, "Only GET is supported for this route.");
                        return;
                    }

                    WriteJson(response, HttpStatusCode.OK, new HealthResponse
                    {
                        Ok = true,
                        BotCount = BotManager.Instance.GetAllBotCharacters().Count,
                        LoadedBotCount = BotManager.Instance.GetLoadedBots().Count
                    });
                    return;
                }

                if (segments[2].Equals("items", StringComparison.OrdinalIgnoreCase))
                {
                    if (!string.Equals(context.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
                    { WriteError(response, HttpStatusCode.MethodNotAllowed, "Only GET is supported."); return; }
                    if (segments.Length < 4 || !uint.TryParse(segments[3], out uint itemEntry))
                    { WriteError(response, HttpStatusCode.BadRequest, "Item entry id is required."); return; }
                    HandleGetItemDetail(response, itemEntry);
                    return;
                }

                if (segments[2].Equals("icons", StringComparison.OrdinalIgnoreCase))
                {
                    if (!string.Equals(context.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
                    { WriteError(response, HttpStatusCode.MethodNotAllowed, "Only GET is supported."); return; }
                    if (segments.Length < 4 || !ushort.TryParse(segments[3], out ushort iconId))
                    { WriteError(response, HttpStatusCode.BadRequest, "Icon id is required."); return; }
                    HandleGetIcon(response, iconId);
                    return;
                }

                if (segments[2].Equals("career-templates", StringComparison.OrdinalIgnoreCase))
                {
                    HandleCareerTemplatesRoute(context.Request, response, segments);
                    return;
                }

                if (!segments[2].Equals("bots", StringComparison.OrdinalIgnoreCase))
                {
                    WriteError(response, HttpStatusCode.NotFound, "Route not found.");
                    return;
                }

                if (segments.Length == 3)
                {
                    if (!string.Equals(context.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
                    {
                        WriteError(response, HttpStatusCode.MethodNotAllowed, "Only GET is supported for this route.");
                        return;
                    }

                    HandleGetBots(response);
                    return;
                }

                if (!uint.TryParse(segments[3], out uint characterId))
                {
                    WriteError(response, HttpStatusCode.BadRequest, "Invalid character id.");
                    return;
                }

                if (!TryResolveBot(characterId, out ResolvedBot bot, out string resolveError))
                {
                    WriteError(response, HttpStatusCode.NotFound, resolveError);
                    return;
                }

                if (segments.Length == 4)
                {
                    if (!string.Equals(context.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
                    {
                        WriteError(response, HttpStatusCode.MethodNotAllowed, "Only GET is supported for this route.");
                        return;
                    }

                    WriteJson(response, HttpStatusCode.OK, BuildBotSheet(bot));
                    return;
                }

                if (segments.Length == 5 && segments[4].Equals("gear", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.Equals(context.Request.HttpMethod, "PUT", StringComparison.OrdinalIgnoreCase))
                    {
                        HandlePutGear(context.Request, response, bot);
                        return;
                    }

                    if (string.Equals(context.Request.HttpMethod, "DELETE", StringComparison.OrdinalIgnoreCase))
                    {
                        HandleDeleteGear(context.Request, response, bot);
                        return;
                    }

                    WriteError(response, HttpStatusCode.MethodNotAllowed, "Only PUT and DELETE are supported for this route.");
                    return;
                }

                if (segments.Length == 5 && segments[4].Equals("items", StringComparison.OrdinalIgnoreCase))
                {
                    if (!string.Equals(context.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
                    {
                        WriteError(response, HttpStatusCode.MethodNotAllowed, "Only GET is supported for this route.");
                        return;
                    }

                    HandleItemSearch(context.Request, response, bot);
                    return;
                }

                if (segments.Length == 5 && segments[4].Equals("profile", StringComparison.OrdinalIgnoreCase))
                {
                    if (!string.Equals(context.Request.HttpMethod, "PUT", StringComparison.OrdinalIgnoreCase))
                    {
                        WriteError(response, HttpStatusCode.MethodNotAllowed, "Only PUT is supported for this route.");
                        return;
                    }

                    HandlePutProfile(context.Request, response, bot);
                    return;
                }

                if (segments.Length == 5 && segments[4].Equals("template", StringComparison.OrdinalIgnoreCase))
                {
                    if (!string.Equals(context.Request.HttpMethod, "PATCH", StringComparison.OrdinalIgnoreCase))
                    {
                        WriteError(response, HttpStatusCode.MethodNotAllowed, "Only PATCH is supported for this route.");
                        return;
                    }

                    HandlePatchTemplate(context.Request, response, bot);
                    return;
                }

                WriteError(response, HttpStatusCode.NotFound, "Route not found.");
            }
            catch (Exception e)
            {
                Log.Error("BotEditorAPI", e.ToString());
                WriteError(response, HttpStatusCode.InternalServerError, "Unhandled bot editor API error.", e.Message);
            }
            finally
            {
                try
                {
                    response.OutputStream.Close();
                }
                catch (Exception)
                {
                }
            }
        }

        private static void HandleGetBots(HttpListenerResponse response)
        {
            List<BotSummaryResponse> bots = BotManager.Instance.GetAllBotCharacters()
                .Select(BuildBotSummary)
                .OrderBy(bot => bot.Name)
                .ToList();

            WriteJson(response, HttpStatusCode.OK, bots);
        }

        private static void HandlePutGear(HttpListenerRequest request, HttpListenerResponse response, ResolvedBot bot)
        {
            UpdateBotGearRequest updateRequest = ReadRequestBody<UpdateBotGearRequest>(request);
            if (updateRequest?.Slots == null)
            {
                WriteError(response, HttpStatusCode.BadRequest, "Request body must contain a slots array.");
                return;
            }

            Dictionary<ushort, uint> overrideEntries = updateRequest.ReplaceOverrides
                ? new Dictionary<ushort, uint>()
                : BotGearOverrideService.GetOverrideEntries(bot.Character.CharacterId);

            foreach (GearSlotUpdate slot in updateRequest.Slots)
            {
                if (slot == null)
                    continue;

                if (!BotLoadoutManager.IsManagedEquipmentSlot(slot.SlotId))
                {
                    WriteError(response, HttpStatusCode.BadRequest, $"Slot {slot.SlotId} is not a managed bot equipment slot.");
                    return;
                }

                if (!slot.ItemEntry.HasValue || slot.ItemEntry.Value == 0)
                    overrideEntries.Remove(slot.SlotId);
                else
                    overrideEntries[slot.SlotId] = slot.ItemEntry.Value;
            }

            if (!TryValidateOverrideEntries(bot, overrideEntries, out string validationError))
            {
                WriteError(response, HttpStatusCode.BadRequest, validationError);
                return;
            }

            BotGearOverrideService.ReplaceOverrides(bot.Character.CharacterId, overrideEntries);
            if (updateRequest.Reapply && bot.LoadedPlayer != null)
                BotManager.Instance.TryReapplyBotLoadout(bot.Character.CharacterId);

            WriteJson(response, HttpStatusCode.OK, BuildBotSheet(RefreshResolvedBot(bot)));
        }

        private static void HandleDeleteGear(HttpListenerRequest request, HttpListenerResponse response, ResolvedBot bot)
        {
            bool reapply = !string.Equals(request.QueryString["reapply"], "false", StringComparison.OrdinalIgnoreCase);

            BotGearOverrideService.RemoveOverrides(bot.Character.CharacterId);
            if (reapply && bot.LoadedPlayer != null)
                BotManager.Instance.TryReapplyBotLoadout(bot.Character.CharacterId);

            WriteJson(response, HttpStatusCode.OK, BuildBotSheet(RefreshResolvedBot(bot)));
        }

        private static void HandlePutProfile(HttpListenerRequest request, HttpListenerResponse response, ResolvedBot bot)
        {
            UpdateBotProfileRequest updateRequest = ReadRequestBody<UpdateBotProfileRequest>(request);
            if (updateRequest == null)
            {
                WriteError(response, HttpStatusCode.BadRequest, "Request body is required.");
                return;
            }

            if (updateRequest.UseDefault || !updateRequest.VariantIndex.HasValue)
                BotTemplateProfileService.RemoveVariantIndex(bot.Character.CharacterId);
            else
                BotTemplateProfileService.SetVariantIndex(bot.Character.CharacterId, updateRequest.VariantIndex.Value);

            if (updateRequest.Reapply && bot.LoadedPlayer != null)
                BotManager.Instance.TryReapplyBotLoadout(bot.Character.CharacterId);

            WriteJson(response, HttpStatusCode.OK, BuildBotSheet(RefreshResolvedBot(bot)));
        }

        private static void HandleItemSearch(HttpListenerRequest request, HttpListenerResponse response, ResolvedBot bot)
        {
            string query = request.QueryString["q"]?.Trim();
            if (string.IsNullOrWhiteSpace(query))
            {
                WriteError(response, HttpStatusCode.BadRequest, "Query parameter 'q' is required.");
                return;
            }

            if (!ushort.TryParse(request.QueryString["slotId"], out ushort slotId))
            {
                WriteError(response, HttpStatusCode.BadRequest, "Query parameter 'slotId' is required.");
                return;
            }

            if (!BotLoadoutManager.IsManagedEquipmentSlot(slotId))
            {
                WriteError(response, HttpStatusCode.BadRequest, $"Slot {slotId} is not a managed bot equipment slot.");
                return;
            }

            int limit = 50;
            if (int.TryParse(request.QueryString["limit"], out int parsedLimit))
                limit = Math.Max(1, Math.Min(200, parsedLimit));

            HashSet<uint> uniqueEquipped = BuildUniqueSetExcludingSlot(bot, slotId);
            bool exactEntry = uint.TryParse(query, out uint entryQuery);

            List<ItemSearchResultResponse> results = ItemService._Item_Info.Values
                .Where(item => MatchesItemQuery(item, query, exactEntry, entryQuery))
                .Where(item => BotLoadoutManager.CanUseItemInLoadout(bot.Character.CareerLine, bot.Character.Race, bot.Value.Skills, bot.Value.Level, bot.Value.RenownRank, slotId, item, uniqueEquipped))
                .OrderBy(item => exactEntry && item.Entry == entryQuery ? 0 : 1)
                .ThenBy(item => item.Name.StartsWith(query, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
                .ThenByDescending(item => item.ObjectLevel)
                .ThenBy(item => item.Name)
                .Take(limit)
                .Select(BuildItemSearchResult)
                .ToList();

            WriteJson(response, HttpStatusCode.OK, results);
        }

        private static ResolvedBot RefreshResolvedBot(ResolvedBot bot)
        {
            TryResolveBot(bot.Character.CharacterId, out ResolvedBot refreshedBot, out _);
            return refreshedBot ?? bot;
        }

        private static bool TryResolveBotRole(Character character, Player loadedPlayer, out BotRole role)
        {
            if (loadedPlayer != null)
            {
                role = loadedPlayer.Role;
                return true;
            }

            return BotManager.TryGetRoleFromBotName(character?.Name, out role);
        }

        private static string GetTemplateVariantLabel(byte variantIndex)
        {
            if (variantIndex < 26)
                return $"Template {(char)('A' + variantIndex)}";

            return $"Template {variantIndex + 1}";
        }

        private static bool TryResolveBot(uint characterId, out ResolvedBot bot, out string error)
        {
            bot = null;
            error = "Bot not found.";

            Character character = CharMgr.GetCharacter(characterId, false);
            if (character == null)
                return false;

            if (!character.Name.StartsWith("Bot_", StringComparison.OrdinalIgnoreCase))
            {
                error = "Character is not a bot.";
                return false;
            }

            if (character.Value == null)
                character.Value = CharMgr.Database.SelectObject<Character_value>($"CharacterId={character.CharacterId}");

            if (character.Value == null)
            {
                error = "Bot character value record is missing.";
                return false;
            }

            Player loadedPlayer = BotManager.Instance.GetLoadedBot(characterId);
            if (!TryResolveBotRole(character, loadedPlayer, out BotRole role))
            {
                error = "Unable to determine bot role from character name.";
                return false;
            }

            bot = new ResolvedBot
            {
                Character = character,
                Value = character.Value,
                LoadedPlayer = loadedPlayer,
                Role = role,
                Tier = ResolveBotTier(character.Value.Level, character.Value.RenownRank)
            };

            return true;
        }

        private static BotSummaryResponse BuildBotSummary(Character character)
        {
            Character_value value = character.Value;
            if (value == null)
                value = CharMgr.Database.SelectObject<Character_value>($"CharacterId={character.CharacterId}");

            Player loadedPlayer = BotManager.Instance.GetLoadedBot(character.CharacterId);
            if (!TryResolveBotRole(character, loadedPlayer, out BotRole role))
                role = BotRole.MeleeDPS;

            bool hasExplicitProfileAssignment = BotTemplateProfileService.TryGetVariantIndex(character.CharacterId, out byte explicitProfileVariantIndex);
            byte profileVariantIndex = hasExplicitProfileAssignment
                ? explicitProfileVariantIndex
                : BotTemplateProfileService.ResolveVariantIndex(character.CharacterId, role);

            return new BotSummaryResponse
            {
                CharacterId = character.CharacterId,
                Name = character.Name,
                GroupPrefix = BotManager.GetGroupPrefixFromBotName(character.Name),
                Loaded = loadedPlayer != null,
                Role = FormatEnumName(role.ToString()),
                CareerLine = character.CareerLine,
                CareerName = FormatEnumName(((CareerLine)character.CareerLine).ToString()),
                Realm = character.Realm,
                RealmName = FormatEnumName(((Realms)character.Realm).ToString()),
                Level = value?.Level ?? 0,
                RenownRank = value?.RenownRank ?? 0,
                ZoneId = loadedPlayer?.Zone?.ZoneId ?? value?.ZoneId ?? 0,
                HasGearOverrides = BotGearOverrideService.HasOverrides(character.CharacterId),
                ProfileVariantIndex = profileVariantIndex,
                ProfileLabel = GetTemplateVariantLabel(profileVariantIndex),
                HasExplicitProfileAssignment = hasExplicitProfileAssignment
            };
        }

        private static BotSheetResponse BuildBotSheet(ResolvedBot bot)
        {
            bool hasExplicitProfileAssignment = BotTemplateProfileService.TryGetVariantIndex(bot.Character.CharacterId, out byte explicitProfileVariantIndex);
            byte profileVariantIndex = hasExplicitProfileAssignment
                ? explicitProfileVariantIndex
                : BotTemplateProfileService.ResolveVariantIndex(bot.Character.CharacterId, bot.Role);

            Dictionary<ushort, uint> templateLoadout = BotLoadoutManager.GetBaseLoadout(bot.Character.CharacterId, bot.Character.CareerLine, bot.Tier, bot.Role)?.SlotItems
                .ToDictionary(entry => entry.Key, entry => entry.Value)
                ?? new Dictionary<ushort, uint>();

            Dictionary<ushort, uint> overrideEntries = BotGearOverrideService.GetOverrideEntries(bot.Character.CharacterId);
            Dictionary<ushort, uint> effectiveLoadout = BotLoadoutManager.GetLoadout(
                bot.Character.CharacterId,
                bot.Character.CareerLine,
                bot.Character.Race,
                bot.Value.Skills,
                bot.Value.Level,
                bot.Value.RenownRank,
                bot.Tier,
                bot.Role)?.SlotItems.ToDictionary(entry => entry.Key, entry => entry.Value)
                ?? new Dictionary<ushort, uint>();

            return new BotSheetResponse
            {
                CharacterId = bot.Character.CharacterId,
                Name = bot.Character.Name,
                GroupPrefix = BotManager.GetGroupPrefixFromBotName(bot.Character.Name),
                Loaded = bot.LoadedPlayer != null,
                Role = FormatEnumName(bot.Role.ToString()),
                CareerLine = bot.Character.CareerLine,
                CareerName = FormatEnumName(((CareerLine)bot.Character.CareerLine).ToString()),
                Realm = bot.Character.Realm,
                RealmName = FormatEnumName(((Realms)bot.Character.Realm).ToString()),
                Level = bot.Value.Level,
                RenownRank = bot.Value.RenownRank,
                ZoneId = bot.LoadedPlayer?.Zone?.ZoneId ?? bot.Value.ZoneId,
                ProfileVariantIndex = profileVariantIndex,
                ProfileLabel = GetTemplateVariantLabel(profileVariantIndex),
                HasExplicitProfileAssignment = hasExplicitProfileAssignment,
                TemplateGear = BuildGearSlots(templateLoadout),
                CustomGearOverrides = BuildGearSlots(overrideEntries),
                EffectiveLoadout = BuildGearSlots(effectiveLoadout),
                EquippedGear = BuildEquippedGear(bot)
            };
        }

        private static List<GearSlotResponse> BuildEquippedGear(ResolvedBot bot)
        {
            List<GearSlotResponse> gear = new List<GearSlotResponse>();

            if (bot.LoadedPlayer?.ItmInterface != null && bot.LoadedPlayer.ItmInterface.IsLoad)
            {
                foreach (ushort slotId in BotLoadoutManager.ManagedEquipmentSlots.OrderBy(slot => slot))
                {
                    WorldItem item = bot.LoadedPlayer.ItmInterface.GetItemInSlot(slotId);
                    if (item?.Info == null)
                        continue;

                    gear.Add(new GearSlotResponse
                    {
                        SlotId = slotId,
                        SlotName = GetSlotName(slotId),
                        ItemEntry = item.Info.Entry,
                        ItemName = item.Info.Name,
                        ModelId = item.Info.ModelId,
                        ObjectLevel = item.Info.ObjectLevel,
                        MinRank = item.Info.MinRank,
                        MinRenown = item.Info.MinRenown,
                        Rarity = item.Info.Rarity,
                        PrimaryDye = item.GetPrimaryDye(),
                        SecondaryDye = item.GetSecondaryDye()
                    });
                }

                return gear;
            }

            IList<CharacterItem> persistedItems = CharMgr.Database.SelectObjects<CharacterItem>($"CharacterId={bot.Character.CharacterId}") ?? new List<CharacterItem>();
            foreach (CharacterItem item in persistedItems
                .Where(item => item != null && BotLoadoutManager.IsManagedEquipmentSlot(item.SlotId))
                .OrderBy(item => item.SlotId))
            {
                Item_Info info = ItemService.GetItem_Info(item.Entry);
                if (info == null)
                    continue;

                gear.Add(new GearSlotResponse
                {
                    SlotId = item.SlotId,
                    SlotName = GetSlotName(item.SlotId),
                    ItemEntry = item.Entry,
                    ItemName = info.Name,
                    ModelId = info.ModelId,
                    ObjectLevel = info.ObjectLevel,
                    MinRank = info.MinRank,
                    MinRenown = info.MinRenown,
                    Rarity = info.Rarity,
                    PrimaryDye = item.PrimaryDye,
                    SecondaryDye = item.SecondaryDye
                });
            }

            return gear;
        }

        private static List<GearSlotResponse> BuildGearSlots(IDictionary<ushort, uint> entries)
        {
            List<GearSlotResponse> gear = new List<GearSlotResponse>();
            foreach (KeyValuePair<ushort, uint> entry in entries.OrderBy(item => item.Key))
            {
                Item_Info info = ItemService.GetItem_Info(entry.Value);
                if (info == null)
                    continue;

                _itemIconIds.TryGetValue(info.Entry, out ushort iconId);

                gear.Add(new GearSlotResponse
                {
                    SlotId = entry.Key,
                    SlotName = GetSlotName(entry.Key),
                    ItemEntry = info.Entry,
                    ItemName = info.Name,
                    ModelId = info.ModelId,
                    ObjectLevel = info.ObjectLevel,
                    MinRank = info.MinRank,
                    MinRenown = info.MinRenown,
                    Rarity = info.Rarity,
                    IconId = iconId
                });
            }

            return gear;
        }

        private static bool TryValidateOverrideEntries(ResolvedBot bot, IDictionary<ushort, uint> overrideEntries, out string error)
        {
            error = null;

            LoadoutValidationContext validationContext = new LoadoutValidationContext(
                bot.Character.CareerLine,
                bot.Character.Race,
                bot.Value.Skills,
                bot.Value.Level,
                bot.Value.RenownRank,
                BotLoadoutManager.GetBaseLoadout(bot.Character.CharacterId, bot.Character.CareerLine, bot.Tier, bot.Role));

            foreach (KeyValuePair<ushort, uint> overrideEntry in overrideEntries.OrderBy(entry => entry.Key))
            {
                if (!validationContext.TryApplyOverride(overrideEntry.Key, overrideEntry.Value, out error))
                    return false;
            }

            return true;
        }

        private static HashSet<uint> BuildUniqueSetExcludingSlot(ResolvedBot bot, ushort excludedSlotId)
        {
            HashSet<uint> uniqueEntries = new HashSet<uint>();
            Dictionary<ushort, uint> effectiveLoadout = BotLoadoutManager.GetLoadout(
                bot.Character.CharacterId,
                bot.Character.CareerLine,
                bot.Character.Race,
                bot.Value.Skills,
                bot.Value.Level,
                bot.Value.RenownRank,
                bot.Tier,
                bot.Role)?.SlotItems
                .ToDictionary(entry => entry.Key, entry => entry.Value)
                ?? new Dictionary<ushort, uint>();

            foreach (KeyValuePair<ushort, uint> entry in effectiveLoadout)
            {
                if (entry.Key == excludedSlotId)
                    continue;

                Item_Info info = ItemService.GetItem_Info(entry.Value);
                if (info?.UniqueEquiped == 1)
                    uniqueEntries.Add(entry.Value);
            }

            return uniqueEntries;
        }

        private static void HandleGetItemDetail(HttpListenerResponse response, uint itemEntry)
        {
            Item_Info info = ItemService.GetItem_Info(itemEntry);
            if (info == null)
            {
                WriteError(response, HttpStatusCode.NotFound, $"Item {itemEntry} not found.");
                return;
            }

            _itemIconIds.TryGetValue(info.Entry, out ushort iconId);

            Dictionary<string, int> stats = new Dictionary<string, int>();
            foreach (KeyValuePair<byte, ushort> stat in info._Stats)
            {
                string statName = Enum.IsDefined(typeof(Stats), (int)stat.Key)
                    ? FormatEnumName(((Stats)stat.Key).ToString())
                    : $"Stat{stat.Key}";
                stats[statName] = stat.Value;
            }

            WriteJson(response, HttpStatusCode.OK, new ItemDetailResponse
            {
                Entry = info.Entry,
                Name = info.Name,
                Description = info.Description,
                Type = info.Type,
                SlotId = info.SlotId,
                SlotName = GetSlotName(info.SlotId),
                Armor = info.Armor,
                Dps = info.Dps,
                Speed = info.Speed,
                MinRank = info.MinRank,
                MinRenown = info.MinRenown,
                ObjectLevel = info.ObjectLevel,
                Rarity = info.Rarity,
                Career = info.Career,
                TwoHanded = info.TwoHanded,
                Bind = info.Bind,
                TalismanSlots = info.TalismanSlots,
                UniqueEquiped = info.UniqueEquiped,
                ItemSet = info.ItemSet,
                ModelId = info.ModelId,
                IconId = iconId,
                Stats = stats,
                Effects = info.EffectsList
            });
        }

        private static void HandleGetIcon(HttpListenerResponse response, ushort iconId)
        {
            string texturesDir = Path.Combine(_iconBaseDir, "textures");
            if (!Directory.Exists(texturesDir))
            {
                WriteError(response, HttpStatusCode.ServiceUnavailable, "Icon texture directory not found.");
                return;
            }

            if (!_iconFileNames.TryGetValue(iconId, out string fileName))
            {
                response.StatusCode = (int)HttpStatusCode.NotFound;
                return;
            }

            string ddsPath = Path.Combine(texturesDir, fileName);
            if (!File.Exists(ddsPath))
            {
                // Try case-insensitive match (Windows FS is case-insensitive but let's be safe)
                string[] matches = Directory.GetFiles(texturesDir, fileName, SearchOption.TopDirectoryOnly);
                if (matches.Length == 0) { response.StatusCode = (int)HttpStatusCode.NotFound; return; }
                ddsPath = matches[0];
            }

            byte[] data = File.ReadAllBytes(ddsPath);
            response.StatusCode = (int)HttpStatusCode.OK;
            response.ContentType = "image/vnd.ms-dds";
            response.ContentLength64 = data.LongLength;
            response.OutputStream.Write(data, 0, data.Length);
        }

        private static void HandlePatchTemplate(HttpListenerRequest request, HttpListenerResponse response, ResolvedBot bot)
        {
            GearUpdateRequest body = ReadRequestBody<GearUpdateRequest>(request);
            if (body?.Slots == null || body.Slots.Count == 0)
            {
                WriteError(response, HttpStatusCode.BadRequest, "Request body must contain at least one slot update.");
                return;
            }

            Dictionary<ushort, uint> updates = new Dictionary<ushort, uint>();
            foreach (GearSlotUpdate slot in body.Slots)
            {
                if (slot.ItemEntry.HasValue)
                    updates[slot.SlotId] = slot.ItemEntry.Value;
                else
                    updates[slot.SlotId] = 0;
            }

            BotLoadoutManager.PatchTemplate(bot.Character.CareerLine, bot.Tier, bot.Role, updates);

            if (body.Reapply && bot.LoadedPlayer != null)
                BotManager.Instance.TryReapplyBotLoadout(bot.Character.CharacterId);

            WriteJson(response, HttpStatusCode.OK, BuildBotSheet(RefreshResolvedBot(bot)));
        }

        private static ItemSearchResultResponse BuildItemSearchResult(Item_Info item)
        {
            _itemIconIds.TryGetValue(item.Entry, out ushort iconId);
            return new ItemSearchResultResponse
            {
                ItemEntry = item.Entry,
                ItemName = item.Name,
                ModelId = item.ModelId,
                SlotId = item.SlotId,
                SlotName = GetSlotName(item.SlotId),
                ObjectLevel = item.ObjectLevel,
                MinRank = item.MinRank,
                MinRenown = item.MinRenown,
                Rarity = item.Rarity,
                IconId = iconId
            };
        }

        // ---- Career template routes ----

        private static void HandleCareerTemplatesRoute(HttpListenerRequest request, HttpListenerResponse response, string[] segments)
        {
            // GET /api/bot-editor/career-templates
            if (segments.Length == 3)
            {
                if (!string.Equals(request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
                { WriteError(response, HttpStatusCode.MethodNotAllowed, "Only GET is supported."); return; }
                HandleGetCareerTemplates(response);
                return;
            }

            // Remaining routes: /career-templates/{careerLine}/{tier}/{variantIndex}[/items]
            if (segments.Length < 6
                || !byte.TryParse(segments[3], out byte careerLine)
                || !TryParseTier(segments[4], out BotLoadoutManager.BotTier tier)
                || !byte.TryParse(segments[5], out byte variantIndex))
            {
                WriteError(response, HttpStatusCode.BadRequest,
                    "Expected /career-templates/{careerLine}/{tier}/{variantIndex}[/items]  (tier = T1|T2|T3|T4|SC)");
                return;
            }

            // GET /…/{careerLine}/{tier}/{variantIndex}/items
            if (segments.Length == 7 && segments[6].Equals("items", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.Equals(request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
                { WriteError(response, HttpStatusCode.MethodNotAllowed, "Only GET is supported."); return; }
                HandleCareerTemplateItemSearch(request, response, careerLine, tier, variantIndex);
                return;
            }

            // PATCH /…/{careerLine}/{tier}/{variantIndex}
            if (segments.Length == 6 && string.Equals(request.HttpMethod, "PATCH", StringComparison.OrdinalIgnoreCase))
            {
                HandlePatchCareerTemplate(request, response, careerLine, tier, variantIndex);
                return;
            }

            WriteError(response, HttpStatusCode.NotFound, "Route not found.");
        }

        private static void HandleGetCareerTemplates(HttpListenerResponse response)
        {
            IReadOnlyList<(byte CareerLine, BotLoadoutManager.BotTier Tier, byte VariantIndex, BotLoadoutManager.Loadout Loadout)> all
                = BotLoadoutManager.GetAllTemplates();

            // Group: career → tier → variants
            Dictionary<byte, CareerTemplateResponse> byCareer = new Dictionary<byte, CareerTemplateResponse>();

            foreach ((byte careerLine, BotLoadoutManager.BotTier tier, byte variantIndex, BotLoadoutManager.Loadout loadout) in all)
            {
                if (!byCareer.TryGetValue(careerLine, out CareerTemplateResponse ct))
                {
                    byte realm = ResolveCareerRealm(careerLine);
                    ct = new CareerTemplateResponse
                    {
                        CareerLine = careerLine,
                        CareerName = FormatEnumName(((CareerLine)careerLine).ToString()),
                        Realm = realm,
                        RealmName = FormatEnumName(((Realms)realm).ToString()),
                        Tiers = new List<CareerTemplateTierResponse>()
                    };
                    byCareer[careerLine] = ct;
                }

                CareerTemplateTierResponse tierEntry = ct.Tiers.FirstOrDefault(t => t.TierName == TierName(tier));
                if (tierEntry == null)
                {
                    tierEntry = new CareerTemplateTierResponse
                    {
                        TierName = TierName(tier),
                        Variants = new List<CareerTemplateVariantResponse>()
                    };
                    ct.Tiers.Add(tierEntry);
                }

                tierEntry.Variants.Add(new CareerTemplateVariantResponse
                {
                    VariantIndex = variantIndex,
                    Label = GetTemplateVariantLabel(variantIndex),
                    Gear = BuildGearSlots(loadout.SlotItems)
                });
            }

            List<CareerTemplateResponse> result = byCareer.Values
                .OrderBy(ct => ct.Realm)
                .ThenBy(ct => ct.CareerLine)
                .ToList();

            WriteJson(response, HttpStatusCode.OK, result);
        }

        private static void HandleCareerTemplateItemSearch(HttpListenerRequest request, HttpListenerResponse response,
            byte careerLine, BotLoadoutManager.BotTier tier, byte variantIndex)
        {
            string query = request.QueryString["q"]?.Trim();
            if (string.IsNullOrWhiteSpace(query))
            { WriteError(response, HttpStatusCode.BadRequest, "Query parameter 'q' is required."); return; }

            if (!ushort.TryParse(request.QueryString["slotId"], out ushort slotId))
            { WriteError(response, HttpStatusCode.BadRequest, "Query parameter 'slotId' is required."); return; }

            if (!BotLoadoutManager.IsManagedEquipmentSlot(slotId))
            { WriteError(response, HttpStatusCode.BadRequest, $"Slot {slotId} is not a managed bot equipment slot."); return; }

            int limit = 50;
            if (int.TryParse(request.QueryString["limit"], out int parsedLimit))
                limit = Math.Max(1, Math.Min(200, parsedLimit));

            // Use a representative character of this career for race/skills filtering.
            byte race = 0;
            long skills = long.MaxValue;
            byte level = tier == BotLoadoutManager.BotTier.T1 ? (byte)11
                       : tier == BotLoadoutManager.BotTier.T2 ? (byte)21
                       : tier == BotLoadoutManager.BotTier.T3 ? (byte)31 : (byte)40;
            byte renownRank = tier == BotLoadoutManager.BotTier.T4 || tier == BotLoadoutManager.BotTier.SC ? (byte)80 : (byte)0;

            Character repChar = BotManager.Instance.GetAllBotCharacters()
                .FirstOrDefault(c => c.CareerLine == careerLine);
            if (repChar?.Value != null)
            {
                race = repChar.Race;
                skills = repChar.Value.Skills;
            }

            BotLoadoutManager.Loadout template = BotLoadoutManager.GetBaseLoadout(careerLine, tier, variantIndex);
            HashSet<uint> uniqueEquipped = new HashSet<uint>();
            if (template != null)
            {
                foreach (KeyValuePair<ushort, uint> kv in template.SlotItems)
                {
                    if (kv.Key == slotId) continue;
                    Item_Info info = ItemService.GetItem_Info(kv.Value);
                    if (info?.UniqueEquiped == 1)
                        uniqueEquipped.Add(info.Entry);
                }
            }

            bool exactEntry = uint.TryParse(query, out uint entryQuery);

            List<ItemSearchResultResponse> results = ItemService._Item_Info.Values
                .Where(item => MatchesItemQuery(item, query, exactEntry, entryQuery))
                .Where(item => BotLoadoutManager.CanUseItemInLoadout(careerLine, race, skills, level, renownRank, slotId, item, uniqueEquipped))
                .OrderBy(item => exactEntry && item.Entry == entryQuery ? 0 : 1)
                .ThenBy(item => item.Name.StartsWith(query, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
                .ThenByDescending(item => item.ObjectLevel)
                .ThenBy(item => item.Name)
                .Take(limit)
                .Select(BuildItemSearchResult)
                .ToList();

            WriteJson(response, HttpStatusCode.OK, results);
        }

        private static void HandlePatchCareerTemplate(HttpListenerRequest request, HttpListenerResponse response,
            byte careerLine, BotLoadoutManager.BotTier tier, byte variantIndex)
        {
            GearUpdateRequest body = ReadRequestBody<GearUpdateRequest>(request);
            if (body?.Slots == null || body.Slots.Count == 0)
            { WriteError(response, HttpStatusCode.BadRequest, "Request body must contain at least one slot update."); return; }

            Dictionary<ushort, uint> updates = new Dictionary<ushort, uint>();
            foreach (GearSlotUpdate slot in body.Slots)
                updates[slot.SlotId] = slot.ItemEntry ?? 0;

            BotLoadoutManager.PatchTemplate(careerLine, tier, variantIndex, updates);

            if (body.Reapply && (tier == BotLoadoutManager.BotTier.T4 || tier == BotLoadoutManager.BotTier.SC))
            {
                foreach (Character botChar in BotManager.Instance.GetAllBotCharacters()
                    .Where(c => c.CareerLine == careerLine))
                {
                    Player loadedBot = BotManager.Instance.GetLoadedBot(botChar.CharacterId);
                    if (loadedBot == null || !TryResolveBotRole(botChar, loadedBot, out BotRole botRole))
                        continue;

                    if (BotLoadoutManager.GetResolvedVariantIndex(botChar.CharacterId, botRole) != variantIndex)
                        continue;

                    BotManager.Instance.TryReapplyBotLoadout(botChar.CharacterId);
                }
            }

            BotLoadoutManager.Loadout updated = BotLoadoutManager.GetTemplate(careerLine, tier, variantIndex);
            CareerTemplateVariantResponse result = new CareerTemplateVariantResponse
            {
                VariantIndex = variantIndex,
                Label = GetTemplateVariantLabel(variantIndex),
                Gear = BuildGearSlots(updated?.SlotItems ?? new Dictionary<ushort, uint>())
            };

            WriteJson(response, HttpStatusCode.OK, result);
        }

        private static byte ResolveCareerRealm(byte careerLine)
        {
            // Try to infer from a live bot character.
            Character c = BotManager.Instance.GetAllBotCharacters()
                .FirstOrDefault(ch => ch.CareerLine == careerLine);
            if (c != null)
                return c.Realm;

            // Fallback: Destruction careers are Choppa (25), Black Orc (23), Squig Herder (22), Shaman (21).
            // Order careers are Slayer (7), Iron Breaker (5), Engineer (4), Rune Priest (3).
            // Use CareerLine enum to determine realm via known destruction careers.
            CareerLine cl = (CareerLine)careerLine;
            switch (cl)
            {
                case CareerLine.CAREERLINE_SHAMAN:
                case CareerLine.CAREERLINE_SQUIG_HERDER:
                case CareerLine.CAREERLINE_BLACK_ORC:
                case CareerLine.CAREERLINE_CHOPPA:
                    return (byte)Realms.REALMS_REALM_DESTRUCTION;
                default:
                    return (byte)Realms.REALMS_REALM_ORDER;
            }
        }

        private static bool MatchesItemQuery(Item_Info item, string query, bool exactEntry, uint entryQuery)
        {
            if (item == null)
                return false;

            if (exactEntry && item.Entry == entryQuery)
                return true;

            return item.Name?.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static BotLoadoutManager.BotTier ResolveBotTier(byte level, byte renownRank)
        {
            if (level <= 11) return BotLoadoutManager.BotTier.T1;
            if (level <= 21) return BotLoadoutManager.BotTier.T2;
            if (level <= 31) return BotLoadoutManager.BotTier.T3;
            return BotLoadoutManager.BotTier.T4;
        }

        private static bool TryParseTier(string s, out BotLoadoutManager.BotTier tier)
        {
            switch (s?.ToUpperInvariant())
            {
                case "T1": tier = BotLoadoutManager.BotTier.T1; return true;
                case "T2": tier = BotLoadoutManager.BotTier.T2; return true;
                case "T3": tier = BotLoadoutManager.BotTier.T3; return true;
                case "T4": tier = BotLoadoutManager.BotTier.T4; return true;
                case "SC": tier = BotLoadoutManager.BotTier.SC; return true;
                default: tier = default; return false;
            }
        }

        private static string TierName(BotLoadoutManager.BotTier tier) => tier.ToString();

        private static string GetSlotName(ushort slotId)
        {
            return FormatEnumName(((EquipSlot)slotId).ToString());
        }

        private static string FormatEnumName(string value)
        {
            return value.Replace('_', ' ');
        }

        private static T ReadRequestBody<T>(HttpListenerRequest request)
        {
            using (StreamReader reader = new StreamReader(request.InputStream, request.ContentEncoding ?? Encoding.UTF8))
            {
                string body = reader.ReadToEnd();
                if (string.IsNullOrWhiteSpace(body))
                    return default(T);

                return JsonConvert.DeserializeObject<T>(body);
            }
        }

        private static void AddCorsHeaders(HttpListenerResponse response)
        {
            response.Headers["Access-Control-Allow-Origin"] = "*";
            response.Headers["Access-Control-Allow-Methods"] = "GET,PUT,PATCH,DELETE,OPTIONS";
            response.Headers["Access-Control-Allow-Headers"] = "Content-Type";
        }

        private static void WriteError(HttpListenerResponse response, HttpStatusCode statusCode, string error, string detail = null)
        {
            WriteJson(response, statusCode, new ErrorResponse
            {
                Error = error,
                Detail = detail
            });
        }

        private static void WriteJson(HttpListenerResponse response, HttpStatusCode statusCode, object payload)
        {
            string json = JsonConvert.SerializeObject(payload, JsonSettings);
            byte[] buffer = Encoding.UTF8.GetBytes(json);

            response.StatusCode = (int)statusCode;
            response.ContentType = "application/json";
            response.ContentEncoding = Encoding.UTF8;
            response.ContentLength64 = buffer.LongLength;
            response.OutputStream.Write(buffer, 0, buffer.Length);
        }

        private sealed class ResolvedBot
        {
            public Character Character { get; set; }
            public Character_value Value { get; set; }
            public Player LoadedPlayer { get; set; }
            public BotRole Role { get; set; }
            public BotLoadoutManager.BotTier Tier { get; set; }
        }

        private sealed class LoadoutValidationContext
        {
            private readonly byte _careerLine;
            private readonly byte _race;
            private readonly long _skills;
            private readonly byte _level;
            private readonly byte _renownRank;
            private readonly Dictionary<ushort, uint> _effectiveEntries;
            private readonly HashSet<uint> _uniqueEntries;

            public LoadoutValidationContext(byte careerLine, byte race, long skills, byte level, byte renownRank, BotLoadoutManager.Loadout baseLoadout)
            {
                _careerLine = careerLine;
                _race = race;
                _skills = skills;
                _level = level;
                _renownRank = renownRank;
                _effectiveEntries = baseLoadout?.SlotItems.ToDictionary(entry => entry.Key, entry => entry.Value) ?? new Dictionary<ushort, uint>();
                _uniqueEntries = new HashSet<uint>(_effectiveEntries.Values
                    .Select(ItemService.GetItem_Info)
                    .Where(info => info?.UniqueEquiped == 1)
                    .Select(info => info.Entry));
            }

            public bool TryApplyOverride(ushort slotId, uint itemEntry, out string error)
            {
                error = null;

                if (!BotLoadoutManager.IsManagedEquipmentSlot(slotId))
                {
                    error = $"Slot {slotId} is not a managed bot equipment slot.";
                    return false;
                }

                Item_Info previousInfo = null;
                if (_effectiveEntries.TryGetValue(slotId, out uint previousEntry))
                {
                    previousInfo = ItemService.GetItem_Info(previousEntry);
                    if (previousInfo?.UniqueEquiped == 1)
                        _uniqueEntries.Remove(previousEntry);
                }

                Item_Info itemInfo = ItemService.GetItem_Info(itemEntry);
                if (!BotLoadoutManager.CanUseItemInLoadout(_careerLine, _race, _skills, _level, _renownRank, slotId, itemInfo, _uniqueEntries))
                {
                    if (previousInfo?.UniqueEquiped == 1)
                        _uniqueEntries.Add(previousInfo.Entry);

                    error = $"Item {itemEntry} is not valid for slot {slotId} on this bot.";
                    return false;
                }

                _effectiveEntries[slotId] = itemEntry;
                if (itemInfo.UniqueEquiped == 1)
                    _uniqueEntries.Add(itemInfo.Entry);

                return true;
            }
        }

        private sealed class ErrorResponse
        {
            public string Error { get; set; }
            public string Detail { get; set; }
        }

        private sealed class HealthResponse
        {
            public bool Ok { get; set; }
            public int BotCount { get; set; }
            public int LoadedBotCount { get; set; }
        }

        private sealed class BotSummaryResponse
        {
            public uint CharacterId { get; set; }
            public string Name { get; set; }
            public string GroupPrefix { get; set; }
            public bool Loaded { get; set; }
            public string Role { get; set; }
            public byte CareerLine { get; set; }
            public string CareerName { get; set; }
            public byte Realm { get; set; }
            public string RealmName { get; set; }
            public byte Level { get; set; }
            public byte RenownRank { get; set; }
            public ushort ZoneId { get; set; }
            public byte ProfileVariantIndex { get; set; }
            public string ProfileLabel { get; set; }
            public bool HasExplicitProfileAssignment { get; set; }
            public bool HasGearOverrides { get; set; }
        }

        private sealed class BotSheetResponse
        {
            public uint CharacterId { get; set; }
            public string Name { get; set; }
            public string GroupPrefix { get; set; }
            public bool Loaded { get; set; }
            public string Role { get; set; }
            public byte CareerLine { get; set; }
            public string CareerName { get; set; }
            public byte Realm { get; set; }
            public string RealmName { get; set; }
            public byte Level { get; set; }
            public byte RenownRank { get; set; }
            public ushort ZoneId { get; set; }
            public byte ProfileVariantIndex { get; set; }
            public string ProfileLabel { get; set; }
            public bool HasExplicitProfileAssignment { get; set; }
            public List<GearSlotResponse> TemplateGear { get; set; }
            public List<GearSlotResponse> CustomGearOverrides { get; set; }
            public List<GearSlotResponse> EffectiveLoadout { get; set; }
            public List<GearSlotResponse> EquippedGear { get; set; }
        }

        private sealed class GearSlotResponse
        {
            public ushort SlotId { get; set; }
            public string SlotName { get; set; }
            public uint ItemEntry { get; set; }
            public string ItemName { get; set; }
            public uint ModelId { get; set; }
            public byte ObjectLevel { get; set; }
            public byte MinRank { get; set; }
            public byte MinRenown { get; set; }
            public byte Rarity { get; set; }
            public ushort IconId { get; set; }
            public ushort PrimaryDye { get; set; }
            public ushort SecondaryDye { get; set; }
        }

        private sealed class UpdateBotGearRequest
        {
            public bool ReplaceOverrides { get; set; } = true;
            public bool Reapply { get; set; } = true;
            public List<GearSlotUpdate> Slots { get; set; }
        }

        private sealed class UpdateBotProfileRequest
        {
            public byte? VariantIndex { get; set; }
            public bool UseDefault { get; set; }
            public bool Reapply { get; set; } = true;
        }

        private sealed class GearUpdateRequest
        {
            public bool ReplaceOverrides { get; set; } = false;
            public bool Reapply { get; set; } = true;
            public List<GearSlotUpdate> Slots { get; set; }
        }

        private sealed class GearSlotUpdate
        {
            public ushort SlotId { get; set; }
            public uint? ItemEntry { get; set; }
        }

        private sealed class ItemSearchResultResponse
        {
            public uint ItemEntry { get; set; }
            public string ItemName { get; set; }
            public uint ModelId { get; set; }
            public ushort SlotId { get; set; }
            public string SlotName { get; set; }
            public byte ObjectLevel { get; set; }
            public byte MinRank { get; set; }
            public byte MinRenown { get; set; }
            public byte Rarity { get; set; }
            public ushort IconId { get; set; }
        }

        private sealed class ItemDetailResponse
        {
            public uint Entry { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public byte Type { get; set; }
            public ushort SlotId { get; set; }
            public string SlotName { get; set; }
            public ushort Armor { get; set; }
            public ushort Dps { get; set; }
            public ushort Speed { get; set; }
            public byte MinRank { get; set; }
            public byte MinRenown { get; set; }
            public byte ObjectLevel { get; set; }
            public byte Rarity { get; set; }
            public uint Career { get; set; }
            public bool TwoHanded { get; set; }
            public byte Bind { get; set; }
            public byte TalismanSlots { get; set; }
            public byte UniqueEquiped { get; set; }
            public uint ItemSet { get; set; }
            public uint ModelId { get; set; }
            public ushort IconId { get; set; }
            public Dictionary<string, int> Stats { get; set; }
            public List<ushort> Effects { get; set; }
        }

        private sealed class CareerTemplateResponse
        {
            public byte CareerLine { get; set; }
            public string CareerName { get; set; }
            public byte Realm { get; set; }
            public string RealmName { get; set; }
            public List<CareerTemplateTierResponse> Tiers { get; set; }
        }

        private sealed class CareerTemplateTierResponse
        {
            public string TierName { get; set; }   // "T1" | "T2" | "T3" | "T4" | "SC"
            public List<CareerTemplateVariantResponse> Variants { get; set; }
        }

        private sealed class CareerTemplateVariantResponse
        {
            public byte VariantIndex { get; set; }
            public string Label { get; set; }
            public List<GearSlotResponse> Gear { get; set; }
        }
    }
}

