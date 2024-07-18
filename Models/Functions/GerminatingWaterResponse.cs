using System;
using Models.Core;
using Models.PMF;
using Models.PMF.Phen;
using APSIM.Shared.Utilities;
using Models.Soils;
using Models.Interfaces;

namespace Models.Functions
{
    /// <summary>This class calculates the impact of soil moisture on germination.</summary>
    [Serializable]
    [Description("Returns the reduction factor on germination duration")]
    [ViewName("UserInterface.Views.PropertyView")]
    [PresenterName("UserInterface.Presenters.PropertyPresenter")]
    [ValidParent(ParentType = typeof(Phenology))]
    public class GerminatingWaterResponse : Model//, IFunction
    {
        //[Link] Plant Plant = null;
        [Link] private IPhysical SoilPhysical = null;
        [Link] private ISoilWater WaterBalance = null;
        [Link] private ISoilTemperature SoilTemperature = null;

        ///<summary>Soil temperature of the layer where the seed is</summary>
        public double SoilTemperatureSeed { get; set; }

        /// <summary>Germination duration reduction rate due to water stress</summary>
        public double GerminationDurationFW { get; set; }

        ///<summary>Germination dormancy phase</summary>
        [Description("Dormancy phase")]
        [Units("oCd")]
        public double DormancyPhase { get; set; } = 0;

        /// <summary>Accumulate TT for this phase</summary>
        private double AccumTTthisPhase { get; set; }

        /// <summary>Called at the start of each day</summary>
        /// <param name="sender">Plant.cs</param>
        /// <param name="e">Event arguments</param>
        [EventSubscribe("StartOfDay")]
        private void OnStartOfDay(object sender, EventArgs e)
        {
            //if (Plant.Phenology.CurrentStageName == "Sowing")
            //{
            int i = 1;// SoilUtilities.LayerIndexOfDepth(SoilPhysical.Thickness, 50);// Plant.SowingData.Depth);
            
                SoilTemperatureSeed = SoilTemperature.Value[i];

                if (AccumTTthisPhase >= DormancyPhase)
                {

                    if (WaterBalance.SWmm[i] < SoilPhysical.LL15mm[i]) //SoilPhysical.LL15mm[i]
                    {
                        GerminationDurationFW = 0.5 * WaterBalance.SWmm[i] / SoilPhysical.LL15mm[i];
                    }
                    else if (WaterBalance.SWmm[i] > SoilPhysical.DULmm[i])
                    {
                        GerminationDurationFW = 1;
                    }
                    else
                    {
                        GerminationDurationFW = (1 - 0.5) * (WaterBalance.SWmm[i] - SoilPhysical.LL15mm[i]) / (SoilPhysical.DULmm[i] - SoilPhysical.LL15mm[i]);
                    }
                }
                AccumTTthisPhase = AccumTTthisPhase + SoilTemperatureSeed;
            //}
        }
   //     /// <summary>
    //    /// Returns the the value of germination duratin FW.
     //   /// </summary>
      //  /// <param name="arrayIndex">Ignored.</param>
     //  public double Value(int arrayIndex = -1)
      //  {
       //     return GerminationDurationFW;
        //}
    }

}