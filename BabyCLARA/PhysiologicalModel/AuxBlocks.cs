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
    internal class AuxBlocks
    {
        /*********************************************************/
        /* IMPORT DLL FUNCTIONS AND DECLARE INPUT/OUTPUT STRUCTS */
        /*********************************************************/

        // Water Vapor DLL
        [DllImport("PWaterVaporDLL.dll")]
        static extern void InitPWaterVaporModel();
        [DllImport("PWaterVaporDLL.dll")]
        public static extern double PWaterVaporModel(IntPtr input_ptr, double SimTimeSec);
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct PWatVapInputs
        {
            public double Temp;
            public double RH;
        }


        // Gas Exchange Factor DLL
        [DllImport("GasExchangeFactorDLL.dll")]
        static extern void InitGasExchangeFactorModel();
        [DllImport("GasExchangeFactorDLL.dll")]
        static extern double GasExchangeFactorModel(IntPtr input_ptr, double SimTimeSec);
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct GasExFacModelInputs
        {
            public double Pb;
            public double Pvap;
        }


        // Basal Metabolic Rate DLL
        [DllImport("BasalMetaRateDLL.dll")]
        static extern void InitBasalMetabolicRateModel();
        [DllImport("BasalMetaRateDLL.dll")]
        static extern double BasalMetabolicRateModel(IntPtr input, double SimStateSim);
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct BasalMRInput
        {
            public double Ht_cm;
            public double SNS_f;
        }


        // Height to Median Body Weight DLL
        [DllImport("HeightToMedBWDLL.dll")]
        static extern void InitHeightToMedianBodyWeightModel();
        [DllImport("HeightToMedBWDLL.dll")]
        static extern double HeightToMedianBodyWeightModel(double Height_cm, double SimTimeSec);


        // Relative Obesity DLL
        [DllImport("RelativeObesityDLL.dll")]
        static extern void InitRelativeObesityModel();
        [DllImport("RelativeObesityDLL.dll")]
        static extern void RelativeObesityModel(IntPtr input, IntPtr output,
            double SimTimeSec);
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct RelativeObesityInputs
        {
            public double Wt_actual;
            public double Wt_ideal_for_Ht;
            public double Ht_cm;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct RelativeObesityOutputs
        {
            public double Obese_f;
            public double BMI_kg_per_m3;
        }


        // Patient Crs DLL
        [DllImport("PatientCrsDLL.dll")]
        static extern void InitPatientCrsModel();
        [DllImport("PatientCrsDLL.dll")]
        static extern void PatientCrsModel(IntPtr input, IntPtr output, double SimTimeSec);
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct PatientCrsModelInputs
        {
            public double Height_cm;
            public double path_f;
            public double Lateral;
            public double Obese_f;
        }
        public struct PatientCrsModelOutputs
        {
            public double Crs;
            public double UA_supine_factor;
        }


        // Lung/Airway Capacities DLL
        [DllImport("LungAirwayCapDLL.dll")]
        static extern void InitLungAirwayCapacitiesModel();
        [DllImport("LungAirwayCapDLL.dll")]
        static extern void LungAirwayCapacitiesModel(IntPtr input_ptr, IntPtr output_ptr,
                                                    double SimTimeSec);
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct LungAirwayCapModelInput
        {
            public double Ht_cm;
            public double Crs_pt;
            public double FRC_pc_normal;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct LungAirwayCapModelOutput
        {
            public double Vd_anat;
            public double V_tcl;
            public double V_ic;
            public double V_frc;
        }


        // Airway Resistance Raw DLL
        [DllImport("AWResRawDLL.dll")]
        static extern void InitAirwayResistanceRawModel();
        [DllImport("AWResRawDLL.dll")]
        static extern void AirwayResistanceRawModel(IntPtr input, IntPtr output, double SimTimeSec);
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct AirwayResistanceModelInputs
        {
            public double Rtot_f;
            public double PtHeight_cm;
        }
        public struct AirwayResistanceModelOutputs
        {
            public double Roronasal;
            public double Rlower_insp;
            public double Rlower_exp;
            public double Rinsp_nominal;
            public double Rexp_nominal;
        }


        // Height to Rate DLL
        [DllImport("HeightToRateDLL.dll")]
        static extern void InitHeightToRateModel();
        [DllImport("HeightToRateDLL.dll")]
        static extern double HeightToRateModel(double Height_cm, double SimTimeSec);


        // Blood In Brain Flow DLL
        [DllImport("BldBrnFlwDLL.dll")]
        static extern void InitBloodInBrainFlowModel();
        [DllImport("BldBrnFlwDLL.dll")]
        static extern double BloodInBrainFlowModel(IntPtr input, double SimTimeSec);
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct BloodInBrainFlowInputs
        {
            public double Qbrain_LPMperKg;
            public double MBW_kg;
            public double PtHeight_cm;
            public double QTotal_LPS;
        };



        /*****************************/
        /* CLASS MEMBERS AND METHODS */
        /*****************************/

        // Class Constructor
        public AuxBlocks() { }

        // Struct and pointer inputs of select auxiliary function blocks
        public PWatVapInputs pWaterVaporInput = new();
        public IntPtr pWaterVaporInput_ptr;

        public GasExFacModelInputs gasExFacModelInput = new();
        public IntPtr gasExFacModelInput_ptr;

        public BasalMRInput basalMRInput = new();
        public IntPtr basalMRInput_ptr;

        public RelativeObesityInputs relativeObesityInput = new();
        public IntPtr relativeObesityInput_ptr;
        public RelativeObesityOutputs relativeObesityOutput = new();
        public IntPtr relativeObesityOutput_ptr;

        public PatientCrsModelInputs patientCrsModelInput = new();
        public IntPtr patientCrsModelInput_ptr;
        public PatientCrsModelOutputs patientCrsModelOutput = new();
        public IntPtr patientCrsModelOutput_ptr;

        public LungAirwayCapModelInput lungAirwayCapInput = new();
        public IntPtr lungAirwayCapInput_ptr;
        public LungAirwayCapModelOutput lungAirwayCapOutput = new();
        public IntPtr lungAirwayCapOutput_ptr;

        public AirwayResistanceModelInputs airwayResistanceModelInput = new();
        public IntPtr airwayResistanceModelInput_ptr;
        public AirwayResistanceModelOutputs airwayResistanceModelOutput = new();
        public IntPtr airwayResistanceModelOutput_ptr;

        public BloodInBrainFlowInputs bloodInBrainFlowInput = new();
        public IntPtr bloodInBrainFlowInput_ptr;



        // Initialize the function blocks
        public void InitAuxModelBlocks()
        {
            InitPWaterVaporModel();
            InitGasExchangeFactorModel();
            InitBasalMetabolicRateModel();
            InitHeightToMedianBodyWeightModel();
            InitRelativeObesityModel();
            InitPatientCrsModel();
            InitLungAirwayCapacitiesModel();
            InitAirwayResistanceRawModel();
            InitHeightToRateModel();
            InitBloodInBrainFlowModel();
        }



        // Allocate memory for the pointers to I/O structs
        public void AllocateAuxModelStructIO()
        {
            pWaterVaporInput_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(pWaterVaporInput));

            gasExFacModelInput_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(gasExFacModelInput));

            basalMRInput_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(basalMRInput));

            relativeObesityInput_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(relativeObesityInput));
            relativeObesityOutput_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(relativeObesityOutput));

            patientCrsModelInput_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(patientCrsModelInput));
            patientCrsModelOutput_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(patientCrsModelOutput));

            lungAirwayCapInput_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(lungAirwayCapInput));
            lungAirwayCapOutput_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(lungAirwayCapOutput));

            airwayResistanceModelInput_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(airwayResistanceModelInput));
            airwayResistanceModelOutput_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(airwayResistanceModelOutput));

            bloodInBrainFlowInput_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(bloodInBrainFlowInput));
        }



        public void AuxModelInitialConditions(SimGlobals simGlobals, Patient patient)
        {
            // Water Vapor Initial Inputs
            pWaterVaporInput.Temp = 37;
            pWaterVaporInput.RH = 100;

            // Gas Exchange Factor Model Initial Inputs
            gasExFacModelInput.Pb = simGlobals.Pbar;
            gasExFacModelInput.Pvap = 0;  // PWaterVaporBlock

            // Basal Metabolic Rate Model Initial Inputs
            basalMRInput.Ht_cm = patient.Height_cm;
            basalMRInput.SNS_f = patient.SNS_Elevation_f;

            // Relative Obesity Model Inital Inputs
            relativeObesityInput.Wt_actual = patient.Weight_kg;
            relativeObesityInput.Wt_ideal_for_Ht = 0;  // Height to Median Body Weight
            relativeObesityInput.Ht_cm = patient.Height_cm;

            // Patient Crs Model Initial Inputs
            patientCrsModelInput.Height_cm = patient.Height_cm;
            patientCrsModelInput.path_f = patient.Crs_f;
            patientCrsModelInput.Lateral = 1; // patient.lateral_posture;
            patientCrsModelInput.Obese_f = 0;   // Relative Obesity Block

            // Lung/Airway Capacities Initial Inputs
            lungAirwayCapInput.Ht_cm = patient.Height_cm;
            lungAirwayCapInput.Crs_pt = 0;  // Patient Crs block
            lungAirwayCapInput.FRC_pc_normal = patient.FRC_perc_Normal;

            // Airway Resistance Initial Inputs
            airwayResistanceModelInput.Rtot_f = patient.Rtot_f;
            airwayResistanceModelInput.PtHeight_cm = patient.Height_cm;

            // Blood In Brain Flow Initial Inputs
            bloodInBrainFlowInput.Qbrain_LPMperKg = simGlobals.Qbrain_LPMperKg;
            bloodInBrainFlowInput.MBW_kg = 0; // Height to Median Body Weight
            bloodInBrainFlowInput.PtHeight_cm = patient.Height_cm;
            bloodInBrainFlowInput.QTotal_LPS = simGlobals.Qtotal_LPS;
        }



        public void AuxBlocksSimStep(SimGlobals simGlobals, Patient patient, double SimTimeSec)
        {
            // Run simulation in order. As each block is run, propagate the outputs to the other blocks that require them
            if (SimTimeSec > 0)
            {
                // Water vapor block
                Marshal.StructureToPtr(pWaterVaporInput, pWaterVaporInput_ptr, true);
                gasExFacModelInput.Pvap = PWaterVaporModel(pWaterVaporInput_ptr, SimTimeSec);

                // Gas Exchange Factor block
                Marshal.StructureToPtr(gasExFacModelInput, gasExFacModelInput_ptr, true);
                patient.K_863 = GasExchangeFactorModel(gasExFacModelInput_ptr, SimTimeSec);

                // Height to Median Body Weight Block
                patient.MBW_kg = HeightToMedianBodyWeightModel(patient.Height_cm, SimTimeSec);

                // Blood in Brain Flow Block
                bloodInBrainFlowInput.MBW_kg = patient.MBW_kg;
                Marshal.StructureToPtr(bloodInBrainFlowInput, bloodInBrainFlowInput_ptr, true);
                patient.QTissue_LPS = BloodInBrainFlowModel(bloodInBrainFlowInput_ptr, SimTimeSec);

                // Relative Obesity Block
                relativeObesityInput.Wt_ideal_for_Ht = patient.MBW_kg;
                Marshal.StructureToPtr(relativeObesityInput, relativeObesityInput_ptr, true);
                RelativeObesityModel(relativeObesityInput_ptr, relativeObesityOutput_ptr, SimTimeSec);
                relativeObesityOutput = (RelativeObesityOutputs)Marshal.PtrToStructure(relativeObesityOutput_ptr, typeof(RelativeObesityOutputs))!;

                // Patient Crs Block
                patientCrsModelInput.Obese_f = relativeObesityOutput.Obese_f;
                Marshal.StructureToPtr(patientCrsModelInput, patientCrsModelInput_ptr, true);
                PatientCrsModel(patientCrsModelInput_ptr, patientCrsModelOutput_ptr, SimTimeSec);
                patientCrsModelOutput = (PatientCrsModelOutputs)Marshal.PtrToStructure(patientCrsModelOutput_ptr, typeof(PatientCrsModelOutputs))!;
                patient.Crs_linear = patientCrsModelOutput.Crs;
                patient.UA_supine_factor = patientCrsModelOutput.UA_supine_factor;

                // Lung/Airway Capacities Block
                lungAirwayCapInput.Crs_pt = patientCrsModelOutput.Crs;
                Marshal.StructureToPtr(lungAirwayCapInput, lungAirwayCapInput_ptr, true);
                LungAirwayCapacitiesModel(lungAirwayCapInput_ptr, lungAirwayCapOutput_ptr, SimTimeSec);
                lungAirwayCapOutput = (LungAirwayCapModelOutput)Marshal.PtrToStructure(lungAirwayCapOutput_ptr, typeof(LungAirwayCapModelOutput))!;
                patient.VD_anat_normal = lungAirwayCapOutput.Vd_anat;
                patient.Vic = lungAirwayCapOutput.V_ic;
                patient.V_FRC = lungAirwayCapOutput.V_frc;

                // Height to Metabolic Rate
                patient.PtRate_nominal = HeightToRateModel(patient.Height_cm, SimTimeSec);

                // Basal Metabolic Rate
                Marshal.StructureToPtr(basalMRInput, basalMRInput_ptr, true);
                patient.MR_basal_kcalPerDay = BasalMetabolicRateModel(basalMRInput_ptr, SimTimeSec);

                // Airway Resistance Raw Model
                Marshal.StructureToPtr(airwayResistanceModelInput, airwayResistanceModelInput_ptr, true);
                AirwayResistanceRawModel(airwayResistanceModelInput_ptr, airwayResistanceModelOutput_ptr, SimTimeSec);
                airwayResistanceModelOutput = (AirwayResistanceModelOutputs)Marshal.PtrToStructure(airwayResistanceModelOutput_ptr, typeof(AirwayResistanceModelOutputs))!;
                patient.Roronasal = airwayResistanceModelOutput.Roronasal;
                patient.Rlower_insp = airwayResistanceModelOutput.Rlower_insp;
                patient.Rlower_exp = airwayResistanceModelOutput.Rlower_exp;
                patient.Rinsp_nominal = airwayResistanceModelOutput.Rinsp_nominal;
                patient.Rexp_nominal = airwayResistanceModelOutput.Rexp_nominal;
            }
            else if (SimTimeSec > 0)
            {
                // Patient Crs Block
                PatientCrsModel(patientCrsModelInput_ptr, patientCrsModelOutput_ptr, SimTimeSec);
                patientCrsModelOutput = (PatientCrsModelOutputs)Marshal.PtrToStructure(patientCrsModelOutput_ptr, typeof(PatientCrsModelOutputs))!;
                patient.Crs_linear = patientCrsModelOutput.Crs;
            }

        }

        public void AuxBlocksFreeMemory()
        {
            Marshal.FreeHGlobal(pWaterVaporInput_ptr);
            Marshal.FreeHGlobal(gasExFacModelInput_ptr);
            Marshal.FreeHGlobal(basalMRInput_ptr);
            Marshal.FreeHGlobal(relativeObesityInput_ptr);
            Marshal.FreeHGlobal(relativeObesityOutput_ptr);
            Marshal.FreeHGlobal(patientCrsModelInput_ptr);
            Marshal.FreeHGlobal(patientCrsModelOutput_ptr);
            Marshal.FreeHGlobal(lungAirwayCapInput_ptr);
            Marshal.FreeHGlobal(lungAirwayCapOutput_ptr);
            Marshal.FreeHGlobal(airwayResistanceModelInput_ptr);
            Marshal.FreeHGlobal(airwayResistanceModelOutput_ptr);
            Marshal.FreeHGlobal(bloodInBrainFlowInput_ptr);
        }
    }
}
