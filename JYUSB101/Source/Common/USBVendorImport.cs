﻿using System;
using System.Runtime.InteropServices;

namespace JYUSB101
{
    #region 原厂驱动定义的常量定和函数导入
    //此段通常原厂驱动会提供线程的代码，只需要修改名称等信息
    internal class USB6101Import
    {
        #region 常量定义
        //ADLink PCI Card Type
        public const ushort USB_1902 = 1;
        public const ushort USB_1903 = 2;        //robin@20110919 add
        public const ushort USB_1901 = 3;        //robin@20110919 add
        public const ushort USB_2401 = 4;
        public const ushort USB_7250 = 5;
        public const ushort USB_7230 = 6;
        public const ushort USB_2405 = 7;
        public const ushort USB_1210 = 8;
        public const ushort USB_2401A3 = 9;
        public const ushort USB_101 = 10;
        public const ushort NUM_MODULE_TYPE = 11;

        public const ushort MAX_USB_DEVICE = 9;

        public const ushort INVALID_CARD_ID = 0xFFFF;

        //Error Number
        public const short NoError = 0;
        public const short ErrorUnknownCardType = -1;
        public const short ErrorInvalidCardNumber = -2;
        public const short ErrorTooManyCardRegistered = -3;
        public const short ErrorCardNotRegistered = -4;
        public const short ErrorFuncNotSupport = -5;
        public const short ErrorInvalidIoChannel = -6;
        public const short ErrorInvalidAdRange = -7;
        public const short ErrorContIoNotAllowed = -8;
        public const short ErrorDiffRangeNotSupport = -9;
        public const short ErrorLastChannelNotZero = -10;
        public const short ErrorChannelNotDescending = -11;
        public const short ErrorChannelNotAscending = -12;
        public const short ErrorOpenDriverFailed = -13;
        public const short ErrorOpenEventFailed = -14;
        public const short ErrorTransferCountTooLarge = -15;
        public const short ErrorNotDoubleBufferMode = -16;
        public const short ErrorInvalidSampleRate = -17;
        public const short ErrorInvalidCounterMode = -18;
        public const short ErrorInvalidCounter = -19;
        public const short ErrorInvalidCounterState = -20;
        public const short ErrorInvalidBinBcdParam = -21;
        public const short ErrorBadCardType = -22;
        public const short ErrorInvalidDaRefVoltage = -23;
        public const short ErrorAdTimeOut = -24;
        public const short ErrorNoAsyncAI = -25;
        public const short ErrorNoAsyncAO = -26;
        public const short ErrorNoAsyncDI = -27;
        public const short ErrorNoAsyncDO = -28;
        public const short ErrorNotInputPort = -29;
        public const short ErrorNotOutputPort = -30;
        public const short ErrorInvalidDioPort = -31;
        public const short ErrorInvalidDioLine = -32;
        public const short ErrorContIoActive = -33;
        public const short ErrorDblBufModeNotAllowed = -34;
        public const short ErrorConfigFailed = -35;
        public const short ErrorInvalidPortDirection = -36;
        public const short ErrorBeginThreadError = -37;
        public const short ErrorInvalidPortWidth = -38;
        public const short ErrorInvalidCtrSource = -39;
        public const short ErrorOpenFile = -40;
        public const short ErrorAllocateMemory = -41;
        public const short ErrorDaVoltageOutOfRange = -42;
        public const short ErrorDaExtRefNotAllowed = -43;
        public const short ErrorDIODataWidthError = -44;
        public const short ErrorTaskCodeError = -45;
        public const short ErrortriggercountError = -46;
        public const short ErrorInvalidTriggerMode = -47;
        public const short ErrorInvalidTriggerType = -48;
        public const short ErrorInvalidCounterValue = -50;
        public const short ErrorInvalidEventHandle = -60;
        public const short ErrorNoMessageAvailable = -61;
        public const short ErrorEventMessgaeNotAdded = -62;
        public const short ErrorCalibrationTimeOut = -63;
        public const short ErrorUndefinedParameter = -64;
        public const short ErrorInvalidBufferID = -65;
        public const short ErrorInvalidSampledClock = -66;
        public const short ErrorInvalisOperationMode = -67;

        //Error number for driver API
        public const short ErrorConfigIoctl = -201;
        public const short ErrorAsyncSetIoctl = -202;
        public const short ErrorDBSetIoctl = -203;
        public const short ErrorDBHalfReadyIoctl = -204;
        public const short ErrorContOPIoctl = -205;
        public const short ErrorContStatusIoctl = -206;
        public const short ErrorPIOIoctl = -207;
        public const short ErrorDIntSetIoctl = -208;
        public const short ErrorWaitEvtIoctl = -209;
        public const short ErrorOpenEvtIoctl = -210;
        public const short ErrorCOSIntSetIoctl = -211;
        public const short ErrorMemMapIoctl = -212;
        public const short ErrorMemUMapSetIoctl = -213;
        public const short ErrorCTRIoctl = -214;
        public const short ErrorGetResloctl = -215;
        public const short ErrorCalloctl = -216;
        public const short ErrorPMIntSetIoctl = -217;

        //Error added for USBDASK
        public const short ErrorAccessViolationDataCopy = -301;
        public const short ErrorNoModuleFound = -302;
        public const short ErrorCardIDDuplicated = -303;
        public const short ErrorCardDisconnected = -304;
        public const short ErrorInvalidScannedIndex = -305;
        public const short ErrorUndefinedException = -306;
        public const short ErrorInvalidDioConfig = -307;
        public const short ErrorInvalidAOCfgCtrl = -308;
        public const short ErrorInvalidAOTrigCtrl = -309;
        public const short ErrorConflictWithSyncMode = -310;
        public const short ErrorConflictWithFifoMode = -311;
        public const short ErrorInvalidAOIteration = -312;
        public const short ErrorZeroChannelNumber = -313;
        public const short ErrorSystemCallFailed = -314;
        public const short ErrorTimeoutFromSyncMode = -315;
        public const short ErrorInvalidPulseCount = -316;
        public const short ErrorInvalidDelayCount = -317;
        public const short ErrorConflictWithDelay2 = -318;
        public const short ErrorAOFifoCountTooLarge = -319;
        public const short ErrorConflictWithWaveRepeat = -320;
        public const short ErrorConflictWithReTrig = -321;
        public const short ErrorInvalidTriggerChannel = -322;
        public const short ErrorInvalidRefVoltage = -323;
        public const short ErrorInvalidConversionSrc = -324;
        public const short ErrorInvalidInputSignal = -325;
        public const short ErrorCalibrateFailed = -326;
        public const short ErrorInvalidCalData = -327;
        public const short ErrorChanGainQueueTooLarge = -328;
        public const short ErrorInvalidCardType = -329;
        public const short ErrorInvlaidSyncMode = -330;
        public const short ErrorIICVersion = -331;
        public const short ErrorFX2UpgradeFailed = -332;
        public const short ErrorInvalidReadCount = -333;
        public const short ErrorTEDSInvalidSensorNo = -334;
        public const short ErroeTEDSAccessTimeout = -335;
        public const short ErrorTEDSChecksumFailed = -336;
        public const short ErrorTEDSNotIEEE1451_4 = -337;
        public const short ErrorTEDSInvalidTemplateID = -338;
        public const short ErrorTEDSInvalidPrecisionValue = -339;
        public const short ErrorTEDSUnsupportedTemplate = -340;
        public const short ErrorTEDSInvalidPropertyID = -341;
        public const short ErrorTEDSNoRawData = -342;

        public const short ErrorInvalidChannel = -397;
        public const short ErrorNullPoint = -398;
        public const short ErrorInvalidParamSetting = -399;

        // -401 ~ -499 the Kernel error
        public const short ErrorAIStartFailed = -401;
        public const short ErrorAOStartFailed = -402;
        public const short ErrorConflictWithGPIOConfig = -403;
        public const short ErrorEepromReadback = -404;
        public const short ErrorConflictWithInfiniteOp = -405;
        public const short ErrorWaitingUSBHostResponse = -406;
        public const short ErrorAOFifoModeTimeout = -407;
        public const short ErrorInvalidModuleFunction = -408;
        public const short ErrorAdFifoFull = -409;
        public const short ErrorInvalidTransferCount = -410;
        public const short ErrorConflictWithAIConfig = -411;
        public const short ErrorDDSConfigFailed = -412;
        public const short ErrorFpgaAccessFailed = -413;
        public const short ErrorPLDBusy = -414;
        public const short ErrorPLDTimeout = -415;

        public const short ErrorUndefinedKernelError = -420;
        public const short ErrorSyncModeNotSupport = -501;

        //UsbThermo Error Message
        public const short ErrorInvalidThermoType = -601;
        public const short ErrorOutThermoRange = -602;
        public const short ErrorThermoTable = -603;

        //AD Range
        public const ushort AD_B_10_V = 1;
        public const ushort AD_B_5_V = 2;
        public const ushort AD_B_2_5_V = 3;
        public const ushort AD_B_1_25_V = 4;
        public const ushort AD_B_0_625_V = 5;
        public const ushort AD_B_0_3125_V = 6;
        public const ushort AD_B_0_5_V = 7;
        public const ushort AD_B_0_05_V = 8;
        public const ushort AD_B_0_005_V = 9;
        public const ushort AD_B_1_V = 10;
        public const ushort AD_B_0_1_V = 11;
        public const ushort AD_B_0_01_V = 12;
        public const ushort AD_B_0_001_V = 13;
        public const ushort AD_U_20_V = 14;
        public const ushort AD_U_10_V = 15;
        public const ushort AD_U_5_V = 16;
        public const ushort AD_U_2_5_V = 17;
        public const ushort AD_U_1_25_V = 18;
        public const ushort AD_U_1_V = 19;
        public const ushort AD_U_0_1_V = 20;
        public const ushort AD_U_0_01_V = 21;
        public const ushort AD_U_0_001_V = 22;
        public const ushort AD_B_2_V = 23;
        public const ushort AD_B_0_25_V = 24;
        public const ushort AD_B_0_2_V = 25;
        public const ushort AD_U_4_V = 26;
        public const ushort AD_U_2_V = 27;
        public const ushort AD_U_0_5_V = 28;
        public const ushort AD_U_0_4_V = 29;
        public const ushort AD_B_1_5_V = 30;
        public const ushort AD_B_0_2125_V = 31;
        public const ushort AD_B_40_V = 32;
        public const ushort AD_B_3_16_V = 33;
        public const ushort AD_B_0_316_V = 34;
        public const ushort AD_B_25_V = 35;
        public const ushort AD_B_12_5_V = 36;

        //THERMO
        public const ushort THERMO_B_TYPE = 37;
        public const ushort THERMO_C_TYPE = 38;
        public const ushort THERMO_E_TYPE = 39;
        public const ushort THERMO_K_TYPE = 40;
        public const ushort THERMO_R_TYPE = 41;
        public const ushort THERMO_S_TYPE = 42;
        public const ushort THERMO_T_TYPE = 43;
        public const ushort THERMO_J_TYPE = 44;      //robin@20120503 add
        public const ushort THERMO_N_TYPE = 45;      //robin@20120503 add
        public const ushort RTD_PT100 = 46;
        public const ushort RTD_PT1000 = 47;      //robin@20120322 add
        public const ushort TWO_FIVEK_OHM = 48;
        public const ushort ONETWOZERO_OHM = 49;
        public const ushort THREEFIVEZERO_OHM = 50;
        public const ushort NULL_OHM = 51;

