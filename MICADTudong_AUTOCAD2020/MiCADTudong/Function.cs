using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;




namespace ACAD_BieuDoKeHoachVatTu
{

    public static class Function
    {


        public static void GetData(string full)
        {

            if (full == null)
            {
                MessageBox.Show("Vui lòng kiểm tra thao tác copy dữ liệu");
            }
            int count = full.Split('\n').Length;

            string[] arr_dataline = full.Split('\n');

            string s = "";

            //
            List<string> _cot1 = new List<string>();
            List<string> _cot2 = new List<string>();




            for (int i = 0; i < count; i++)
            {

                s = arr_dataline[i];

                if (s != string.Empty )
                {
                    //int col = s.Split('\t').Length;
                    string[] valcol = s.Split('\t');

                    for (int j = 1; j < valcol.Length; j++)
                    {
                        if (valcol[j] == null || valcol[j] == "") valcol[j] = "0";
                    }

                    for (int j = 0; j < valcol.Length; j++)
                    {
                        if (valcol[0].Length > 0)
                        {
                            switch (j)
                            {
                                case 0:
                                    _cot1.Add(valcol[0].ToString());

                                    break;

                                case 1:
                                    if (valcol[1] == "\r")
                                    {
                                        _cot2.Add("0");
                                    }
                                    else
                                    {
                                        _cot2.Add(valcol[1].ToString());
                                    }
                                    break;
                            }

                        }

                    }



                }

            }

            List<int> Thoigian = ConvertToListInteger(_cot1);
            List<double> dataList = ConvertToListDouble(_cot2);

            Data.input = new Inputdata(Data.TenVatLieu, Data.NgayDuTru, Thoigian, dataList);


           

            //ACAD_VanChuyenVatLieu.StreamFile.WriteTextFile.WriteGeneralMatrix("", matrix1,"Thoi gian", true, false);
            //ACAD_VanChuyenVatLieu.StreamFile.WriteTextFile.WriteGeneralMatrix("", matrix2, "Hang ngay", true, false);

            //ACAD_VanChuyenVatLieu.StreamFile.WriteTextFile.WriteGeneralMatrix("", matrix3, "Ke hoach", true, false);
            //ACAD_VanChuyenVatLieu.StreamFile.WriteTextFile.WriteGeneralMatrix("", matrix4, "Thuc te", true, false);


        }


        //***********************************************************************************************************************************************************************
        //********************************************      DANH MUC HAM THUC THI TRUY XUAT VA LAY DU LIEU BINH DO CAC THANH PHAN TREN TUYEN         ****************************
        //***********************************************************************************************************************************************************************

        public static List<int> ConvertToListInteger(List<string> _listString)
        {
            List<int> _kq = new List<int>();

            for (int i = 0; i < _listString.Count; i++)
            {
                int tmp = Convert.ToInt32(_listString[i].ToString());
                _kq.Add(tmp);
            }

            return _kq;
        }

        /// <summary>
        /// Convert from List String to List Double
        /// </summary>
        /// <param name="_listString"></param>
        /// <returns></returns>
        public static List<double> ConvertToListDouble(List<string> _listString)
        {
            List<double> _kq = new List<double>();

            for (int i = 0; i < _listString.Count; i++)
            {
                double tmp = Convert.ToDouble(_listString[i].ToString());
                _kq.Add(tmp);
            }

            return _kq;
        }

        /// <summary>
        /// lAY DU LIEU TU CLIP BOEAR
        /// </summary>
        /// <returns></returns>
        public static string GetTextClipboard()
        {
            string returnText = null;
            if (Clipboard.ContainsText(TextDataFormat.UnicodeText))
            {
                returnText = Clipboard.GetText(TextDataFormat.UnicodeText);

            }
            try
            {
                Clipboard.Clear();
            }
            catch (Exception)
            {

                //throw;
            }
            return returnText;
        }







    }

}
