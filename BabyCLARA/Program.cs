using BabyCLARA.PatientModel;
using BabyCLARA.PhysiologicalModel;
using BabyCLARA.FlowAnalyzerInterface;
using BabyCLARA.ASLInterface;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using NationalInstruments.DAQmx;
using BabyCLARA.Globals;

namespace BabyCLARA
{
   class Program
    {
        static void Main()
        {
            Console.WriteLine("This is Baby CLARA.");

            // Declare patient type and load up the variables from the catalog
            string type = "OHS_lateral";
            PatientPosture posture = PatientPosture.NOT_LATERAL;
            Patient patient = new(type, posture);
            double maxSimTime = 600;
            SimGlobals globals = new(maxSimTime);
            AuxBlocks auxBlocks = new();
            MainModel mainModel = new();

            // Initalize Control of Breathing Model and relevant parameters
            InitializeSim(patient, globals, mainModel, auxBlocks);

            // Create the DAQ task and channel
            NationalInstruments.DAQmx.Task daqTask = new NationalInstruments.DAQmx.Task();
            SetupDAQ(daqTask);
            AnalogSingleChannelWriter analogChannelWriter = new AnalogSingleChannelWriter(daqTask.Stream);

            // Set up the flow analyzer
            FlowAnalyzer flowAnalyzer = new FlowAnalyzer();
            flowAnalyzer.Init();
            flowAnalyzer.StartDataCapture();

            // Set up stopwatch
            Stopwatch stopwatch = Stopwatch.StartNew();
            double prevTime = stopwatch.Elapsed.TotalMilliseconds;

            // Run Simulation
            RunCLARA(daqTask, analogChannelWriter, flowAnalyzer, stopwatch, prevTime,
                patient, globals, mainModel, auxBlocks);

            // Clean-up
            CleanUpSim(mainModel, auxBlocks);
        }

        

        // Main loop for CLARA. Collects data from flow analyzer, runs ten iterations of the
        // Control of Breathing model simulation, and outputs voltage from the DAQ every 10 ms.
        static void RunCLARA(NationalInstruments.DAQmx.Task daqTask, 
            AnalogSingleChannelWriter analogChannelWriter, FlowAnalyzer flowAnalyzer,
            Stopwatch stopwatch, double prevTime, Patient patient, SimGlobals globals, 
            MainModel mainModel, AuxBlocks auxBlocks)
        {
            Console.WriteLine("Inside RunCLARA function");

            // Test
            const double MinVoltage = 0;
            const double MaxVoltage = 10;
            const double SignalAmplitude = MaxVoltage / 2;
            const double SignalBias = MaxVoltage / 2;

            // Sampling interval is 10 ms which gives a sampling freuqency of 100 Hz
            const double SamplingInterval = 10;
            double simTime = 0;
        
            while (true)
            {
                // Take time measurement 
                double elapsedTime = stopwatch.Elapsed.TotalMilliseconds;

                if (elapsedTime - prevTime >= SamplingInterval)
                {
                    double measuredPressure = flowAnalyzer.Pressure;
                    double measuredFlow = flowAnalyzer.Flow;
                    MainSimLoop(patient, globals, mainModel, auxBlocks, 0,
                        0, simTime);
                    Console.WriteLine($"{simTime}, {patient.Pmus}");
                    analogChannelWriter.WriteSingleSample(true, patient.Pmus + SignalBias);
                    prevTime = elapsedTime;
                    simTime += 0.01;
                }

                if (stopwatch.Elapsed.TotalSeconds > globals.MaxSimTime) { break; }
            }

        }



        // Main simulation loop for Control of Breathing Model
        static void MainSimLoop(Patient patient, SimGlobals globals, MainModel mainModel,
            AuxBlocks auxBlocks, double measuredFlow, double measuredPressure, double currSimTime)
        {
            for (double SimTimeSec = currSimTime; SimTimeSec <= currSimTime + 0.01; SimTimeSec += 0.001)
            { 
                if (SimTimeSec > 0) { auxBlocks.AuxBlocksSimStep(globals, patient, SimTimeSec); }
                mainModel.MainModelSimStep(globals, patient, measuredFlow, measuredPressure, SimTimeSec);
            }
        }


        // Import all DLLs, allocate heap memory for simulation, and initialize all simulation functions
        static void InitializeSim(Patient patient, SimGlobals globals, MainModel mainModel, AuxBlocks auxBlocks)
        {
            // Prepare auxiliary models
            auxBlocks.InitAuxModelBlocks();
            auxBlocks.AllocateAuxModelStructIO();
            auxBlocks.AuxModelInitialConditions(globals, patient);
            auxBlocks.AuxBlocksSimStep(globals, patient, 0);

            // Prepare Main model blocks
            mainModel.InitMainModelBlocks();
            mainModel.AllocateMainModelStructIO();
            mainModel.MainModelInitialConditions(globals, patient);
        }



        // Free up global heap memory of simulation
        static void CleanUpSim(MainModel mainModel, AuxBlocks auxBlocks)
        {
            auxBlocks.AuxBlocksFreeMemory();
            mainModel.MainModelFreeMemory();
        }

        

        // Declare paramters for DAQ and initialize the daqTask object
        static void SetupDAQ(NationalInstruments.DAQmx.Task daqTask)
        {
            // Set up DAQ Paramters
            const string PhysicalChannel = "Dev1/ao0";
            const double MinVoltage = 0;
            const double MaxVoltage = 10;

            // Create the task and channel
            // Create the task and channel
            daqTask.AOChannels.CreateVoltageChannel(
                PhysicalChannel,
                string.Empty,
                MinVoltage,
                MaxVoltage,
                AOVoltageUnits.Volts);

            // Verify the task before doing the waveform calculations
            daqTask.Control(TaskAction.Verify);
        }
    }
}
