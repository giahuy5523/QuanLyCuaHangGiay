using Microsoft.Reporting.WinForms;
using QuanLyShopGiay.Models;
using System;
using System.Linq;
using System.Windows.Forms;

namespace QuanLyShopGiay.Reports
{
    public class ReportService
    {
        private ReportViewer _reportViewer;

        public ReportViewer GetReportViewer() => _reportViewer;

        public ReportViewer TaoVaHienThiBaoCao(
            string loaiBaoCao,
            string maHD = null,
            DateTime? tuNgay = null,
            DateTime? denNgay = null)
        {
            _reportViewer = new ReportViewer();
            _reportViewer.Dock = DockStyle.Fill;
            _reportViewer.ProcessingMode = ProcessingMode.Local;

            using (var db = new QLShopGiayEntities())
            {
                _reportViewer.LocalReport.DataSources.Clear();

                // ===== BÁO CÁO TỒN KHO =====
                if (loaiBaoCao == "TonKho")
                {
                    var data = db.SanPhams
                        .Select(x => new
                        {
                            STT = "", // Để trống cho RDLC tự tính số thứ tự hoặc map
                            MaSP = x.MaSP,
                            TenSP = x.TenSP,
                            Size = x.Size,
                            MauSac = x.MauSac,
                            GiaBan = x.GiaBan,
                            TonKho = x.SoLuongTon.ToString(), // Đẩy dữ liệu vào trường TonKho
                            SoluongTon = x.SoLuongTon.ToString() // Đẩy đồng thời vào trường SoluongTon viết thường để dự phòng
                        })
                        .ToList();

                    _reportViewer.LocalReport.ReportPath = "Reports/Rpt_TonKho.rdlc";

                    // Sử dụng chính xác tên DataSet thiết kế trong file RDLC của bạn
                    _reportViewer.LocalReport.DataSources.Add(
                        new ReportDataSource("DataSetTonKho", data));
                }

                // ===== BÁO CÁO THỐNG KÊ DOANH THU =====
                // ===== BÁO CÁO THỐNG KÊ DOANH THU =====
                else if (loaiBaoCao == "ThongKe")
                {
                    _reportViewer.LocalReport.ReportPath = "Reports/Rpt_ThongKeDoanhThu.rdlc";

                    // 1. Tự động lấy ngày nhỏ nhất và lớn nhất từ bảng Hóa đơn trong Database
                    // Nếu Database trống chưa có hóa đơn, mặc định lấy từ 30 ngày trước đến ngày hiện tại
                    DateTime dateTuNgay = db.HoaDons.Select(hd => (DateTime?)hd.NgayLap).Min()
                                          ?? DateTime.Now.AddDays(-30);
                    DateTime dateDenNgay = db.HoaDons.Select(hd => (DateTime?)hd.NgayLap).Max()
                                          ?? DateTime.Now;

                    // 2. Gọi Stored Procedure truyền vào 2 mốc ngày lấy từ Database
                    var data = db.sp_ThongKeDoanhThuTheoSanPham(dateTuNgay, dateDenNgay).ToList();

                    // 3. Khai báo Parameter để truyền hiển thị lên giao diện Report
                    ReportParameter[] reportParameters = new ReportParameter[]
                    {
        // Định dạng dd/MM/yyyy để hiển thị lên Report cho đẹp mắt
        new ReportParameter("ParamTuNgay", dateTuNgay.ToString("dd/MM/yyyy")),
        new ReportParameter("ParamDenNgay", dateDenNgay.ToString("dd/MM/yyyy"))
                    };

                    // 4. Thiết lập các tham số và nguồn dữ liệu cho LocalReport
                    _reportViewer.LocalReport.SetParameters(reportParameters);
                    _reportViewer.LocalReport.DataSources.Add(new ReportDataSource("DataSetTK", data));
                }

                _reportViewer.RefreshReport();
            }

            return _reportViewer;
        }
    }
}