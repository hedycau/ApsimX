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
        public double MaxColeoptileLength { get; set; }

        /// <summary>Maximum Coleoptile growth rate </summary>
        [Units("mm/oCd")]
        [Description("Maximum coleoptile growth rate (oCd/mm)")]
        public double MaxColeoptileGrowthRate { get; set; }

        /// <summary>Coleoptile growth lag phase </summary>
        [Units("oCd")]
        [Description("Coleoptile growth lag phase (oCd)")]
        public double ColeoptileLagphase { get; set; }

    }
}