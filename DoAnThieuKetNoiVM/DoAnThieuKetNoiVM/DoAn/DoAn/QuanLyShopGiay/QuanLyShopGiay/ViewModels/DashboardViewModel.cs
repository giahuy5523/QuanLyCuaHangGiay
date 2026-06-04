using QuanLyShopGiay.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace QuanLyShopGiay.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private string _revenue, _orderCount, _productCount, _processingCount;
        private ObservableCollection<object> _recentOrders;

        public string Revenue { get => _revenue; set => SetProperty(ref _revenue, value); }
        public string OrderCount { get => _orderCount; set => SetProperty(ref _orderCount, value); }
        public string ProductCount { get => _productCount; set => SetProperty(ref _productCount, value); }
        public string ProcessingCount { get => _processingCount; set => SetProperty(ref _processingCount, value); }
        public ObservableCollection<object> RecentOrders { get => _recentOrders; set => SetProperty(ref _recentOrders, value); }

        public DashboardViewModel()
        {
            LoadData();
        }

        private void LoadData()
        {
            using (var db = new QLShopGiayEntities())
            {
                // 1. Doanh thu: Lấy tất cả đơn "Đã thanh toán" (không lọc ngày)
                Revenue = string.Format("{0:N0} ₫", db.HOA_DON
                    .Where(x => x.TrangThai == "Đã thanh toán") // Chú ý chữ N nếu DB là nvarchar
                    .Sum(x => (decimal?)x.TongTien) ?? 0);

                // 2. Tổng đơn hàng: Đếm tất cả hóa đơn
                OrderCount = db.HOA_DON.Count().ToString();

                // 3. Sản phẩm: Đếm tổng số sản phẩm trong bảng SAN_PHAM
                ProductCount = db.SAN_PHAM.Count().ToString();

                // 4. Đơn xử lý: Đếm các đơn không phải "Đã thanh toán"
                ProcessingCount = db.HOA_DON.Count(x => x.TrangThai != "Đã thanh toán").ToString();

                // 5. Danh sách đơn gần nhất: Giữ nguyên logic lấy 10 đơn mới nhất
                RecentOrders = new ObservableCollection<object>((from h in db.HOA_DON
                                                                 join k in db.KHACH_HANG on h.MaKH equals k.MaKH into customers
                                                                 from k in customers.DefaultIfEmpty()
                                                                 orderby h.NgayLap descending
                                                                 select new
                                                                 {
                                                                     h.MaHD,
                                                                     TenKH = k == null ? "Khách lẻ" : k.TenKH,
                                                                     h.NgayLap,
                                                                     TongTien = h.TongTien ?? 0,
                                                                     h.TrangThai
                                                                 }).Take(10).ToList());
            }
        }
    }
}