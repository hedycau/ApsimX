﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using System.Collections;  //enumerator
using System.Xml.Serialization;
using System.Runtime.Serialization;
using Models.Core;



namespace Models.WholeFarm
{

    ///<summary>
    /// Store for all the food designated for Household to eat (eg. Grain, Tree Crops (nuts) etc.)
    ///</summary> 
    [Serializable]
    [ViewName("UserInterface.Views.GridView")]
    [PresenterName("UserInterface.Presenters.PropertyPresenter")]
    [ValidParent(DropAnywhere = true)]
    public class Pasture: Model
    {


        /// <summary>
        /// Current state of this resource.
        /// </summary>
        [XmlIgnore]
        public List<PastureItem> Pastures;


        /// <summary>An event handler to allow us to initialise ourselves.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        [EventSubscribe("Commencing")]
        private void OnSimulationCommencing(object sender, EventArgs e)
        {
            Pastures = new List<PastureItem>();

            List<IModel> childNodes = Apsim.Children(this, typeof(IModel));

            foreach (IModel childModel in childNodes)
            {
                //cast the generic IModel to a specfic model.
                PastureType pastureInit = childModel as PastureType;
                PastureItem pasture = pastureInit.CreateListItem();
                Pastures.Add(pasture);
            }
        }

    }


}
