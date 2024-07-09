using System;
using System.Collections.Generic;
using System.Security.Policy;
using APSIM.Shared.Documentation;
using Models.Core;

namespace Models.Functions
{
    /// <summary>This class calculates the impact of dwarfing genes on coleoptile length.</summary>
    [Serializable]
    [Description("Returns the reduction factor on coleoptile length")]
    [ViewName("UserInterface.Views.PropertyView")]
    [PresenterName("UserInterface.Presenters.PropertyPresenter")]
    public class DwarfingGeneResponse : Model, IFunction
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
            Rht1,
            ///<summary>GAI dwarfing gene</summary>
            Rht2,
            ///<summary>Two GAI dwarfing genes</summary>
            Rht1Rht2
        };

        ///<summary>Selected DwarfingGeneType</summary>
        [Description("What dwarfing genes does the genotype have?")]
        public DwarfingGenesOption DwarfingGeneType { get; set; }

        ///<summary>The reduction factor</summary>
        public double ReductionFactor { get; private set; }

        /// <summary>Gets the coleoptile length reduction factor based on the selected dwarfing gene type.</summary>
        public double Value(int arrayIndex = -1)
        {
            // Set the ReductionFactor property based on the selected dwarfing gene type
            ReductionFactor = DwarfingGeneType switch
            {
                DwarfingGenesOption.rht => 1,
                DwarfingGenesOption.Rht8 => 1,
                DwarfingGenesOption.Rht13 => 1,
                DwarfingGenesOption.Rht1 => 0.75,
                DwarfingGenesOption.Rht2 => 0.75,
                DwarfingGenesOption.Rht1Rht2 => 0.5,
                _ => throw new ArgumentException($"Unsupported genetic type: {DwarfingGeneType}")
            };

            return ReductionFactor;
        }

        /// <summary>
        /// Document the model.
        /// </summary>
        public override IEnumerable<ITag> Document()
        {
            yield return new Paragraph($"*{Name} is a function of coleoptile reduction factor determined by dwarfing genes.");
        }

    }
}

