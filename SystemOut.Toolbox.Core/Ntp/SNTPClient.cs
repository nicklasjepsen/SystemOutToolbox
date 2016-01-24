using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;

namespace SystemOut.Toolbox.Core.Ntp
{
    /// <summary>
    /// An SNTP client implementation that allows querying an internet time server.
    /// </summary>
    public class SntpClient
    {
        private Socket _udpSocket;
        private SocketAsyncEventArgs _socketEventArgs;

        /// <summary>
        /// Occurs when the request operation has completed.
        /// </summary>
        public event EventHandler<SntpResultEventArgs> Completed;

        #region Convenient methods

        /// <summary>
        /// A convenient method that builds an NTP message suitable to query an internet time server for its time
        /// and sends the query to the given remote server.
        /// </summary>
        /// <param name="timeServer">The endpoint of the time server to use.</param>
        public void GetTimeAsync(EndPoint timeServer)
        {
            var message = new Message();
            message.Mode = Mode.Client;
            // set the transmit timestamp;
            // the server will copy this timestamp to the "OriginTimestamp" field in its reply
            message.TransmitTimestamp = DateTime.Now.ToUniversalTime();
            SendRequestAsync(timeServer, message);
        }

        /// <summary>
        /// A convenient method that builds an NTP message suitable to query an internet time server for its time
        /// and sends the query to the given remote server.
        /// </summary>
        /// <param name="timeServer">The host name of the time server to use.</param>
        public void GetTimeAsync(string timeServerHostName)
        {
            var endPoint = new DnsEndPoint(timeServerHostName, Constants.NtpPort);
            GetTimeAsync(endPoint);
        }

        /// <summary>
        /// A convenient method that builds an NTP message suitable to query an internet time server for its time
        /// and sends the query to the given remote server.
        /// </summary>
        /// <param name="timeServer">The IP address of the time server to use.</param>
        public void GetTimeAsync(IPAddress timeServerAddress)
        {
            var endPoint = new IPEndPoint(timeServerAddress, Constants.NtpPort);
            GetTimeAsync(endPoint);
        }

        #endregion

        /// <summary>
        /// A method that allows to send an externally build and configured NTP message to a remote server.
        /// </summary>
        /// <param name="timeServer">The endpoint of the time server to use.</param>
        /// <param name="message">The pre-build and configured message to send.</param>
        public void SendRequestAsync(EndPoint timeServer, Message message)
        {
            _udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socketEventArgs = new SocketAsyncEventArgs();
            _socketEventArgs.Completed += SocketEventsArgs_Completed;

            // set the remote endpoint and then send the actual data
            _socketEventArgs.RemoteEndPoint = timeServer;
            var buffer = message.ToRawData();
            _socketEventArgs.SetBuffer(buffer, 0, buffer.Length);

            if (!_udpSocket.SendToAsync(_socketEventArgs))
            {
                SocketEventsArgs_Completed(_udpSocket, _socketEventArgs);
            }
        }

        /// <summary>
        /// A method that allows to send an externally build and configured NTP message to a remote server.
        /// </summary>
        /// <param name="timeServerHostName">The host name of the time server to use.</param>
        /// <param name="message">The pre-build and configured message to send.</param>
        public void SendRequestAsync(string timeServerHostName, Message message)
        {
            var endPoint = new DnsEndPoint(timeServerHostName, Constants.NtpPort);
            SendRequestAsync(endPoint, message);
        }

        /// <summary>
        /// A method that allows to send an externally build and configured NTP message to a remote server.
        /// </summary>
        /// <param name="timeServerAddress">The IP address of the time server to use.</param>
        /// <param name="message">The pre-build and configured message to send.</param>
        public void SendRequestAsync(IPAddress timeServerAddress, Message message)
        {
            var endPoint = new IPEndPoint(timeServerAddress, Constants.NtpPort);
            SendRequestAsync(endPoint, message);
        }

        private void SocketEventsArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            // first check for errors
            if (_socketEventArgs.SocketError != SocketError.Success)
            {
                // clean up and notify the outside world
                CleanUp();
                RaiseCompletedEvent(e.SocketError, null);
                return;
            }

            // check to see what we need to do next
            switch (_socketEventArgs.LastOperation)
            {
                case SocketAsyncOperation.SendTo:
                    // sending the request was successful, now reconfigure for receiving the response
                    _socketEventArgs.RemoteEndPoint = new IPEndPoint(IPAddress.Any, Constants.NtpPort);
                    if (!_udpSocket.ReceiveFromAsync(_socketEventArgs))
                    {
                        SocketEventsArgs_Completed(_udpSocket, _socketEventArgs);
                    }
                    break;
                case SocketAsyncOperation.ReceiveFrom:
                    // receiving the response was successful => cleanup and prepare data
                    CleanUp();

                    // we also set the destination timestamp
                    var message = new Message(e.Buffer);
                    message.DestinationTimestamp = DateTime.Now.ToUniversalTime();

                    // notify the outside world
                    RaiseCompletedEvent(SocketError.Success, message);
                    break;
            }
        }

        private void CleanUp()
        {
            // release all resources potentially acquired by the event args and socket
            if (_socketEventArgs != null)
            {
                _socketEventArgs.Dispose();
                _socketEventArgs = null;
            }

            if (_udpSocket != null)
            {
                _udpSocket.Dispose();
                _udpSocket = null;
            }
        }

        private void RaiseCompletedEvent(SocketError error, Message message)
        {
            var handlers = Completed;
            if (handlers != null)
            {
                // build the event args and raise event
                var eventArgs = new SntpResultEventArgs();
                eventArgs.Error = error;
                eventArgs.Message = message;

                handlers(this, eventArgs);
            }
        }

        // TODO: add a way to abort pending operations
    }
}
