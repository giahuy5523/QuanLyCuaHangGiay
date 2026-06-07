using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using QuanLyShopGiay.Models;
using QuanLyShopGiay.Command;
using QuanLyShopGiay.Helpers;

namespace QuanLyShopGiay.ViewModels
{
    public class NhaCungCapViewModel : BaseViewModel
    {
        private readonly QLShopGiayEntities _db;

        // Dùng SessionManager.CurrentUser thay vì HoTenNV/TenDangNhap/TenVT
        public string TenTaiKhoan => SessionManager.CurrentUser?.TenNhanVien ?? "Chưa đăng nhập";
        public string TenVaiTro => SessionManager.CurrentUser?.Quyen ?? "N/A";

        private ObservableCollection<NhaCungCap> _listNhaCungCap;
        public ObservableCollection<NhaCungCap> ListNhaCungCap
        {
            get => _listNhaCungCap;
            set => SetProperty(ref _listNhaCungCap, value);
        }

        private NhaCungCap _selectedItem;
        public NhaCungCap SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (SetProperty(ref _selectedItem, value) && value != null)
                {
                    MaNCC = value.MaNCC;
                    TenNCC = value.TenNCC;
                    SDT = value.SDT;
                    DiaChi = value.DiaChi;
                    IsMaNCCEnabled = false;
                }
            }
        }

        private string _maNCC;
        public string MaNCC { get => _maNCC; set => SetProperty(ref _maNCC, value); }

        private string _tenNCC;
        public string TenNCC { get => _tenNCC; set => SetProperty(ref _tenNCC, value); }

        private string _sdt;
        public string SDT { get => _sdt; set => SetProperty(ref _sdt, value); }

        private string _diaChi;
        public string DiaChi { get => _diaChi; set => SetProperty(ref _diaChi, value); }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set { if (SetProperty(ref _searchText, value)) LoadData(); }
        }

        private bool _isMaNCCEnabled = true;
        public bool IsMaNCCEnabled { get => _isMaNCCEnabled; set => SetProperty(ref _isMaNCCEnabled, value); }

        public ICommand AddCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand ClearCommand { get; set; }

        public NhaCungCapViewModel()
        {
            try
            {
                _db = new QLShopGiayEntities();
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể kết nối cơ sở dữ liệu:\n" + ex.Message,
                    "Lỗi kết nối", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            AddCommand = new RelayCommand(
                (p) =>
                {
                    try
                    {
                        string targetMaNCC = MaNCC?.Trim();
                        string sdtInput = SDT?.Trim() ?? "";

                        if (string.IsNullOrWhiteSpace(targetMaNCC))
                        {
                            MessageBox.Show("Vui lòng nhập Mã Nhà Cung Cấp!", "Thiếu thông tin",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        if (sdtInput.Length < 10 || sdtInput.Length > 15 || !sdtInput.All(char.IsDigit))
                        {
                            MessageBox.Show("Số điện thoại phải là chuỗi số từ 10 đến 15 ký tự!", "Sai định dạng",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        if (_db.NhaCungCaps.Any(x => x.MaNCC == targetMaNCC))
                        {
                            MessageBox.Show("Mã nhà cung cấp này đã tồn tại!", "Trùng mã",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        _db.NhaCungCaps.Add(new NhaCungCap
                        {
                            MaNCC = targetMaNCC,
                            TenNCC = TenNCC.Trim(),
                            SDT = sdtInput,
                            DiaChi = string.IsNullOrWhiteSpace(DiaChi) ? null : DiaChi.Trim()
                        });
                        _db.SaveChanges();
                        LoadData();
                        ClearInputs();
                        MessageBox.Show("Thêm nhà cung cấp thành công!", "Thành công",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
                },
                (p) => IsMaNCCEnabled &&
                       !string.IsNullOrWhiteSpace(MaNCC) &&
                       !string.IsNullOrWhiteSpace(TenNCC) &&
                       !string.IsNullOrWhiteSpace(SDT)
            );

            SaveCommand = new RelayCommand(
                (p) =>
                {
                    try
                    {
                        string sdtInput = SDT?.Trim() ?? "";
                        if (sdtInput.Length < 10 || sdtInput.Length > 15 || !sdtInput.All(char.IsDigit))
                        {
                            MessageBox.Show("Số điện thoại không hợp lệ (10-15 ký tự số).", "Sai định dạng",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        var ncc = _db.NhaCungCaps.FirstOrDefault(x => x.MaNCC == SelectedItem.MaNCC);
                        if (ncc != null)
                        {
                            ncc.TenNCC = TenNCC.Trim();
                            ncc.SDT = sdtInput;
                            ncc.DiaChi = string.IsNullOrWhiteSpace(DiaChi) ? null : DiaChi.Trim();
                            _db.SaveChanges();
                            LoadData();
                            ClearInputs();
                            MessageBox.Show("Lưu thay đổi thành công!", "Thành công",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    catch (Exception ex) { MessageBox.Show("Lỗi hệ thống: " + ex.Message); }
                },
                (p) => SelectedItem != null &&
                       !string.IsNullOrWhiteSpace(TenNCC) &&
                       !string.IsNullOrWhiteSpace(SDT)
            );

            DeleteCommand = new RelayCommand(
                (p) =>
                {
                    if (MessageBox.Show($"Xác nhận xóa nhà cung cấp '{SelectedItem.TenNCC}'?",
                        "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        try
                        {
                            string targetMaNCC = SelectedItem.MaNCC;
                            if (_db.SanPhams.Any(sp => sp.MaNCC == targetMaNCC) ||
                                _db.HoaDonNhaps.Any(hdn => hdn.MaNCC == targetMaNCC))
                            {
                                MessageBox.Show("Không thể xóa! Nhà cung cấp đang có sản phẩm hoặc hóa đơn nhập liên quan.",
                                    "Không thể xóa", MessageBoxButton.OK, MessageBoxImage.Stop);
                                return;
                            }

                            var ncc = _db.NhaCungCaps.FirstOrDefault(x => x.MaNCC == targetMaNCC);
                            if (ncc != null)
                            {
                                _db.NhaCungCaps.Remove(ncc);
                                _db.SaveChanges();
                                LoadData();
                                ClearInputs();
                                MessageBox.Show("Đã xóa nhà cung cấp thành công.");
                            }
                        }
                        catch (Exception ex) { MessageBox.Show("Lỗi khi xóa: " + ex.Message); }
                    }
                },
                (p) => SelectedItem != null
            );

            ClearCommand = new RelayCommand((p) => ClearInputs(), (p) => true);
        }

        private void LoadData()
        {
            var query = _db.NhaCungCaps.AsQueryable();
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string kw = SearchText.Trim().ToLower();
                query = query.Where(x =>
                    x.TenNCC.ToLower().Contains(kw) ||
                    x.MaNCC.ToLower().Contains(kw) ||
                    x.SDT.Contains(kw));
            }
            ListNhaCungCap = new ObservableCollection<NhaCungCap>(
                query.OrderBy(x => x.MaNCC).ToList());
        }

        private void ClearInputs()
        {
            _selectedItem = null;
            OnPropertyChanged(nameof(SelectedItem));
            MaNCC = TenNCC = SDT = DiaChi = string.Empty;
            IsMaNCCEnabled = true;
        }
    }
}
