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
                if (SetProperty(ref _giaNhap, value))
                {
                    if (value.HasValue)
                    {
                        GiaBan = value.Value * 1.1m;
                    }
                    else
                    {
                        GiaBan = 0;
                    }
                }
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
                execute: p => {
                    if (string.IsNullOrWhiteSpace(MaSP) || string.IsNullOrWhiteSpace(TenSP) || SelectedLoai == null || SelectedNCC == null)
                    {
                        MessageBox.Show("Vui lòng điền các thông tin bắt buộc có dấu (*) !", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    if (db.SanPhams.Any(x => x.MaSP == MaSP.Trim()))
                    {
                        MessageBox.Show("Mã sản phẩm này đã tồn tại trong cơ sở dữ liệu!", "Lỗi trùng mã", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if ((GiaBan ?? 0) < (GiaNhap ?? 0))
                    {
                        MessageBox.Show("Giá bán ra không được phép nhỏ hơn giá nhập vào kho!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var newSP = new SanPham()
                    {
                        MaSP = MaSP.Trim(),
                        TenSP = TenSP.Trim(),
                        MaLoai = SelectedLoai.MaLoai,
                        MaNCC = SelectedNCC.MaNCC,
                        Size = Size?.Trim(),
                        MauSac = MauSac?.Trim(),
                        GiaNhap = GiaNhap ?? 0m,
                        GiaBan = GiaBan ?? 0m,
                        SoLuongTon = SoLuongTon ?? 0,
                        GhiChu = ""
                    };

                    db.SanPhams.Add(newSP);
                    db.SaveChanges();

                    MessageBox.Show("Thêm mới sản phẩm thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                    ExecuteSearch();
                    ResetForm();
                },
                canExecute: p => true
            );

            // 2. CẬP NHẬT SẢN PHẨM (ĐÃ SỬA ĐỔI TOÀN DIỆN KIỂU DỮ LIỆU)
            EditCommand = new RelayCommand(
                execute: p => {
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
                execute: p => {
                    if (!IsEditMode)
                    {
                        MessageBox.Show("Vui lòng chọn sản phẩm cần xóa từ bảng danh sách bên phải!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var confirm = MessageBox.Show($"Bạn có chắc chắn muốn xóa sản phẩm mã {MaSP} không?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (confirm == MessageBoxResult.Yes)
                    {
                        try
                        {
                            var itemToRemove = db.SanPhams.SingleOrDefault(x => x.MaSP == MaSP);
                            if (itemToRemove != null)
                            {
                                db.SanPhams.Remove(itemToRemove);
                                db.SaveChanges();
                                MessageBox.Show("Xóa sản phẩm thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                                ExecuteSearch();
                                ResetForm();
                            }
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("Không thể xóa sản phẩm này vì mã sản phẩm đã phát sinh lịch sử mua bán trong các Hóa Đơn!", "Lỗi ràng buộc dữ liệu", MessageBoxButton.OK, MessageBoxImage.Error);
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
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                ListSanPham = new ObservableCollection<SanPham>(
                    db.SanPhams.Include(x => x.LoaiSanPham).Include(x => x.NhaCungCap).ToList()
                );
            }
            else
            {
                string key = SearchText.Trim().ToLower();
                ListSanPham = new ObservableCollection<SanPham>(
                    db.SanPhams.Include(x => x.LoaiSanPham).Include(x => x.NhaCungCap)
                               .Where(x => x.TenSP.ToLower().Contains(key)).ToList()
                );
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