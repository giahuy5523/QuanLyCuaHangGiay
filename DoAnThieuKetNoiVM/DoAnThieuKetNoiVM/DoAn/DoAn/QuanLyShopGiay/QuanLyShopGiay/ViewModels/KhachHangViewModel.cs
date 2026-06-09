using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using QuanLyShopGiay.Models;
using QuanLyShopGiay.Command;

namespace QuanLyShopGiay.ViewModels
{
    public class KhachHangDisplayModel
    {
        public string MaKhachHang { get; set; }
        public string TenKhachHang { get; set; }
        public string DienThoai { get; set; }
        public int Diem { get; set; }
        public decimal TongChiTieu { get; set; }
        public string HangThanhVien { get; set; }
    }

    // ── Display model cho lịch sử phiếu nhập ────────────────────────────────────
    public class LichSuPhieuNhapDisplay
    {
        public string MaHDN { get; set; }
        public string TenNCC { get; set; }
        public string TenNhanVien { get; set; }
        public DateTime? NgayNhap { get; set; }
        public decimal? TongTien { get; set; }
    }

    public class KhachHangViewModel : BaseViewModel
    {
        private readonly QLShopGiayEntities _db;

        private ObservableCollection<KhachHangDisplayModel> _listKhachHang;
        public ObservableCollection<KhachHangDisplayModel> ListKhachHang
        {
            get => _listKhachHang;
            set => SetProperty(ref _listKhachHang, value);
        }

