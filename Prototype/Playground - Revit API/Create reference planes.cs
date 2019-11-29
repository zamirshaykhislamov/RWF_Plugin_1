// Create method
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

          

// CREATE REFRENCE PLANES
            /// Exterior frame referenceplane
            var frameExtPlane = refPlaneCreator.Create(
                centerPlane, 
                rightView, 
                new XYZ(0, wall.Width / 2 - frameOffset, 0), 
                new XYZ(0, 0, 1), 
                "RefExteriorFrame");
                //refPlaneCreator.GetReferenceType(frameExtPlane);
            doc.Regenerate();

