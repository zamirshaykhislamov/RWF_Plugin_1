using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;

namespace RWF_Plugin_1
{
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        Application app;
        Document doc;

        View rightView;
        View leftView;
        View exteriorView;
        View interiorView;

        ReferencePlane centerPlane;
        ReferencePlane topPlane;
        ReferencePlane bottomPlane;
        ReferencePlane rightPlane;
        ReferencePlane leftPlane;

        Wall wall;
        Face exteriorWallFace;
        Face interiorWallFace;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            UIDocument uiDoc = uiApp.ActiveUIDocument;

            app = uiApp.Application;
            doc = uiDoc.Document;

            if (!doc.IsFamilyDocument)
            {
                TaskDialog.Show("Error", "This is not a family document");
                return Result.Succeeded;
            }

            try
            {
                Debug.WriteLine("It's a family document");

                if (!CreateCommon())
                {
                    return Result.Failed;
                }

                using (var transaction = new Transaction(doc, "Create Window"))
                {
                    transaction.Start();

                    CreateWindow();

                    transaction.Commit();
                }

                return Result.Succeeded;
            }
            catch (Exception exception)
            {
                message = exception.Message;
                return Result.Failed;
            }

        }

        private bool CreateCommon()
        {
            // Get reference planes
            List<ReferencePlane> planes = Utility.GetElements<ReferencePlane>(app, doc);
            foreach (ReferencePlane plane in planes)
            {
                if (plane.Name.Equals("Center (Front/Back)"))
                    centerPlane = plane;
                if (plane.Name.Equals("Top") || plane.Name.Equals("Head"))
                    topPlane = plane;
                if (plane.Name.Equals("Bottom") || plane.Name.Equals("Sill"))
                    bottomPlane = plane;
                if (plane.Name.Equals("Left"))
                    leftPlane = plane;
                if (plane.Name.Equals("Right"))
                    rightPlane = plane;

            }

            // Get views
            List<View> views = Utility.GetElements<View>(app, doc);
            foreach (View view in views)
            {
                if (view.Name.Equals("Right"))
                    rightView = view;
                if (view.Name.Equals("Left"))
                    leftView = view;
                if (view.Name.Equals("Exterior"))
                    exteriorView = view;
                if (view.Name.Equals("Interior"))
                    interiorView = view;
            }

            // Get Wall
            List<Wall> walls = Utility.GetElements<Wall>(app, doc);
            wall = walls[0];

            if (wall == null)
            {
                return false;
            }

            // Get wall exterior face
            exteriorWallFace = GeoHelper.GetWallFace(wall, rightView, WallSide.Exterior);

            // Get wall interior face
            interiorWallFace = GeoHelper.GetWallFace(wall, rightView, WallSide.Interior);

            Debug.WriteLine("\n=======\n");
            Debug.WriteLine(String.Format("centerPlane: {0}", centerPlane));
            Debug.WriteLine(String.Format("topPlane: {0}", topPlane));
            Debug.WriteLine(String.Format("bottomPlane: {0}", bottomPlane));
            Debug.WriteLine(String.Format("wall: {0}", wall));
            Debug.WriteLine(String.Format("exteriorWallFace: {0}", exteriorWallFace));
            Debug.WriteLine(String.Format("interiorWallFace: {0}", interiorWallFace));
            Debug.WriteLine(String.Format("rightView: {0}", rightView));
            Debug.WriteLine(String.Format("leftView: {0}", leftView));
     
            return true;

        }

        private void CreateWindow()
        {

        }


        public static class Utility
        {
            public static List<T> GetElements<T>(Application app, Document doc) where T : Element
            {
                var elements = new List<T>();

                var collector = new FilteredElementCollector(doc);
                collector.OfClass(typeof(T));
                FilteredElementIterator iterator = collector.GetElementIterator();
                iterator.Reset();
                while (iterator.MoveNext())
                {
                    var element = iterator.Current as T;
                    if (element != null)
                    {
                        elements.Add(element);
                    }
                }
                return elements;
            }

            public static double MetricToImperial(double value)
            {
                return value / 304.8;
            }

            public static double ImperialToMetric(double value)
            {
                return value * 304.8;
            }

        }

        public static class GeoHelper
        {
            // store the const precision
            private const double Precision = 0.0001;

            public static Face GetWallFace(Wall wall, View view, WallSide side)
            {
                Face face = null;
                FaceArray faces = new FaceArray();

                Options options = new Options();
                options.ComputeReferences = true;
                options.View = view;

                if (wall != null)
                {
                    IEnumerator<GeometryObject> objects = wall.get_Geometry(options).GetEnumerator();
                    while (objects.MoveNext())
                    {
                        var currentObject = objects.Current;
                        var solid = currentObject as Solid;
                        if (solid != null)
                        {
                            faces = solid.Faces;
                        }
                    }

                    face = GetFace(faces, side);
                }

                return face;
            }

            public static Face GetFace(FaceArray faces, WallSide side)
            {
                double elevation = 0;
                double tempElevation = 0;
                Mesh mesh;
                Face face = null;

                foreach (Face _face in faces)
                {
                    tempElevation = 0;
                    mesh = _face.Triangulate();
                    foreach (XYZ xyz in mesh.Vertices)
                    {
                        tempElevation += xyz.Y;
                    }
                    tempElevation /= mesh.Vertices.Count;

                    if (side == WallSide.Exterior && (elevation < tempElevation || null == face))
                    {
                        face = _face;
                        elevation = tempElevation;
                    }
                    else if (side == WallSide.Interior && (elevation > tempElevation || null == face))
                    {
                        face = _face;
                        elevation = tempElevation;
                    }
                }

                return face;
            }
        }

    }
}
