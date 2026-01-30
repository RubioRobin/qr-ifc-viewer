using System;
using System.Windows;
using System.Windows.Media;
using QrIfcPlugin.Models;
using QrIfcPlugin.Services;

namespace QrIfcPlugin.UI
{
    /// <summary>
    /// Settings window code-behind
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private PluginSettings _settings;

        public SettingsWindow()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            _settings = PluginSettings.Load();

            ApiBaseUrlTextBox.Text = _settings.ApiBaseUrl;
            ViewerBaseUrlTextBox.Text = _settings.ViewerBaseUrl;
            ProjectSlugTextBox.Text = _settings.ProjectSlug;
            DefaultModelVersionTextBox.Text = _settings.DefaultModelVersion;
            ExpiryDaysTextBox.Text = _settings.ExpiryDays.ToString();
            QrSizeTextBox.Text = _settings.QrSizeMm.ToString("F1");
            ShowLabelCheckBox.IsChecked = _settings.ShowLabel;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate and save settings
                _settings.ApiBaseUrl = ApiBaseUrlTextBox.Text.TrimEnd('/');
                _settings.ViewerBaseUrl = ViewerBaseUrlTextBox.Text.TrimEnd('/');
                _settings.ProjectSlug = ProjectSlugTextBox.Text.Trim();
                _settings.DefaultModelVersion = DefaultModelVersionTextBox.Text.Trim();

                if (!int.TryParse(ExpiryDaysTextBox.Text, out int expiryDays) || expiryDays < 1)
                {
                    MessageBox.Show(
                        "Verlooptijd moet een positief getal zijn.",
                        "Validatie Fout",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    return;
                }
                _settings.ExpiryDays = expiryDays;

                if (!double.TryParse(QrSizeTextBox.Text, out double qrSize) || qrSize < 10)
                {
                    MessageBox.Show(
                        "QR grootte moet minimaal 10mm zijn.",
                        "Validatie Fout",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    return;
                }
                _settings.QrSizeMm = qrSize;

                _settings.ShowLabel = ShowLabelCheckBox.IsChecked ?? true;

                // Validate required fields
                if (string.IsNullOrWhiteSpace(_settings.ApiBaseUrl))
                {
                    MessageBox.Show(
                        "API Base URL is verplicht.",
                        "Validatie Fout",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    return;
                }

                if (string.IsNullOrWhiteSpace(_settings.ProjectSlug))
                {
                    MessageBox.Show(
                        "Project Slug is verplicht.",
                        "Validatie Fout",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    return;
                }

                // Validate URLs
                if (!Uri.TryCreate(_settings.ApiBaseUrl, UriKind.Absolute, out _))
                {
                    MessageBox.Show(
                        "API Base URL is geen geldige URL.",
                        "Validatie Fout",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    return;
                }

                if (!Uri.TryCreate(_settings.ViewerBaseUrl, UriKind.Absolute, out _))
                {
                    MessageBox.Show(
                        "Viewer Base URL is geen geldige URL.",
                        "Validatie Fout",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    return;
                }

                // Save to disk
                _settings.Save();

                MessageBox.Show(
                    "Instellingen succesvol opgeslagen.",
                    "Opgeslagen",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Fout bij opslaan van instellingen:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private async void TestConnection_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ConnectionStatusText.Text = "Verbinding testen...";
                ConnectionStatusText.Foreground = new SolidColorBrush(Colors.Gray);

                // Create temporary settings for testing
                var testSettings = new PluginSettings
                {
                    ApiBaseUrl = ApiBaseUrlTextBox.Text.TrimEnd('/')
                };

                var apiService = new ApiService(testSettings);
                var isConnected = await apiService.TestConnectionAsync();

                if (isConnected)
                {
                    ConnectionStatusText.Text = "✓ Verbinding succesvol";
                    ConnectionStatusText.Foreground = new SolidColorBrush(Colors.Green);
                }
                else
                {
                    ConnectionStatusText.Text = "✗ Verbinding mislukt - controleer URL";
                    ConnectionStatusText.Foreground = new SolidColorBrush(Colors.Red);
                }
            }
            catch (Exception ex)
            {
                ConnectionStatusText.Text = $"✗ Fout: {ex.Message}";
                ConnectionStatusText.Foreground = new SolidColorBrush(Colors.Red);
            }
        }
    }
}
