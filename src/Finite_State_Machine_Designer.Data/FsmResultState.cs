using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finite_State_Machine_Designer.Data
{
    public enum FsmResultState
    {
        /// <summary>
        /// Fully Loaded the Finite State Machine
        /// </summary>
        Success,
        /// <summary>
        /// Loaded only the metadata of Finite State machine
        /// such as Id, Name and Description
        /// </summary>
        MetaDataOnly,
        /// <summary>
        /// Operation to load Finite State Machine was interrupted
        /// </summary>
        Interrupted,
        /// <summary>
        /// Couldn't load the Finite State Machine
        /// </summary>
        Fail
    }
}
