using APSIM.Core;
using APSIM.Shared.Utilities;
using Models.Core;
using Models.Interfaces;
using Models.PMF;
using Models.PMF.Phen;
using Models.Soils;
using System;

namespace Models.Functions
{
    /// <summary>This class calculates the impact of soil moisture on germination.</summary>
    [Serializable]
    [Description("Returns the reduction factor on germination duration")]
    [ViewName("UserInterface.Views.PropertyView")]
    [PresenterName("UserInterface.Presenters.PropertyPresenter")]
    [ValidParent(ParentType = typeof(GerminatingPhaseSoil))]

    public class GerminatingWaterResponse : Model//, IFunction
    {
        [Link] Plant Plant = null;
        //[Link] GerminatingPhaseSoil GerminatingPhaseSoil = null;
        [Link] private IPhysical SoilPhysical = null;
        [Link] private ISoilWater WaterBalance = null;
        //[Link] private ISoilTemperature soilTemperature = null;
        //[Link] private CERESSoilTemperature SoilTemperature = null;

        /// <summary>soil water response varies with T, f0(T) </summary>
        [Link(Type = LinkType.Child, ByName = true)]
        private IFunction WaterResponseT0 = null;

        /// <summary>soil water response varies with T, f1(T) </summary>
        [Link(Type = LinkType.Child, ByName = true)]
        private IFunction WaterResponseT1 = null;

        /// <summary>Germination duration reduction rate due to water stress</summary>
        public double GerminationDurationFW { get; set; }

        /// <summary>SW when Water potential = -0.5</summary>
        private double WP1 { get; set; }

        /// <summary>FW when Water potential = -0.5</summary>
        private double FWP1 { get; set; }

        /// <summary>FW when SW = LL15 </summary>
        private double FW15 { get; set; }

        /// <summary>Called at the start of each day</summary>
        /// <param name="sender">Plant.cs</param>
        /// <param name="e">Event arguments</param>
        [EventSubscribe("StartOfDay")]
        private void OnStartOfDay(object sender, EventArgs e)
        {
            int i = SoilUtilities.LayerIndexOfDepth(SoilPhysical.Thickness, Plant.SowingData.Depth);
            // Calculate soil water content when Water Potential -0.5, assuming DUL is WP = 0, LL15 is WP = 1.5
            WP1  = SoilPhysical.LL15mm[i] + (SoilPhysical.DULmm[i] - SoilPhysical.LL15mm[i]) * 2/3;
            FWP1 = WaterResponseT1.Value();
            FW15  = Math.Max(0, FWP1 * (SoilPhysical.LL15mm[i] -  WaterResponseT0.Value()) / (WP1 - WaterResponseT0.Value()));

            if (WaterBalance.SWmm[i] <= SoilPhysical.LL15mm[i])
             {
                //GerminationDurationFW = 0.5 * WaterBalance.SWmm[i] / SoilPhysical.LL15mm[i];
                GerminationDurationFW = Math.Max(0, FW15 * WaterBalance.SWmm[i] / SoilPhysical.LL15mm[i]);
             }
            else if (WaterBalance.SWmm[i] >= SoilPhysical.DULmm[i])
             {
               GerminationDurationFW = 1;
             }
            else if (WaterBalance.SWmm[i] < SoilPhysical.DULmm[i] && WaterBalance.SWmm[i] > WP1)
            {                 
               GerminationDurationFW = (1 - FWP1) * (WaterBalance.SWmm[i] - WP1) / (SoilPhysical.DULmm[i] - WP1) + FWP1;
                
            }
                else if (WaterBalance.SWmm[i] <= WP1 && WaterBalance.SWmm[i] > SoilPhysical.LL15mm[i])
            {
               GerminationDurationFW = (FWP1 - FW15) * (WaterBalance.SWmm[i] - SoilPhysical.LL15mm[i]) / (WP1- SoilPhysical.LL15mm[i]) + FW15;
             }
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