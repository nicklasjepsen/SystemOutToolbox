using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemOut.Toolbox.Core.Ntp
{
    /// <summary>
    /// Warning of an impending leap second to be inserted or deleted in the last minute of the current month.
    /// </summary>
    public enum LeapIndicator
    {
        /// <summary>
        /// No leap seconds apply to the current month.
        /// </summary>
        NoWarning = 0,
        /// <summary>
        /// The last minute of the day has 61 seconds.
        /// </summary>
        InsertSecond,
        /// <summary>
        /// The last minute of the day has 59 seconds.
        /// </summary>
        DeleteSecond,
        /// <summary>
        /// Unknown because e.g. the clock is unsynchronized.
        /// </summary>
        Unknown
    }
}
