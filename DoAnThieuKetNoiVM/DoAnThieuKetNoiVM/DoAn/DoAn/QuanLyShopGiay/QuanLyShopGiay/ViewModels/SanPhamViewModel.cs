using QuanLyShopGiay.Helpers;
using QuanLyShopGiay.Models;
using QuanLyShopGiay.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace QuanLyShopGiay.ViewModels
{
    public class SanPhamViewModel : BaseViewModel
    {
        private readonly SanPhamService _service = new SanPhamService();

        // ══════════════════════════════════════════════════════════════════════
        // BINDING — DANH SÁCH & COMBOBOX
        // ══════════════════════════════════════════════════════════════════════
        private ObservableCollection<object> _danhSachSP;
        public ObservableCollection<object> DanhSachSP { get => _danhSachSP; set => SetProperty(ref _danhSachSP, value); }

        public ObservableCollection<LOAI_HANG> DanhSachLoai { get; set; }
        public ObservableCollection<NHA_SAN_XUAT> DanhSachNSX { get; set; }
        public ObservableCollection<DON_VI_TINH> DanhSachDVT { get; set; }

        // ── Hàng đang chọn trên DataGrid (Thay thế cho sự kiện OnSelectionChanged) ──
        private object _selectedSP;
        public object SelectedSP
        {
            get => _selectedSP;
            set
            {
                if (SetProperty(ref _selectedSP, value))
                    DienVaoForm(value);
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // BINDING — FORM FIELDS
        // ══════════════════════════════════════════════════════════════════════
        private string _maSP;
        private string _tenSP;
        private string _giaBanText;
        private string _maLoai;
        private string _maNSX;
        private string _maDVT;
        private string _tieuDeForm = "Thêm sản phẩm mới";
        private string _tenNutLuu = "Lưu mới";
        private bool _maSPReadOnly;
        private bool _coTheXoa;
        private bool _isEditing;

        public string MaSP { get => _maSP; set => SetProperty(ref _maSP, value); }
        public string TenSP { get => _tenSP; set => SetProperty(ref _tenSP, value); }
        public string GiaBanText { get => _giaBanText; set => SetProperty(ref _giaBanText, value); }
        public string MaLoai { get => _maLoai; set => SetProperty(ref _maLoai, value); }
        public string MaNSX { get => _maNSX; set => SetProperty(ref _maNSX, value); }
        public string MaDVT { get => _maDVT; set => SetProperty(ref _maDVT, value); }
        public string TieuDeForm { get => _tieuDeForm; set => SetProperty(ref _tieuDeForm, value); }
        public string TenNutLuu { get => _tenNutLuu; set => SetProperty(ref _tenNutLuu, value); }
        public bool MaSPReadOnly { get => _maSPReadOnly; set => SetProperty(ref _maSPReadOnly, value); }
        public bool CoTheXoa { get => _coTheXoa; set => SetProperty(ref _coTheXoa, value); }

        // ── Tìm kiếm ──
        private string _tuKhoa = "";
        public string TuKhoa
        {
            get => _tuKhoa;
            set { if (SetProperty(ref _tuKhoa, value)) LoadDanhSach(); }
        }

        // ── Phân quyền (Để tạm true để kiểm thử dễ dàng, điều chỉnh lại khi ráp logic Session) ──
        public bool CoQuyenChinhSua => true; //SessionManager.IsAdmin;

        // ══════════════════════════════════════════════════════════════════════
        // COMMANDS
        // ══════════════════════════════════════════════════════════════════════
        public ICommand ThemMoiCommand { get; }
        public ICommand LuuCommand { get; }
        public ICommand XoaCommand { get; }
        public ICommand HuyCommand { get; }

        // ══════════════════════════════════════════════════════════════════════
        // CONSTRUCTOR
        // ══════════════════════════════════════════════════════════════════════
        public SanPhamViewModel()
        {
            // Command (Cho tham số CanExecute = true để test không bị mờ nút)
            ThemMoiCommand = new RelayCommand(_ => XuLyThemMoi(), _ => true);
            LuuCommand = new RelayCommand(_ => XuLyLuu(), _ => true);
            XoaCommand = new RelayCommand(_ => XuLyXoa(), _ => true);
            HuyCommand = new RelayCommand(_ => XuLyHuy(), _ => true);

            LoadComboBoxes();
            LoadDanhSach();
            XoaForm();
        }

        // ══════════════════════════════════════════════════════════════════════
        // LOAD DATA
        // ══════════════════════════════════════════════════════════════════════
        private void LoadDanhSach()
        {
            string kw = TuKhoa ?? "";
            using (var db = new QuanLyShopGiayEntities())
            {
                // BỎ ĐIỀU KIỆN '.Where(x => x.IsDeleted == false)' TRONG SERVICE
                // Hoặc load trực tiếp tất cả sản phẩm tại đây:
                var ds = db.SAN_PHAM
                            .Include("LOAI_HANG")
                            .Include("NHA_SAN_XUAT")
                            .Include("DON_VI_TINH")
                            .Where(sp => sp.TenSP.Contains(kw)) // Chỉ lọc theo từ khóa
                            .OrderBy(sp => sp.MaSP)
                            .Select(sp => new
                            {
                                sp.MaSP,
                                sp.TenSP,
                                TenLoai = sp.LOAI_HANG != null ? sp.LOAI_HANG.TenLoai : "",
                                TenNSX = sp.NHA_SAN_XUAT != null ? sp.NHA_SAN_XUAT.TenNSX : "",
                                TenDVT = sp.DON_VI_TINH != null ? sp.DON_VI_TINH.TenDVT : "",
                                sp.GiaBan,
                                sp.IsDeleted, // Đảm bảo lấy trường này để DataGrid hiển thị
                                sp.MaLoai,
                                sp.MaNSX,
                                sp.MaDVT
                            })
                            .ToList()
                            .Cast<object>();

                DanhSachSP = new ObservableCollection<object>(ds);
            }
        }

        private void LoadComboBoxes()
        {
            using (var db = new QLShopGiayEntities())
            {
                DanhSachLoai = new ObservableCollection<LOAI_HANG>(db.LOAI_HANG.OrderBy(x => x.TenLoai).ToList());
                DanhSachNSX = new ObservableCollection<NHA_SAN_XUAT>(db.NHA_SAN_XUAT.OrderBy(x => x.TenNSX).ToList());
                DanhSachDVT = new ObservableCollection<DON_VI_TINH>(db.DON_VI_TINH.OrderBy(x => x.TenDVT).ToList());
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // FORM HELPERS
        // ══════════════════════════════════════════════════════════════════════
        private void DienVaoForm(dynamic row)
        {
            if (row == null) { XoaForm(); return; }

            TieuDeForm = "Chỉnh sửa sản phẩm";
            TenNutLuu = "Cập nhật";
            MaSP = row.MaSP;
            TenSP = row.TenSP;
            GiaBanText = row.GiaBan?.ToString("G0"); // Xóa số 0 thừa thập phân
            MaLoai = row.MaLoai;
            MaNSX = row.MaNSX;
            MaDVT = row.MaDVT;
            MaSPReadOnly = true;
            CoTheXoa = true;
            _isEditing = true;
        }

        private void XoaForm()
        {
            TieuDeForm = "Thêm sản phẩm mới";
            TenNutLuu = "💾 Lưu mới";
            MaSP = "";
            TenSP = "";
            GiaBanText = "";
            MaLoai = null;
            MaNSX = null;
            MaDVT = null;
            MaSPReadOnly = false;
            CoTheXoa = false;
            _isEditing = false;
        }

        // ══════════════════════════════════════════════════════════════════════
        // COMMAND HANDLERS
        // ══════════════════════════════════════════════════════════════════════
        private void XuLyThemMoi()
        {
            SelectedSP = null; // Tự kích hoạt trigger Xóa Form
        }

        private void XuLyLuu()
        {
            if (string.IsNullOrWhiteSpace(MaSP) || string.IsNullOrWhiteSpace(TenSP) || string.IsNullOrWhiteSpace(GiaBanText))
            {
                MessageBox.Show("Vui lòng điền đầy đủ thông tin (Mã SP, Tên SP, Giá Bán)!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!decimal.TryParse(GiaBanText, out decimal gia) || gia < 0)
            {
                MessageBox.Show("Giá bán không hợp lệ!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                ServiceResult result;
                string maSP = MaSP.Trim();

                if (_isEditing)
                {
                    result = _service.Update(new SAN_PHAM
                    {
                        MaSP = maSP,
                        TenSP = TenSP.Trim(),
                        MaLoai = MaLoai,
                        MaNSX = MaNSX,
                        MaDVT = MaDVT,
                        GiaBan = gia
                    });
                }
                else
                {
                    result = _service.Insert(new SAN_PHAM
                    {
                        MaSP = maSP,
                        TenSP = TenSP.Trim(),
                        MaLoai = MaLoai,
                        MaNSX = MaNSX,
                        MaDVT = MaDVT,
                        GiaBan = gia,
                        IsDeleted = false
                    });
                }

                if (!result.Success)
                {
                    MessageBox.Show(result.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                MessageBox.Show(result.Message, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                LoadDanhSach();
                XuLyThemMoi();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void XuLyXoa()
        {
            if (!_isEditing || string.IsNullOrEmpty(MaSP))
            {
                MessageBox.Show("Vui lòng chọn một sản phẩm từ danh sách để xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirm = MessageBox.Show("Bạn có chắc muốn ngừng kinh doanh sản phẩm này?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes) return;

            var result = _service.Delete(MaSP);
            MessageBox.Show(result.Message, "Thông báo",
                result.Success ? MessageBoxButton.OK : MessageBoxButton.OK,
                result.Success ? MessageBoxImage.Information : MessageBoxImage.Error);

            if (result.Success)
            {
                LoadDanhSach();
                XuLyThemMoi();
            }
        }

        private void XuLyHuy() => XuLyThemMoi();
    }
}