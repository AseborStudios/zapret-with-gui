using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Windows.Forms;
using System.Drawing;

// Псевдонимы для избежания конфликтов
using MessageBox = System.Windows.MessageBox;
using Application = System.Windows.Application;
using ContextMenuStrip = System.Windows.Forms.ContextMenuStrip;
using NotifyIcon = System.Windows.Forms.NotifyIcon;
using Button = System.Windows.Controls.Button;
using TextBlock = System.Windows.Controls.TextBlock;
using ComboBoxItem = System.Windows.Controls.ComboBoxItem;
using ColorDialog = System.Windows.Forms.ColorDialog;

namespace ZapretGUI
{
    public partial class MainWindow : Window
    {
        private string _repoPath = @"F:\zapret-gui\zapret-with-gui";
        private bool _isRunning = false;
        private StrategyProfile _selectedProfile = null;
        private string _currentListPath = "";

        private NotifyIcon _notifyIcon;
        private bool _isExit = false;

        private SolidColorBrush _colorGray = new SolidColorBrush(System.Windows.Media.Color.FromRgb(80, 80, 80));
        private SolidColorBrush _colorBlue = new SolidColorBrush(System.Windows.Media.Color.FromRgb(33, 150, 243));
        private SolidColorBrush _colorGreen = new SolidColorBrush(System.Windows.Media.Color.FromRgb(76, 175, 80));
        private SolidColorBrush _colorRed = new SolidColorBrush(System.Windows.Media.Color.FromRgb(244, 67, 54));

        public List<StrategyProfile> AllProfiles = new List<StrategyProfile>();

        // Хранилище для цветов
        private string _bgColor = "#121212";
        private string _sidebarColor = "#1E1E1E";
        private string _profileTextColor = "#4CAF50";
        private string _logTextColor = "#00FF00";

        public MainWindow()
        {
            InitializeComponent();

            SetupTray();
            BtnMain.Click += BtnMain_Click;
            BtnMain.Background = _colorGray;

            LoadStrategies();
            LoadSettings();
            CheckIfRunning();

            AddLog("Приложение запущено.");
        }

        // === ПРИМЕНЕНИЕ ЦВЕТОВ ===
        private void ApplyColors()
        {
            try
            {
                // 1. Фон окна
                if (!string.IsNullOrEmpty(_bgColor))
                    this.Background = (SolidColorBrush)new BrushConverter().ConvertFromString(_bgColor);

                // 2. Фон бокового меню
                if (!string.IsNullOrEmpty(_sidebarColor))
                    Sidebar.Background = (SolidColorBrush)new BrushConverter().ConvertFromString(_sidebarColor);

                // 3. Текст в профилях
                if (!string.IsNullOrEmpty(_profileTextColor))
                {
                    var brush = (SolidColorBrush)new BrushConverter().ConvertFromString(_profileTextColor);
                    TxtCurrentProfile.Foreground = brush;
                }

                // 4. Текст логов
                if (!string.IsNullOrEmpty(_logTextColor))
                    TxtLogs.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString(_logTextColor);
            }
            catch { }
        }

