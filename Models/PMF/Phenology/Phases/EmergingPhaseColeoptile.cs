using System;
using System.Collections.Generic;
using System.Text;
using APSIM.Shared.Documentation;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Spreadsheet;
using Models.Core;
using Models.Functions;
using Newtonsoft.Json;
using System.Diagnostics;
using Models.PMF;


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

        //Thermal time calculated by soil temperature
        [Link(Type = LinkType.Child, ByName = true)]
        IFunction SoilThermalTime = null;

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
        public double FractionComplete { get { return 0; } }

        //parameters used in calculation, can be set private later
        /// <summary>Delta coleoptile length </summary>
        [Units("mm")]
        public double DeltaColeoptileLength { get; set; }

        /// <summary>Actual coleoptile length </summary>
        [Units("mm")]
        public double ColeoptileLength { get; set; }

        /// <summary>Actual maximum coleoptile length </summary>
        [Units("mm")]
        public double ActualMaxLength { get; set; }

        /// <summary>Actual Coleoptile growth rate </summary>
        [Units("mm/oCd")]
        public double ColeoptileGrowthRate { get; set; }

        /// <summary>Accumulate TT for this phase</summary>
        private double AccumTTthisPhase { get; set; }

        /// <summary>The index to calculate the first day completeing lag phase</summary>
        public double Lagphasecompleteday { get; set; }

        /// <summary>Emergence probability calcualted on the ratio of sowing depth to coleoptile depth</summary>
        public double probEmergence { get; private set; }
        

        //3. Public method
        //-----------------------------------------------------------------------------------------------------------------

        /// <summary>Do our timestep development</summary>
        public bool DoTimeStep(ref double propOfDayToUse)
        {
            
            bool proceedToNextPhase = false;

            if (AccumTTthisPhase >= EmergingColeoptileParameter.ColeoptileLagphase)
            {
                Lagphasecompleteday += 1;
                
                ColeoptileGrowthRate = EmergingColeoptileParameter.MaxColeoptileGrowthRate * ColeoptileElongationRateFactor.ColeoptileGrowthRateReductionFactor;

                if (Lagphasecompleteday == 1)
                {
                    DeltaColeoptileLength = (AccumTTthisPhase - EmergingColeoptileParameter.ColeoptileLagphase + SoilThermalTime.Value()) / ColeoptileGrowthRate;
                    //Plant.Phenology.thermalTime.Value()
                }
                else
                {
                    DeltaColeoptileLength = SoilThermalTime.Value()/ ColeoptileGrowthRate;
                    //ColeoptileElongationRateFactor.SoilTemperatureColeoptileTip / ColeoptileGrowthRate;
                    //Plant.Phenology.thermalTime.Value()
                }
                ColeoptileLength = ColeoptileLength + DeltaColeoptileLength;
            }

            ActualMaxLength = Math.Min(Plant.SowingData.Depth, EmergingColeoptileParameter.MaxColeoptileLength * DwarfingGeneResponse.ColeoptileReductionFactor);
            AccumTTthisPhase = AccumTTthisPhase + ColeoptileElongationRateFactor.SoilTemperatureColeoptileTip;//Plant.Phenology.thermalTime.Value()

            if (ColeoptileLength >= ActualMaxLength) 
                {
                proceedToNextPhase = true;
                }
            else if (AccumTTthisPhase >= MaxGrowthDuration.Value())
               {
                proceedToNextPhase = true;
               }
            else
                proceedToNextPhase = false;

            return proceedToNextPhase;
        }

        /// <summary>Reset phase</summary>
        public void ResetPhase()
        {
            DeltaColeoptileLength = 0;
            ColeoptileLength = 0;
            ActualMaxLength = 0;
            ColeoptileGrowthRate = 0;
            AccumTTthisPhase = 0;
            Lagphasecompleteday = 0;
        }
        //4 Private method
        //-----------------------------------------------------------------------------------------------------------------
        /// <summary>Reset Plant density according to sowing depth and coleoptile length </summary>
        [EventSubscribe("DoDailyInitialisation")]
        private void OnDoDailyInitialisation(object sender, EventArgs e)
        {
            if (Plant.Phenology.CurrentStageName == "Emergence")
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
                Plant.Population = Plant.SowingData.Population * probEmergence;
                Plant.SowingData.Population = Plant.SowingData.Population * probEmergence;
            }
        }
        /// <summary>
        /// Document the model.
        /// </summary>
        public override IEnumerable<ITag> Document()
        {
            // Write description of this class.
            StringBuilder text = new StringBuilder($"This phase goes from {Start} to {End}. ");
            text.Append("The phase ends when coleoptile length has reaches its possible max lenght, either the max coleoptile length or sowing depth, or growht duration reaches one phyllochron. ");
            yield return new APSIM.Shared.Documentation.Paragraph(text.ToString());
        }


    }




}
