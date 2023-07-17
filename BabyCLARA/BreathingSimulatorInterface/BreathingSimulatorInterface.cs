using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;
using System.Runtime.CompilerServices;

namespace BabyCLARA.ASLInterface
{
    internal static class BreathingSimulatorInterface
    {
        private const string InputPrompt = ">ASL";
        private const string ResponsePrompt = ">ASL";
        private const string ErrorPrompt = "!ASL";

        private const int ResponseReadTimeout = 45000;

        private static TcpClient _client;
        private static NetworkStream _networkStream;
        private static StreamWriter _streamWriter;
        private static StreamReader _streamReader;


        public static string Host { get; set; } = "localhost";

        public static int Port { get; set; } = 6341;

        public static bool IsConnected
        {
            get { return _client != null;}
            set {}
        }

        // Connect asynchronously to the ASL5000 via TCP 
        public static async Task<bool> Connect()
        {
            // Ensure that the prior connection has been terminated
            Disconnect();

            _client = new TcpClient();

            try
            {
                await _client.ConnectAsync(Host, Port);

                _networkStream = _client.GetStream();
                _networkStream.ReadTimeout = ResponseReadTimeout;

                _streamWriter = new StreamWriter(_networkStream);
                _streamReader = new StreamReader(_networkStream);

                // Read the prompt
                await _streamReader.ReadLineAsync();

                return true;
            }
            catch (Exception ex)
            {
                _client = null;

                Console.WriteLine(ex.Message);
            }

            return false;
        }



        // Disconnect from the ASL
        public static void Disconnect()
        {
            if (_networkStream != null)
            {
                try
                {
                    _networkStream.Close();
                    _networkStream.Dispose();
                }
                catch (Exception ex)
                {
                    // Ignore all errors
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    _networkStream = null;
                }
            }

            if (_client != null)
            {
                try
                {
                    _client.Close();
                    _client.Dispose();
                }
                catch (Exception ex)
                {
                    // Ignore all errors
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    _client = null;
                }
            }
        }


        // Send command to the ASL
        public static async Task<string> SendCommand(string command)
        {
            string response = null;
            bool result = true;
            StringBuilder stringBuilder = new StringBuilder();

            try
            {
                Console.WriteLine(command);
                await _streamWriter.WriteLineAsync(command);
                await _streamWriter.FlushAsync();

                string responseLine;

                do
                {
                    responseLine = await _streamReader.ReadLineAsync();
                    Console.WriteLine(responseLine);

                    if (responseLine != null)
                    {
                        stringBuilder.AppendLine(responseLine);

                        if (responseLine.StartsWith(ErrorPrompt))
                        {
                            result = false;
                        }
                    }
                }
                while (responseLine != null && !responseLine.StartsWith(InputPrompt));

                if (result)
                {
                    response = stringBuilder.ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return response;
        }



        // Request parameter value from ASL and recieve response
        public static string GetParamValue(string response, string paramName)
        {
            if (response == null)
            {
                return null;
            }

            string[] stringSeparators = new string[] { "\r\n" };
            string[] responseLines = response.ToUpper().Split(stringSeparators, StringSplitOptions.None);
            string[] paramNameValues = responseLines[0].Split(' ');

            paramName = paramName.ToUpper();

            foreach (string paramNameValue in paramNameValues)
            {
                string[] tokens = paramNameValue.Split('=');
                if (tokens[0] == paramName && tokens.Length == 2)
                {
                    return tokens[1];
                }
            }

            return null;
        }
    }
}
