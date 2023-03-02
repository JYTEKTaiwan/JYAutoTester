using System;
using System.Runtime.InteropServices;

namespace JYUSB101
{
    #region 原厂驱动定义的常量定和函数导入
    internal class VendorImportX86
    {

        public const string UDDASK_DLL_FILE_NAME = "usb-dask.dll";
        public const string UDDASK_THERMAL_DLL_FILE_NAME = "usbthermo.dll";
        /*----------------------------------------------------------------------------*/
        /* USB-DASK Function prototype                                               */
        /*----------------------------------------------------------------------------*/
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_Register_Card(ushort CardType, ushort card_num);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_Release_Card(ushort CardNumber);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_Device_Scan(out ushort pModuleNum, out USBDAQ_DEVICE pAvailModules);
        /*----------------------------------------------------------------------------*/
        /* AI Function */
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short GetActualRate_9524(ushort CardNumber, ushort Group, double SampleRate, out double ActualRate);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short EMGShutDownControl(ushort CardNumber, byte ctrl);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short EMGShutDownStatus(ushort CardNumber, out byte sts);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short HotResetHoldControl(ushort CardNumber, byte enable);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short HotResetHoldStatus(ushort CardNumber, out byte sts);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short GetInitPattern(ushort CardNumber, byte patID, out uint pattern);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short SetInitPattern(ushort CardNumber, byte patID, uint pattern);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short IdentifyLED_Control(ushort CardNumber, byte ctrl);
        /*---------------------------------------------------------------------------*/
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AI_1902_Config(ushort CardNumber, ushort wConfigCtrl, ushort wTrigCtrl, uint dwTrgLevel, uint wReTriggerCnt, uint dwDelayCount);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AI_2401_Config(ushort CardNumber, ushort wChanCfg1, ushort wChanCfg2, ushort wChanCfg3, ushort wChanCfg4, ushort wTrigCtrl);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AI_2401_PollConfig(ushort CardNumber, ushort wPollSpeed, ushort wMAvgStageCh1, ushort wMAvgStageCh2, ushort wMAvgStageCh3, ushort wMAvgStageCh4);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AI_1902_CounterInterval(ushort CardNumber, uint ScanIntrv, uint SampIntrv);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AI_AsyncCheck(ushort CardNumber, out byte Stopped, out ulong AccessCnt);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AI_AsyncClear(ushort CardNumber, out ulong AccessCnt);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AI_AsyncDblBufferHalfReady(ushort CardNumber, out byte HalfReady, out byte StopFlag);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AI_AsyncDblBufferMode(ushort CardNumber, bool Enable);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AI_AsyncDblBufferTransfer32(ushort CardNumber, IntPtr Buffer);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AI_AsyncDblBufferTransfer(ushort CardNumber, IntPtr Buffer);               //robin@20111222 modify uint -> ushort     //robin@20111228 modify short[] => IntPtr
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short _AI_AsyncBufferTransfer(ushort CardNumber, out ulong count, IntPtr Buffer);     //robin@20120111 add
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AI_AsyncDblBufferOverrun(ushort CardNumber, ushort op, out ushort overrunFlag);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AI_AsyncDblBufferOverrun(ushort CardNumber, ushort op, IntPtr overrunFlag);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AI_AsyncDblBufferHandled(ushort CardNumber);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AI_AsyncDblBufferToFile(ushort CardNumber);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AI_ContReadChannel(ushort CardNumber, ushort Channel, ushort AdRange, IntPtr Buffer, uint ReadCount, double SampleRate, ushort SyncMode);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AI_ContReadMultiChannels(ushort CardNumber, ushort NumChans, ushort[] Chans, ushort[] AdRanges, IntPtr Buffer, uint ReadCount, double SampleRate, ushort SyncMode);     //robin@20111006 modify uint -> ushort (buffer)      //robin@20111228 modify ushort[] => IntPtr
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AI_2401_Scale32(ushort CardNumber, ushort AdRange, ushort inType, uint reading, out double voltage);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AI_2401_ContVScale32(ushort CardNumber, ushort AdRange, ushort inType, uint[] readingArray, double[] voltageArray, int count);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AI_ContReadChannelToFile(ushort CardNumber, ushort Channel, ushort AdRange, string FileName, uint ReadCount, double SampleRate, ushort SyncMode);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AI_ContReadMultiChannelsToFile(ushort CardNumber, ushort NumChans, ushort[] Chans, ushort[] AdRanges, string FileName, uint ReadCount, double SampleRate, ushort SyncMode);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AI_EventCallBack(ushort CardNumber, ushort mode, ushort EventType, MulticastDelegate callbackAddr);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AI_InitialMemoryAllocated(ushort CardNumber, out uint MemSize);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AI_ReadChannel(ushort CardNumber, ushort Channel, ushort AdRange, out ushort Value);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AI_ReadMultiChannels(ushort CardNumber, ushort NumChans, ushort[] Chans, ushort[] AdRanges, ushort[] Buffer);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AI_ReadMultiChannels(ushort CardNumber, ushort NumChans, ushort[] Chans, ushort[] AdRanges, IntPtr Buffer);            //robin@20120323 add
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AI_VReadChannel(ushort CardNumber, ushort Channel, ushort AdRange, out double voltage);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AI_VoltScale(ushort CardNumber, ushort AdRange, ushort reading, out double voltage);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AIVoltScale(ushort CardType, ushort AdRange, short reading, out double voltage);      //robin@20111004 add
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AI_ContVScale(ushort CardNumber, ushort adRange, ushort[] readingArray, double[] voltageArray, int count);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AI_SetTimeOut(ushort CardNumber, uint TimeOut);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AI_AsyncReTrigNextReady(ushort CardNumber, out byte Ready, out byte StopFlag, out uint RdyTrigCnt);    //robin@20111225 modify
        // 2012Oct18, Jeff added for USB-2405
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AI_2405_Chan_Config(ushort CardNumber, ushort wChanCfg1, ushort wChanCfg2, ushort wChanCfg3, ushort wChanCfg4);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AI_2405_Trig_Config(ushort CardNumber, ushort wConvSrc, ushort wTrigMode, ushort wTrigCtrl, uint wReTrigCnt, uint dwDLY1Cnt, uint dwDLY2Cnt, uint dwTrgLevel);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AI_Channel_Config(ushort CardNumber, ushort wChanCfg1, ushort wChanCfg2, ushort wChanCfg3, ushort wChanCfg4);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AI_Trigger_Config(ushort CardNumber, ushort wConvSrc, ushort wTrigMode, ushort wTrigCtrl, uint wReTrigCnt, uint dwDLY1Cnt, uint dwDLY2Cnt, uint dwTrgLevel);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AI_VoltScale32(ushort CardNumber, ushort AdRange, ushort inType, uint reading, out double voltage);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AI_ContVScale32(ushort CardNumber, ushort AdRange, ushort inType, uint[] readingArray, double[] voltageArray, int count);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AI_DDS_ActualRate_Get(ushort CardNumber, double SampleRate, out double ActualRate);
        /*---------------------------------------------------------------------------*/
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AO_1902_Config(ushort CardNumber, ushort ConfigCtrl, ushort TrigCtrl, uint ReTrgCnt, uint DLY1Cnt, uint DLY2Cnt);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AO_AsyncCheck(ushort CardNumber, out byte Stopped, out uint AccessCnt);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        //public static extern short UD_AO_AsyncClear(ushort CardNumber, out uint AccessCnt);
        public static extern short UD_AO_AsyncClear(ushort CardNumber, out uint AccessCnt, ushort stop_mode);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AO_AsyncDblBufferMode(ushort CardNumber, bool Enable, bool bEnFifoMode);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AO_AsyncDblBufferHalfReady(ushort CardNumber, out byte bHalfReady);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AO_AsyncDblBufferTransfer(ushort CardNumber, ushort wbufferId, ushort[] buffer);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AO_ContBufferCompose(ushort CardNumber, ushort TotalChnCount, ushort ChnNum, uint UpdateCount, uint[] ConBuffer, uint[] Buffer);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AO_ContWriteChannel(ushort CardNumber, ushort Channel, ushort[] AOBuffer, uint WriteCount, uint Iterations, uint CHUI, ushort finite, ushort SyncMode);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AO_ContWriteMultiChannels(ushort CardNumber, ushort NumChans, ushort[] Chans, short[] AOBuffer, uint WriteCount, uint Iterations, uint CHUI, ushort finite, ushort SyncMode);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AO_InitialMemoryAllocated(ushort CardNumber, out uint MemSize);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AO_SetTimeOut(ushort CardNumber, uint TimeOut);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AO_SimuVWriteChannel(ushort CardNumber, ushort Group, double[] VBuffer);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AO_SimuWriteChannel(ushort CardNumber, ushort Group, short[] Buffer);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AO_VWriteChannel(ushort CardNumber, ushort Channel, double Voltage);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AO_WriteChannel(ushort CardNumber, ushort Channel, short Value);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AO_EventCallBack(ushort CardNumber, ushort mode, ushort EventType, MulticastDelegate callbackAddr);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AO_VoltScale(ushort CardNumber, ushort Channel, double Voltage, out short binValue);
        /*---------------------------------------------------------------------------*/
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_DIO_1902_Config(ushort CardNumber, ushort wPart1Cfg, ushort wPart2Cfg);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_DIO_2401_Config(ushort wCardNumber, ushort wPart1Cfg);
        // 2012Oct18, Jeff added for USB-2405
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_DIO_2405_Config(ushort wCardNumber, ushort wPart1Cfg, ushort wPart2Cfg);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_DIO_Config(ushort wCardNumber, ushort wPart1Cfg, ushort wPart2Cfg);
        /*---------------------------------------------------------------------------*/
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_DI_ReadLine(ushort CardNumber, ushort Port, ushort Line, out ushort State);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_DI_ReadPort(ushort CardNumber, ushort Port, out uint Value);
        /*---------------------------------------------------------------------------*/
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_DO_ReadLine(ushort CardNumber, ushort Port, ushort Line, out ushort Value);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_DO_ReadPort(ushort CardNumber, ushort Port, out uint Value);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_DO_WriteLine(ushort CardNumber, ushort Port, ushort Line, ushort Value);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_DO_WritePort(ushort CardNumber, ushort Port, uint Value);
        /*---------------------------------------------------------------------------*/
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_GPTC_Clear(ushort CardNumber, ushort GCtr);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_GPTC_Control(ushort CardNumber, ushort GCtr, ushort ParamID, ushort Value);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_GPTC_Read(ushort CardNumber, ushort GCtr, out uint Value);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_GPTC_Setup(ushort CardNumber, ushort GCtr, ushort Mode, ushort SrcCtrl, ushort PolCtrl, uint LReg1_Val, uint LReg2_Val, uint PulseCount);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_GPTC_Setup_N(ushort CardNumber, ushort GCtr, ushort Mode, ushort SrcCtrl, ushort PolCtrl, uint LReg1_Val, uint LReg2_Val, uint PulseCount);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_GPTC_Status(ushort CardNumber, ushort GCtr, out ushort Value);
        /*---------------------------------------------------------------------------*/
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AI_GetEvent(ushort CardNumber, out IntPtr hEvent);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AO_GetEvent(ushort CardNumber, out IntPtr hEvent);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AI_GetView(ushort CardNumber, out uint View);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AO_GetView(ushort CardNumber, out uint View);
        /*---------------------------------------------------------------------------*/
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_GetActualRate(ushort CardNumber, double fSampleRate, out double fActualRate);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_GetCardIndexFromID(ushort CardNumber, out ushort cardType, out ushort cardIndex);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_GetCardType(ushort CardNumber, out ushort cardType);
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_IdentifyLED_Control(ushort CardNumber, byte ctrl);
        /*---------------------------------------------------------------------------*/
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_GetFPGAVersion(ushort CardNumber, out uint pdwFPGAVersion);

        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_1902_Trimmer_Set(ushort CardNumber, byte bValue);

        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short usbdaq_1902_RefVol_WriteEeprom(ushort CardNumber, double[] RefVol, ushort wTrimmer);

        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short usbdaq_1902_RefVol_ReadEeprom(ushort CardNumber, double[] RefVol, out ushort wTrimmer);

        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short usbdaq_1902_CalSrc_Switch(ushort CardNumber, ushort wOperation, ushort wCalSrc);

        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short usbdaq_1902_Calibration_All(ushort CardNumber, out ushort pCalOp, out ushort pCalSrc);

        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short usbdaq_1902_Calibration_All(ushort CardNumber, double RefVol_10V, out ushort pCalOp, out ushort pCalSrc);

        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short usbdaq_1902_Current_Calibration(ushort CardNumber, ushort wOperation, ushort wCalChan, double fRefCur, out uint pCalReg);

        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short usbdaq_1902_WriteEeprom(ushort CardNumber, ushort wTrimmer, byte[] CALdata);

        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short usbdaq_ReadPort(ushort CardNumber, ushort wPortAddr, out uint pdwData);

        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AI_2401_Stop_Poll(ushort wCardNumber);       //robin@20120517 add

        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_Read_ColdJunc_Thermo(ushort wCardNumber, out double pfValue);         //robin@20120405 add

