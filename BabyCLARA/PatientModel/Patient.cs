using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;

namespace BabyCLARA.PatientModel
{
    public enum PatientType
    {
        COPD,
        NMD,
        OHS_SUPINE,
        OHS_LATERAL
    }

    public enum PatientPosture
    {
        NOT_LATERAL,
        LATERAL,
    }

    enum PtCatIndexNum
    {
        REFERENCE = 0,
        PATHOLOGY = 1,
        PATHOLOGY_SEVERITY = 2,
        HEIGHT_CM = 3,
        WEIGHT_KG = 4,
        AGE_YR = 5,
        DAYTIME_CO2_MMHG = 6,
        PCHAMBER_REM = 7,
        MIP_CM_H2O = 8,
        MIP_PRESERVED_IN_REM = 9,
        RQ = 10,
        SNS_ELEVATION = 11,
        FRC_PERC_NORM = 12,
        SHUNT_AT_FRC = 13,
        SHUNT_AT_PEEP20 = 14,
        LC = 15,
        LP = 16,
        GP = 17,
        GC = 18,
        SAO2_TARGET = 19,
        HGRAD = 20,
        DAR_REM = 21,
        RTOT = 24,
        CRS_F = 25,
        VD_FACTOR = 26,
        PMUS_OPENLOOP = 27,
        TI_EQN_B = 28,
        CRS_LINEAR = 30,
        RIN = 31,
        REX = 32,
        F = 33,
        PMAX = 34,
        EXPEFFORT = 35,
        TI_TTOT_RATIO = 36,
        PCHAMBER_N1 = 37,
        PCHAMBER_N2 = 38,
        PCHAMBER_SWS = 39,
        RUPPER = 40,
        RLOWER = 41,
        RASL = 42
    }

    internal class Patient
    {
        // Class constructor
        public Patient(string patient_type, PatientPosture posture) 
        {
            LoadPatientCatalog(patient_type);
            SetLateralPosture((double) posture);
        }

        // Path for catalog of patient variables
        static string Pt_Catalog_path = "AE_Pt_Catalog.csv";

        // Patient catalog variables
        public string? Reference { set; get; }
        public double Pathology;
        public double Pathology_Severity_0_3;
        public double Height_cm;
        public double Weight_kg;
        public double Age_yr;
        public double Daytime_CO2_mmHg;
        public double Pchamber_REM_cmH2O;
        public double MIP_cmH2O;
        public double MIP_preserved_in_REM_percent;
        public double RQ;
        public double SNS_Elevation_f;
        public double FRC_perc_Normal;
        public double Shunt_At_FRC_perc;
        public double Shunt_at_PEEP20perc;
        public double lc;
        public double lp;
        public double Gp;
        public double Gc;
        public double SaO2_target;
        public double hGrad;
        public double Dar_REM;
        public double Rtot_f;
        public double Crs_f;
        public double Vd_factor;
        public double Pmus_Open;
        public double Ti_eqn_b;
        public double Crs_linear_cat;
        public double Rin;
        public double Rex;
        public double f;
        public double Pmax;
        public double ExpEffort_perc;
        public double Ti_Ttot_ratio_perc;
        public double Rupper;
        public double Rlower;
        public double Rasl;
        public double lateral_posture;

        // Aux block output variables
        public double K_863;
        public double MBW_kg;
        public double QTissue_LPS;
        public double PtRate_nominal;
        public double MR_basal_kcalPerDay;
        public double Roronasal;
        public double Rlower_insp;
        public double Rlower_exp;
        public double Rinsp_nominal;
        public double Rexp_nominal;
        public double VD_anat_normal;
        public double Vic;
        public double V_FRC;
        public double Crs_linear;
        public double UA_supine_factor;

        // Main Model output variables
        public double Dchemo;
        public double Pmus;
        public double SaO2;
        public double PmCO2;


