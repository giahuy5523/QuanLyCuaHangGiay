using QuanLyShopGiay.Models;
using QuanLyShopGiay.Helpers;
using QuanLyShopGiay.Command;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace QuanLyShopGiay.ViewModels 
{
    public class HoaDonNhapViewModel : BaseViewModel
    {
        // 1. Khởi tạo DbContext khớp với database QLShopGiay của bạn
        private QLShopGiayEntities db = new QLShopGiayEntities();

        // Biến lưu trữ cho thuộc tính phiếu nhập
        private string _maPhieuNhap;
        private string _maNCC;
        private DateTime _ngayNhap = DateTime.Now;
        private decimal _tongTien;

        // Biến hỗ trợ nhập liệu dòng chi tiết hiện tại từ giao diện
        private PhieuNhapInputFields _chiTietInput = new PhieuNhapInputFields();

        // Các danh sách Binding lên giao diện UI
        private ObservableCollection<ChiTietPhieuNhapDisplay> _dsChiTiet;
        private ObservableCollection<SanPham> _dsSanPham;
        private ObservableCollection<NhaCungCap> _dsNCC;

        // Hệ thống Commands (Sử dụng ICommand để chuẩn hóa MVVM)
        public ICommand TaoPhieuCommand { get; set; }
        public ICommand ThemSanPhamCommand { get; set; }
        public ICommand LuuPhieuCommand { get; set; }
        public ICommand HuyCommand { get; set; }

        // --- PROPERTIES BINDING ---
        public string MaPhieuNhap
        {
            get => _maPhieuNhap;
            set { _maPhieuNhap = value; OnPropertyChanged(); }
        }

        public string MaNCC
        {
            get => _maNCC;
            set { _maNCC = value; OnPropertyChanged(); }
        }

        public DateTime NgayNhap
        {
            get => _ngayNhap;
            set { _ngayNhap = value; OnPropertyChanged(); }
        }

        public decimal TongTien
        {
            get => _tongTien;
            set { _tongTien = value; OnPropertyChanged(); }
        }

        public PhieuNhapInputFields ChiTietInput
        {
            get => _chiTietInput;
            set { _chiTietInput = value; OnPropertyChanged(); }
        }

        public ObservableCollection<ChiTietPhieuNhapDisplay> DsChiTiet
        {
            get => _dsChiTiet;
            set { _dsChiTiet = value; OnPropertyChanged(); }
        }

        public ObservableCollection<SanPham> DsSanPham
        {
            get => _dsSanPham;
            set { _dsSanPham = value; OnPropertyChanged(); }
        }

        public ObservableCollection<NhaCungCap> DsNCC
        {
            get => _dsNCC;
            set { _dsNCC = value; OnPropertyChanged(); }
        }

        // --- CONSTRUCTOR ---
        public HoaDonNhapViewModel()
        {
            LoadData();
            InitCommands();
        }

        private void LoadData()
        {
            try
            {
                DsSanPham = new ObservableCollection<SanPham>(db.SanPhams.ToList());
                DsNCC = new ObservableCollection<NhaCungCap>(db.NhaCungCaps.ToList());
                DsChiTiet = new ObservableCollection<ChiTietPhieuNhapDisplay>();
                TongTien = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message);
            }
        }

        private void InitCommands()
        {

            // Lệnh 1: Tạo mới phiếu nhập & Tự động sinh mã phiếu
            TaoPhieuCommand = new RelayCommand(o=>
            {
                var lastPhieu = db.HoaDonNhaps
                    .OrderByDescending(x => x.MaHDN)
                    .FirstOrDefault();

                int nextNum = 1;
                if (lastPhieu != null && lastPhieu.MaHDN.Length > 10)
                {
                    string suffix = lastPhieu.MaHDN.Substring(10);
                    if (int.TryParse(suffix, out int num))
                    {
                        nextNum = num + 1;
                    }
                }

                MaPhieuNhap = "PN" + DateTime.Now.ToString("yyyyMMdd") + nextNum.ToString("D4");
                NgayNhap = DateTime.Now;
                MaNCC = null;
                ResetChiTiet();
            });

            // Lệnh 2: Thêm tạm thời sản phẩm từ các ô nhập liệu vào DataGrid
            ThemSanPhamCommand = new RelayCommand(o =>
            {
                if (string.IsNullOrEmpty(ChiTietInput.MaSP) || ChiTietInput.SoLuong <= 0 || ChiTietInput.GiaNhap <= 0)
                {
                    MessageBox.Show("Vui lòng kiểm tra lại Sản phẩm, Số lượng và Đơn giá nhập!",
                        "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var existing = DsChiTiet.FirstOrDefault(ct => ct.MaSanPham == ChiTietInput.MaSP);

                if (existing != null)
                {
                    existing.SoLuong += ChiTietInput.SoLuong;
                    existing.ThanhTien = existing.SoLuong * existing.DonGiaNhap;

                    // Làm mới Grid để UI cập nhật dữ liệu
                    var temp = DsChiTiet;
                    DsChiTiet = null;
                    DsChiTiet = temp;
                }
                else
                {
                    var sanPham = DsSanPham.FirstOrDefault(sp => sp.MaSP == ChiTietInput.MaSP);

                    if (sanPham != null)
                    {
                        DsChiTiet.Add(new ChiTietPhieuNhapDisplay
                        {
                            MaSanPham = sanPham.MaSP,
                            TenSanPham = $"{sanPham.TenSP} | Size {sanPham.Size} - {sanPham.MauSac}",
                            SoLuong = ChiTietInput.SoLuong,
                            DonGiaNhap = ChiTietInput.GiaNhap,
                            ThanhTien = ChiTietInput.SoLuong * ChiTietInput.GiaNhap
                        });
                    }
                }

                TongTien = DsChiTiet.Sum(ct => ct.ThanhTien);
                ResetChiTiet();
            });

            // Lệnh 3: Lưu chính thức thông tin xuống Database
            LuuPhieuCommand = new RelayCommand(o =>
            {
                if (string.IsNullOrWhiteSpace(MaPhieuNhap))
                {
                    MessageBox.Show("Bạn phải nhấn nút Tạo phiếu nhập trước!",
                        "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(MaNCC))
                {
                    MessageBox.Show("Vui lòng chọn Nhà cung cấp!",
                        "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (DsChiTiet.Count == 0)
                {
                    MessageBox.Show("Danh sách sản phẩm nhập không được để trống!",
                        "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    var hoaDonNhap = new HoaDonNhap
                    {
                        MaHDN = MaPhieuNhap,
                        NgayNhap = NgayNhap,
                        TongTien = TongTien,
                        MaNCC = MaNCC,        
                        MaNhanVien = SessionHelper.MaNhanVienHienTai
                    };

                    db.HoaDonNhaps.Add(hoaDonNhap);
                    db.SaveChanges();

                    foreach (var ct in DsChiTiet)
                    {
                        // 1. Chỉ thêm chi tiết hóa đơn nhập (Trigger SQL sẽ tự động gánh việc tăng số lượng tồn)
                        var chiTiet = new ChiTietHoaDonNhap
                        {
                            MaHDN = MaPhieuNhap,
                            MaSP = ct.MaSanPham,
                            SoLuong = ct.SoLuong,
                            GiaNhap = ct.DonGiaNhap
                        };
                        db.ChiTietHoaDonNhaps.Add(chiTiet);

                        // 2. Nếu muốn cập nhật giá nhập mới nhất cho sản phẩm đó, ta cập nhật trực tiếp 
                        // giá mà KHÔNG ĐỘNG CHẠM gì tới thuộc tính SoLuongTon trong C# nữa.
                        var sp = db.SanPhams.FirstOrDefault(x => x.MaSP == ct.MaSanPham);
                        if (sp != null)
                        {
                            sp.GiaNhap = ct.DonGiaNhap;
                            db.Entry(sp).State = System.Data.Entity.EntityState.Modified;
                        }
                    }

                    db.SaveChanges();

                    MessageBox.Show("Lưu phiếu nhập kho thành công và đã tự động cập nhật số lượng tồn hàng!",
                        "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    ResetForm();
                }
                catch (Exception ex)
                {
                    string errorMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                    MessageBox.Show("Lỗi hệ thống khi lưu: " + errorMsg, "Thất bại", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });

            // Lệnh 4: Hủy thao tác hiện tại
            HuyCommand = new RelayCommand(o =>
            {
                ResetForm();
            });
        }

        private void ResetChiTiet()
        {
            ChiTietInput = new PhieuNhapInputFields();
            ChiTietInput.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(PhieuNhapInputFields.MaSP))
                {
                    var sp = DsSanPham?.FirstOrDefault(x => x.MaSP == ChiTietInput.MaSP);
                    if (sp != null)
                        ChiTietInput.GiaNhap = sp.GiaNhap ?? 0;
                }
            };
        }

        private void ResetForm()
        {
            MaPhieuNhap = string.Empty;
            MaNCC = null;
            NgayNhap = DateTime.Now;
            DsChiTiet?.Clear();
            TongTien = 0;
            ResetChiTiet();
        }
    }

    // Các class phụ trợ giữ nguyên cấu trúc kế thừa từ BaseViewModel chung của bạn
    public class PhieuNhapInputFields : BaseViewModel
    {
        private string _maSP;
        private int _soLuong;
        private decimal _giaNhap;

        public string MaSP
        {
            get => _maSP;
            set { _maSP = value; OnPropertyChanged(); }
        }
        public int SoLuong
        {
            get => _soLuong;
            set { _soLuong = value; OnPropertyChanged(); }
        }
        public decimal GiaNhap
        {
            get => _giaNhap;
            set { _giaNhap = value; OnPropertyChanged(); }
        }
    }

    public class ChiTietPhieuNhapDisplay : BaseViewModel
    {
        private int _soLuong;
        private decimal _thanhTien;

        public string MaSanPham { get; set; }
        public string TenSanPham { get; set; }

        public int SoLuong
        {
            get => _soLuong;
            set { _soLuong = value; OnPropertyChanged(); }
        }

        public decimal DonGiaNhap { get; set; }

        public decimal ThanhTien
        {
            get => _thanhTien;
            set { _thanhTien = value; OnPropertyChanged(); }
        }
    }
}