// Copyright (c) 2015 Daniel Elps <daniel.j.elps@gmail.com>
// 
// All rights reserved.
// 
// Redistribution and use of netSharp in source and binary forms, with or without 
// modification, are permitted provided that the following conditions 
// are met:
// 
// * Redistributions of source code must retain the above copyright notice, 
//   this list of conditions and the following disclaimer. 
// 
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution. 
// 
// * Neither the name of Daniel Elps nor the names of its contributors may be 
//   used to endorse or promote products derived from this software without 
//   specific prior written permission. 
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF 
// THE POSSIBILITY OF SUCH DAMAGE.
// 
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace netSharp.Factories.DataStream
{
    public static class DataStreamFactory
    {
        public static Data.DataStream InitializeStreamObject(byte[] _protocolInfoBytes)
        {
            var stream = new Data.DataStream();
            using (var memStream = new MemoryStream(_protocolInfoBytes))
            {
                var payloadLengthBuffer = new byte[4];
                memStream.Read(payloadLengthBuffer, 0, payloadLengthBuffer.Length);
                stream.PayloadLength = BitConverter.ToInt32(payloadLengthBuffer, 0);

                var guidBuffer = new byte[4];
                memStream.Read(guidBuffer, 0, guidBuffer.Length);
                stream.Guid = Encoding.UTF8.GetString(guidBuffer,0,4);

                var payloadTypeBuffer = new byte[2];
                memStream.Read(payloadTypeBuffer, 0, payloadTypeBuffer.Length);
                stream.PayloadType = BitConverter.ToUInt16(payloadTypeBuffer, 0);
            }
            return stream;
        }

        public static byte[] GetStreamByteArray(Data.DataStream _dataStream)
        {
            var byteArrayList = new List<byte[]>();

            byteArrayList.Add(BitConverter.GetBytes(Convert.ToInt32(_dataStream.PayloadByteArray.Length)));
                // Add payloadLength to list
            byteArrayList.Add(Encoding.UTF8.GetBytes(_dataStream.Guid)); // Add GUID to list
            byteArrayList.Add(BitConverter.GetBytes(_dataStream.PayloadType)); // Add payloadType to list
            byteArrayList.Add(_dataStream.PayloadByteArray); // Add payload to list

            return ByteArrayListCombinator(byteArrayList);
        }

        private static byte[] ByteArrayListCombinator(List<byte[]> _byteArrayList)
        {
            var returnArray = new byte[_byteArrayList.Sum(_a => _a.Length)];
            var offset = 0;
            foreach (var array in _byteArrayList)
            {
                Buffer.BlockCopy(array, 0, returnArray, offset, array.Length);
                offset += array.Length;
            }
            return returnArray;
        }
    }
}