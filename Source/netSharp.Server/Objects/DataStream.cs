﻿using netSharp.Server.Components;

namespace netSharp.Server.Objects
{
    public class DataStream
    {
        /// <summary>
        ///     Use this constructor to istantiate a new DataStream object with paramaters
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="payloadType"></param>
        /// <param name="payloadObject"></param>
        public DataStream(string guid, ushort payloadType, byte[] payloadObject)
        {
            Guid = guid;
            PayloadType = payloadType;

            if (payloadObject != null)
            {
                PayloadByteArray = payloadObject;
            }
        }

        /// <summary>
        ///     Use this constructor to instantiate a new DataStream object with null props
        ///     that can be set later.
        /// </summary>
        public DataStream()
        {
        }

        public string Guid { get; set; }
        public int PayloadLength { get; set; }
        public ushort PayloadType { get; set; }
        public byte[] PayloadByteArray { get; set; }

        public object GetPayloadObject()
        {
            if (PayloadByteArray != null)
            {
                return DataStreamFactory.PayloadDeserializer(PayloadByteArray);
            }
            return null;
        }
    }
}