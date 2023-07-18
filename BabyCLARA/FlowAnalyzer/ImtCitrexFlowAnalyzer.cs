using System;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Management;
using System.Threading;
using System.Threading.Tasks;

namespace BabyCLARA.FlowAnalyzerInterface

{
    internal class ImtCitrexFlowAnalyzer : IFlowAnalyzer
    {
        private const byte FrameEnd = 0x7E; // ASCII '~'
        private const int MaxReadTimeoutMs = 4000;

        // IMT Citrex interface
        private SerialPort _serialPort;

        private Task _dataCaptureTask;
        private CancellationTokenSource _dataCaptureCancellationTokenSource;

        public static ushort VendorId => 0xC1CA;

        public static ushort ProductId => 0xBAC1;

        public bool IsInitialized => _serialPort != null;

        public float Flow { get; private set; }

        public float Pressure { get; private set; }

        public float DiffPressure { get; private set; }

        public bool Init()
        {
            if (IsInitialized)
            {
                return true;
            }

            string portName = GetSerialPortName();
            if (!string.IsNullOrWhiteSpace(portName))
            {
                try
                {
                    _serialPort = new SerialPort(portName, 9600, Parity.None, 8, StopBits.One);
                    _serialPort.ReadBufferSize = 131072;
                    _serialPort.WriteTimeout = 3000;
                    _serialPort.ReadTimeout = 1000;
                    _serialPort.Handshake = Handshake.None;
                    _serialPort.Open();

                    // Initialize IMT Citrex flow analyzer
                    SendRequest(new DeviceConfigurationGasTypeRequest(GasType.Air));
                    SendRequest(new DeviceConfigurationVolumeStandardRequest(VolumeStandard.BTPS));

                    return true;
                }
                catch
                {
                    // Ignore all errors
                }
            }

            return false;
        }

        public void Close()
        {
            StopDataCapture();

            if (_serialPort != null)
            {
                try
                {
                    _serialPort.Close();
                    _serialPort.Dispose();
                }
                catch
                {
                    // Ignore all errors
                }
                finally
                {
                    _serialPort = null;
                }
            }
        }

        public void StartDataCapture()
        {
            if (!IsInitialized || _dataCaptureTask != null)
            {
                return;
            }

            _dataCaptureCancellationTokenSource = new CancellationTokenSource();

            _dataCaptureTask = Task.Run(() =>
            {
                while (!_dataCaptureCancellationTokenSource.IsCancellationRequested)
                {
                    SendRequest(new DeviceDetectionRequest());

                    IRemoteObject remoteObject = remoteObject = ReadRemoteObject();

                    if (remoteObject != null)
                    {
                        if (remoteObject is ValuesFastData)
                        {
                            ValuesFastData valuesFastData = remoteObject as ValuesFastData;

                            Flow = valuesFastData.Flow;
                            Pressure = valuesFastData.pChannel;
                            DiffPressure = valuesFastData.PDiff;
                        }
                        else if (remoteObject is ValuesAllData)
                        {
                            ValuesAllData valuesAllData = remoteObject as ValuesAllData;

                            Flow = valuesAllData.Flow;
                            Pressure = valuesAllData.pChannel;
                            DiffPressure = valuesAllData.PDiff;
                        }
                    }
                }
            });
        }

        public void StopDataCapture()
        {
            if (_dataCaptureTask == null)
            {
                return;
            }

            try
            {
                _dataCaptureCancellationTokenSource.Cancel();
                _dataCaptureTask.Wait();
            }
            catch
            {
            }

            _dataCaptureTask = null;
        }

