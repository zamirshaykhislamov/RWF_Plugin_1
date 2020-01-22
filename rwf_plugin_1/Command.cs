using System;
using System.IO;
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
using Newtonsoft.Json;

namespace rwf_plugin_1
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
        ReferencePlane panelPlane;
        ReferencePlane frameExtPlane;
        ReferencePlane rightPlane;
        ReferencePlane leftPlane;

        Wall wall;
        Face exteriorWallFace;
        Face interiorWallFace;
        Face exteriorExtrusionFace;
        Face interiorExtrusionFace;
        Face leftExtrusionFace;
        //Face rightExtrusionFace;
        //Face topExtrusionFace;
        //Face bottomExtrusionFace;


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

                    CreateFrame();

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
                if (plane.Name.Equals("RefWinPanel"))
                    panelPlane = plane;
                if (plane.Name.Equals("RefExteriorFrame"))
                    frameExtPlane = plane;
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
            exteriorWallFace = GeoHelper.GetWallFace(wall, rightView, ObjectSide.Exterior);

            // Get wall interior face
            interiorWallFace = GeoHelper.GetWallFace(wall, rightView, ObjectSide.Interior);


            return true;
        }

        private void CreateFrame()
        {
            /*
            
            var windowJsonData = WindowJsonData.Parse("C:\\Users\\Zamir\\Desktop\\exportTest.json");

            familyManager.Set(familyManager.get_Parameter(BuiltInParameter.WINDOW_HEIGHT), windowJsonData.Height);
            familyManager.Set(familyManager.get_Parameter(BuiltInParameter.WINDOW_WIDTH), windowJsonData.Width);
            familyManager.Set(familyManager.get_Parameter("Default Sill Height"), windowJsonData.SillHeight);
            */

            FamilyManager familyManager = doc.FamilyManager;

            var refPlaneCreator = new CreateRefPlane(app, doc);
            var dimensionCreator = new CreateDimension(app, doc);
            var extrusionCreator = new CreateExtrusion(app, doc);
            var alignmentCreator = new CreatAlignment(app, doc);

            // Store the values
            var height = Utility.MetricToImperial(1500);
            var width = Utility.MetricToImperial(1000);
            var sillHeight = Utility.MetricToImperial(800);

            Debug.WriteLine("==> height {0}", height);
            throw new System.ArgumentException("==> some error");

            var frameOffset = Utility.MetricToImperial(100);
            var frameDepth = Utility.MetricToImperial(50);
            var frameWidth = Utility.MetricToImperial(70);
            var frameBottomInset = Utility.MetricToImperial(15);
            var frameTopInset = Utility.MetricToImperial(15);
            var frameLeftInset = Utility.MetricToImperial(15);
            var frameRightInset = Utility.MetricToImperial(15);
            var frameSpacing = Utility.MetricToImperial(70);

            var panelProfileWidth = Utility.MetricToImperial(60);
            var panelProfileDepth = Utility.MetricToImperial(50);
            var panelPosition = Utility.MetricToImperial(15);
            var panelOverLap = Utility.MetricToImperial(10);

            var glassThicknes = Utility.MetricToImperial(5);
            var glassPosition = Utility.MetricToImperial(20);


            // RefrenecePlanes
            var frameExtPlane = refPlaneCreator.Create(centerPlane, rightView, new XYZ(0, wall.Width / 2 - frameOffset, 0), new XYZ(0, 0, 1), "FrameExterior");
            var frameIntPlane = refPlaneCreator.Create(frameExtPlane, rightView, new XYZ(0, - frameDepth, 0), new XYZ(0, 0, 1), "FrameInterior");
            var frameOuterTop = refPlaneCreator.Create(topPlane, exteriorView, new XYZ(0, 0, -frameTopInset), new XYZ(0, -1, 0), "FrameOuterTop");
            var frameOuterBottom = refPlaneCreator.Create(bottomPlane, exteriorView, new XYZ(0, 0, frameBottomInset), new XYZ(0, -1, 0), "FrameOuterBottom");
            var frameOuterRight = refPlaneCreator.Create(rightPlane, exteriorView, new XYZ(-frameRightInset, 0, 0), new XYZ(0, 0, 1), "FrameOuterRight");
            var frameOuterLeft = refPlaneCreator.Create(leftPlane, exteriorView, new XYZ(frameLeftInset, 0, 0), new XYZ(0, 0, 1), "FrameOuterLeft");
            doc.Regenerate();

            // Dimension
            var windowOffsetDimension = dimensionCreator.AddDimension(rightView, exteriorWallFace, frameExtPlane);
            var frameDepthDim = dimensionCreator.AddDimension(rightView, frameExtPlane, frameIntPlane);
            //var frameDepthPara = familyManager.AddParameter("Frame Depth", BuiltInParameterGroup.INVALID, ParameterType.Length, true);
            //familyManager.Set(frameDepthPara, frameDepth);
            //frameDepthDim.FamilyLabel = frameDepthPara;
            //familyManager.SetParameterLocked(frameDepthPara, true);

            var frameTopInsetDim = dimensionCreator.AddDimension(exteriorView, topPlane, frameOuterTop);
            var frameBottomDim = dimensionCreator.AddDimension(exteriorView, bottomPlane, frameOuterBottom);
            var frameLeftDim = dimensionCreator.AddDimension(exteriorView, leftPlane, frameOuterLeft);
            var frameRightDim = dimensionCreator.AddDimension(exteriorView, rightPlane, frameOuterRight);
            doc.Regenerate();

            //Extrusions
            // external side of frame
            CurveArray frameCurveArr1 = extrusionCreator.CreateRectangle(
                width / 2 - frameLeftInset,
                -(width / 2 - frameLeftInset),
                sillHeight + height - frameTopInset,
                sillHeight + frameBottomInset,
                0);
            // bottom
            CurveArray frameCurveArr2 = extrusionCreator.CreateRectangle(
                width / 2 - frameWidth - frameLeftInset,
                -(width / 2 - frameWidth - frameRightInset),
                sillHeight + frameBottomInset + frameWidth + Utility.MetricToImperial(900) - frameSpacing,
                sillHeight + frameBottomInset + frameWidth,
                0);
            // top left
            CurveArray frameCurveArr3 = extrusionCreator.CreateRectangle(
                width / 2 - frameWidth - frameLeftInset,
                frameSpacing / 2,
                sillHeight + height - frameWidth - frameTopInset,
                sillHeight + frameBottomInset + frameWidth + Utility.MetricToImperial(900),
                0);
            // top right
            CurveArray frameCurveArr4 = extrusionCreator.CreateRectangle(-(frameSpacing / 2),
                -(width / 2 - frameWidth - frameRightInset),
                sillHeight + height - frameWidth - frameTopInset,
                sillHeight + frameBottomInset + frameWidth + Utility.MetricToImperial(900),
                0);

            CurveArrArray frameCurveArrArray = new CurveArrArray();
            frameCurveArrArray.Append(frameCurveArr1);
            frameCurveArrArray.Append(frameCurveArr2);
            frameCurveArrArray.Append(frameCurveArr3);
            frameCurveArrArray.Append(frameCurveArr4);
            var frameExt = extrusionCreator.NewExtrusion(frameCurveArrArray, frameExtPlane, frameDepth, 0);
            frameExt.SetVisibility(Utility.CreateVisibility());
            frameExt.Subcategory = doc.Settings.Categories.get_Item(BuiltInCategory.OST_WindowsFrameMullionProjection);
            doc.Regenerate();


            //Constrain
            //var frameFaceExt = GeoHelper.GetExtrusionFace(frameExt, rightView, true);
            exteriorExtrusionFace = GeoHelper.GetExtrusionFace(frameExt, rightView, ObjectSide.Exterior);
            alignmentCreator.AddAlignment(rightView, exteriorExtrusionFace, frameExtPlane);
            doc.Regenerate();

            //var frameFaceInt = GeoHelper.GetExtrusionFace(frameExt, rightView, false);
            interiorExtrusionFace = GeoHelper.GetExtrusionFace(frameExt, rightView, ObjectSide.Interior);
            alignmentCreator.AddAlignment(rightView, interiorExtrusionFace, frameIntPlane);
            doc.Regenerate();

            //leftExtrusionFace = GeoHelper.GetExtrusionFace(frameExt, rightView, ObjectSide.Left);
            //alignmentCreator.AddAlignment(exteriorView, leftExtrusionFace, frameOuterLeft);
            
            //doc.Regenerate();

        }

    }
}
