
using Serilog;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Geonorge.MassivNedlasting.Gui
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class SettingsDialog
    {
        private AppSettings _appSettings;
        private List<string> _configFiles;
        private static readonly HttpClient Client = new HttpClient() { Timeout = TimeSpan.FromMilliseconds(30000) };

        public SettingsDialog()
        {
            _appSettings = ApplicationService.GetAppSettings();

            InitializeComponent();

            txtUsername.Text = _appSettings.Username;
            txtPassword.Password = ProtectionService.GetUnprotectedPassword(_appSettings.Password);
            ConfigFilesList.ItemsSource = ApplicationService.NameConfigFiles();
        }
        

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            _appSettings.Password = ProtectionService.CreateProtectedPassword(txtPassword.Password);
            _appSettings.Username = txtUsername.Text;

            if(!string.IsNullOrEmpty(_appSettings.Username) && !string.IsNullOrEmpty(_appSettings.Password))
            {
                var urlValidateUser = "https://nedlasting.geonorge.no/api/download/validate-user";

                var byteArray = Encoding.ASCII.GetBytes(_appSettings.Username + ":" + ProtectionService.GetUnprotectedPassword(_appSettings.Password));
                Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                using (var response = Client.GetAsync(urlValidateUser, HttpCompletionOption.ResponseHeadersRead))
                {

                    if (!response.Result.IsSuccessStatusCode && response.Result.StatusCode != System.Net.HttpStatusCode.NotFound)
                    {
                        var message = response.Result.Content.ReadAsStringAsync().Result;
                        Log.Error(message);
                        Console.WriteLine(message);
                        MessageBox.Show(message);
                    }
                    else { 
                        ApplicationService.WriteToAppSettingsFile(_appSettings); 
                        Close();
                    }

                }
            }
            else
            {
                ApplicationService.WriteToAppSettingsFile(_appSettings);
                Close();
            }
        }

        private void ButtonEditConfig_OnClick(object sender, RoutedEventArgs e)
        {
            var configDialog = new ConfigDialog();
            configDialog.ShowDialog();
            ConfigFilesList.ItemsSource = ApplicationService.NameConfigFiles();
            _appSettings = ApplicationService.GetAppSettings();
        }
    }

}
