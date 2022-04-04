using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UCNLDrivers;

namespace RedPhoneDXConfig
{
    public class RedPhoneDXManager
    {
        #region Properties

        NMEASerialPort port;
        PrecisionTimer timer;

        public bool IsRunning
        {
            get { return (port != null) && (port.IsOpen); }
        }

        #endregion

        #region Constructor

        public RedPhoneDXManager()
        {

        }
        
        #endregion

        #region Methods


        #endregion

        #region Handlers


        #endregion

        #region Events


        #endregion
    }
}
