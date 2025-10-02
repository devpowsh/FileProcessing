using FileProcessing.Models;
using Microsoft.Win32;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FileProcessing.DataProcessor;

namespace FileProcessing
{
    public partial class MainWindow : Window
    {
        private DataProcessor.DataProcessor _processor;
        private List<InputRecord> _inputRecords;
        private List<TypeInfo> _typeInfos;
        private List<OutputItem> _outputItems;

        public MainWindow()
        {
            InitializeComponent();
            _processor = new DataProcessor.DataProcessor();
            _inputRecords = new List<InputRecord>();
            _typeInfos = new List<TypeInfo>();
        }

        private void LoadCsv_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                Title = "Выберите Input.csv"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _inputRecords = _processor.LoadInputCsv(dialog.FileName);
                    DataGrid.ItemsSource = _inputRecords;
                    UpdateStatus($"Загружено {_inputRecords.Count} записей из CSV");

                    // Автоматически выбираем все записи после загрузки
                    foreach (var record in _inputRecords)
                    {
                        record.IsSelected = true;
                    }
                    DataGrid.Items.Refresh();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки CSV: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void LoadJson_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                Title = "Выберите TypeInfos.json"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _typeInfos = _processor.LoadTypeInfos(dialog.FileName);
                    UpdateStatus($"Загружено {_typeInfos.Count} типов из JSON");

                    // Показываем информацию о загруженных типах
                    var typeNames = string.Join(", ", _typeInfos.Select(t => t.TypeName));
                    MessageBox.Show($"Загружены типы: {typeNames}", "Информация о типах");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки JSON: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ProcessData_Click(object sender, RoutedEventArgs e)
        {
            if (!_inputRecords.Any())
            {
                MessageBox.Show("Сначала загрузите CSV файл", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!_typeInfos.Any())
            {
                MessageBox.Show("Сначала загрузите JSON файл", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedRecords = _inputRecords.Where(r => r.IsSelected).ToList();
            if (!selectedRecords.Any())
            {
                MessageBox.Show("Выберите хотя бы одну строку для обработки", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _outputItems = _processor.ProcessData(_inputRecords, _typeInfos);
                UpdateStatus($"Обработано {_outputItems.Count} элементов");

                // Показываем превью результатов
                var preview = string.Join(Environment.NewLine,
                    _outputItems.Take(10).Select(item => $"{item.Tag} -> {item.Offset}"));

                MessageBox.Show($"Превью результатов (первые 10):\n\n{preview}",
                    "Результаты обработки", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обработки: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveXml_Click(object sender, RoutedEventArgs e)
        {
            if (_outputItems == null || !_outputItems.Any())
            {
                MessageBox.Show("Сначала обработайте данные", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*",
                Title = "Сохранить XML файл",
                DefaultExt = ".xml"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var xml = _processor.GenerateXml(_outputItems);
                    File.WriteAllText(dialog.FileName, xml);
                    UpdateStatus($"XML сохранен: {dialog.FileName}");
                    MessageBox.Show("XML файл успешно сохранен", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка сохранения XML: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var record in _inputRecords)
            {
                record.IsSelected = true;
            }
            DataGrid.Items.Refresh();
        }

        private void DeselectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var record in _inputRecords)
            {
                record.IsSelected = false;
            }
            DataGrid.Items.Refresh();
        }

        private void UpdateStatus(string message)
        {
            StatusText.Text = $"{DateTime.Now:HH:mm:ss} - {message}";
        }
    }
}