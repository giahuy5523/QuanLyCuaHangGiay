//using QuanLyShopGiay.Helpers;
//using QuanLyShopGiay.Models;
//using System;
//using System.Collections.ObjectModel;
//using System.ComponentModel;
//using System.Linq;
//using System.Runtime.CompilerServices;
//using System.Windows;
//using System.Windows.Input;

//namespace QuanLyShopGiay.ViewModels
//{
//    // ════════════════════════════════════════════════════════════════════
//    // GioHangItem — item trong giỏ hàng, không phải HoaDonViewModel
//    // ════════════════════════════════════════════════════════════════════
//    public class GioHangItem : BaseViewModel
//    {
//        private int _soLuong;

//        public int MaBienThe { get; set; }
//        public string MaSP { get; set; }
//        public string TenSP { get; set; }
//        public string TenBienThe { get; set; }
//        public decimal DonGia { get; set; }

//        public int SoLuong
//        {
//            get => _soLuong;
//            set { _soLuong = value; OnPropertyChanged(); OnPropertyChanged(nameof(ThanhTien)); }
//        }

//        public decimal ThanhTien => DonGia * SoLuong;
//    }

//    // ════════════════════════════════════════════════════════════════════
//    // HoaDonViewModel — ViewModel chính
//    // ════════════════════════════════════════════════════════════════════
//    public class HoaDonViewModel : BaseViewModel
//    {
//        private readonly QLShopGiayEntities _db = new QLShopGiayEntities();
//        private decimal _tongThanhToan;
//        private KHACH_HANG _khachHangDangChon;
//        private PHUONG_THUC_TT _phuongThucDangChon;

//        // ─── PROPERTIES ───────────────────────────────────────────────
//        public ObservableCollection<SAN_PHAM> ListSanPham { get; set; }
//            = new ObservableCollection<SAN_PHAM>();

//        public ObservableCollection<GioHangItem> GioHang { get; set; }
//            = new ObservableCollection<GioHangItem>();

//        public ObservableCollection<KHACH_HANG> ListKhachHang { get; set; }
//            = new ObservableCollection<KHACH_HANG>();

//        public ObservableCollection<PHUONG_THUC_TT> ListPhuongThuc { get; set; }
//            = new ObservableCollection<PHUONG_THUC_TT>();

//        public ObservableCollection<LOAI_HANG> ListLoaiHang { get; set; }
//            = new ObservableCollection<LOAI_HANG>();

//        public decimal TongThanhToan
//        {
//            get => _tongThanhToan;
//            set { _tongThanhToan = value; OnPropertyChanged(); }
//        }

//        public KHACH_HANG KhachHangDangChon
//        {
//            get => _khachHangDangChon;
//            set { _khachHangDangChon = value; OnPropertyChanged(); }
//        }

//        public PHUONG_THUC_TT PhuongThucDangChon
//        {
//            get => _phuongThucDangChon;
//            set { _phuongThucDangChon = value; OnPropertyChanged(); }
//        }

//        // ─── COMMANDS ──────────────────────────────────────────────────
//        public ICommand ThemVaoGioCommand { get; set; }
//        public ICommand ThanhToanCommand { get; set; }
//        public ICommand XoaItemCommand { get; set; }
//        public ICommand XoaTatCaCommand { get; set; }

//        // ────────────────────────────────────────────────────────────────
//        public HoaDonViewModel()
//        {
//            // Load dữ liệu ban đầu
//            LoadSanPham();
//            LoadKhachHang();
//            LoadPhuongThuc();
//            LoadLoaiHang();

//            // Khởi tạo commands — sử dụng explicit constructor
//            ThemVaoGioCommand = new RelayCommand(
//                execute: p => ThemVaoGio((SAN_PHAM)p),
//                canExecute: p => p is SAN_PHAM);

//            ThanhToanCommand = new RelayCommand(
//                execute: p => ThanhToan(),
//                canExecute: p => GioHang.Count > 0);

