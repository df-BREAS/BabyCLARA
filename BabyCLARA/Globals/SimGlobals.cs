using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BabyCLARA.Globals
{
    internal class SimGlobals
    {
        public SimGlobals(double maxTime) 
        {
            MaxSimTime = maxTime;
        }

        public double MaxSimTime;

        readonly public double SaO2Emergency = 90;
        readonly public double SaO2Offset = 97;
        readonly public double DriveScaling = 1;
        readonly public double Dmin = 1.5;
        readonly public double Minimum_wakefulness_drive = 2;
        readonly public double T1_h = 1;
        readonly public double T2_h = 2;
        readonly public double Td_l_to_p_Khoo1990 = 1;
        readonly public double Tc_Khoo1982 = 7.1;
        readonly public double Tp_Khoo1982 = 4;
        readonly public double KCO2 = 0.0057;
        readonly public double SCO2 = 0.0043;
        readonly public double Qtotal_LPS = 0.1;
        readonly public double Qbrain_LPMperKg = 0.5;
        readonly public double KbCO2 = 0.0036;
        readonly public double MRbCO2 = 0.05167;
        readonly public double VtCO2 = 6;
        readonly public double VtO2 = 7.7;
        public double VarRateBaseFoffset = 3;
        readonly public double Gw_DtoPmus_tweak = 2.7;
        public int ApneaCtrl = 0;
        public int StopBreathCtrl = 0;
        public int NoiseCtrl = 1;
        readonly public double BodyTemp = 37;
        readonly public double Alveolar_RH = 100;
        readonly public double Pbar = 759.8513;
    }
}