        // === ВЫБОР ЦВЕТОВ ===
        private void BtnPickBgColor_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new ColorDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    _bgColor = $"#{dialog.Color.R:X2}{dialog.Color.G:X2}{dialog.Color.B:X2}";
                    ApplyColors();
                    SaveSettings(); // Сохраняем СРАЗУ
                }
            }
        }

        private void BtnPickSidebarColor_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new ColorDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    _sidebarColor = $"#{dialog.Color.R:X2}{dialog.Color.G:X2}{dialog.Color.B:X2}";
                    ApplyColors();
                    SaveSettings(); // Сохраняем СРАЗУ
                }
            }
        }

        private void BtnPickProfileTextColor_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new ColorDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    _profileTextColor = $"#{dialog.Color.R:X2}{dialog.Color.G:X2}{dialog.Color.B:X2}";
                    ApplyColors();
                    SaveSettings(); // Сохраняем СРАЗУ
                }
            }
        }

        private void BtnPickLogTextColor_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new ColorDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    _logTextColor = $"#{dialog.Color.R:X2}{dialog.Color.G:X2}{dialog.Color.B:X2}";
                    ApplyColors();
                    SaveSettings(); // Сохраняем СРАЗУ
                }
            }
        }

        // === ТРЕЙ ===
        private void SetupTray()
        {
            _notifyIcon = new NotifyIcon();

            // Пытаемся загрузить нашу иконку, если нет — ставим стандартную
            string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.ico");
            if (File.Exists(iconPath))
                _notifyIcon.Icon = new System.Drawing.Icon(iconPath);
            else
                _notifyIcon.Icon = SystemIcons.Application;

            _notifyIcon.Text = "Zapret GUI";
            _notifyIcon.Visible = true;

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Показать", null, (s, e) => { ShowWindow(); });
            contextMenu.Items.Add("Выход", null, (s, e) => { _isExit = true; SaveSettings(); Close(); });
            _notifyIcon.ContextMenuStrip = contextMenu;

            _notifyIcon.DoubleClick += (s, e) => { ShowWindow(); };
        }

        private void ShowWindow()
        {
            this.Show();
            this.WindowState = WindowState.Normal;
            this.Activate();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_isExit)
            {
                e.Cancel = true;
                this.Hide();
                SaveSettings(); // Сохраняем при сворачивании
                AddLog("Свернуто в трей.");
            }
            else
            {
                _notifyIcon.Dispose();
            }
        }

        // === ПЕРЕКЛЮЧЕНИЕ СТРАНИЦ ===
        // (Код переключения страниц остался прежним, я его не менял, чтобы не раздувать ответ)
        private void MenuMain_Click(object sender, MouseButtonEventArgs e)
        {
            PageMain.Visibility = Visibility.Visible;
            PageProfiles.Visibility = Visibility.Collapsed;
            PageLists.Visibility = Visibility.Collapsed;
            PageSettings.Visibility = Visibility.Collapsed;
            PageLogs.Visibility = Visibility.Collapsed;

            MenuMain.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(76, 175, 80));
            MenuProfiles.Foreground = System.Windows.Media.Brushes.White;
            MenuLists.Foreground = System.Windows.Media.Brushes.White;
            MenuSettings.Foreground = System.Windows.Media.Brushes.White;
            MenuLogs.Foreground = System.Windows.Media.Brushes.White;
        }

        private void MenuProfiles_Click(object sender, MouseButtonEventArgs e)
        {
            PageMain.Visibility = Visibility.Collapsed;
            PageProfiles.Visibility = Visibility.Visible;
            PageLists.Visibility = Visibility.Collapsed;
            PageSettings.Visibility = Visibility.Collapsed;
            PageLogs.Visibility = Visibility.Collapsed;

            MenuMain.Foreground = System.Windows.Media.Brushes.White;
            MenuProfiles.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(76, 175, 80));
            MenuLists.Foreground = System.Windows.Media.Brushes.White;
            MenuSettings.Foreground = System.Windows.Media.Brushes.White;
            MenuLogs.Foreground = System.Windows.Media.Brushes.White;
        }

        private void MenuLists_Click(object sender, MouseButtonEventArgs e)
        {
            PageMain.Visibility = Visibility.Collapsed;
            PageProfiles.Visibility = Visibility.Collapsed;
            PageLists.Visibility = Visibility.Visible;
            PageSettings.Visibility = Visibility.Collapsed;
            PageLogs.Visibility = Visibility.Collapsed;

            MenuMain.Foreground = System.Windows.Media.Brushes.White;
            MenuProfiles.Foreground = System.Windows.Media.Brushes.White;
            MenuLists.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(76, 175, 80));
            MenuSettings.Foreground = System.Windows.Media.Brushes.White;
            MenuLogs.Foreground = System.Windows.Media.Brushes.White;
        }

        private void MenuSettings_Click(object sender, MouseButtonEventArgs e)
        {
            PageMain.Visibility = Visibility.Collapsed;
            PageProfiles.Visibility = Visibility.Collapsed;
            PageLists.Visibility = Visibility.Collapsed;
            PageSettings.Visibility = Visibility.Visible;
            PageLogs.Visibility = Visibility.Collapsed;

            MenuMain.Foreground = System.Windows.Media.Brushes.White;
            MenuProfiles.Foreground = System.Windows.Media.Brushes.White;
            MenuLists.Foreground = System.Windows.Media.Brushes.White;
            MenuSettings.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(76, 175, 80));
            MenuLogs.Foreground = System.Windows.Media.Brushes.White;
        }

        private void MenuLogs_Click(object sender, MouseButtonEventArgs e)
        {
            PageMain.Visibility = Visibility.Collapsed;
            PageProfiles.Visibility = Visibility.Collapsed;
            PageLists.Visibility = Visibility.Collapsed;
            PageSettings.Visibility = Visibility.Collapsed;
            PageLogs.Visibility = Visibility.Visible;

            MenuMain.Foreground = System.Windows.Media.Brushes.White;
            MenuProfiles.Foreground = System.Windows.Media.Brushes.White;
            MenuLists.Foreground = System.Windows.Media.Brushes.White;
            MenuSettings.Foreground = System.Windows.Media.Brushes.White;
            MenuLogs.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(76, 175, 80));
        }

        // === ЛОГИ ===
        private void AddLog(string message)
        {
            string time = DateTime.Now.ToString("HH:mm:ss");
            TxtLogs.Text += $"\n[{time}] {message}";
            TxtLogs.ScrollToEnd();
        }

        // === ИЗБРАННОЕ ===
        private void BtnFav_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var profile = btn.DataContext as StrategyProfile;

            if (profile != null)
            {
                profile.IsFavorite = !profile.IsFavorite;
                LstStrategies.SelectedItem = profile;
                SaveSettings(); // Сохраняем СРАЗУ
                RefreshProfilesList(profile.FileName);
            }
        }

        private void RefreshProfilesList(string selectedFileName = null)
        {
            var sorted = AllProfiles
                .OrderByDescending(p => p.IsFavorite)
                .ThenBy(p => p.Name)
                .ToList();

            LstStrategies.ItemsSource = sorted;

            if (selectedFileName != null)
            {
                var toSelect = sorted.FirstOrDefault(p => p.FileName == selectedFileName);
                if (toSelect != null) LstStrategies.SelectedItem = toSelect;
            }
        }

        // === СОХРАНЕНИЕ И ЗАГРУЗКА ===
        private void SaveSettings()
        {
            try
            {
                string settingsPath = Path.Combine(_repoPath, "settings.json");
                string favs = string.Join(",", AllProfiles.Where(p => p.IsFavorite).Select(p => p.FileName));

                string profileName = "";
                if (LstStrategies.SelectedItem is StrategyProfile currentProfile) profileName = currentProfile.FileName;

                string gameFilter = (CmbGameFilter.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "disabled";

                string json = $"{{\n  \"Profile\": \"{profileName}\",\n  \"GameFilter\": \"{gameFilter}\",\n  \"Favorites\": \"{favs}\",\n  \"BgColor\": \"{_bgColor}\",\n  \"SidebarColor\": \"{_sidebarColor}\",\n  \"ProfileTextColor\": \"{_profileTextColor}\",\n  \"LogTextColor\": \"{_logTextColor}\"\n}}";
                File.WriteAllText(settingsPath, json);
            }
            catch (Exception ex)
            {
                AddLog("Ошибка сохранения настроек: " + ex.Message);
            }
        }

        private void LoadSettings()
        {
            try
            {
                string settingsPath = Path.Combine(_repoPath, "settings.json");
                if (File.Exists(settingsPath))
                {
                    string json = File.ReadAllText(settingsPath);

                    if (json.Contains("\"BgColor\":")) _bgColor = ParseString(json, "BgColor");
                    if (json.Contains("\"SidebarColor\":")) _sidebarColor = ParseString(json, "SidebarColor");
                    if (json.Contains("\"ProfileTextColor\":")) _profileTextColor = ParseString(json, "ProfileTextColor");
                    if (json.Contains("\"LogTextColor\":")) _logTextColor = ParseString(json, "LogTextColor");

                    ApplyColors();

                    if (json.Contains("\"Favorites\":"))
                    {
                        string favsStr = ParseString(json, "Favorites");
                        if (!string.IsNullOrEmpty(favsStr))
                        {
                            string[] favNames = favsStr.Split(',');
                            foreach (var p in AllProfiles)
                            {
                                if (favNames.Contains(p.FileName)) p.IsFavorite = true;
                            }
                        }
                    }

                    RefreshProfilesList();

                    if (json.Contains("\"Profile\":"))
                    {
                        string profileName = ParseString(json, "Profile");
                        if (!string.IsNullOrEmpty(profileName))
                        {
                            var toSelect = AllProfiles.FirstOrDefault(p => p.FileName == profileName);
                            if (toSelect != null)
                            {
                                LstStrategies.SelectedItem = toSelect;
                                TxtCurrentProfile.Text = $"Нынешний профиль: {toSelect.Name}";
                            }
                        }
                    }

                    if (json.Contains("\"GameFilter\":"))
                    {
                        string gameFilter = ParseString(json, "GameFilter");

                        CmbGameFilter.SelectionChanged -= CmbGameFilter_SelectionChanged;

                        if (gameFilter == "disabled") CmbGameFilter.SelectedIndex = 0;
                        else if (gameFilter == "all") CmbGameFilter.SelectedIndex = 1;
                        else if (gameFilter == "tcp") CmbGameFilter.SelectedIndex = 2;
                        else if (gameFilter == "udp") CmbGameFilter.SelectedIndex = 3;

                        CmbGameFilter.SelectionChanged += CmbGameFilter_SelectionChanged;
                    }
                }
            }
            catch { }
        }

        private string ParseString(string json, string key)
        {
            try
            {
                int start = json.IndexOf($"\"{key}\": \"") + key.Length + 5;
                int end = json.IndexOf("\"", start);
                return json.Substring(start, end - start);
            }
            catch { return ""; }
        }

        // === ПРОФИЛИ ===
        public class StrategyProfile : System.ComponentModel.INotifyPropertyChanged
        {
            public string Name { get; set; } = "";
            public string Description { get; set; } = "";
            public string FileName { get; set; } = "";

            private bool _isFavorite;
            public bool IsFavorite
            {
                get => _isFavorite;
                set
                {
                    _isFavorite = value;
                    PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(IsFavorite)));
                }
            }

            public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        }

        private void LoadStrategies()
        {
            try
            {
                if (!Directory.Exists(_repoPath)) return;

                string[] files = Directory.GetFiles(_repoPath, "general*.bat");
                AllProfiles.Clear();

                foreach (var file in files)
                {
                    string fileName = Path.GetFileName(file);
                    StrategyProfile profile = new StrategyProfile { FileName = fileName, Name = fileName };

                    if (fileName == "general.bat") profile.Description = "Базовая стратегия (Fake + Multisplit).";
                    else if (fileName == "general (ALT).bat") profile.Description = "Для провайдеров с блокировкой TLS (Fake, fakedsplit).";
                    else if (fileName == "general (ALT2).bat") profile.Description = "Мультисплит с seqovl=652. Хорошо обходит Youtube.";
                    else if (fileName == "general (ALT3).bat") profile.Description = "Hostfakesplit. Меняет SNI на ya.ru и google.com.";
                    else if (fileName == "general (ALT4).bat") profile.Description = "Fake + Multisplit с badseq. Обход через неверные пакеты.";
                    else if (fileName == "general (ALT5).bat") profile.Description = "Syndata + Multidisorder. Агрессивный метод (НЕ РЕКОМЕНДУЕТСЯ).";
                    else if (fileName == "general (ALT6).bat") profile.Description = "Multisplit с seqovl=681. Для некоторых провайдеров.";
                    else if (fileName == "general (ALT7).bat") profile.Description = "Multisplit с sniext+1 (сдвиг SNI). Очень эффективен.";
                    else if (fileName == "general (ALT8).bat") profile.Description = "Fake + TLS mod=none. Для простых блокировок.";
                    else if (fileName == "general (ALT9).bat") profile.Description = "Hostfakesplit (host=ozon.ru). Обход через подмену хоста.";
                    else if (fileName == "general (ALT10).bat") profile.Description = "Fake с фейками от 4pda и sferum. Обычно хорошо работает.";
                    else if (fileName == "general (ALT11).bat") profile.Description = "Fake + Multisplit с увеличенным повтором пакетов.";
                    else if (fileName == "general (ALT12).bat") profile.Description = "Fake + Multisplit с seqovl=664 (sferum).";
                    else if (fileName == "general (ALT13).bat") profile.Description = "Hostfakesplit (host=mail.ru, sochi_park). Для юга России.";
                    else if (fileName == "general (EXP).bat") profile.Description = "Экспериментальная. Fake + Multisplit с stun2.";
                    else if (fileName == "general (FAKE TLS AUTO).bat") profile.Description = "Автоматический подбор фейка с badseq (increment=2).";
                    else if (fileName == "general (FAKE TLS AUTO ALT).bat") profile.Description = "Fake с автоматическим SNI google.com.";
                    else if (fileName == "general (FAKE TLS AUTO ALT2).bat") profile.Description = "Fake + Multisplit с badseq (increment=10000000).";
                    else if (fileName == "general (FAKE TLS AUTO ALT3).bat") profile.Description = "Fake + Multisplit с ts (вместо badseq).";
                    else if (fileName == "general (SIMPLE FAKE).bat") profile.Description = "Простой фейк TLS + Hostfakesplit.";
                    else if (fileName == "general (SIMPLE FAKE ALT).bat") profile.Description = "Простой фейк TLS с badseq (increment=2).";
                    else if (fileName == "general (SIMPLE FAKE ALT2).bat") profile.Description = "Простой фейк TLS с ts.";
                    else profile.Description = "Альтернативная стратегия.";

                    AllProfiles.Add(profile);
                }

                RefreshProfilesList();
                if (AllProfiles.Count > 0) LstStrategies.SelectedIndex = 0;
            }
            catch { }
        }

        private async void LstStrategies_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LstStrategies.SelectedItem is StrategyProfile profile)
            {
                _selectedProfile = profile;
                TxtCurrentProfile.Text = $"Нынешний профиль: {profile.Name}";
                TxtCurrentProfile.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString(_profileTextColor);
                SaveSettings(); // Сохраняем СРАЗУ

                if (_isRunning)
                {
                    BtnMain_Click(BtnMain, new RoutedEventArgs());
                    await Task.Delay(2000);
                    BtnMain_Click(BtnMain, new RoutedEventArgs());
                }
            }
        }

        // === РЕДАКТОР СПИСКОВ ===
        private void CmbLists_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbLists.SelectedItem is ComboBoxItem selectedItem)
            {
                string fileName = selectedItem.Tag.ToString();
                _currentListPath = Path.Combine(_repoPath, "lists", fileName);

                if (File.Exists(_currentListPath))
                {
                    try
                    {
                        TxtListEditor.Text = File.ReadAllText(_currentListPath);
                        TxtListEditor.IsEnabled = true;
                        BtnSaveList.IsEnabled = true;
                        TxtListHint.Text = $"Редактирование: {_currentListPath}";
                    }
                    catch (Exception ex)
                    {
                        TxtListEditor.Text = "Ошибка чтения: " + ex.Message;
                        TxtListEditor.IsEnabled = false;
                        BtnSaveList.IsEnabled = false;
                    }
                }
                else
                {
                    TxtListEditor.Text = "";
                    TxtListEditor.IsEnabled = true;
                    BtnSaveList.IsEnabled = true;
                    TxtListHint.Text = $"Файл {_currentListPath} не найден, будет создан при сохранении.";
                }
            }
        }

        private void BtnSaveList_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_currentListPath)) return;

            try
            {
                File.WriteAllText(_currentListPath, TxtListEditor.Text);
                MessageBox.Show("Список успешно сохранен!", "Успех");
                AddLog($"Сохранен файл: {Path.GetFileName(_currentListPath)}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения: " + ex.Message, "Ошибка");
            }
        }

        // === ОСТАЛЬНЫЕ ФУНКЦИИ ===

        private void BtnUpdateIpset_Click(object sender, RoutedEventArgs e)
        {
            AddLog("Обновление списков IP...");
            try
            {
                string listFile = Path.Combine(_repoPath, "lists", "ipset-all.txt");
                string url = "https://raw.githubusercontent.com/Flowseal/zapret-discord-youtube/refs/heads/main/.service/ipset-service.txt";

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -Command \"Invoke-WebRequest -Uri '{url}' -OutFile '{listFile}'\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                var process = Process.Start(psi);
                process.WaitForExit();

                MessageBox.Show("Списки IP успешно обновлены!", "Успех");
                AddLog("Списки IP обновлены.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка обновления списков: " + ex.Message);
                AddLog("Ошибка обновления IP: " + ex.Message);
            }
        }

        private void BtnClearDiscord_Click(object sender, RoutedEventArgs e)
        {
            AddLog("Очистка кэша Discord...");
            try
            {
                var discordProcs = Process.GetProcessesByName("Discord");
                foreach (var p in discordProcs) p.Kill();

                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string[] discordFolders = { "discord", "discordptb", "discordcanary" };
                string[] cacheFolders = { "Cache", "Code Cache", "GPUCache" };

                foreach (var folder in discordFolders)
                {
                    foreach (var cache in cacheFolders)
                    {
                        string path = Path.Combine(appData, folder, cache);
                        if (Directory.Exists(path)) Directory.Delete(path, true);
                    }
                }
                MessageBox.Show("Кэш Discord успешно очищен!", "Успех");
                AddLog("Кэш Discord очищен.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка очистки кэша: " + ex.Message);
                AddLog("Ошибка очистки кэша: " + ex.Message);
            }
        }

        private async void BtnCheckUpdates_Click(object sender, RoutedEventArgs e)
        {
            AddLog("Проверка обновлений...");
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c \"\"{Path.Combine(_repoPath, "service.bat")}\" check_updates\"",
                    WorkingDirectory = _repoPath,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    StandardOutputEncoding = System.Text.Encoding.UTF8
                };

                var process = Process.Start(psi);
                string output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();

                if (output.Contains("Latest version installed"))
                {
                    string version = "";
                    foreach (var line in output.Split('\n'))
                    {
                        if (line.Contains("Latest version installed")) { version = line.Trim(); break; }
                    }
                    MessageBox.Show($"У вас установлена самая свежая версия.\n{version}", "Обновления", MessageBoxButton.OK, MessageBoxImage.Information);
                    AddLog("Обновлений нет.");
                }
                else if (output.Contains("New version available") || output.Contains("Opening the download page"))
                {
                    MessageBox.Show("Доступна новая версия Zapret!\nСтраница скачивания открыта в браузере.", "Обновления", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    AddLog("Найдено обновление.");
                }
                else if (output.Contains("Warning: failed to fetch"))
                {
                    MessageBox.Show("Не удалось проверить обновления (нет связи с GitHub).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    AddLog("Ошибка сети при проверке.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при проверке обновлений: " + ex.Message, "Ошибка");
            }
        }

        private void AddDiagItem(string text, string type)
        {
            SolidColorBrush color;
            string icon;

            if (type == "ok") { color = System.Windows.Media.Brushes.LightGreen; icon = "✓"; }
            else if (type == "error") { color = System.Windows.Media.Brushes.Red; icon = "✗"; }
            else { color = System.Windows.Media.Brushes.Orange; icon = "!"; }

            var panel = new StackPanel { Orientation = System.Windows.Controls.Orientation.Horizontal, Margin = new Thickness(0, 5, 0, 5) };

            var iconText = new TextBlock { Text = icon, Foreground = color, FontSize = 16, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 10, 0) };
            var messageText = new TextBlock { Text = text, Foreground = System.Windows.Media.Brushes.White, FontSize = 14, TextWrapping = TextWrapping.Wrap };

            panel.Children.Add(iconText);
            panel.Children.Add(messageText);
            PnlDiagnostics.Children.Add(panel);
        }

        private void BtnDiagnostics_Click(object sender, RoutedEventArgs e)
        {
            PnlDiagnostics.Children.Clear();
            AddLog("Запуск диагностики...");

            AddDiagItem($"Zapret установлен в: {_repoPath}", "ok");

            try
            {
                ProcessStartInfo psi = new ProcessStartInfo { FileName = "sc", Arguments = "query BFE", UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true };
                var p = Process.Start(psi);
                if (p.StandardOutput.ReadToEnd().Contains("RUNNING")) AddDiagItem("Base Filtering Engine работает", "ok");
                else AddDiagItem("Base Filtering Engine НЕ работает (критично!)", "error");
                p.WaitForExit();
            }
            catch { AddDiagItem("Не удалось проверить BFE", "warn"); }

            try
            {
                var regKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings");
                if (regKey != null && regKey.GetValue("ProxyEnable")?.ToString() == "1")
                    AddDiagItem("Включен системный прокси (может конфликтовать)", "warn");
                else
                    AddDiagItem("Системный прокси выключен", "ok");
            }
            catch { AddDiagItem("Не удалось проверить прокси", "warn"); }

            try
            {
                ProcessStartInfo psi = new ProcessStartInfo { FileName = "netsh", Arguments = "interface tcp show global", UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, StandardOutputEncoding = System.Text.Encoding.UTF8 };
                var p = Process.Start(psi);
                string output = p.StandardOutput.ReadToEnd();
                if (output.Contains("timestamps") && output.Contains("enabled")) AddDiagItem("TCP Timestamps включены", "ok");
                else AddDiagItem("TCP Timestamps выключены (могут быть проблемы)", "warn");
                p.WaitForExit();
            }
            catch { AddDiagItem("Не удалось проверить TCP Timestamps", "warn"); }

            if (Process.GetProcessesByName("AdguardSvc").Length > 0) AddDiagItem("Найден Adguard (конфликтует с Zapret!)", "error");
            else AddDiagItem("Adguard не найден", "ok");

            try
            {
                ProcessStartInfo psi = new ProcessStartInfo { FileName = "sc", Arguments = "query", UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true };
                var p = Process.Start(psi);
                string output = p.StandardOutput.ReadToEnd();
                if (output.Contains("Killer")) AddDiagItem("Найдены службы Killer (конфликтуют)", "error");
                else AddDiagItem("Killer не найден", "ok");
                p.WaitForExit();
            }
            catch { AddDiagItem("Не удалось проверить Killer", "warn"); }

            try
            {
                ProcessStartInfo psi = new ProcessStartInfo { FileName = "sc", Arguments = "query", UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true };
                var p = Process.Start(psi);
                string output = p.StandardOutput.ReadToEnd();
                if (output.Contains("Intel") && output.Contains("Connectivity")) AddDiagItem("Найдена Intel Connectivity (конфликтует)", "error");
                else AddDiagItem("Intel Connectivity не найдена", "ok");
                p.WaitForExit();
            }
            catch { AddDiagItem("Не удалось проверить Intel Connectivity", "warn"); }

            try
            {
                ProcessStartInfo psi = new ProcessStartInfo { FileName = "sc", Arguments = "query", UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true };
                var p = Process.Start(psi);
                string output = p.StandardOutput.ReadToEnd();
                if (output.Contains("TracSrvWrapper") || output.Contains("EPWD")) AddDiagItem("Найден Check Point (конфликтует)", "error");
                else AddDiagItem("Check Point не найден", "ok");
                p.WaitForExit();
            }
            catch { AddDiagItem("Не удалось проверить Check Point", "warn"); }

            try
            {
                ProcessStartInfo psi = new ProcessStartInfo { FileName = "sc", Arguments = "query", UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true };
                var p = Process.Start(psi);
                string output = p.StandardOutput.ReadToEnd();
                if (output.Contains("SmartByte")) AddDiagItem("Найден SmartByte (конфликтует)", "error");
                else AddDiagItem("SmartByte не найден", "ok");
                p.WaitForExit();
            }
            catch { AddDiagItem("Не удалось проверить SmartByte", "warn"); }

            try
            {
                string psCommand = "Get-Service | Where-Object { $_.Name -match 'vpn|happ|radmin|check|trac' -or $_.DisplayName -match 'vpn|happ|radmin|check|trac' } | Select-Object -ExpandProperty Name";

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -Command \"{psCommand}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    StandardOutputEncoding = System.Text.Encoding.UTF8
                };

                var p = Process.Start(psi);
                string output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();

                string[] services = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                System.Collections.Generic.List<string> vpnServices = new System.Collections.Generic.List<string>();

                foreach (var svc in services)
                {
                    string trimmed = svc.Trim();
                    if (!string.IsNullOrEmpty(trimmed) && !vpnServices.Contains(trimmed))
                    {
                        vpnServices.Add(trimmed);
                    }
                }

                if (vpnServices.Count > 0)
                {
                    AddDiagItem($"Найдены VPN/конфликтующие службы:", "error");
                    foreach (var vpn in vpnServices)
                    {
                        AddDiagItem($"   - {vpn}", "error");
                    }
                }
                else
                {
                    AddDiagItem("VPN-службы не найдены", "ok");
                }
            }
            catch { AddDiagItem("Не удалось проверить VPN", "warn"); }

            AddDiagItem("Проверьте настройки Secure DNS в браузере (это важно)", "warn");

            var winws = Process.GetProcessesByName("winws");
            if (winws.Length > 0) AddDiagItem("winws.exe запущен (обход активен)", "ok");
            else AddDiagItem("winws.exe не запущен (обход выключен)", "warn");

            AddLog("Диагностика завершена.");
        }

        private void LoadGameFilterSettings()
        {
            try
            {
                string gameFilterPath = Path.Combine(_repoPath, "utils", "game_filter.enabled");
                if (File.Exists(gameFilterPath))
                {
                    string content = File.ReadAllText(gameFilterPath);
                    if (content.Contains("mode=all")) CmbGameFilter.SelectedIndex = 1;
                    else if (content.Contains("mode=tcp")) CmbGameFilter.SelectedIndex = 2;
                    else if (content.Contains("mode=udp")) CmbGameFilter.SelectedIndex = 3;
                }
                else CmbGameFilter.SelectedIndex = 0;
            }
            catch { }
        }

        private async void CmbGameFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            try
            {
                string utilsDir = Path.Combine(_repoPath, "utils");
                string gameFilterPath = Path.Combine(utilsDir, "game_filter.enabled");
                if (!Directory.Exists(utilsDir)) Directory.CreateDirectory(utilsDir);

                if (CmbGameFilter.SelectedItem is ComboBoxItem selectedItem)
                {
                    string mode = selectedItem.Tag.ToString();
                    if (mode == "disabled")
                    {
                        if (File.Exists(gameFilterPath)) File.Delete(gameFilterPath);
                    }
                    else
                    {
                        File.WriteAllText(gameFilterPath, $"mode={mode}\ntcp=1024-65535\nudp=1024-65535");
                    }
                    SaveSettings(); // Сохраняем СРАЗУ

                    if (_isRunning)
                    {
                        AddLog("Смена игрового режима. Перезапуск...");
                        BtnMain_Click(BtnMain, new RoutedEventArgs());
                        await Task.Delay(2000);
                        BtnMain_Click(BtnMain, new RoutedEventArgs());
                    }
                }
            }
            catch { }
        }

        private void CheckIfRunning()
        {
            var processes = Process.GetProcessesByName("winws");
            if (processes.Length > 0) { _isRunning = true; UpdateUI(true); }
            else { _isRunning = false; UpdateUI(false); }
        }

        private void UpdateUI(bool isRunning)
        {
            if (isRunning)
            {
                BtnMain.Background = _colorGreen;
                TxtStatusCenter.Text = "Подключено";
                TxtStatusCenter.Foreground = _colorGreen;
                TxtStatusLeft.Text = "Включено";
                TxtStatusLeft.Foreground = _colorGreen;
            }
            else
            {
                BtnMain.Background = _colorGray;
                TxtStatusCenter.Text = "Отключено";
                TxtStatusCenter.Foreground = System.Windows.Media.Brushes.Gray;
                TxtStatusLeft.Text = "Отключено";
                TxtStatusLeft.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }

        private async void BtnMain_Click(object sender, RoutedEventArgs e)
        {
            if (!_isRunning)
            {
                if (_selectedProfile == null)
                {
                    MessageBox.Show("Выберите стратегию в разделе 'Профили'!"); return;
                }

                string batName = _selectedProfile.FileName;
                string batPath = Path.Combine(_repoPath, batName);

                BtnMain.Background = _colorBlue;
                TxtStatusCenter.Text = "Подключение..."; TxtStatusCenter.Foreground = System.Windows.Media.Brushes.White;
                TxtStatusLeft.Text = "Подключение..."; TxtStatusLeft.Foreground = System.Windows.Media.Brushes.White;
                AddLog($"Запуск профиля: {batName}");

                try
                {
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/c \"\"{batPath}\"\"",
                        WorkingDirectory = _repoPath,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden
                    };
                    Process.Start(psi);

                    bool processFound = false;
                    for (int i = 0; i < 10; i++)
                    {
                        await Task.Delay(500);
                        if (Process.GetProcessesByName("winws").Length > 0) { processFound = true; break; }
                    }

                    if (processFound) { _isRunning = true; UpdateUI(true); AddLog("Обход успешно запущен."); }
                    else throw new Exception("Процесс winws.exe не найден за 5 секунд.");
                }
                catch (Exception ex)
                {
                    BtnMain.Background = _colorRed;
                    TxtStatusCenter.Text = "Ошибка"; TxtStatusCenter.Foreground = _colorRed;
                    TxtStatusLeft.Text = "Ошибка"; TxtStatusLeft.Foreground = _colorRed;
                    AddLog("Ошибка запуска: " + ex.Message);
                    MessageBox.Show("Ошибка: " + ex.Message);
                }
            }
            else
            {
                try
                {
                    BtnMain.Background = _colorBlue;
                    TxtStatusCenter.Text = "Отключение..."; TxtStatusCenter.Foreground = System.Windows.Media.Brushes.White;
                    TxtStatusLeft.Text = "Отключение..."; TxtStatusLeft.Foreground = System.Windows.Media.Brushes.White;
                    AddLog("Выключение обхода...");

                    var processes = Process.GetProcessesByName("winws");
                    foreach (var p in processes) { p.Kill(); p.WaitForExit(); }

                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = "/c net stop winws",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden
                    };
                    Process.Start(psi);

                    await Task.Delay(1000);
                    _isRunning = false;
                    UpdateUI(false);
                    AddLog("Обход отключен.");
                }
                catch (Exception ex) { MessageBox.Show("Ошибка при остановке: " + ex.Message); }
            }
        }
    }
}