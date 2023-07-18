using imtmedical.Pf300Communication;
using imtmedical.Pf300Communication.Base;
using imtmedical.Pf300Communication.Ftdi;
using imtmedical.Pf300Communication.Pf300Data;
using imtmedical.Pf300Communication.Pf300Setting;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BabyCLARA.FlowAnalyzerInterface
{
    internal class ImtFlowAnalyzer : IFlowAnalyzer
    {
        private const int ReadTimeout = 1000; // ms

        // IMT PF-300 interface
        private Pf300Equipment _pf300;
        private Task _dataCaptureTask;
        private CancellationTokenSource _dataCaptureCancellationTokenSource;

        public static ushort VendorId => 0x0403;

        public static ushort ProductId => 0xF798;

        public bool IsInitialized => _pf300 != null;

        public float Flow { get; private set; }

        public float Pressure { get; private set; }

        public float DiffPressure { get; private set; }

        public bool Init()
        {
            if (IsInitialized)
            {
                return true;
            }

            DeviceCollectionIfc deviceCollection = new FtdiDeviceCollection();
            deviceCollection.ReadDeviceList();

            foreach (DeviceIfc device in deviceCollection.DeviceList)
            {
                if (device.Description.Contains("FlowAnalyser"))
                {
                    _pf300 = new Pf300Equipment(device);
                    _pf300.Activate();

                    _pf300.SetStandardisation(Pf300StandardisationType.BTPS);
                    _pf300.SetGas(Pf300GasType.Air);

                    break;
                }
            }

            return _pf300 != null;
        }

        public void Close()
        {
            StopDataCapture();

            if (_pf300 != null)
            {
                _pf300.Deactivate();
                _pf300 = null;
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
                // Discard first batch of data
                IList<Pf300ReceiveDataActual> receiveDataList;
                _pf300.ConsumeActualData(out receiveDataList);

                while (!_dataCaptureCancellationTokenSource.IsCancellationRequested)
                {
                    _pf300.ConsumeActualData(out receiveDataList);

                    foreach (Pf300ReceiveDataActual receivedData in receiveDataList)
                    {
                        Pressure = receivedData.PressureDiff;
                        Flow = receivedData.FlowHigh;
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
                    info = "IMT PF-300";
                    break;

                case "SerialNumber":
                    info = _pf300.GetSerialNumber();
                    break;

                case "FirmwareVersion":
                    {
                        Version ver = _pf300.GetFirmwareVersion();
                        if (ver != null)
                        {
                            info = ver.ToString();
                        }
                    }
                    break;
            }

            return info;
        }
    }
}