//            XoaItemCommand = new RelayCommand(
//                execute: p => XoaItem((GioHangItem)p),
//                canExecute: p => p is GioHangItem);

//            XoaTatCaCommand = new RelayCommand(
//                execute: p => XoaTatCa(),
//                canExecute: p => GioHang.Count > 0);

//            // Subscribe thay đổi giỏ hàng
//            GioHang.CollectionChanged += (s, e) => TinhTongTien();
//        }

//        // ════════════════════════════════════════════════════════════════
//        // LOAD DỮ LIỆU
//        // ════════════════════════════════════════════════════════════════

//        private void LoadSanPham(string keyword = "", string maLoai = "")
//        {
//            ListSanPham.Clear();
//            var ds = _db.SAN_PHAM
//                .Include("ANH_SP")
//                .Where(sp => sp.IsDeleted == false
//                          && sp.TenSP.Contains(keyword)
//                          && (string.IsNullOrEmpty(maLoai) || sp.MaLoai == maLoai))
//                .OrderBy(sp => sp.TenSP)
//                .ToList();

//            foreach (var sp in ds)
//                ListSanPham.Add(sp);
//        }

//        private void LoadKhachHang()
//        {
//            ListKhachHang.Clear();
//            var ds = _db.KHACH_HANG.OrderBy(kh => kh.TenKH).ToList();
//            foreach (var kh in ds)
//                ListKhachHang.Add(kh);
//        }

//        private void LoadPhuongThuc()
//        {
//            ListPhuongThuc.Clear();
//            var ds = _db.PHUONG_THUC_TT.OrderBy(pt => pt.TenPT).ToList();
//            foreach (var pt in ds)
//                ListPhuongThuc.Add(pt);
//        }

//        private void LoadLoaiHang()
//        {
//            ListLoaiHang.Clear();
//            var loai = new LOAI_HANG { MaLoai = "", TenLoai = "Tất cả" };
//            ListLoaiHang.Add(loai);

//            var ds = _db.LOAI_HANG.OrderBy(lh => lh.TenLoai).ToList();
//            foreach (var lh in ds)
//                ListLoaiHang.Add(lh);
//        }

//        // ════════════════════════════════════════════════════════════════
//        // THÊM VÀO GIỎ
//        // ════════════════════════════════════════════════════════════════

//        private void ThemVaoGio(SAN_PHAM sp)
//        {
//            if (sp == null) return;

//            try
//            {
//                // Lấy biến thể đầu tiên còn hàng
//                var bienThe = _db.BIEN_THE_SP
//                    .Include("SIZE_GIAY")
//                    .Include("MAU_SAC")
//                    .Include("TON_KHO")
//                    .Where(bt => bt.MaSP == sp.MaSP
//                              && bt.IsDeleted == false
//                              && bt.TON_KHO.Any(tk => tk.SoLuongTon > 0))
//                    .FirstOrDefault();

//                if (bienThe == null)
//                {
//                    MessageBox.Show("Sản phẩm này đã hết hàng!",
//                        "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
//                    return;
//                }

//                string tenBienThe = $"{bienThe.SIZE_GIAY.TenSize} - {bienThe.MAU_SAC.TenMau}";

//                // Kiểm tra đã có trong giỏ chưa
//                var existItem = GioHang.FirstOrDefault(x => x.MaBienThe == bienThe.MaBienThe);
//                if (existItem != null)
//                {
//                    existItem.SoLuong++;
//                }
//                else
//                {
//                    GioHang.Add(new GioHangItem
//                    {
//                        MaBienThe = bienThe.MaBienThe,
//                        MaSP = sp.MaSP,
//                        TenSP = sp.TenSP,
//                        TenBienThe = tenBienThe,
//                        DonGia = sp.GiaBan ?? 0,
//                        SoLuong = 1
//                    });
//                }

