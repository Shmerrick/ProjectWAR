using FrameWork;

namespace WorldServer.NetWork.Crypt
{
    [Crypt("RC4Crypto")]
    public class RC4Crypto : ICryptHandler
    {
        public void Crypt(CryptKey key, byte[] packet, int offset, int len)
        {
            PacketOut.EncryptMythicRC4(key.GetbKey(), packet, offset, len);
        }

        public void Decrypt(CryptKey key, byte[] packet, int offset, int len)
        {
            PacketIn.DecryptMythicRC4(key.GetbKey(), packet, offset, len);
        }

        public CryptKey GenerateKey(BaseClient client)
        {
            return new CryptKey(new byte[0]);
        }
    }
}
