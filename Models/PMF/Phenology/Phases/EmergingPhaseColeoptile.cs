using System;
using APSIM.Core;
using APSIM.Shared.Utilities;
using Models.Core;
using Models.Functions;
using Newtonsoft.Json;

namespace Models.PMF.Phen
{
    /// <summary>
    /// This phase goes from a start stage to an end stage and simulates time to
    /// emergence as a function of sowing depth, dwarfing gene type, max coleoptile length, coleoptile elongation rate and phyllochron.
    /// Progress toward emergence is driven by a thermal time and actual elongation rate, the maximum growth duration is one phyllochron.
    /// </summary>
    [Serializable]
    [ViewName("UserInterface.Views.PropertyView")]
    [PresenterName("UserInterface.Presenters.PropertyPresenter")]
    [ValidParent(ParentType = typeof(Phenology))]
    public class EmergingPhaseColeoptile : Model, IPhase
    {
        // 1. Links
        //----------------------------------------------------------------------------------------------------------------
        [Link] Plant Plant = null;

        /// <summary>The coleoptile length dwarfingGene response function</summary>
        [Link(Type = LinkType.Child, ByName = true)]
        DwarfingGeneResponse DwarfingGeneResponse = null;

        // Coleoptile parameters
        [Link(Type = LinkType.Child, ByName = true)]
        EmergingColeoptileParameter EmergingColeoptileParameter = null;

        //Maximum growth duration - one phyllochron
        [Link(Type = LinkType.Child, ByName = true)]
        IFunction MaxGrowthDuration = null;

        //Coleoptile growth rate reduction factor
        [Link(Type = LinkType.Child, ByName = true)]
        ColeoptileElongationRateFactor ColeoptileElongationRateFactor = null;

        //Leaf Thermal time calculated by soil temperature
        [Link(Type = LinkType.Child, ByName = true)]
        IFunction SoilThermalTime = null;

        //Copeoptile Thermal time calculated by soil temperature
        [Link(Type = LinkType.Child, ByName = true)]
        IFunction ColeoptileSoilThermalTime = null;

        //First Leaf Length Rate according to seed weight Zhao et al(2019) JEB
        [Link(Type = LinkType.Child, ByName = true)]
        IFunction FirstLeafLengthRate = null;

        //First Leaf Width Rate according to seed weight Zhao et al(2019) JEB
        [Link(Type = LinkType.Child, ByName = true)]
        IFunction FirstLeafWidthRate = null;
        

        //2. Public properties
        //-----------------------------------------------------------------------------------------------------------------

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
        public double FractionComplete { get; set; }

        //parameters used in calculation, can be set private later
        /// <summary>Delta coleoptile length </summary>
        [Units("mm")]
        public double DeltaColeoptileLength { get; set; }

        /// <summary>Actual coleoptile length </summary>
        [Units("mm")]
        public double ColeoptileLength { get; set; }

        /// <summary>Actual maximum coleoptile+leaf length </summary>
        [Units("mm")]
        public double ActualMaxLength { get; set; }

        /// <summary>Actual maximum coleoptile length </summary>
        [Units("mm")]
        public double ActualMaxColeoptileLength { get; set; }

        /// <summary>Actual Coleoptile growth rate </summary>
        [Units("mm/oCd")]
        public double ColeoptileGrowthRate { get; set; }

        /// <summary>Accumulate leaf TT for this phase</summary>
        private double AccumLeafTTthisPhase { get; set; }
        
        /// <summary>Accumulate coleoptile TT for this phase</summary>
        private double AccumColeoptileTTthisPhase { get; set; }

        /// <summary>The index to calculate the first day completeing lag phase</summary>
        public double Lagphasecompleteday { get; set; }

        /// <summary>Emergence probability calcualted on the ratio of sowing depth to coleoptile depth</summary>
        public double probEmergence { get; private set; }

        //parameters used in leaf underground grow calculation, can be set private later
        /// <summary>Delta first leaf length </summary>
        [Units("mm")]
        public double DeltaLeafLength { get; set; }

        /// <summary>Actual shoot length (coleoptile + first leaf) </summary>
        [Units("mm")]
        public double TotalShootLength { get; set; }

        /// <summary>First Leaf Growth Rate </summary>
        [Units("mm/oCd")]
        public double FirstLeafGrowthRate { get; set; }

        /// <summary>First Leaf Width rate - use to define initial leaf area at emergence </summary>
        [Units("")]
        public double FirstLeafWidthRateValue { get; set; }


        //3. Public method
        //-----------------------------------------------------------------------------------------------------------------

