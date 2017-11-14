using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ESRI.ArcGIS.ArcMap;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.CartoUI;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;


namespace Project
{
    public partial class Missing_Search : Form
    {
        public Missing_Search()
        {
            InitializeComponent();
        }

        IMxDocument pMD;
        IMap pMap;
        IFeatureLayer pFL;
        IFeatureClass pFC;
        int[] ID;
        string fieldname;

        private void Missing_Search_Load(object sender, EventArgs e)
        {
            comboBox1.Items.Clear();
            this.dataGridView1.DataSource = null;
            pMD = ArcMap.Document;
            pMap = pMD.FocusMap;
            ILayer pLayer = pMap.get_Layer(0);

            pFL = pLayer as IFeatureLayer;
          
            pFC = pFL.FeatureClass;
            for (int i = 0; i < pFC.Fields.FieldCount; i++)
            {
                comboBox1.Items.Add(pFC.Fields.get_Field(i).Name);
            }

            
        }

        public void button1_Click(object sender, EventArgs e)
        {
            fieldname = comboBox1.SelectedItem.ToString();
            DataTable DT = new DataTable();
            DT.Columns.Add("OID");
            DT.Columns.Add("Name");
            DT.Columns.Add("Name A");
            DT.Columns.Add("Suggestion_Value A");
            DT.Columns.Add("Name B");
            DT.Columns.Add("Suggestion_Value B");

            for (int i = 0; i < pFC.FeatureCount(null); i++)
            {
                string fieldvalue=pFC.GetFeature(i).get_Value(pFC.GetFeature(i).Fields.FindField(fieldname)).ToString();
                if (fieldvalue== " ")
                {
                    DataRow DR;
                    DR = DT.NewRow();
                    DR[0] = i.ToString();
                    DR[1] = pFC.GetFeature(i).get_Value(pFC.GetFeature(i).Fields.FindField("NAME")).ToString();

                    IFeature pFF = pFC.GetFeature(i);
                    
                    IPoint fG = pFC.GetFeature(i).Shape as IPoint;
                
                    IPoint fT = fG as IPoint;
                    double fx = fT.X;
                    double fy = fT.Y;
                                       
                    double[,] pDIS=new double[pFC.FeatureCount(null),2];

                    //get distance to all features//
                    for (int m = 0; m < pFC.FeatureCount(null); m++)
                    {                       
                        IGeometry cG = pFC.GetFeature(m).Shape;
                        IPoint cT = cG as IPoint;

                        double cx = cT.X;
                        double cy = cT.Y;

                        double dis = Math.Sqrt((fx - cx) * (fx - cx) + (fy - cy) * (fy - cy));
                        pDIS[m, 0] = Convert.ToDouble(m);
                        pDIS[m,1] = dis;                      
                    }

                    //sort the distances//
                    ArrayList pAL = new ArrayList();
                    for (int Q = 0; Q < pFC.FeatureCount(null); Q++)
                    {
                        pAL.Add(pDIS[Q, 1]);
                    }
                    pAL.Sort();

                    //get first two distances//
                    double n1 = Convert.ToDouble(pAL[1]);
                    double n2 = Convert.ToDouble(pAL[2]);
                    ArrayList pID=new ArrayList();
                    int ID_count = 0;
                    for (int r = 0; r < pFC.FeatureCount(null); r++)
                    {
                        if(pDIS[r,1]==n1||pDIS[r,1]==n2)
                        {
                            pID.Add(pDIS[r, 0]);
                            ID_count++;
                        }
                    }

                    //get feature OID of first two distances//
                    ID = new int[ID_count];
                    int l=0;
                    foreach(object num in pID)
                    {
                        ID[l] = Convert.ToInt16(num);
                        l++;
                    }

                    //get suggestion features's name and value//
                    int cc=2;
                    for (int d = 0; d < ID_count; d++)
                    {
                        if (cc < 5)
                        {
                            IFeature pF = pFC.GetFeature(ID[d]);
                            DR[cc] = pF.get_Value(pF.Fields.FindField("NAME"));
                            DR[cc + 1] = pF.get_Value(pF.Fields.FindField(fieldname));
                            cc = cc + 2;
                        }

                    }

                    DT.Rows.Add(DR);
                }
            }

            dataGridView1.DataSource = DT;
            MessageBox.Show("done");
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {           
            IFeatureSelection pFS = pFL as IFeatureSelection;
            pFS.Clear();

            IGeoFeatureLayer geoflay = pFL as IGeoFeatureLayer;
            IAnnotateLayerPropertiesCollection annopropcol;
            IAnnotateLayerProperties annolayprop;
            IElementCollection enumvisiblecol;
            annopropcol = geoflay.AnnotationProperties;
            annopropcol.QueryItem(0, out annolayprop, out enumvisiblecol, out enumvisiblecol);
            ILabelEngineLayerProperties labengprop = annolayprop as ILabelEngineLayerProperties;
            labengprop.Expression = "[NAME]";  // field name
            geoflay.DisplayAnnotation = true;
            annolayprop.LabelWhichFeatures = esriLabelWhichFeatures.esriSelectedFeatures;
            

            int rn = e.RowIndex;
            int pOID;
            pOID = Convert.ToInt16(dataGridView1.CurrentRow.Cells[0].Value);
            QueryFilter pQF = new QueryFilter();
            pQF.WhereClause="FID = " + pOID.ToString();
            
                       
            pFS.SelectFeatures(pQF, esriSelectionResultEnum.esriSelectionResultAdd, false);

            for (int k = 0; k < ID.Length; k++)
            {
                string m = dataGridView1.CurrentRow.Cells[2*(k+1)].Value.ToString();
                QueryFilter pQF2 = new QueryFilter();
                pQF2.WhereClause = "NAME = '" + m.ToString()+"'";
                               
                pFS.SelectFeatures(pQF2, esriSelectionResultEnum.esriSelectionResultAdd, false);
            }

            ISelectionSet pS = pFS.SelectionSet;
            ICursor pCursor;
            pS.Search(null, true, out pCursor);
            IFeatureCursor pFCursor = pCursor as IFeatureCursor;

            IFeature pFeature;
            IEnvelope pEnv = new Envelope() as IEnvelope;
      
            while ((pFeature = pFCursor.NextFeature()) != null)
            {
                IGeometry pGeometry = pFeature.Shape;
                IEnvelope pEN = pGeometry.Envelope;
                

                pEnv.Union(pEN); 
            }

             
            pEnv.Expand(1.3, 1.3, true);
            pMD.ActiveView.Extent = pEnv;
            pMD.ActiveView.Refresh();

            

            pMD.ActiveView.Refresh();
        }
    }
}
