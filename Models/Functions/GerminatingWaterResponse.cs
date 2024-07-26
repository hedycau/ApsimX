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
    [ValidParent(ParentType = typeof(GerminatingPhaseSoil))]

    public class GerminatingWaterResponse : Model//, IFunction
    {
        [Link] Plant Plant = null;
        //[Link] GerminatingPhaseSoil GerminatingPhaseSoil = null;
        [Link] private IPhysical SoilPhysical = null;
        [Link] private ISoilWater WaterBalance = null;
        //[Link] private ISoilTemperature soilTemperature = null;


        //[Link] private CERESSoilTemperature SoilTemperature = null;



        /// <summary>Germination duration reduction rate due to water stress</summary>
        public double GerminationDurationFW { get; set; }

        /// <summary>Called at the start of each day</summary>
        /// <param name="sender">Plant.cs</param>
        /// <param name="e">Event arguments</param>
        [EventSubscribe("StartOfDay")]
        private void OnStartOfDay(object sender, EventArgs e)
        {
            int i = SoilUtilities.LayerIndexOfDepth(SoilPhysical.Thickness, Plant.SowingData.Depth);

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
               GerminationDurationFW = (1 - 0.5) * (WaterBalance.SWmm[i] - SoilPhysical.LL15mm[i]) / (SoilPhysical.DULmm[i] - SoilPhysical.LL15mm[i]) + 0.5;
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