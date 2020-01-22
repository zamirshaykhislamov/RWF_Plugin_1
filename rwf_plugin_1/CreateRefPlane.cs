using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.ApplicationServices;

namespace rwf_plugin_1
{
    public class CreateRefPlane
    {
        readonly Application app;
        readonly Document doc;

        public CreateRefPlane(Application app, Document doc)
        {
            this.app = app;
            this.doc = doc;
        }

        public ReferencePlane Create(ReferencePlane host, View view, XYZ offset, XYZ cutVector, string name)
        {
            ReferencePlane refPlane = null;

            try
            {
                if (host != null)
                {
                    var bubbleEnd = host.BubbleEnd.Add(offset);
                    var freeEnd = host.FreeEnd.Add(offset);

                    var subTransaction = new SubTransaction(doc);
                    subTransaction.Start();
                    refPlane = doc.FamilyCreate.NewReferencePlane(bubbleEnd, freeEnd, cutVector, view);
                    refPlane.Name = name;
                    subTransaction.Commit();
                }
            }
            catch
            {

            }

            return refPlane;
        }

        public FamilyInstanceReferenceType GetReferenceType(ReferencePlane reference)
        {
            var subTransaction = new SubTransaction(doc);
            subTransaction.Start();

            subTransaction.Commit();
            return FamilyInstanceReferenceType.NotAReference;
        }

    }
}
