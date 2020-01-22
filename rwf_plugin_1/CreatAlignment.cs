using Autodesk.Revit.DB;
using Autodesk.Revit.ApplicationServices;

namespace rwf_plugin_1
{
    public class CreatAlignment
    {
        readonly Application app;
        readonly Document doc;

        public CreatAlignment(Application app, Document doc)
        {
            this.app = app;
            this.doc = doc;
        }

        public void AddAlignment(View view, Face face1, Face face2)
        {
            PlanarFace pFace1 = null;
            PlanarFace pFace2 = null;
            if (face1 is PlanarFace)
                pFace1 = face1 as PlanarFace;
            if (face2 is PlanarFace)
                pFace2 = face2 as PlanarFace;
            if (pFace1 != null && pFace2 != null)
            {
                SubTransaction subTransaction = new SubTransaction(doc);
                subTransaction.Start();
                doc.FamilyCreate.NewAlignment(view, pFace1.Reference, pFace2.Reference);
                subTransaction.Commit();
            }
        }
        
        public void AddAlignment(View view, Face face, ReferencePlane referencePlane)
        {
            PlanarFace pFace = null;
            Reference ref1;
            ReferenceArray refArray = new ReferenceArray();
            ref1 = referencePlane.GetReference();
            if (face is PlanarFace)
                pFace = face as PlanarFace;
            if (pFace != null && ref1 != null)
            {
                refArray.Append(ref1);
            }
            SubTransaction subTransaction = new SubTransaction(doc);
            subTransaction.Start();
            doc.FamilyCreate.NewAlignment(view, pFace.Reference, ref1);
            subTransaction.Commit();
        }

    }
}
