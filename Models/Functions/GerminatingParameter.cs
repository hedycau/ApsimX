using System;
using Models.Core;
using Models.PMF.Phen;

namespace Models.Functions
{
    /// <summary>
    /// Germinating parameters.
    /// </summary>
    [Serializable]
    [Description("Germinationg parameters")]
    [ViewName("UserInterface.Views.PropertyView")]
    [PresenterName("UserInterface.Presenters.PropertyPresenter")]
    [ValidParent(ParentType = typeof(GerminatingPhaseSoil))]
    public class GerminatingParameter : Model
    {
        /// <summary>Dormancy Phase </summary>
        [Units("mm/oCd")]
        [Description("Dormancy phase(oCd)")]
        public double DormancyPhase { get; set; } = 0;

        /// <summary>Germination Target </summary>
        [Units("oCd")]
        [Description("Germination Target (oCd)")]
        public double GerminationTarget { get; set; } = 35;

    }
}