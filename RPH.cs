using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedPhoneDXConfig
{
    #region Custom enums
    public enum ICs
    {
        IC_ACK,
        IC_SETS,
        IC_SETS2,
        IC_STMD,
        IC_DINFO_GET,
        IC_DINFO,
        IC_ANY,
        IC_INVALID
    }
    
    public enum IC_RESULT_Enum
    {
        IC_RES_OK = 0,
        IC_RES_INVALID_SYNTAX = 1,
        IC_RES_UNSUPPORTED_CMD = 2,
        IC_RES_ARGUMENT_OUT_OF_RANGE = 3,
        IC_RES_INVALID_OPERATION = 4,
        IC_RES_VALUE_UNAVAILABLE = 5,
        IC_RES_INVALID
    }
   
    public enum RPH_DEVICE_TYPE_Enum
    {
        DT_DX,
        DT_OEM,
        DT_OS,
        DT_UNKNOWN
    } 

    public enum SSB_CH_ID_Enum
    {
        CH_ID_C1_LOW = 0,
        CH_ID_C1_HIGH = 1,
        CH_ID_C2_LOW = 2,
        CH_ID_C2_HIGH = 3,
        CH_ID_C3_LOW = 4,
        CH_ID_C3_HIGH = 5,
        CH_ID_C4_LOW = 6,
        CH_ID_C4_HIGH = 7,
        CH_ID_C5_LOW = 8,
        CH_ID_C5_HIGH = 9,
        CH_ID_C6_LOW = 10,
        CH_ID_C6_HIGH = 11,
        CH_ID_C7_LOW = 12,
        CH_ID_C7_HIGH = 13,
        CH_ID_C8_LOW = 14,
        CH_ID_C8_HIGH = 15,
        CH_ID_INVALID
    }

    #endregion

    public static class RPH
    {
        #region Properties


        public static readonly Func<object, IC_RESULT_Enum> O2_IC_RESULT_Enum = o => o == null ? IC_RESULT_Enum.IC_RES_INVALID : (IC_RESULT_Enum)(int)o;
        public static readonly Func<object, RPH_DEVICE_TYPE_Enum> O2_RPH_DEVICE_TYPE_Enum = o => o == null ? RPH_DEVICE_TYPE_Enum.DT_UNKNOWN : (RPH_DEVICE_TYPE_Enum)(int)o;        


        public static readonly Func<object, string> O2S = o => o == null ? string.Empty : (string)o;
        public static readonly Func<object, double> O2D = o => o == null ? double.NaN : (double)o;
        public static readonly Func<object, int> O2S32 = o => o == null ? -1 : (int)o;
        public static readonly Func<object, UInt16> O2U16 = o => o == null ? UInt16.MinValue : (UInt16)(int)o;

        #endregion

        #region Methods       

        public static ICs ICsByMessageID(string msgID) => msgID switch
        {
            "0" => ICs.IC_ACK,
            "1" => ICs.IC_SETS,
            "2" => ICs.IC_SETS2,
            "3" => ICs.IC_STMD,       
            "?" => ICs.IC_DINFO_GET,
            "!" => ICs.IC_DINFO,
            "-" => ICs.IC_ANY,
            _ => ICs.IC_INVALID
        };

        public static string BCDVersionToStr(int versionData)
        {
            return $"{(versionData >> 0x08)}.{(versionData & 0xff):X2}";
        }

        #endregion
    }
}
