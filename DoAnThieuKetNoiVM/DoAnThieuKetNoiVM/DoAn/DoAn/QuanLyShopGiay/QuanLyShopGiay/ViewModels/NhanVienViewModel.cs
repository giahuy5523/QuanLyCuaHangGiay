using QuanLyShopGiay.Helpers;
using QuanLyShopGiay.Models;
using QuanLyShopGiay.Command; // Đảm bảo đúng namespace RelayCommand của bạn
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace QuanLyShopGiay.ViewModels
{
    /// <summary>
    /// ViewModel cho trang Nhân viên — CRUD đầy đủ, dùng EF (QLShopGiayEntities3).
    /// Chỉ Admin (SessionManager.IsAdmin) mới được Thêm / Sửa / Xóa.
    /// </summary>
    public class NhanVienViewModel : BaseViewModel
    {
        // ══════════════════════════════════════════════════════════════════════
        // PROPERTIES — DataGrid
        // ══════════════════════════════════════════════════════════════════════

        private ObservableCollection<NhanVien> _danhSachNV;
        public ObservableCollection<NhanVien> DanhSachNV
        {
            get => _danhSachNV;
            set => SetProperty(ref _danhSachNV, value);
        }

        private NhanVien _selectedNV;
        public NhanVien SelectedNV
        {
            get => _selectedNV;
            set
            {
                if (SetProperty(ref _selectedNV, value))
                {
                    DienVaoForm(value);
                    // Khi chọn hoặc bỏ chọn nhân viên dưới lưới, ép hệ thống quét lại điều kiện nút Xóa
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // PROPERTIES — Form Binding
        // ══════════════════════════════════════════════════════════════════════

        private string _maNV;
        public string MaNV
        {
            get => _maNV;
            set => SetProperty(ref _maNV, value);
        }

        private string _tenNV;
        public string TenNV
        {
            get => _tenNV;
            set => SetProperty(ref _tenNV, value);
        }

        private string _gioiTinh;
        public string GioiTinh
        {
            get => _gioiTinh;
            set => SetProperty(ref _gioiTinh, value);
        }

        private DateTime? _ngaySinh;
        public DateTime? NgaySinh
        {
            get => _ngaySinh;
            set => SetProperty(ref _ngaySinh, value);
        }

        private string _sdt;
        public string SDT
        {
            get => _sdt;
            set => SetProperty(ref _sdt, value);
        }

        private string _diaChi;
        public string DiaChi
        {
            get => _diaChi;
            set => SetProperty(ref _diaChi, value);
        }

        private string _tenDangNhap;
        public string TenDangNhap
        {
            get => _tenDangNhap;
            set => SetProperty(ref _tenDangNhap, value);
        }

        private string _matKhau;
        public string MatKhau
        {
            get => _matKhau;
            set => SetProperty(ref _matKhau, value);
        }

        private string _quyen;
        public string Quyen
        {
            get => _quyen;
            set => SetProperty(ref _quyen, value);
        }

        private ObservableCollection<string> _quyenList;
        public ObservableCollection<string> QuyenList
        {
            get => _quyenList;
            set => SetProperty(ref _quyenList, value);
        }

        // ══════════════════════════════════════════════════════════════════════
        // PROPERTIES — State Giao diện & Phân quyền (ĐÃ SỬA ĐỒNG BỘ)
        // ══════════════════════════════════════════════════════════════════════

        private bool _coQuyenChinhSua;
        public bool CoQuyenChinhSua
        {
            get => _coQuyenChinhSua;
            set
            {
                if (SetProperty(ref _coQuyenChinhSua, value))
                {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        private bool _coTheXoa;
        public bool CoTheXoa
        {
            get => _selectedNV != null; // Trả về true nếu đang chọn 1 nhân viên dưới Grid
        }

        private bool _maNVReadOnly;
        public bool MaNVReadOnly
        {
            get => _maNVReadOnly;
            set => SetProperty(ref _maNVReadOnly, value);
        }

        private string _tuKhoa;
        public string TuKhoa
        {
            get => _tuKhoa;
            set
            {
                if (SetProperty(ref _tuKhoa, value))
                {
                    XulyTimKiem();
                }
            }
        }

        private string _tieuDeForm;
        public string TieuDeForm
        {
            get => _tieuDeForm;
            set => SetProperty(ref _tieuDeForm, value);
        }

        private string _tenNutLuu;
        public string TenNutLuu
        {
            get => _tenNutLuu;
            set => SetProperty(ref _tenNutLuu, value);
        }

        // ══════════════════════════════════════════════════════════════════════
        // COMMANDS
        // ══════════════════════════════════════════════════════════════════════
        public ICommand ThemMoiCommand { get; set; }
        public ICommand LuuCommand { get; set; }
        public ICommand XoaCommand { get; set; }
        public ICommand HuyCommand { get; set; }
        public ICommand LoadQuyenCommand { get; set; }

        // ══════════════════════════════════════════════════════════════════════
        // CONSTRUCTOR — KHỞI TẠO VÀ PHÂN QUYỀN TRÊN COMMAND
        // ══════════════════════════════════════════════════════════════════════
        public NhanVienViewModel()
        {
            // 1. Gán quyền dựa vào SessionManager thực tế của bạn
            CoQuyenChinhSua = SessionManager.IsAdmin;

            // 2. Khởi tạo danh sách quyền hiển thị lên ComboBox
            QuyenList = new ObservableCollection<string> { "QuanLy", "BanHang", "KhoQuy" };
            // Đăng ký lệnh cập nhật quyền khi màn hình load
            LoadQuyenCommand = new RelayCommand(_ => XuLyLoadQuyen());

            // Gọi luôn một lần sau khi khởi tạo
            XuLyLoadQuyen();

            // 3. Phân quyền và ràng buộc điều kiện CanExecute trực tiếp tại đây
            ThemMoiCommand = new RelayCommand(_ => XuLyThemMoi(), _ => CoQuyenChinhSua);
            LuuCommand = new RelayCommand(_ => XuLyLuu(), _ => CoQuyenChinhSua);
            XoaCommand = new RelayCommand(_ => XuLyXoa(), _ => CoQuyenChinhSua && CoTheXoa);
            HuyCommand = new RelayCommand(_ => XuLyHuy());

            LoadDanhSach();
            XoaForm();
        }

        // ══════════════════════════════════════════════════════════════════════
        // METHODS / LOGIC XỬ LÝ
        // ══════════════════════════════════════════════════════════════════════

        private void LoadDanhSach()
        {
            try
            {
                using (var db = new QLShopGiayEntities())
                {
                    var list = db.NhanViens.ToList();
                    DanhSachNV = new ObservableCollection<NhanVien>(list);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void XulyTimKiem()
        {
            try
            {
                using (var db = new QLShopGiayEntities())
                {
                    if (string.IsNullOrWhiteSpace(_tuKhoa))
                    {
                        var list = db.NhanViens.ToList();
                        DanhSachNV = new ObservableCollection<NhanVien>(list);
                    }
                    else
                    {
                        string tk = _tuKhoa.Trim().ToLower();
                        var filtered = db.NhanViens.Where(n =>
                            n.MaNhanVien.ToLower().Contains(tk) ||
                            n.TenNhanVien.ToLower().Contains(tk) ||
                            n.SoDienThoai.Contains(tk)
                        ).ToList();
                        DanhSachNV = new ObservableCollection<NhanVien>(filtered);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tìm kiếm: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DienVaoForm(NhanVien nv)
        {
            if (nv == null) return;

            TieuDeForm = "📝 Cập nhật thông tin";
            TenNutLuu = "💾 Lưu cập nhật";
            MaNVReadOnly = true;

            MaNV = nv.MaNhanVien;
            TenNV = nv.TenNhanVien;
            GioiTinh = nv.GioiTinh;
            NgaySinh = nv.NgaySinh;
            SDT = nv.SoDienThoai;
            DiaChi = nv.DiaChi;
            TenDangNhap = nv.TenDangNhap;
            MatKhau = ""; // Để trống khi cập nhật nếu không muốn đổi mật khẩu
            Quyen = nv.Quyen;
        }

        private void XoaForm()
        {
            TieuDeForm = "✨ Thêm nhân viên mới";
            TenNutLuu = "➕ Thêm mới";
            MaNVReadOnly = false;

            MaNV = "";
            TenNV = "";
            GioiTinh = "Nam";
            NgaySinh = DateTime.Now.AddYears(-20);
            SDT = "";
            DiaChi = "";
            TenDangNhap = "";
            MatKhau = "";
            Quyen = "BanHang";
        }

        private void XuLyThemMoi()
        {
            SelectedNV = null;
            XoaForm();
        }

        private void XuLyLuu()
        {
            if (string.IsNullOrWhiteSpace(MaNV) || string.IsNullOrWhiteSpace(TenNV) || string.IsNullOrWhiteSpace(TenDangNhap))
            {
                MessageBox.Show("Vui lòng điền đầy đủ các thông tin bắt buộc (*)", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!Regex.IsMatch(SDT, @"^[0-9]{10,11}$") && !string.IsNullOrEmpty(SDT))
            {
                MessageBox.Show("Số điện thoại không hợp lệ (Phải từ 10-11 số)", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new QLShopGiayEntities())
                {
                    if (!MaNVReadOnly) // THỜI ĐIỂM: THÊM MỚI
                    {
                        if (db.NhanViens.Any(n => n.MaNhanVien == MaNV))
                        {
                            MessageBox.Show("Mã nhân viên này đã tồn tại trong hệ thống!", "Trùng khóa chính", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        if (db.NhanViens.Any(n => n.TenDangNhap == TenDangNhap))
                        {
                            MessageBox.Show("Tên đăng nhập này đã được sử dụng!", "Trùng tài khoản", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        if (string.IsNullOrWhiteSpace(MatKhau))
                        {
                            MessageBox.Show("Mật khẩu không được để trống khi thêm mới!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        var newNv = new NhanVien
                        {
                            MaNhanVien = MaNV.Trim(),
                            TenNhanVien = TenNV.Trim(),
                            GioiTinh = GioiTinh,
                            NgaySinh = NgaySinh,
                            SoDienThoai = SDT,
                            DiaChi = DiaChi,
                            TenDangNhap = TenDangNhap.Trim(),
                            MatKhau = HashSHA256(MatKhau),
                            Quyen = Quyen
                        };

                        db.NhanViens.Add(newNv);
                        db.SaveChanges();
                        MessageBox.Show("Đã thêm mới nhân viên thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else // THỜI ĐIỂM: SỬA ĐỔI / CẬP NHẬT
                    {
                        var editNv = db.NhanViens.FirstOrDefault(n => n.MaNhanVien == MaNV);
                        if (editNv != null)
                        {
                            if (db.NhanViens.Any(n => n.TenDangNhap == TenDangNhap && n.MaNhanVien != MaNV))
                            {
                                MessageBox.Show("Tên đăng nhập này đã được sử dụng bởi một nhân viên khác!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return;
                            }

                            editNv.TenNhanVien = TenNV.Trim();
                            editNv.GioiTinh = GioiTinh;
                            editNv.NgaySinh = NgaySinh;
                            editNv.SoDienThoai = SDT;
                            editNv.DiaChi = DiaChi;
                            editNv.TenDangNhap = TenDangNhap.Trim();
                            editNv.Quyen = Quyen;

                            if (!string.IsNullOrWhiteSpace(MatKhau))
                            {
                                editNv.MatKhau = HashSHA256(MatKhau);
                            }

                            db.SaveChanges();
                            MessageBox.Show("Cập nhật thông tin nhân viên thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }

                LoadDanhSach();
                XoaForm();
                SelectedNV = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi trong quá trình lưu dữ liệu:\n{ex.Message}", "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void XuLyXoa()
        {
            if (SelectedNV == null) return;

            var res = MessageBox.Show($"Bạn có thực sự muốn xóa nhân viên '{SelectedNV.TenNhanVien}' ra khỏi hệ thống?",
                "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (res != MessageBoxResult.Yes) return;

            try
            {
                using (var db = new QLShopGiayEntities())
                {
                    var nv = db.NhanViens.FirstOrDefault(n => n.MaNhanVien == SelectedNV.MaNhanVien);
                    if (nv == null) return;

                    // Kiểm tra ràng buộc dữ liệu hóa đơn của nhân viên trước khi xóa nhằm bảo vệ database
                    if (db.HoaDons.Any(h => h.MaNhanVien == nv.MaNhanVien))
                    {
                        MessageBox.Show("Nhân viên này đã từng lập hóa đơn bán hàng cho dữ liệu.\nKhông thể xóa nhân viên này khỏi hệ thống để bảo toàn lịch sử bán hàng.",
                            "Ràng buộc dữ liệu - Không thể xóa", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    db.NhanViens.Remove(nv);
                    db.SaveChanges();
                }

                MessageBox.Show("Đã xóa nhân viên thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                LoadDanhSach();
                XoaForm();
                SelectedNV = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi trong quá trình xóa dữ liệu:\n{ex.Message}", "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void XuLyHuy()
        {
            SelectedNV = null;
            XoaForm();
        }
        private void XuLyLoadQuyen()
        {
            // Kiểm tra quyền từ đúng nơi đã lưu ở Bước 1
            if (SessionManager.CurrentUser != null && SessionManager.CurrentUser.Quyen.Trim() == "QuanLy")
            {
                CoQuyenChinhSua = true;
            }
            else
            {
                CoQuyenChinhSua = false;
            }

            // Ép giao diện cập nhật ngay
            System.Windows.Input.CommandManager.InvalidateRequerySuggested();
        }

        // ══════════════════════════════════════════════════════════════════════
        // CRYPTOGRAPHY HELPERS
        // ══════════════════════════════════════════════════════════════════════
        private static string HashSHA256(string input)
        {
            using (var sha = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(input);
                byte[] hashed = sha.ComputeHash(bytes);
                return BitConverter.ToString(hashed).Replace("-", "").ToLower();
            }
        }
    }
}