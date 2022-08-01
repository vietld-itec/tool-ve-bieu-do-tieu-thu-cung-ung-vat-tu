using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using Exception = System.Exception;

namespace ACAD_BieuDoKeHoachVatTu
{

    public static class Library
    {


        public static void AttachRasterImage(Point3d insPt, string tenbienbao, double angle, double rong)
        {
            // Get the current database and start a transaction

            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;


            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Define the name and image to use
                string strImgName = tenbienbao;
                string strFileName = Data.pathpic + @"\" + tenbienbao + ".jpg";

                if (File.Exists(strFileName))
                {
                    #region Khi ton tai file anh

                    RasterImageDef acRasterDef;
                    bool bRasterDefCreated = false;
                    ObjectId acImgDefId;

                    // Get the image dictionary
                    ObjectId acImgDctID = RasterImageDef.GetImageDictionary(acCurDb);

                    // Check to see if the dictionary does not exist, it not then create it
                    if (acImgDctID.IsNull)
                    {
                        acImgDctID = RasterImageDef.CreateImageDictionary(acCurDb);
                    }

                    // Open the image dictionary
                    DBDictionary acImgDict = acTrans.GetObject(acImgDctID, OpenMode.ForRead) as DBDictionary;

                    // Check to see if the image definition already exists
                    if (acImgDict.Contains(strImgName))
                    {
                        acImgDefId = acImgDict.GetAt(strImgName);

                        acRasterDef = acTrans.GetObject(acImgDefId, OpenMode.ForWrite) as RasterImageDef;
                    }
                    else
                    {
                        // Create a raster image definition
                        RasterImageDef acRasterDefNew = new RasterImageDef();

                        // Set the source for the image file
                        acRasterDefNew.SourceFileName = strFileName;

                        // Load the image into memory
                        acRasterDefNew.Load();

                        // Add the image definition to the dictionary
                        acImgDict.UpgradeOpen();
                        acImgDefId = acImgDict.SetAt(strImgName, acRasterDefNew);

                        acTrans.AddNewlyCreatedDBObject(acRasterDefNew, true);

                        acRasterDef = acRasterDefNew;

                        bRasterDefCreated = true;
                    }

                    // Open the Block table for read
                    BlockTable acBlkTbl;
                    acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                    // Open the Block table record Model space for write
                    BlockTableRecord acBlkTblRec;
                    acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                                                    OpenMode.ForWrite) as BlockTableRecord;

                    // Create the new image and assign it the image definition
                    using (RasterImage acRaster = new RasterImage())
                    {
                        acRaster.ImageDefId = acImgDefId;

                        // Use ImageWidth and ImageHeight to get the size of the image in pixels (1024 x 768).
                        // Use ResolutionMMPerPixel to determine the number of millimeters in a pixel so you 
                        // can convert the size of the drawing into other units or millimeters based on the 
                        // drawing units used in the current drawing.

                        // Define the width and height of the image
                        Vector3d width;
                        Vector3d height;

                        // Check to see if the measurement is set to English (Imperial) or Metric units
                        if (acCurDb.Measurement == MeasurementValue.English)
                        {
                            width = new Vector3d((acRasterDef.ResolutionMMPerPixel.X * acRaster.ImageWidth) / 25.4, 0, 0);
                            height = new Vector3d(0, (acRasterDef.ResolutionMMPerPixel.Y * acRaster.ImageHeight) / 25.4, 0);
                        }
                        else
                        {
                            width = new Vector3d(acRasterDef.ResolutionMMPerPixel.X * acRaster.ImageWidth, 0, 0);
                            height = new Vector3d(0, acRasterDef.ResolutionMMPerPixel.Y * acRaster.ImageHeight, 0);
                        }

                        // Define the position for the image 

                        double scaleX = rong / width.X;
                        double scaleY = scaleX;

                        // Define and assign a coordinate system for the image's orientation
                        CoordinateSystem3d coordinateSystem = new CoordinateSystem3d(insPt, width * scaleX, height * scaleY);
                        acRaster.Orientation = coordinateSystem;

                        // Set the rotation angle for the image
                        acRaster.Rotation = angle * Math.PI / 180;

                        // Add the new object to the block table record and the transaction
                        acBlkTblRec.AppendEntity(acRaster);
                        acTrans.AddNewlyCreatedDBObject(acRaster, true);

                        // Connect the raster definition and image together so the definition
                        // does not appear as "unreferenced" in the External References palette.
                        RasterImage.EnableReactors(true);
                        acRaster.AssociateRasterDef(acRasterDef);

                        if (bRasterDefCreated)
                        {
                            acRasterDef.Dispose();
                        }
                    }

                    // Save the new object to the database
                    acTrans.Commit();

                    // Dispose of the transaction

                    #endregion
                }



            }
        }


        public static void UpdateTextFont(string font)
        {
            // Get the current document and database
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the current text style for write
                TextStyleTableRecord acTextStyleTblRec;
                acTextStyleTblRec = acTrans.GetObject(acCurDb.Textstyle,
                OpenMode.ForWrite) as
                TextStyleTableRecord;
                // Get the current font settings
                Autodesk.AutoCAD.GraphicsInterface.FontDescriptor acFont;
                acFont = acTextStyleTblRec.Font;
                // Update the text style's typeface with "PlayBill"
                Autodesk.AutoCAD.GraphicsInterface.FontDescriptor acNewFont;
                acNewFont = new
                Autodesk.AutoCAD.GraphicsInterface.FontDescriptor(font, acFont.Bold, acFont.Italic, acFont.CharacterSet, acFont.PitchAndFamily);

                acTextStyleTblRec.Font = acNewFont;
                acDoc.Editor.Regen();
                // Save the changes and dispose of the transaction
                acTrans.Commit();
            }
        }

        public static Point3d OverPoint3D(Point3d inpPoint, double value, double heightext, Point3d beforePoint, double beforevalue, Point3d pOxy)
        {
            Point3d output = new Point3d();

            double newX = 0;
            double newY = 0;

            Point3d[] listInput = new Point3d[4];

            double width = 4 * heightext;
            double heigh = 1.2 * heightext;

            listInput[0] = new Point3d(beforePoint.X, beforePoint.Y, 0);
            listInput[1] = new Point3d(beforePoint.X, beforePoint.Y + heigh, 0);
            listInput[2] = new Point3d(beforePoint.X + width, beforePoint.Y + heigh, 0);
            listInput[3] = new Point3d(beforePoint.X + width, beforePoint.Y, 0);


            Point3d sosanh = new Point3d();

            if (beforevalue <= value)
            {
                sosanh = inpPoint;
            }
            else
            {
                sosanh = new Point3d(inpPoint.X, inpPoint.Y + 1.2 * heightext, 0);

            }

            bool inside = IsPointInPolygon(sosanh, listInput);

            if (inside == false)
            {
                newX = inpPoint.X;
                newY = inpPoint.Y;
            }
            else
            {
                if (beforevalue <= value)
                {
                    newX = inpPoint.X;
                    newY = beforePoint.Y + 1.5 * heightext;


                }
                else
                {
                    newX = inpPoint.X;
                    newY = beforePoint.Y - 1.5 * heightext;


                }

            }
            output = new Point3d(newX, newY, 0);
            if (output.Y <= pOxy.Y)
            {
                output = new Point3d(newX, pOxy.Y, 0);
            }



            double delta = inpPoint.Y - pOxy.Y;

            double divide = delta / heightext;

            if (divide < 1)
            {
                output = new Point3d(newX, pOxy.Y + heightext, 0);
            }


            return output;
        }
        public static bool IsPointInPolygon(Point3d point, Point3d[] polygon1)
        {
            bool isInside = false;
            Point3d[] polygon = new Point3d[polygon1.Length - 1];
            for (int k = 0; k < polygon.Length; k++)
                polygon[k] = polygon1[k];
            for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
            {
                if (((polygon[i].Y > point.Y) != (polygon[j].Y > point.Y)) &&
                (point.X < (polygon[j].X - polygon[i].X) * (point.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) + polygon[i].X))
                {
                    isInside = !isInside;
                }
            }
            return isInside;
        }

        public static void markAxis(Point3d markpoint, double width, bool horizontal)
        {
            Point3d point1 = new Point3d(markpoint.X - 0.5 * width, markpoint.Y, markpoint.Z);
            Point3d point2 = new Point3d(markpoint.X + 0.5 * width, markpoint.Y, markpoint.Z);

            if (horizontal)
            {
                point1 = new Point3d(markpoint.X, markpoint.Y - 0.5 * width, markpoint.Z);
                point2 = new Point3d(markpoint.X, markpoint.Y + 0.5 * width, markpoint.Z);
            }

            double x1 = point1.X;
            double x2 = point2.X;
            double y1 = point1.Y;
            double y2 = point2.Y;
            // Get the current document and database
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
                OpenMode.ForRead) as BlockTable;
                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                OpenMode.ForWrite) as BlockTableRecord;
                // Create a line that starts at 5,5 and ends at 12,3
                Line acLine = new Line(new Point3d(x1, y1, 0), new Point3d(x2, y2, 0));
                acLine.SetDatabaseDefaults();
                // Add the new object to the block table record and the transaction
                acBlkTblRec.AppendEntity(acLine);
                acTrans.AddNewlyCreatedDBObject(acLine, true);
                // Save the new object to the database
                acTrans.Commit();
            }
        }
        /// <summary>
        /// Draw line between 2 point
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        public static void line(Point3d point1, Point3d point2)
        {
            double x1 = point1.X;
            double x2 = point2.X;
            double y1 = point1.Y;
            double y2 = point2.Y;
            // Get the current document and database
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
                OpenMode.ForRead) as BlockTable;
                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                OpenMode.ForWrite) as BlockTableRecord;
                // Create a line that starts at 5,5 and ends at 12,3
                Line acLine = new Line(new Point3d(x1, y1, 0), new Point3d(x2, y2, 0));
                acLine.SetDatabaseDefaults();
                // Add the new object to the block table record and the transaction
                acBlkTblRec.AppendEntity(acLine);
                acTrans.AddNewlyCreatedDBObject(acLine, true);
                // Save the new object to the database
                acTrans.Commit();
            }
        }
        /// <summary>
        /// Ham thuc thi ve POLYLINE
        /// </summary>
        /// <param name="arr_2Dpoint"></param>
        public static void Polyline(List<Point2d> arr_2Dpoint)
        {
            // Get the current document and database
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
                OpenMode.ForRead) as BlockTable;
                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                OpenMode.ForWrite) as BlockTableRecord;
                // Create a polyline with two segments (3 points)
                Polyline acPoly = new Polyline();
                acPoly.SetDatabaseDefaults();

                for (int i = 0; i < arr_2Dpoint.Count; i++)
                {
                    acPoly.AddVertexAt(i, new Point2d(arr_2Dpoint[i].X, arr_2Dpoint[i].Y), 0, 0, 0);
                }



                // Add the new object to the block table record and the transaction
                acBlkTblRec.AppendEntity(acPoly);
                acTrans.AddNewlyCreatedDBObject(acPoly, true);
                // Save the new object to the database
                acTrans.Commit();
            }
        }

        /// <summary>
        /// Create line with 2 points
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        public static void line(double x1, double y1, double x2, double y2)
        {
            // Get the current document and database
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
                OpenMode.ForRead) as BlockTable;
                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                OpenMode.ForWrite) as BlockTableRecord;
                // Create a line that starts at 5,5 and ends at 12,3
                Line acLine = new Line(new Point3d(x1, y1, 0), new Point3d(x2, y2, 0));
                acLine.SetDatabaseDefaults();
                // Add the new object to the block table record and the transaction
                acBlkTblRec.AppendEntity(acLine);
                acTrans.AddNewlyCreatedDBObject(acLine, true);
                // Save the new object to the database
                acTrans.Commit();
            }
        }

        /// <summary>
        /// Get point from user
        /// </summary>
        /// <returns></returns>
        public static Point3d GetPointsFromUser()
        {
            // Get the current database and start the Transaction Manager
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            PromptPointResult pPtRes;
            PromptPointOptions pPtOpts = new PromptPointOptions("");
            // Prompt for the start point
            pPtOpts.Message = "\nVui lòng click chọn điểm: ";
            pPtRes = acDoc.Editor.GetPoint(pPtOpts);
            Point3d ptStart = pPtRes.Value;
            // Exit if the user presses ESC or cancels the command
            if (pPtRes.Status == PromptStatus.Cancel) return ptStart;

            return ptStart;
        }
        public static Point3d GetPointsFromUser(string annou)
        {
            // Get the current database and start the Transaction Manager
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            PromptPointResult pPtRes;
            PromptPointOptions pPtOpts = new PromptPointOptions("");
            // Prompt for the start point
            pPtOpts.Message = "\n" + annou + " :";
            pPtRes = acDoc.Editor.GetPoint(pPtOpts);
            Point3d ptStart = pPtRes.Value;
            // Exit if the user presses ESC or cancels the command
            if (pPtRes.Status == PromptStatus.Cancel) return ptStart;

            return ptStart;
        }
        /// <summary>
        /// Get integer value
        /// </summary>
        public static int GetIntegerFromUser(string annoucement)
        {
            int kq = 0;
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            PromptIntegerOptions pIntOpts = new PromptIntegerOptions("");
            pIntOpts.Message = "\n " + annoucement + ":";
            try
            {
                // Get the value entered by the user
                PromptIntegerResult pIntRes = acDoc.Editor.GetInteger(pIntOpts);
                kq = pIntRes.Value;
            }
            catch (Exception)
            {
                pIntOpts.Message = "Kiểm tra lại giá trị nhập vào phải là số nguyên";
                //throw;
            }

            return kq;

        }
        /// <summary>
        /// Get du lieu kieu so tu nguoi dung nhap
        /// </summary>
        /// <param name="annoucement"></param>
        /// <returns></returns>
        public static double GetDoubleFromUser(string annoucement)
        {
            double kq = 0;
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            PromptDoubleOptions pIntOpts = new PromptDoubleOptions("");
            pIntOpts.Message = "\n " + annoucement;
            try
            {
                // Get the value entered by the user
                PromptDoubleResult pIntRes = acDoc.Editor.GetDouble(pIntOpts);
                kq = pIntRes.Value;
            }
            catch (Exception)
            {
                pIntOpts.Message = "Kiểm tra lại giá trị nhập vào phải là thực";
                //throw;
            }

            return kq;

        }

        /// <summary>
        /// Get string from user
        /// </summary>
        /// <param name="annoucement"></param>
        /// <returns></returns>
        public static string GetStringFromUser(string annoucement)
        {
            string kq = "";
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            PromptStringOptions pStrOpts = new PromptStringOptions("\n" + annoucement);
            pStrOpts.AllowSpaces = true;
            PromptResult pStrRes = acDoc.Editor.GetString(pStrOpts);

            kq = (string)pStrRes.StringResult;
            return kq;
        }


        /// <summary>
        /// Khoi tao layer moi
        /// </summary>
        /// <param name="sLayerName"></param>
        /// <param name="_colors"></param>
        public static void newlayer(string sLayerName, short colorID)
        {
            // Get the current document and database
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Layer table for read
                LayerTable acLyrTbl;
                acLyrTbl = acTrans.GetObject(acCurDb.LayerTableId,
                OpenMode.ForRead) as LayerTable;

                if (acLyrTbl.Has(sLayerName) == false)
                {
                    LayerTableRecord acLyrTblRec = new LayerTableRecord();
                    // Assign the layer the ACI color 1 and a name
                    acLyrTblRec.Color = Color.FromColorIndex(ColorMethod.ByAci, colorID);
                    acLyrTblRec.Name = sLayerName;
                    // Upgrade the Layer table for write
                    acLyrTbl.UpgradeOpen();
                    // Append the new layer to the Layer table and the transaction
                    acLyrTbl.Add(acLyrTblRec);
                    acTrans.AddNewlyCreatedDBObject(acLyrTblRec, true);
                }
                // Open the Block table for read
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
                OpenMode.ForRead) as BlockTable;
                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                OpenMode.ForWrite) as BlockTableRecord;

                // Save the changes and dispose of the transaction
                acTrans.Commit();
            }
        }

        /// <summary>
        /// Set current layer
        /// </summary>
        /// <param name="sLayerName"></param>
        public static void layercurrent(string sLayerName)
        {
            // Get the current document and database
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Layer table for read
                LayerTable acLyrTbl;
                acLyrTbl = acTrans.GetObject(acCurDb.LayerTableId,
                OpenMode.ForRead) as LayerTable;

                if (acLyrTbl.Has(sLayerName) == true)
                {
                    // Set the layer Center current
                    acCurDb.Clayer = acLyrTbl[sLayerName];
                    // Save the changes
                    acTrans.Commit();
                }
                // Dispose of the transaction
            }
        }
        /// <summary>
        /// Add hatch vao hinh chu nhat
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="ngang"></param>
        /// <param name="doc"></param>
        /// <param name="kyhieuHatch"></param>
        public static void AddHatchRectangular(Point3d p1, double ngang, double doc, string kyhieuHatch)
        {
            //////
            /// p1 -> p2
            /// p4 -> p3
            // Get the current document and database
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
                OpenMode.ForRead) as BlockTable;
                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                OpenMode.ForWrite) as BlockTableRecord;

                //Crea a rectangular object for the closed boundary to hatch
                Point3d p2 = new Point3d(p1.X + ngang, p1.Y, p1.Z);
                Point3d p3 = new Point3d(p1.X + ngang, p2.Y - doc, p1.Z);
                Point3d p4 = new Point3d(p1.X, p1.Y - doc, p1.Z);

                Polyline rect = new Polyline();

                rect.SetDatabaseDefaults();
                rect.AddVertexAt(0, new Point2d(p1.X, p1.Y), 0, 0, 0);
                rect.AddVertexAt(1, new Point2d(p2.X, p2.Y), 0, 0, 0);
                rect.AddVertexAt(2, new Point2d(p3.X, p3.Y), 0, 0, 0);
                rect.AddVertexAt(3, new Point2d(p4.X, p4.Y), 0, 0, 0);
                rect.AddVertexAt(4, new Point2d(p1.X, p1.Y), 0, 0, 0);


                // Add the new circle object to the block table record and the transaction
                acBlkTblRec.AppendEntity(rect);
                acTrans.AddNewlyCreatedDBObject(rect, true);

                // Adds the circle to an object id array
                ObjectIdCollection acObjIdColl = new ObjectIdCollection();
                acObjIdColl.Add(rect.ObjectId);

                // Create the hatch object and append it to the block table record
                Hatch acHatch = new Hatch();
                acBlkTblRec.AppendEntity(acHatch);
                acTrans.AddNewlyCreatedDBObject(acHatch, true);
                // Set the properties of the hatch object
                // Associative must be set after the hatch object is appended to the 
                // block table record and before AppendLoop
                acHatch.SetDatabaseDefaults();
                acHatch.SetHatchPattern(HatchPatternType.PreDefined, kyhieuHatch);
                acHatch.Associative = true;
                acHatch.AppendLoop(HatchLoopTypes.Outermost, acObjIdColl);
                acHatch.EvaluateHatch(true);
                // Save the new object to the database
                acTrans.Commit();
            }
        }


        public static void AddHatchRectangular(Point3d pstart, Point3d pend, double bedaynet, string kyhieuHatch)
        {

            double ngang = pend.X - pstart.X;
            double doc = bedaynet;
            //////
            /// p1 -> p2
            /// p4 -> p3
            // Get the current document and database
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
                OpenMode.ForRead) as BlockTable;
                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                OpenMode.ForWrite) as BlockTableRecord;

                //Crea a rectangular object for the closed boundary to hatch
                Point3d p2 = new Point3d(pstart.X + ngang, pstart.Y, pstart.Z);
                Point3d p3 = new Point3d(pstart.X + ngang, p2.Y - doc, pstart.Z);
                Point3d p4 = new Point3d(pstart.X, pstart.Y - doc, pstart.Z);

                Polyline rect = new Polyline();

                rect.SetDatabaseDefaults();
                rect.AddVertexAt(0, new Point2d(pstart.X, pstart.Y), 0, 0, 0);
                rect.AddVertexAt(1, new Point2d(p2.X, p2.Y), 0, 0, 0);
                rect.AddVertexAt(2, new Point2d(p3.X, p3.Y), 0, 0, 0);
                rect.AddVertexAt(3, new Point2d(p4.X, p4.Y), 0, 0, 0);
                rect.AddVertexAt(4, new Point2d(pstart.X, pstart.Y), 0, 0, 0);


                // Add the new circle object to the block table record and the transaction
                acBlkTblRec.AppendEntity(rect);
                acTrans.AddNewlyCreatedDBObject(rect, true);

                // Adds the circle to an object id array
                ObjectIdCollection acObjIdColl = new ObjectIdCollection();
                acObjIdColl.Add(rect.ObjectId);

                // Create the hatch object and append it to the block table record
                Hatch acHatch = new Hatch();
                acBlkTblRec.AppendEntity(acHatch);
                acTrans.AddNewlyCreatedDBObject(acHatch, true);
                // Set the properties of the hatch object
                // Associative must be set after the hatch object is appended to the 
                // block table record and before AppendLoop
                acHatch.SetDatabaseDefaults();
                acHatch.SetHatchPattern(HatchPatternType.PreDefined, kyhieuHatch);
                acHatch.Associative = true;
                acHatch.AppendLoop(HatchLoopTypes.Outermost, acObjIdColl);
                acHatch.EvaluateHatch(true);
                // Save the new object to the database
                acTrans.Commit();
            }
        }

        /// <summary>
        /// HIEN THI KIEU CHU DON
        /// </summary>
        /// <param name="mpoint"></param>
        /// <param name="hienthi"></param>
        /// <param name="angle"></param>
        /// <param name="height"></param>
        public static void CreateSingleText(Point3d mpoint, string hienthi, double angle, double height)
        {

            if (height == 0)
            {
                height = 2.5;
            }
            // Get the current document and database
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
                OpenMode.ForRead) as BlockTable;
                // Open the Block table record Model space forwrite
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                OpenMode.ForWrite) as BlockTableRecord;
                // Create a single-line text object
                DBText acText = new DBText();
                acText.SetDatabaseDefaults();


                acText.Position = mpoint;
                acText.Height = height;
                acText.TextString = hienthi;
                // Change the oblique angle of the text object to 45 degrees(0.707 in radians)
                acText.Rotation = angle * Math.PI / 180;
                //acText.Oblique = angle * Math.PI / 180;
                acBlkTblRec.AppendEntity(acText);
                acTrans.AddNewlyCreatedDBObject(acText, true);
                // Save the changes and dispose of the transaction
                acTrans.Commit();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mpoint"></param>
        /// <param name="hienthi"></param>
        /// <param name="angle"></param>
        /// <param name="height"></param>
        public static void CreateMultipleText(Point3d mpoint, string hienthi, double angle, double height)
        {
            // Get the current document and database
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
                OpenMode.ForRead) as BlockTable;
                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                OpenMode.ForWrite) as BlockTableRecord;
                // Create a multiline text object
                MText acMText = new MText();

                acMText.SetDatabaseDefaults();
                acMText.Location = mpoint;
                acMText.Rotation = angle * Math.PI / 180;
                //acMText.Width = 4;
                acMText.Height = height;
                acMText.Contents = hienthi;


                acBlkTblRec.AppendEntity(acMText);
                acTrans.AddNewlyCreatedDBObject(acMText, true);
                // Save the changes and dispose of the transaction
                acTrans.Commit();
            }
        }

        public static double distance(Point3d first, Point3d end)
        {
            double kq = 0;

            double x1 = first.X;
            double y1 = first.Y;
            double z1 = first.Z;

            double x2 = end.X;
            double y2 = end.Y;
            double z2 = end.Z;

            double tmpx1 = Math.Pow(x2 - x1, 2);
            double tmpy1 = Math.Pow(y2 - y1, 2);
            double tmpz1 = Math.Pow(z2 - z1, 2);

            kq = Math.Sqrt(tmpx1 + tmpy1 + tmpz1);

            return kq;
        }
        public static void Dimension(Point3d mp1, Point3d mp2, double angle)
        {
            // Get the current document and database
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
                OpenMode.ForRead) as BlockTable;
                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                OpenMode.ForWrite) as BlockTableRecord;

                RotatedDimension acRotDim = new RotatedDimension();

                acRotDim.XLine1Point = mp1;
                acRotDim.XLine2Point = mp2;

                acRotDim.Rotation = angle * Math.PI / 180;
                acRotDim.DimLinePoint = new Point3d(0, 5, 0);
                acRotDim.DimensionStyle = acCurDb.Dimstyle;

                acBlkTblRec.AppendEntity(acRotDim);
                acTrans.AddNewlyCreatedDBObject(acRotDim, true);
                // Save the changes and dispose of the transaction
                acTrans.Commit();
            }
        }


    }

}
