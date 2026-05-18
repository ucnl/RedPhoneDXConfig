using UCNLDrivers;
using UCNLDrivers.uAux;
using UCNLNMEA;

namespace RedPhoneDXConfig
{
    public class ACKReceivedEventArgs : EventArgs
    {
        // $RPH0,[cmdID],result

        public ICs SentenceID { get; private set; }
        public IC_RESULT_Enum ResultID { get; private set; }

        public ACKReceivedEventArgs(ICs sntID, IC_RESULT_Enum resID)
        {
            SentenceID = sntID;
            ResultID = resID;
        }
    }

    public class SETSReceivedEventArgs : EventArgs
    {
        // $PRPH1,[ssbChID],[isRWLT],[RWLT_DiverID],[RWLT_ChID]

        public SSB_CH_ID_Enum SSBChannelID { get; private set; }
        public bool IsRWLT { get; private set; }
        public int RWLT_DiverID { get; private set; }
        public int RWLT_ChID { get; private set; }

        public SETSReceivedEventArgs(SSB_CH_ID_Enum ssbChID, bool isRWLT, int rwltDiverID, int rwltChID)
        {
            SSBChannelID = ssbChID;
            IsRWLT = isRWLT;
            RWLT_DiverID = rwltDiverID;
            RWLT_ChID = rwltChID;
        }
    }

    public class SETS2ReceivedEventArgs : EventArgs
    {
        // $PRPH2,[flashWrite],[ssbChID],[hpOutVolume],[vadSensitivity],[lowBatThresholdV],[is_rwlt],[rwltDiverID],[flags1],[res2],[res3]

        public bool FlashWrite { get; private set; }
        public SSB_CH_ID_Enum SSBChannelID { get; private set; }
        public int HeadphoneOutVolume { get; private set; }
        public int VADSensitivity { get; private set; }
        public double LowBatteryThresholdV { get; private set; }
        public bool IsRWLT { get; private set; }
        public int RWLT_DiverID { get; private set; }
        public int Flags1 { get; private set; }
        public int Reserved1 { get; private set; }
        public int Reserved2 { get; private set; }

        public SETS2ReceivedEventArgs(bool flashWrite, SSB_CH_ID_Enum ssbChID, int hdOutVolume,
            int vadSensitivity, double lowBatThresholdV, bool isRWLT, int rwltDiverID,
            int flags1, int res1, int res2)
        {
            FlashWrite = flashWrite;
            SSBChannelID = ssbChID;
            HeadphoneOutVolume = hdOutVolume;
            VADSensitivity = vadSensitivity;
            LowBatteryThresholdV = lowBatThresholdV;
            IsRWLT = isRWLT;
            RWLT_DiverID = rwltDiverID;
            Flags1 = flags1;
            Reserved1 = res1;
            Reserved2 = res2;
        }
    }

    public class STMDReceivedEventArgs : EventArgs
    {
        // $PRPH3,[ssbChID],[mode],[res1],[res2]

        public SSB_CH_ID_Enum SSBChannelID { get; private set; }
        public int Mode { get; private set; }
        public int Reserved1 { get; private set; }
        public int Reserved2 { get; private set; }

        public STMDReceivedEventArgs(SSB_CH_ID_Enum ssbChID, int mode, int res1, int res2)
        {
            SSBChannelID = ssbChID;
            Mode = mode;
            Reserved1 = res1;
            Reserved2 = res2;
        }
    }


    public class RPHPort : uAuxPort
    {
        private static bool _nmeaSingleton = false;