        [DllImport(UDDASK_THERMAL_DLL_FILE_NAME)]
        public static extern short ADC_to_Thermo(ushort wThermoType, double fScaledADC, double fColdJuncTemp, out double pfTemp);

        //For USB-1900 Series
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AI_AsyncBufferTransfer(ushort wCardNumber, IntPtr pwBuffer, uint offset, uint count);     //robin@20120611 add

        // For USB-2405
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AI_AsyncBufferTransfer32(ushort CardNumber, IntPtr pwBuffer, uint offset, uint count);

        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AI_AsyncBufferTransfer32(ushort CardNumber, uint[] pdwBuffer, uint offset, uint count);

        //For USB-7250, USB-7230
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_CTR_Control(ushort wCardNumber, ushort wCtr, uint dwCtrl);                  //robin@20120925 add begin

        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_CTR_ReadFrequency(ushort wCardNumber, ushort wCtr, out double pfValue);

        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_CTR_ReadEdgeCounter(ushort wCardNumber, ushort wCtr, out uint dwValue);

        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_CTR_ReadRisingEdgeCounter(ushort wCardNumber, ushort wCtr, out uint dwValue);

        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_CTR_SetupMinPulseWidth(ushort wCardNumber, ushort wCtr, ushort Value);       //robin@20121016 double -> ushort

        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_DI_SetupMinPulseWidth(ushort wCardNumber, ushort Value);                     //robin@20121016 double -> ushort

        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_DI_Control(ushort wCardNumber, ushort wPort, uint dwCtrl);

        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_DI_SetCOSInterrupt32(ushort wCardNumber, ushort wPort, uint dwCtrl, out IntPtr hEvent, bool ManualReset);

        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_DI_GetCOSLatchData32(ushort wCardNumber, ushort wPort, out uint pwCosLData);        //robin@20121001 add

        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_DO_GetInitPattern(ushort wCardNumber, ushort wPort, out uint pdwPattern);

        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_DO_SetInitPattern(ushort wCardNumber, ushort wPort, out uint pdwPattern);           //robin@20120925 add End