        public const ushort THERMO_MAX_TYPE = 9;

        //Synchronous Mode
        public const ushort SYNCH_OP = 1;
        public const ushort ASYNCH_OP = 2;


        // Input Type
        public const ushort UD_AI_NonRef_SingEnded = 0x01;
        public const ushort UD_AI_SingEnded = 0x02;
        public const ushort UD_AI_Differential = 0x04;
        public const ushort UD_AI_PseudoDifferential = 0x08;

        // Input Coupling
        public const ushort UD_AI_EnableIEPE = 0x10;
        public const ushort UD_AI_DisableIEPE = 0x20;
        public const ushort UD_AI_Coupling_AC = 0x40;
        public const ushort UD_AI_Coupling_None = 0x80;



        // Conversion Source
        public const ushort UD_AI_CONVSRC_INT = 0x01;
        public const ushort UD_AI_CONVSRC_EXT = 0x02;


        // wTrigCtrl in UD_AI_Trigger_Config()

        // Trigger Source (bit9:0)
        public const ushort UD_AI_TRGSRC_AI0 = 0x0200;
        public const ushort UD_AI_TRGSRC_AI1 = 0x0201;
        public const ushort UD_AI_TRGSRC_AI2 = 0x0202;
        public const ushort UD_AI_TRGSRC_AI3 = 0x0203;
        public const ushort UD_AI_TRGSRC_AI4 = 0x0204;
        public const ushort UD_AI_TRGSRC_AI5 = 0x0205;
        public const ushort UD_AI_TRGSRC_AI6 = 0x0206;
        public const ushort UD_AI_TRGSRC_AI7 = 0x0207;
        public const ushort UD_AI_TRGSRC_AI8 = 0x0208;
        public const ushort UD_AI_TRGSRC_AI9 = 0x0209;
        public const ushort UD_AI_TRGSRC_AI10 = 0x020A;
        public const ushort UD_AI_TRGSRC_AI11 = 0x020B;
        public const ushort UD_AI_TRGSRC_AI12 = 0x020C;
        public const ushort UD_AI_TRGSRC_AI13 = 0x020D;
        public const ushort UD_AI_TRGSRC_AI14 = 0x020E;
        public const ushort UD_AI_TRGSRC_AI15 = 0x020F;
        public const ushort UD_AI_TRGSRC_SOFT = 0x0380;
        public const ushort UD_AI_TRGSRC_DTRIG = 0x0388;


        // Trigger Edge (bit14)
        public const ushort UD_AI_TrigPositive = 0x4000;
        public const ushort UD_AI_TrigNegative = 0x0000;

        public const ushort UD_AI_Gate_PauseLow = 0x4000;
        public const ushort UD_AI_Gate_PauseHigh = 0x0000;

        // ReTrigger (bit13)
        public const ushort UD_AI_EnReTrigger = 0x2000; // 0x02000000
        public const ushort UD_AI_DisReTrigger = 0x0000; // 0x00000000

        // AI Trigger Mode
        public const ushort UD_AI_TRGMOD_POST = 0x0000; // 0x00000000
        public const ushort UD_AI_TRGMOD_DELAY = 0x4000; // 0x40000000
        public const ushort UD_AI_TRGMOD_PRE = 0x8000; // 0x80000000
        public const ushort UD_AI_TRGMOD_MIDDLE = 0xC000; // 0xC0000000
        public const ushort UD_AI_TRGMOD_GATED = 0x1000; // 0x10000000


        // AO Trigger Source (bit9:0)
        public const ushort UD_AO_TRGSRC_SOFT = 0x0380;
        public const ushort UD_AO_TRGSRC_DTRIG = 0x0388;

        // AO Trigger Mode
        public const ushort UD_AO_TRGMOD_POST = 0x0000;

        // AO Trigger Edge (bit14)
        public const ushort UD_AO_TrigPositive = 0x4000;
        public const ushort UD_AO_TrigNegative = 0x0000;

        // AO Conversion Source
        public const ushort UD_AO_CONVSRC_INT = 0x01;
        public const ushort UD_AO_CONVSRC_EX = 0x02;

        // DIO_Config
        public const ushort UD_DIO_DIGITAL_INPUT = 0x30;
        public const ushort UD_DIO_COUNTER_INPUT = 0x31;
        public const ushort UD_DIO_DIGITAL_OUTPUT = 0x32;
        public const ushort UD_DIO_PULSE_OUTPUT = 0x33;

        // TEDS Property IDs
        public const ushort UD_TEDS_PROPERTY_TEMPLATE = 1;
        public const ushort UD_TEDS_PROPERTY_ElecSigType = 2;
        public const ushort UD_TEDS_PROPERTY_PhysMeasType = 3;
        public const ushort UD_TEDS_PROPERTY_MinPhysVal = 4;
        public const ushort UD_TEDS_PROPERTY_MaxPhysVal = 5;
        public const ushort UD_TEDS_PROPERTY_MinElecVal = 6;
        public const ushort UD_TEDS_PROPERTY_MaxElecVal = 7;
        public const ushort UD_TEDS_PROPERTY_MapMeth = 8;
        public const ushort UD_TEDS_PROPERTY_BridgeType = 9;
        public const ushort UD_TEDS_PROPERTY_SensorImped = 10;
        public const ushort UD_TEDS_PROPERTY_RespTime = 11;
        public const ushort UD_TEDS_PROPERTY_ExciteAmplNom = 12;
        public const ushort UD_TEDS_PROPERTY_ExciteAmplMin = 13;
        public const ushort UD_TEDS_PROPERTY_ExciteAmplMax = 14;
        public const ushort UD_TEDS_PROPERTY_CalDate = 15;
        public const ushort UD_TEDS_PROPERTY_CalInitials = 16;
        public const ushort UD_TEDS_PROPERTY_CalPeriod = 17;
        public const ushort UD_TEDS_PROPERTY_MeasID = 18;

        //-------- Constants for USB-1902 --------------------

        //Input Type
        public const ushort P1902_AI_NonRef_SingEnded = 0x00;
        public const ushort P1902_AI_SingEnded = 0x01;
        public const ushort P1902_AI_PseudoDifferential = 0x02;
        public const ushort P1902_AI_Differential = 0x02;

        //Conversion Source
        public const ushort P1902_AI_CONVSRC_INT = 0x00;
        public const ushort P1902_AI_CONVSRC_EXT = 0x80;


        // wTrigCtrl in UD_AI_1902_Config()
        // Trigger Source
        public const ushort P1902_AI_TRGSRC_AI0 = 0x020;
        public const ushort P1902_AI_TRGSRC_AI1 = 0x021;
        public const ushort P1902_AI_TRGSRC_AI2 = 0x022;
        public const ushort P1902_AI_TRGSRC_AI3 = 0x023;
        public const ushort P1902_AI_TRGSRC_AI4 = 0x024;
        public const ushort P1902_AI_TRGSRC_AI5 = 0x025;
        public const ushort P1902_AI_TRGSRC_AI6 = 0x026;
        public const ushort P1902_AI_TRGSRC_AI7 = 0x027;
        public const ushort P1902_AI_TRGSRC_AI8 = 0x028;
        public const ushort P1902_AI_TRGSRC_AI9 = 0x029;
        public const ushort P1902_AI_TRGSRC_AI10 = 0x02A;
        public const ushort P1902_AI_TRGSRC_AI11 = 0x02B;
        public const ushort P1902_AI_TRGSRC_AI12 = 0x02C;
        public const ushort P1902_AI_TRGSRC_AI13 = 0x02D;
        public const ushort P1902_AI_TRGSRC_AI14 = 0x02E;
        public const ushort P1902_AI_TRGSRC_AI15 = 0x02F;
        public const ushort P1902_AI_TRGSRC_SOFT = 0x030;
        public const ushort P1902_AI_TRGSRC_DTRIG = 0x031;


        // Trigger Edge
        public const ushort P1902_AI_TrgPositive = 0x040;
        public const ushort P1902_AI_TrgNegative = 0x000;

        // Gated Trigger Level
        public const ushort P1902_AI_Gate_PauseLow = 0x000;
        public const ushort P1902_AI_Gate_PauseHigh = 0x040;

        // Trigger Mode
        public const ushort P1902_AI_TRGMOD_POST = 0x000;
        public const ushort P1902_AI_TRGMOD_GATED = 0x080;
        public const ushort P1902_AI_TRGMOD_DELAY = 0x100;

        // ReTrigger
        public const ushort P1902_AI_EnReTigger = 0x200;

        //
        // AO Constants
        //

        // Conversion Source
        public const ushort P1902_AO_CONVSRC_INT = 0x00;

        // Trigger Mode
        public const ushort P1902_AO_TRGMOD_POST = 0x00;
        public const ushort P1902_AO_TRGMOD_DELAY = 0x01;

        // Trigger Source
        public const ushort P1902_AO_TRGSRC_SOFT = 0x00;
        public const ushort P1902_AO_TRGSRC_DTRIG = 0x10;

        // Trigger Edge
        public const ushort P1902_AO_TrgPositive = 0x100;
        public const ushort P1902_AO_TrgNegative = 0x000;

        // Enable Re-Trigger
        public const ushort P1902_AO_EnReTigger = 0x200;
        // Flag for AO Waveform Seperation Interval 
        public const ushort P1902_AO_EnDelay2 = 0x400;

        //-------- Constants for USB-2401 --------------------
        // wConfigCtrl in UD_AI_2401_Config()
        // Input Type, V >=2.5V, V<2.5, Current, RTD (4 wire), RTD (3-wire), RTD (2-wire), Resistor, Thermocouple, Full-Bridge, Half-Bridge
        public const ushort P2401_Voltage_2D5V_Above = 0x00;
        public const ushort P2401_Voltage_2D5V_Below = 0x01;
        public const ushort P2401_Current = 0x02;
        public const ushort P2401_RTD_4_Wire = 0x03;
        public const ushort P2401_RTD_3_Wire = 0x04;
        public const ushort P2401_RTD_2_Wrie = 0x05;
        public const ushort P2401_Resistor = 0x06;
        public const ushort P2401_ThermoCouple = 0x07;
        public const ushort P2401_Full_Bridge = 0x08;
        public const ushort P2401_Half_Bridge = 0x09;
        public const ushort P2401_ThermoCouple_Differential = 0x0A;
        public const ushort P2401_350Ohm_Full_Bridge = 0x0B;
        public const ushort P2401_350Ohm_Half_Bridge = 0x0C;
        public const ushort P2401_120Ohm_Full_Bridge = 0x0D;
        public const ushort P2401_120Ohm_Half_Bridge = 0x0E;

        // Conversion Source 
        public const ushort P2401_AI_CONVSRC_INT = 0x00;

        // wTrigCtrl in UD_AI_2401_Config()
        // Trigger Source, bit 8:3 in AI_ACQMCR
        public const ushort P2401_AI_TRGSRC_SOFT = 0x030;

