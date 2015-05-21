using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace netSharp.TCP.Classes
{
    public class CurrentStream
    {
        public ushort @payloadType { get; set; }
        public int @payloadLength { get; set; }
        public byte[] @payload { get; set; }
    }
}
