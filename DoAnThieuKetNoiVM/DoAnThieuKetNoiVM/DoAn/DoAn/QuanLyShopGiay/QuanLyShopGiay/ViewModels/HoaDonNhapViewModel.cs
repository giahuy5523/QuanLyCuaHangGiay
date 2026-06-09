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

        private string _maPhieuNhap;
        private string _maNCC;
        private DateTime _ngayNhap = DateTime.Now;
        private decimal _tongTien;
        private PhieuNhapInputFields _chiTietInput = new PhieuNhapInputFields();

        private ObservableCollection<ChiTietPhieuNhapDisplay> _dsChiTiet;
        private ObservableCollection<HoaDonNhapDisplay> _dsHoaDonNhap; // 
        private ObservableCollection<SanPham> _dsSanPham;
        private ObservableCollection<NhaCungCap> _dsNCC;

        public ICommand TaoPhieuCommand { get; set; }
        public ICommand ThemSanPhamCommand { get; set; }
        public ICommand LuuPhieuCommand { get; set; }
        public ICommand HuyCommand { get; set; }

        // ── Properties ──────────────────────────────────────────────────────────
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

        public ObservableCollection<HoaDonNhapDisplay> DsHoaDonNhap
        {
            get => _dsHoaDonNhap;
            set { _dsHoaDonNhap = value; OnPropertyChanged(); }
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

        // ── Constructor ─────────────────────────────────────────────────────────
        public HoaDonNhapViewModel()
        {
            LoadData();
            InitCommands();
        }

        // ── LoadData ────────────────────────────────────────────────────────────
        private void LoadData()
        {
            try
            {
                DsSanPham = new ObservableCollection<SanPham>(db.SanPhams.ToList());
                DsNCC = new ObservableCollection<NhaCungCap>(db.NhaCungCaps.ToList());
                DsChiTiet = new ObservableCollection<ChiTietPhieuNhapDisplay>();
                TongTien = 0;
                ResetChiTiet();
                LoadDsHoaDonNhap(); 
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message);
            }
        }

        private void LoadDsHoaDonNhap()
        {
            var dsHDN = db.HoaDonNhaps
                .Join(db.NhaCungCaps,
                      hdn => hdn.MaNCC,
                      ncc => ncc.MaNCC,
                      (hdn, ncc) => new HoaDonNhapDisplay
                      {
                          MaHDN = hdn.MaHDN,
                          TenNCC = ncc.TenNCC,
                          NgayNhap = hdn.NgayNhap,
                          TongTien = hdn.TongTien
                      })
                .OrderByDescending(x => x.NgayNhap)
                .ToList();

            DsHoaDonNhap = new ObservableCollection<HoaDonNhapDisplay>(dsHDN);
        }

        // ── Commands ────────────────────────────────────────────────────────────
        private void InitCommands()
        {
            TaoPhieuCommand = new RelayCommand(o =>
            {
                var lastPhieu = db.HoaDonNhaps
                    .OrderByDescending(x => x.MaHDN)
                    .FirstOrDefault();

                int nextNum = 1;
                if (lastPhieu != null && lastPhieu.MaHDN.Length > 10)
                {
                    string suffix = lastPhieu.MaHDN.Substring(10);
                    if (int.TryParse(suffix, out int num))
                        nextNum = num + 1;
                }

                MaPhieuNhap = "PN" + DateTime.Now.ToString("yyyyMMdd") + nextNum.ToString("D4");
                NgayNhap = DateTime.Now;
                MaNCC = null;
                ResetChiTiet();
            });

            ThemSanPhamCommand = new RelayCommand(o =>
            {
                if (string.IsNullOrEmpty(ChiTietInput.MaSP) ||
                    ChiTietInput.SoLuong <= 0 ||
                    ChiTietInput.GiaNhap <= 0)
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
                        db.ChiTietHoaDonNhaps.Add(new ChiTietHoaDonNhap
                        {
                            MaHDN = MaPhieuNhap,
                            MaSP = ct.MaSanPham,
                            SoLuong = ct.SoLuong,
                            GiaNhap = ct.DonGiaNhap
                        });

                        var sp = db.SanPhams.FirstOrDefault(x => x.MaSP == ct.MaSanPham);
                        if (sp != null)
                        {
                            sp.GiaNhap = ct.DonGiaNhap;
                            db.Entry(sp).State = System.Data.Entity.EntityState.Modified;
                        }
                    }

                    db.SaveChanges();

                    MessageBox.Show("Lưu phiếu nhập kho thành công!",
                        "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                    LoadDsHoaDonNhap();
                    ResetForm();
                }
                catch (Exception ex)
                {
                    string errorMsg = ex.InnerException?.Message ?? ex.Message;
                    MessageBox.Show("Lỗi hệ thống khi lưu: " + errorMsg,
                        "Thất bại", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });

            HuyCommand = new RelayCommand(o => ResetForm());
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

    // ── Helper classes ───────────────────────────────────────────────────────────

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

    public class HoaDonNhapDisplay
    {
        public string MaHDN { get; set; }
        public string TenNCC { get; set; }
        public DateTime? NgayNhap { get; set; }
        public decimal? TongTien { get; set; }
    }
}