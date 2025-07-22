using System;
using Models.Core;
using Models.PMF.Phen;

namespace Models.Functions
{
    /// <summary>
    /// Coleoptile growth parameters.
    /// </summary>
    [Serializable]
    [Description("Coleoptile growth parameters")]
    [ViewName("UserInterface.Views.PropertyView")]
    [PresenterName("UserInterface.Presenters.PropertyPresenter")]
    [ValidParent(ParentType = typeof(EmergingPhaseColeoptile))]
    public class EmergingColeoptileParameter : Model
    {

        /// <summary>Maximum coleoptile length (mm)</summary>
        [Units("mm")]
        [Description("Maximum coleoptile length (mm)")]
        public double MaxColeoptileLength { get; set; } = 135;

        /// <summary>Coleoptile growth lag phase </summary>
        [Units("oCd")]
        [Description("Coleoptile growth lag phase (oCd)")]
        public double ColeoptileLagphase { get; set; } = 0;

        /// <summary>first leaf length</summary>
        [Units("mm")]
        [Description("First leaf length (mm)")]
        public double FirstLeafLength { get; set; } = 73.9;

        /// <summary>Seed Weight</summary>
        [Units("mg")]
        [Description("Seed Weight(mg)")]
        public double SeedWeight { get; set; } = 40;

    }
}