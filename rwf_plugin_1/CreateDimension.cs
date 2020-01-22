using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Autodesk.Revit.DB;
using Autodesk.Revit.ApplicationServices;

namespace rwf_plugin_1
{
    public class CreateDimension
    {
        readonly Application app;
        readonly Document doc;

        public CreateDimension(Application app, Document doc)
        {
            this.app = app;
            this.doc = doc;

        }

        public Dimension Create(View view, List<ReferencePlane> refPlanes, List<Face> faces, XYZ startPoint, XYZ endPoint)
        {
            Debug.WriteLine(String.Format("startPoint: {0} endPoint: {1} refPlanes: {2} faces: {3}", startPoint, endPoint, refPlanes.Count, faces.Count));

            if (startPoint == null || endPoint == null)
            {
                return null;
            }

            var refArray = new ReferenceArray();

            foreach (ReferencePlane refPlane in refPlanes)
            {
                var reference = refPlane.GetReference();
                if (reference != null)
                {
                    Debug.WriteLine("Append Plane Reference...");
                    refArray.Append(reference);
                }
            }

            foreach (Face face in faces)
            {
                Debug.WriteLine(String.Format("Append Face Reference... face: {0}", face));
                PlanarFace planarFace = face as PlanarFace;
                Debug.WriteLine(String.Format("Append Face Reference... planarFace: {0} - {1}", planarFace, face.Reference));
                if (planarFace != null)
                {
                    Reference reference = planarFace.Reference;
                    Debug.WriteLine(String.Format("Append Face Reference... reference: {0}", reference));
                    if (reference != null)
                    {
                        Debug.WriteLine("Append Face Reference...");
                        refArray.Append(reference);
                    }
                }
            }

            var subTransaction = new SubTransaction(doc);
            subTransaction.Start();
            var line = Line.CreateBound(startPoint, endPoint);
            var dim = doc.FamilyCreate.NewDimension(view, line, refArray);
            subTransaction.Commit();

            return dim;
        }

        public Dimension AddDimension(View view, ReferencePlane refPlane1, ReferencePlane refPlane2, ReferencePlane refPlane3)
        {
            Dimension dim;
            XYZ startPoint = new XYZ();
            XYZ endPoint = new XYZ();
            Line line;
            Reference ref1;
            Reference ref2;
            Reference ref3;
            ReferenceArray refArray = new ReferenceArray();
            ref1 = refPlane1.GetReference();
            ref2 = refPlane2.GetReference();
            ref3 = refPlane3.GetReference();
            startPoint = refPlane1.FreeEnd;
            endPoint = refPlane2.FreeEnd;
            if (null != ref1 && null != ref2 && null != ref3)
            {
                refArray.Append(ref1);
                refArray.Append(ref3);
                refArray.Append(ref2);
            }
            SubTransaction subTransaction = new SubTransaction(doc);
            subTransaction.Start();
            line = Line.CreateBound(startPoint, endPoint);
            dim = doc.FamilyCreate.NewDimension(view, line, refArray);
            subTransaction.Commit();
            return dim;

        }

        // The method is used to create dimension between referenceplane and face
        public Dimension AddDimension(View view, Face face, ReferencePlane refPlane)
        {
            Dimension dim;
            XYZ startPoint = new XYZ();
            XYZ endPoint = new XYZ();
            Line line;
            Reference ref1;
            Reference ref2;
            ReferenceArray refArray = new ReferenceArray();
            ref1 = refPlane.GetReference();
            PlanarFace pFace = face as PlanarFace;
            ref2 = pFace.Reference;
            if (null != ref1 && null != ref2)
            {
                refArray.Append(ref1);
                refArray.Append(ref2);
            }
            startPoint = refPlane.FreeEnd;
            endPoint = new XYZ(startPoint.X, pFace.Origin.Y, startPoint.Z);
            SubTransaction subTransaction = new SubTransaction(doc);
            subTransaction.Start();
            line = Line.CreateBound(startPoint, endPoint);
            dim = doc.FamilyCreate.NewDimension(view, line, refArray);
            subTransaction.Commit();
            return dim;
        }

        // The method is used to create dimension between two faces
        public Dimension AddDimension(View view, Face face1, Face face2)
        {
            Dimension dim;
            XYZ startPoint = new XYZ();
            XYZ endPoint = new XYZ();
            Line line;
            Reference ref1;
            Reference ref2;
            ReferenceArray refArray = new ReferenceArray();
            PlanarFace pFace1 = face1 as PlanarFace;
            ref1 = pFace1.Reference;
            PlanarFace pFace2 = face2 as PlanarFace;
            ref2 = pFace2.Reference;
            if (null != ref1 && null != ref2)
            {
                refArray.Append(ref1);
                refArray.Append(ref2);
            }
            startPoint = pFace1.Origin;
            endPoint = new XYZ(startPoint.X, pFace2.Origin.Y, startPoint.Z);
            SubTransaction subTransaction = new SubTransaction(doc);
            subTransaction.Start();
            line = Line.CreateBound(startPoint, endPoint);
            dim = doc.FamilyCreate.NewDimension(view, line, refArray);
            subTransaction.Commit();
            return dim;
        }

        // The method is used to create dimension between two referncePlanes
        public Dimension AddDimension(View view, ReferencePlane refPlane1, ReferencePlane refPlane2)
        {
            Dimension dim;
            XYZ startPoint = new XYZ();
            XYZ endPoint = new XYZ();
            Line line;
            Reference ref1;
            Reference ref2;
            ReferenceArray refArray = new ReferenceArray();
            ref1 = refPlane1.GetReference();
            ref2 = refPlane2.GetReference();
            startPoint = refPlane1.FreeEnd;
            endPoint = refPlane2.FreeEnd;
            if (null != ref1 && null != ref2)
            {
                refArray.Append(ref1);
                refArray.Append(ref2);
            }
            SubTransaction subTransaction = new SubTransaction(doc);
            subTransaction.Start();
            line = Line.CreateBound(startPoint, endPoint);
            dim = doc.FamilyCreate.NewDimension(view, line, refArray);
            subTransaction.Commit();
            return dim;

        }


    }
}
