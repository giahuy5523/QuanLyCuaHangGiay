using QuanLyShopGiay.Command;
using QuanLyShopGiay.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace QuanLyShopGiay.ViewModels
{
    public class LoaiSanPhamViewModel : BaseViewModel
    {
        private QLShopGiayEntities db = new QLShopGiayEntities();

        // Danh sách nạp lên DataGrid loại sản phẩm
        private ObservableCollection<LoaiSanPham> _listLoaiSP;
        public ObservableCollection<LoaiSanPham> ListLoaiSP
        {
            get => _listLoaiSP;
            set => SetProperty(ref _listLoaiSP, value);
        }

        // Bắt sự kiện chọn dòng trên bảng DataGrid
        private LoaiSanPham _selectedLoaiSP;
        public LoaiSanPham SelectedLoaiSP
        {
            get => _selectedLoaiSP;
            set
            {
                if (SetProperty(ref _selectedLoaiSP, value) && value != null)
                {
                    MaLoai = value.MaLoai;
                    TenLoai = value.TenLoai;
                    IsEditMode = true;
                }
            }
        }

        // Biến kiểm soát trạng thái form (Thêm mới hay Sửa)
        private bool _isEditMode;
        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetProperty(ref _isEditMode, value);
        }

        // Ô nhập tìm kiếm (Gõ chữ tới đâu tự lọc tới đó)
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

        // Thuộc tính binding trực tiếp với TextBox nhập liệu
        private string _maLoai; public string MaLoai { get => _maLoai; set => SetProperty(ref _maLoai, value); }
        private string _tenLoai; public string TenLoai { get => _tenLoai; set => SetProperty(ref _tenLoai, value); }

        // ===== LỊCH SỬ THAO TÁC =====
        private ObservableCollection<string> _lichSuThaoTac = new ObservableCollection<string>();
        public ObservableCollection<string> LichSuThaoTac
        {
            get => _lichSuThaoTac;
            set => SetProperty(ref _lichSuThaoTac, value);
        }

        private void ThemLichSu(string hanhDong, string maLoai, string tenLoai)
        {
            string thoiGian = DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy");
            string ghiChu = $"[{thoiGian}]  {hanhDong}  |  Mã: {maLoai}  |  Tên: {tenLoai}";
            LichSuThaoTac.Insert(0, ghiChu); // Chèn lên đầu để thao tác mới nhất hiển thị trên cùng
        }

        public ICommand ClearHistoryCommand { get; set; }

        // Khai báo các lệnh thực thi hành động
        public ICommand AddCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand ResetCommand { get; set; }

        public LoaiSanPhamViewModel()
        {
            ExecuteSearch();

            ResetCommand = new RelayCommand(p => ResetForm());

            ClearHistoryCommand = new RelayCommand(p =>
            {
                LichSuThaoTac.Clear();
            });

            // 1. CHỨC NĂNG THÊM MỚI
            AddCommand = new RelayCommand(p =>
            {
                if (string.IsNullOrWhiteSpace(MaLoai) || string.IsNullOrWhiteSpace(TenLoai))
                {
                    MessageBox.Show("Vui lòng điền đầy đủ Mã loại và Tên loại sản phẩm!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (db.LoaiSanPhams.Any(x => x.MaLoai == MaLoai.Trim()))
                {
                    MessageBox.Show("Mã loại sản phẩm này đã tồn tại trên hệ thống!", "Lỗi dữ liệu", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var newLoai = new LoaiSanPham()
                {
                    MaLoai = MaLoai.Trim(),
                    TenLoai = TenLoai.Trim()
                };

                db.LoaiSanPhams.Add(newLoai);
                db.SaveChanges();
                MessageBox.Show("Thêm mới loại sản phẩm thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                ThemLichSu("✅ THÊM MỚI", MaLoai.Trim(), TenLoai.Trim());
                ExecuteSearch();
                ResetForm();
            });

            // 2. CHỨC NĂNG CẬP NHẬT
            EditCommand = new RelayCommand(p =>
            {
                if (!IsEditMode)
                {
                    MessageBox.Show("Vui lòng click chọn một loại sản phẩm từ bảng bên phải trước!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(TenLoai))
                {
                    MessageBox.Show("Tên loại sản phẩm không được phép để trống!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var currentLoai = db.LoaiSanPhams.SingleOrDefault(x => x.MaLoai == MaLoai);
                if (currentLoai != null)
                {
                    string tenCu = currentLoai.TenLoai;
                    currentLoai.TenLoai = TenLoai.Trim();
                    db.SaveChanges();
                    MessageBox.Show("Cập nhật thông tin thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                    ThemLichSu($"✏️ CẬP NHẬT  (Tên cũ: {tenCu})", MaLoai, TenLoai.Trim());
                    ExecuteSearch();
                    ResetForm();
                }
            });

            // 3. CHỨC NĂNG XÓA
            DeleteCommand = new RelayCommand(p =>
            {
                if (!IsEditMode)
                {
                    MessageBox.Show("Vui lòng chọn loại sản phẩm cần xóa trên danh sách!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var confirm = MessageBox.Show($"Bạn có chắc chắn muốn xóa loại sản phẩm: {TenLoai}?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (confirm == MessageBoxResult.Yes)
                {
                    try
                    {
                        var target = db.LoaiSanPhams.SingleOrDefault(x => x.MaLoai == MaLoai);
                        if (target != null)
                        {
                            string maXoa = target.MaLoai;
                            string tenXoa = target.TenLoai;

                            db.LoaiSanPhams.Remove(target);
                            db.SaveChanges();
                            MessageBox.Show("Xóa danh mục loại sản phẩm thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                            ThemLichSu("🗑️ XÓA BỎ", maXoa, tenXoa);
                            ExecuteSearch();
                            ResetForm();
                        }
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Không thể xóa loại sản phẩm này vì đang có sản phẩm thuộc danh mục này trong kho hàng!", "Lỗi ràng buộc dữ liệu", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            });
        }

        private void ExecuteSearch()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                ListLoaiSP = new ObservableCollection<LoaiSanPham>(db.LoaiSanPhams.ToList());
            }
            else
            {
                string key = SearchText.Trim().ToLower();
                ListLoaiSP = new ObservableCollection<LoaiSanPham>(
                    db.LoaiSanPhams.Where(x => x.TenLoai.ToLower().Contains(key)).ToList()
                );
            }
        }

        private void ResetForm()
        {
            MaLoai = string.Empty;
            TenLoai = string.Empty;
            IsEditMode = false;
            SelectedLoaiSP = null;
        }
    }
}