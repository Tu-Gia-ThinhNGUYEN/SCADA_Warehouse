using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SCADA_WAREHOUSE
{
    public static class class_KEPServerEX
    {
        // Class Khai báo tag
        public static string[] tagread(int tagnumber)
        {
            string tagID_1 = "Channel1.Device1.tag_Bool";
            string tagID_2 = "Channel1.Device1.tag_Byte";
            string tagID_3 = "Channel1.Device1.tag_Integer";
            string tagID_4 = "Channel1.Device1.tag_Real";

            string[] tags;
            tags = new string[tagnumber];
            tags.SetValue(tagID_1, 1);
            tags.SetValue(tagID_2, 2);
            tags.SetValue(tagID_3, 3);
            tags.SetValue(tagID_4, 4);
            return tags;
        }
        // Class tạo array đọc ID tags - mặc định không đổi
        public static Int32[] tagID(int tagnumber)
        {
            Int32[] cltarr;
            cltarr = new Int32[tagnumber];
            for (int i = 1; i < tagnumber; i++)
            {
                cltarr.SetValue(i, i);
            }
            return cltarr;
        }
    }
}