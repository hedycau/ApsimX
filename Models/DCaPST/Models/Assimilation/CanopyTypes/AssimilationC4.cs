﻿namespace Models.DCAPST
{
    /// <summary>
    /// Defines the pathway functions for a C4 canopy
    /// </summary>
    public class AssimilationC4 : Assimilation
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public AssimilationC4(
            DCaPSTParameters dcapstParameters,
            CanopyParameters canopy, 
            PathwayParameters parameters, 
            double ambientCO2
        ) : 
            base(dcapstParameters, canopy, parameters, ambientCO2)
        { 
        }

        /// <inheritdoc/>
        public override void UpdateIntercellularCO2(AssimilationPathway pathway, double gt, double waterUseMolsSecond)
        {
            pathway.IntercellularCO2 = ((gt - waterUseMolsSecond / 2.0) * ambientCO2 - pathway.CO2Rate) / (gt + waterUseMolsSecond / 2.0);
        }

        /// <inheritdoc/>
        protected override void UpdateMesophyllCO2(AssimilationPathway pathway, double leafGmT)
        {
            pathway.MesophyllCO2 = pathway.IntercellularCO2 - pathway.CO2Rate / leafGmT;
        }

        /// <inheritdoc/>
        protected override AssimilationFunction GetAc1Function(AssimilationPathway pathway, TemperatureResponse leaf)
        {
            var pathwayGbs = pathway.Gbs;
            var alpha = 0.1 / (canopy.DiffusivitySolubilityRatio * pathwayGbs);
            var leafGamma = leaf.Gamma;
            var leafKo = leaf.Ko;
            var leafKc = leaf.Kc;
            var canopyAirO2 = dcapstParameters.AirO2;

            var x = new Terms()
            {
                _1 = leaf.VcMaxT,
                _2 = leafKc + canopyAirO2 * (leafKc / leafKo),
                _3 = leaf.VpMaxT / (pathway.MesophyllCO2 + leaf.Kp),
                _4 = 0.0,
                _5 = 1.0,
                _6 = 1.0,
                _7 = alpha * leafGamma,
                _8 = leafGamma * canopyAirO2,
                _9 = (leafKc / leafKo) * alpha
            };

            var func = new AssimilationFunction()
            {
                x = x,
                MesophyllRespiration = leaf.GmRd,
                BundleSheathConductance = pathwayGbs,
                Respiration = leaf.RdT
            };

            return func;
        }

        /// <inheritdoc/>
        protected override AssimilationFunction GetAc2Function(AssimilationPathway pathway, TemperatureResponse leaf)
        {
            var pathwayGbs = pathway.Gbs;
            var alpha = 0.1 / (canopy.DiffusivitySolubilityRatio * pathwayGbs);
            var leafGamma = leaf.Gamma;
            var leafKc = leaf.Kc;
            var leafKo = leaf.Ko;
            var canopyAirO2 = dcapstParameters.AirO2;

            var x = new Terms()
            {
                _1 = leaf.VcMaxT,
                _2 = leafKc + canopyAirO2 * (leafKc / leafKo),
                _3 = 0.0,
                _4 = pathway.Vpr,
                _5 = 1.0,
                _6 = 1.0,
                _7 = alpha * leafGamma,
                _8 = leafGamma * canopyAirO2,
                _9 = (leafKc / leafKo) * alpha
            };

            var func = new AssimilationFunction()
            {
                x = x,
                MesophyllRespiration = leaf.GmRd,
                BundleSheathConductance = pathwayGbs,
                Respiration = leaf.RdT
            };

            return func;
        }
        
        /// <inheritdoc/>
        protected override AssimilationFunction GetAjFunction(AssimilationPathway pathway, TemperatureResponse leaf)
        {
            var extraATPCost = parameters.ExtraATPCost;
            var pathwayGbs = pathway.Gbs;
            var alpha = 0.1 / (canopy.DiffusivitySolubilityRatio * pathwayGbs);
            var leafGamma = leaf.Gamma;
            var leafJ = leaf.J;
            var mesophyllElectronTransportFraction = extraATPCost / (3 + extraATPCost);
            var canopyAir02 = dcapstParameters.AirO2;
            var fractionOfCyclicElectronTransport = 0.25 * extraATPCost;
            var z = (3 - fractionOfCyclicElectronTransport) / (4 * (1 - fractionOfCyclicElectronTransport));

            var x = new Terms()
            {
                _1 = z * (1.0 - mesophyllElectronTransportFraction) * leafJ / 3.0,
                _2 = canopyAir02 * (7.0 / 3.0) * leafGamma,
                _3 = 0.0,
                _4 = z * mesophyllElectronTransportFraction * leafJ / extraATPCost,
                _5 = 1.0,
                _6 = 1.0,
                _7 = alpha * leafGamma,
                _8 = leafGamma * canopyAir02,
                _9 = (7.0 / 3.0) * leafGamma * alpha
            };

            var func = new AssimilationFunction()
            {
                x = x,
                MesophyllRespiration = leaf.GmRd,
                BundleSheathConductance = pathwayGbs,
                Respiration = leaf.RdT
            };

            return func;
        }
    }
}
