/*
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
using System.Runtime.Remoting.Messaging;
//using System.Linq;
using System.Text;
using System.Threading;

namespace FrameWork
{
    public class CryptKey
    {
        private byte[] _bKey;

        public CryptKey(byte[] Key)
        {
            SetKey(Key);
        }

        public void SetKey(byte[] Key)
        {
            _bKey = (byte[])Key.Clone();
        }

        public byte[] GetbKey()
        {
            return _bKey;
        }
    }

    public interface ICryptHandler
    {
        void Decrypt(CryptKey key, byte[] packet, int offset, int len);

        void Crypt(CryptKey key, byte[] packet, int offset, int len);

        CryptKey GenerateKey(BaseClient client);
    }
}
