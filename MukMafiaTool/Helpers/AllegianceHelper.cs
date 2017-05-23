namespace MukMafiaTool.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using MukMafiaTool.Model;

    public static class AllegianceHelper
    {
        public static IEnumerable<SelectListItem> GenerateAllegiencesForDropDown()
        {
            var alleigiances = (Allegiance[])Enum.GetValues(typeof(Allegiance));
            var selectableAllegiances = alleigiances.Select(a =>
                {
                    var selectListItem = new SelectListItem { Text = a.ToString(), Value = a.ToString() };

                    if (a == Allegiance.Town)
                    {
                        selectListItem.Selected = true;
                    }

                    return selectListItem;
                });

            return selectableAllegiances;
        }
    }
}