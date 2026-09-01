-- SUPERSEDED BY 08_move_creature_wards_to_spawns.sql.
--
-- This historical update restored ward bits by prototype before it was confirmed that the same
-- prototype is reused with different wards in different locations. Script 08 reverses the 79
-- changes made here and moves authoritative ward state to concrete spawn rows. This file remains
-- in sequence so existing installations and fresh imports converge on the same final state.
--
-- F_CREATE_MONSTER offset 35 is creature_protos.Unk2. Its low three bits encode the ward tier;
-- upper bits are unrelated flags and must be preserved. Capture-derived rows require an exact,
-- unique name + model + level match and a consistent tier across all matched packets. This update
-- does not infer wards from creature rank or instance membership. Existing tiers are preserved.

USE `war_world`;

UPDATE `creature_protos`
SET `Unk2` = `Unk2` + 1
WHERE `Entry` IN (
    41, 65, 93, 185, 198, 349, 350, 612, 3915, 7328, 26724, 33369, 35364,
    36040, 38089, 38624, 39098, 39757, 39758, 44192, 44647, 72610, 99418,
    778234, 2501338, 2501340, 10505158, 10505159,
    3040, 3649, 3650, 3651, 3659, 6807, 6834, 6842, 6850, 6856, 7358, 8530,
    16078, 16085, 19409, 20756, 20760, 25721, 26812, 26814, 26815, 33172,
    33173, 33180, 33181, 33182, 33401, 41775, 45224, 46325, 46327, 46334,
    47438, 48128, 49164, 52462, 52594, 61598, 61599, 61601, 93692, 93757,
    93814, 93834, 93835, 93836, 93987, 94101, 94102, 94103, 94190, 94192,
    94272, 94273, 94389, 97425, 97435, 97441, 778041, 1000728, 1000731,
    2000684, 2000725, 2000764, 2000765, 2000766, 2000767, 2000772, 2000774,
    10600231
)
  AND (`Unk2` & 7) = 0;

UPDATE `creature_protos`
SET `Unk2` = `Unk2` + 2
WHERE `Entry` IN (618, 6858, 40782, 97420, 99434)
  AND (`Unk2` & 7) = 0;

UPDATE `creature_protos`
SET `Unk2` = `Unk2` + 3
WHERE `Entry` IN (99621, 99624)
  AND (`Unk2` & 7) = 0;

UPDATE `creature_protos`
SET `Unk2` = `Unk2` + 4
WHERE `Entry` IN (98657, 98663, 98678, 98843, 191191920)
  AND (`Unk2` & 7) = 0;
