using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorldServer.API
{
    public enum Opcodes
    {
        OK,
        ERROR,
        LOGIN,
        DISCONNECT_PLAYER,
        GET_CHARACTER_STATE,
        GET_CHARACTER_LIST,
        START_SERVER,
        STOP_SERVER,
        RESTART_SERVER,
        CHAR_MANAGER_ADD_CHARACTER,
        CHAR_MANAGER_REMOVE_CHARACTER,

        ITEM_ADD,
        ITEM_UPDATE, //include stats
        ITEM_DELETE,
        
        ITEM_SET_ADD,
        ITEM_SET_UPDATE,
        ITEM_SET_DELETE,

        ITEM_SET_BONUS_ADD,
        ITEM_SET_BONUS_UPDATE,
        ITEM_SET_BONUS_DELETE,

        CHAR_CONNECTED,
        CHAR_DISCONNECTED,
        CHAR_TELEPORTED,

        CHAR_LIST,
        CHAR_ITEM_ADD,
        CHAR_ITEM_ADD_RES,
        CHAR_ITEM_DELETE,
        CHAR_ITEM_SET_SLOT_MODEL,
        CHAR_ITEM_UPDATE,
        CHAR_ADD_ABILTIES,
        CHAR_REMOVE_ABILITIES,
        CHAR_TELEPORT,
        CHAR_QUEUE_SCENARIO,
        CHAR_JOIN_SCENARIO,
        CHAR_JOIN_PARTY,
        CHAR_LEAVE_PARTY,
        CHAR_JOIN_WARBAND,
        CHAR_LEAVE_WARBAND,
        CHAR_UPDATE_STATE,
        CHAR_SEND_PACKET,
        CHAR_SEND_TEXT,
        CHAR_DISABLE_INIT,
        CHAR_GET_STATS,
        CHAR_STATS,
        CHAR_USE_ABILITY,
        CHAR_ABILITY_RESULT,
        MONSTER_ADD,
        MONSTER_UPDATE,
        MONSTER_DELTE,
        MONSTER_ADD_AT_PLAYER,

        STATIC_ADD,
        STATIC_UPDATE,
        STATIC_DELETE,

        SERVER_PACKET,
        SERVER_CLIENT_PACKET,
        SERVER_LOG_PACKETS,

        SET_IMAGE_NUM,

        GET_IN_RANGE,
        IN_RANGE_LIST,

        EXECUTE_SCRIPT,
        EXECUTE_SCRIPT_ERROR,
        EXECUTE_SCRIPT_PRINT,
        EXECUTE_SCRIPT_EXCEPTION,
        EXECUTE_SCRIPT_OK,

        CREATE_STATIC,

        ZONE_PIN,
        ZONE_GET_LIST,
        ZONE_LIST,

        SCENARIO_INFO,
    }
}
