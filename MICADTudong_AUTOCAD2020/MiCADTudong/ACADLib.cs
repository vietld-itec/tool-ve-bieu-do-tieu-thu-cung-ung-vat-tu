using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;

using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using Exception = System.Exception;
using MenuItem = Autodesk.AutoCAD.Windows.MenuItem;


namespace ACAD_BieuDoKeHoachVatTu
{

    public class ACADLib : IExtensionApplication
    {
        public static Point3d pO = new Point3d();
        public static Point3d pX = new Point3d();
        public static Point3d pY = new Point3d();
        /// <summary>
        /// KHOI TAO LAYER
        /// </summary>
        
        [CommandMethod("KHOITAOLAYER")]
        public static void KhoiTaoLayer()
        {
            Library.UpdateTextFont("Arial");

            Library.newlayer("CHU", 50);
            Library.newlayer("CHUNGAY", 3);
            Library.newlayer("CHU_VC", 50);
            Library.newlayer("CHU_XE", 142);
            Library.newlayer("TRUC", 4);
            Library.newlayer("TIEUTHU", 3);
            Library.newlayer("DUTRU", 7);
            Library.newlayer("KEHOACH", 202);
            Library.newlayer("VANCHUYEN", 1);
            Library.newlayer("THUCTE", 5);
            Library.newlayer("NGAY", 251);

        }


        [CommandMethod("NHAPLIEUVATLIEU")]
        public static void NhapLieuVatLieu()
        {
            frmNhapLieu frm = new frmNhapLieu();
            frm.Show();
        }

