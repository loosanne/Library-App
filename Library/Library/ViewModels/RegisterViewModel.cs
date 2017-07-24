﻿using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Library
{
    class RegisterViewModel : ViewModelBase
    {
        private string _PasswordInVM;
        private string _username;
        private string _email;

        public RegisterViewModel()
        {
            SubmitCommand = new RelayCommand(Submit);
        }

        public ICommand SubmitCommand
        {
            get;
            private set;
        }

        public string UserName
        {
            get { return _username; }
            set
            {
                _username = value;
                OnPropertyChanged("UserName");
            }
        }

        public string Email
        {
            get { return _email; }
            set
            {
                _email = value;
                OnPropertyChanged("Email");
            }
        }

        public string PasswordInVM
        {
            get
            {
                return _PasswordInVM;
            }
            set
            {
                _PasswordInVM = value;
                OnPropertyChanged("PasswordInVM");
            }
        }

        /// <summary>
        /// Submiting a new account
        /// </summary>
        /// <param name="parameter"></param>
        private void Submit(object parameter)
        {
            var passwordContainer = parameter as IHavePassword;
            if (passwordContainer != null)
            {
                var secureString = passwordContainer.Password;
                PasswordInVM = ConvertToUnsecureString(secureString);
                PasswordInVM = sha256_hash(PasswordInVM);
            }

            SqlConnection connection = new SqlConnection(@"Data Source=localhost\SQLEXPRESS; Initial Catalog=LibraryDB; Integrated Security=True;");

            try
            {
                if (connection.State == ConnectionState.Closed)
                    connection.Open();

                String query = "SELECT COUNT(1) FROM user_tbl WHERE Login=@Login";

                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@Login", UserName);

                int count = Convert.ToInt32(cmd.ExecuteScalar());

                if (count == 1)
                {
                    MessageBox.Show("This login is already taken.");
                }
                else
                {
                    query = "insert into user_tbl (Login, Email, Password, TetrisScore, ReservedBook1, ReservedBook2, ReservedBook3, BorrowedBook1, BorrowedBook2, BorrowedBook3, BorrowedBook4, BorrowedBook5, Type) values(@Login, @Email, @Password, 0, 0, 0, 0, 0, 0, 0, 0, 0, 'Normal')";

                    cmd = new SqlCommand(query, connection);
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@Login", this.UserName);
                    cmd.Parameters.AddWithValue("@Email", this.Email);
                    cmd.Parameters.AddWithValue("@Password", this.PasswordInVM);

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Rejestracja przebiegła pomyślnie. Możesz się zalogować.");
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection.Close();
            }
        }

        private string ConvertToUnsecureString(SecureString securePassword)
        {
            if (securePassword == null)
            {
                return string.Empty;
            }

            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(securePassword);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }

        public static String sha256_hash(String value)
        {
            using (SHA256 hash = SHA256Managed.Create())
            {
                return String.Concat(hash
                  .ComputeHash(Encoding.UTF8.GetBytes(value))
                  .Select(item => item.ToString("x2")));
            }
        }
    }
}
