using Microsoft.Win32;
using QuanLyShopGiay.Command;
using QuanLyShopGiay.Models;
using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Data.Entity;

namespace QuanLyShopGiay.ViewModels
{
    public class SanPhamViewModel : BaseViewModel
    {
        private QLShopGiayEntities3 db = new QLShopGiayEntities3();

        private ObservableCollection<SanPham> _listSanPham;
        public ObservableCollection<SanPham> ListSanPham
        {
            get => _listSanPham;
            set => SetProperty(ref _listSanPham, value);
        }

        public ObservableCollection<LoaiSanPham> ListLoaiSP { get; set; }
        public ObservableCollection<NhaCungCap> ListNCC { get; set; }

        private SanPham _selectedSanPham;
        public SanPham SelectedSanPham
        {
            get => _selectedSanPham;
            set
            {
                if (SetProperty(ref _selectedSanPham, value) && value != null)
                {
                    MaSP = value.MaSP;
                    TenSP = value.TenSP;
                    Size = value.Size;
                    MauSac = value.MauSac;
                    GiaNhap = (decimal?)value.GiaNhap;
                    GiaBan = (decimal?)value.GiaBan;
                    SoLuongTon = value.SoLuongTon ?? 0;
                    GhiChu = value.GhiChu;

                    SelectedLoai = ListLoaiSP.FirstOrDefault(x => x.MaLoai == value.MaLoai);
                    SelectedNCC = ListNCC.FirstOrDefault(x => x.MaNCC == value.MaNCC);

                    IsEditMode = true;
                }
            }
        }

