using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemOut.Toolbox.Core.Ntp
{
    /// <summary>
    /// The mode that is used.
    /// </summary>
    public enum Mode
    {
        /// <summary>
        /// A reserved value that should not be used.
        /// </summary>
        Reserved = 0,
        /// <summary>
        /// Symmetric active mode.
        /// </summary>
        SymmetricActive,
        /// <summary>
        /// Symmetric passive mode.
        /// </summary>
        SymmetricPassive,
        /// <summary>
        /// Client mode.
        /// </summary>
        Client,
        /// <summary>
        /// Server mode.
        /// </summary>
        Server,
        /// <summary>
        /// Broadcast mode.
        /// </summary>
        Broadcast,
        /// <summary>
        /// An NTP control message.
        /// </summary>
        ControlMessage,
        /// <summary>
        /// A reserved value that should not be used.
        /// </summary>
        ReservedForPrivateUse
    }
}
