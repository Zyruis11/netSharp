using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace netSharp.Classes
{
    public static class StreamEngine
    {
        //public static string GetGuid(byte[] guidBytes)
        //{
        //    if (guidBytes.Length != 4)
        //    {
        //        throw new Exception("Invalid input byte array length");
        //    }
        //    return null;
        //}

        //public static ushort GetPayloadType(byte[] payloadTypeBytes)
        //{
        //    if (payloadTypeBytes.Length != 2)
        //    {
        //        throw new Exception("Invalid input byte array length");
        //    }
        //    return 0;
        //}

        public static ushort GetPayloadLength(byte[] payloadLengthBytes)
        {
            if (payloadLengthBytes.Length != 2)
            {
                throw new Exception("Invalid input byte array length");
            }
            return 0;
        }

        public static byte[] StreamToByteArray(Stream stream)
        {
            var byteArrayList = new List<byte[]>();
            var futureUsePadding = new byte[4];

            byteArrayList.Add(Encoding.Default.GetBytes(stream.Guid)); // Add GUID to list
            byteArrayList.Add(futureUsePadding); // Add 4-byte future use padding to list
            byteArrayList.Add(BitConverter.GetBytes(stream.PayloadType)); // Add payloadType to list
            byteArrayList.Add(BitConverter.GetBytes(Convert.ToUInt16(stream.Payload.Length))); // Add payloadLength to list
            byteArrayList.Add(stream.Payload); // Add payload to list

            return ByteArrayListCombinator(byteArrayList);
        }

        public static Stream ByteArrayToStream(byte[] streamBytes)
        {
            var superPacket = new Stream();
            var byteIndex = 0;

            using (var memoryStream = new MemoryStream(streamBytes))
            {
                var guidBuffer = new byte[4];
                byteIndex = memoryStream.Read(guidBuffer, byteIndex, guidBuffer.Length);
                superPacket.Guid = Convert.ToString(guidBuffer);

                byteIndex += 4; //Future use padding

                var payloadTypeBuffer = new byte[2];
                byteIndex = memoryStream.Read(payloadTypeBuffer, byteIndex, payloadTypeBuffer.Length);
                superPacket.PayloadType = BitConverter.ToUInt16(payloadTypeBuffer, 0);

                var payloadLengthBuffer = new byte[2];
                byteIndex = memoryStream.Read(payloadLengthBuffer, byteIndex, payloadLengthBuffer.Length);
                superPacket.PayloadType = BitConverter.ToUInt16(payloadLengthBuffer, 0);

                var payloadBuffer = new byte[superPacket.PayloadLength];
                byteIndex = memoryStream.Read(payloadBuffer, byteIndex, superPacket.PayloadLength);
                superPacket.Payload = payloadBuffer;
            }
            return superPacket;
        }

        private static byte[] PayloadSerializer(object _payload)
        {
            using (var ms = new MemoryStream())
            {
                var binForm = new BinaryFormatter();
                binForm.Serialize(ms, _payload);
                return ms.ToArray();
            }
        }

        private static object PayloadDeserializer(byte[] _payload)
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