        // Trigger Mode
        public const ushort P2401_AI_TRGMOD_POST = 0x000;


        // wMAvgStageCh1 ~ wMAvgStageCh4 in UD_AI_2401_PollConfig()
        public const ushort P2401_Polling_MAvg_Disable = 0x00;
        public const ushort P2401_Polling_MAvg_2_Sampes = 0x01;
        public const ushort P2401_Polling_MAvg_4_Sampes = 0x02;
        public const ushort P2401_Polling_MAvg_8_Sampes = 0x03;
        public const ushort P2401_Polling_MAvg_16_Sampes = 0x04;

        // wEnContPolling in UD_AI_2401_PollConfig()
        public const ushort P2401_Continue_Polling_Disable = 0x00;
        public const ushort P2401_Continue_Polling_Enable = 0x01;

        // wPollSpeed in UD_AI_2401_PollConfig()
        public const ushort P2401_ADC_2000_SPS = 0x09;
        public const ushort P2401_ADC_1000_SPS = 0x08;
        public const ushort P2401_ADC_640_SPS = 0x07;
        public const ushort P2401_ADC_320_SPS = 0x06;
        public const ushort P2401_ADC_160_SPS = 0x05;
        public const ushort P2401_ADC_80_SPS = 0x04;
        public const ushort P2401_ADC_40_SPS = 0x03;
        public const ushort P2401_ADC_20_SPS = 0x02;

        // AI Select Channels
        public const ushort P2405_AI_CH_0 = 0;
        public const ushort P2405_AI_CH_1 = 1;
        public const ushort P2405_AI_CH_2 = 2;
        public const ushort P2405_AI_CH_3 = 3;

        // UD_AI_2405_Chan_Config
        // Input Coupling
        public const ushort P2405_AI_EnableIEPE = 0x0004;
        public const ushort P2405_AI_DisableIEPE = 0x0008;
        public const ushort P2405_AI_Coupling_AC = 0x0010;
        public const ushort P2405_AI_Coupling_None = 0x0020;

        // Input Type
        public const ushort P2405_AI_Differential = 0x0000;
        public const ushort P2405_AI_PseudoDifferential = 0x0040;


        // UD_AI_2405_Trig_Config()
        // Conversion Source
        public const ushort P2405_AI_CONVSRC_INT = 0x0000;
        public const ushort P2405_AI_CONVSRC_EXT = 0x0200;

        // Trigger Source
        public const ushort P2405_AI_TRGSRC_AI0 = 0x0200;
        public const ushort P2405_AI_TRGSRC_AI1 = 0x0208;
        public const ushort P2405_AI_TRGSRC_AI2 = 0x0210;
        public const ushort P2405_AI_TRGSRC_AI3 = 0x0218;
        public const ushort P2405_AI_TRGSRC_SOFT = 0x0380;
        public const ushort P2405_AI_TRGSRC_DTRIG = 0x0388; // digital-trigger

        // Trigger Edge
        public const ushort P2405_AI_TrgPositive = 0x0004;
        public const ushort P2405_AI_TrgNegative = 0x0000;

        // Gated Trigger Level  
        public const ushort P2405_AI_Gate_PauseLow = 0x0004;
        public const ushort P2405_AI_Gate_PauseHigh = 0x0000;

        // ReTrigger
        public const ushort P2405_AI_EnReTigger = 0x2000;

        // AI Trigger Mode
        public const ushort P2405_AI_TRGMOD_POST = 0x0000;
        public const ushort P2405_AI_TRGMOD_DELAY = 0x4000;
        public const ushort P2405_AI_TRGMOD_PRE = 0x8000;
        public const ushort P2405_AI_TRGMOD_MIDDLE = 0xC000;
        public const ushort P2405_AI_TRGMOD_GATED = 0x1000;

        // UD_DIO_2405_Config() 
        public const ushort P2405_DIGITAL_INPUT = 0x30;
        public const ushort P2405_COUNTER_INPUT = 0x31;
        public const ushort P2405_DIGITAL_OUTPUT = 0x32;
        public const ushort P2405_PULSE_OUTPUT = 0x33;


        //-------------------------------
        // GPIO/GPTC Configuration       
        //-------------------------------
        public const ushort IGNORE_CONFIG = 0x00;
        public const ushort GPIO_IGNORE_CONFIG = 0x00;

        public const ushort GPTC0_GPO1 = 0x01;
        public const ushort GPTC0_ENABLE = 0x01;
        public const ushort GPI0_3_GPO0_1 = 0x02;
        //    public const ushort ENC0_GPO0          = 0x04;
        public const ushort GPTC0_TC1 = 0x08;

        public const ushort GPTC2_GPO3 = 0x10;
        public const ushort GPTC1_ENABLE = 0x10;
        public const ushort GPI4_7_GPO2_3 = 0x20;
        //    public const ushort ENC1_GPO1          = 0x40;
        public const ushort GPTC2_TC3 = 0x80;

        // GPIO Port
        public const ushort GPIO_PortA = 0;
        public const ushort GPIO_PortB = 1;

        /*UD_DIO_Config for USB-101*/
        public const ushort GPO0		=		0x1100;
        public const ushort GPO1		=		0x1200;
        public const ushort GPO2		=		0x1400;
        public const ushort GPO3		=		0x1800;
        public const ushort GPI0_3		=		0x2000;

        //Counter Mode
        public const ushort SimpleGatedEventCNT = 0x01;
        public const ushort SinglePeriodMSR = 0x02;
        public const ushort SinglePulseWidthMSR = 0x03;
        public const ushort SingleGatedPulseGen = 0x04;
        public const ushort SingleTrigPulseGen = 0x05;
        public const ushort RetrigSinglePulseGen = 0x06;
        public const ushort SingleTrigContPulseGen = 0x07;
        public const ushort ContGatedPulseGen = 0x08;
        public const ushort EdgeSeparationMSR = 0x09;
        public const ushort SingleTrigContPulseGenPWM = 0x0a;
        public const ushort ContGatedPulseGenPWM = 0x0b;
        public const ushort CW_CCW_Encoder = 0x0c;
        public const ushort x1_AB_Phase_Encoder = 0x0d;
        public const ushort x2_AB_Phase_Encoder = 0x0e;
        public const ushort x4_AB_Phase_Encoder = 0x0f;
        public const ushort Phase_Z = 0x10;
        public const ushort MultipleGatedPulseGen = 0x11;

        //GPTC clock source
        public const ushort GPTC_CLK_SRC_Ext = 0x01;
        public const ushort GPTC_CLK_SRC_Int = 0x00;
        public const ushort GPTC_GATE_SRC_Ext = 0x02;
        public const ushort GPTC_GATE_SRC_Int = 0x00;
        public const ushort GPTC_UPDOWN_Ext = 0x04;
        public const ushort GPTC_UPDOWN_Int = 0x00;

        //GPTC clock polarity
        public const ushort GPTC_CLKSRC_LACTIVE = 0x01;
        public const ushort GPTC_CLKSRC_HACTIVE = 0x00;
        public const ushort GPTC_GATE_LACTIVE = 0x02;
        public const ushort GPTC_GATE_HACTIVE = 0x00;
        public const ushort GPTC_UPDOWN_LACTIVE = 0x04;
        public const ushort GPTC_UPDOWN_HACTIVE = 0x00;
        public const ushort GPTC_OUTPUT_LACTIVE = 0x08;
        public const ushort GPTC_OUTPUT_HACTIVE = 0x00;

        public const ushort IntGate = 0x0;
        public const ushort IntUpDnCTR = 0x1;
        public const ushort IntENABLE = 0x2;




        // DAQ Event type for the event message */
        public const ushort AIEnd = 0;
        public const ushort AOEnd = 0;
        public const ushort DIEnd = 0;
        public const ushort DOEnd = 0;
        public const ushort DBEvent = 1;
        public const ushort TrigEvent = 2;

        // CTR parameters
        public const ushort UD_CTR_Filter_Disable = 0;
        public const ushort UD_CTR_Filter_Enable = 1;
        public const ushort UD_CTR_Reset_Edge_Counter = 2;
        public const ushort UD_CTR_Reset_Frequency_Counter = 4;
        public const ushort UD_CTR_Polarity_Positive = 0;           //robin@20121015 add
        public const ushort UD_CTR_Polarity_Negative = 8;           //robin@20121015 add

        // Calibration 
        public const ushort Cal_Op_Offset = 0;
        public const ushort Cal_Op_Gain = 1;

        public const ushort U1902_CalSrc_REF_5V = 0;
        public const ushort U1902_CalSrc_REF_10V = 1;
        public const ushort U1902_CalSrc_REF_2V = 2;
        public const ushort U1902_CalSrc_REF_1V = 3;
        public const ushort U1902_CalSrc_REF_0_2V = 4;
        public const ushort U1902_CalSrc_AO_0 = 5;
        public const ushort U1902_CalSrc_AO_1 = 6;

        //ColdJuction
        public const ushort U2401_ColdJuction_Disable = 1000;
        public const ushort U2401_ColdJuction_Enable = 1001;
        public const ushort U2401_ColdJuction_User_define = 1002;

        #endregion

