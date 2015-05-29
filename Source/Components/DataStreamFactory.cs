using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using netSharp.Objects;

namespace netSharp.Components
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
                stream.Guid = Encoding.Default.GetString(guidBuffer);

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
            byteArrayList.Add(Encoding.Default.GetBytes(DataStream.Guid)); // Add GUID to list
            byteArrayList.Add(BitConverter.GetBytes(DataStream.PayloadType)); // Add payloadType to list
            byteArrayList.Add(DataStream.PayloadByteArray); // Add payload to list

            return ByteArrayListCombinator(byteArrayList);
        }

        /// <summary>
        ///     Uses a memory DataStream and a binary formatter to serialize the PayloadObject
        /// </summary>
        /// <param name="_payload"></param>
        /// <returns></returns>
        public static byte[] PayloadSerializer(object _payload)
        {
            using (var ms = new MemoryStream())
            {
                var binForm = new BinaryFormatter();
                binForm.Serialize(ms, _payload);
                return ms.ToArray();
            }
        }

        /// <summary>
        ///     Uses a memory DataStream and a binary formatter to deserialize the PayloadByteArray
        /// </summary>
        /// <returns>Deserialized object</returns>
        public static object PayloadDeserializer(byte[] _payload)
        {
            using (var memStream = new MemoryStream())
            {
                memStream.Write(_payload, 0, _payload.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                var binForm = new BinaryFormatter();
                var obj = binForm.Deserialize(memStream);
                return obj;
            }
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