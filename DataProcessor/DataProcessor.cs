using FileProcessing.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FileProcessing.DataProcessor
{
    public class DataProcessor
    {
        // Словарь для преобразования типов данных в размеры в байтах
        private readonly Dictionary<string, int> _typeSizes = new Dictionary<string, int>
    {
        { "int", 4 },
        { "double", 8 },
        { "bool", 1 },
        { "long", 8 }
    };

        public List<TypeInfo> LoadTypeInfos(string filePath)
        {
            var json = File.ReadAllText(filePath);
            var root = JsonSerializer.Deserialize<TypeInfosRoot>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return root?.TypeInfos ?? new List<TypeInfo>();
        }

        public List<InputRecord> LoadInputCsv(string filePath)
        {
            var records = new List<InputRecord>();

            if (!File.Exists(filePath))
                return records;

            try
            {
                using var reader = new StreamReader(filePath);

                // Пропускаем заголовок если есть
                string header = reader.ReadLine();

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var values = line.Split(';');

                    if (values.Length >= 2)
                    {
                        records.Add(new InputRecord
                        {
                            Path = values[0].Trim(),
                            Type = values[1].Trim()
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка чтения CSV файла: {ex.Message}", ex);
            }

            return records;
        }

        public List<OutputItem> ProcessData(List<InputRecord> inputRecords, List<TypeInfo> typeInfos)
        {
            var output = new List<OutputItem>();

            if (inputRecords == null || typeInfos == null)
                return output;

            foreach (var record in inputRecords.Where(r => r.IsSelected))
            {
                // Ищем тип по TypeName
                var typeInfo = typeInfos.FirstOrDefault(t =>
                    t.TypeName.Equals(record.Type, StringComparison.OrdinalIgnoreCase));

                if (typeInfo != null && typeInfo.Properties != null)
                {
                    int currentOffset = 0;

                    foreach (var property in typeInfo.Properties)
                    {
                        string fieldName = property.Key;
                        string dataType = property.Value;

                        // Получаем размер типа данных
                        if (_typeSizes.TryGetValue(dataType, out int fieldSize))
                        {
                            output.Add(new OutputItem
                            {
                                Tag = $"{record.Path}.{fieldName}",
                                Offset = currentOffset
                            });

                            currentOffset += fieldSize;
                        }
                        else
                        {
                            // Если тип неизвестен, используем размер по умолчанию (4 байта)
                            output.Add(new OutputItem
                            {
                                Tag = $"{record.Path}.{fieldName}",
                                Offset = currentOffset
                            });

                            currentOffset += 4;
                        }
                    }
                }
            }

            return output;
        }

        public string GenerateXml(List<OutputItem> items)
        {
            if (items == null || !items.Any())
                return "<items></items>";

            var xml = new XElement("items");

            foreach (var item in items)
            {
                var itemElement = new XElement("item",
                    new XAttribute("Binding", "Introduced"),
                    new XElement("node_path", item.Tag ?? ""),
                    new XElement("address", item.Offset)
                );
                xml.Add(itemElement);
            }

            return xml.ToString();
        }
    }
}