        /*----------------------------------------------------------------------------*/
        /* USB-DASK Function prototype                                               */
        /*----------------------------------------------------------------------------*/
        public static short UD_Register_Card(ushort CardType, ushort card_num)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_Register_Card(CardType, card_num);
            }
            else
            {
                return VendorImportX86.UD_Register_Card(CardType, card_num);
            }
        }

        public static  short UD_Release_Card(ushort CardNumber)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_Release_Card(CardNumber);
            }
            else
            {
                return VendorImportX86.UD_Release_Card(CardNumber);
            }

        }

        public static  short UD_Device_Scan(out ushort pModuleNum, out USBDAQ_DEVICE pAvailModules)
        {
            if(Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_Device_Scan(out pModuleNum, out  pAvailModules);
            }
            else
            {
                return VendorImportX86.UD_Device_Scan(out pModuleNum, out  pAvailModules);
            }

        }
        /*----------------------------------------------------------------------------*/
        /* AI Function */

        public static  short GetActualRate_9524(ushort CardNumber, ushort Group, double SampleRate, out double ActualRate)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.GetActualRate_9524( CardNumber,  Group,  SampleRate, out ActualRate);
            }
            else
            {
                return VendorImportX86.GetActualRate_9524( CardNumber,  Group,  SampleRate, out ActualRate);
            }

        }

        public static  short EMGShutDownControl(ushort CardNumber, byte ctrl)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.EMGShutDownControl( CardNumber,  ctrl);
            }
            else
            {
                return VendorImportX86.EMGShutDownControl( CardNumber,  ctrl);
            }
        }

        public static  short EMGShutDownStatus(ushort CardNumber, out byte sts)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.EMGShutDownStatus( CardNumber, out sts);
            }
            else
            {
                return VendorImportX86.EMGShutDownStatus( CardNumber, out sts);
            }
        }

        public static  short HotResetHoldControl(ushort CardNumber, byte enable)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.HotResetHoldControl( CardNumber,  enable);
            }
            else
            {
                return VendorImportX86.HotResetHoldControl( CardNumber,  enable);
            }
        }


        public static  short HotResetHoldStatus(ushort CardNumber, out byte sts)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.HotResetHoldStatus( CardNumber, out sts);
            }
            else
            {
                return VendorImportX86.HotResetHoldStatus( CardNumber, out sts);
            }
        }

        public static  short GetInitPattern(ushort CardNumber, byte patID, out uint pattern)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.GetInitPattern( CardNumber,  patID, out pattern);
            }
            else
            {
                return VendorImportX86.GetInitPattern( CardNumber,  patID, out pattern);
            }
        }

        public static  short SetInitPattern(ushort CardNumber, byte patID, uint pattern)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.SetInitPattern( CardNumber,  patID,  pattern);
            }
            else
            {
                return VendorImportX86.SetInitPattern( CardNumber,  patID,  pattern);
            }
        }

        public static  short IdentifyLED_Control(ushort CardNumber, byte ctrl)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.IdentifyLED_Control( CardNumber,  ctrl);
            }
            else
            {
                return VendorImportX86.IdentifyLED_Control( CardNumber,  ctrl);
            }
        }
        /*---------------------------------------------------------------------------*/

        public static  short UD_AI_1902_Config(ushort CardNumber, ushort wConfigCtrl, ushort wTrigCtrl, uint dwTrgLevel, uint wReTriggerCnt, uint dwDelayCount)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AI_1902_Config(CardNumber, wConfigCtrl,  wTrigCtrl,  dwTrgLevel,  wReTriggerCnt,  dwDelayCount);
            }
            else
            {
                return VendorImportX86.UD_AI_1902_Config(CardNumber,  wConfigCtrl,  wTrigCtrl,  dwTrgLevel,  wReTriggerCnt,  dwDelayCount);
            }
        }

        public static  short UD_AI_2401_Config(ushort CardNumber, ushort wChanCfg1, ushort wChanCfg2, ushort wChanCfg3, ushort wChanCfg4, ushort wTrigCtrl)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AI_2401_Config( CardNumber,  wChanCfg1,  wChanCfg2,  wChanCfg3,  wChanCfg4,  wTrigCtrl);
            }
            else
            {
                return VendorImportX86.UD_AI_2401_Config( CardNumber,  wChanCfg1,  wChanCfg2,  wChanCfg3,  wChanCfg4,  wTrigCtrl);
            }
        }

        public static  short UD_AI_2401_PollConfig(ushort CardNumber, ushort wPollSpeed, ushort wMAvgStageCh1, ushort wMAvgStageCh2, ushort wMAvgStageCh3, ushort wMAvgStageCh4)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AI_2401_PollConfig( CardNumber,  wPollSpeed,  wMAvgStageCh1,  wMAvgStageCh2,  wMAvgStageCh3,  wMAvgStageCh4);
            }
            else
            {
                return VendorImportX86.UD_AI_2401_PollConfig( CardNumber,  wPollSpeed,  wMAvgStageCh1,  wMAvgStageCh2,  wMAvgStageCh3,  wMAvgStageCh4);
            }
        }

        public static  short UD_AI_1902_CounterInterval(ushort CardNumber, uint ScanIntrv, uint SampIntrv)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AI_1902_CounterInterval( CardNumber,  ScanIntrv,  SampIntrv);
            }
            else
            {
                return VendorImportX86.UD_AI_1902_CounterInterval( CardNumber,  ScanIntrv,  SampIntrv);
            }
        }

        public static  short UD_AI_AsyncCheck(ushort CardNumber, out byte Stopped, out ulong AccessCnt)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AI_AsyncCheck( CardNumber, out Stopped, out AccessCnt);
            }
            else
            {
                return VendorImportX86.UD_AI_AsyncCheck( CardNumber, out Stopped, out AccessCnt);
            }
        }

        public static  short UD_AI_AsyncClear(ushort CardNumber, out ulong AccessCnt)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AI_AsyncClear( CardNumber, out AccessCnt);
            }
            else
            {
                return VendorImportX86.UD_AI_AsyncClear( CardNumber, out AccessCnt);
            }
        }

        public static  short UD_AI_AsyncDblBufferHalfReady(ushort CardNumber, out byte HalfReady, out byte StopFlag)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AI_AsyncDblBufferHalfReady( CardNumber, out HalfReady, out StopFlag);
            }
            else
            {
                return VendorImportX86.UD_AI_AsyncDblBufferHalfReady( CardNumber, out HalfReady, out StopFlag);
            }
        }

        public static  short UD_AI_AsyncDblBufferMode(ushort CardNumber, bool Enable)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AI_AsyncDblBufferMode( CardNumber,  Enable);
            }
            else
            {
                return VendorImportX86.UD_AI_AsyncDblBufferMode( CardNumber,  Enable);
            }
        }

        public static  short UD_AI_AsyncDblBufferTransfer32(ushort CardNumber, IntPtr Buffer)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AI_AsyncDblBufferTransfer32( CardNumber,  Buffer);
            }
            else
            {
                return VendorImportX86.UD_AI_AsyncDblBufferTransfer32( CardNumber,  Buffer);
            }
        }

        public static  short UD_AI_AsyncDblBufferTransfer(ushort CardNumber, IntPtr Buffer)              //robin@20111222 modify uint -> ushort     //robin@20111228 modify short[] => IntPtr
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AI_AsyncDblBufferTransfer( CardNumber,  Buffer);
            }
            else
            {
                return VendorImportX86.UD_AI_AsyncDblBufferTransfer( CardNumber,  Buffer);
            }
        }

        public static  short _AI_AsyncBufferTransfer(ushort CardNumber, out ulong count, IntPtr Buffer)    //robin@20120111 add
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64._AI_AsyncBufferTransfer( CardNumber, out count,  Buffer);
            }
            else
            {
                return VendorImportX86._AI_AsyncBufferTransfer( CardNumber, out count,  Buffer);
            }
        }


        public static  short UD_AI_AsyncDblBufferOverrun(ushort CardNumber, ushort op, out ushort overrunFlag)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AI_AsyncDblBufferOverrun( CardNumber,  op, out overrunFlag);
            }
            else
            {
                return VendorImportX86.UD_AI_AsyncDblBufferOverrun( CardNumber,  op, out overrunFlag);
            }
        }

        public static  short UD_AI_AsyncDblBufferOverrun(ushort CardNumber, ushort op, IntPtr overrunFlag)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AI_AsyncDblBufferOverrun( CardNumber,  op,  overrunFlag);
            }
            else
            {
                return VendorImportX86.UD_AI_AsyncDblBufferOverrun( CardNumber,  op,  overrunFlag);
            }
        }

        public static  short UD_AI_AsyncDblBufferHandled(ushort CardNumber)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AI_AsyncDblBufferHandled( CardNumber);
            }
            else
            {
                return VendorImportX86.UD_AI_AsyncDblBufferHandled( CardNumber);
            }
        }

        public static  short UD_AI_AsyncDblBufferToFile(ushort CardNumber)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AI_AsyncDblBufferToFile( CardNumber);
            }
            else
            {
                return VendorImportX86.UD_AI_AsyncDblBufferToFile( CardNumber);
            }
        }

        public static  short UD_AI_ContReadChannel(ushort CardNumber, ushort Channel, ushort AdRange, IntPtr Buffer, uint ReadCount, double SampleRate, ushort SyncMode)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AI_ContReadChannel( CardNumber,  Channel,  AdRange,  Buffer,  ReadCount,  SampleRate,  SyncMode);
            }
            else
            {
                return VendorImportX86.UD_AI_ContReadChannel( CardNumber,  Channel,  AdRange,  Buffer,  ReadCount,  SampleRate,  SyncMode);
            }
        }

        public static short UD_AI_ContReadMultiChannels(ushort CardNumber, ushort NumChans, ushort[] Chans, ushort[] AdRanges, IntPtr Buffer, uint ReadCount, double SampleRate, ushort SyncMode)     //robin@20111006 modify uint -> ushort (buffer)      //robin@20111228 modify ushort[] => IntPtr
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AI_ContReadMultiChannels(CardNumber, NumChans, Chans, AdRanges, Buffer, ReadCount, SampleRate, SyncMode);
            }
            else
            {
                return VendorImportX86.UD_AI_ContReadMultiChannels(CardNumber, NumChans, Chans, AdRanges, Buffer, ReadCount, SampleRate, SyncMode);
            }
        }

        public static  short UD_AI_2401_Scale32(ushort CardNumber, ushort AdRange, ushort inType, uint reading, out double voltage)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AI_2401_Scale32( CardNumber,  AdRange,  inType,  reading, out voltage);
            }
            else
            {
                return VendorImportX86.UD_AI_2401_Scale32( CardNumber,  AdRange,  inType,  reading, out voltage);
            }
        }

        public static  short UD_AI_2401_ContVScale32(ushort CardNumber, ushort AdRange, ushort inType, uint[] readingArray, double[] voltageArray, int count)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AI_2401_ContVScale32(CardNumber, AdRange, inType, readingArray, voltageArray, count);
            }

            else
            {
                return VendorImportX86.UD_AI_2401_ContVScale32(CardNumber, AdRange, inType, readingArray, voltageArray, count);
            }
        }

        public static  short UD_AI_ContReadChannelToFile(ushort CardNumber, ushort Channel, ushort AdRange, string FileName, uint ReadCount, double SampleRate, ushort SyncMode)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AI_ContReadChannelToFile( CardNumber,  Channel,  AdRange,  FileName,  ReadCount,  SampleRate,  SyncMode);
            }
            else
            {
                return VendorImportX86.UD_AI_ContReadChannelToFile( CardNumber,  Channel,  AdRange,  FileName,  ReadCount,  SampleRate,  SyncMode);
            }
        }

        public static  short UD_AI_ContReadMultiChannelsToFile(ushort CardNumber, ushort NumChans, ushort[] Chans, ushort[] AdRanges, string FileName, uint ReadCount, double SampleRate, ushort SyncMode)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AI_ContReadMultiChannelsToFile(CardNumber, NumChans, Chans, AdRanges, FileName, ReadCount, SampleRate, SyncMode);
            }
            else
            {
                return VendorImportX86.UD_AI_ContReadMultiChannelsToFile(CardNumber, NumChans, Chans, AdRanges, FileName, ReadCount, SampleRate, SyncMode);
            }
        }

        public static  short UD_AI_EventCallBack(ushort CardNumber, ushort mode, ushort EventType, MulticastDelegate callbackAddr)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AI_EventCallBack( CardNumber,  mode,  EventType,  callbackAddr);
            }
            else
            {
                return VendorImportX86.UD_AI_EventCallBack( CardNumber,  mode,  EventType,  callbackAddr);
            }
        }

        public static  short UD_AI_InitialMemoryAllocated(ushort CardNumber, out uint MemSize)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AI_InitialMemoryAllocated( CardNumber, out MemSize);
            }
            else
            {
                return VendorImportX86.UD_AI_InitialMemoryAllocated( CardNumber, out MemSize);
            }
        }

        public static  short UD_AI_ReadChannel(ushort CardNumber, ushort Channel, ushort AdRange, out ushort Value)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AI_ReadChannel( CardNumber,  Channel,  AdRange, out Value);
            }
            else
            {
                return VendorImportX86.UD_AI_ReadChannel( CardNumber,  Channel,  AdRange, out Value);
            }
        }

        public static  short UD_AI_ReadMultiChannels(ushort CardNumber, ushort NumChans, ushort[] Chans, ushort[] AdRanges, ushort[] Buffer)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AI_ReadMultiChannels( CardNumber,  NumChans,  Chans,  AdRanges,  Buffer);
            }
            else
            {
                return VendorImportX86.UD_AI_ReadMultiChannels( CardNumber,  NumChans,  Chans,  AdRanges,  Buffer);
            }
        }

        public static  short UD_AI_ReadMultiChannels(ushort CardNumber, ushort NumChans, ushort[] Chans, ushort[] AdRanges, IntPtr Buffer)            //robin@20120323 add
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AI_ReadMultiChannels( CardNumber,  NumChans,  Chans,  AdRanges,  Buffer);
            }
            else
            {
                return VendorImportX86.UD_AI_ReadMultiChannels( CardNumber,  NumChans,  Chans,  AdRanges,  Buffer);
            }
        }

        public static  short UD_AI_VReadChannel(ushort CardNumber, ushort Channel, ushort AdRange, out double voltage)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AI_VReadChannel( CardNumber,  Channel,  AdRange, out voltage);
            }
            else
            {
                return VendorImportX86.UD_AI_VReadChannel( CardNumber,  Channel,  AdRange, out voltage);
            }
        }

        public static  short UD_AI_VoltScale(ushort CardNumber, ushort AdRange, ushort reading, out double voltage)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AI_VoltScale( CardNumber,  AdRange,  reading, out voltage);
            }
            else
            {
                return VendorImportX86.UD_AI_VoltScale( CardNumber,  AdRange,  reading, out voltage);
            }
        }

        public static  short UD_AIVoltScale(ushort CardType, ushort AdRange, short reading, out double voltage)      //robin@20111004 add
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AIVoltScale( CardType,  AdRange,  reading, out voltage);
            }
            else
            {
                return VendorImportX86.UD_AIVoltScale( CardType,  AdRange,  reading, out voltage);
            }
        }

        public static  short UD_AI_ContVScale(ushort CardNumber, ushort adRange, ushort[] readingArray, double[] voltageArray, int count)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AI_ContVScale( CardNumber,  adRange,  readingArray,  voltageArray,  count);
            }
            else
            {
                return VendorImportX86.UD_AI_ContVScale( CardNumber,  adRange,  readingArray,  voltageArray,  count);
            }
        }

        public static  short UD_AI_SetTimeOut(ushort CardNumber, uint TimeOut)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AI_SetTimeOut( CardNumber,  TimeOut);
            }
            else
            {
                return VendorImportX86.UD_AI_SetTimeOut( CardNumber,  TimeOut);
            }
        }

        public static  short UD_AI_AsyncReTrigNextReady(ushort CardNumber, out byte Ready, out byte StopFlag, out uint RdyTrigCnt)   //robin@20111225 modify
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AI_AsyncReTrigNextReady( CardNumber, out Ready, out StopFlag, out RdyTrigCnt);
            }
            else
            {
                return VendorImportX86.UD_AI_AsyncReTrigNextReady( CardNumber, out Ready, out StopFlag, out RdyTrigCnt);
            }
        }
        // 2012Oct18, Jeff added for USB-2405

        public static  short UD_AI_2405_Chan_Config(ushort CardNumber, ushort wChanCfg1, ushort wChanCfg2, ushort wChanCfg3, ushort wChanCfg4)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AI_2405_Chan_Config( CardNumber,  wChanCfg1,  wChanCfg2,  wChanCfg3,  wChanCfg4);
            }
            else
            {
                return VendorImportX86.UD_AI_2405_Chan_Config( CardNumber,  wChanCfg1,  wChanCfg2,  wChanCfg3,  wChanCfg4);
            }
        }

        public static  short UD_AI_2405_Trig_Config(ushort CardNumber, ushort wConvSrc, ushort wTrigMode, ushort wTrigCtrl, uint wReTrigCnt, uint dwDLY1Cnt, uint dwDLY2Cnt, uint dwTrgLevel)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AI_2405_Trig_Config( CardNumber,  wConvSrc,  wTrigMode,  wTrigCtrl,  wReTrigCnt,  dwDLY1Cnt,  dwDLY2Cnt,  dwTrgLevel);
            }
            else
            {
                return VendorImportX86.UD_AI_2405_Trig_Config( CardNumber,  wConvSrc,  wTrigMode,  wTrigCtrl,  wReTrigCnt,  dwDLY1Cnt,  dwDLY2Cnt,  dwTrgLevel);
            }
        }

        public static  short UD_AI_Channel_Config(ushort CardNumber, ushort wChanCfg1, ushort wChanCfg2, ushort wChanCfg3, ushort wChanCfg4)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AI_Channel_Config( CardNumber,  wChanCfg1,  wChanCfg2,  wChanCfg3,  wChanCfg4);
            }
            else
            {
                return VendorImportX86.UD_AI_Channel_Config( CardNumber,  wChanCfg1,  wChanCfg2,  wChanCfg3,  wChanCfg4);
            }
        }

        public static  short UD_AI_Trigger_Config(ushort CardNumber, ushort wConvSrc, ushort wTrigMode, ushort wTrigCtrl, uint wReTrigCnt, uint dwDLY1Cnt, uint dwDLY2Cnt, uint dwTrgLevel)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AI_Trigger_Config( CardNumber,  wConvSrc,  wTrigMode,  wTrigCtrl,  wReTrigCnt,  dwDLY1Cnt,  dwDLY2Cnt,  dwTrgLevel);
            }
            else
            {
                return VendorImportX86.UD_AI_Trigger_Config( CardNumber,  wConvSrc,  wTrigMode,  wTrigCtrl,  wReTrigCnt,  dwDLY1Cnt,  dwDLY2Cnt,  dwTrgLevel);
            }
        }

        public static  short UD_AI_VoltScale32(ushort CardNumber, ushort AdRange, ushort inType, uint reading, out double voltage)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AI_VoltScale32( CardNumber,  AdRange,  inType,  reading, out voltage);
            }
            else
            {
                return VendorImportX86.UD_AI_VoltScale32( CardNumber,  AdRange,  inType,  reading, out voltage);
            }
        }

        public static  short UD_AI_ContVScale32(ushort CardNumber, ushort AdRange, ushort inType, uint[] readingArray, double[] voltageArray, int count)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AI_ContVScale32( CardNumber,  AdRange,  inType,  readingArray,  voltageArray,  count);
            }
            else
            {
                return VendorImportX86.UD_AI_ContVScale32( CardNumber,  AdRange,  inType,  readingArray,  voltageArray,  count);
            }
        }

        public static  short UD_AI_DDS_ActualRate_Get(ushort CardNumber, double SampleRate, out double ActualRate)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AI_DDS_ActualRate_Get( CardNumber,  SampleRate, out ActualRate);
            }
            else
            {
                return VendorImportX86.UD_AI_DDS_ActualRate_Get( CardNumber,  SampleRate, out ActualRate);
            }
        }
        /*---------------------------------------------------------------------------*/

        public static  short UD_AO_1902_Config(ushort CardNumber, ushort ConfigCtrl, ushort TrigCtrl, uint ReTrgCnt, uint DLY1Cnt, uint DLY2Cnt)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AO_1902_Config( CardNumber,  ConfigCtrl,  TrigCtrl,  ReTrgCnt,  DLY1Cnt,  DLY2Cnt);
            }
            else
            {
                return VendorImportX86.UD_AO_1902_Config( CardNumber,  ConfigCtrl,  TrigCtrl,  ReTrgCnt,  DLY1Cnt,  DLY2Cnt);
            }
        }

        public static  short UD_AO_AsyncCheck(ushort CardNumber, out byte Stopped, out uint AccessCnt)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AO_AsyncCheck( CardNumber, out Stopped, out AccessCnt);
            }
            else
            {
                return VendorImportX86.UD_AO_AsyncCheck( CardNumber, out Stopped, out AccessCnt);
            }
        }


        //public static extern short UD_AO_AsyncClear(ushort CardNumber, out uint AccessCnt);
        public static  short UD_AO_AsyncClear(ushort CardNumber, out uint AccessCnt, ushort stop_mode)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AO_AsyncClear( CardNumber, out AccessCnt,  stop_mode);
            }
            else
            {
                return VendorImportX86.UD_AO_AsyncClear( CardNumber, out AccessCnt,  stop_mode);
            }
        }

        public static  short UD_AO_AsyncDblBufferMode(ushort CardNumber, bool Enable, bool bEnFifoMode)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AO_AsyncDblBufferMode( CardNumber,  Enable,  bEnFifoMode);
            }
            else
            {
                return VendorImportX86.UD_AO_AsyncDblBufferMode( CardNumber,  Enable,  bEnFifoMode);
            }
        }

        public static  short UD_AO_AsyncDblBufferHalfReady(ushort CardNumber, out byte bHalfReady)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AO_AsyncDblBufferHalfReady( CardNumber, out bHalfReady);
            }
            else
            {
                return VendorImportX86.UD_AO_AsyncDblBufferHalfReady( CardNumber, out bHalfReady);
            }
        }

        public static  short UD_AO_AsyncDblBufferTransfer(ushort CardNumber, ushort wbufferId, ushort[] buffer)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AO_AsyncDblBufferTransfer( CardNumber,  wbufferId,  buffer);
            }
            else
            {
                return VendorImportX86.UD_AO_AsyncDblBufferTransfer( CardNumber,  wbufferId,  buffer);
            }
        }

        public static  short UD_AO_ContBufferCompose(ushort CardNumber, ushort TotalChnCount, ushort ChnNum, uint UpdateCount, uint[] ConBuffer, uint[] Buffer)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AO_ContBufferCompose( CardNumber,  TotalChnCount,  ChnNum,  UpdateCount,  ConBuffer,  Buffer);
            }
            else
            {
                return VendorImportX86.UD_AO_ContBufferCompose( CardNumber,  TotalChnCount,  ChnNum,  UpdateCount,  ConBuffer,  Buffer);
            }
        }

        public static  short UD_AO_ContWriteChannel(ushort CardNumber, ushort Channel, ushort[] AOBuffer, uint WriteCount, uint Iterations, uint CHUI, ushort finite, ushort SyncMode)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AO_ContWriteChannel( CardNumber,  Channel,  AOBuffer,  WriteCount,  Iterations,  CHUI,  finite,  SyncMode);
            }
            else
            {
                return VendorImportX86.UD_AO_ContWriteChannel( CardNumber,  Channel,  AOBuffer,  WriteCount,  Iterations,  CHUI,  finite,  SyncMode);
            }
        }

        public static  short UD_AO_ContWriteMultiChannels(ushort CardNumber, ushort NumChans, ushort[] Chans, short[] AOBuffer, uint WriteCount, uint Iterations, uint CHUI, ushort finite, ushort SyncMode)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AO_ContWriteMultiChannels( CardNumber,  NumChans,  Chans,  AOBuffer,  WriteCount,  Iterations,  CHUI,  finite,  SyncMode);
            }
            else
            {
                return VendorImportX86.UD_AO_ContWriteMultiChannels( CardNumber,  NumChans,  Chans,  AOBuffer,  WriteCount,  Iterations,  CHUI,  finite,  SyncMode);
            }
        }

        public static  short UD_AO_InitialMemoryAllocated(ushort CardNumber, out uint MemSize)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AO_InitialMemoryAllocated( CardNumber, out MemSize);
            }
            else
            {
                return VendorImportX86.UD_AO_InitialMemoryAllocated( CardNumber, out MemSize);
            }
        }

        public static  short UD_AO_SetTimeOut(ushort CardNumber, uint TimeOut)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AO_SetTimeOut( CardNumber,  TimeOut);
            }
            else
            {
                return VendorImportX86.UD_AO_SetTimeOut( CardNumber,  TimeOut);
            }
        }

        public static  short UD_AO_SimuVWriteChannel(ushort CardNumber, ushort Group, double[] VBuffer)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AO_SimuVWriteChannel( CardNumber,  Group,  VBuffer);
            }
            else
            {
                return VendorImportX86.UD_AO_SimuVWriteChannel( CardNumber,  Group,  VBuffer);
            }
        }

        public static  short UD_AO_SimuWriteChannel(ushort CardNumber, ushort Group, short[] Buffer)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AO_SimuWriteChannel( CardNumber,  Group,  Buffer);
            }
            else
            {
                return VendorImportX86.UD_AO_SimuWriteChannel( CardNumber,  Group,  Buffer);
            }
        }

        public static  short UD_AO_VWriteChannel(ushort CardNumber, ushort Channel, double Voltage)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AO_VWriteChannel( CardNumber,  Channel,  Voltage);
            }
            else
            {
                return VendorImportX86.UD_AO_VWriteChannel( CardNumber,  Channel,  Voltage);
            }
        }

        public static  short UD_AO_WriteChannel(ushort CardNumber, ushort Channel, short Value)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AO_WriteChannel( CardNumber,  Channel,  Value);
            }
            else
            {
                return VendorImportX86.UD_AO_WriteChannel( CardNumber,  Channel,  Value);
            }
        }

        public static  short UD_AO_EventCallBack(ushort CardNumber, ushort mode, ushort EventType, MulticastDelegate callbackAddr)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AO_EventCallBack( CardNumber,  mode,  EventType,  callbackAddr);
            }
            else
            {
                return VendorImportX86.UD_AO_EventCallBack( CardNumber,  mode,  EventType,  callbackAddr);
            }
        }

        public static  short UD_AO_VoltScale(ushort CardNumber, ushort Channel, double Voltage, out short binValue)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AO_VoltScale( CardNumber,  Channel,  Voltage, out binValue);
            }
            else
            {
                return VendorImportX86.UD_AO_VoltScale( CardNumber,  Channel,  Voltage, out binValue);
            }
        }
        /*---------------------------------------------------------------------------*/

        public static  short UD_DIO_1902_Config(ushort CardNumber, ushort wPart1Cfg, ushort wPart2Cfg)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_DIO_1902_Config( CardNumber,  wPart1Cfg,  wPart2Cfg);
            }
            else
            {
                return VendorImportX86.UD_DIO_1902_Config( CardNumber,  wPart1Cfg,  wPart2Cfg);
            }
        }

        public static  short UD_DIO_2401_Config(ushort wCardNumber, ushort wPart1Cfg)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_DIO_2401_Config( wCardNumber,  wPart1Cfg);
            }
            else
            {
                return VendorImportX86.UD_DIO_2401_Config( wCardNumber,  wPart1Cfg);
            }
        }
        // 2012Oct18, Jeff added for USB-2405

        public static  short UD_DIO_2405_Config(ushort wCardNumber, ushort wPart1Cfg, ushort wPart2Cfg)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_DIO_2405_Config( wCardNumber,  wPart1Cfg,  wPart2Cfg);
            }
            else
            {
                return VendorImportX86.UD_DIO_2405_Config( wCardNumber,  wPart1Cfg,  wPart2Cfg);
            }
        }

        public static  short UD_DIO_Config(ushort wCardNumber, ushort wPart1Cfg, ushort wPart2Cfg)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_DIO_Config( wCardNumber,  wPart1Cfg,  wPart2Cfg);
            }
            else
            {
                return VendorImportX86.UD_DIO_Config( wCardNumber,  wPart1Cfg,  wPart2Cfg);
            }
        }
        /*---------------------------------------------------------------------------*/

        public static  short UD_DI_ReadLine(ushort CardNumber, ushort Port, ushort Line, out ushort State)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_DI_ReadLine( CardNumber,  Port,  Line, out State);
            }
            else
            {
                return VendorImportX86.UD_DI_ReadLine( CardNumber,  Port,  Line, out State);
            }
        }

        public static  short UD_DI_ReadPort(ushort CardNumber, ushort Port, out uint Value)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_DI_ReadPort( CardNumber,  Port, out Value);
            }
            else
            {
                return VendorImportX86.UD_DI_ReadPort( CardNumber,  Port, out Value);
            }
        }
        /*---------------------------------------------------------------------------*/

        public static  short UD_DO_ReadLine(ushort CardNumber, ushort Port, ushort Line, out ushort Value)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_DO_ReadLine( CardNumber,  Port,  Line, out Value);
            }
            else
            {
                return VendorImportX86.UD_DO_ReadLine( CardNumber,  Port,  Line, out Value);
            }
        }

        public static  short UD_DO_ReadPort(ushort CardNumber, ushort Port, out uint Value)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_DO_ReadPort( CardNumber,  Port, out Value);
            }
            else
            {
                return VendorImportX86.UD_DO_ReadPort( CardNumber,  Port, out Value);
            }
        }

        public static  short UD_DO_WriteLine(ushort CardNumber, ushort Port, ushort Line, ushort Value)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_DO_WriteLine( CardNumber,  Port,  Line,  Value);
            }
            else
            {
                return VendorImportX86.UD_DO_WriteLine( CardNumber,  Port,  Line,  Value);
            }
        }

        public static  short UD_DO_WritePort(ushort CardNumber, ushort Port, uint Value)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_DO_WritePort( CardNumber,  Port,  Value);
            }
            else
            {
                return VendorImportX86.UD_DO_WritePort( CardNumber,  Port,  Value);
            }
        }
        /*---------------------------------------------------------------------------*/

        public static  short UD_GPTC_Clear(ushort CardNumber, ushort GCtr)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_GPTC_Clear( CardNumber,  GCtr);
            }
            else
            {
                return VendorImportX86.UD_GPTC_Clear( CardNumber,  GCtr);
            }
        }


        public static  short UD_GPTC_Control(ushort CardNumber, ushort GCtr, ushort ParamID, ushort Value)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_GPTC_Control( CardNumber,  GCtr,  ParamID,  Value);
            }
            else
            {
                return VendorImportX86.UD_GPTC_Control( CardNumber,  GCtr,  ParamID,  Value);
            }
        }

        public static  short UD_GPTC_Read(ushort CardNumber, ushort GCtr, out uint Value)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_GPTC_Read( CardNumber,  GCtr, out Value);
            }
            else
            {
                return VendorImportX86.UD_GPTC_Read( CardNumber,  GCtr, out Value);
            }
        }

        public static  short UD_GPTC_Setup(ushort CardNumber, ushort GCtr, ushort Mode, ushort SrcCtrl, ushort PolCtrl, uint LReg1_Val, uint LReg2_Val, uint PulseCount)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_GPTC_Setup( CardNumber,  GCtr,  Mode,  SrcCtrl,  PolCtrl,  LReg1_Val,  LReg2_Val,  PulseCount);
            }
            else
            {
                return VendorImportX86.UD_GPTC_Setup( CardNumber,  GCtr,  Mode,  SrcCtrl,  PolCtrl,  LReg1_Val,  LReg2_Val,  PulseCount);
            }
        }

        public static  short UD_GPTC_Setup_N(ushort CardNumber, ushort GCtr, ushort Mode, ushort SrcCtrl, ushort PolCtrl, uint LReg1_Val, uint LReg2_Val, uint PulseCount)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_GPTC_Setup_N( CardNumber,  GCtr,  Mode,  SrcCtrl,  PolCtrl,  LReg1_Val,  LReg2_Val,  PulseCount);
            }
            else
            {
                return VendorImportX86.UD_GPTC_Setup_N( CardNumber,  GCtr,  Mode,  SrcCtrl,  PolCtrl,  LReg1_Val,  LReg2_Val,  PulseCount);
            }
        }

        public static  short UD_GPTC_Status(ushort CardNumber, ushort GCtr, out ushort Value)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_GPTC_Status( CardNumber,  GCtr, out Value);
            }
            else
            {
                return VendorImportX86.UD_GPTC_Status( CardNumber,  GCtr, out Value);
            }
        }
        /*---------------------------------------------------------------------------*/

        public static  short UD_AI_GetEvent(ushort CardNumber, out IntPtr hEvent)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AI_GetEvent( CardNumber, out hEvent);
            }
            else
            {
                return VendorImportX86.UD_AI_GetEvent( CardNumber, out hEvent);
            }
        }

        public static  short UD_AO_GetEvent(ushort CardNumber, out IntPtr hEvent)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AO_GetEvent( CardNumber, out hEvent);
            }
            else
            {
                return VendorImportX86.UD_AO_GetEvent( CardNumber, out hEvent);
            }
        }

        public static  short UD_AI_GetView(ushort CardNumber, out uint View)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AI_GetView( CardNumber, out View);
            }
            else
            {
                return VendorImportX86.UD_AI_GetView( CardNumber, out View);
            }
        }

        public static  short UD_AO_GetView(ushort CardNumber, out uint View)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AO_GetView( CardNumber, out View);
            }
            else
            {
                return VendorImportX86.UD_AO_GetView( CardNumber, out View);
            }
        }
        /*---------------------------------------------------------------------------*/

        public static  short UD_GetActualRate(ushort CardNumber, double fSampleRate, out double fActualRate)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_GetActualRate( CardNumber,  fSampleRate, out fActualRate);
            }
            else
            {
                return VendorImportX86.UD_GetActualRate( CardNumber,  fSampleRate, out fActualRate);
            }
        }


        public static  short UD_GetCardIndexFromID(ushort CardNumber, out ushort cardType, out ushort cardIndex)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_GetCardIndexFromID( CardNumber, out cardType, out cardIndex);
            }
            else
            {
                return VendorImportX86.UD_GetCardIndexFromID( CardNumber, out cardType, out cardIndex);
            }
        }

        public static  short UD_GetCardType(ushort CardNumber, out ushort cardType)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_GetCardType( CardNumber, out cardType);
            }
            else
            {
                return VendorImportX86.UD_GetCardType( CardNumber, out cardType);
            }
        }

        public static  short UD_IdentifyLED_Control(ushort CardNumber, byte ctrl)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_IdentifyLED_Control( CardNumber,  ctrl);
            }
            else
            {
                return VendorImportX86.UD_IdentifyLED_Control( CardNumber,  ctrl);
            }
        }
        /*---------------------------------------------------------------------------*/

        public static  short UD_GetFPGAVersion(ushort CardNumber, out uint pdwFPGAVersion)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_GetFPGAVersion( CardNumber, out pdwFPGAVersion);
            }
            else
            {
                return VendorImportX86.UD_GetFPGAVersion( CardNumber, out pdwFPGAVersion);
            }
        }


        public static  short UD_1902_Trimmer_Set(ushort CardNumber, byte bValue)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_1902_Trimmer_Set( CardNumber,  bValue);
            }
            else
            {
                return VendorImportX86.UD_1902_Trimmer_Set( CardNumber,  bValue);
            }
        }


        public static  short usbdaq_1902_RefVol_WriteEeprom(ushort CardNumber, double[] RefVol, ushort wTrimmer)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.usbdaq_1902_RefVol_WriteEeprom( CardNumber,  RefVol,  wTrimmer);
            }
            else
            {
                return VendorImportX86.usbdaq_1902_RefVol_WriteEeprom( CardNumber,  RefVol,  wTrimmer);
            }
        }


        public static  short usbdaq_1902_RefVol_ReadEeprom(ushort CardNumber, double[] RefVol, out ushort wTrimmer)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.usbdaq_1902_RefVol_ReadEeprom( CardNumber,  RefVol, out wTrimmer);
            }
            else
            {
                return VendorImportX86.usbdaq_1902_RefVol_ReadEeprom( CardNumber,  RefVol, out wTrimmer);
            }
        }


        public static  short usbdaq_1902_CalSrc_Switch(ushort CardNumber, ushort wOperation, ushort wCalSrc)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.usbdaq_1902_CalSrc_Switch( CardNumber,  wOperation,  wCalSrc);
            }
            else
            {
                return VendorImportX86.usbdaq_1902_CalSrc_Switch( CardNumber,  wOperation,  wCalSrc);
            }
        }


        public static  short usbdaq_1902_Calibration_All(ushort CardNumber, out ushort pCalOp, out ushort pCalSrc)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.usbdaq_1902_Calibration_All( CardNumber, out pCalOp, out pCalSrc);
            }
            else
            {
                return VendorImportX86.usbdaq_1902_Calibration_All( CardNumber, out pCalOp, out pCalSrc);
            }
        }


        public static  short usbdaq_1902_Calibration_All(ushort CardNumber, double RefVol_10V, out ushort pCalOp, out ushort pCalSrc)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.usbdaq_1902_Calibration_All( CardNumber,  RefVol_10V, out pCalOp, out pCalSrc);
            }
            else
            {
                return VendorImportX86.usbdaq_1902_Calibration_All( CardNumber,  RefVol_10V, out pCalOp, out pCalSrc);
            }
        }


        public static  short usbdaq_1902_Current_Calibration(ushort CardNumber, ushort wOperation, ushort wCalChan, double fRefCur, out uint pCalReg)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.usbdaq_1902_Current_Calibration( CardNumber,  wOperation,  wCalChan,  fRefCur, out pCalReg);
            }
            else
            {
                return VendorImportX86.usbdaq_1902_Current_Calibration( CardNumber,  wOperation,  wCalChan,  fRefCur, out pCalReg);
            }
        }


        public static  short usbdaq_1902_WriteEeprom(ushort CardNumber, ushort wTrimmer, byte[] CALdata)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.usbdaq_1902_WriteEeprom( CardNumber,  wTrimmer,  CALdata);
            }
            else
            {
                return VendorImportX86.usbdaq_1902_WriteEeprom( CardNumber,  wTrimmer,  CALdata);
            }
        }


        public static  short usbdaq_ReadPort(ushort CardNumber, ushort wPortAddr, out uint pdwData)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.usbdaq_ReadPort( CardNumber,  wPortAddr, out pdwData);
            }
            else
            {
                return VendorImportX86.usbdaq_ReadPort( CardNumber,  wPortAddr, out pdwData);
            }
        }


        public static  short UD_AI_2401_Stop_Poll(ushort wCardNumber)       //robin@20120517 add
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AI_2401_Stop_Poll( wCardNumber);
            }
            else
            {
                return VendorImportX86.UD_AI_2401_Stop_Poll( wCardNumber);
            }
        }

        public static  short UD_Read_ColdJunc_Thermo(ushort wCardNumber, out double pfValue)        //robin@20120405 add
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_Read_ColdJunc_Thermo( wCardNumber, out pfValue);
            }
            else
            {
                return VendorImportX86.UD_Read_ColdJunc_Thermo( wCardNumber, out pfValue);
            }
        }


        public static  short ADC_to_Thermo(ushort wThermoType, double fScaledADC, double fColdJuncTemp, out double pfTemp)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.ADC_to_Thermo( wThermoType,  fScaledADC,  fColdJuncTemp, out pfTemp);
            }
            else
            {
                return VendorImportX86.ADC_to_Thermo( wThermoType,  fScaledADC,  fColdJuncTemp, out pfTemp);
            }
        }
        //For USB-101

    
        //For USB-1900 Series

        public static  short UD_AI_AsyncBufferTransfer(ushort wCardNumber, IntPtr pwBuffer, uint offset, uint count)   //robin@20120611 add
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AI_AsyncBufferTransfer( wCardNumber,  pwBuffer,  offset,  count);
            }
            else
            {
                return VendorImportX86.UD_AI_AsyncBufferTransfer( wCardNumber,  pwBuffer,  offset,  count);
            }
        }


        // For USB-2405

        public static  short UD_AI_AsyncBufferTransfer32(ushort CardNumber, IntPtr pwBuffer, uint offset, uint count)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AI_AsyncBufferTransfer32( CardNumber,  pwBuffer,  offset,  count);
            }
            else
            {
                return VendorImportX86.UD_AI_AsyncBufferTransfer32( CardNumber,  pwBuffer,  offset,  count);
            }
        }


        public static  short UD_AI_AsyncBufferTransfer32(ushort CardNumber, uint[] pdwBuffer, uint offset, uint count)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AI_AsyncBufferTransfer32( CardNumber,  pdwBuffer,  offset,  count);
            }
            else
            {
                return VendorImportX86.UD_AI_AsyncBufferTransfer32( CardNumber,  pdwBuffer,  offset,  count);
            }
        }

        //For USB-7250, USB-7230

        public static  short UD_CTR_Control(ushort wCardNumber, ushort wCtr, uint dwCtrl)                  //robin@20120925 add begin
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_CTR_Control( wCardNumber,  wCtr,  dwCtrl);
            }
            else
            {
                return VendorImportX86.UD_CTR_Control( wCardNumber,  wCtr,  dwCtrl);
            }
        }

        public static  short UD_CTR_ReadFrequency(ushort wCardNumber, ushort wCtr, out double pfValue)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_CTR_ReadFrequency( wCardNumber,  wCtr, out pfValue);
            }
            else
            {
                return VendorImportX86.UD_CTR_ReadFrequency( wCardNumber,  wCtr, out pfValue);
            }
        }


        public static  short UD_CTR_ReadEdgeCounter(ushort wCardNumber, ushort wCtr, out uint dwValue)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_CTR_ReadEdgeCounter( wCardNumber,  wCtr, out dwValue);
            }
            else
            {
                return VendorImportX86.UD_CTR_ReadEdgeCounter( wCardNumber,  wCtr, out dwValue);
            }
        }


        public static  short UD_CTR_ReadRisingEdgeCounter(ushort wCardNumber, ushort wCtr, out uint dwValue)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_CTR_ReadRisingEdgeCounter( wCardNumber,  wCtr, out dwValue);
            }
            else
            {
                return VendorImportX86.UD_CTR_ReadRisingEdgeCounter( wCardNumber,  wCtr, out dwValue);
            }
        }


        public static  short UD_CTR_SetupMinPulseWidth(ushort wCardNumber, ushort wCtr, ushort Value)       //robin@20121016 double -> ushort
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_CTR_SetupMinPulseWidth( wCardNumber,  wCtr,  Value);
            }
            else
            {
                return VendorImportX86.UD_CTR_SetupMinPulseWidth( wCardNumber,  wCtr,  Value);
            }
        }

        public static  short UD_DI_SetupMinPulseWidth(ushort wCardNumber, ushort Value)                     //robin@20121016 double -> ushort
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_DI_SetupMinPulseWidth( wCardNumber,  Value);
            }
            else
            {
                return VendorImportX86.UD_DI_SetupMinPulseWidth( wCardNumber,  Value);
            }
        }

        public static  short UD_DI_Control(ushort wCardNumber, ushort wPort, uint dwCtrl)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_DI_Control( wCardNumber,  wPort,  dwCtrl);
            }
            else
            {
                return VendorImportX86.UD_DI_Control( wCardNumber,  wPort,  dwCtrl);
            }
        }


        public static  short UD_DI_SetCOSInterrupt32(ushort wCardNumber, ushort wPort, uint dwCtrl, out IntPtr hEvent, bool ManualReset)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_DI_SetCOSInterrupt32( wCardNumber,  wPort,  dwCtrl, out hEvent,  ManualReset);
            }
            else
            {
                return VendorImportX86.UD_DI_SetCOSInterrupt32( wCardNumber,  wPort,  dwCtrl, out hEvent,  ManualReset);
            }
        }


        public static  short UD_DI_GetCOSLatchData32(ushort wCardNumber, ushort wPort, out uint pwCosLData)       //robin@20121001 add
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_DI_GetCOSLatchData32( wCardNumber,  wPort, out pwCosLData);
            }
            else
            {
                return VendorImportX86.UD_DI_GetCOSLatchData32( wCardNumber,  wPort, out pwCosLData);
            }
        }

        public static  short UD_DO_GetInitPattern(ushort wCardNumber, ushort wPort, out uint pdwPattern)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_DO_GetInitPattern( wCardNumber,  wPort, out pdwPattern);
            }
            else
            {
                return VendorImportX86.UD_DO_GetInitPattern( wCardNumber,  wPort, out pdwPattern);
            }
        }


        public static  short UD_DO_SetInitPattern(ushort wCardNumber, ushort wPort, out uint pdwPattern)           //robin@20120925 add End
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_DO_SetInitPattern( wCardNumber,  wPort, out pdwPattern);
            }
            else
            {
                return VendorImportX86.UD_DO_SetInitPattern( wCardNumber,  wPort, out pdwPattern);
            }
        }
        // override 

        public static  short UD_AI_ReadChannel(ushort CardNumber, ushort Channel, ushort AdRange, out uint Value)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AI_ReadChannel(CardNumber, Channel, AdRange, out Value);
            }
            else
            {
                return VendorImportX86.UD_AI_ReadChannel(CardNumber, Channel, AdRange, out Value);
            }  
        }


        public static  short UD_AI_ReadMultiChannels(ushort CardNumber, ushort NumChans, ushort[] Chans, ushort[] AdRanges, uint[] Buffer)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AI_ReadMultiChannels( CardNumber,  NumChans,  Chans,  AdRanges,  Buffer);
            }
            else
            {
                return VendorImportX86.UD_AI_ReadMultiChannels( CardNumber,  NumChans,  Chans,  AdRanges,  Buffer);
            }
        }


        public static  short UD_AI_AsyncCheck(ushort CardNumber, out byte Stopped, out uint AccessCnt)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AI_AsyncCheck( CardNumber, out Stopped, out AccessCnt);
            }
            else
            {
                return VendorImportX86.UD_AI_AsyncCheck( CardNumber, out Stopped, out AccessCnt);
            }
        }


        public static  short UD_AI_AsyncClear(ushort CardNumber, out uint AccessCnt)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AI_AsyncClear( CardNumber, out AccessCnt);
            }
            else
            {
                return VendorImportX86.UD_AI_AsyncClear( CardNumber, out AccessCnt);
            }
        }


        public static  short _AI_AsyncBufferTransfer(ushort CardNumber, out uint count, IntPtr Buffer)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64._AI_AsyncBufferTransfer( CardNumber, out count,  Buffer);
            }
            else
            {
                return VendorImportX86._AI_AsyncBufferTransfer( CardNumber, out count,  Buffer);
            }
        }


        public static  short UD_AI_ContReadChannel(ushort CardNumber, ushort Channel, ushort AdRange, ushort[] Buffer, uint ReadCount, double SampleRate, ushort SyncMode)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AI_ContReadChannel( CardNumber,  Channel,  AdRange,  Buffer,  ReadCount,  SampleRate,  SyncMode);
            }
            else
            {
                return VendorImportX86.UD_AI_ContReadChannel( CardNumber,  Channel,  AdRange,  Buffer,  ReadCount,  SampleRate,  SyncMode);
            }
        }


        public static  short UD_AI_ContReadChannel(ushort CardNumber, ushort Channel, ushort AdRange, uint[] Buffer, uint ReadCount, double SampleRate, ushort SyncMode)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AI_ContReadChannel( CardNumber,  Channel,  AdRange,  Buffer,  ReadCount,  SampleRate,  SyncMode);
            }
            else
            {
                return VendorImportX86.UD_AI_ContReadChannel( CardNumber,  Channel,  AdRange,  Buffer,  ReadCount,  SampleRate,  SyncMode);
            }
        }


        public static  short UD_AI_ContReadMultiChannels(ushort CardNumber, ushort NumChans, ushort[] Chans, ushort[] AdRanges, ushort[] Buffer, uint ReadCount, double SampleRate, ushort SyncMode)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AI_ContReadMultiChannels( CardNumber,  NumChans,  Chans,  AdRanges,  Buffer,  ReadCount,  SampleRate,  SyncMode);
            }
            else
            {
                return VendorImportX86.UD_AI_ContReadMultiChannels( CardNumber,  NumChans,  Chans,  AdRanges,  Buffer,  ReadCount,  SampleRate,  SyncMode);
            }
        }


        //public static  short UD_AI_ContReadMultiChannels(ushort CardNumber, ushort NumChans, ushort[] Chans, ushort[] AdRanges, uint[] Buffer, uint ReadCount, double SampleRate, ushort SyncMode)
        //{
        //    if (Environment.Is64BitProcess)
        //    {
        //        return VendorImportX64.UD_AI_ContReadMultiChannels( CardNumber,  NumChans,  Chans,  AdRanges,  Buffer,  ReadCount,  SampleRate,  SyncMode);
        //    }
        //    else
        //    {
        //        return VendorImportX86.UD_AI_ContReadMultiChannels( CardNumber,  NumChans,  Chans,  AdRanges,  Buffer,  ReadCount,  SampleRate,  SyncMode);
        //    }
        //}


        public static  short UD_DIO_INT_EventMessage(ushort wCardNumber, int mode, IntPtr evt, IntPtr windowHandle, uint message, MulticastDelegate callbackAddr)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_DIO_INT_EventMessage( wCardNumber,  mode,  evt,  windowHandle,  message,  callbackAddr);
            }
            else
            {
                return VendorImportX86.UD_DIO_INT_EventMessage( wCardNumber,  mode,  evt,  windowHandle,  message,  callbackAddr);
            }
        }


        public static  short UD_AI_AsyncDblBufferTransfer32(ushort CardNumber, uint[] Buffer)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AI_AsyncDblBufferTransfer32( CardNumber,  Buffer);
            }
            else
            {
                return VendorImportX86.UD_AI_AsyncDblBufferTransfer32( CardNumber,  Buffer);
            }
        }


        public static  short UD_AI_FIFOOverflow(ushort CardNumber, out byte Overflow)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AI_FIFOOverflow( CardNumber, out Overflow);
            }
            else
            {
                return VendorImportX86.UD_AI_FIFOOverflow( CardNumber, out Overflow);
            }
        }

        public static short UD_AO_Trigger_Config(ushort CardNumber, ushort wConvSrc, ushort wTrigMode, ushort wTrigCtrl)
        {
            if (Environment.Is64BitProcess)
            {
                return VendorImportX64.UD_AO_Trigger_Config(CardNumber, wConvSrc, wTrigMode, wTrigCtrl);
            }
            else
            {
                return VendorImportX86.UD_AO_Trigger_Config(CardNumber, wConvSrc, wTrigMode, wTrigCtrl);
            }
        }
    }

    #endregion

    #region 原厂驱动定义
    internal delegate void CallbackDelegate();

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct USBDAQ_DEVICE
    {
        [MarshalAs(UnmanagedType.U2)]
        public ushort wModuleType;
        [MarshalAs(UnmanagedType.U2)]
        public ushort wCardID;
    }

    internal enum ADDI_INFO_TYPE
    {
        //Device info
        ALL_nNumOfDevice = 0,
        ALL_nFlag = 1,
        ALL_bHaveSubType = 2,
        ALL_szDeviceName = 3,
        ALL_uDeviceStyle = 4,
        //AI channel table info
        AI_uChannelStyle = 50,
        AI_uNumOfChannels = 51,
        AI_bSyncRange = 52,
        AI_szRange = 53,
        AI_bSyncCurrentRange = 54,
        AI_szCurrentRange = 55,
        AI_bSyncTerminal = 56,
        AI_szRefGround = 57,
        AI_nResolution = 58,
        AI_nFIFOSize = 59,
        //AO channel table info
        AO_uChannelStyle = 100,
        AO_uNumOfChannels = 101,
        AO_bSyncRange = 102,
        AO_szRange = 103,
        AO_bSyncPolarity = 104,
        AO_bSyncCurrentRange = 105,
        AO_szCurrentRange = 106,
        AO_szPolarity = 107,
        AO_bSyncRefVoltage = 108,
        AO_szRefVoltage = 109,
        AO_nResolution = 110,
        AO_nFIFOSize = 111,
        //DI channel table info
        DI_uChannelStyle = 150,
        DI_uNumOfDIOChannels = 151,
        DI_lpszNoteString = 152,
        DI_dbMaxSamplingRate = 153,
        DI_dbMinSamplingRate = 154,
        DI_uNumOfPortChannels = 155,
        DI_nFIFOSize = 156,
        //DO channel table info
        DO_uChannelStyle = 200,
        DO_uNumOfDIOChannels = 201,
        DO_lpszNoteString = 202,
        DO_dbMaxUpdateRate = 203,
        DO_dbMinUpdateRate = 204,
        DO_uNumOfPortChannels = 205,
        DO_nFIFOSize = 206,
        //Timer-Counter channel table info
        TC_uChannelStyle = 250,
        TC_uNumOfChannels = 251,
        TC_szTCMode = 252,
        TC_uInterval = 253,
        TC_dbTimeBase = 254,
        TC_nInitialValue = 255,
        TC_szBinOrBcd = 256,
        TC_szUpOrDown = 257,
        TC_szClockSource = 258,
        TC_szGateSource = 259,
        TC_szUpDownSource = 260,
        TC_bDebounceSource = 261,
        TC_bD2KGptcPolarity = 262,
        //Timing table info
        AI_uTimingStyle = 300,
        AI_szClockSource = 301,
        AI_szConvertSource = 302,
        AI_dbMaxSamplingRate = 303,
        AI_dbMinSamplingRate = 304,
        AO_uTimingStyle = 350,
        AO_szClockSource = 351,
        AO_szDASource = 352,
        AO_dbMaxUpdateRate = 353,
        AO_dbMinUpdateRate = 354,
        DI_uTimingStyle = 400,
        DI_szClockSource = 401,
        DI_dbMaxTransferRate = 402,
        DI_dbMinTransferRate = 403,
        DO_uTimingStyle = 450,
        DO_szClockSource = 451,
        DO_dbMaxTransferRate = 452,
        DO_dbMinTransferRate = 453,
        //AI Trigger table info
        AI_uBasicTriggerStyle = 500,
        AI_szBasicTriggerSource = 501,
        AI_szBasicTriggerMode = 502,
        AI_szBasicDigTriggerPol = 503,
        AI_szBasicAnaTriggerPol = 504,
        AI_szBasicReTriggerMode = 505,
        AI_dbBasicTriggerVoltMax = 506,
        AI_dbBasicTriggerVoltMin = 507,
        //AO Trigger table info
        AO_uBasicTriggerStyle = 550,
        AO_szBasicTriggerSource = 551,
        AO_szBasicTriggerMode = 552,
        AO_szBasicDigTriggerPol = 553,
        AO_szBasicAnaTriggerPol = 554,
        AO_szBasicReTriggerMode = 555,
        AO_dbBasicTriggerVoltMax = 556,
        AO_dbBasicTriggerVoltMin = 557,
        //D2K AI Trigger table info
        AI_uD2KAITriggerStyle = 600,
        AI_szD2KTriggerSource = 601,
        AI_szD2KTriggerMode = 602,
        AI_dbD2KTriggerVoltMax = 603,
        AI_dbD2KTriggerVoltMin = 604,

        AI_szD2KDigTriggerPol = 605,
        AI_szD2KAnalogSource = 606,
        AI_szD2KAnalogPolarity = 607,
        AI_szD2KReTriggerMode = 608,
        AI_szD2KDelaySource = 609,
        AI_szD2KDelayCounterSource = 610,
        //D2K AO Trigger table info
        AO_uD2KAOTriggerStyle = 650,
        AO_szD2KTriggerSource = 651,
        AO_szD2KTriggerMode = 652,
        AO_dbD2KTriggerVoltMax = 653,
        AO_dbD2KTriggerVoltMin = 654,

        AO_szD2KDigTriggerPol = 655,
        AO_szDigAnalogSource = 656,
        AO_szD2KAnalogSource = 657,
        AO_szD2KAnalogPolarity = 658,
        AO_szD2KReTriggerMode = 659,
        AO_szD2KDelaySource = 660,
        AO_szD2KDelaySource2 = 661,
        AO_szD2KDelayCounterSource = 662,
        AO_szD2KBreakDelayCounterSource = 663,
    }

    internal enum USB_7250_7230_CTR              //robin@20120925 add
    {
        UD_CTR_Filter_Disable,
        UD_CTR_Filter_Enable = 1,
        UD_CTR_Reset_Rising_Edge_Counter = 2,
        UD_CTR_Reset_Frequency_Counter = 4,
        UD_CTR_Polarity_Positive = 0,           //robin@20121015 add
        UD_CTR_Polarity_Negative = 8,           //robin@20121015 add
    }
    #endregion
}
