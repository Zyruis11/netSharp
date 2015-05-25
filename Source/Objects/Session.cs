﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using netSharp.Components;
using netSharp.Events;

namespace netSharp.Objects
{
    public class Session : IDisposable
    {
        private readonly TcpClient _tcpClient;
        private ASCIIEncoding _asciiEncoding;
        private Action _messageRecievedCallback;
        private NetworkStream _networkStream;
        private Task _streamReaderThread;

        /// <summary>
        /// </summary>
        /// <param name="ipEndPoint"></param>
        /// <param name="sessionType">0 = Session to Client, 1 = Session to ToServer</param>
        /// <param name="tcpClient"></param>
        public Session(int sessionType, IPEndPoint ipEndPoint, string localGuid, TcpClient tcpClient = null)
        {
            switch (sessionType)
            {
                case 0: // Session to Client, generated by Server.
                {
                    if (tcpClient == null)
                    {
                        throw new Exception("Invalid sessionType");
                    }
                    _tcpClient = tcpClient;
                    RemoteEndpointIpAddressPort = _tcpClient.Client.RemoteEndPoint.ToString();
                    UseHeartbeat = false;
                    break;
                }
                case 1: // Session to Server, generated by Client.
                {
                    if (tcpClient != null)
                    {
                        throw new Exception("Invalid sessionType");
                    }
                    _tcpClient = new TcpClient();
                    RemoteEndpointIp = ipEndPoint;
                    UseHeartbeat = false;
                    break;
                }
                default:
                {
                    Dispose();
                    throw new Exception("Invalid/Unspecified sessionType.");
                }
            }
            LocalEndpointGuid = localGuid;
            RemoteEndpointGuid = "notset";
            if (UseHeartbeat)
            {
                TimeSinceLastHeartbeatRecieve = 0;
                MaxTimeSinceLastHeartbeatReceive = 30;
                TimeUntilNextHeartbeatSend = 0;
            }
            IdleTime = 0;
            MaxIdleTime = 900;
            Connect();
        }

        public bool IsDisposed { get; set; }
        public string RemoteEndpointGuid { get; set; }
        public string LocalEndpointGuid { get; set; }
        public IPEndPoint RemoteEndpointIp { get; set; }
        public string RemoteEndpointIpAddressPort { get; set; }
        public int TimeSinceLastHeartbeatRecieve { get; set; }
        public int MaxTimeSinceLastHeartbeatReceive { get; set; }
        public int TimeUntilNextHeartbeatSend { get; set; }
        public bool UseHeartbeat { get; set; }
        public int IdleTime { get; set; }
        public int MaxIdleTime { get; set; }
        public byte Cost { get; set; }
        public bool SentGuid { get; set; }

        public void Dispose()
        {
            Disconnect();
            IsDisposed = true;
        }

        public event EventHandler<NetSharpEventArgs> SessionDataRecieved;
        public event EventHandler<NetSharpEventArgs> SessionErrorOccured;
        // Event Handler-Trigger Binding
        protected virtual void EventInvocationWrapper(NetSharpEventArgs netSharpEventArgs,
            EventHandler<NetSharpEventArgs> eventHandler)
        {
            if (eventHandler != null)
            {
                eventHandler(this, netSharpEventArgs);
            }
        }

        public void SessionDataRecievedTrigger(DataStream DataStream, Session session)
        {
            EventInvocationWrapper(new NetSharpEventArgs(DataStream, session), SessionDataRecieved);
        }

        public void SessionErrorOccuredTrigger(string exceptionMessage)
        {
            EventInvocationWrapper(new NetSharpEventArgs(null, null, exceptionMessage), SessionErrorOccured);
        }

        private void Connect()
        {
            if (!_tcpClient.Connected)
            {
                _tcpClient.Connect(RemoteEndpointIp);
                RemoteEndpointIpAddressPort = _tcpClient.Client.RemoteEndPoint.ToString();
            }
            _networkStream = _tcpClient.GetStream();
            StartStreamReaderTask();
        }

        public void Disconnect()
        {
            if (_tcpClient.Connected)
            {
                _networkStream.Close();
                _tcpClient.Close();
            }
        }

        public void StartStreamReaderTask()
        {
            _streamReaderThread = new Task(StreamReader);
            _streamReaderThread.Start();
        }

        public void SendData(DataStream DataStream)
        {
            if (DataStream != null)
            {
                StreamWriter(DataStream);
            }
        }

        /// <summary>
        ///     Blocks on the NetworkStream of the TcpClient, it recieves data sent across
        ///     the stream and sends it to a parsing function for further processing.
        /// </summary>
        private void StreamReader()
        {
            try
            {
                while (!IsDisposed)
                {
                    if (_networkStream.CanRead)
                    {
                        var payloadLengthBuffer = new byte[2];
                        // Create a buffer to hold the contents of the payload length value
                        var initialBytesRead = _networkStream.Read(payloadLengthBuffer, 0, payloadLengthBuffer.Length);

                        var streamBuffer = new byte[DataStreamFactory.GetPayloadLength(payloadLengthBuffer) + 12];
                        // Create a buffer to hold the contents of the full DataStream

                        if (initialBytesRead == 0)
                            // If the streamIndex remains 0 after the blocking read exits we didn't recieve any data.
                        {
                            break;
                        }

                        _networkStream.Read(streamBuffer, 0, streamBuffer.Length);
                        // Read the rest of the network stream into the buffer
                        var dataStream = DataStreamFactory.ByteArrayToStream(streamBuffer,
                            DataStreamFactory.GetPayloadLength(payloadLengthBuffer));
                        // Pass the buffer to the deserializer and fill the DataStream object.

                        _networkStream.Flush();

                        if (RemoteEndpointGuid == "notset")
                        {
                            RemoteEndpointGuid = dataStream.Guid;
                        }

                        SessionDataRecievedTrigger(dataStream, this);
                        // Pass the DataStream object into the event invocation wrapper and fire the SessionDataRecievedEvent
                    }
                }
            }
            catch (Exception exception)
            {
                SessionErrorOccuredTrigger(exception.Message);
            }
            finally
            {
                _networkStream.Close();
            }
        }

        private void StreamWriter(DataStream DataStream)
        {
            if (_networkStream.CanWrite)
            {
                var serializedDataStream = DataStreamFactory.StreamToByteArray(DataStream);

                _networkStream.Write(serializedDataStream, 0, serializedDataStream.Length);
            }
        }
    }
}