        private KhachHangDisplayModel _selectedItem;
        public KhachHangDisplayModel SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (SetProperty(ref _selectedItem, value) && value != null)
                {
                    MaKhachHang = value.MaKhachHang;
                    TenKhachHang = value.TenKhachHang;
                    DienThoai = value.DienThoai;
                    Diem = value.Diem;
                    TongMuaText = string.Format("{0:N0} ₫", value.TongChiTieu);
                }
            }
        }

        private string _maKhachHang;
        public string MaKhachHang { get => _maKhachHang; set => SetProperty(ref _maKhachHang, value); }

        private string _tenKhachHang;
        public string TenKhachHang { get => _tenKhachHang; set => SetProperty(ref _tenKhachHang, value); }

        private string _dienThoai;
        public string DienThoai { get => _dienThoai; set => SetProperty(ref _dienThoai, value); }

        private int _diem;
        public int Diem { get => _diem; set => SetProperty(ref _diem, value); }

        private string _tongMuaText = "0 ₫";
        public string TongMuaText { get => _tongMuaText; set => SetProperty(ref _tongMuaText, value); }

        private string _searchKeyword;
        public string SearchKeyword { get => _searchKeyword; set => SetProperty(ref _searchKeyword, value); }

        // ── Lịch sử phiếu nhập ──────────────────────────────────────────────────
        private ObservableCollection<LichSuPhieuNhapDisplay> _lichSuPhieuNhap;
        public ObservableCollection<LichSuPhieuNhapDisplay> LichSuPhieuNhap
        {
            get => _lichSuPhieuNhap;
            set => SetProperty(ref _lichSuPhieuNhap, value);
        }

        private string _tongTienNhapText = "0 ₫";
        public string TongTienNhapText { get => _tongTienNhapText; set => SetProperty(ref _tongTienNhapText, value); }

        private int _soPhieuNhap;
        public int SoPhieuNhap { get => _soPhieuNhap; set => SetProperty(ref _soPhieuNhap, value); }

        // ── Lọc lịch sử phiếu nhập ──────────────────────────────────────────────
        private string _searchPhieuNhap;
        public string SearchPhieuNhap
        {
            get => _searchPhieuNhap;
            set => SetProperty(ref _searchPhieuNhap, value);
        }

        private DateTime? _tuNgayFilter;
        public DateTime? TuNgayFilter
        {
            get => _tuNgayFilter;
            set => SetProperty(ref _tuNgayFilter, value);
        }

        private DateTime? _denNgayFilter;
        public DateTime? DenNgayFilter
        {
            get => _denNgayFilter;
            set => SetProperty(ref _denNgayFilter, value);
        }

        public ICommand SearchCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand SearchPhieuNhapCommand { get; }
        public ICommand ResetPhieuNhapCommand { get; }

        public KhachHangViewModel()
        {
            try
            {
                _db = new QLShopGiayEntities();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể kết nối cơ sở dữ liệu:\n" + ex.Message);
                return;
            }

            SearchCommand = new RelayCommand(_ => LoadData());
            AddCommand = new RelayCommand(_ => ExecuteAdd(), _ => CanExecuteSave());
            EditCommand = new RelayCommand(_ => ExecuteEdit(), _ => SelectedItem != null && CanExecuteSave());
            DeleteCommand = new RelayCommand(_ => ExecuteDelete(), _ => SelectedItem != null && SelectedItem.MaKhachHang != "KH01");
            ClearCommand = new RelayCommand(_ => ClearInputs());
            SearchPhieuNhapCommand = new RelayCommand(_ => LoadLichSuPhieuNhap());
            ResetPhieuNhapCommand = new RelayCommand(_ => ResetFilterPhieuNhap());

            LoadData();
            LoadLichSuPhieuNhap();
        }

        private void LoadData()
        {
            if (_db == null) return;
            try
            {
                var query = _db.KhachHangs.AsQueryable();

                if (!string.IsNullOrWhiteSpace(SearchKeyword))
                {
                    string keyword = SearchKeyword.Trim().ToLower();
                    query = query.Where(kh =>
                        kh.TenKhachHang.ToLower().Contains(keyword) ||
                        kh.MaKhachHang.ToLower().Contains(keyword) ||
                        (kh.DienThoai != null && kh.DienThoai.Contains(keyword)));
                }

                var tongChiTieuDict = _db.HoaDons
                    .Where(h => h.TrangThai == "Đã thanh toán" && h.MaKhachHang != null)
                    .GroupBy(h => h.MaKhachHang)
                    .Select(g => new { MaKH = g.Key, Tong = g.Sum(h => (decimal?)h.TongTien) ?? 0 })
                    .ToDictionary(x => x.MaKH, x => x.Tong);

                ListKhachHang = new ObservableCollection<KhachHangDisplayModel>(
                    query.ToList()
                    .Where(kh => kh.MaKhachHang != "KH01")
                    .Select(kh =>
                    {
                        int diem = kh.Diem ?? 0;
                        decimal tongChiTieu = tongChiTieuDict.ContainsKey(kh.MaKhachHang)
                            ? tongChiTieuDict[kh.MaKhachHang]
                            : 0m;

                        return new KhachHangDisplayModel
                        {
                            MaKhachHang = kh.MaKhachHang,
                            TenKhachHang = kh.TenKhachHang,
                            DienThoai = kh.DienThoai,
                            Diem = diem,
                            TongChiTieu = tongChiTieu,
                            HangThanhVien = GetRankName(tongChiTieu)
                        };
                    })
                    .OrderBy(x => x.MaKhachHang)
                    .ToList());
            }
            catch (Exception ex) { MessageBox.Show("Lỗi nạp dữ liệu: " + ex.Message); }
        }

        // ── Tải lịch sử phiếu nhập ──────────────────────────────────────────────
        private void LoadLichSuPhieuNhap()
        {
            if (_db == null) return;
            try
            {
                var query = _db.HoaDonNhaps
                    .Join(_db.NhaCungCaps,
                          hdn => hdn.MaNCC,
                          ncc => ncc.MaNCC,
                          (hdn, ncc) => new { hdn, TenNCC = ncc.TenNCC })
                    .Join(_db.NhanViens,
                          x => x.hdn.MaNhanVien,
                          nv => nv.MaNhanVien,
                          (x, nv) => new { x.hdn, x.TenNCC, TenNhanVien = nv.TenNhanVien })
                    .AsQueryable();

                // Lọc theo từ khóa (mã HĐN hoặc tên NCC)
                if (!string.IsNullOrWhiteSpace(SearchPhieuNhap))
                {
                    string kw = SearchPhieuNhap.Trim().ToLower();
                    query = query.Where(x =>
                        x.hdn.MaHDN.ToLower().Contains(kw) ||
                        x.TenNCC.ToLower().Contains(kw) ||
                        x.TenNhanVien.ToLower().Contains(kw));
                }

                // Lọc theo khoảng thời gian
                if (TuNgayFilter.HasValue)
                    query = query.Where(x => x.hdn.NgayNhap >= TuNgayFilter.Value);
                if (DenNgayFilter.HasValue)
                {
                    var denNgayCuoiNgay = DenNgayFilter.Value.Date.AddDays(1);
                    query = query.Where(x => x.hdn.NgayNhap < denNgayCuoiNgay);
                }

                var result = query
                    .OrderByDescending(x => x.hdn.NgayNhap)
                    .Select(x => new LichSuPhieuNhapDisplay
                    {
                        MaHDN = x.hdn.MaHDN,
                        TenNCC = x.TenNCC,
                        TenNhanVien = x.TenNhanVien,
                        NgayNhap = x.hdn.NgayNhap,
                        TongTien = x.hdn.TongTien
                    })
                    .ToList();

                LichSuPhieuNhap = new ObservableCollection<LichSuPhieuNhapDisplay>(result);
                SoPhieuNhap = result.Count;
                TongTienNhapText = string.Format("{0:N0} ₫", result.Sum(x => x.TongTien ?? 0));
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải lịch sử phiếu nhập: " + ex.Message); }
        }

        private void ResetFilterPhieuNhap()
        {
            SearchPhieuNhap = string.Empty;
            TuNgayFilter = null;
            DenNgayFilter = null;
            LoadLichSuPhieuNhap();
        }

        private static string GetRankName(decimal tongChiTieu)
        {
            if (tongChiTieu >= 50_000_000) return "Kim cương 💎";
            if (tongChiTieu >= 20_000_000) return "Vàng 🥇";
            if (tongChiTieu >= 5_000_000) return "Bạc 🥈";
            return "Thành viên mới 🌱";
        }

        private void ClearInputs()
        {
            _selectedItem = null;
            OnPropertyChanged(nameof(SelectedItem));
            MaKhachHang = TenKhachHang = DienThoai = string.Empty;
            Diem = 0;
            TongMuaText = "0 ₫";
        }

        private bool CanExecuteSave()
        {
            return !string.IsNullOrWhiteSpace(TenKhachHang) &&
                   !string.IsNullOrWhiteSpace(DienThoai);
        }

        private void ExecuteAdd()
        {
            if (_db.KhachHangs.Any(k => k.DienThoai == DienThoai.Trim() && k.MaKhachHang != "KH01"))
            {
                MessageBox.Show("Số điện thoại này đã tồn tại cho khách hàng khác!",
                    "Trùng dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string newId = GenerateNextId();
                var kh = new KhachHang
                {
                    MaKhachHang = newId,
                    TenKhachHang = TenKhachHang.Trim(),
                    DienThoai = DienThoai.Trim(),
                    Diem = Diem
                };

                _db.KhachHangs.Add(kh);
                _db.SaveChanges();
                LoadData();
                ClearInputs();
                MessageBox.Show($"Đã thêm khách hàng {newId} thành công!", "Thành công",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex) { MessageBox.Show("Lỗi thêm mới: " + ex.Message); }
        }

        private void ExecuteEdit()
        {
            if (_db.KhachHangs.Any(k => k.DienThoai == DienThoai.Trim()
                                     && k.MaKhachHang != SelectedItem.MaKhachHang
                                     && k.MaKhachHang != "KH01"))
            {
                MessageBox.Show("Số điện thoại này đã tồn tại cho khách hàng khác!",
                    "Trùng dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var kh = _db.KhachHangs.FirstOrDefault(x => x.MaKhachHang == SelectedItem.MaKhachHang);
                if (kh != null)
                {
                    kh.TenKhachHang = TenKhachHang.Trim();
                    kh.DienThoai = DienThoai.Trim();
                    kh.Diem = Diem;

                    _db.SaveChanges();
                    LoadData();
                    ClearInputs();
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi cập nhật: " + ex.Message); }
        }

        private void ExecuteDelete()
        {
            if (MessageBox.Show(
                $"Xóa khách hàng '{SelectedItem.TenKhachHang}' và toàn bộ dữ liệu liên quan?",
                "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    var kh = _db.KhachHangs.FirstOrDefault(x => x.MaKhachHang == SelectedItem.MaKhachHang);
                    if (kh != null)
                    {
                        var hds = _db.HoaDons.Where(h => h.MaKhachHang == kh.MaKhachHang).ToList();
                        foreach (var hd in hds)
                        {
                            var cthds = _db.ChiTietHoaDons.Where(c => c.MaHD == hd.MaHD);
                            _db.ChiTietHoaDons.RemoveRange(cthds);
                        }
                        _db.HoaDons.RemoveRange(hds);
                        _db.KhachHangs.Remove(kh);
                        _db.SaveChanges();
                        LoadData();
                        ClearInputs();
                    }
                }
                catch (Exception ex) { MessageBox.Show("Lỗi khi xóa: " + ex.Message); }
            }
        }

        private string GenerateNextId()
        {
            var maxId = _db.KhachHangs
                .Select(x => x.MaKhachHang)
                .ToList()
                .Where(x => x.StartsWith("KH") && x.Length > 2)
                .Select(x => int.TryParse(x.Substring(2), out int id) ? id : 0)
                .DefaultIfEmpty(0)
                .Max();

            return string.Format("KH{0:D2}", maxId + 1);
        }
    }
}
