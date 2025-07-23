using System;
using APSIM.Core;
using APSIM.Shared.Utilities;
using Models.Core;
using Models.Functions;
using Models.Interfaces;
using Models.Soils;
using Newtonsoft.Json;

namespace Models.PMF.Phen
{
    /// <summary>
    /// This phase goes from a start stage to an end stage and assumes
    /// germination will be reached on TT 35oCd, adjusted by soil water availability."
    /// </summary>
    [Serializable]
    [ViewName("UserInterface.Views.PropertyView")]
    [PresenterName("UserInterface.Presenters.PropertyPresenter")]
    [ValidParent(ParentType = typeof(Phenology))]
    public class GerminatingPhaseSoil : Model, IPhase
    {
        // 1. Links
        //----------------------------------------------------------------------------------------------------------------
        [Link] private ISoilTemperature soilTemperature = null;
        [Link] private IPhysical SoilPhysical = null;
        [Link] Plant Plant = null;

        // Germinating parameters
        [Link(Type = LinkType.Child, ByName = true)]
        GerminatingParameter GerminatingParameter = null;

        /// <summary>Germinating Water Response and get soil temprature on sowing depth</summary>
        [Link(Type = LinkType.Child, ByName = true)]
        GerminatingWaterResponse GerminatingWaterResponse = null;

        /// <summary>Thermal time calculated by soil temperature</summary>
        [Link(Type = LinkType.Child, ByName = true)]
        public IFunction SoilThermalTime = null;

        //2. Public properties
        //-----------------------------------------------------------------------------------------------------------------

        /// <summary>Occurs when a plant is about to be sown.</summary>
        public event EventHandler SeedImbibed;

        /// <summary>The phenological stage at the start of this phase.</summary>
        [Description("Start")]
        public string Start { get; set; }

        /// <summary>The phenological stage at the end of this phase.</summary>
        [Models.Core.Description("End")]
        public string End { get; set; }

        /// <summary>Is the phase emerged from the ground?</summary>
        [Description("Is the phase emerged?")]
        public bool IsEmerged { get; set; } = false;

        /// <summary>Fraction of phase that is complete (0-1).</summary>
        [JsonIgnore]
        public double FractionComplete { get { return 0; } }

        //parameters used in calculation, can be set private later
        /// <summary>Accumulate TT for this phase</summary>
        private double AccumTTthisPhase { get; set; }

        /// <summary>Accumulate TT for germination target</summary>
        private double AccumTargetTT { get; set; }

        /// <summary>The index to calculate the first day completeing lag phase</summary>
        private double Dormancyphasecompleteday { get; set; }

        ///<summary>Soil temperature of the layer where the seed is</summary>
        public double SoilTemperatureSeed { get; set; }


        //3. Public method
        //-----------------------------------------------------------------------------------------------------------------

        /// <summary>Do our timestep development</summary>
        public bool DoTimeStep(ref double propOfDayToUse)
        {

            bool proceedToNextPhase = false;

            int i = SoilUtilities.LayerIndexOfDepth(SoilPhysical.Thickness, Plant.SowingData.Depth);
            SoilTemperatureSeed = soilTemperature.Value[i];

            if (AccumTTthisPhase >= GerminatingParameter.DormancyPhase)
            {
                Dormancyphasecompleteday += 1;

                if (Dormancyphasecompleteday == 1)
                {
                    AccumTargetTT = (AccumTTthisPhase - GerminatingParameter.DormancyPhase + SoilThermalTime.Value() * GerminatingWaterResponse.GerminationDurationFW);
                    //Plant.Phenology.thermalTime.Value()
                }
                else
                {
                    AccumTargetTT = AccumTargetTT + SoilThermalTime.Value() * GerminatingWaterResponse.GerminationDurationFW;
                    //ColeoptileElongationRateFactor.SoilTemperatureColeoptileTip / ColeoptileGrowthRate;
                    //Plant.Phenology.thermalTime.Value()
                }
            }

            AccumTTthisPhase = AccumTTthisPhase + SoilThermalTime.Value() * GerminatingWaterResponse.GerminationDurationFW;//Plant.Phenology.thermalTime.Value()

            if (AccumTargetTT >= GerminatingParameter.GerminationTarget)
            {
                proceedToNextPhase = true;
                doGermination(ref proceedToNextPhase, ref propOfDayToUse);
            }
            else
                proceedToNextPhase = false;

            return proceedToNextPhase;
        }

        /// <summary>Reset phase</summary>
        public void ResetPhase()
        {
            AccumTTthisPhase = 0;
            AccumTargetTT = 0;
            Dormancyphasecompleteday = 0;
        }
        // 4. Private methods
        //-----------------------------------------------------------------------------------------------------------------

        private void doGermination(ref bool proceedToNextPhase, ref double propOfDayToUse)
        {
            if (SeedImbibed != null)
                SeedImbibed.Invoke(this, new EventArgs());
            proceedToNextPhase = true;
            propOfDayToUse = 1;
        }
    }
}