        // override 
        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AI_ReadChannel(ushort CardNumber, ushort Channel, ushort AdRange, out uint Value);

        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AI_ReadMultiChannels(ushort CardNumber, ushort NumChans, ushort[] Chans, ushort[] AdRanges, uint[] Buffer);

        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AI_AsyncCheck(ushort CardNumber, out byte Stopped, out uint AccessCnt);

        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AI_AsyncClear(ushort CardNumber, out uint AccessCnt);

        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short _AI_AsyncBufferTransfer(ushort CardNumber, out uint count, IntPtr Buffer);

        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AI_ContReadChannel(ushort CardNumber, ushort Channel, ushort AdRange, ushort[] Buffer, uint ReadCount, double SampleRate, ushort SyncMode);

        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AI_ContReadChannel(ushort CardNumber, ushort Channel, ushort AdRange, uint[] Buffer, uint ReadCount, double SampleRate, ushort SyncMode);

        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AI_ContReadMultiChannels(ushort CardNumber, ushort NumChans, ushort[] Chans, ushort[] AdRanges, ushort[] Buffer, uint ReadCount, double SampleRate, ushort SyncMode);

        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AI_ContReadMultiChannels(ushort CardNumber, ushort NumChans, ushort[] Chans, ushort[] AdRanges, uint[] Buffer, uint ReadCount, double SampleRate, ushort SyncMode);

        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_DIO_INT_EventMessage(ushort wCardNumber, int mode, IntPtr evt, IntPtr windowHandle, uint message, MulticastDelegate callbackAddr);

        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AI_AsyncDblBufferTransfer32(ushort CardNumber, uint[] Buffer);

        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AI_FIFOOverflow(ushort CardNumber, out byte Overflow);

        [DllImport(UDDASK_DLL_FILE_NAME)]
        public static extern short UD_AO_Trigger_Config(ushort CardNumber, ushort wConvSrc, ushort wTrigMode, ushort wTrigCtrl);
    }

    #endregion


}
