using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Autodesk.Revit.DB;
using Autodesk.Revit;
using Autodesk.Revit.ApplicationServices;

namespace rwf_plugin_1
{
    public class CreateExtrusion
    {
        readonly Application app;
        readonly Document doc;

        public CreateExtrusion(Application app, Document doc)
        {
            this.app = app;
            this.doc = doc;

        }

        // The method is used to create a CurveArray with four double parameters and one y coordinate value
        public CurveArray CreateRectangle(double left, double right, double top, double bottom, double y_coordinate)
        {
            CurveArray curveArray = new CurveArray();
            try
            {
                // Rectangular profile
                XYZ p0 = new XYZ(left, y_coordinate, top);
                XYZ p1 = new XYZ(right, y_coordinate, top);
                XYZ p2 = new XYZ(right, y_coordinate, bottom);
                XYZ p3 = new XYZ(left, y_coordinate, bottom);

                curveArray.Append(Line.CreateBound(p0, p1));
                curveArray.Append(Line.CreateBound(p1, p2));
                curveArray.Append(Line.CreateBound(p2, p3));
                curveArray.Append(Line.CreateBound(p3, p0));

                return curveArray;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return null;
            }
        }



        // The method is used to create extrusion using FamilyItemFactory.NewExtrusion()
        public Extrusion NewExtrusion(CurveArrArray curveArrArray, ReferencePlane workPlane, double startOffset, double endOffset)
        {
            Extrusion rectExtrusion = null;
            try
            {
                SubTransaction subTransaction = new SubTransaction(doc);
                subTransaction.Start();
                SketchPlane sketch = SketchPlane.Create(doc, workPlane.GetPlane());
                rectExtrusion = doc.FamilyCreate.NewExtrusion(true, curveArrArray, sketch, Math.Abs(endOffset - startOffset));
                rectExtrusion.StartOffset = startOffset;
                rectExtrusion.EndOffset = endOffset;
                subTransaction.Commit();
                return rectExtrusion;
            }
            catch
            {
                return null;
            }
        }
    }
}
