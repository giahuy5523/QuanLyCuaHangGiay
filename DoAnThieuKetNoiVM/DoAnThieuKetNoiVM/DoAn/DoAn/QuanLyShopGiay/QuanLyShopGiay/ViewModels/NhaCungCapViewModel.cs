using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using QuanLyShopGiay.Models;
using QuanLyShopGiay.Command;

namespace QuanLyShopGiay.ViewModels
{
    public class NhaCungCapViewModel : BaseViewModel
    {
        private readonly QLShopGiayEntities _db;

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
                    IsMaNCCEnabled = false; // Chọn nhà cung cấp cũ thì KHÓA ô sửa mã lại
                }
            }
        }

        #region Input Form Properties
        private string _maNCC;
        public string MaNCC
        {
            get => _maNCC;
            set => SetProperty(ref _maNCC, value);
        }

        private string _tenNCC;
        public string TenNCC
        {
            get => _tenNCC;
            set => SetProperty(ref _tenNCC, value);
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

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set { if (SetProperty(ref _searchText, value)) LoadData(); }
        }

        private bool _isMaNCCEnabled = true;
        public bool IsMaNCCEnabled
        {
            get => _isMaNCCEnabled;
            set => SetProperty(ref _isMaNCCEnabled, value);
        }
        #endregion

        public ICommand AddCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand ClearCommand { get; set; }

        public NhaCungCapViewModel()
        {
            _db = new QLShopGiayEntities();
            LoadData();

            AddCommand = new RelayCommand(
                (p) => {
                    try
                    {
                        string targetMaNCC = MaNCC?.Trim();
                        string sdtInput = SDT?.Trim() ?? "";

                        // Bắt buộc kiểm tra định dạng độ dài 10-15 ký tự số trước khi đẩy về SQL tránh dính CHECK constraint
                        if (sdtInput.Length < 10 || sdtInput.Length > 15 || !sdtInput.All(char.IsDigit))
                        {
                            MessageBox.Show("Lỗi: Số điện thoại phải là chuỗi số có độ dài từ 10 đến 15 số!", "Sai định dạng", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        if (_db.NhaCungCap.Any(x => x.MaNCC == targetMaNCC))
                        {
                            MessageBox.Show("Mã đối tác cung cấp này đã có sẵn trên hệ thống!");
                            return;
                        }

                        var ncc = new NhaCungCap()
                        {
                            MaNCC = targetMaNCC,
                            TenNCC = TenNCC.Trim(),
                            SDT = sdtInput,
                            DiaChi = string.IsNullOrWhiteSpace(DiaChi) ? null : DiaChi.Trim()
                        };
                        _db.NhaCungCap.Add(ncc);
                        _db.SaveChanges();
                        LoadData();
                        ClearInputs();
                        MessageBox.Show("Đã thêm mới đối tác nhà cung cấp thành công!");
                    }
                    catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
                },
                (p) => IsMaNCCEnabled && !string.IsNullOrWhiteSpace(MaNCC) && !string.IsNullOrWhiteSpace(TenNCC) && !string.IsNullOrWhiteSpace(SDT)
            );

            SaveCommand = new RelayCommand(
                (p) => {
                    try
                    {
                        string sdtInput = SDT?.Trim() ?? "";
                        if (sdtInput.Length < 10 || sdtInput.Length > 15 || !sdtInput.All(char.IsDigit))
                        {
                            MessageBox.Show("Lỗi số điện thoại không hợp lệ (Phải từ 10-15 số).");
                            return;
                        }

                        var ncc = _db.NhaCungCap.FirstOrDefault(x => x.MaNCC == SelectedItem.MaNCC);
                        if (ncc != null)
                        {
                            ncc.TenNCC = TenNCC.Trim();
                            ncc.SDT = sdtInput;
                            ncc.DiaChi = string.IsNullOrWhiteSpace(DiaChi) ? null : DiaChi.Trim();
                            _db.SaveChanges();
                            LoadData();
                            ClearInputs();
                            MessageBox.Show("Lưu thay đổi thông tin nhà cung cấp thành công!");
                        }
                    }
                    catch (Exception ex) { MessageBox.Show("Lỗi hệ thống: " + ex.Message); }
                },
                (p) => SelectedItem != null && !string.IsNullOrWhiteSpace(TenNCC) && !string.IsNullOrWhiteSpace(SDT)
            );

            DeleteCommand = new RelayCommand(
                (p) => {
                    var confirm = MessageBox.Show($"Xác nhận xóa nhà cung cấp '{SelectedItem.TenNCC}'?", "Hỏi xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (confirm == MessageBoxResult.Yes)
                    {
                        string targetMaNCC = SelectedItem.MaNCC;

                        // Chống crash: Kiểm tra xem NCC này đã có Sản phẩm hoặc Hóa đơn nhập nào chưa trước khi xóa khỏi DB
                        bool hasRelatedData = _db.SanPham.Any(sp => sp.MaNCC == targetMaNCC) || _db.HoaDonNhap.Any(hdn => hdn.MaNCC == targetMaNCC);
                        if (hasRelatedData)
                        {
                            MessageBox.Show("Không cho phép xóa! Nhà cung cấp đã có dữ liệu hàng hóa hoặc hóa đơn nhập hàng tồn tại.", "Lỗi dữ liệu", MessageBoxButton.OK, MessageBoxImage.Stop);
                            return;
                        }

                        var ncc = _db.NhaCungCap.FirstOrDefault(x => x.MaNCC == targetMaNCC);
                        if (ncc != null)
                        {
                            _db.NhaCungCap.Remove(ncc);
                            _db.SaveChanges();
                            LoadData();
                            ClearInputs();
                            MessageBox.Show("Đã xóa nhà cung cấp.");
                        }
                    }
                },
                (p) => SelectedItem != null
            );

            ClearCommand = new RelayCommand((p) => ClearInputs(), (p) => true);
        }

        private void LoadData()
        {
            var query = _db.NhaCungCap.AsQueryable();
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string kw = SearchText.Trim().ToLower();
                query = query.Where(x => x.TenNCC.ToLower().Contains(kw) || x.MaNCC.ToLower().Contains(kw) || x.SDT.Contains(kw));
            }
            ListNhaCungCap = new ObservableCollection<NhaCungCap>(query.OrderBy(x => x.MaNCC).ToList());
        }

        private void ClearInputs()
        {
            _selectedItem = null; OnPropertyChanged(nameof(SelectedItem));
            MaNCC = TenNCC = SDT = DiaChi = string.Empty; IsMaNCCEnabled = true;
        }
    }
}