        public void LoadPatientCatalog(string patient_type)
        {
            var parser = new Microsoft.VisualBasic.FileIO.TextFieldParser(Pt_Catalog_path);
            parser.TextFieldType = Microsoft.VisualBasic.FileIO.FieldType.Delimited;
            parser.SetDelimiters(",");
            while (!parser.EndOfData)
            {
                // Parse row
                string[] fields = parser.ReadFields()!;
                // Search for desired patient model
                if (fields[(int)PtCatIndexNum.REFERENCE] != patient_type)
                {
                    continue;
                }
                else
                {
                    Pathology = Convert.ToDouble(fields[(int)PtCatIndexNum.PATHOLOGY]);
                    Pathology_Severity_0_3 = Convert.ToDouble(fields[(int)PtCatIndexNum.PATHOLOGY_SEVERITY]);
                    Height_cm = Convert.ToDouble(fields[(int)PtCatIndexNum.HEIGHT_CM]);
                    Weight_kg = Convert.ToDouble(fields[(int)PtCatIndexNum.WEIGHT_KG]);
                    Age_yr = Convert.ToDouble(fields[(int)PtCatIndexNum.AGE_YR]);
                    Daytime_CO2_mmHg = Convert.ToDouble(fields[(int)PtCatIndexNum.DAYTIME_CO2_MMHG]);
                    Pchamber_REM_cmH2O = Convert.ToDouble(fields[(int)PtCatIndexNum.PCHAMBER_REM]);
                    MIP_cmH2O = Convert.ToDouble(fields[(int)PtCatIndexNum.MIP_CM_H2O]);
                    MIP_preserved_in_REM_percent = Convert.ToDouble(fields[(int)PtCatIndexNum.MIP_PRESERVED_IN_REM]);
                    RQ = Convert.ToDouble(fields[(int)PtCatIndexNum.RQ]);
                    SNS_Elevation_f = Convert.ToDouble(fields[(int)PtCatIndexNum.SNS_ELEVATION]);
                    FRC_perc_Normal = Convert.ToDouble(fields[(int)PtCatIndexNum.FRC_PERC_NORM]);
                    Shunt_At_FRC_perc = Convert.ToDouble(fields[(int)PtCatIndexNum.SHUNT_AT_FRC]);
                    Shunt_at_PEEP20perc = Convert.ToDouble(fields[(int)PtCatIndexNum.SHUNT_AT_PEEP20]);
                    lc = Convert.ToDouble(fields[(int)PtCatIndexNum.LC]);
                    lp = Convert.ToDouble(fields[(int)PtCatIndexNum.LP]);
                    Gp = Convert.ToDouble(fields[(int)PtCatIndexNum.GP]);
                    Gc = Convert.ToDouble(fields[(int)PtCatIndexNum.GC]);
                    SaO2_target = Convert.ToDouble(fields[(int)PtCatIndexNum.SAO2_TARGET]);
                    hGrad = Convert.ToDouble(fields[(int)PtCatIndexNum.HGRAD]);
                    Dar_REM = Convert.ToDouble(fields[(int)PtCatIndexNum.DAR_REM]);
                    Rtot_f = Convert.ToDouble(fields[(int)PtCatIndexNum.RTOT]);
                    Crs_f = Convert.ToDouble(fields[(int)PtCatIndexNum.CRS_F]);
                    Vd_factor = Convert.ToDouble(fields[(int)PtCatIndexNum.VD_FACTOR]);
                    Ti_eqn_b = Convert.ToDouble(fields[(int)PtCatIndexNum.TI_EQN_B]);
                    Crs_linear = Convert.ToDouble(fields[(int)PtCatIndexNum.CRS_LINEAR]);
                    Rin = Convert.ToDouble(fields[(int)PtCatIndexNum.RIN]);
                    Rex = Convert.ToDouble(fields[(int)PtCatIndexNum.REX]);
                    f = Convert.ToDouble(fields[(int)PtCatIndexNum.F]);
                    Pmax = Convert.ToDouble(fields[(int)PtCatIndexNum.PMAX]);
                    ExpEffort_perc = Convert.ToDouble(fields[(int)PtCatIndexNum.EXPEFFORT]);
                    Ti_Ttot_ratio_perc = Convert.ToDouble(fields[(int)PtCatIndexNum.TI_TTOT_RATIO]);
                    Rupper = Convert.ToDouble(fields[(int)PtCatIndexNum.RUPPER]);
                    Rlower = Convert.ToDouble(fields[(int)PtCatIndexNum.RLOWER]);
                    Rasl = Convert.ToDouble(fields[(int)PtCatIndexNum.RASL]);
                }
            }
        }
        public void SetLateralPosture(double posture)
        {
            lateral_posture = posture;
        }
    }
}
