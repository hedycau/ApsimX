using System;
using System.Collections.Generic;
using System.Security.Policy;
using APSIM.Shared.Documentation;
using Models.Core;
using Models.PMF.Phen;

namespace Models.Functions
{
    /// <summary>This class calculates the impact of dwarfing genes on max coleoptile length and max leaf size.</summary>
    [Serializable]
    [Description("Returns the reduction factor on coleoptile length")]
    [ViewName("UserInterface.Views.PropertyView")]
    [PresenterName("UserInterface.Presenters.PropertyPresenter")]
    [ValidParent(ParentType = typeof(EmergingPhaseColeoptile))]
    public class DwarfingGeneResponse : Model
    {
        /// <summary>Dwarfing Genes options</summary>
        public enum DwarfingGenesOption
        {
            ///<summary>Tall wheat</summary>
            rht,
            ///<summary>GAS dwarfing gene</summary>
            Rht8,
            ///<summary>GAS dwarfing gene</summary>
            Rht13,
            ///<summary>GAI dwarfing gene</summary>
            Rht18,
            ///<summary>GAI dwarfing gene</summary>
            Rht1,
            ///<summary>GAI dwarfing gene</summary>
            Rht2,
            ///<summary>Two GAI dwarfing genes</summary>
            Rht1_Rht2
        };

        ///<summary>Selected DwarfingGeneType</summary>
        [Description("What dwarfing genes does the genotype have?")]
        public DwarfingGenesOption DwarfingGeneType { get; set; }

        ///<summary>The reduction factor on coleoptile length</summary>
        public double ColeoptileReductionFactor { get; set; }

        ///<summary>The impact factor on coleoptile elongation rate</summary>
        public double ColeoptileElongationRate { get; set; }

        ///<summary>The reduction factor on leaf size</summary>
        public double LeafSizeReductionFactor { get; set; }

        /// <summary>Gets the coleoptile length reduction factor based on the selected dwarfing gene type.</summary>
        [EventSubscribe("Sowing")]
        private void OnSowing(object sender, EventArgs e)
        {
            // Set the ReductionFactor property based on the selected dwarfing gene type
            ColeoptileReductionFactor = DwarfingGeneType switch
            {
                DwarfingGenesOption.rht => 1,
                DwarfingGenesOption.Rht8 => 1,
                DwarfingGenesOption.Rht13 => 1,
                DwarfingGenesOption.Rht1 => 0.75,
                DwarfingGenesOption.Rht2 => 0.75,
                DwarfingGenesOption.Rht1_Rht2 => 0.5,
                _ => throw new ArgumentException($"Unsupported genetic type: {DwarfingGeneType}")
            };

            ColeoptileElongationRate = DwarfingGeneType switch
            {
                DwarfingGenesOption.rht => 0.78, //1.248 ,// 1.56,
                DwarfingGenesOption.Rht8 => 0.815, //1.304, //1.63,
                DwarfingGenesOption.Rht13 => 0.815, //1.304, //1.63,
                DwarfingGenesOption.Rht18 => 0.765, //1.224, //1.53,
                DwarfingGenesOption.Rht1 => 0.535, //0.856, //1.07,
                DwarfingGenesOption.Rht2 => 0.525,//0.84, //1.05,
                DwarfingGenesOption.Rht1_Rht2 => 0.454, //0.7264, //0.908,
                _ => throw new ArgumentException($"Unsupported genetic type: {DwarfingGeneType}")
            };

            LeafSizeReductionFactor = DwarfingGeneType switch
            {
                DwarfingGenesOption.rht => 1.25,
                DwarfingGenesOption.Rht8 => 1.25,
                DwarfingGenesOption.Rht13 => 1.25,
                DwarfingGenesOption.Rht1 => 1.0625,
                DwarfingGenesOption.Rht2 => 1.0625,
                DwarfingGenesOption.Rht1_Rht2 => 1,
                _ => throw new ArgumentException($"Unsupported genetic type: {DwarfingGeneType}")
            };
        }

    }
}

