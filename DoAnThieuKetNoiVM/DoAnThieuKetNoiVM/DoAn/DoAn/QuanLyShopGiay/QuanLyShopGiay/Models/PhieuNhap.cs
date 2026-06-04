using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyShopGiay.Models
{
    public partial class PHIEU_NHAP
    {
        public int TongSoLuong
        {
            get
            {
                // Thêm "this." vào trước
                if (this.CT_PHIEU_NHAP == null) return 0;
                return this.CT_PHIEU_NHAP.Sum(x => x.SoLuong ?? 0);
            }
        }

        public decimal TongTien
        {
            get
            {
                // Thêm "this." vào trước
                if (this.CT_PHIEU_NHAP == null) return 0;
                return this.CT_PHIEU_NHAP.Sum(x => (x.SoLuong ?? 0) * (x.GiaNhap ?? 0));
            }
        }
    }
}
