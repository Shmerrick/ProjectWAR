namespace LauncherServer.Server
{
    public class Opcodes
    {
        public const byte CL_CHECK = 0x01;   // The client sends his version for verification (version of the launcher, check file start)
        public const byte LCR_CHECK = 0x02;   // The server returns the answer and disconnects if necessary.

        public const byte CL_START = 0x03;   // The client requests permission to start. (Username + pass sha256)
        public const byte LCR_START = 0x04;  // The server returns the authorization or not to start the client, the client will launch with the server info

        public const byte CL_CREATE = 0x05;   // The client requests to create an account (Username + Pass + IP);
        public const byte LCR_CREATE = 0x06;  // Server response to client request (response in message string + error bool)

        public const byte CL_INFO = 0x07;   // The client requests information about the realms
        public const byte LCR_INFO = 0x08;   // server response on realms

        //NEW CLIENT LAUNCHER PROTOCOL

        public const byte CL_VERSION = 100;                   //client tells server current launcher exe version
        public const byte LCR_VERSION = 101;                  //server responds with current ror launcher version

        //CL_REQUEST_PATCHER_DOWNLOAD = 102,  //client asks for new launcher exe
        //LCR_PATCHER_DOWNLOAD = 103,         //server responds with launcher exe (gzip compressed).
        public const byte CL_REQUEST_MANIFEST = 104;          //client asks for list of hashes for specified myp

        //LCR_MANIFEST = 105,                 //server responds with gzip compressed list of hashes for specified myp
        //CLR_REQUEST_ASSETS = 106,           //client requests list of assets.
        //LCR_ASSET = 107,                    //server sends mypID, hash, assetData for requested asset
        public const byte CL_LOGIN = 108;                     //client sends username/password/patcher version/computer info

        public const byte LCR_LOGIN = 109;                    //server responds with session token, accountID. Client writes accountID into data.myp
        //CL_REQUEST_NEWS = 110,              //client request current news from ror site (patch notes, etc)
        //LCR_REQUEST_NEWS = 111,             //server responds with news from site in xml document
        //CL_REQUEST_ADDON_LIST = 112,        //client requests list of approved addons
        //LCR_ADDON_LIST = 113,               //server responds with list of approved addons
        //CL_REQUEST_ADDON_DOWNLOAD = 114,    //client requests addon to download
        //LCR_ADDON = 115,                    //server responds with requested addon
        //CL_CRASH_LOG = 116,                 //client patcher has crashed

        //CL_SET_NEWS = 150,
        //CL_ADD_ASSET = 151,
        //CL_UPDATE_ASSET = 152,
        //CL_DELETE_ASSET = 153,
        //CL_SET_NEW_LAUNCHER_FILE = 154,
        //CL_SET_MANIFEST = 155,
        //CL_SET_ASSET_INFO = 156,
        public const byte LCR_DATA_START = 157;

        public const byte LCR_DATA_PART = 158;

        //LCR_DATA_COMPLETE = 159,
        //CL_DATA_START = 160,
        //CL_DATA_PART = 161,
        //CL_DATA_COMPLETE = 162,
        public const byte CL_DATA_REQUEST_PARTS = 163;

        //LCR_DATA_ABORT = 164,      //server tells clients patching has been aborted (happens if devs have changed server files). Further patch requests are halted
        public const byte LCR_ERROR = 165;

        //CL_REQUEST_SERVER_STATUS = 166, //returns info on patcher server, lobby server, game servers
        public const byte LCR_SERVER_STATUS = 167;

        //CL_SET_SERVER_STATUS = 168,
        //LCR_SERVER_STATUS_UPDATE = 169,
        //LCR_DATA_REQUEST_PARTS = 170,
        //CL_SET_SERVER_STATE = 171,
        //LCR_SERVER_STATE_UPDATE = 172,
        //LCR_DATA_READY = 173, //tell client server is able to receive more files
        public const byte CL_REQUEST_MANIFEST_LIST = 174;

        public const byte LCR_REQUEST_MANIFEST_LIST = 175;

        public const byte LCR_DATA_NOT_FOUND = 176; //server does not have requested asset

        //CL_SERVER_CMD = 178,
        //LCR_ASSET_CHANGES_DETECTED = 179,
        public const byte CL_DATA_READY = 180;

        public const byte CL_REQUEST_ASSET = 181;
        public const byte LCR_PATCH_NOTES = 182;
        public const byte CL_SET_PATCH_NOTES = 183;

        //CL_DELETE_ASSETS = 184;
        public const byte CL_PATCHER_LOG = 185;

        public const byte CL_EMAIL_REGISTRATION = 186;
        public const byte LCR_EMAIL_RESPONCE = 187;
    }

    public enum AbortReason : byte
    {
        ACCESS_DENIED = 1,
        SERVER_UPDATE = 2,
        ERROR = 3,
    }

    public enum FileType : int
    {
        MYP_ASSET = 0, //full myp is stored ons erver
        MANIFEST_SET = 1, //client is sending hash manifest
        PATCHER = 2, //admin client is uploading new patcher
        GENERIC = 3, //generic file that server will redistribute to anyone
        FILE_REQUEST = 4,
    }

    public enum FileCompressionMode : byte
    {
        NONE = 0,
        BLOCK = 1,
        WHOLE = 2,
    }

    public enum LauncherErrorCode : int
    {
        UPLOAD_TEMP_FOLDER_NOT_SET = 1,
        PATCHER_FILES_NOT_SET = 2,
        ERROR_STARTING_FILE_TRANSFER = 3,
        INSUFFICIENT_GM_LEVEL = 4,
        UPLOAD_FILE_HASH_ERROR = 5,
        UPLOAD_FILE_EXCEPTION = 6,
        ERROR_CREATING_ACCOUNT = 7
    }

    public enum CheckResult : byte
    {
        LAUNCHER_OK = 0x00,   // Le Launcher est ok
        LAUNCHER_VERSION = 0x01,   // Mauvaise version du launcher
        LAUNCHER_FILE = 0x02   // Fichier manquant dans le launcher
    };

    public enum DownloadResult
    {
        SUCCESS = 0,
        INVALID_HASH = 1,
        FILE_CORRUPT = 2
    };

    public enum Archive
    {
        NONE = 0,
        MFT = 1,
        ART = 2,
        ART2 = 3,
        ART3 = 4,
        AUDIO = 5,
        DATA = 6,
        WORLD = 7,
        INTERFACE = 8,
        VIDEO = 9,
        BLOODHUNT = 10,
        DEV = 11,
        VO_ENGLISH = 12,
        VO_FRENCH = 13,
        VIDEO_FRENCH = 14,
        VO_GERMAN = 15,
        VIDEO_GERMAN = 16,
        VO_ITALIAN = 17,
        VIDEO_ITALIAN = 18,
        VO_SPANISH = 19,
        VIDEO_SPANISH = 20,
        VO_KOREAN = 21,
        VIDEO_KOREAN = 22,
        VO_CHINESE = 23,
        VIDEO_CHINESE = 24,
        VO_JAPANESE = 25,
        VIDEO_JAPANESE = 26,
        VO_RUSSIAN = 27,
        VIDEO_RUSSIAN = 28,
        WARTEST = 29,
        ADDON = 0xFF
    }
}