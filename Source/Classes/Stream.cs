using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace netSharp.Classes
{
    public class Stream
    {
        public string Guid { get; set; }
        public ushort PayloadType { get; set; }
        public ushort PayloadLength {
            get { return Convert.ToUInt16(Payload.Length); }
        }
        public byte[] Payload { get; set; }
    }
}
