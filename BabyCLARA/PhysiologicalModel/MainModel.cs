using BabyCLARA.Globals;
using BabyCLARA.PatientModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BabyCLARA.PhysiologicalModel
{
    internal class MainModel
    {
        /*********************************************************/
        /* IMPORT DLL FUNCTIONS AND DECLARE INPUT/OUTPUT STRUCTS */
        /*********************************************************/

        // Lung to Brain Transport Delay
        [DllImport("LBTDLL.dll")]
        static extern void InitLungToBrainTransportDelayModel();

        [DllImport("LBTDLL.dll")]
        static extern double LungToBrainTransportDelayModel(IntPtr input_ptr, IntPtr output_ptr, double SimTimeSec);
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        protected struct LBTDModelInputs
        {
            public double PmCO2_mmHg;
            public double PmO2_mmHg;
            public double Tc_Khoo1982;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        protected struct LBTDModelOutputs
        {
            public double PaCO2;
        }

        // Lung to Carotid Transport Delay
        [DllImport("LCTDLL.dll")]
        static extern void InitLungToCarotidTransportDelayModel();
        [DllImport("LCTDLL.dll")]
        static extern void LungToCarotidTransportDelayModel(IntPtr input_ptr, IntPtr output_ptr, double SimTimeSec);
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        protected struct LCTDelayModelInputs
        {
            public double PmCO2;
            public double PmO2;
            public double Tp_Khoo1982;
            public double Tdeducted;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        protected struct LCTDelayModelOutputs
        {
            public double PpCO2;
            public double PpO2;
        }


        // Peripheral (Carotid) Controller
        [DllImport("PCtlDLL.dll")]
        static extern void InitPeripheralControllerModel();

        [DllImport("PCtlDLL.dll")]
        static extern void PeripheralControllerModel(IntPtr input, IntPtr output, double SimTimeSec);
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        protected struct PeriCtrlModelInputs
        {
            public double PpCO2;
            public double PpO2;
            public double lp;
            public double Gp;
            public double SaO2Target;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        protected struct PeriCtrlModelOutputs
        {
            public double Peripheral_PCO2;
            public double DP;
            public double SaO2;
        }


        // Brain Tissue Compartment Model (With Sleep State Machine)
        [DllImport("BrainTissueCompDLL.dll")]
        static extern void InitBrainTissueCompartmentModel();
        [DllImport("BrainTissueCompDLL.dll")]
        static extern void BrainTissueCompartmentModel(IntPtr input_ptr, IntPtr output_ptr, double SimTimeSec);

        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        protected struct BTCModelInputs
        {
            public double D_Ve_drive;
            public double Pmus_perception;
            public double PaCO2;
            public double Dar;
            public double Dar_REM;
            public double NoiseCtrl;
            public double Gw_DtoPmus_tweak;
            public double SleepManualIncr;
            public double SleepLightsOut;
            public double AbbreviateFactor;
            public double SleepAutoCyclesN;
            public double t_simulation;
            public double KCO2;
            public double KbCO2;
            public double Qbrain_LPMperKg;
            public double MRbCO2;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        protected struct BTCModelOutputs
        {
            public uint arousal;
            public double Gw_WakeSleepMR;
            public int SleepState;
            public int CyclesCompleted;
            public double PbCO2;
        }


        // Central Controller (Medulla) DLL
        [DllImport("CentCtrlDLL.dll")]
        static extern void InitCentralControllerModel();
        [DllImport("CentCtrlDLL.dll")]
        static extern void CentralControllerModel(IntPtr inputs, IntPtr outputs, double SimTimeSec);

        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        protected struct CentCtrlModelInput
        {
            public double PmCO2;
            public double Vpt;
            public double PcentralCO2;
            public double PtRate_nominal;
            public double Ti_eqn_b;
            public double Gc;
            public double Ic;
            public double Vic;
            public double V_FRC;
            public double Crs_linear;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        protected struct CentCtrlModelOutput
        {
            public double Ti;
            public double BrRate;
            public double Dc;
        }


        // Chemoreflex Drive DLL
        [DllImport("ChemRefDriveDLL.dll")]
        static extern void InitChemoreflexDriveModel();
        [DllImport("ChemRefDriveDLL.dll")]
        static extern double ChemoreflexDriveModel(IntPtr input, double SimTimeSec);

        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        protected struct ChemRefDriveInput
        {
            public double SaO2;
            public double Dp;
            public double SleepState;
            public double Gw;
            public double Dc;
            public double SaO2Offset;
            public double SaO2Emergency;
            public double hGrad;
            public double Minimum_wakefulness_drive;
        }


        // Breathing Effort DLL
        [DllImport("BreathEffortDLL.dll")]
        static extern void InitBreathingEffortModel();
        [DllImport("BreathEffortDLL.dll")]
        static extern void BreathingEffortModel(IntPtr input, IntPtr output, double SimTimeSec);

        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        protected struct BreathEffortModelInputs
        {
            public double Dchemo;
            public double SleepState;
            public double ptRate;
            public double Ti;
            public double VolAboveFRC;
            public double Dmin;
            public double StopBreathCtrl;
            public double MIP_preserved_in_REM_pc;
            public double MIP_cmH2O;
            public double Pmus_OpenLoop;
            public double DriveScaling;
            public int CGF_ctrl;
        }

        protected struct BreathEffortModelOutputs
        {
            public double Penvelope;
            public double Pmus;
            public double Inspiration;
            public double breatht;
        }


        // Body Tissue Compartment Model DLL
        [DllImport("BodyTissueCompDLL.dll")]
        static extern void InitBodyTissueCompartmentModel();
        [DllImport("BodyTissueCompDLL.dll")]
        static extern void BodyTissueCompartmentModel(IntPtr input_ptr, IntPtr output_ptr, double SimTimeSec);
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        protected struct BodyTissueModelInputs
        {
            public double PmCO2_mmHg;
            public double PmO2_mmHg;
            public double SleepState;
            public double BrEffort;
            public double Qtissue_LPS;
            public double VtCO2;
            public double VtO2;
            public double MIP_cmH2O;
            public double MR_basal_kcalPerDay;
            public double Pt_RQ;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        protected struct BodyTissueModelOutputs
        {
            public double Cv_CO2;
            public double Cv_O2;
        }


        // Airway, Lungs, Alveolar, Airway Mixing
        [DllImport("AirLungAlvMixDLL.dll")]
        static extern void InitAirwayLungsAlveolarMixingModel();
        [DllImport("AirLungAlvMixDLL.dll")]
        static extern void AirwayLungsAlveolarMixingModel(IntPtr input, IntPtr output,
            double SimTimeSec);

        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        protected struct AirLungAlvMixModelInputs
        {
            public double Pmus;
            public double Ti;
            public double Inspiration;
            public double breath_T;
            public double SleepState;
            public double Pao;
            public double cv_CO2;
            public double cv_O2;
            public double Qpt_source;
            public double VD_anat_normal;
            public double UA_posture_f;
            public double UA_CPAP_titration_cmH2O;
            public double pi_O2;
            public double pi_CO2;
            public double v_frc;
            public double k_863;
            public double Qtotal_LPS;
            public double Crs_linear;
            public double ua_Ctrl;
            public double Vic;
            public double Shunt_At_FRC_pc;
            public double Shunt_At_PEEP20_pc;
            public double Roronasal;
            public double Rlower_insp;
            public double Rlower_exp;
        };

        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        protected struct AirLungAlvMixModelOutputs
        {
            public double P_UA_trans_lumen;
            public double Qpt_system;
            public double Vpt;
            public double VolAboveFRC;
            public double Vt;
            public double Ttot;
            public double VE_dot;
            public double UA_area;
            public double PmouthCO2;
            public double RinspMeanUpperPlusLower;
            public double RexpMeanUpperPlusLower;
            public double PA_oset_eff;
            public double Pa_co2;
            public double Pa_o2;
            public double Qpt_sclm;
        };


        // Heart Vasculature Mixing DLL
        [DllImport("HeartVascMixDLL.dll")]
        static extern void InitHeartVasculatureMixingModel();
        [DllImport("HeartVascMixDLL.dll")]
        static extern void HeartVasculatureMixingModel(IntPtr input, IntPtr output,
            double SimTimeSec);
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        protected struct HeartVascModelInputs
        {
            public double PaCO2;
            public double PaO2;
            public double Td_l_to_p_Khoo_1990;
            public double T1_h;
            public double T2_h;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        protected struct HeartVascModelOutputs
        {
            public double PmCO2_mmHg;
            public double PmO2_mmHg;
        }


        // Sleep State to Chamber Pressure (Static Change) DLL
        [DllImport("SleepPChStaticDLL.dll")]
        static extern void InitSleepToPChamberModel();
        [DllImport("SleepPChStaticDLL.dll")]
        static extern double SleepToPChamberModel(IntPtr input, double SimTimeSec);

        protected struct SleepToPChamberStaticInputs
        {
            public double SleepState;
            public double PChamber_N1;
            public double PChamber_N2;
            public double PChamber_SWS;
            public double PChamber_REM;
        }




        /*****************************/
        /* CLASS MEMBERS AND METHODS */
        /*****************************/

        // Class Constructor
        public MainModel() { }

        // Declared instances of structs and pointers
        protected LBTDModelInputs lbtdModelInput = new();
        protected IntPtr lbtdModelInput_ptr;
        protected LBTDModelOutputs lbtdModelOutput = new();
        protected IntPtr lbtdModelOutput_ptr;

        protected LCTDelayModelInputs lctDelayModelInput = new();
        protected IntPtr lctDelayModelInput_ptr;
        protected LCTDelayModelOutputs lctDelayModelOutput = new();
        protected IntPtr lctDelayModedOutput_ptr;

        protected PeriCtrlModelInputs periCtrlModelInput = new();
        protected IntPtr periCtrlModelInput_ptr;
        protected PeriCtrlModelOutputs periCtrlModelOutput = new();
        protected IntPtr periCtrlModelOutput_ptr;

        protected BTCModelInputs btcModelInput = new();
        protected IntPtr btcModelInput_ptr;
        protected BTCModelOutputs btcModelOutput = new();
        protected IntPtr btcModelOutput_ptr;

        protected CentCtrlModelInput centCtrlModelInput = new();
        protected IntPtr centCtrlModelInput_ptr;
        protected CentCtrlModelOutput centCtrlModelOutput = new();
        protected IntPtr centCtrlModelOutput_ptr;

        protected ChemRefDriveInput chemRefDriveInput = new();
        protected IntPtr chemRefDriveInput_ptr;

        protected BreathEffortModelInputs breathEffortModelInput = new();
        protected IntPtr breathEffortModelInput_ptr;
        protected BreathEffortModelOutputs breathEffortModelOutput = new();
        protected IntPtr breathEffortModelOutput_ptr;

        protected BodyTissueModelInputs bodyTissueModelInput = new();
        protected IntPtr bodyTissueModelInput_ptr;
        protected BodyTissueModelOutputs bodyTissueModelOutput = new();
        protected IntPtr bodyTissueModelOutput_ptr;

        protected AirLungAlvMixModelInputs airLungAlvMixModelInput = new();
        protected IntPtr airLungAlvMixModelInput_ptr;
        protected AirLungAlvMixModelOutputs airLungAlvMixModelOutput = new();
        protected IntPtr airLungAlvMixModelOutput_ptr;

        protected HeartVascModelInputs heartVascModelInput = new();
        protected IntPtr heartVascModelInput_ptr;
        protected HeartVascModelOutputs heartVascModelOutput = new();
        protected IntPtr heartVascModelOutput_ptr;

        protected SleepToPChamberStaticInputs sleepToPChamberStaticInput = new();
        protected IntPtr sleepToPChamberStaticModelInput_ptr;

        // Initialize Embed Models for each DLL
        public void InitMainModelBlocks()
        {
            InitLungToBrainTransportDelayModel();
            InitLungToCarotidTransportDelayModel();
            InitPeripheralControllerModel();
            InitBrainTissueCompartmentModel();
            InitCentralControllerModel();
            InitChemoreflexDriveModel();
            InitBreathingEffortModel();
            InitBodyTissueCompartmentModel();
            InitAirwayLungsAlveolarMixingModel();
            InitHeartVasculatureMixingModel();
            InitSleepToPChamberModel();
        }

        // Allocate memory for the pointers to I/O structs
        public void AllocateMainModelStructIO()
        {
            lbtdModelInput_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(lbtdModelInput));
            lbtdModelOutput_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(lbtdModelOutput));

            lctDelayModelInput_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(lctDelayModelInput));
            lctDelayModedOutput_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(lctDelayModelOutput));

            periCtrlModelInput_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(periCtrlModelInput));
            periCtrlModelOutput_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(periCtrlModelOutput));

            btcModelInput_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(btcModelInput));
            btcModelOutput_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(btcModelOutput));

            centCtrlModelInput_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(centCtrlModelInput));
            centCtrlModelOutput_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(centCtrlModelOutput));

            chemRefDriveInput_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(chemRefDriveInput));

            breathEffortModelInput_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(breathEffortModelInput));
            breathEffortModelOutput_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(breathEffortModelOutput));

            bodyTissueModelInput_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(bodyTissueModelInput));
            bodyTissueModelOutput_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(bodyTissueModelOutput));

            airLungAlvMixModelInput_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(airLungAlvMixModelInput));
            airLungAlvMixModelOutput_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(airLungAlvMixModelOutput));

            heartVascModelInput_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(heartVascModelInput));
            heartVascModelOutput_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(heartVascModelOutput));

            sleepToPChamberStaticModelInput_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(sleepToPChamberStaticInput));
        }


        // Set up initial conditions for the inputs of each Main Model block
        public void MainModelInitialConditions(SimGlobals simGlobals, Patient patient)
        {
            // Lung to Brain Transport Delay
            lbtdModelInput.PmCO2_mmHg = 36.0177;    // First condition. OUTPUT of Mixing in Heart, Lungs, Vasculature
            lbtdModelInput.PmO2_mmHg = 108.675;      // First condition. OUTPUT of Mixing in Heart, Lungs, Vasculature
            lbtdModelInput.Tc_Khoo1982 = simGlobals.Tc_Khoo1982;

            // Lung to Carotid Transport Delay
            lctDelayModelInput.PmCO2 = 36.0177;     // First condition. OUTPUT of Mixing in Heart, Lungs, Vasculature
            lctDelayModelInput.PmO2 = 108.675;       // First condition. OUTPUT of Mixing in Heart, Lungs, Vasculature
            lctDelayModelInput.Tp_Khoo1982 = simGlobals.Tp_Khoo1982;
            lctDelayModelInput.Tdeducted = 0; // ref Embed Model

            // Peripheral Controller (Carotid)
            periCtrlModelInput.PpCO2 = 0;   // Lung to carotid delay
            periCtrlModelInput.PpO2 = 0;    // Lung to carotid delay
            periCtrlModelInput.lp = patient.lp;
            periCtrlModelInput.Gp = patient.Gp;
            periCtrlModelInput.SaO2Target = patient.SaO2_target;

            // Brain Tissue Compartment
            btcModelInput.D_Ve_drive = 1;                        // ref Dchemo, Embed model
            btcModelInput.Pmus_perception = patient.Pmus_Open;
            btcModelInput.PaCO2 = 0;                             // OUTPUT: Lung to Brain Transport Delay output
            btcModelInput.Dar = 0;                               // useless input, ignore
            btcModelInput.Dar_REM = patient.Dar_REM;
            btcModelInput.NoiseCtrl = simGlobals.NoiseCtrl;
            btcModelInput.Gw_DtoPmus_tweak = simGlobals.Gw_DtoPmus_tweak;
            btcModelInput.SleepManualIncr = 0;  // Used for manual increments of sleep stage, ignore
            btcModelInput.SleepLightsOut = 0;   // Used for manual engagement of sleep stage, ignore
            btcModelInput.AbbreviateFactor = 3; // Ref embed model
            btcModelInput.SleepAutoCyclesN = 1; // Ref embed model
            btcModelInput.t_simulation = 0;     // SimTimeSec value
            btcModelInput.KCO2 = simGlobals.KCO2;
            btcModelInput.KbCO2 = simGlobals.KbCO2;
            btcModelInput.Qbrain_LPMperKg = simGlobals.Qbrain_LPMperKg;
            btcModelInput.MRbCO2 = simGlobals.MRbCO2;

            // Central Controller
            centCtrlModelInput.PmCO2 = 36.0177;     // First condition. OUTPUT of Mixing in Heart, Lungs, Vasculature
            centCtrlModelInput.Vpt = 2.3275;    // Ref Embed model
            centCtrlModelInput.PcentralCO2 = 0; // OUTPUT: Brain Tissue Compartment Model
            centCtrlModelInput.PtRate_nominal = patient.PtRate_nominal;
            centCtrlModelInput.Ti_eqn_b = patient.Ti_eqn_b;
            centCtrlModelInput.Gc = patient.Gc;
            centCtrlModelInput.Ic = patient.lc;
            centCtrlModelInput.Vic = patient.Vic;
            centCtrlModelInput.V_FRC = patient.V_FRC;
            centCtrlModelInput.Crs_linear = patient.Crs_linear;

            // Chemoreflex Drive
            chemRefDriveInput.SaO2 = 0;         // OUTPUT: Peripheral Controller Model
            chemRefDriveInput.Dp = 0;           // OUTPUT: Peripheral Controller Model
            chemRefDriveInput.SleepState = 0;   // OUTPUT: Brain Tissues Model
            chemRefDriveInput.Gw = 0;           // OUTPUT: Brain Tissues Model
            chemRefDriveInput.Dc = 0;           // OUTPUT: Central Controller Model
            chemRefDriveInput.SaO2Offset = simGlobals.SaO2Offset;
            chemRefDriveInput.SaO2Emergency = simGlobals.SaO2Emergency;
            chemRefDriveInput.hGrad = patient.hGrad;
            chemRefDriveInput.Minimum_wakefulness_drive = simGlobals.Minimum_wakefulness_drive;

            // Breathing Effort
            breathEffortModelInput.Dchemo = 0;              // OUTPUT: Chemoreflex drive
            breathEffortModelInput.SleepState = 0;          // OUTPUT: Brain Tissues Model
            breathEffortModelInput.ptRate = 0;              // OUTPUT: Central Controller
            breathEffortModelInput.Ti = 0;                  // OUTPUT: Central Controller
            breathEffortModelInput.VolAboveFRC = 0;         // OUTPUT: Airway, Alveolar, Lungs, Mixing Output
            breathEffortModelInput.Dmin = simGlobals.Dmin;
            breathEffortModelInput.StopBreathCtrl = 0;      // Manual breath controller, ingore
            breathEffortModelInput.MIP_preserved_in_REM_pc = patient.MIP_preserved_in_REM_percent;
            breathEffortModelInput.MIP_cmH2O = patient.MIP_cmH2O;
            breathEffortModelInput.Pmus_OpenLoop = patient.Pmus_Open;
            breathEffortModelInput.DriveScaling = 1;
            breathEffortModelInput.CGF_ctrl = 0;

            // Body Tissues Compartment
            bodyTissueModelInput.PmCO2_mmHg = 36.0177;       // First condition. OUTPUT of Mixing in Heart, Lungs, Vasculature Model
            bodyTissueModelInput.PmO2_mmHg = 108.675;        // First condition. OUTPUT of Mixing in Heart, Lungs, Vasculature Model
            bodyTissueModelInput.SleepState = 0;        // OUTPUT: Brain Tissues Model
            bodyTissueModelInput.BrEffort = 0;          // OUTPUT: Breathing Effort Model. Claims that it should be P envelope
            bodyTissueModelInput.Qtissue_LPS = patient.QTissue_LPS;
            bodyTissueModelInput.VtCO2 = simGlobals.VtCO2;
            bodyTissueModelInput.VtO2 = simGlobals.VtO2;
            bodyTissueModelInput.MIP_cmH2O = patient.MIP_cmH2O;
            bodyTissueModelInput.MR_basal_kcalPerDay = patient.MR_basal_kcalPerDay;
            bodyTissueModelInput.Pt_RQ = patient.RQ;


            // Airway Lungs Alveolar Mixing
            airLungAlvMixModelInput.Pmus = 0;           // OUTPUT: Breathing Effort
            airLungAlvMixModelInput.Ti = 0;             // OUTPUT: Central Controller
            airLungAlvMixModelInput.Inspiration = 0;    // OUTPUT: Breathing Effort
            airLungAlvMixModelInput.breath_T = 0;       // OUTPUT: Breathing Effort
            airLungAlvMixModelInput.SleepState = 0;     // OUTPUT: Brain Tissues
            airLungAlvMixModelInput.Pao = 0;            // CITREX PRESSURE MEASUREMENT
            airLungAlvMixModelInput.cv_CO2 = 0;         // OUTPUT: Body Tissues Compartment
            airLungAlvMixModelInput.cv_O2 = 0;          // OUTPUT: Body Tissues Compartment
            airLungAlvMixModelInput.Qpt_source = 0;     // CITREX FLOW MEASUREMENT
            airLungAlvMixModelInput.VD_anat_normal = patient.VD_anat_normal;
            airLungAlvMixModelInput.UA_posture_f = patient.UA_supine_factor;
            airLungAlvMixModelInput.UA_CPAP_titration_cmH2O = patient.Pchamber_REM_cmH2O;
            airLungAlvMixModelInput.pi_CO2 = 0.3;       // ref Embed model. This can be calculatd, but that logic has not yet been exported.
            airLungAlvMixModelInput.pi_O2 = 156.577;    // ref Embed model. This can be calculatd, but that logic has not yet been exported.
            airLungAlvMixModelInput.v_frc = patient.V_FRC;
            airLungAlvMixModelInput.k_863 = patient.K_863;
            airLungAlvMixModelInput.Qtotal_LPS = simGlobals.Qtotal_LPS;
            airLungAlvMixModelInput.Crs_linear = patient.Crs_linear;
            airLungAlvMixModelInput.ua_Ctrl = 0;        // ref Embed model. We are not going to use the complex virtual collapsible airway
            airLungAlvMixModelInput.Vic = patient.Vic;
            airLungAlvMixModelInput.Shunt_At_FRC_pc = patient.Shunt_At_FRC_perc;
            airLungAlvMixModelInput.Shunt_At_PEEP20_pc = patient.Shunt_at_PEEP20perc;
            airLungAlvMixModelInput.Roronasal = patient.Roronasal;
            airLungAlvMixModelInput.Rlower_insp = patient.Rlower_insp;
            airLungAlvMixModelInput.Rlower_exp = patient.Rlower_exp;

            // Mixing in Heart, Lungs, VascModelulature
            heartVascModelInput.PaCO2 = 0;                   // OUTPUT: AirwayLungsAlveolarMixing
            heartVascModelInput.PaO2 = 0;                    // OUTPUT: AirwayLungsAlveolarMixing
            heartVascModelInput.Td_l_to_p_Khoo_1990 = simGlobals.Td_l_to_p_Khoo1990;
            heartVascModelInput.T1_h = simGlobals.T1_h;
            heartVascModelInput.T2_h = simGlobals.T2_h;

            // Sleep to PChamber (static) Model
            sleepToPChamberStaticInput.SleepState = 0;
            sleepToPChamberStaticInput.PChamber_REM = patient.Pchamber_REM_cmH2O;
            sleepToPChamberStaticInput.PChamber_N1 = patient.Pchamber_N1_cmH2O;
            sleepToPChamberStaticInput.PChamber_N2 = patient.Pchamber_N2_cmH2O;
            sleepToPChamberStaticInput.PChamber_SWS = patient.Pchamber_SWS_cmH2O;
        }


        // Run Model
        public void MainModelSimStep(SimGlobals simGlobals, Patient patient, double measuredFlow,
            double measuredPressure, double SimTimeSec)
        {
            // Run simulation in order. As each block is run, propagate the outputs to the other blocks that require them

            // If we have done a complete first pass of the simulation, then we can explicitly hook the outputs of the heartVasc model
            if (SimTimeSec > 0)
            {
                lbtdModelInput.PmCO2_mmHg = heartVascModelOutput.PmCO2_mmHg;
                lbtdModelInput.PmO2_mmHg = heartVascModelOutput.PmO2_mmHg;

                lctDelayModelInput.PmCO2 = heartVascModelOutput.PmCO2_mmHg;
                lctDelayModelInput.PmO2 = heartVascModelOutput.PmO2_mmHg;

                centCtrlModelInput.PmCO2 = heartVascModelOutput.PmCO2_mmHg;

                bodyTissueModelInput.PmCO2_mmHg = heartVascModelOutput.PmCO2_mmHg;
                bodyTissueModelInput.PmO2_mmHg = heartVascModelOutput.PmO2_mmHg;
            }

            // Lung To Brain Transport Delay Block
            Marshal.StructureToPtr(lbtdModelInput, lbtdModelInput_ptr, true);
            LungToBrainTransportDelayModel(lbtdModelInput_ptr, lbtdModelOutput_ptr, SimTimeSec);
            lbtdModelOutput = (LBTDModelOutputs)Marshal.PtrToStructure(lbtdModelOutput_ptr, typeof(LBTDModelOutputs))!;

            // Lung to Carotid Transport Delay Block
            Marshal.StructureToPtr(lctDelayModelInput, lctDelayModelInput_ptr, true);
            LungToCarotidTransportDelayModel(lctDelayModelInput_ptr, lctDelayModedOutput_ptr, SimTimeSec);
            lctDelayModelOutput = (LCTDelayModelOutputs)Marshal.PtrToStructure(lctDelayModedOutput_ptr, typeof(LCTDelayModelOutputs))!;

            // Peripheral Controller (Carotid)
            periCtrlModelInput.PpCO2 = lctDelayModelOutput.PpCO2;
            periCtrlModelInput.PpO2 = lctDelayModelOutput.PpO2;
            Marshal.StructureToPtr(periCtrlModelInput, periCtrlModelInput_ptr, true);
            PeripheralControllerModel(periCtrlModelInput_ptr, periCtrlModelOutput_ptr, SimTimeSec);
            periCtrlModelOutput = (PeriCtrlModelOutputs)Marshal.PtrToStructure(periCtrlModelOutput_ptr, typeof(PeriCtrlModelOutputs))!;

            // Brain Tissue Compartment
            if (patient.patientManualControl)
            {
                btcModelInput.SleepLightsOut = patient.SleepLightsOut;
                btcModelInput.SleepManualIncr = patient.SleepManualIncrement;
            }
            btcModelInput.Pmus_perception = breathEffortModelOutput.Penvelope;
            btcModelInput.D_Ve_drive = patient.Dchemo;
            btcModelInput.PaCO2 = lbtdModelOutput.PaCO2;
            btcModelInput.t_simulation = SimTimeSec;
            Marshal.StructureToPtr(btcModelInput, btcModelInput_ptr, true);
            BrainTissueCompartmentModel(btcModelInput_ptr, btcModelOutput_ptr, SimTimeSec);
            btcModelOutput = (BTCModelOutputs)Marshal.PtrToStructure(btcModelOutput_ptr, typeof(BTCModelOutputs))!;
            if (patient.patientManualControl)
            {
                btcModelInput.SleepManualIncr = 0;
                patient.patientManualControl = false;
            }

            // Central Controller
            centCtrlModelInput.PtRate_nominal = patient.PtRate_nominal;
            centCtrlModelInput.Gc = patient.Gc;
            centCtrlModelInput.Ic = patient.lc;
            centCtrlModelInput.Vic = patient.Vic;
            centCtrlModelInput.V_FRC = patient.V_FRC;
            centCtrlModelInput.Crs_linear = patient.Crs_linear;
            centCtrlModelInput.PcentralCO2 = btcModelOutput.PbCO2;
            centCtrlModelInput.Vpt = airLungAlvMixModelOutput.Vpt;
            Marshal.StructureToPtr(centCtrlModelInput, centCtrlModelInput_ptr, true);
            CentralControllerModel(centCtrlModelInput_ptr, centCtrlModelOutput_ptr, SimTimeSec);
            centCtrlModelOutput = (CentCtrlModelOutput)Marshal.PtrToStructure(centCtrlModelOutput_ptr, typeof(CentCtrlModelOutput))!;

            // Chemoreflex Drive
            chemRefDriveInput.SaO2 = periCtrlModelOutput.SaO2;
            chemRefDriveInput.Dp = periCtrlModelOutput.DP;
            chemRefDriveInput.SleepState = btcModelOutput.SleepState;
            chemRefDriveInput.Gw = btcModelOutput.Gw_WakeSleepMR;
            chemRefDriveInput.Dc = centCtrlModelOutput.Dc;
            Marshal.StructureToPtr(chemRefDriveInput, chemRefDriveInput_ptr, true);
            patient.Dchemo = ChemoreflexDriveModel(chemRefDriveInput_ptr, SimTimeSec);

            // Breathing Effort 
            breathEffortModelInput.Dchemo = patient.Dchemo;
            breathEffortModelInput.SleepState = btcModelOutput.SleepState;
            breathEffortModelInput.ptRate = centCtrlModelOutput.BrRate;
            breathEffortModelInput.Ti = centCtrlModelOutput.Ti;
            breathEffortModelInput.VolAboveFRC = airLungAlvMixModelOutput.VolAboveFRC;
            breathEffortModelInput.CGF_ctrl = patient.CGF_ctrl;
            Marshal.StructureToPtr(breathEffortModelInput, breathEffortModelInput_ptr, true);
            BreathingEffortModel(breathEffortModelInput_ptr, breathEffortModelOutput_ptr, SimTimeSec);
            breathEffortModelOutput = (BreathEffortModelOutputs)Marshal.PtrToStructure(breathEffortModelOutput_ptr, typeof(BreathEffortModelOutputs))!;
            patient.Pmus = breathEffortModelOutput.Pmus;    // Saving this value for ease of plotting

            // Body Tissues Compartment
            bodyTissueModelInput.SleepState = btcModelOutput.SleepState;
            bodyTissueModelInput.BrEffort = breathEffortModelOutput.Penvelope;
            Marshal.StructureToPtr(bodyTissueModelInput, bodyTissueModelInput_ptr, true);
            BodyTissueCompartmentModel(bodyTissueModelInput_ptr, bodyTissueModelOutput_ptr, SimTimeSec);
            bodyTissueModelOutput = (BodyTissueModelOutputs)Marshal.PtrToStructure(bodyTissueModelOutput_ptr, typeof(BodyTissueModelOutputs))!;

            // Airway Lungs Alveolar Mixing
            airLungAlvMixModelInput.Crs_linear = patient.Crs_linear;
            airLungAlvMixModelInput.Vic = patient.Vic;
            airLungAlvMixModelInput.Shunt_At_FRC_pc = patient.Shunt_At_FRC_perc;
            airLungAlvMixModelInput.Shunt_At_PEEP20_pc = patient.Shunt_at_PEEP20perc;
            airLungAlvMixModelInput.Roronasal = patient.Roronasal;
            airLungAlvMixModelInput.Rlower_insp = patient.Rlower_insp;
            airLungAlvMixModelInput.Rlower_exp = patient.Rlower_exp;
            airLungAlvMixModelInput.Pmus = breathEffortModelOutput.Pmus;
            airLungAlvMixModelInput.Ti = centCtrlModelOutput.Ti;
            airLungAlvMixModelInput.Inspiration = breathEffortModelOutput.Inspiration;
            airLungAlvMixModelInput.breath_T = breathEffortModelOutput.breatht;
            airLungAlvMixModelInput.SleepState = btcModelOutput.SleepState;
            airLungAlvMixModelInput.Pao = measuredPressure;             // CITREX PRESSURE MEASUREMENT
            airLungAlvMixModelInput.cv_CO2 = bodyTissueModelOutput.Cv_CO2;
            airLungAlvMixModelInput.cv_O2 = bodyTissueModelOutput.Cv_O2;
            airLungAlvMixModelInput.Qpt_source = measuredFlow; //airLungAlvMixModelOutput.Qpt_sclm;  // measuredFlow (converted to LPS);          // CITREX FLOW MEASUREMENT
            Marshal.StructureToPtr(airLungAlvMixModelInput, airLungAlvMixModelInput_ptr, true);
            AirwayLungsAlveolarMixingModel(airLungAlvMixModelInput_ptr, airLungAlvMixModelOutput_ptr, SimTimeSec);
            airLungAlvMixModelOutput = (AirLungAlvMixModelOutputs)Marshal.PtrToStructure(airLungAlvMixModelOutput_ptr, typeof(AirLungAlvMixModelOutputs))!;

            // Mixing in Heart, Lungs, Vasculature Model
            heartVascModelInput.PaCO2 = airLungAlvMixModelOutput.Pa_co2;
            heartVascModelInput.PaO2 = airLungAlvMixModelOutput.Pa_o2;
            Marshal.StructureToPtr(heartVascModelInput, heartVascModelInput_ptr, true);
            HeartVasculatureMixingModel(heartVascModelInput_ptr, heartVascModelOutput_ptr, SimTimeSec);
            heartVascModelOutput = (HeartVascModelOutputs)Marshal.PtrToStructure(heartVascModelOutput_ptr, typeof(HeartVascModelOutputs))!;

            // Sleep State to Chamber Pressure (static)
            sleepToPChamberStaticInput.SleepState = btcModelOutput.SleepState;
            Marshal.StructureToPtr(sleepToPChamberStaticInput, sleepToPChamberStaticModelInput_ptr, true);
            patient.ChamberPressure = SleepToPChamberModel(sleepToPChamberStaticModelInput_ptr, SimTimeSec);

            // Store output variables for printing
            patient.PmCO2 = heartVascModelOutput.PmCO2_mmHg;
            patient.SaO2 = periCtrlModelOutput.SaO2;
            patient.SleepState = btcModelOutput.SleepState;  
        }



        // Free Memory
        public void MainModelFreeMemory()
        {
            Marshal.FreeHGlobal(lbtdModelInput_ptr);
            Marshal.FreeHGlobal(lbtdModelOutput_ptr);
            Marshal.FreeHGlobal(lctDelayModelInput_ptr);
            Marshal.FreeHGlobal(lctDelayModedOutput_ptr);
            Marshal.FreeHGlobal(periCtrlModelInput_ptr);
            Marshal.FreeHGlobal(periCtrlModelOutput_ptr);
            Marshal.FreeHGlobal(btcModelInput_ptr);
            Marshal.FreeHGlobal(btcModelOutput_ptr);
            Marshal.FreeHGlobal(centCtrlModelInput_ptr);
            Marshal.FreeHGlobal(centCtrlModelOutput_ptr);
            Marshal.FreeHGlobal(chemRefDriveInput_ptr);
            Marshal.FreeHGlobal(breathEffortModelInput_ptr);
            Marshal.FreeHGlobal(bodyTissueModelInput_ptr);
            Marshal.FreeHGlobal(airLungAlvMixModelInput_ptr);
            Marshal.FreeHGlobal(airLungAlvMixModelOutput_ptr);
            Marshal.FreeHGlobal(heartVascModelInput_ptr);
            Marshal.FreeHGlobal(heartVascModelOutput_ptr);
            Marshal.FreeHGlobal(sleepToPChamberStaticModelInput_ptr);
        }
    }
}
