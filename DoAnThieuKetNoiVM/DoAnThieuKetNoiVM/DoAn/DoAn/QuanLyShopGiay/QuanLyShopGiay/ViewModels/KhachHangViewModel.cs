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
        private readonly QuanLyShopGiayEntities _db;

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

                    IsMaKHEnabled = true;
                }
            }
        }

        #region Thuộc tính Binding Ô nhập liệu và Trạng thái Form
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
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    LoadData();
                }
            }
        }

        private string _tongMuaText = "0 ₫";
        public string TongMuaText
        {
            get => _tongMuaText;
            set => SetProperty(ref _tongMuaText, value);
        }

        private string _hangHienTaiText = "Thành viên mới";
        public string HangHienTaiText
        {
            get => _hangHienTaiText;
            set => SetProperty(ref _hangHienTaiText, value);
        }
        #endregion

        public ICommand AddCommand { get; set; }     // Thêm mới
        public ICommand SaveCommand { get; set; }    // Lưu chỉnh sửa
        public ICommand DeleteCommand { get; set; }  // Xóa khách hàng
        public ICommand ClearCommand { get; set; }   // Làm mới form

        public KhachHangViewModel()
        {
            _db = new QuanLyShopGiayEntities();
            LoadData();

            // 1. NÚT "THÊM": THÊM MỚI KHÁCH HÀNG
            AddCommand = new RelayCommand(
                (p) => {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(MaKH))
                        {
                            MessageBox.Show("Vui lòng nhập Mã khách hàng!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        if (string.IsNullOrWhiteSpace(TenKH))
                        {
                            MessageBox.Show("Vui lòng nhập Tên khách hàng!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        string targetMaKH = MaKH.Trim();
                        string sdtInput = string.IsNullOrWhiteSpace(SDT) ? null : SDT.Trim();

                        if (_db.KHACH_HANG.Any(kh => kh.MaKH == targetMaKH))
                        {
                            MessageBox.Show($"Mã khách hàng '{targetMaKH}' đã tồn tại trong hệ thống!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        if (sdtInput != null && _db.KHACH_HANG.Any(kh => kh.SDT == sdtInput))
                        {
                            MessageBox.Show("Số điện thoại này đã được đăng ký bởi khách hàng khác!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        var newKH = new KHACH_HANG()
                        {
                            MaKH = targetMaKH,
                            TenKH = TenKH.Trim(),
                            SDT = sdtInput,
                            DiaChi = string.IsNullOrWhiteSpace(DiaChi) ? null : DiaChi.Trim(),
                            DiemTichLuy = 0
                        };

                        _db.KHACH_HANG.Add(newKH);
                        _db.SaveChanges();

                        LoadData();
                        ClearInputs();

                        MessageBox.Show("Thêm mới khách hàng thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi khi thêm dữ liệu: " + ex.Message, "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                },
                (p) => !string.IsNullOrWhiteSpace(MaKH) && !string.IsNullOrWhiteSpace(TenKH)
            );

            // 2. NÚT "LƯU": LƯU THAY ĐỔI CHỈNH SỬA KHÁCH HÀNG CŨ
            SaveCommand = new RelayCommand(
                (p) => {
                    try
                    {
                        if (SelectedItem == null)
                        {
                            MessageBox.Show("Vui lòng chọn khách hàng trên danh sách để chỉnh sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        string originalMaKH = SelectedItem.MaKH;
                        var khTarget = _db.KHACH_HANG.FirstOrDefault(kh => kh.MaKH == originalMaKH);

                        if (khTarget == null)
                        {
                            MessageBox.Show("Dữ liệu khách hàng không tồn tại hoặc đã bị xóa!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        string currentMaKHInput = MaKH.Trim();
                        string sdtInput = string.IsNullOrWhiteSpace(SDT) ? null : SDT.Trim();

                        if (originalMaKH != currentMaKHInput)
                        {
                            if (_db.KHACH_HANG.Any(kh => kh.MaKH == currentMaKHInput))
                            {
                                MessageBox.Show($"Mã khách hàng mới '{currentMaKHInput}' bị trùng lặp dữ liệu!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return;
                            }

                            var updatedKH = new KHACH_HANG()
                            {
                                MaKH = currentMaKHInput,
                                TenKH = TenKH.Trim(),
                                SDT = sdtInput,
                                DiaChi = string.IsNullOrWhiteSpace(DiaChi) ? null : DiaChi.Trim(),
                                DiemTichLuy = khTarget.DiemTichLuy
                            };

                            _db.KHACH_HANG.Add(updatedKH);
                            _db.KHACH_HANG.Remove(khTarget);
                        }
                        else
                        {
                            khTarget.TenKH = TenKH.Trim();
                            khTarget.SDT = sdtInput;
                            khTarget.DiaChi = string.IsNullOrWhiteSpace(DiaChi) ? null : DiaChi.Trim();
                        }

                        _db.SaveChanges();
                        LoadData();
                        ClearInputs();

                        MessageBox.Show("Lưu thay đổi thông tin khách hàng thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi khi thực hiện lưu chỉnh sửa: " + ex.Message, "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                },
                (p) => SelectedItem != null && !string.IsNullOrWhiteSpace(MaKH) && !string.IsNullOrWhiteSpace(TenKH)
            );

            // 3. NÚT "XÓA": XÓA KHÁCH HÀNG ĐANG CHỌN (CÓ KIỂM TRA RÀNG BUỘC HÓA ĐƠN)
            DeleteCommand = new RelayCommand(
                (p) => {
                    try
                    {
                        if (SelectedItem == null)
                        {
                            MessageBox.Show("Vui lòng chọn khách hàng cần xóa từ danh sách!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        // Hỏi xác nhận trước khi xóa thực sự
                        var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa khách hàng '{SelectedItem.TenKH}' ra khỏi hệ thống không?",
                                                     "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);

                        if (result == MessageBoxResult.Yes)
                        {
                            string targetMaKH = SelectedItem.MaKH;

                            // KIỂM TRA RÀNG BUỘC: Nếu khách hàng này đã có hóa đơn trong hệ thống thì KHÔNG ĐƯỢC XÓA
                            bool hasInvoices = _db.HOA_DON.Any(hd => hd.MaKH == targetMaKH);
                            if (hasInvoices)
                            {
                                MessageBox.Show("Không thể xóa khách hàng này vì lịch sử giao dịch chứa dữ liệu hóa đơn của họ!",
                                                "Không thể xóa", MessageBoxButton.OK, MessageBoxImage.Stop);
                                return;
                            }

                            var khTarget = _db.KHACH_HANG.FirstOrDefault(kh => kh.MaKH == targetMaKH);
                            if (khTarget != null)
                            {
                                _db.KHACH_HANG.Remove(khTarget);
                                _db.SaveChanges();

                                LoadData();
                                ClearInputs();
                                MessageBox.Show("Đã xóa khách hàng thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi hệ thống khi xóa: " + ex.Message, "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                },
                (p) => SelectedItem != null // Chỉ sáng nút xóa khi có một dòng được chọn trong DataGrid
            );

            // 4. LÀM MỚI FORM ĐỂ SẴN SÀNG NHẬP MỚI
            ClearCommand = new RelayCommand(
                (p) => { ClearInputs(); },
                (p) => true
            );
        }

        private void LoadData()
        {
            try
            {
                var query = _db.KHACH_HANG.AsQueryable();

                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    string keyword = SearchText.Trim().ToLower();
                    query = query.Where(kh => kh.TenKH.ToLower().Contains(keyword)
                                           || kh.MaKH.ToLower().Contains(keyword)
                                           || (!string.IsNullOrEmpty(kh.SDT) && kh.SDT.Contains(keyword)));
                }

                var rawList = query.ToList();

                var displayList = rawList.Select(kh => {
                    decimal tongChiTieu = _db.HOA_DON
                        .Where(hd => hd.MaKH == kh.MaKH && hd.TrangThai != "Đã hủy")
                        .Sum(hd => (decimal?)hd.TongTien) ?? 0;

                    return new KhachHangDisplayModel
                    {
                        MaKH = kh.MaKH,
                        TenKH = kh.TenKH,
                        SDT = kh.SDT,
                        DiaChi = kh.DiaChi,
                        TongChiTieu = tongChiTieu,
                        HangThanhVien = GetRankName(tongChiTieu)
                    };
                }).OrderBy(x => x.MaKH).ToList();

                ListKhachHang = new ObservableCollection<KhachHangDisplayModel>(displayList);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi nạp danh sách dữ liệu: " + ex.Message, "Lỗi kết nối", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
            _selectedItem = null;
            OnPropertyChanged(nameof(SelectedItem));

            MaKH = string.Empty;
            TenKH = string.Empty;
            SDT = string.Empty;
            DiaChi = string.Empty;
            TongMuaText = "0 ₫";
            HangHienTaiText = "Thành viên mới 🌱";
            IsMaKHEnabled = true;
        }
    }
}