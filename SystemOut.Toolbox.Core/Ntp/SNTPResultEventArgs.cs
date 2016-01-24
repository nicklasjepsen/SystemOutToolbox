using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SystemOut.Toolbox.Core.Ntp
{
    /// <summary>
    /// Represents the result of an NTP request.
    /// </summary>
    public class SntpResultEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the error that occurred during the operation.
        /// If no error happened, this is set to <see cref="SocketError.Success"/>.
        /// </summary>
        public SocketError Error
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the NTP message that was received from the server.
        /// This is <c>null</c> in case of errors.
        /// </summary>
        public Message Message
        {
            get;
            set;
        }
    }
}
