using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyShopGiay.Models
{
    public class BienTheHienThiModel
    {
        // Tương ứng với cột MaBienThe của bảng BIEN_THE_SP trong SQL
        public int MaBienThe { get; set; }

        // Chuỗi hiển thị lên giao diện, ví dụ: "Trắng (Tồn: 50)"
        public string TenMauDisplay { get; set; }
    }
}