        public string ReadInfoString(string infoStringName)
        {
            if (!IsInitialized || string.IsNullOrWhiteSpace(infoStringName))
            {
                return null;
            }

            string info = null;

            switch (infoStringName)
            {
                case "Model":
                    {
                        DeviceDetection deviceDetection = ReadDeviceDetection();

                        switch (deviceDetection.DeviceType)
                        {
                            case DeviceType.ImtMedicalH4:
                                info = "IMT CITREX H4";
                                break;

                            case DeviceType.FlukeVT305:
                                info = "Fluke VT305";
                                break;

                            case DeviceType.ImtMedicalH5:
                                info = "IMT CITREX H5";
                                break;

                            case DeviceType.ImtMedicalH3:
                                info = "IMT CITREX H3";
                                break;
                        }
                    }
                    break;

                case "SerialNumber":
                    {
                        DeviceDetection deviceDetection = ReadDeviceDetection();

                        info = deviceDetection.DeviceType == DeviceType.FlukeVT305 ? "BF" + deviceDetection.SerialNumber : "BB" + deviceDetection.SerialNumber;
                    }
                    break;

                case "FirmwareVersion":
                    {
                        DeviceDetection deviceDetection = ReadDeviceDetection();

                        //info = $"{deviceDetection.SoftwareVersionMajor}.{deviceDetection.SoftwareVersionMinor}.{deviceDetection.SoftwareVersionBuild}";
                    }
                    break;
            }

            return info;
        }

