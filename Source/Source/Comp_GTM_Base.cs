using System;
using Verse;

namespace GTM
{
    public class Comp_GTM_Base : CompProperties
    {
        public Comp_GTM_Base()
        {
            this.compClass = typeof(GTM_Comp);
        }
        public string Customhatch = "";
        public string CustomxPath = "";
        public string[] SizeList = { "GTM_Hatch_Small","GTM_Hatch", "GTM_Hatch_Large",
        "GTM_Hatch_XLarge","GTM_Hatch_Huge","GTM_Hatch_XHuge","GTM_Hatch_XHuge"};
    }
}
