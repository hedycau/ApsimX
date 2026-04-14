using System;
using Models.Core;
using Models.PMF;
using Models.PMF.Phen;
using APSIM.Shared.Utilities;
using Models.Soils;
using Models.Interfaces;

namespace Models.Functions
{
    /// <summary>This class calculates the impact of soil moisture and soil hardness on coleoptile growth rate.</summary>
    [Serializable]
    [Description("Returns the reduction factor on coleoptile elongation rate")]
    [ViewName("UserInterface.Views.PropertyView")]
    [PresenterName("UserInterface.Presenters.PropertyPresenter")]
    [ValidParent(ParentType = typeof(EmergingPhaseColeoptile))]

    public class ColeoptileElongationRateFactor : Model

    {
        [Link] EmergingPhaseColeoptile EmergingPhaseColeoptile = null;
        [Link] Plant Plant = null;
        [Link] private IPhysical SoilPhysical = null;
        [Link] private ISoilWater WaterBalance = null;
        [Link] private ISoilTemperature SoilTemperature = null;

        /// <summary>Coleoptile Growth Rate Reduction Factor </summary>
        public double ColeoptileGrowthRateReductionFactor { get; set; }

        /// <summary>The depth of coleoptile tip</summary>
        public double ColeoptileTipDepth { get; set; }

        /// <summary>The zone where the plant is growing</summary>
        public double ColeoptileGrowthRateFW { get; set; }

        /// <summary>The zone where the plant is growing</summary>
        public double ColeoptileGrowthRateFBulkDensity { get; set; }

        ///<summary>Critical Bulk Density for coleoptile growth</summary>
        [Description("Soil bulk density above this value will limit coleoptile growth")]
        [Units("g/cm3")]
        public double CriticalBDColeoptile { get; set; } = 1.5;

        ///<summary>Critical Bulk Density for coleoptile growth</summary>
        [Description("Soil bulk density above this value will stop coleoptile growth")]
        [Units("g/cm3")]
        public double MaxBDColeoptile { get; set; } = 2.65;

        ///<summary>Soil temperature of the layer where the coleoptile tip grow in</summary>
        public double SoilTemperatureColeoptileTip { get; set; }

        /// <summary>Called at the start of each day</summary>
        /// <param name="sender">Plant.cs</param>
        /// <param name="e">Event arguments</param>
        [EventSubscribe("StartOfDay")]
        private void OnStartOfDay(object sender, EventArgs e)
        {
            ColeoptileTipDepth = Plant.SowingData.Depth - EmergingPhaseColeoptile.ColeoptileLength;
            ColeoptileTipDepth = Math.Max(ColeoptileTipDepth, 0.00001);

            if (ColeoptileTipDepth > 0)
            {
                //Calculate soil moisture factor
                int j = SoilUtilities.LayerIndexOfDepth(SoilPhysical.Thickness, ColeoptileTipDepth);//soil temp and soil hardness-coleoptile tip layer
                int i = SoilUtilities.LayerIndexOfDepth(SoilPhysical.Thickness, Plant.SowingData.Depth);//soil moisture-seed layer
                SoilTemperatureColeoptileTip = SoilTemperature.Value[j];

                if (WaterBalance.SWmm[i] < SoilPhysical.LL15mm[i])                    
                {
                    ColeoptileGrowthRateFW = WaterBalance.SWmm[i] / SoilPhysical.LL15mm[i];                
                }       
                else if (WaterBalance.SWmm[i] > SoilPhysical.DULmm[i])
                { 
                    ColeoptileGrowthRateFW = (WaterBalance.SWmm[i] - SoilPhysical.SATmm[i])/(SoilPhysical.DULmm[i] - SoilPhysical.SATmm[i]) ; 
                }
                else 
                { 
                    ColeoptileGrowthRateFW = 1; 
                }

                ColeoptileGrowthRateFW = Math.Min(Math.Max(ColeoptileGrowthRateFW, 0), 1);

                //Calculate soil hardness factor
                if (SoilPhysical.BD[i] <= CriticalBDColeoptile)
                { 
                    ColeoptileGrowthRateFBulkDensity = 1; 
                }
                else if (SoilPhysical.BD[i] >= MaxBDColeoptile)
                { 
                    ColeoptileGrowthRateFBulkDensity = 0.000001; //coleoptile cannot elongation when the soil is too hard
                    throw new ArgumentException("Seed cannot emergence in this soil!");
                }
                else if (SoilPhysical.BD[i] > CriticalBDColeoptile & SoilPhysical.BD[i] < MaxBDColeoptile)
                { 
                    ColeoptileGrowthRateFBulkDensity =  (MaxBDColeoptile - SoilPhysical.BD[i])/(MaxBDColeoptile - CriticalBDColeoptile); 
                }
                else 
                { 
                    throw new ArgumentException("Soil BD may be not right!"); 
                }

                ColeoptileGrowthRateFBulkDensity = Math.Min(Math.Max(ColeoptileGrowthRateFBulkDensity, 0), 1);

                //Calculate actual elongation rate
                ColeoptileGrowthRateReductionFactor = ColeoptileGrowthRateFBulkDensity * ColeoptileGrowthRateFW;
                ColeoptileGrowthRateReductionFactor = Math.Min(Math.Max(ColeoptileGrowthRateReductionFactor, 0),1);

            }
        }
        

    }
}
