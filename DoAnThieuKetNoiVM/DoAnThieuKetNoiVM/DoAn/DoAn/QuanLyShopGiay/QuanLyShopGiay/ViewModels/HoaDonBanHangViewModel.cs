using QuanLyShopGiay.Command;
using QuanLyShopGiay.Models;
using QuanLyShopGiay.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using QuanLyShopGiay.Views.Dialogs;

namespace QuanLyShopGiay.ViewModels
{
    public class GioHangItem : BaseViewModel
    {
        private int _soLuong;

        public string MaSP { get; set; }
        public string TenSP { get; set; }
        public string Size { get; set; }
        public string MauSac { get; set; }
        public string TenBienThe => $"Size {Size} - {MauSac}";
        public decimal DonGia { get; set; }
        public int SoLuongTonKho { get; set; }

        public int SoLuong
        {
            get => _soLuong;
            set
            {
                _soLuong = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ThanhTien));
            }
        }

        public decimal ThanhTien => DonGia * SoLuong;
    }


    public class LichSuHoaDonItem
    {
        public string MaHD { get; set; }
        public string TenKhachHang { get; set; }
        public string TenNhanVien { get; set; }
        public DateTime? NgayLap { get; set; }
        public string TenPhuongThuc { get; set; }
        public decimal? TongTien { get; set; }
        public string TrangThai { get; set; }
    }

    public class HoaDonBanHangViewModel : BaseViewModel
    {
        private readonly QLShopGiayEntities3 _db = new QLShopGiayEntities3();

        private List<SanPham> _listSanPhamGoc = new List<SanPham>();

        // ── Collections ─────────────────────────────────────────────────────────
        public ObservableCollection<SanPham> ListSanPham { get; } = new ObservableCollection<SanPham>();
        public ObservableCollection<GioHangItem> GioHang { get; } = new ObservableCollection<GioHangItem>();
        public ObservableCollection<KhachHang> ListKhachHang { get; } = new ObservableCollection<KhachHang>();
        public ObservableCollection<PhuongThucThanhToan> ListPhuongThuc { get; } = new ObservableCollection<PhuongThucThanhToan>();
        public ObservableCollection<LoaiSanPham> ListLoaiHang { get; } = new ObservableCollection<LoaiSanPham>(); // ← FIX: thêm collection này
        public ObservableCollection<LichSuHoaDonItem> LichSuHoaDon { get; } = new ObservableCollection<LichSuHoaDonItem>();

        // ── Properties ──────────────────────────────────────────────────────────
        private KhachHang _khachHangDangChon;
        public KhachHang KhachHangDangChon
        {
            get => _khachHangDangChon;
            set { _khachHangDangChon = value; OnPropertyChanged(); }
        }

        private PhuongThucThanhToan _phuongThucDangChon;
        public PhuongThucThanhToan PhuongThucDangChon
        {
            get => _phuongThucDangChon;
            set { _phuongThucDangChon = value; OnPropertyChanged(); }
        }

        private string _selectedMaLoai;
        public string SelectedMaLoai
        {
            get => _selectedMaLoai;
            set
            {
                _selectedMaLoai = value;
                OnPropertyChanged();
                ApplyFilter(_tuKhoa, string.IsNullOrEmpty(value) ? null : value);
            }
        }
        private decimal _tongThanhToan;
        public decimal TongThanhToan
        {
            get => _tongThanhToan;
            set { _tongThanhToan = value; OnPropertyChanged(); }
        }

        private string _tuKhoa = "";

        // ── Commands ────────────────────────────────────────────────────────────
        public ICommand ThemVaoGioCommand { get; }
        public ICommand ThanhToanCommand { get; }
        public ICommand XoaItemCommand { get; }
        public ICommand XoaTatCaCommand { get; }
        public ICommand TangSoLuongCommand { get; }
        public ICommand GiamSoLuongCommand { get; }

        // ── Constructor ─────────────────────────────────────────────────────────
        public HoaDonBanHangViewModel()
        {
            LoadData();

            GioHang.CollectionChanged += (_, __) => RecalcTong();

            ThemVaoGioCommand = new RelayCommand(p =>
            {
                if (p is SanPham sp) MoChonBienThe(sp);
            });
            ThanhToanCommand = new RelayCommand(_ => ThanhToan(), _ => CoTheThanhToan());
            XoaItemCommand = new RelayCommand(p => XoaItem(p as GioHangItem));
            XoaTatCaCommand = new RelayCommand(_ => GioHang.Clear());
            TangSoLuongCommand = new RelayCommand(p => TangSoLuong(p as GioHangItem));
            GiamSoLuongCommand = new RelayCommand(p => GiamSoLuong(p as GioHangItem));
        }

        // ── LoadData ────────────────────────────────────────────────────────────
        private void LoadData()
        {
            ListLoaiHang.Add(new LoaiSanPham { MaLoai = "", TenLoai = "-- Tất cả --" });
            foreach (var l in _db.LoaiSanPhams.ToList())
                ListLoaiHang.Add(l);
            _listSanPhamGoc = _db.SanPhams.ToList();
            foreach (var sp in _listSanPhamGoc) ListSanPham.Add(sp);

            foreach (var kh in _db.KhachHangs.OrderBy(k => k.TenKhachHang).ToList())
                ListKhachHang.Add(kh);

            foreach (var pt in _db.PhuongThucThanhToans.Where(p => p.TrangThai == true).ToList())
                ListPhuongThuc.Add(pt);

            // ← FIX: add vào ListLoaiHang thay vì ListSanPham
            foreach (var l in _db.LoaiSanPhams.ToList())
                ListLoaiHang.Add(l);

            KhachHangDangChon = ListKhachHang.FirstOrDefault(k => k.MaKhachHang == "KH01");
            PhuongThucDangChon = ListPhuongThuc.FirstOrDefault(p => p.MaPhuongThuc == "PT01");
            LoadLichSuHoaDon();
        }

        // ── ApplyFilter ─────────────────────────────────────────────────────────
        public void ApplyFilter(string tuKhoa, string maLoai)
        {
            _tuKhoa = tuKhoa ?? "";

            var filtered = _listSanPhamGoc.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(_tuKhoa))
                filtered = filtered.Where(sp =>
                    sp.TenSP.IndexOf(_tuKhoa, StringComparison.OrdinalIgnoreCase) >= 0);

            if (!string.IsNullOrWhiteSpace(maLoai))
                filtered = filtered.Where(sp => sp.MaLoai == maLoai);

            ListSanPham.Clear();
            foreach (var sp in filtered) ListSanPham.Add(sp);
        }

        // ── Mở dialog chọn biến thể ─────────────────────────────────────────────
        private void MoChonBienThe(SanPham sp)
        {
            if (sp == null) return;

            if (sp.SoLuongTon <= 0)
            {
                MessageBox.Show("Sản phẩm này đã hết hàng!",
                    "Hết hàng", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var cacBienThe = _listSanPhamGoc
                .Where(x => x.TenSP == sp.TenSP && x.SoLuongTon > 0)
                .ToList();

            SanPham spDaChon;

            if (cacBienThe.Count == 1)
            {
                spDaChon = cacBienThe[0];
            }
            else
            {
                var dialog = new ChonBienTheDialog(cacBienThe);
                if (dialog.ShowDialog() != true || dialog.SpDaChon == null) return;
                spDaChon = dialog.SpDaChon;
            }

            ThemVaoGio(spDaChon);
        }

        // ── ThemVaoGio ──────────────────────────────────────────────────────────
        private void ThemVaoGio(SanPham sp)
        {
            var item = GioHang.FirstOrDefault(x => x.MaSP == sp.MaSP);

            if (item != null)
            {
                if (item.SoLuong >= item.SoLuongTonKho)
                {
                    MessageBox.Show($"Chỉ còn {item.SoLuongTonKho} sản phẩm trong kho!",
                        "Hết hàng", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                item.SoLuong++;
            }
            else
            {
                GioHang.Add(new GioHangItem
                {
                    MaSP = sp.MaSP,
                    TenSP = sp.TenSP,
                    Size = sp.Size,
                    MauSac = sp.MauSac,
                    DonGia = sp.GiaBan ?? 0,
                    SoLuong = 1,
                    SoLuongTonKho = sp.SoLuongTon ?? 0
                });
            }

            RecalcTong();
        }

        // ── Tăng / Giảm số lượng ────────────────────────────────────────────────
        private void TangSoLuong(GioHangItem item)
        {
            if (item == null) return;
            if (item.SoLuong >= item.SoLuongTonKho)
            {
                MessageBox.Show($"Chỉ còn {item.SoLuongTonKho} sản phẩm trong kho!",
                    "Hết hàng", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            item.SoLuong++;
            RecalcTong();
        }

        private void GiamSoLuong(GioHangItem item)
        {
            if (item == null) return;
            if (item.SoLuong > 1)
            {
                item.SoLuong--;
                RecalcTong();
            }
            else
            {
                var r = MessageBox.Show("Xóa sản phẩm này khỏi giỏ hàng?", "Xác nhận",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (r == MessageBoxResult.Yes) GioHang.Remove(item);
            }
        }

        private void XoaItem(GioHangItem item)
        {
            if (item != null) GioHang.Remove(item);
        }

        private void RecalcTong()
        {
            TongThanhToan = GioHang.Sum(x => x.ThanhTien);
        }

        private bool CoTheThanhToan() =>
            GioHang.Any() &&
            KhachHangDangChon != null &&
            PhuongThucDangChon != null;

        // ── ThanhToan ───────────────────────────────────────────────────────────
        private void ThanhToan()
        {
            if (!CoTheThanhToan())
            {
                MessageBox.Show(
                    "Vui lòng chọn khách hàng, phương thức thanh toán\nvà có ít nhất 1 sản phẩm!",
                    "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    string maHD = "HD" + DateTime.Now.ToString("yyMMddHHmmss");

                    var hoaDon = new HoaDon
                    {
                        MaHD = maHD,
                        NgayLap = DateTime.Now,
                        MaKhachHang = KhachHangDangChon.MaKhachHang,
                        MaNhanVien = SessionHelper.MaNhanVienHienTai,
                        MaPhuongThuc = PhuongThucDangChon.MaPhuongThuc,
                        TongTien = 0,
                        TrangThai = "Đã thanh toán"
                    };
                    _db.HoaDons.Add(hoaDon);
                    _db.SaveChanges();

                    foreach (var item in GioHang)
                    {
                        _db.ChiTietHoaDons.Add(new ChiTietHoaDon
                        {
                            MaHD = maHD,
                            MaSP = item.MaSP,
                            SoLuong = item.SoLuong,
                            GiaBan = item.DonGia
                        });
                    }
                    _db.SaveChanges(); // trigger tự trừ kho + tính TongTien

                    var diemCong = (int)(TongThanhToan / 10000);
                    if (diemCong > 0)
                    {
                        KhachHangDangChon.Diem = (KhachHangDangChon.Diem ?? 0) + diemCong;
                        _db.SaveChanges();
                    }

                    transaction.Commit();

                    _db.Entry(hoaDon).Reload();

                    MessageBox.Show(
                        $"✅ Thanh toán thành công!\n" +
                        $"Mã hóa đơn: {maHD}\n" +
                        $"Tổng tiền: {hoaDon.TongTien:N0} ₫\n" +
                        $"Điểm tích lũy: +{diemCong} điểm",
                        "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                    ResetAfterCheckout();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    var msg = ex.InnerException?.Message ?? ex.Message;
                    MessageBox.Show($"Lỗi khi thanh toán:\n{msg}",
                        "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        // ── Lịch sử hóa đơn ──────────────────────────────────────────────────────
        private void LoadLichSuHoaDon()
        {
            LichSuHoaDon.Clear();
            var dsHD = (from hd in _db.HoaDons
                        join kh in _db.KhachHangs on hd.MaKhachHang equals kh.MaKhachHang into khGroup
                        from kh in khGroup.DefaultIfEmpty()
                        join nv in _db.NhanViens on hd.MaNhanVien equals nv.MaNhanVien into nvGroup
                        from nv in nvGroup.DefaultIfEmpty()
                        join pt in _db.PhuongThucThanhToans on hd.MaPhuongThuc equals pt.MaPhuongThuc into ptGroup
                        from pt in ptGroup.DefaultIfEmpty()
                        orderby hd.NgayLap descending
                        select new LichSuHoaDonItem
                        {
                            MaHD = hd.MaHD,
                            TenKhachHang = kh != null ? kh.TenKhachHang : hd.MaKhachHang,
                            TenNhanVien = nv != null ? nv.TenNhanVien : hd.MaNhanVien,
                            NgayLap = hd.NgayLap,
                            TenPhuongThuc = pt != null ? pt.TenPhuongThuc : hd.MaPhuongThuc,
                            TongTien = hd.TongTien,
                            TrangThai = hd.TrangThai
                        }).Take(50).ToList();

            foreach (var item in dsHD)
                LichSuHoaDon.Add(item);
        }

        // ── Reset sau thanh toán ─────────────────────────────────────────────────
        private void ResetAfterCheckout()
        {
            GioHang.Clear();
            KhachHangDangChon = ListKhachHang.FirstOrDefault(k => k.MaKhachHang == "KH01");
            PhuongThucDangChon = ListPhuongThuc.FirstOrDefault(p => p.MaPhuongThuc == "PT01");

            _listSanPhamGoc = _db.SanPhams.ToList();
            ListSanPham.Clear();
            foreach (var sp in _listSanPhamGoc) ListSanPham.Add(sp);
            LoadLichSuHoaDon();
        }
    }
}