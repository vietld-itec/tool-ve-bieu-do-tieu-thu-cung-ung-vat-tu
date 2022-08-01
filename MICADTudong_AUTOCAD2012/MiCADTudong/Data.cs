using System.Linq;
using Autodesk.AutoCAD.Geometry;

using System;
using System.Collections.Generic;
using System.Windows.Forms;


namespace ACAD_BieuDoKeHoachVatTu
{

    #region New Structure

    public struct Outpudata
    {
        public int IDday;
        public double VanChuyen;
        public double Kehoach;
        public double ThucTe;
        public double DuTru;

        public int SoXe;
        public int SoCa;


        public Outpudata(int _IDday, double _VanChuyen, double _Kehoach, double _ThucTe, int _SoXe, int _SoCa)
        {
            IDday = _IDday;

            VanChuyen = _VanChuyen;
            Kehoach = _Kehoach;
            ThucTe = _ThucTe;

            double _DuTru = VanChuyen - Kehoach;

            DuTru = _DuTru;
            SoXe = _SoXe;
            SoCa = _SoCa;
        }

    }

    public struct Inputdata
    {
        public string TenVatLieu;
        public int NgayDuTru;
        public double MaxY;
        public List<int> ThoiGian;
        public List<double> HangNgay;
        public List<double> CongDonKeHoach;
        public List<double> CongDonThucTe;

        public Inputdata(string _TenVatLieu, int _NgayDuTru, List<int> _ThoiGian, List<double> _HangNgay)
        {

            TenVatLieu = _TenVatLieu;
            NgayDuTru = _NgayDuTru;
            ThoiGian = _ThoiGian;
            HangNgay = _HangNgay;

            List<double> _CongDonKeHoach = new List<double>();
            List<double> _CongDonThucTe = new List<double>();
            double _MaxY = 0;
            if (_HangNgay.Count > 1)
            {
                double initial = _HangNgay[0];
                _CongDonKeHoach.Add(initial);
                for (int i = 1; i < _HangNgay.Count; i++)
                {
                    double tmp = _HangNgay[i];
                    if (tmp >= _MaxY)
                    {
                        _MaxY = tmp;
                    }
                    initial = initial + tmp;
                    _CongDonKeHoach.Add(initial);
                }
                for (int i = 0; i < _CongDonKeHoach.Count + NgayDuTru; i++)
                {
                    if (i >=  NgayDuTru && i < _CongDonKeHoach.Count - NgayDuTru)
                    {
                        _CongDonThucTe.Add(_CongDonKeHoach[i + NgayDuTru]);
                        
                    }
                    else if(i >= _CongDonKeHoach.Count - NgayDuTru)
                    {
                        _CongDonThucTe.Add(_CongDonKeHoach.Last());
                    }


                }

            }
            MaxY = _MaxY;
            CongDonKeHoach = _CongDonKeHoach;
            CongDonThucTe = _CongDonThucTe;
        }
    }

    #endregion



    public class Data
    {
        public static string TenVatLieu = "";
        public static int NgayDuTru = 3;

        public static Inputdata input = new Inputdata();
        public static double heightext = 0;
        public static byte[] _certPubicKeyData;


        public static bool error = false;

        public static string path = "";
        public static string pathhacth = "";
        public static string pathpic = "";

        public static Point3d[] DSPoint;
        public static double DSPointVal;


        public static int datetimeline = 0;
        public static int tonspaceline = 0;

        public static List<Outpudata> DSOutData = new List<Outpudata>();
        public static double DeltaXkh = 0;
        public static double DeltaYkh = 0;


    }

}
