using QuanLyShopGiay.Helpers;
using QuanLyShopGiay.Models;
using QuanLyShopGiay.Services;
using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace QuanLyShopGiay.ViewModels
{
    /// <summary>
    /// ViewModel cho TaiKhoanPage.
    /// TAB 1: Quản lý tài khoản (CRUD) — chỉ Admin
    /// TAB 2: Duyệt yêu cầu đăng ký — chỉ Admin
    /// </summary>
    public class TaiKhoanViewModel : BaseViewModel
    {
        private readonly TaiKhoanService _taiKhoanService = new TaiKhoanService();

        // ════════════════════════════════════════════════════════════════════════
        // PHÂN QUYỀN
        // ════════════════════════════════════════════════════════════════════════

        /// <summary>Chỉ Admin mới có quyền CRUD tài khoản.</summary>
        public bool CoQuyenAdmin => SessionManager.IsAdmin;

        /// <summary>Tab "Duyệt yêu cầu" chỉ hiện với Admin.</summary>
        public Visibility HienTabDuyet =>
            SessionManager.IsAdmin ? Visibility.Visible : Visibility.Collapsed;

        // ════════════════════════════════════════════════════════════════════════
        // TAB 1 — QUẢN LÝ TÀI KHOẢN — PROPERTIES
        // ════════════════════════════════════════════════════════════════════════

        // ── Tìm kiếm ──────────────────────────────────────────────────────────
        private string _tuKhoaTK = "";
        public string TuKhoaTK
        {
            get => _tuKhoaTK;
            set { if (SetProperty(ref _tuKhoaTK, value)) LoadTaiKhoan(); }
        }

        // ── DataGrid ───────────────────────────────────────────────────────────
        private ObservableCollection<TAI_KHOAN> _danhSachTK;
        public ObservableCollection<TAI_KHOAN> DanhSachTK
        {
            get => _danhSachTK;
            set => SetProperty(ref _danhSachTK, value);
        }

        // ── Selection ──────────────────────────────────────────────────────────
        private TAI_KHOAN _selectedTK;
        public TAI_KHOAN SelectedTK
        {
            get => _selectedTK;
            set
            {
                if (SetProperty(ref _selectedTK, value))
                {
                    if (value != null)
                        DienVaoFormTK(value);
                    else
                        ClearFormTK();

                    // FIX: Cập nhật lại trạng thái enable/disable các nút
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        // ── Form fields ───────────────────────────────────────────────────────
        private string _tieuDeFormTK = "Thêm tài khoản mới";
        public string TieuDeFormTK
        {
            get => _tieuDeFormTK;
            set => SetProperty(ref _tieuDeFormTK, value);
        }

        private string _maTK = "";
        public string MaTK
        {
            get => _maTK;
            set => SetProperty(ref _maTK, value);
        }

        private bool _maTKReadOnly = false;
        public bool MaTKReadOnly
        {
            get => _maTKReadOnly;
            set => SetProperty(ref _maTKReadOnly, value);
        }

        private string _tenDangNhap = "";
        public string TenDangNhap
        {
            get => _tenDangNhap;
            set => SetProperty(ref _tenDangNhap, value);
        }

        private string _maNV_TK = null;
        public string MaNV_TK
        {
            get => _maNV_TK;
            set => SetProperty(ref _maNV_TK, value);
        }

        private string _maVT = null;
        public string MaVT
        {
            get => _maVT;
            set => SetProperty(ref _maVT, value);
        }

        private bool _coTheXoaTK = false;
        public bool CoTheXoaTK
        {
            get => _coTheXoaTK;
            set
            {
                if (SetProperty(ref _coTheXoaTK, value))
                    CommandManager.InvalidateRequerySuggested();
            }
        }

        // ── ComboBox sources ───────────────────────────────────────────────────
        private ObservableCollection<NHAN_VIEN> _danhSachNV;
        public ObservableCollection<NHAN_VIEN> DanhSachNV
        {
            get => _danhSachNV;
            set => SetProperty(ref _danhSachNV, value);
        }

        private ObservableCollection<VAI_TRO> _danhSachVT;
        public ObservableCollection<VAI_TRO> DanhSachVT
        {
            get => _danhSachVT;
            set => SetProperty(ref _danhSachVT, value);
        }

        // ── PasswordBox wire-up ────────────────────────────────────────────────
        /// <summary>
        /// Delegate để lấy mật khẩu từ PasswordBox.
        /// Set từ TaiKhoanPage.xaml.cs code-behind.
        /// </summary>
        public Func<string> LayMatKhauMoi { get; set; }

        // ────────────────────────────────────────────────────────────────────────
        // TAB 1 — COMMANDS
        // ────────────────────────────────────────────────────────────────────────

        public ICommand ThemMoiTKCommand { get; }
        public ICommand LuuTKCommand { get; }
        public ICommand XoaTKCommand { get; }
        public ICommand HuyTKCommand { get; }

        // ════════════════════════════════════════════════════════════════════════
        // TAB 2 — DUYỆT YÊU CẦU — PROPERTIES
        // ════════════════════════════════════════════════════════════════════════

        private ObservableCollection<YEU_CAU_DANG_KY> _danhSachYeuCau;
        public ObservableCollection<YEU_CAU_DANG_KY> DanhSachYeuCau
        {
            get => _danhSachYeuCau;
            set => SetProperty(ref _danhSachYeuCau, value);
        }

        private YEU_CAU_DANG_KY _selectedYC;
        public YEU_CAU_DANG_KY SelectedYC
        {
            get => _selectedYC;
            set
            {
                if (SetProperty(ref _selectedYC, value))
                {
                    if (value != null)
                        DienVaoFormYC(value);
                    else
                        ClearFormYC();

                    // FIX: Cập nhật lại trạng thái enable/disable các nút
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        private string _yC_MaTKMoi = "";
        public string YC_MaTKMoi
        {
            get => _yC_MaTKMoi;
            set => SetProperty(ref _yC_MaTKMoi, value);
        }

        private string _yC_MaVT = null;
        public string YC_MaVT
        {
            get => _yC_MaVT;
            set => SetProperty(ref _yC_MaVT, value);
        }

        private ObservableCollection<VAI_TRO> _danhSachVT_YC;
        public ObservableCollection<VAI_TRO> DanhSachVT_YC
        {
            get => _danhSachVT_YC;
            set => SetProperty(ref _danhSachVT_YC, value);
        }

        // ────────────────────────────────────────────────────────────────────────
        // TAB 2 — COMMANDS
        // ────────────────────────────────────────────────────────────────────────

        public ICommand DuyetYeuCauCommand { get; }
        public ICommand TuChoiYeuCauCommand { get; }

        // ════════════════════════════════════════════════════════════════════════
        // CONSTRUCTOR
        // ════════════════════════════════════════════════════════════════════════

        public TaiKhoanViewModel()
        {
            // TAB 1 Commands
            ThemMoiTKCommand = new RelayCommand(_ => XuLyThemMoiTK(), _ => CoQuyenAdmin);
            LuuTKCommand = new RelayCommand(_ => XuLyLuuTK(), _ => CoQuyenAdmin);
            XoaTKCommand = new RelayCommand(_ => XuLyKhoaTK(), _ => CoQuyenAdmin && CoTheXoaTK);
            HuyTKCommand = new RelayCommand(_ => XuLyHuyTK());

            // TAB 2 Commands
            DuyetYeuCauCommand = new RelayCommand(_ => XuLyDuyetYC(), _ => CoQuyenAdmin && SelectedYC != null);
            TuChoiYeuCauCommand = new RelayCommand(_ => XuLyTuChoiYC(), _ => CoQuyenAdmin && SelectedYC != null);

            // Load data
            LoadComboBoxes();
            LoadTaiKhoan();
            LoadYeuCau();
            ClearFormTK();
            ClearFormYC();
        }

        // ════════════════════════════════════════════════════════════════════════
        // LOAD DATA
        // ════════════════════════════════════════════════════════════════════════

        private void LoadTaiKhoan()
        {
            try
            {
                using (var db = new QLShopGiayEntities())
                {
                    var kw = TuKhoaTK ?? "";
                    var ds = db.TAI_KHOAN
                        .Include("NHAN_VIEN")
                        .Include("VAI_TRO")
                        .Where(tk => tk.TenDangNhap.Contains(kw) ||
                                   (tk.NHAN_VIEN != null && tk.NHAN_VIEN.HoTen.Contains(kw)))
                        .OrderBy(tk => tk.MaTK)
                        .ToList();
                    DanhSachTK = new ObservableCollection<TAI_KHOAN>(ds);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi load tài khoản: " + ex.Message,
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadYeuCau()
        {
            if (!CoQuyenAdmin) return;
            try
            {
                using (var db = new QLShopGiayEntities())
                {
                    var ds = db.YEU_CAU_DANG_KY
                        .Where(yc => yc.TrangThai == "ChoXet")
                        .OrderByDescending(yc => yc.NgayTao)
                        .ToList();
                    DanhSachYeuCau = new ObservableCollection<YEU_CAU_DANG_KY>(ds);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi load yêu cầu: " + ex.Message,
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadComboBoxes()
        {
            try
            {
                using (var db = new QLShopGiayEntities())
                {
                    // Load vai trò
                    var roles = db.VAI_TRO.OrderBy(v => v.MaVT).ToList();
                    DanhSachVT = new ObservableCollection<VAI_TRO>(roles);
                    DanhSachVT_YC = new ObservableCollection<VAI_TRO>(roles);

                    // FIX: Load TẤT CẢ nhân viên chưa bị xóa
                    // (không lọc theo "chưa có TK" vì khi sửa cần hiện NV hiện tại)
                    var nv = db.NHAN_VIEN
                        .Where(n => n.IsDeleted == false)
                        .OrderBy(n => n.HoTen)
                        .ToList();
                    DanhSachNV = new ObservableCollection<NHAN_VIEN>(nv);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi load combobox: " + ex.Message,
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ════════════════════════════════════════════════════════════════════════
        // TAB 1 — FORM HELPERS
        // ════════════════════════════════════════════════════════════════════════

        private void DienVaoFormTK(TAI_KHOAN tk)
        {
            if (tk == null) return;
            TieuDeFormTK = "Chỉnh sửa tài khoản";
            MaTK = tk.MaTK;
            TenDangNhap = tk.TenDangNhap;
            MaNV_TK = tk.MaNV;
            MaVT = tk.MaVT;
            MaTKReadOnly = true;

            // FIX: Cho phép khoá cả tài khoản đang hoạt động lẫn đã bị khoá (để toggle),
            // nhưng không được khoá tài khoản đang đăng nhập
            CoTheXoaTK = tk.MaTK != SessionManager.MaTK;

            System.Diagnostics.Debug.WriteLine(
                $"DienVaoFormTK: MaTK={tk.MaTK}, IsDeleted={tk.IsDeleted}, " +
                $"SessionMaTK={SessionManager.MaTK}, CoTheXoaTK={CoTheXoaTK}");
        }

        private void ClearFormTK()
        {
            TieuDeFormTK = "Thêm tài khoản mới";
            MaTK = "";
            TenDangNhap = "";
            MaNV_TK = null;
            MaVT = null;
            MaTKReadOnly = false;
            CoTheXoaTK = false;
        }

        // ════════════════════════════════════════════════════════════════════════
        // TAB 1 — COMMAND HANDLERS
        // ════════════════════════════════════════════════════════════════════════

        private void XuLyThemMoiTK()
        {
            SelectedTK = null;
            ClearFormTK();
        }

        private void XuLyLuuTK()
        {
            // Validate
            if (string.IsNullOrWhiteSpace(MaTK) ||
                string.IsNullOrWhiteSpace(TenDangNhap) ||
                string.IsNullOrWhiteSpace(MaVT))
            {
                MessageBox.Show("Vui lòng điền đầy đủ: Mã TK, Tên đăng nhập, Vai trò!",
                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (MaTKReadOnly)
                {
                    // UPDATE
                    using (var db = new QLShopGiayEntities())
                    {
                        var tk = db.TAI_KHOAN.FirstOrDefault(t => t.MaTK == MaTK);
                        if (tk == null)
                        {
                            MessageBox.Show("Tài khoản không tồn tại!",
                                "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        tk.TenDangNhap = TenDangNhap.Trim();
                        tk.MaVT = MaVT;
                        tk.MaNV = MaNV_TK;

                        // Nếu có nhập mật khẩu mới thì đổi
                        string matKhauMoi = LayMatKhauMoi?.Invoke();
                        if (!string.IsNullOrWhiteSpace(matKhauMoi))
                        {
                            var salt = Guid.NewGuid();
                            tk.Salt = salt;
                            tk.MatKhau = System.Security.Cryptography.HashAlgorithm
                                .Create("SHA512") == null
                                ? HashPassword(salt, matKhauMoi)
                                : HashPassword(salt, matKhauMoi);
                        }

                        db.SaveChanges();
                    }

                    MessageBox.Show("Cập nhật tài khoản thành công!",
                        "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // INSERT — cần mật khẩu
                    string matKhau = LayMatKhauMoi?.Invoke();
                    if (string.IsNullOrWhiteSpace(matKhau))
                    {
                        MessageBox.Show("Vui lòng nhập mật khẩu!",
                            "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var result = _taiKhoanService.Insert(new TAI_KHOAN
                    {
                        MaTK = MaTK.Trim(),
                        TenDangNhap = TenDangNhap.Trim(),
                        MaVT = MaVT,
                        MaNV = MaNV_TK
                    }, matKhau);

                    if (!result.Success)
                    {
                        MessageBox.Show(result.Message,
                            "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    MessageBox.Show("Tạo tài khoản thành công!",
                        "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                LoadTaiKhoan();
                SelectedTK = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message,
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// FIX: Đổi từ xóa vật lý (Remove) sang soft-delete (IsDeleted = true).
        /// Phù hợp với logic hiển thị "🚫 Bị khoá" trên DataGrid.
        /// </summary>
        private void XuLyKhoaTK()
        {
            if (MaTK == SessionManager.MaTK)
            {
                MessageBox.Show("Không thể khoá tài khoản đang sử dụng!",
                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new QLShopGiayEntities())
                {
                    var tk = db.TAI_KHOAN.FirstOrDefault(t => t.MaTK == MaTK);
                    if (tk == null)
                    {
                        MessageBox.Show("Tài khoản không tồn tại!",
                            "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Toggle: đang khoá thì mở, đang hoạt động thì khoá
                    bool dangKhoa = tk.IsDeleted ?? false;
                    string hanhDong = dangKhoa ? "mở khoá" : "khoá";

                    var confirm = MessageBox.Show(
                        $"Xác nhận {hanhDong} tài khoản \"{TenDangNhap}\"?",
                        "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (confirm != MessageBoxResult.Yes) return;

                    tk.IsDeleted = !dangKhoa;
                    db.SaveChanges();
                }

                MessageBox.Show("Cập nhật trạng thái tài khoản thành công!",
                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadTaiKhoan();
                SelectedTK = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message,
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void XuLyHuyTK()
        {
            SelectedTK = null;
            ClearFormTK();
        }

        // ════════════════════════════════════════════════════════════════════════
        // TAB 2 — FORM HELPERS
        // ════════════════════════════════════════════════════════════════════════

        private void DienVaoFormYC(YEU_CAU_DANG_KY yc)
        {
            if (yc == null) return;
            // Gợi ý mã TK tự động
            YC_MaTKMoi = "TK" + DateTime.Now.ToString("mmss");
            YC_MaVT = null;
        }

        private void ClearFormYC()
        {
            YC_MaTKMoi = "";
            YC_MaVT = null;
        }

        // ════════════════════════════════════════════════════════════════════════
        // TAB 2 — COMMAND HANDLERS
        // ════════════════════════════════════════════════════════════════════════

        private void XuLyDuyetYC()
        {
            if (SelectedYC == null) return;

            if (string.IsNullOrWhiteSpace(YC_MaTKMoi) || string.IsNullOrWhiteSpace(YC_MaVT))
            {
                MessageBox.Show("Vui lòng nhập Mã TK mới và chọn Vai trò!",
                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new QLShopGiayEntities())
                {
                    // Kiểm tra mã TK có tồn tại không
                    if (db.TAI_KHOAN.Any(tk => tk.MaTK == YC_MaTKMoi.Trim()))
                    {
                        MessageBox.Show($"Mã TK \"{YC_MaTKMoi}\" đã tồn tại!",
                            "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var yc = db.YEU_CAU_DANG_KY.FirstOrDefault(y => y.MaYeuCau == SelectedYC.MaYeuCau);
                    if (yc == null || yc.TrangThai != "ChoXet")
                    {
                        MessageBox.Show("Yêu cầu không còn hiệu lực!",
                            "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    db.TAI_KHOAN.Add(new TAI_KHOAN
                    {
                        MaTK = YC_MaTKMoi.Trim(),
                        TenDangNhap = yc.TenDangNhap,
                        MatKhau = yc.MatKhau,
                        Salt = yc.Salt,
                        MaVT = YC_MaVT,
                        MaNV = yc.MaNV,
                        IsDeleted = false
                    });

                    yc.TrangThai = "DaDuyet";
                    db.SaveChanges();
                }

                MessageBox.Show($"Duyệt yêu cầu và tạo TK \"{YC_MaTKMoi}\" thành công!",
                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                LoadYeuCau();
                LoadTaiKhoan();
                SelectedYC = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message,
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void XuLyTuChoiYC()
        {
            if (SelectedYC == null) return;

            var confirm = MessageBox.Show(
                $"Từ chối yêu cầu của \"{SelectedYC.HoTen}\"?",
                "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                using (var db = new QLShopGiayEntities())
                {
                    var yc = db.YEU_CAU_DANG_KY.FirstOrDefault(y => y.MaYeuCau == SelectedYC.MaYeuCau);
                    if (yc != null)
                    {
                        yc.TrangThai = "TuChoi";
                        db.SaveChanges();
                    }
                }

                MessageBox.Show("Từ chối yêu cầu thành công!",
                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                LoadYeuCau();
                SelectedYC = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message,
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ════════════════════════════════════════════════════════════════════════
        // HELPER: Hash password (giống logic trong TaiKhoanService)
        // ════════════════════════════════════════════════════════════════════════
        private byte[] HashPassword(Guid salt, string password)
        {
            using (var sha = System.Security.Cryptography.SHA512.Create())
            {
                var input = System.Text.Encoding.UTF8.GetBytes(salt.ToString() + password);
                return sha.ComputeHash(input);
            }
        }
    }
}