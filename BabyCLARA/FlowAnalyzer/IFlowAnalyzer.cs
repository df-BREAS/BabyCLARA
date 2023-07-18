using System.IO;

namespace BabyCLARA.FlowAnalyzerInterface
{
    interface IFlowAnalyzer
    {
        bool IsInitialized { get; }

        float Flow { get; }

        float Pressure { get; }

        float DiffPressure { get; }

        bool Init();

        void Close();

        void StartDataCapture();

        void StopDataCapture();

        string ReadInfoString(string infoStringName);
    }
}