        private bool _isEditMode;
        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetProperty(ref _isEditMode, value);
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                ExecuteSearch();
            }
        }

        private string _maSP; public string MaSP { get => _maSP; set => SetProperty(ref _maSP, value); }
        private string _tenSP; public string TenSP { get => _tenSP; set => SetProperty(ref _tenSP, value); }
        private LoaiSanPham _selectedLoai; public LoaiSanPham SelectedLoai { get => _selectedLoai; set => SetProperty(ref _selectedLoai, value); }
        private NhaCungCap _selectedNCC; public NhaCungCap SelectedNCC { get => _selectedNCC; set => SetProperty(ref _selectedNCC, value); }
        private string _size; public string Size { get => _size; set => SetProperty(ref _size, value); }
        private string _mauSac; public string MauSac { get => _mauSac; set => SetProperty(ref _mauSac, value); }
        private string _ghiChu; public string GhiChu { get => _ghiChu; set => SetProperty(ref _ghiChu, value); }

        private int? _soLuongTon;
        public int? SoLuongTon
        {
            get => _soLuongTon;
            set => SetProperty(ref _soLuongTon, value);
        }

        private decimal? _giaNhap;
        public decimal? GiaNhap
        {
            get => _giaNhap;
            set
            {
                SetProperty(ref _giaNhap, value);
            }
        }

        private decimal? _giaBan;
        public decimal? GiaBan
        {
            get => _giaBan;
            set => SetProperty(ref _giaBan, value);
        }

        public ICommand AddCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand ResetCommand { get; set; }
        public ICommand SearchCommand { get; set; }

        public SanPhamViewModel()
        {
            LoadAllDataSources();

            SearchCommand = new RelayCommand(p => ExecuteSearch(), p => true);
            ResetCommand = new RelayCommand(p => ResetForm(), p => true);

            // 1. THÊM MỚI SẢN PHẨM
            AddCommand = new RelayCommand(
                execute: p =>
                {
                    // Kiểm tra nhập liệu cơ bản (Bỏ bớt bắt buộc điền Mã sản phẩm bằng tay vì sẽ tự kiểm tra trùng)
                    if (string.IsNullOrWhiteSpace(TenSP) || SelectedLoai == null || SelectedNCC == null)
                    {
                        MessageBox.Show("Vui lòng điền các thông tin bắt buộc: Tên SP, Loại và Nhà cung cấp!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    try
                    {
                        // CHỨC NĂNG QUAN TRỌNG: Kiểm tra xem có sản phẩm nào đã tồn tại trùng khớp các thuộc tính cốt lõi không
                        var spTrung = db.SanPhams.FirstOrDefault(x =>
                            x.TenSP.Trim().ToLower() == TenSP.Trim().ToLower() &&
                            x.MaLoai == SelectedLoai.MaLoai &&
                            x.MaNCC == SelectedNCC.MaNCC &&
                            (x.Size ?? "").Trim().ToLower() == (Size ?? "").Trim().ToLower() &&
                            (x.MauSac ?? "").Trim().ToLower() == (MauSac ?? "").Trim().ToLower()
                        );

                        if (spTrung != null)
                        {
                            // NẾU TỒN TẠI SẢN PHẨM TRÙNG: Tiến hành hỏi ý kiến để cộng dồn số lượng vào kho
                            var xacNhan = MessageBox.Show($"Sản phẩm này đã tồn tại trong hệ thống (Mã: {spTrung.MaSP} - Số lượng hiện tại: {spTrung.SoLuongTon}).\n\nBạn có muốn CỘNG DỒN {SoLuongTon ?? 0} sản phẩm mới nhập này vào lô hàng cũ không?",
                                                          "Phát hiện sản phẩm trùng khớp", MessageBoxButton.YesNo, MessageBoxImage.Question);

                            if (xacNhan == MessageBoxResult.Yes)
                            {
                                // Cộng dồn số lượng tồn kho
                                spTrung.SoLuongTon = (spTrung.SoLuongTon ?? 0) + (SoLuongTon ?? 0);

                                // Cập nhật lại giá bán mới nếu người dùng nhập thay đổi
                                if ((GiaBan ?? 0) > 0)
                                {
                                    spTrung.GiaBan = GiaBan ?? 0m;
                                }

                                db.Entry(spTrung).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();

                                MessageBox.Show($"Đã cộng dồn thành công vào sản phẩm {spTrung.MaSP}!\nSố lượng tồn kho mới sau cộng: {spTrung.SoLuongTon}", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                                ExecuteSearch();
                                ResetForm();
                            }
                            return; // Thoát hàm xử lý thêm mới, không chạy xuống phần sinh mã mới phía dưới
                        }

                        // --- TRƯỜNG HỢP LÀ SẢN PHẨM MỚI HOÀN TOÀN (TỰ ĐỘNG SINH MÃ SP0001, SP0002...) ---
                        string maMoi = MaSP?.Trim();
                        if (string.IsNullOrWhiteSpace(maMoi))
                        {
                            // Tìm mã sản phẩm lớn nhất hiện tại trong DB
                            var lastSP = db.SanPhams.OrderByDescending(x => x.MaSP).FirstOrDefault();
                            int nextNum = 1;
                            if (lastSP != null && lastSP.MaSP.StartsWith("SP"))
                            {
                                if (int.TryParse(lastSP.MaSP.Substring(2), out int num))
                                {
                                    nextNum = num + 1;
                                }
                            }
                            maMoi = "SP" + nextNum.ToString("D4"); // Định dạng thành chuỗi dạng SP0001
                        }
                        else
                        {
                            // Nếu người dùng cố tình tự nhập mã thủ công, kiểm tra xem mã đó có bị trùng trong DB không
                            if (db.SanPhams.Any(x => x.MaSP == maMoi))
                            {
                                MessageBox.Show("Mã sản phẩm này đã được sử dụng trong hệ thống!", "Lỗi trùng mã", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                        }

                        if ((GiaBan ?? 0) < (GiaNhap ?? 0))
                        {
                            MessageBox.Show("Giá bán ra không được phép nhỏ hơn giá nhập vào kho!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        // Tiến hành chèn bản ghi mới hoàn toàn vào Database
                        var newSP = new SanPham()
                        {
                            MaSP = maMoi,
                            TenSP = TenSP.Trim(),
                            MaLoai = SelectedLoai.MaLoai,
                            MaNCC = SelectedNCC.MaNCC,
                            Size = Size?.Trim(),
                            MauSac = MauSac?.Trim(),
                            GiaNhap = GiaNhap ?? 0m,
                            GiaBan = GiaBan ?? 0m,
                            SoLuongTon = SoLuongTon ?? 0,
                            GhiChu = GhiChu?.Trim() ?? ""
                        };

                        db.SanPhams.Add(newSP);
                        db.SaveChanges();

                        MessageBox.Show($"Thêm mới sản phẩm thành công với mã tự động: {maMoi}!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                        ExecuteSearch();
                        ResetForm();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi trong quá trình xử lý lưu sản phẩm: " + ex.Message, "Thất bại", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                },
                canExecute: p => true
            );

            // 2. CẬP NHẬT SẢN PHẨM (ĐÃ SỬA ĐỔI TOÀN DIỆN KIỂU DỮ LIỆU)
            EditCommand = new RelayCommand(
                execute: p =>
                {
                    if (!IsEditMode)
                    {
                        MessageBox.Show("Vui lòng chọn một sản phẩm từ bảng danh sách trước khi chỉnh sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(TenSP) || SelectedLoai == null || SelectedNCC == null)
                    {
                        MessageBox.Show("Vui lòng không bỏ trống các trường bắt buộc (*) !", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    if ((GiaBan ?? 0) < (GiaNhap ?? 0))
                    {
                        MessageBox.Show("Giá bán ra không được nhỏ hơn giá nhập kho!", "Cảnh báo giá", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var currentSP = db.SanPhams.SingleOrDefault(x => x.MaSP == MaSP);
                    if (currentSP != null)
                    {
                        currentSP.TenSP = TenSP.Trim();
                        currentSP.MaLoai = SelectedLoai.MaLoai;
                        currentSP.MaNCC = SelectedNCC.MaNCC;
                        currentSP.Size = Size?.Trim();
                        currentSP.MauSac = MauSac?.Trim();
                        currentSP.GiaNhap = GiaNhap ?? 0m;
                        currentSP.GiaBan = GiaBan ?? 0m;
                        currentSP.SoLuongTon = SoLuongTon ?? 0;
                        currentSP.GhiChu = GhiChu?.Trim();

                        db.SaveChanges();
                        MessageBox.Show("Cập nhật thông tin sản phẩm thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                        ExecuteSearch();
                        ResetForm();
                    }
                },
                canExecute: p => true
            );

            // 3. XÓA SẢN PHẨM
            DeleteCommand = new RelayCommand(
                execute: p =>
                {
                    // Kiểm tra xem người dùng đã thực sự click chọn một dòng trên DataGrid chưa
                    if (SelectedSanPham == null)
                    {
                        MessageBox.Show("Vui lòng click chọn 1 sản phẩm từ bảng danh sách bên phải trước khi bấm Xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Hỏi xác nhận trước khi xóa vĩnh viễn
                    var confirm = MessageBox.Show($"Bạn có chắc chắn muốn xóa vĩnh viễn sản phẩm: {SelectedSanPham.TenSP} (Mã: {SelectedSanPham.MaSP}) không?",
                                                 "Xác nhận xóa hàng", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (confirm == MessageBoxResult.Yes)
                    {
                        try
                        {
                            // Tìm thực thể chuẩn xác nhất đang nằm trong Database
                            var itemToRemove = db.SanPhams.FirstOrDefault(x => x.MaSP == SelectedSanPham.MaSP);
                            if (itemToRemove != null)
                            {
                                // KIỂM TRA RÀNG BUỘC TOÀN VẸN: Nếu sản phẩm này đã phát sinh giao dịch trong Chi Tiết Phiếu Nhập hoặc Chi Tiết Hóa Đơn Bán
                                bool daTungSuDung = db.ChiTietHoaDonNhaps.Any(x => x.MaSP == itemToRemove.MaSP) ||
                                                    db.ChiTietHoaDons.Any(x => x.MaSP == itemToRemove.MaSP);

                                if (daTungSuDung)
                                {
                                    MessageBox.Show("Không thể xóa sản phẩm này! Vì mã của nó đã nằm trong lịch sử hóa đơn nhập/xuất kho của cửa hàng.",
                                                    "Lỗi ràng buộc dữ liệu SQL", MessageBoxButton.OK, MessageBoxImage.Stop);
                                    return;
                                }

                                // Tiến hành xóa khỏi database nếu không dính dáng đến hóa đơn nào
                                db.SanPhams.Remove(itemToRemove);
                                db.SaveChanges();

                                MessageBox.Show("Đã xóa sản phẩm thành công khỏi hệ thống!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                                ExecuteSearch();
                                ResetForm();
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Không thể xóa sản phẩm do lỗi hệ thống: " + ex.Message, "Lỗi kết nối", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                },
                canExecute: p => true
            );
        }
        private void LoadAllDataSources()
        {
            try
            {
                ListLoaiSP = new ObservableCollection<LoaiSanPham>(db.LoaiSanPhams.ToList());
                ListNCC = new ObservableCollection<NhaCungCap>(db.NhaCungCaps.ToList());
                ExecuteSearch();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message, "Lỗi kết nối", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteSearch()
        {
            try
            {
                // BƯỚC QUAN TRỌNG: Làm mới toàn bộ danh sách Sản phẩm từ Database gốc, 
                // hủy bỏ mọi dữ liệu cũ đang bị lưu tạm trên RAM của trang này.
                foreach (var entry in db.ChangeTracker.Entries<SanPham>())
                {
                    entry.Reload();
                }

                if (string.IsNullOrWhiteSpace(SearchText))
                {
                    ListSanPham = new ObservableCollection<SanPham>(
                        db.SanPhams.Include(x => x.LoaiSanPham).Include(x => x.NhaCungCap).AsNoTracking().ToList()
                    );
                }
                else
                {
                    string key = SearchText.Trim().ToLower();
                    ListSanPham = new ObservableCollection<SanPham>(
                        db.SanPhams.Include(x => x.LoaiSanPham).Include(x => x.NhaCungCap)
                                   .Where(x => x.TenSP.ToLower().Contains(key)).AsNoTracking().ToList()
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi làm mới danh sách sản phẩm: " + ex.Message);
            }
        }

        private void ResetForm()
        {
            MaSP = string.Empty;
            TenSP = string.Empty;
            SelectedLoai = null;
            SelectedNCC = null;
            Size = string.Empty;
            MauSac = string.Empty;
            GiaNhap = 0;
            GiaBan = 0;
            SoLuongTon = 0;
            GhiChu = string.Empty;
            IsEditMode = false;
            SelectedSanPham = null;
        }
    }
}