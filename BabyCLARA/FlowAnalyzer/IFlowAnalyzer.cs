namespace BabyCLARA.FlowAnalyzerInterface
{
    interface IFlowAnalyzer
    {
        bool IsInitialized { get; }

        float Flow { get; }

        float Pressure { get; }

        bool Init();

        void Close();

        void StartDataCapture();

        void StopDataCapture();

        string ReadInfoString(string infoStringName);
    }
}
