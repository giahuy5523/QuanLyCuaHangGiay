using QuanLyShopGiay.Helpers;
using QuanLyShopGiay.Command;
using QuanLyShopGiay.Models;
using QuanLyShopGiay.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;

namespace QuanLyShopGiay.ViewModels
{
    public class HoaDonBanHangViewModel : BaseViewModel
    {
        private QLShopGiayEntities3 db = new QLShopGiayEntities3();

        // ===== HÓA ĐƠN =====
        private string _maHD;
        private string _maKhachHang;
        private DateTime _ngayLap = DateTime.Now;
        private string _trangThai = "Chưa thanh toán";
        private decimal _tongTien;

        // ===== CHI TIẾT NHẬP LIỆU =====
        private ChiTietHoaDon _chiTietHoaDon = new ChiTietHoaDon();
        private ObservableCollection<ChiTietHoaDonBanDisplay> _dsChiTiet;
        private ObservableCollection<SanPham> _dsSanPham;
        private ObservableCollection<KhachHang> _dsKhachHang;
        private SanPham _selectedSanPham;

        // ===== PROPERTIES =====
        public string MaHD
        {
            get => _maHD;
            set { _maHD = value; OnPropertyChanged(); }
        }

        public string MaKhachHang
        {
            get => _maKhachHang;
            set { _maKhachHang = value; OnPropertyChanged(); }
        }

        public DateTime NgayLap
        {
            get => _ngayLap;
            set { _ngayLap = value; OnPropertyChanged(); }
        }

        public string TrangThai
        {
            get => _trangThai;
            set { _trangThai = value; OnPropertyChanged(); }
        }

        public decimal TongTien
        {
            get => _tongTien;
            set { _tongTien = value; OnPropertyChanged(); }
        }

        public ChiTietHoaDon ChiTietHoaDon
        {
            get => _chiTietHoaDon;
            set { _chiTietHoaDon = value; OnPropertyChanged(); }
        }

        public ObservableCollection<ChiTietHoaDonBanDisplay> DsChiTiet
        {
            get => _dsChiTiet;
            set { _dsChiTiet = value; OnPropertyChanged(); }
        }

        public ObservableCollection<SanPham> DsSanPham
        {
            get => _dsSanPham;
            set { _dsSanPham = value; OnPropertyChanged(); }
        }

        public ObservableCollection<KhachHang> DsKhachHang
        {
            get => _dsKhachHang;
            set { _dsKhachHang = value; OnPropertyChanged(); }
        }

        public SanPham SelectedSanPham
        {
            get => _selectedSanPham;
            set
            {
                if (_selectedSanPham == value) return;
                _selectedSanPham = value;

                if (value != null)
                {
                    ChiTietHoaDon.GiaBan = value.GiaBan ?? 0;
                    ChiTietHoaDon.MaSP = value.MaSP;
                }
                OnPropertyChanged();
            }
        }

        // ===== COMMANDS =====
        public RelayCommand TaoHoaDonCommand { get; set; }
        public RelayCommand ThemSanPhamCommand { get; set; }
        public RelayCommand XoaChiTietCommand { get; set; }
        public RelayCommand LuuHoaDonCommand { get; set; }
        public RelayCommand ThanhToanCommand { get; set; }
        public RelayCommand HuyCommand { get; set; }

        public HoaDonBanHangViewModel()
        {
            LoadData();
            InitCommands();
        }

        private void LoadData()
        {
            db?.Dispose();
            db = new QLShopGiayEntities3();

            DsSanPham = new ObservableCollection<SanPham>(
                db.SanPhams.Include("LoaiSanPham").Include("NhaCungCap").ToList()
            );

            DsKhachHang = new ObservableCollection<KhachHang>(
                db.KhachHangs.ToList()
            );

            if (DsChiTiet == null)
            {
                DsChiTiet = new ObservableCollection<ChiTietHoaDonBanDisplay>();
            }
            TongTien = 0;
        }

