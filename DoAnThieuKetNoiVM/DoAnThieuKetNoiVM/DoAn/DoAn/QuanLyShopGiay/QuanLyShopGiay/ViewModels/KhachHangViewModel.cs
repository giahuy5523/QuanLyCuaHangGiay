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
        public string MaKH { get; set; }
        public string TenKH { get; set; }
        public string SDT { get; set; }
        public string DiaChi { get; set; }
        public decimal TongChiTieu { get; set; }
        public string HangThanhVien { get; set; }
    }

    public class KhachHangViewModel : BaseViewModel
    {
        private readonly QLShopGiayEntities _db; // Khớp chuẩn xác tên DbContext của bạn

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
                    MaKH = value.MaKH;
                    TenKH = value.TenKH;
                    SDT = value.SDT;
                    DiaChi = value.DiaChi;

                    TongMuaText = string.Format("{0:N0} ₫", value.TongChiTieu);
                    HangHienTaiText = value.HangThanhVien;

                    IsMaKHEnabled = false; // Khi chọn khách hàng cũ từ bảng, KHÓA ô nhập mã lại để tránh lỗi Primary Key
                }
            }
        }

        #region Properties Binding
        private string _maKH;
        public string MaKH
        {
            get => _maKH;
            set => SetProperty(ref _maKH, value);
        }

        private string _tenKH;
        public string TenKH
        {
            get => _tenKH;
            set => SetProperty(ref _tenKH, value);
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

        private bool _isMaKHEnabled = true;
        public bool IsMaKHEnabled
        {
            get => _isMaKHEnabled;
            set => SetProperty(ref _isMaKHEnabled, value);
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set { if (SetProperty(ref _searchText, value)) LoadData(); }
        }

        private string _tongMuaText = "0 ₫";
        public string TongMuaText
        {
            get => _tongMuaText;
            set => SetProperty(ref _tongMuaText, value);
        }

        private string _hangHienTaiText = "Thành viên mới 🌱";
        public string HangHienTaiText
        {
            get => _hangHienTaiText;
            set => SetProperty(ref _hangHienTaiText, value);
        }
        #endregion

        public ICommand AddCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand ClearCommand { get; set; }
        public ICommand HistoryCommand { get; set; }

        public KhachHangViewModel()
        {
            _db = new QLShopGiayEntities();
            LoadData();

            AddCommand = new RelayCommand(
                (p) => {
                    try
                    {
                        string targetMaKH = MaKH?.Trim();
                        string sdtInput = string.IsNullOrWhiteSpace(SDT) ? null : SDT.Trim();

                        // Kiểm tra ràng buộc định dạng CHECK số điện thoại từ SQL
                        if (sdtInput != null && (sdtInput.Length < 10 || sdtInput.Length > 15 || !sdtInput.All(char.IsDigit)))
                        {
                            MessageBox.Show("Số điện thoại không hợp lệ! Phải là chữ số có độ dài từ 10 đến 15 ký tự.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        if (_db.KhachHang.Any(kh => kh.MaKH == targetMaKH))
                        {
                            MessageBox.Show($"Mã khách hàng '{targetMaKH}' đã tồn tại!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        var newKH = new KhachHang()
                        {
                            MaKH = targetMaKH,
                            TenKH = TenKH.Trim(),
                            SDT = sdtInput,
                            DiaChi = string.IsNullOrWhiteSpace(DiaChi) ? null : DiaChi.Trim()
                        };

                        _db.KhachHang.Add(newKH);
                        _db.SaveChanges();

                        LoadData();
                        ClearInputs();
                        MessageBox.Show("Thêm mới khách hàng thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
                },
                (p) => IsMaKHEnabled && !string.IsNullOrWhiteSpace(MaKH) && !string.IsNullOrWhiteSpace(TenKH)
            );

            SaveCommand = new RelayCommand(
                (p) => {
                    try
                    {
                        var khTarget = _db.KhachHang.FirstOrDefault(kh => kh.MaKH == SelectedItem.MaKH);
                        if (khTarget != null)
                        {
                            string sdtInput = string.IsNullOrWhiteSpace(SDT) ? null : SDT.Trim();
                            if (sdtInput != null && (sdtInput.Length < 10 || sdtInput.Length > 15 || !sdtInput.All(char.IsDigit)))
                            {
                                MessageBox.Show("Số điện thoại không hợp lệ (Phải từ 10-15 ký tự số).");
                                return;
                            }

                            khTarget.TenKH = TenKH.Trim();
                            khTarget.SDT = sdtInput;
                            khTarget.DiaChi = string.IsNullOrWhiteSpace(DiaChi) ? null : DiaChi.Trim();

                            _db.SaveChanges();
                            LoadData();
                            ClearInputs();
                            MessageBox.Show("Cập nhật thông tin khách hàng thành công!");
                        }
                    }
                    catch (Exception ex) { MessageBox.Show("Lỗi cập nhật: " + ex.Message); }
                },
                (p) => SelectedItem != null && !string.IsNullOrWhiteSpace(TenKH)
            );

            DeleteCommand = new RelayCommand(
                (p) => {
                    try
                    {
                        var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa khách hàng '{SelectedItem.TenKH}'?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (result == MessageBoxResult.Yes)
                        {
                            string targetMaKH = SelectedItem.MaKH;

                            // Kiểm tra ràng buộc khóa ngoại thực tế từ file SQL của bảng HoaDon
                            if (_db.HoaDon.Any(hd => hd.MaKH == targetMaKH))
                            {
                                MessageBox.Show("Không thể xóa khách hàng này vì lịch sử hệ thống đang lưu giữ hóa đơn giao dịch của họ!", "Ràng buộc dữ liệu", MessageBoxButton.OK, MessageBoxImage.Stop);
                                return;
                            }

                            var khTarget = _db.KhachHang.FirstOrDefault(kh => kh.MaKH == targetMaKH);
                            if (khTarget != null)
                            {
                                _db.KhachHang.Remove(khTarget);
                                _db.SaveChanges();
                                LoadData();
                                ClearInputs();
                                MessageBox.Show("Xóa dữ liệu khách hàng thành công!");
                            }
                        }
                    }
                    catch (Exception ex) { MessageBox.Show("Lỗi hệ thống: " + ex.Message); }
                },
                (p) => SelectedItem != null
            );

            ClearCommand = new RelayCommand((p) => { ClearInputs(); }, (p) => true);

            HistoryCommand = new RelayCommand(
                (p) => MessageBox.Show($"Đang kết nối xem lịch sử giao dịch hóa đơn của: {SelectedItem?.TenKH}"),
                (p) => SelectedItem != null
            );
        }

        private void LoadData()
        {
            try
            {
                var query = _db.KhachHang.AsQueryable();
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    string keyword = SearchText.Trim().ToLower();
                    query = query.Where(kh => kh.TenKH.ToLower().Contains(keyword) || kh.MaKH.ToLower().Contains(keyword) || (kh.SDT != null && kh.SDT.Contains(keyword)));
                }

                ListKhachHang = new ObservableCollection<KhachHangDisplayModel>(query.ToList().Select(kh => {
                    decimal tongChiTieu = _db.HoaDon.Where(hd => hd.MaKH == kh.MaKH && hd.TrangThai == N"Đã thanh toán").Sum(hd => (decimal?)hd.TongTien) ?? 0;
                    return new KhachHangDisplayModel
                    {
                        MaKH = kh.MaKH,
                        TenKH = kh.TenKH,
                        SDT = kh.SDT,
                        DiaChi = kh.DiaChi,
                        TongChiTieu = tongChiTieu,
                        HangThanhVien = GetRankName(tongChiTieu)
                    };
                }).OrderBy(x => x.MaKH).ToList());
            }
            catch (Exception ex) { MessageBox.Show("Lỗi nạp dữ liệu: " + ex.Message); }
        }

        private static string GetRankName(decimal totalAmount)
        {
            if (totalAmount >= 30000000) return "Kim cương 💎";
            if (totalAmount >= 15000000) return "Vàng 🥇";
            if (totalAmount >= 5000000) return "Bạc 🥈";
            return "Thành viên mới 🌱";
        }

        private void ClearInputs()
        {
            _selectedItem = null; OnPropertyChanged(nameof(SelectedItem));
            MaKH = TenKH = SDT = DiaChi = string.Empty;
            TongMuaText = "0 ₫"; HangHienTaiText = "Thành viên mới 🌱"; IsMaKHEnabled = true;
        }
    }
}