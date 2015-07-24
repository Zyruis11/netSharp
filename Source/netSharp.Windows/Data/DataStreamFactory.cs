using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace netSharp.Data
{
    public static class DataStreamFactory
    {
        public static DataStream InitializeStreamObject(byte[] protocolInfoBytes)
        {
            var stream = new DataStream();
            using (var memStream = new MemoryStream(protocolInfoBytes))
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

        public static byte[] GetStreamByteArray(DataStream DataStream)
        {
            var byteArrayList = new List<byte[]>();

            byteArrayList.Add(BitConverter.GetBytes(Convert.ToInt32(DataStream.PayloadByteArray.Length)));
                // Add payloadLength to list
            byteArrayList.Add(Encoding.UTF8.GetBytes(DataStream.Guid)); // Add GUID to list
            byteArrayList.Add(BitConverter.GetBytes(DataStream.PayloadType)); // Add payloadType to list
            byteArrayList.Add(DataStream.PayloadByteArray); // Add payload to list

            return ByteArrayListCombinator(byteArrayList);
        }

        private static byte[] ByteArrayListCombinator(List<byte[]> byteArrayList)
        {
            var returnArray = new byte[byteArrayList.Sum(a => a.Length)];
            var offset = 0;
            foreach (var array in byteArrayList)
            {
                Buffer.BlockCopy(array, 0, returnArray, offset, array.Length);
                offset += array.Length;
            }
            return returnArray;
        }
    }
}