//                TinhTongTien();
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show("Lỗi thêm vào giỏ: " + ex.Message,
//                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
//            }
//        }

//        // ════════════════════════════════════════════════════════════════
//        // XOÁ ITEM
//        // ════════════════════════════════════════════════════════════════

//        private void XoaItem(GioHangItem item)
//        {
//            if (item == null) return;
//            GioHang.Remove(item);
//            TinhTongTien();
//        }

//        private void XoaTatCa()
//        {
//            GioHang.Clear();
//            TinhTongTien();
//        }

//        // ════════════════════════════════════════════════════════════════
//        // TÍNH TỔNG TIỀN
//        // ════════════════════════════════════════════════════════════════

//        private void TinhTongTien()
//        {
//            TongThanhToan = GioHang.Sum(item => item.ThanhTien);
//        }

//        // ════════════════════════════════════════════════════════════════
//        // THANH TOÁN
//        // ════════════════════════════════════════════════════════════════

//        private void ThanhToan()
//        {
//            if (GioHang.Count == 0)
//            {
//                MessageBox.Show("Giỏ hàng trống!",
//                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
//                return;
//            }

//            if (KhachHangDangChon == null)
//            {
//                MessageBox.Show("Vui lòng chọn khách hàng!",
//                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
//                return;
//            }

//            if (PhuongThucDangChon == null)
//            {
//                MessageBox.Show("Vui lòng chọn phương thức thanh toán!",
//                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
//                return;
//            }

//            if (string.IsNullOrEmpty(SessionManager.MaTK))
//            {
//                MessageBox.Show("Chưa đăng nhập!",
//                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
//                return;
//            }

//            // Kiểm tra session
//            try
//            {
//                using (var transaction = _db.Database.BeginTransaction())
//                {
//                    try
//                    {
//                        // Tạo hóa đơn
//                        string maHD = "HD" + DateTime.Now.ToString("yyMMddHHmmss");
//                        var hoaDon = new HOA_DON
//                        {
//                            MaHD = maHD,
//                            MaTK = SessionManager.MaTK,
//                            MaKH = KhachHangDangChon.MaKH,
//                            MaKho = "K01",
//                            MaPT = PhuongThucDangChon.MaPT,
//                            TrangThai = "Đã thanh toán"
//                        };
//                        _db.HOA_DON.Add(hoaDon);
//                        _db.SaveChanges();

//                        // Thêm chi tiết hóa đơn
//                        foreach (var item in GioHang)
//                        {
//                            _db.CT_HOA_DON.Add(new CT_HOA_DON
//                            {
//                                MaHD = maHD,
//                                MaBienThe = item.MaBienThe,
//                                SoLuong = item.SoLuong,
//                                GiaBan = item.DonGia
//                            });
//                        }
//                        _db.SaveChanges();

//                        // Trigger TRG_UpdateKho_Xuat sẽ tự trừ TON_KHO
//                        transaction.Commit();

//                        MessageBox.Show(
//                            $"Thanh toán thành công!\n\nMã hóa đơn: {maHD}\nTổng tiền: {TongThanhToan:N0} ₫",
//                            "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

//                        // Reset form
//                        GioHang.Clear();
//                        TongThanhToan = 0;
//                        KhachHangDangChon = null;
//                        PhuongThucDangChon = null;
//                        LoadSanPham();
//                    }
//                    catch (Exception ex)
//                    {
//                        transaction.Rollback();
//                        MessageBox.Show("Lỗi thanh toán: " + ex.Message,
//                            "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show("Lỗi: " + ex.Message,
//                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
//            }
//        }

//        // ════════════════════════════════════════════════════════════════
//        // LỌC SẢN PHẨM
//        // ════════════════════════════════════════════════════════════════

//        public void LocSanPham(string keyword, string maLoai)
//        {
//            LoadSanPham(keyword, maLoai);
//        }
//    }
//}