        [CommandMethod("VEBIEUDOTIEUTHU")]
        public static void VeBieuDoTieuThu()
        {
            try
            {
                pO = Library.GetPointsFromUser("Chọn điểm gốc:");
                pX = Library.GetPointsFromUser("Chọn điểm đánh dấu trục X:");
                pY = Library.GetPointsFromUser("Chọn điểm đánh dấu trục Y:");
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Lỗi khởi tạo", "Trợ giúp", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {


                double heightext = Library.GetIntegerFromUser("Nhập chiều cao chữ ( ví dụ 25):");
                Data.heightext = heightext;
                int datespace = 20;
                try
                {
                    datespace = Library.GetIntegerFromUser("Bước thời gian trục X (đv: ngày, ví dụ : Nhập 20 với bước là 20 ngày):");
                    Data.datetimeline = datespace;
                }
                catch (Exception)
                {
                    MessageBox.Show("Lỗi nhập liệu, giá trị mặc định sẽ lấy 20 ngày", "Trợ giúp", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    //throw;
                }

                int tonspace = 5;
                try
                {
                    tonspace = Library.GetIntegerFromUser("Bước q(tấn/ngày) trục Y (đv: tấn/ngày, ví dụ chọn bước 5 tấn, nhập 5):");
                }
                catch (Exception)
                {
                    MessageBox.Show("Lỗi nhập liệu, giá trị mặc định sẽ lấy 5 tấn", "Trợ giúp", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    //throw;
                }

                Data.tonspaceline = tonspace;
                //Tính toán tỷ lệ

                double MaxY = Data.input.MaxY;
                double deltaX = Library.distance(pO, pX) / (Data.input.ThoiGian.Count + datespace); //Extend X axle
                double deltaY = Library.distance(pO, pY) / (Convert.ToInt32(MaxY) + tonspace); // Extend Y axle

                //giá trị hàng ngày lớn nhất (tấn)
                int ny = Convert.ToInt32(MaxY) + 1;

                // Trục Ox -  Trục thời gian
                for (int i = 1; i <= Data.input.ThoiGian.Count; i++)
                {
                    if (i == Data.input.ThoiGian.Count)
                    {
                        // 
                        Library.layercurrent("CHUNGAY");
                        Library.CreateSingleText(new Point3d((pO.X + (i * deltaX)), (pO.Y - (1.1 * heightext)), 0), "t (ngày)", 0, heightext);
                        break;
                    }

                    if ((i % datespace) == 0)
                    {
                        //Vẽ dấu ngang trên trục
                        Library.layercurrent("TRUC");

                        Point3d mark = new Point3d(pO.X + i * deltaX, pO.Y, 0);
                        Library.markAxis(mark, heightext, true);

                        //Ghi chữ


                        Library.layercurrent("CHU");
                        Library.CreateSingleText(new Point3d(pO.X + i * deltaX, pO.Y - 1.1 * heightext, 0), ("" + i), 0, heightext);
                    }

                }

                Point3d beforePoint = new Point3d(0, 0, 0);
                double beforeVal = 0;

                for (int i = 0; i < Data.input.ThoiGian.Count; i++)
                {
                    Library.layercurrent("NGAY");
                    Point3d pleftbot = new Point3d(pO.X + (i * deltaX), pO.Y, 0);
                    Point3d plefttop = new Point3d(pleftbot.X, pleftbot.Y + Data.input.HangNgay[i] * deltaY, 0);
                    Point3d prighttop = new Point3d(pleftbot.X + deltaX, plefttop.Y, 0);
                    Point3d prightbot = new Point3d(prighttop.X, pleftbot.Y, 0);

                    Library.line(plefttop, prighttop);
                    Library.line(plefttop, pleftbot);
                    Library.line(prighttop, prightbot);


                    if (i >= 1)
                    {
                        if (Data.input.HangNgay[i] != Data.input.HangNgay[i - 1] && Data.input.HangNgay[i] != 0)
                        {
                            Library.layercurrent("CHU");


                            double curVal = Math.Round(Data.input.HangNgay[i], 3);
                            double tmpX = plefttop.X + 1 / 2 * deltaX;
                            double tmpY = plefttop.Y;

                            Point3d currentPoint = new Point3d(tmpX, tmpY, 0);

                            Point3d textPoint = Library.OverPoint3D(currentPoint, curVal, heightext, beforePoint, beforeVal, pO);


                            Library.CreateSingleText(new Point3d(textPoint.X, textPoint.Y, 0),
                              curVal.ToString(), 0, heightext);

                            beforePoint = new Point3d(tmpX, tmpY, 0);
                            beforeVal = curVal;
                        }


                    }
                }

                // 
                // 
                for (int i = 0; i <= ny; i++)
                {
                    if (i == ny)
                    {
                        // 
                        Library.layercurrent("CHU");
                        Library.CreateSingleText(new Point3d(pO.X + 1 / 4 * deltaX * 5, pO.Y + i * deltaY, 0), "q (tấn/ngày)", 0, heightext);

                    }

                    if ((i % tonspace) == 0)
                    {
                        Library.layercurrent("TRUC");
                        Point3d mark = new Point3d(pO.X, pO.Y + i * deltaY, 0);
                        Library.markAxis(mark, heightext, false);

                        //Library.line(pO.X - 1 / 4 * deltaX * 5, pO.Y + i * deltaY, pO.X + 1 / 4 * deltaX * 5, pO.Y + i * deltaY);

                        Library.layercurrent("CHU");
                        Library.CreateSingleText(new Point3d(pO.X + heightext, pO.Y + i * deltaY - 1.1 * (heightext / 2), 0), ("" + i), 0, heightext);




                    }

                }

                MessageBox.Show("Vẽ thành công ", "Thông tin", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Lỗi thực thi ", "Trợ giúp", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

        }


        public static Point3d pOkh = new Point3d();
        public static Point3d pXkh = new Point3d();
        public static Point3d pYkh = new Point3d();
        public static Point3d pDtr = new Point3d();

        [CommandMethod("VEBIEUDOKEHOACH")]
        public static void VeBieuDoKeHoach()
        {
            try
            {
                pOkh = Library.GetPointsFromUser("Chọn điểm gốc:");
                pXkh = Library.GetPointsFromUser("Chọn điểm đánh dấu trục X:");
                pYkh = Library.GetPointsFromUser("Chọn điểm đánh dấu trục Y:");

            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Lỗi khởi tạo", "Trợ giúp", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {

                double heightext = Data.heightext;//Library.GetIntegerFromUser("Nhập chiều cao chữ ( ví dụ 25):");
                int datespace = 20;
                try
                {
                    datespace = Data.datetimeline;// Library.GetIntegerFromUser("Bước thời gian trục X (đv: ngày, ví dụ : Nhập 20 với bước là 20 ngày):");
                }
                catch (Exception)
                {
                    MessageBox.Show("Lỗi nhập liệu, giá trị mặc định sẽ lấy 20 ngày", "Trợ giúp", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    //throw;
                }

                double MaxY = Data.input.CongDonKeHoach.Last();
                int tonspace = 20;
                try
                {
                    tonspace = Library.GetIntegerFromUser("Bước q(tấn) trục Y (đv: tấn, ví dụ chọn bước 50 tấn, nhập 50), GIÁ TRỊ LỚN NHẤT " + Math.Round(MaxY, 2) + ", vui lòng nhập =");
                }
                catch (Exception)
                {
                    MessageBox.Show("Lỗi nhập liệu, giá trị mặc định sẽ lấy 20 tấn", "Trợ giúp", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    //throw;
                }


                //Tính toán tỷ lệ


                double deltaX = Library.distance(pOkh, pXkh) / (Data.input.ThoiGian.Count + datespace); //Extend X axle
                double deltaY = Library.distance(pOkh, pYkh) / (Convert.ToInt32(MaxY) + tonspace); // Extend Y axle

                Data.DeltaXkh = deltaX;
                Data.DeltaYkh = deltaY;


                //giá trị hàng ngày lớn nhất (tấn)
                int ny = Convert.ToInt32(MaxY) + 1;

                // Trục Ox -  Trục thời gian
                for (int i = 1; i <= Data.input.ThoiGian.Count; i++)
                {
                    if (i == Data.input.ThoiGian.Count)
                    {
                        // 
                        Library.layercurrent("CHUNGAY");
                        Library.CreateSingleText(new Point3d((pOkh.X + (i * deltaX)), (pOkh.Y - (1.1 * heightext)), 0), "t (ngày)", 0, heightext);
                        break;
                    }

                    if ((i % datespace) == 0)
                    {
                        //Vẽ dấu ngang trên trục
                        Library.layercurrent("TRUC");

                        Point3d mark = new Point3d(pOkh.X + i * deltaX, pOkh.Y, 0);
                        Library.markAxis(mark, heightext, true);

                        //Ghi chữ


                        Library.layercurrent("CHU");
                        Library.CreateSingleText(new Point3d(pOkh.X + i * deltaX, pOkh.Y - 1.1 * heightext, 0), ("" + i), 0, heightext);
                    }

                }



                //Vẽ đường vận chuyển và thực tế

                for (int i = 1; i < Data.input.CongDonKeHoach.Count - 1; i++)
                {

                    Point3d pleftbot = new Point3d(pOkh.X + (i * deltaX), pOkh.Y, 0);
                    Point3d plefttop = new Point3d(pleftbot.X, pleftbot.Y + Data.input.CongDonKeHoach[i - 1] * deltaY, 0);
                    Point3d prightbot = new Point3d(pleftbot.X + deltaX, pleftbot.Y, 0);
                    Point3d prighttop = new Point3d(prightbot.X, prightbot.Y + Data.input.CongDonKeHoach[i] * deltaY, 0);

                    Library.layercurrent("KEHOACH");
                    Library.line(plefttop, prighttop);
                    Library.layercurrent("NGAY");
                    Library.line(plefttop, pleftbot);
                    Library.line(prighttop, prightbot);

                }

                //Vẽ đường vận chuyển và thực tế

                for (int i = 1; i < Data.input.CongDonThucTe.Count - 1; i++)
                {
                    Point3d pleftbot = new Point3d(pOkh.X + ((i) * deltaX), pOkh.Y, 0);
                    Point3d plefttop = new Point3d(pleftbot.X, pleftbot.Y + Data.input.CongDonThucTe[i - 1] * deltaY, 0);
                    Point3d prightbot = new Point3d(pleftbot.X + deltaX, pleftbot.Y, 0);
                    Point3d prighttop = new Point3d(prightbot.X, prightbot.Y + Data.input.CongDonThucTe[i] * deltaY, 0);

                    Library.layercurrent("THUCTE");
                    Library.line(plefttop, prighttop);


                }

                // 
                // Ghi chữ ngày
                for (int i = 0; i <= ny; i++)
                {
                    if (i == ny)
                    {
                        // 
                        Library.layercurrent("CHU");
                        Library.CreateSingleText(new Point3d(pOkh.X + 1 / 4 * deltaX * 5, pOkh.Y + i * deltaY, 0), "q (tấn/ngày)", 0, heightext);

                    }

                    if ((i % tonspace) == 0)
                    {
                        Library.layercurrent("TRUC");
                        Point3d mark = new Point3d(pOkh.X, pOkh.Y + i * deltaY, 0);
                        Library.markAxis(mark, heightext, false);

                        //Library.line(pO.X - 1 / 4 * deltaX * 5, pO.Y + i * deltaY, pO.X + 1 / 4 * deltaX * 5, pO.Y + i * deltaY);

                        Library.layercurrent("CHU");
                        Library.CreateSingleText(new Point3d(pOkh.X + heightext, pOkh.Y + i * deltaY - 1.1 * (heightext / 2), 0), ("" + i), 0, heightext);




                    }

                }

                MessageBox.Show("Vẽ thành công ", "Thông tin", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Lỗi thực thi ", "Trợ giúp", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

        }


        [CommandMethod("VEBIEUDOVANCHUYEN")]
        public static void VeBieuDoVanChuyen()
        {


            try
            {

                double heightext = Data.heightext;//Library.GetIntegerFromUser("Nhập chiều cao chữ ( ví dụ 25):");


                double Qsxe = 0;



                try
                {
                    Qsxe = Library.GetDoubleFromUser("Năng suất 1 xe Q(tấn/ca)=");
                }
                catch (Exception)
                {

                    //throw;
                }

                Point3d pc1 = Library.GetPointsFromUser("Chọn điểm bắt đầu");

                double BGVanChuyen = 0;
                double BGKehoach = 0;
                double BGThucte = 0;

                int BGday = ReturnDay(pc1, ref BGKehoach, ref BGThucte);

                if (Data.DSOutData.Count > 0)
                {
                    Data.DSOutData.Clear();
                }


                int Nxe = 0;

                Data.DSOutData.Add(new Outpudata(BGday, BGVanChuyen, BGKehoach, BGThucte, Nxe, 0));
                int k = 0;
                int currentday = 0;
                double nowVanchuyen = 0;
                do
                {
                    //Neu nhan nut ESC
                    if (HostApplicationServices.Current.UserBreak())
                    {
                        return;
                    }

                    k++;

                    Nxe = 1;
                    int beforeday = Data.DSOutData[k - 1].IDday;
                    double beforeVanChuyen = Data.DSOutData[k - 1].VanChuyen;

                    Point3d PickPoint = Library.GetPointsFromUser("Chọn điểm tiếp theo:");

                    int tieptuc = Library.GetIntegerFromUser("Tiếp tục vẽ nhập 1, DỪNG nhập 0");

                    if (tieptuc == 0) break;

                    double currentThucte = 0;
                    double currentKehoach = 0;
                    double currentVanchuyen = 0;

                    currentday = ReturnDay(PickPoint, ref currentKehoach, ref currentThucte);

                    int SoCa = currentday - beforeday;

                    if (beforeVanChuyen < currentThucte)
                    {
                        currentVanchuyen = beforeVanChuyen + Qsxe * SoCa * Nxe;
                        if (currentVanchuyen < currentThucte)
                        {
                            do
                            {
                                Nxe++;
                                currentVanchuyen = beforeVanChuyen + Qsxe * SoCa * Nxe;
                            } while (currentVanchuyen < currentThucte); //Điều kiện sai thì thoát vòng lặp
                        }
                        Library.layercurrent("VANCHUYEN");

                        Point3d start = new Point3d(pOkh.X + beforeday * Data.DeltaXkh, pOkh.Y + beforeVanChuyen * Data.DeltaYkh, 0);
                        Point3d end = new Point3d(pOkh.X + currentday * Data.DeltaXkh, pOkh.Y + currentVanchuyen * Data.DeltaYkh, 0);

                        Library.line(start, end);

                       

                        Point3d textPoint = new Point3d(start.X + 0.5 * heightext, start.Y + 0.1 * heightext, 0);

                        double angle = AngleVector(start, end);

                        Library.layercurrent("CHU_XE");
                        Library.CreateSingleText(textPoint, Nxe + "xe (" + SoCa + ")", angle, heightext);


                        Library.layercurrent("CHU_VC");
                        Point3d textPointEnd = new Point3d(end.X, end.Y + 0.5 * heightext, 0);

                        Library.CreateSingleText(textPointEnd, currentVanchuyen.ToString(), 90, heightext);

                        nowVanchuyen = currentVanchuyen;
                    }
                    else
                    {
                        currentVanchuyen = nowVanchuyen;
                        Nxe = 0;
                        SoCa = 0;

                        Library.layercurrent("VANCHUYEN");

                        Point3d start = new Point3d(pOkh.X + beforeday * Data.DeltaXkh, pOkh.Y + beforeVanChuyen * Data.DeltaYkh, 0);
                        Point3d end = new Point3d(pOkh.X + currentday * Data.DeltaXkh, pOkh.Y + currentVanchuyen * Data.DeltaYkh, 0);

                        Library.line(start, end);
                    }


                    Data.DSOutData.Add(new Outpudata(currentday, currentVanchuyen, currentKehoach, currentThucte, Nxe, SoCa));




                } while (currentday < Data.input.ThoiGian.Last());



                MessageBox.Show("Vẽ thành công ", "Thông tin", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Lỗi thực thi ", "Trợ giúp", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

        }



        [CommandMethod("VEBIEUDODUTRU")]
        public static void VeBieuDoDuTru()
        {




            try
            {

                try
                {
                    pDtr = Library.GetPointsFromUser("Chọn điểm đánh dấu trục vẽ biểu đồ dự trữ (Trục âm của Trục Y đồ thị):");


                }
                catch (System.Exception ex)
                {
                    MessageBox.Show("Lỗi khởi tạo", "Trợ giúp", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }



                List<double> listDutru = new List<double>();

                for (int i = 0; i < Data.DSOutData.Count; i++)
                {
                    listDutru.Add(Data.DSOutData[i].DuTru);
                }

                double maxY = listDutru.Max();

                double tonspace =
                    Library.GetDoubleFromUser("Bước trục biểu đồ dự trữ (Ví dụ) 5 tấn, GIÁ TRỊ LỚN NHẤT là " + Math.Round(maxY, 2) + "(tấn), vui lòng nhập bước=");

                double deltaY = Library.distance(pOkh, pDtr) / maxY;

                int ny = Convert.ToInt32(maxY);
                double heightext = Data.heightext;
                // Ghi chữ ngày
                for (int i = 0; i <= ny; i++)
                {


                    if ((i % tonspace) == 0)
                    {
                        Library.layercurrent("TRUC");
                        Point3d mark = new Point3d(pOkh.X, pOkh.Y - i * deltaY, 0);
                        Library.markAxis(mark, heightext, false);

                        //Library.line(pO.X - 1 / 4 * deltaX * 5, pO.Y + i * deltaY, pO.X + 1 / 4 * deltaX * 5, pO.Y + i * deltaY);

                        Library.layercurrent("CHU");
                        Library.CreateSingleText(new Point3d(pOkh.X + heightext, pOkh.Y - i * deltaY - 1.1 * (heightext / 2), 0), ("" + i), 0, heightext);

                    }

                }

                for (int i = 1; i < Data.DSOutData.Count; i++)
                {

                    int befday = Data.DSOutData[i - 1].IDday;
                    double befdutru = Data.DSOutData[i - 1].DuTru;

                    Point3d befintoDutru = new Point3d(pOkh.X + Data.DeltaXkh * befday, pOkh.Y - befdutru * deltaY, 0);

                    int curday = Data.DSOutData[i].IDday;
                    double kehoach = Data.DSOutData[i].Kehoach;
                    double dutru = Data.DSOutData[i].DuTru;

                    Library.layercurrent("CHU_VC");

                    Point3d intoKehoach = new Point3d(pOkh.X + Data.DeltaXkh * curday, pOkh.Y + kehoach * Data.DeltaYkh, 0);
                    Point3d Center = new Point3d(pOkh.X + Data.DeltaXkh * curday, pOkh.Y, 0);

                    Point3d textKehoach = new Point3d((intoKehoach.X + Center.X) / 2, (intoKehoach.Y + Center.Y) / 2, 0);

                    Library.CreateSingleText(textKehoach, Math.Round(kehoach, 2).ToString(), 90, heightext);

                    Point3d intoDutru = new Point3d(pOkh.X + Data.DeltaXkh * curday, pOkh.Y - dutru * deltaY, 0);


                    Point3d textDutru = new Point3d(intoDutru.X, intoDutru.Y - 4 * heightext, 0);

                    Library.CreateSingleText(textDutru, Math.Round(dutru, 2).ToString(), 90, heightext);

                    Library.layercurrent("KEHOACH");

                    Library.line(intoKehoach, Center);
                    Library.line(intoDutru, Center);

                    Library.layercurrent("DUTRU");
                    Library.line(intoDutru, befintoDutru);

                }




                MessageBox.Show("Vẽ thành công ", "Thông tin", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Lỗi thực thi ", "Trợ giúp", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

        }



        public static double AngleVector(Point3d _start, Point3d _end)
        {
            double angle = 0;

            double xDiff = _end.X - _start.X;
            double yDiff = _end.Y - _start.Y;
            angle = (double)Math.Atan2(yDiff, xDiff) * (double)(180 / Math.PI);

            return angle;
        }

        public static int ReturnDay(Point3d _pickPoint, ref double _KeHoach, ref double _ThucTe)
        {
            int day = 0;

            double distance = (_pickPoint.X - pOkh.X) / Data.DeltaXkh;

            day = Convert.ToInt32(distance);

            for (int i = 0; i < Data.input.CongDonKeHoach.Count; i++)
            {
                if (i == day)
                {
                    _KeHoach = Data.input.CongDonKeHoach[i];


                }
            }
            for (int i = 0; i < Data.input.CongDonThucTe.Count; i++)
            {
                if (i == day)
                {

                    _ThucTe = Data.input.CongDonThucTe[i];

                }
            }

            return day;
        }
       

        /// <summary>
        /// THUC THI CUA CHUONG TRINH
        /// </summary>
        public void Initialize()
        {

            ApplicationMenu.Attach();

        }
        /// <summary>
        /// KET THUC
        /// </summary>
        public void Terminate()

        {

            ApplicationMenu.Detach();

        }

      

        [CommandMethod("INFOVIET")]
        public static void ThongtinBanQuyen()
        {
            //frm_LicenseInformation frm = new frm_LicenseInformation();
            //frm.Show();
        }

        [CommandMethod("LICENSEVIET")]
        public static void KichHoat()
        {
            //frmActivation frm = new frmActivation();
            //frm.Show();
        }


        [CommandMethod("VIETCAD")]

        static public void AttachContextMenus()
        {

            //ApplicationMenu.CheckError();
            ApplicationMenu.Attach();

            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            ed.WriteMessage("\nĐã tải xong chương trình");
            ed.WriteMessage("\nVui lòng click chuột phải để thao tác menu chương trình");

        }

        [CommandMethod("NOCONTEXT")]

        static public void DetachContextMenus()

        {



            ApplicationMenu.Detach();

        }


        [CommandMethod("COUNT", CommandFlags.UsePickSet)]

        static public void CountSelection()

        {

            Editor ed =

              Application.DocumentManager.MdiActiveDocument.Editor;


            PromptSelectionResult psr = ed.GetSelection();

            if (psr.Status == PromptStatus.OK)

            {

                ed.WriteMessage(

                  "\nSelected {0} entities.",

                  psr.Value.Count

                );

            }

        }

    }

    /// <summary>
    /// Menu dem so luong doi tuong
    /// </summary>
    public class CountMenu

    {

        private static ContextMenuExtension cme;


        public static void Attach()

        {

            if (cme == null)

            {

                cme = new ContextMenuExtension();

                MenuItem mi = new MenuItem("Count");

                mi.Click += new EventHandler(OnCount);

                cme.MenuItems.Add(mi);

            }

            RXClass rxc = Entity.GetClass(typeof(Entity));

            Application.AddObjectContextMenuExtension(rxc, cme);

        }

        public static void Detach()

        {

            //RXClass rxc = Entity.GetClass(typeof(Entity));

            //Application.RemoveObjectContextMenuExtension(rxc, cme);

        }

        private static void OnCount(Object o, EventArgs e)

        {

            Document doc =

              Application.DocumentManager.MdiActiveDocument;

            doc.SendStringToExecute("_.COUNT ", true, false, false);

        }

    }

    /// <summary>
    /// MENU UNG DUNG
    /// </summary>
    public class ApplicationMenu

    {

        private static ContextMenuExtension cmeKhoiTaoTuyen;
        private static ContextMenuExtension cmeAnToanGiaoThong;
        private static ContextMenuExtension cmeHuHongDuong;

        private static ContextMenuExtension cmeTuyChonVe;
        private static ContextMenuExtension cmeThongTin;




        public static void Attach()

        {

            if (cmeKhoiTaoTuyen == null)

            {

                cmeKhoiTaoTuyen = new ContextMenuExtension();

                cmeKhoiTaoTuyen.Title = "Khởi tạo và nhập liệu";

                MenuItem mi1 = new MenuItem("Nhập liệu");

                mi1.Click += new EventHandler(On1stNhapLieu);

                cmeKhoiTaoTuyen.MenuItems.Add(mi1);

                MenuItem mi2 = new MenuItem("Khởi tạo layer");

                mi2.Click += new EventHandler(On2ndBinhDoCau);

                cmeKhoiTaoTuyen.MenuItems.Add(mi2);



            }
            Application.AddDefaultContextMenuExtension(cmeKhoiTaoTuyen);

            if (cmeAnToanGiaoThong == null)
            {
                cmeAnToanGiaoThong = new ContextMenuExtension();

                cmeAnToanGiaoThong.Title = "Vẽ các biểu đồ";


                MenuItem mi1 = new MenuItem("Biểu độ tiêu thụ hàng ngày");

                mi1.Click += new EventHandler(On1stBinhDoBienBao);

                cmeAnToanGiaoThong.MenuItems.Add(mi1);

                MenuItem mi2 = new MenuItem("Biểu đồ kế hoạch/thực tế");

                mi2.Click += new EventHandler(On2ndBinhDoTonSong);

                cmeAnToanGiaoThong.MenuItems.Add(mi2);
            }
            Application.AddDefaultContextMenuExtension(cmeAnToanGiaoThong);
            if (cmeHuHongDuong == null)
            {
                cmeHuHongDuong = new ContextMenuExtension();

                cmeHuHongDuong.Title = "Thiết kế vận chuyển vật liệu";

                MenuItem mi1 = new MenuItem("Vẽ đường vận chuyển");

                mi1.Click += new EventHandler(On1stHuHongDuong);

                cmeHuHongDuong.MenuItems.Add(mi1);

                MenuItem mi2 = new MenuItem("Điền biểu đồ dự trữ");

                mi2.Click += new EventHandler(On2ndVeHuHongDuong);

                cmeHuHongDuong.MenuItems.Add(mi2);
            }

            Application.AddDefaultContextMenuExtension(cmeHuHongDuong);



            if (cmeTuyChonVe == null)
            {
                cmeTuyChonVe = new ContextMenuExtension();
                cmeTuyChonVe.Title = "Tùy chọn vẽ";




            }

            Application.AddDefaultContextMenuExtension(cmeTuyChonVe);

        }

        public static void Detach()

        {
            Application.RemoveDefaultContextMenuExtension(cmeKhoiTaoTuyen);

        }


        private static void OnAbout(Object o, EventArgs e)
        {

            Document doc = Application.DocumentManager.MdiActiveDocument;

            doc.SendStringToExecute("_.ABOUTVIET", true, false, false);

            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            ed.WriteMessage("\nVui lòng click chuột phải");
        }

        private static void OnInformation(Object o, EventArgs e)
        {

            Document doc = Application.DocumentManager.MdiActiveDocument;

            doc.SendStringToExecute("_.INFOVIET", true, false, false);

            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            ed.WriteMessage("\nVui lòng click chuột phải");
        }

        private static void OnLicense(Object o, EventArgs e)
        {

            Document doc = Application.DocumentManager.MdiActiveDocument;

            doc.SendStringToExecute("_.LICENSEVIET", true, false, false);
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            ed.WriteMessage("\nVui lòng click chuột phải");

        }



        private static void On1stHuHongDuong(Object o, EventArgs e)
        {

            Document doc = Application.DocumentManager.MdiActiveDocument;

            doc.SendStringToExecute("_.VEBIEUDOVANCHUYEN", true, false, false);
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            ed.WriteMessage("\nClick chuột phải để bắt đầu vẽ");
        }

        private static void On2ndVeHuHongDuong(Object o, EventArgs e)
        {

            Document doc = Application.DocumentManager.MdiActiveDocument;

            doc.SendStringToExecute("_.VEBIEUDODUTRU", true, false, false);
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            ed.WriteMessage("\nClick chuột phải để bắt đầu vẽ");
        }

        private static void On1stBinhDoBienBao(Object o, EventArgs e)
        {

            Document doc = Application.DocumentManager.MdiActiveDocument;

            doc.SendStringToExecute("_.VEBIEUDOTIEUTHU", true, false, false);
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            ed.WriteMessage("\nClick chuột phải để bắt đầu vẽ");
        }

        private static void On2ndBinhDoTonSong(Object o, EventArgs e)
        {

            Document doc = Application.DocumentManager.MdiActiveDocument;

            doc.SendStringToExecute("_.VEBIEUDOKEHOACH", true, false, false);
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            ed.WriteMessage("\nClick chuột phải để bắt đầu vẽ");

        }

        private static void On1stNhapLieu(Object o, EventArgs e)

        {

            Document doc = Application.DocumentManager.MdiActiveDocument;

            doc.SendStringToExecute("_.NHAPLIEUVATLIEU", true, false, false);
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            ed.WriteMessage("\nClick chuột phải để bắt đầu vẽ");

        }

        private static void On2ndBinhDoCau(Object o, EventArgs e)

        {

            Document doc = Application.DocumentManager.MdiActiveDocument;

            doc.SendStringToExecute("_.KHOITAOLAYER", true, false, false);
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            ed.WriteMessage("\nClick chuột phải để bắt đầu vẽ");

        }

        private static void On3rdBinhDoThoatNuoc(Object o, EventArgs e)

        {

            Document doc = Application.DocumentManager.MdiActiveDocument;

            doc.SendStringToExecute("_.BINHDOCONG", true, false, false);
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            ed.WriteMessage("\nClick chuột phải để bắt đầu vẽ");

        }

        
        public static DateTime GetNistTime()
        {
            DateTime dateTime = DateTime.MinValue;
            try
            {


                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://nist.time.gov/actualtime.cgi?lzbc=siqm9b");
                request.Method = "GET";
                request.Accept = "text/html, application/xhtml+xml, */*";
                request.UserAgent = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.1; Trident/6.0)";
                request.ContentType = "application/x-www-form-urlencoded";
                request.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore); //No caching
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    StreamReader stream = new StreamReader(response.GetResponseStream());
                    string html = stream.ReadToEnd();//<timestamp time=\"1395772696469995\" delay=\"1395772696469995\"/>
                    string time = Regex.Match(html, @"(?<=\btime="")[^""]*").Value;
                    double milliseconds = Convert.ToInt64(time) / 1000.0;
                    dateTime = new DateTime(1970, 1, 1).AddMilliseconds(milliseconds).ToLocalTime();
                }

            }
            catch (Exception)
            {

                //throw;
            }

            return dateTime;
        }

    }


}