        private void InitCommands()
        {
            // ===== TẠO HÓA ĐƠN MỚI =====
            TaoHoaDonCommand = new RelayCommand(o =>
            {
                try
                {
                    var lastHD = db.HoaDons.OrderByDescending(h => h.MaHD).FirstOrDefault();
                    int nextNum = 1;
                    string datePrefix = DateTime.Now.ToString("yyyyMMdd");
                    string expectedPrefix = "HD" + datePrefix;

                    if (lastHD != null && lastHD.MaHD.StartsWith(expectedPrefix))
                    {
                        if (int.TryParse(lastHD.MaHD.Substring(10), out int num))
                            nextNum = num + 1;
                    }

                    MaHD = "HD" + datePrefix + nextNum.ToString("D4");
                    NgayLap = DateTime.Now;
                    MaKhachHang = null;
                    TrangThai = "Chưa thanh toán";
                    DsChiTiet.Clear();
                    TongTien = 0;
                    ResetChiTiet();

                    MessageBox.Show($"Tạo hóa đơn {MaHD} thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi tạo mã hóa đơn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });

            // ===== THÊM SẢN PHẨM VÀO GIỎ =====
            ThemSanPhamCommand = new RelayCommand(o =>
            {
                // Ép kiểu chống lỗi null ngầm định
                int hienTaiSoLuong = ChiTietHoaDon.SoLuong ?? 0;
                decimal hienTaiGiaBan = ChiTietHoaDon.GiaBan ?? 0;

                if (hienTaiSoLuong <= 0 || hienTaiGiaBan < 0 || string.IsNullOrEmpty(ChiTietHoaDon.MaSP))
                {
                    MessageBox.Show("Vui lòng chọn sản phẩm và nhập số lượng hợp lệ!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var sanPham = DsSanPham.FirstOrDefault(sp => sp.MaSP == ChiTietHoaDon.MaSP);
                if (sanPham == null)
                {
                    MessageBox.Show("Sản phẩm không tồn tại trong hệ thống!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Sửa lỗi dòng 244 cũ (so sánh an toàn int? với int)
                if ((sanPham.SoLuongTon ?? 0) < hienTaiSoLuong)
                {
                    MessageBox.Show($"Tồn kho không đủ!\nCòn: {sanPham.SoLuongTon ?? 0} cái\nYêu cầu: {hienTaiSoLuong} cái", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var existing = DsChiTiet.FirstOrDefault(ct => ct.MaSP == ChiTietHoaDon.MaSP);

                if (existing != null)
                {
                    int newQuantity = existing.SoLuong + hienTaiSoLuong;
                    if (newQuantity > (sanPham.SoLuongTon ?? 0))
                    {
                        MessageBox.Show($"Tồn kho không đủ cho tổng số lượng này!\nTồn hiện tại: {sanPham.SoLuongTon ?? 0}, Giỏ hàng đã có: {existing.SoLuong}, Thêm: {hienTaiSoLuong}", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    existing.SoLuong = newQuantity;
                    existing.ThanhTien = existing.SoLuong * existing.GiaBan;
                }
                else
                {
                    // Sửa lỗi các dòng 262, 263, 264 cũ (Sử dụng toán tử ?? để ép kiểu sạch từ Nullable về gốc)
                    DsChiTiet.Add(new ChiTietHoaDonBanDisplay
                    {
                        MaSP = sanPham.MaSP,
                        TenSP = sanPham.TenSP,
                        Size = sanPham.Size ?? "",
                        MauSac = sanPham.MauSac ?? "",
                        SoLuong = hienTaiSoLuong,
                        GiaBan = hienTaiGiaBan,
                        ThanhTien = (decimal)(hienTaiSoLuong * hienTaiGiaBan)
                    });
                }

                UpdateTongTien();
                ResetChiTiet();
            });

            // ===== XÓA CHI TIẾT KHỎI GIỎ =====
            XoaChiTietCommand = new RelayCommand(o =>
            {
                if (o is ChiTietHoaDonBanDisplay chiTiet)
                {
                    var result = MessageBox.Show($"Xóa sản phẩm {chiTiet.TenSP} khỏi danh sách?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        DsChiTiet.Remove(chiTiet);
                        UpdateTongTien();
                    }
                }
            });

            // ===== LƯU HÓA ĐƠN XUỐNG DB =====
            LuuHoaDonCommand = new RelayCommand(o =>
            {
                if (string.IsNullOrWhiteSpace(MaHD))
                {
                    MessageBox.Show("Vui lòng nhấn 'Tạo hóa đơn' trước khi lưu!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (string.IsNullOrWhiteSpace(MaKhachHang))
                {
                    MessageBox.Show("Vui lòng chọn khách hàng!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (DsChiTiet.Count == 0)
                {
                    MessageBox.Show("Hóa đơn phải có ít nhất 1 sản phẩm!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    var existingHD = db.HoaDons.FirstOrDefault(h => h.MaHD == MaHD);
                    if (existingHD != null)
                    {
                        MessageBox.Show("Hóa đơn này đã được lưu trước đó!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var hoaDon = new HoaDon
                    {
                        MaHD = MaHD,
                        NgayLap = NgayLap,
                        MaKhachHang = MaKhachHang,
                        MaNhanVien = UserSession.MaNV,
                        TongTien = TongTien,
                        TrangThai = TrangThai
                    };

                    db.HoaDons.Add(hoaDon);
                    db.SaveChanges();

                    foreach (var ct in DsChiTiet)
                    {
                        var chiTiet = new ChiTietHoaDon
                        {
                            MaHD = MaHD,
                            MaSP = ct.MaSP,
                            SoLuong = ct.SoLuong,
                            GiaBan = ct.GiaBan
                        };
                        db.ChiTietHoaDons.Add(chiTiet);
                    }

                    db.SaveChanges();
                    MessageBox.Show($"Lưu hóa đơn {MaHD} thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                    LoadData();
                    ResetForm();
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException ex)
                {
                    var errorMessage = string.Join("\n", ex.EntityValidationErrors.SelectMany(e => e.ValidationErrors.Select(v => v.ErrorMessage)));
                    MessageBox.Show($"Lỗi dữ liệu: {errorMessage}", "Lỗi dữ liệu hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi lưu hóa đơn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });

            // ===== THANH TOÁN HÓA ĐƠN =====
            ThanhToanCommand = new RelayCommand(o =>
            {
                if (string.IsNullOrWhiteSpace(MaHD))
                {
                    MessageBox.Show("Chưa có hóa đơn hợp lệ để thanh toán!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (TrangThai == "Đã thanh toán")
                {
                    MessageBox.Show("Hóa đơn này đã được thanh toán rồi!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var result = MessageBox.Show($"Thanh toán hóa đơn {MaHD}?\nTổng số tiền: {TongTien:#,##0} đ", "Xác nhận thanh toán", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var hoaDon = db.HoaDons.FirstOrDefault(h => h.MaHD == MaHD);
                        if (hoaDon == null)
                        {
                            MessageBox.Show("Hóa đơn này chưa được lưu! Vui lòng nhấn nút 'Lưu hóa đơn' trước khi thanh toán.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        hoaDon.TrangThai = "Đã thanh toán";
                        db.SaveChanges();

                        TrangThai = "Đã thanh toán";
                        MessageBox.Show("Thanh toán thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi xử lý thanh toán: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            });

            // ===== HỦY HÓA ĐƠN =====
            HuyCommand = new RelayCommand(o =>
            {
                if (string.IsNullOrWhiteSpace(MaHD))
                {
                    MessageBox.Show("Chưa chọn hóa đơn để hủy!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show($"Bạn chắc chắn muốn hủy hóa đơn {MaHD}?", "Xác nhận hủy", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var hoaDon = db.HoaDons.FirstOrDefault(h => h.MaHD == MaHD);
                        if (hoaDon != null)
                        {
                            hoaDon.TrangThai = "Đã hủy";
                            db.SaveChanges();
                            LoadData();
                            MessageBox.Show("Hóa đơn đã được hủy trên hệ thống!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Đã hủy bỏ hóa đơn nháp thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        ResetForm();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi khi hủy hóa đơn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            });
        }

        private void UpdateTongTien()
        {
            TongTien = DsChiTiet != null && DsChiTiet.Count > 0 ? DsChiTiet.Sum(ct => ct.ThanhTien) : 0;
        }

        private void ResetChiTiet()
        {
            ChiTietHoaDon = new ChiTietHoaDon();
            SelectedSanPham = null;
        }

        private void ResetForm()
        {
            MaHD = string.Empty;
            MaKhachHang = null;
            NgayLap = DateTime.Now;
            TrangThai = "Chưa thanh toán";
            DsChiTiet?.Clear();
            TongTien = 0;
            ResetChiTiet();
        }
    }

    public class ChiTietHoaDonBanDisplay : BaseViewModel
    {
        private int _soLuong;
        private decimal _thanhTien;

        public string MaSP { get; set; }
        public string TenSP { get; set; }
        public string Size { get; set; }
        public string MauSac { get; set; }
        public decimal GiaBan { get; set; }

        public int SoLuong
        {
            get => _soLuong;
            set { _soLuong = value; OnPropertyChanged(); }
        }

        public decimal ThanhTien
        {
            get => _thanhTien;
            set { _thanhTien = value; OnPropertyChanged(); }
        }
    }
}