        /// <summary>Do our timestep development</summary>
        public bool DoTimeStep(ref double propOfDayToUse)
        {
            FirstLeafWidthRateValue = FirstLeafWidthRate.Value();
            bool proceedToNextPhase = false;
            ActualMaxLength = EmergingColeoptileParameter.MaxColeoptileLength * DwarfingGeneResponse.ColeoptileReductionFactor + EmergingColeoptileParameter.FirstLeafLength/2;
            ActualMaxColeoptileLength = EmergingColeoptileParameter.MaxColeoptileLength * DwarfingGeneResponse.ColeoptileReductionFactor;
            FirstLeafGrowthRate = EmergingColeoptileParameter.FirstLeafLength * FirstLeafLengthRate.Value()/ MaxGrowthDuration.Value();

            if (AccumColeoptileTTthisPhase >= EmergingColeoptileParameter.ColeoptileLagphase)                
            {
                if (AccumLeafTTthisPhase <= MaxGrowthDuration.Value())
                {
                    Lagphasecompleteday += 1;

                    ColeoptileGrowthRate = DwarfingGeneResponse.ColeoptileElongationRate * ColeoptileElongationRateFactor.ColeoptileGrowthRateReductionFactor;
                    //ColeoptileGrowthRate = ActualMaxColeoptileLength / MaxGrowthDuration.Value() * ColeoptileElongationRateFactor.ColeoptileGrowthRateReductionFactor;

                    if (Lagphasecompleteday == 1)
                    {
                        DeltaColeoptileLength = (AccumColeoptileTTthisPhase - EmergingColeoptileParameter.ColeoptileLagphase + ColeoptileSoilThermalTime.Value()) * ColeoptileGrowthRate;
                        //Plant.Phenology.thermalTime.Value()
                    }
                    else
                    {
                        DeltaColeoptileLength = ColeoptileSoilThermalTime.Value() * ColeoptileGrowthRate;
                        //ColeoptileElongationRateFactor.SoilTemperatureColeoptileTip / ColeoptileGrowthRate;
                        //Plant.Phenology.thermalTime.Value()
                    }
                    ColeoptileLength = ColeoptileLength + DeltaColeoptileLength;
                    
                } else if (AccumLeafTTthisPhase > MaxGrowthDuration.Value())
                {
                    DeltaLeafLength = FirstLeafGrowthRate * ColeoptileElongationRateFactor.ColeoptileGrowthRateReductionFactor * SoilThermalTime.Value() /2; //assume the leaf is folded
                }
                TotalShootLength = ColeoptileLength + DeltaLeafLength;
                TotalShootLength = Math.Min(TotalShootLength, Plant.SowingData.Depth); //Math.Min(Math.Min(TotalShootLength, Plant.SowingData.Depth), ActualMaxLength);
                ColeoptileLength = Math.Min(ColeoptileLength, Plant.SowingData.Depth); //Math.Min(Math.Min(ColeoptileLength, Plant.SowingData.Depth), ActualMaxColeoptileLength);
            }


            AccumColeoptileTTthisPhase = AccumColeoptileTTthisPhase + ColeoptileElongationRateFactor.SoilTemperatureColeoptileTip;//Plant.Phenology.thermalTime.Value()
            AccumLeafTTthisPhase = AccumLeafTTthisPhase + SoilThermalTime.Value();

            //if (Plant.SowingData.Depth <= ActualMaxLength)
             //{
                if (TotalShootLength >= Plant.SowingData.Depth)
                {
                    proceedToNextPhase = true;
                }
                else if (AccumLeafTTthisPhase >= 2 * MaxGrowthDuration.Value())
                {
                    proceedToNextPhase = true;
                }
                else
                    proceedToNextPhase = false;
             //}
            
            //if (Plant.SowingData.Depth > ActualMaxLength)
            //{
            //    if (AccumLeafTTthisPhase >= 2 * MaxGrowthDuration.Value())
             //   {
             //       proceedToNextPhase = true;
              //  }
               // else
                //    proceedToNextPhase = false;

            //}
            return proceedToNextPhase;
        }
        /// <summary>Reset phase</summary>
        public void ResetPhase()
        {
            DeltaColeoptileLength = 0;
            ColeoptileLength = 0;
            ActualMaxLength = 0;
            ColeoptileGrowthRate = 0;
            AccumLeafTTthisPhase = 0;
            AccumColeoptileTTthisPhase = 0;
            Lagphasecompleteday = 0;
            DeltaLeafLength = 0;
            TotalShootLength = 0;
            FirstLeafGrowthRate = 0;
        }


        //4 Private method
        //-----------------------------------------------------------------------------------------------------------------
        /// <summary>Called when [simulation commencing].</summary>
        [EventSubscribe("Commencing")]
        private void OnSimulationCommencing(object sender, EventArgs e)
        {
            ResetPhase();
        }

        /// <summary>Reset Plant density according to sowing depth and coleoptile length </summary>
        [EventSubscribe("DoDailyInitialisation")]
        private void OnDoDailyInitialisation(object sender, EventArgs e)
        {
            if (Plant.Phenology.CurrentStageName == "Emergence")
            {
                if (ColeoptileLength < Plant.SowingData.Depth) 
                { 
                double belta = Plant.SowingData.Depth / ColeoptileLength;

                if (belta > 2)
                {
                    probEmergence = 0;
                }
                else if (belta < 0.6)
                {
                    probEmergence = 1;
                }
                else
                {
                    probEmergence = (2 - belta) / (2 - 0.6);
                }
                Plant.Population = Math.Max(1, Plant.SowingData.Population * probEmergence);
                Plant.SowingData.Population = Math.Max(1, Plant.SowingData.Population * probEmergence);
                }
            }
        }
    }
}
