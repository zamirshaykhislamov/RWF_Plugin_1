using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.ApplicationServices;

namespace rwf_plugin_1
{
    public enum ObjectSide
    {
        Exterior,
        Interior,
        Left,
        Right,
        Top, 
        Bottom
    }

    public static class Utility
    {
        public static T GetElementByName<T>(string name, Application app, Document doc) where T : Element
        {
            T element = null;
            var collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(T));
            var iterator = collector.GetElementIterator();
            iterator.Reset();
            while (iterator.MoveNext())
            {
                var current = iterator.Current as T;
                if (current.Name.Equals(name))
                {
                    element = current;
                    break;
                }
            }
            return element;
        }

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

        // The method is used to create a FamilyElementVisibility instance
        public static FamilyElementVisibility CreateVisibility()
        {
            FamilyElementVisibility familyElemVisibility = new FamilyElementVisibility(FamilyElementVisibilityType.Model);
            familyElemVisibility.IsShownInCoarse = false;
            familyElemVisibility.IsShownInFine = true;
            familyElemVisibility.IsShownInMedium = true;
            familyElemVisibility.IsShownInFrontBack = true;
            familyElemVisibility.IsShownInLeftRight = true;
            familyElemVisibility.IsShownInPlanRCPCut = false;
            return familyElemVisibility;
        }

    }

    public static class GeoHelper
    {
        // store the const precision
        private const double Precision = 0.0001;

        public static Face GetWallFace(Wall wall, View view, ObjectSide side)
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

        public static Face GetExtrusionFace(Extrusion extrusion, View view, ObjectSide side)
        {
            Face face = null;
            FaceArray faces = new FaceArray();

            Options options = new Options();
            options.ComputeReferences = true;
            options.View = view;

            if (extrusion != null)
            {
                IEnumerator<GeometryObject> objects = extrusion.get_Geometry(options).GetEnumerator();
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


        public static Face GetFace(FaceArray faces, ObjectSide side)
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

                if (side == ObjectSide.Exterior && (elevation < tempElevation || null == face))
                {
                    face = _face;
                    elevation = tempElevation;
                }
                else if (side == ObjectSide.Interior && (elevation > tempElevation || null == face))
                {
                    face = _face;
                    elevation = tempElevation;
                }

                if (side == ObjectSide.Left && (elevation < tempElevation || null == face))
                {
                    face = _face;
                    elevation = tempElevation;
                }

            }

            return face;
        }

        // The method is used to get extrusion's face along to the specified parameters
        static public Face GetExtrusionFace(Extrusion extrusion, View view, bool ExtOrInt)
        {
            Face face = null;
            FaceArray faces = null;
            if (extrusion.IsSolid)
            {
                Options options = new Options();
                options.ComputeReferences = true;
                options.View = view;
                //GeometryObjectArray geoArr = extrusion.get_Geometry(options).Objects;
                IEnumerator<GeometryObject> Objects = extrusion.get_Geometry(options).GetEnumerator();
                //foreach (GeometryObject geoObj in geoArr)
                while (Objects.MoveNext())
                {
                    GeometryObject geoObj = Objects.Current;

                    if (geoObj is Solid)
                    {
                        Solid s = geoObj as Solid;
                        faces = s.Faces;
                    }
                }
                if (ExtOrInt)
                    face = GetExteriorFace(faces);
                else
                    face = GetExteriorFace(faces);
            }
            return face;
        }

        // The assistant method is used for getting wall face and getting extrusion face
        static private Face GetExteriorFace(FaceArray faces)
        {
            double elevation = 0;
            double tempElevation = 0;
            Mesh mesh = null;
            Face face = null;
            foreach (Face f in faces)
            {
                tempElevation = 0;
                mesh = f.Triangulate();
                foreach (XYZ xyz in mesh.Vertices)
                {
                    tempElevation = tempElevation + xyz.Y;
                }
                tempElevation = tempElevation / mesh.Vertices.Count;
                if (elevation < tempElevation || null == face)
                {
                    face = f;
                    elevation = tempElevation;
                }
            }
            return face;
        }

    }
}