        private string GetSerialPortName()
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(
                    "root\\cimv2",
                    "SELECT * FROM Win32_PNPEntity"))
                {
                    foreach (ManagementObject mgtObj in searcher.Get())
                    {
                        object classGuid = mgtObj["ClassGuid"];
                        object name = mgtObj["name"];
                        object deviceId = mgtObj["DeviceID"];

                        // Serial and parallel ports have the ClassGuid {4D36E978-E325-11CE-BFC1-08002BE10318}
                        // Fluke Flow Analyzer
                        if (classGuid != null && classGuid.ToString() == "{4d36e978-e325-11ce-bfc1-08002be10318}"
                            && deviceId != null && deviceId.ToString().Contains(@"USB\VID_C1CA&PID_BAC1")
                            && name != null)
                        {
                            string deviceName = name.ToString();

                            // Isolate the "COMxx" part for use by SerialPort object
                            string serialPortName = deviceName
                                .Substring(deviceName.LastIndexOf("(COM"))
                                .Replace("(", string.Empty)
                                .Replace(")", string.Empty);

                            return serialPortName;
                        }
                    }
                }
            }
            catch
            {
                // Ignore all errors
            }

            return null;
        }

        private void SendRequest(IRemoteObjectRequest request)
        {
            _serialPort.DiscardOutBuffer();
            _serialPort.DiscardInBuffer();

            byte[] bytes = request.GetBytes();
            _serialPort.Write(bytes, 0, bytes.Length);
            bytes[0] = FrameEnd;
            _serialPort.Write(bytes, 0, 1);
        }

        private IRemoteObject ReadRemoteObject()
        {
            IRemoteObject remoteObject = null;

            // Read remote object id
            byte[] citrexData = ReadCitrexData(sizeof(uint));

            RemoteObjectId RemoteObjectId;

            using (MemoryStream stream = new MemoryStream(citrexData))
            using (BigEndianBinaryReader reader = new BigEndianBinaryReader(stream))
            {
                RemoteObjectId = (RemoteObjectId)reader.ReadUInt32();
            }

            switch (RemoteObjectId)
            {
                case RemoteObjectId.DeviceDetection:
                    // Read DeviceDetection + Frame-End byte
                    citrexData = ReadCitrexData(DeviceDetection.DataLength + 1);
                    if (citrexData[DeviceDetection.DataLength] == FrameEnd)
                    {
                        remoteObject = new DeviceDetection(citrexData);
                    }
                    break;

                case RemoteObjectId.TriggerConfiguration:
                    citrexData = ReadCitrexData(52);
                    break;

                case RemoteObjectId.DeviceConfiguration:
                    citrexData = ReadCitrexData(88);
                    break;

                case RemoteObjectId.ValuesFastData:
                    // Read ValuesFastData + Frame-End byte
                    citrexData = ReadCitrexData(ValuesFastData.DataLength + 1);
                    if (citrexData[ValuesFastData.DataLength] == FrameEnd)
                    {
                        remoteObject = new ValuesFastData(citrexData);
                    }
                    break;

                case RemoteObjectId.ValuesAllData:
                    // Read ValuesAllData + Frame-End byte
                    citrexData = ReadCitrexData(ValuesAllData.DataLength + 1);
                    if (citrexData[ValuesAllData.DataLength] == FrameEnd)
                    {
                        remoteObject = new ValuesAllData(citrexData);
                    }
                    break;
            }

            if (remoteObject == null)
            {
                try
                {
                    var stopwatch = Stopwatch.StartNew();

                    while (_serialPort.ReadByte() != FrameEnd && stopwatch.ElapsedMilliseconds < MaxReadTimeoutMs) ;
                }
                catch (Exception)
                {
                }
            }

            return remoteObject;
        }

        private byte[] ReadCitrexData(int length)
        {
            byte[] data = new byte[length];

            try
            {
                for (int i = 0; i < length; i++)
                {
                    data[i] = (byte)_serialPort.ReadByte();
                }
            }
            catch (Exception)
            {
            }

            return data;
        }

        private DeviceDetection ReadDeviceDetection()
        {
            DeviceDetection deviceDetection = null;
            IRemoteObject remoteObject;
            var stopwatch = Stopwatch.StartNew();

            while (deviceDetection == null && stopwatch.ElapsedMilliseconds < MaxReadTimeoutMs)
            {
                SendRequest(new DeviceDetectionRequest());

                while ((remoteObject = ReadRemoteObject()) != null && stopwatch.ElapsedMilliseconds < MaxReadTimeoutMs)
                {
                    if (remoteObject is DeviceDetection)
                    {
                        deviceDetection = remoteObject as DeviceDetection;

                        break;
                    }
                }
            }

            if (stopwatch.ElapsedMilliseconds >= MaxReadTimeoutMs)
            {
                throw new Exception("Failed to read data from Citrex, check license");
            }

            return deviceDetection;
        }

        private ValuesFastData ReadFastData()
        {
            ValuesFastData valuesFastData = null;
            var stopwatch = Stopwatch.StartNew();

            while (valuesFastData == null && stopwatch.ElapsedMilliseconds < MaxReadTimeoutMs)
            {
                SendRequest(new DeviceDetectionRequest());

                IRemoteObject remoteObject;
                while ((remoteObject = ReadRemoteObject()) != null && stopwatch.ElapsedMilliseconds < MaxReadTimeoutMs)
                {
                    if (remoteObject is ValuesFastData)
                    {
                        valuesFastData = remoteObject as ValuesFastData;

                        break;
                    }
                }
            }

            if (stopwatch.ElapsedMilliseconds >= MaxReadTimeoutMs)
            {
                throw new Exception("Failed to read data from Citrex, check license");
            }

            return valuesFastData;
        }

        private ValuesAllData ReadAllData()
        {
            ValuesAllData valuesAllData = null;
            var stopwatch = Stopwatch.StartNew();

            while (valuesAllData == null && stopwatch.ElapsedMilliseconds < MaxReadTimeoutMs)
            {
                SendRequest(new DeviceDetectionRequest());

                IRemoteObject remoteObject;
                while ((remoteObject = ReadRemoteObject()) != null && stopwatch.ElapsedMilliseconds < MaxReadTimeoutMs)
                {
                    if (remoteObject is ValuesAllData)
                    {
                        valuesAllData = remoteObject as ValuesAllData;

                        break;
                    }
                }
            }

            if (stopwatch.ElapsedMilliseconds >= MaxReadTimeoutMs)
            {
                throw new Exception("Failed to read data from Citrex, check license");
            }

            return valuesAllData;
        }

        private enum RemoteObjectId
        {
            DeviceDetection = 1,
            TriggerConfiguration = 4,
            DeviceConfiguration = 5,
            ValuesFastData = 6,
            ValuesAllData = 7
        }

        private enum DeviceType : byte
        {
            Invalid = 0,
            ImtMedicalH4 = 1,
            FlukeVT305 = 2,
            ImtMedicalH5 = 3,
            ImtMedicalH3 = 4
        }

        private enum GasType : byte
        {
            Air = 0,
            AirO2xMan = 1,
            AirO2xAut = 2,
            N2OxO2xMan = 3,
            N2OxO2xAut = 4,
            Heliox = 5,
            HeO2xMan = 6,
            HeO2xAut = 7,
            N2 = 8,
            CO2 = 9
        }

        private enum VolumeStandard : byte
        {
            ATP = 0,
            STP = 1,
            BTPS = 2,
            BTPD = 3,
            STD_0x1013 = 4,
            STD_20x981 = 5,
            STD_15x1013 = 6,
            STD_20x1013 = 7,
            STD_25x991 = 8,
            AP21 = 9,
            STPH = 10,
            ATPD = 11,
            ATPS = 12,
            BTPS_A = 13,
            BTPD_A = 14,
            NTPD = 15,
            NTPS = 16
        }

        interface IRemoteObjectRequest
        {
            RemoteObjectId RemoteObjectId { get; }

            byte[] GetBytes();
        }

        interface IRemoteObject
        {
            bool Decode(byte[] bytes);
        }

        private class DeviceDetectionRequest : IRemoteObjectRequest
        {
            public RemoteObjectId RemoteObjectId => RemoteObjectId.DeviceDetection;

            public byte[] GetBytes()
            {
                byte[] data = new byte[5];

                using (MemoryStream stream = new MemoryStream(data))
                using (BigEndianBinaryWriter writer = new BigEndianBinaryWriter(stream))
                {
                    writer.Write(Convert.ToUInt32(RemoteObjectId));
                    writer.Write((byte)1);
                }

                return data;
            }
        }

        private class DeviceDetection : IRemoteObject
        {
            public DeviceDetection(byte[] data)
            {
                Decode(data);
            }

            public static int DataLength => 24;

            public byte FlowLabProtocolVersion { get; private set; }

            public bool LicenseValid { get; private set; }

            public DeviceType DeviceType { get; private set; }

            public uint SerialNumber { get; private set; }

            public byte HardwareRevision { get; private set; }

            public uint SoftwareVersionMajor { get; private set; }

            public uint SoftwareVersionMinor { get; private set; }

            public uint SoftwareVersionBuild { get; private set; }

            public uint LastAdjustmentDate { get; private set; }

            public bool Decode(byte[] data)
            {
                try
                {
                    using (MemoryStream stream = new MemoryStream(data))
                    using (BigEndianBinaryReader reader = new BigEndianBinaryReader(stream))
                    {
                        FlowLabProtocolVersion = reader.ReadByte();
                        LicenseValid = reader.ReadBoolean();
                        DeviceType = (DeviceType)reader.ReadByte();
                        SerialNumber = reader.ReadUInt32();
                        HardwareRevision = reader.ReadByte();
                        SoftwareVersionMajor = reader.ReadUInt32();
                        SoftwareVersionMinor = reader.ReadUInt32();
                        SoftwareVersionBuild = reader.ReadUInt32();
                        LastAdjustmentDate = reader.ReadUInt32();
                    }

                    return true;
                }
                catch
                {
                }

                return false;
            }
        }

        private class DeviceConfigurationRequest : IRemoteObjectRequest
        {
            public RemoteObjectId RemoteObjectId => RemoteObjectId.DeviceConfiguration;

            public byte[] GetBytes()
            {
                byte[] data = new byte[8];

                using (MemoryStream stream = new MemoryStream(data))
                using (BigEndianBinaryWriter writer = new BigEndianBinaryWriter(stream))
                {
                    writer.Write(Convert.ToUInt32(RemoteObjectId));
                    writer.Write(0);
                }

                return data;
            }
        }

        private class DeviceConfigurationGasTypeRequest : IRemoteObjectRequest
        {
            public DeviceConfigurationGasTypeRequest(GasType gasType)
            {
                GasType = gasType;
            }

            public RemoteObjectId RemoteObjectId => RemoteObjectId.DeviceConfiguration;

            public GasType GasType { get; private set; }

            public byte[] GetBytes()
            {
                byte[] data = new byte[9];

                using (MemoryStream stream = new MemoryStream(data))
                using (BigEndianBinaryWriter writer = new BigEndianBinaryWriter(stream))
                {
                    writer.Write(Convert.ToUInt32(RemoteObjectId));
                    writer.Write(4010);
                    writer.Write((byte)GasType);
                }

                return data;
            }
        }

        private class DeviceConfigurationVolumeStandardRequest : IRemoteObjectRequest
        {
            public DeviceConfigurationVolumeStandardRequest(VolumeStandard volumeStandard)
            {
                VolumeStandard = volumeStandard;
            }

            public RemoteObjectId RemoteObjectId => RemoteObjectId.DeviceConfiguration;

            public VolumeStandard VolumeStandard { get; private set; }

            public byte[] GetBytes()
            {
                byte[] data = new byte[9];

                using (MemoryStream stream = new MemoryStream(data))
                using (BigEndianBinaryWriter writer = new BigEndianBinaryWriter(stream))
                {
                    writer.Write(Convert.ToUInt32(RemoteObjectId));
                    writer.Write(4011);
                    writer.Write((byte)VolumeStandard);
                }

                return data;
            }
        }

        private class ValuesFastData : IRemoteObject
        {
            public ValuesFastData(byte[] data)
            {
                Decode(data);
            }

            public static int DataLength => 24;

            public float Flow { get; private set; }

            public float RawDifferentalPressureFlow { get; private set; }

            public float PDiff { get; private set; }

            public float pChannel { get; private set; }

            public float pHigh { get; private set; }

            public float Volume { get; private set; }

            public bool Decode(byte[] data)
            {
                try
                {
                    using (MemoryStream stream = new MemoryStream(data))
                    using (BigEndianBinaryReader reader = new BigEndianBinaryReader(stream))
                    {
                        Flow = reader.ReadSingle();
                        RawDifferentalPressureFlow = reader.ReadSingle();
                        PDiff = reader.ReadSingle();
                        pChannel = reader.ReadSingle();
                        pHigh = reader.ReadSingle();
                        Volume = reader.ReadSingle();
                    }

                    return true;
                }
                catch
                {
                }

                return false;
            }
        }

        private class ValuesAllData : IRemoteObject
        {
            public ValuesAllData(byte[] data)
            {
                Decode(data);
            }

            public static int DataLength => 108;

            public float Flow { get; private set; }

            public float RawDifferentalPressureFlow { get; private set; }

            public float PDiff { get; private set; }

            public float pChannel { get; private set; }

            public float pHigh { get; private set; }

            public float Volume { get; private set; }

            public float Oxygen { get; private set; }

            public float Humidity { get; private set; }

            public float Temperature { get; private set; }

            public float PressureAmbient { get; private set; }

            public float InspTime { get; private set; }

            public float ExpTime { get; private set; }

            public float iTOE { get; private set; }

            public float BreathRate { get; private set; }

            public float Vti { get; private set; }

            public float Vte { get; private set; }

            public float V { get; private set; }

            public float Ve { get; private set; }

            public float PeakPressure { get; private set; }

            public float MeanPressure { get; private set; }

            public float Peep { get; private set; }

            public float TiTcycle { get; private set; }

            public float PeakFlowInsp { get; private set; }

            public float PeakFlowExp { get; private set; }

            public float PlateauPressure { get; private set; }

            public float Compliance { get; private set; }

            public float IPAP { get; private set; }

            public bool Decode(byte[] data)
            {
                try
                {
                    using (MemoryStream stream = new MemoryStream(data))
                    using (BigEndianBinaryReader reader = new BigEndianBinaryReader(stream))
                    {
                        Flow = reader.ReadSingle();
                        RawDifferentalPressureFlow = reader.ReadSingle();
                        PDiff = reader.ReadSingle();
                        pChannel = reader.ReadSingle();
                        pHigh = reader.ReadSingle();
                        Volume = reader.ReadSingle();
                        Oxygen = reader.ReadSingle();
                        Humidity = reader.ReadSingle();
                        Temperature = reader.ReadSingle();
                        PressureAmbient = reader.ReadSingle();
                        InspTime = reader.ReadSingle();
                        ExpTime = reader.ReadSingle();
                        iTOE = reader.ReadSingle();
                        BreathRate = reader.ReadSingle();
                        Vti = reader.ReadSingle();
                        Vte = reader.ReadSingle();
                        V = reader.ReadSingle();
                        Ve = reader.ReadSingle();
                        PeakPressure = reader.ReadSingle();
                        MeanPressure = reader.ReadSingle();
                        Peep = reader.ReadSingle();
                        TiTcycle = reader.ReadSingle();
                        PeakFlowInsp = reader.ReadSingle();
                        PeakFlowExp = reader.ReadSingle();
                        PlateauPressure = reader.ReadSingle();
                        Compliance = reader.ReadSingle();
                        IPAP = reader.ReadSingle();
                    }

                    return true;
                }
                catch
                {
                }

                return false;
            }
        }
    }
}