        private bool _isWaitingLocal;
        public bool IsWaitingLocal
        {
            get => _isWaitingLocal;
            private set
            {
                if (_isWaitingLocal != value)
                {
                    _isWaitingLocal = value;
                    IsWaitingLocalChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private const int DefaultTimeoutMs = 1000;
        private const int SETSTimeoutMs = 3000;

        private ICs? _lastQueryID = ICs.IC_INVALID;

        public bool IsDeviceInfoValid { get; private set; }
        public RPH_DEVICE_TYPE_Enum DeviceType { get; private set; }
        public string SerialNumber { get; private set; } = string.Empty;
        public string SystemInfo { get; private set; } = string.Empty;
        public string SystemVersion { get; private set; } = string.Empty;

        public override string Id { get; }

        public override AuxSourceKind Kind => AuxSourceKind.Custom;


        public RPHPort(string id, BaudRate baudRate) 
            : base(baudRate)
        {
            Id = id;
            PortDescription = "RPH";
            IsLogIncoming = true;
            IsTryAlways = true;

            NMEAInit();
        }

        public override void InitQuerySend()
        {
            var msg = NMEAParser.BuildProprietarySentence(ManufacturerCodes.RPH, "?", new object[] { 0 });
            Send(msg);
        }

        public override void ProcessIncoming(NMEASentence sentence)
        {
            if (sentence is not NMEAProprietarySentence pSentence ||
                pSentence.Manufacturer != ManufacturerCodes.RPH)
                return;

            if (!detected && "0123!".Contains(pSentence.SentenceIDString))
            {
                detected = true;
                StopTimer();
            }

            switch (pSentence.SentenceIDString)
            {
                case "0": Parse_ACK(pSentence.parameters); break;
                case "1": Parse_SETS(pSentence.parameters); break;
                case "2": Parse_SETS2(pSentence.parameters); break;
                case "3": Parse_STMD(pSentence.parameters); break;
                case "!": Parse_DINFO(pSentence.parameters); break;
            }
        }



        public bool Query_DINFO()
        {
            StopTimer();
            var msg = NMEAParser.BuildProprietarySentence(ManufacturerCodes.AZM, "?", new object[] { 0 });
            return TrySend(msg, ICs.IC_DINFO_GET);
        }
        
        public bool Query_SETS(SSB_CH_ID_Enum ssbChID = SSB_CH_ID_Enum.CH_ID_C1_LOW, bool isRWLT = false, byte RWLT_DiverID = 0, byte RWLT_ChID = 0)
        {
            // $PRPH1,[ssbChID],[isRWLT],[RWLT_DiverID],[RWLT_ChID]

            var msg = NMEAParser.BuildProprietarySentence(ManufacturerCodes.RPH, "1",
                new object?[]
                {
                    ssbChID == SSB_CH_ID_Enum.CH_ID_INVALID ? (int?)ssbChID : null,
                    Convert.ToInt32(isRWLT),
                    (int)RWLT_DiverID,
                    (int)RWLT_ChID,
                });

            return TrySend(msg, ICs.IC_SETS);
        }

        // Для чтения настроек - все параметры null
        public bool Query_SETS2_Read()
        {
            var msg = NMEAParser.BuildProprietarySentence(ManufacturerCodes.RPH, "2",
                new object?[] { null, null, null, null, null, null, null, null, null, null });

            return TrySend(msg, ICs.IC_SETS);
        }

        // Для записи настроек - только указанные параметры
        public bool Query_SETS2_Write(
            bool? flashWrite = null,
            SSB_CH_ID_Enum? ssbChID = null,
            byte? hdOutVolume = null,
            byte? vadSensitivity = null,
            double? lowBatThresholdV = null,
            bool? isRWLT = null,
            byte? RWLT_DiverID = null,
            byte? flags = null,
            byte? res1 = null,
            byte? res2 = null)
        {
            var msg = NMEAParser.BuildProprietarySentence(ManufacturerCodes.RPH, "2",
                new object?[]
                {
            flashWrite.HasValue ? Convert.ToInt32(flashWrite.Value) : null,
            ssbChID.HasValue && ssbChID.Value != SSB_CH_ID_Enum.CH_ID_INVALID ? (int)ssbChID.Value : null,
            hdOutVolume.HasValue ? (int)hdOutVolume.Value : null,
            vadSensitivity.HasValue ? (int)vadSensitivity.Value : null,
            lowBatThresholdV.HasValue ? lowBatThresholdV.Value : null,
            isRWLT.HasValue ? Convert.ToInt32(isRWLT.Value) : null,
            RWLT_DiverID.HasValue ? (int)RWLT_DiverID.Value : null,
            flags.HasValue ? (int)flags.Value : null,
            res1.HasValue ? (int)res1.Value : null,
            res2.HasValue ? (int)res2.Value : null,
                });

            return TrySend(msg, ICs.IC_SETS);
        }

        public bool Query_STMD(SSB_CH_ID_Enum ssbChID = SSB_CH_ID_Enum.CH_ID_INVALID, int mode = 0, int res1 = 0, int res2 = 0)
        {
            // $PRPH3,[ssbChID],[mode],[res1],[res2]

            var msg = NMEAParser.BuildProprietarySentence(ManufacturerCodes.RPH, "3",
                new object?[]
                {
                    ssbChID == SSB_CH_ID_Enum.CH_ID_INVALID ? (int?)ssbChID : null,
                    mode >= 0 ? (int?)mode : null,
                    res1 >= 0 ? (int?)res1 : null,
                    res2 >= 0 ? (int?)res2 : null
                });

            return TrySend(msg, ICs.IC_SETS);
        }

        private void Parse_ACK(object[] parameters) => SafeParse(() =>
        {
            ICs sntID = RPH.ICsByMessageID(parameters[0].ToString());
            IC_RESULT_Enum resID = RPH.O2_IC_RESULT_Enum(parameters[1]);

            StopTimer();
            IsWaitingLocal = false;

            ACKReceived?.Invoke(this, new ACKReceivedEventArgs(sntID, resID));
        }, "RPH0");

        private void Parse_SETS(object[] parameters) => SafeParse(() =>
        {
            StopTimer();
            IsWaitingLocal = false;            

            SSB_CH_ID_Enum ssbChID = parameters[0] == null ? SSB_CH_ID_Enum.CH_ID_INVALID : (SSB_CH_ID_Enum)(int)parameters[0];
            bool isRWLT = parameters[1] == null ? false : (bool)parameters[1];
            int RWLT_DiverID = parameters[2] == null ? 0 : (int)parameters[2];
            int RWLT_ChID = parameters[3] == null ? 0 : (int)parameters[3];


            SETSReceived?.Invoke(this, new SETSReceivedEventArgs(ssbChID, isRWLT, RWLT_DiverID, RWLT_ChID));
        }, "AZM1");

        private void Parse_SETS2(object[] parameters) => SafeParse(() =>
        {
            StopTimer();
            IsWaitingLocal = false;

            /*            
            #define IC_SETS2     '2' // $PRPH2,[flashWrite],[ssbChID],[hpOutVolume],[vadSensitivity],[lowBatThresholdV],[is_rwlt],[rwltDiverID],[flags1],[res2],[res3]
            #define IC_STMD      '3' // $PRPH3,[ssbChID],[mode],[res1],[res2]
            */

            bool flashWrite = parameters[0] == null ? false : (bool)parameters[0];
            SSB_CH_ID_Enum ssbChID = parameters[1] == null ? SSB_CH_ID_Enum.CH_ID_INVALID : (SSB_CH_ID_Enum)(int)parameters[1];
            int hdOutVolume = parameters[2] == null ? 0 : (int)parameters[2];
            int vadSensitivity = parameters[3] == null ? 0 : (int)parameters[3];
            double lowBatThresholdV = parameters[4] == null ? 0 : (double)parameters[4];

            bool isRWLT = parameters[5] == null ? false : Convert.ToBoolean((int)parameters[5]);
            int RWLT_DiverID = parameters[6] == null ? 0 : (int)parameters[6];            

            int flags1 = parameters[7] == null ? 0 : (int)parameters[7];
            int res1 = parameters[8] == null ? 0 : (int)parameters[8];
            int res2 = parameters[9] == null ? 0 : (int)parameters[9];

            SETS2Received?.Invoke(this, new SETS2ReceivedEventArgs(flashWrite, ssbChID, hdOutVolume, vadSensitivity, lowBatThresholdV, isRWLT, RWLT_DiverID, flags1, res1, res2));
        }, "AZM2");

        private void Parse_STMD(object[] parameters) => SafeParse(() =>
        {
            StopTimer();
            if (Status != AuxStatus.Inactive)
                StartTimer(3000);

            /*                        
            #define IC_STMD      '3' // $PRPH3,[ssbChID],[mode],[res1],[res2]
            */

            SSB_CH_ID_Enum ssbChID = parameters[0] == null ? SSB_CH_ID_Enum.CH_ID_INVALID : (SSB_CH_ID_Enum)(int)parameters[0];
            int mode = parameters[2] == null ? 0 : (int)parameters[2];
            int res1 = parameters[3] == null ? 0 : (int)parameters[3];
            int res2 = parameters[4] == null ? 0 : (int)parameters[4];

            STMDReceived?.Invoke(this, new STMDReceivedEventArgs(ssbChID, mode, res1, res2));
        }, "AZM3");

        private void Parse_DINFO(object[] parameters) => SafeParse(() =>
        {
            // $PRPH!,[dtype],sn,sys_info,sys_ver,[res1],[res2],[res3]

            DeviceType = RPH.O2_RPH_DEVICE_TYPE_Enum(parameters[0]);
            
            SerialNumber = RPH.O2S(parameters[1]);
            SystemInfo = RPH.O2S(parameters[2]);
            SystemVersion = RPH.BCDVersionToStr(RPH.O2S32(parameters[3]));
            int res1 = parameters[4] == null ? 0 : (int)parameters[4];
            int res2 = parameters[5] == null ? 0 : (int)parameters[5];
            int res3 = parameters[6] == null ? 0 : (int)parameters[6];

            IsDeviceInfoValid = (DeviceType != RPH_DEVICE_TYPE_Enum.DT_UNKNOWN) &&
                                !string.IsNullOrEmpty(SerialNumber);

            DeviceInfoValidChanged?.Invoke(this, EventArgs.Empty);
        }, "RPH!");



        private void LogError(string context, Exception ex)
        {
            LogEventHandler?.Invoke(this,
                new LogEventArgs(LogLineType.ERROR,
                    $"AuxRPHPort ({PortName}): {context} - {ex.Message}"));
        }

        private static void NMEAInit()
        {
            if (_nmeaSingleton) return;
            _nmeaSingleton = true;

            NMEAParser.AddManufacturerToProprietarySentencesBase(ManufacturerCodes.RPH);
            
            /*
            #define IC_ACK       '0' // $PRPH0,[cmdID],result
            #define IC_SETS      '1' // $PRPH1,[ssbChID],[isRWLT],[RWLT_DiverID],[RWLT_ChID]
            #define IC_SETS2     '2' // $PRPH2,[flashWrite],[ssbChID],[hpOutVolume],[vadSensitivity],[lowBatThresholdV],[is_rwlt],[rwltDiverID],[flags1],[res2],[res3]
            #define IC_STMD      '3' // $PRPH3,[ssbChID],[mode],[res1],[res2]

            #define IC_DINFO_GET '?' // $PRPH?,[res1]
            #define IC_DINFO     '!' // $PRPH!,[dtype],sn,sys_info,sys_ver,[res1],[res2],[res3]
            #define IC_ANY       '-' //
            */


            var sentences = new[]
            {
                ("0", "x,x"),                   // $PRPH0,[cmdID],result
                ("1", "x,x,x,x"),               // $PRPH1,[ssbChID],[isRWLT],[RWLT_DiverID],[RWLT_ChID]
                ("2", "x,x,x,x,x.x,x,x,x,x,x"), // $PRPH2,[flashWrite],[ssbChID],[hpOutVolume],[vadSensitivity],[lowBatThresholdV],[is_rwlt],[rwltDiverID],[flags1],[res2],[res3]
                ("3", "x,x,x,x"),               // $PRPH3,[ssbChID],[mode],[res1],[res2]                
                ("?", "x"),                     // $PRPH?,[res1]
                ("!", "x,c--c,c--c,x,x,x,x"),     // $PRPH!,[dtype],sn,sys_info,sys_ver,[res1],[res2],[res3]
            };

            foreach (var (id, format) in sentences)
                NMEAParser.AddProprietarySentenceDescription(ManufacturerCodes.RPH, id, format);
        }

        private bool TrySend(string message, ICs queryID)
        {
            if (!detected || IsWaitingLocal)
                return false;

            try
            {
                Send(message);

                int timeout = DefaultTimeoutMs;
                if ((queryID == ICs.IC_SETS) ||
                    (queryID == ICs.IC_SETS2))
                    timeout = SETSTimeoutMs;

                StartTimer(timeout);

                IsWaitingLocal = true;
                _lastQueryID = queryID;

                return true;
            }
            catch (Exception ex)
            {
                LogEventHandler?.Invoke(this, new LogEventArgs(LogLineType.ERROR, ex));
                return false;
            }
        }

        private void SafeParse(Action parseAction, string sentenceType)
        {
            try
            {
                parseAction();
            }
            catch (Exception ex)
            {
                LogError($"Error parsing {sentenceType}", ex);
            }
        }


        public EventHandler? IsWaitingLocalChanged;
        public EventHandler? DeviceInfoValidChanged;
        public EventHandler<ACKReceivedEventArgs>? ACKReceived;
        public EventHandler<SETSReceivedEventArgs>? SETSReceived;
        public EventHandler<SETS2ReceivedEventArgs>? SETS2Received;
        public EventHandler<STMDReceivedEventArgs>? STMDReceived;
    }
}
