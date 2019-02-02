using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebMedico.Models
{
    public class ParametersViewModels
    {
        public string MedicationDate { get; set; }
        public string MedicationTime { get; set; }
        public string Observation { get; set; }
    }

    public class ColesterolViewModel: ParametersViewModels
    {
        public string Total { get; set; }
        public string Trigliceridos { get; set; }
        public string Hdl { get; set; }
        public string Ldl { get; set; }
    }

    public class InsulinaViewModel : ParametersViewModels
    {
        public string Puid { get; set; }
        public string AdminUnits { get; set; }
        public string MomentValue { get; set; }
        
    }


    public class GlicemiaViewModel : ParametersViewModels
    {
        public string Capilar { get; set; }
        public string MomentValue { get; set; }
        public string MomentDescription { get; set; }
        public string MomentName { get; set; }
        public string TypeDescription { get; set; }
        public string TypeName { get; set; }

    }

    public class WeightMeasuresViewModel : ParametersViewModels
    {
        public string Weight { get; set; }
        public string Chest { get; set; }
        public string Waist { get; set; }
        public string Hips { get; set; }
        public string LeftLeg { get; set; }
        public string RightLeg { get; set; }
        public string LeftArm { get; set; }
        public string RightArm { get; set; }        
    }

    public class BloodPressureViewModel : ParametersViewModels
    {
        public string HeartRate { get; set; }
        public string Max { get; set; }
        public string Min { get; set; }       
    }

    public class DefaultViewModel : ParametersViewModels
    {
        public string Value { get; set; }
    }
}