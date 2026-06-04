using QuanLyShopGiay.Helpers;
using QuanLyShopGiay.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace QuanLyShopGiay.ViewModels
{
    /// <summary>
    /// ViewModel cho trang Nhân viên.
    /// Gom toàn bộ Load / Thêm / Sửa / Xóa + binding dữ liệu.
    /// Chỉ Admin mới được Thêm / Sửa / Xóa.
    /// </summary>
    public class NhanVienViewModel : BaseViewModel
    {
        // ══════════════════════════════════════════════════════════════════════
        // BINDING PROPERTIES
        // ══════════════════════════════════════════════════════════════════════

        // ── Danh sách hiển thị trên DataGrid ─────────────────────────────────
        private ObservableCollection<NHAN_VIEN> _danhSachNV;
        public ObservableCollection<NHAN_VIEN> DanhSachNV
        {
            get => _danhSachNV;
            set => SetProperty(ref _danhSachNV, value);
        }

        // ── Hàng đang chọn ────────────────────────────────────────────────────
        private NHAN_VIEN _selectedNV;
        public NHAN_VIEN SelectedNV
        {
            get => _selectedNV;
            set
            {
                if (SetProperty(ref _selectedNV, value))
                    DienVaoForm(value);
            }
        }

        // ── Form fields ───────────────────────────────────────────────────────
        private string _maNV;
        private string _hoTen;
        private DateTime? _ngaySinh;
        private string _email;
        private string _tieuDeForm = "Thêm nhân viên mới";
        private bool _maNVReadOnly;
        private bool _isEditing;
        private string _tenNutLuu = "Lưu mới";

        public string MaNV
        {
            get => _maNV;
            set => SetProperty(ref _maNV, value);
        }
        public string HoTen
        {
            get => _hoTen;
            set => SetProperty(ref _hoTen, value);
        }
        public DateTime? NgaySinh
        {
            get => _ngaySinh;
            set => SetProperty(ref _ngaySinh, value);
        }
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }
        public string TieuDeForm
        {
            get => _tieuDeForm;
            set => SetProperty(ref _tieuDeForm, value);
        }
        public bool MaNVReadOnly
        {
            get => _maNVReadOnly;
            set => SetProperty(ref _maNVReadOnly, value);
        }
        public string TenNutLuu { get => _tenNutLuu; set => SetProperty(ref _tenNutLuu, value); }

        // ── Tìm kiếm ──────────────────────────────────────────────────────────
        private string _tuKhoa = "";
        public string TuKhoa
        {
            get => _tuKhoa;
            set
            {
                if (SetProperty(ref _tuKhoa, value))
                    LoadDanhSach();
            }
        }

        // ── Phân quyền ────────────────────────────────────────────────────────
        /// <summary>Chỉ Admin mới thấy/dùng được nút Thêm, Lưu, Xóa.</summary>
        public bool CoQuyenChinhSua => SessionManager.IsAdmin;

        // ── Trạng thái nút Xóa ────────────────────────────────────────────────
        private bool _coTheXoa;
        public bool CoTheXoa
        {
            get => _coTheXoa;
            set => SetProperty(ref _coTheXoa, value);
        }

        // ══════════════════════════════════════════════════════════════════════
        // COMMANDS  (Thêm / Sửa / Xóa / Hủy / Tìm kiếm)
        // ══════════════════════════════════════════════════════════════════════
        public ICommand ThemMoiCommand { get; }
        public ICommand LuuCommand { get; }
        public ICommand XoaCommand { get; }
        public ICommand HuyCommand { get; }

        // ══════════════════════════════════════════════════════════════════════
        // CONSTRUCTOR
        // ══════════════════════════════════════════════════════════════════════
        public NhanVienViewModel()
        {
            ThemMoiCommand = new RelayCommand(_ => XuLyThemMoi(), _ => CoQuyenChinhSua);
            LuuCommand = new RelayCommand(_ => XuLyLuu(), _ => CoQuyenChinhSua);
            XoaCommand = new RelayCommand(_ => XuLyXoa(), _ => CoQuyenChinhSua && CoTheXoa);
            HuyCommand = new RelayCommand(_ => XuLyHuy());

            LoadDanhSach();
            XoaForm();
        }

        // ══════════════════════════════════════════════════════════════════════
        // LOAD
        // ══════════════════════════════════════════════════════════════════════
        private void LoadDanhSach()
        {
            string kw = TuKhoa ?? "";
            using (var db = new QLShopGiayEntities())
            {
                var ds = db.NHAN_VIEN
                               .Where(nv => (nv.HoTen.Contains(kw) || nv.MaNV.Contains(kw)) && nv.IsDeleted == false)
                               .OrderBy(nv => nv.MaNV)
                               .ToList();

                DanhSachNV = new ObservableCollection<NHAN_VIEN>(ds);
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // FORM HELPERS
        // ══════════════════════════════════════════════════════════════════════
        private void DienVaoForm(NHAN_VIEN nv)
        {
            if (nv == null) { XoaForm(); return; }

            // Gán dữ liệu từ object vào các Property để Binding ra UI
            MaNV = nv.MaNV;
            HoTen = nv.HoTen;
            NgaySinh = nv.NgaySinh;
            Email = nv.Email ?? "";

            // Thiết lập trạng thái Form
            TieuDeForm = "Chỉnh sửa nhân viên";
            TenNutLuu = "Cập nhật";    // Đổi tên nút thành "Cập nhật"
            MaNVReadOnly = true;       // Khóa Mã NV để không cho sửa mã khi cập nhật
            CoTheXoa = true;           // Cho phép hiện nút "Nghỉ việc"
            _isEditing = true;         // Đánh dấu đây là hành động Update
        }

        private void XoaForm()
        {
            // Reset các giá trị về mặc định
            MaNV = "";
            HoTen = "";
            NgaySinh = null;
            Email = "";

            // Reset trạng thái Form
            TieuDeForm = "Thêm nhân viên mới";
            TenNutLuu = "Lưu mới";     // Đổi tên nút thành "Lưu mới"
            MaNVReadOnly = false;      // Cho phép nhập Mã NV mới
            CoTheXoa = false;          // Ẩn nút "Nghỉ việc" khi đang thêm mới
            _isEditing = false;        // Đánh dấu đây là hành động Insert
        }

        // ══════════════════════════════════════════════════════════════════════
        // COMMAND HANDLERS
        // ══════════════════════════════════════════════════════════════════════

        // ── Thêm mới ──────────────────────────────────────────────────────────
        private void XuLyThemMoi()
        {
            SelectedNV = null;
            XoaForm();
        }

        // ── Lưu (Insert / Update) ─────────────────────────────────────────────
        private void XuLyLuu()
        {
            if (string.IsNullOrWhiteSpace(MaNV) || string.IsNullOrWhiteSpace(HoTen))
            {
                MessageBox.Show("Mã nhân viên và họ tên không được để trống!",
                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!string.IsNullOrWhiteSpace(Email))
            {
                bool emailHopLe = Regex.IsMatch(Email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                if (!emailHopLe)
                {
                    MessageBox.Show("Email không đúng định dạng!",
                        "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            try
            {
                using (var db = new QLShopGiayEntities())
                {
                    if (_isEditing)
                    {
                        // ── UPDATE ──────────────────────────────────────────
                        var existing = db.NHAN_VIEN.FirstOrDefault(nv => nv.MaNV == MaNV);
                        if (existing == null)
                        {
                            MessageBox.Show("Không tìm thấy nhân viên!", "Lỗi",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        existing.HoTen = HoTen.Trim();
                        existing.NgaySinh = NgaySinh;
                        existing.Email = string.IsNullOrWhiteSpace(Email) ? null : Email.Trim();
                    }
                    else
                    {
                        // ── INSERT ──────────────────────────────────────────
                        if (db.NHAN_VIEN.Any(nv => nv.MaNV == MaNV.Trim()))
                        {
                            MessageBox.Show("Mã nhân viên đã tồn tại!", "Thông báo",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        db.NHAN_VIEN.Add(new NHAN_VIEN
                        {
                            MaNV = MaNV.Trim(),
                            HoTen = HoTen.Trim(),
                            NgaySinh = NgaySinh,
                            Email = string.IsNullOrWhiteSpace(Email) ? null : Email.Trim(),
                            IsDeleted = false
                        });
                    }
                    db.SaveChanges();
                }

                MessageBox.Show(_isEditing ? "Cập nhật thành công!" : "Thêm nhân viên thành công!",
                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                LoadDanhSach();
                XoaForm();
                SelectedNV = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ── Xóa (Soft Delete) ─────────────────────────────────────────────────
        private void XuLyXoa()
        {
            var confirm = MessageBox.Show(
                $"Bạn có chắc muốn đánh dấu \"{HoTen}\" là nghỉ việc?",
                "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                using (var db = new QLShopGiayEntities())
                {
                    var nv = db.NHAN_VIEN.FirstOrDefault(x => x.MaNV == MaNV);
                    if (nv != null)
                    {
                        nv.IsDeleted = true;
                        db.SaveChanges();
                    }
                }

                MessageBox.Show("Đã đánh dấu nhân viên nghỉ việc!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                LoadDanhSach();
                XoaForm();
                SelectedNV = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ── Hủy ──────────────────────────────────────────────────────────────
        private void XuLyHuy()
        {
            SelectedNV = null;
            XoaForm();
        }
    }
}