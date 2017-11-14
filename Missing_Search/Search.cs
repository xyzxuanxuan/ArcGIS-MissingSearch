using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Project
{
    public class Search : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public Search()
        {
        }

        protected override void OnClick()
        {
            //
            //  TODO: Sample code showing how to access button host
            //
            ArcMap.Application.CurrentTool = null;
            Missing_Search MS = new Missing_Search();
            MS.Show();

        }
        protected override void OnUpdate()
        {
            Enabled = ArcMap.Application != null;
        }
    }

}
