using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyShopGiay.Helpers
{
    public static class PasswordHelper
    {
        public static byte[] HashPassword(string password, Guid salt)
        {
            using (SHA512 sha512 = SHA512.Create())
            {
                // Thêm .ToUpper() vào đây để khớp với cách SQL Server CAST Guid sang Varchar
                string saltAndPwd = salt.ToString().ToUpper() + password;

                byte[] inputBytes = Encoding.UTF8.GetBytes(saltAndPwd);
                return sha512.ComputeHash(inputBytes);
            }
        }
    }
}
