namespace BabyCLARA.FlowAnalyzerInterface
{
    public class FlowAnalyzer
    {
        private IFlowAnalyzer _flowAnalyzer;

        private IFlowAnalyzer[] _supportedFlowAnalyzers = new IFlowAnalyzer[]
        {
            new ImtFlowAnalyzer(),
            new ImtCitrexFlowAnalyzer()
        };

        public static UsbId[] SupportedDevices { get; } = new UsbId[]
        {
            new UsbId(ImtFlowAnalyzer.VendorId, ImtFlowAnalyzer.ProductId),
            new UsbId(ImtCitrexFlowAnalyzer.VendorId, ImtCitrexFlowAnalyzer.ProductId)
        };

        public string InstrumentName => "Flow Analyzer";

        public bool IsInitialized => _flowAnalyzer != null && _flowAnalyzer.IsInitialized;

        public float Flow => _flowAnalyzer != null ? _flowAnalyzer.Flow : 0;

        public float Pressure => _flowAnalyzer != null ? _flowAnalyzer.Pressure : 0;

        public bool Init()
        {
            foreach (IFlowAnalyzer flowAnalyzer in _supportedFlowAnalyzers)
            {
                if (flowAnalyzer.Init())
                {
                    _flowAnalyzer = flowAnalyzer;

                    return _flowAnalyzer.IsInitialized;
                }
            }

            return false;
        }

        public void Close()
        {
            if (_flowAnalyzer != null)
            {
                _flowAnalyzer.Close();
            }
        }

        public void StartDataCapture()
        {
            if (_flowAnalyzer != null)
            {
                _flowAnalyzer.StartDataCapture();
            }
        }

        public void StopDataCapture()
        {
            if (_flowAnalyzer != null)
            {
                _flowAnalyzer.StopDataCapture();
            }
        }

        public string ReadInfoString(string infoStringName)
        {
            if (_flowAnalyzer != null)
            {
                return _flowAnalyzer.ReadInfoString(infoStringName);
            }
            else
            {
                return null;
            }
        }
    }

    public class UsbId
    {
        public UsbId(ushort vendorId, ushort productId)
        {
            VendorId = vendorId;
            ProductId = productId;
        }

        public ushort VendorId { get; private set; }

        public ushort ProductId { get; private set; }
    }
}
