using I2LanguagesLib;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace I2Editor.Common;

public static class ExportUtils
{
	public static void SaveProject(ExportedFile file, string path)
	{
		string metadataPath = Path.Combine(path, "metadata.json");
		string termsPath = Path.Combine(path, "terms.json");
		string languagesPath = Path.Combine(path, "languages.json");

		string metadataJson = JsonConvert.SerializeObject(file.Metadata, Formatting.Indented);
		string termsJson = JsonConvert.SerializeObject(file.Terms, Formatting.Indented);
		string languagesJson = JsonConvert.SerializeObject(file.Languages, Formatting.Indented);

		File.WriteAllText(metadataPath, metadataJson);
		File.WriteAllText(termsPath, termsJson);
		File.WriteAllText(languagesPath, languagesJson);

		// Create locales folder
		string localesPath = Path.Combine(path, "locales");
		if (!Directory.Exists(localesPath))
			Directory.CreateDirectory(localesPath);

		// Create a file for each language
		foreach (var lang in file.Translations)
		{
			string langPath = Path.Combine(localesPath, $"{lang.Key}.json");
			string langJson = JsonConvert.SerializeObject(lang.Value, Formatting.Indented);
			File.WriteAllText(langPath, langJson);
		}
	}

	public static ExportedFile OpenProject(string path)
	{
		string metadataPath = Path.Combine(path, "metadata.json");
		string termsPath = Path.Combine(path, "terms.json");
		string languagesPath = Path.Combine(path, "languages.json");
		string localesPath = Path.Combine(path, "locales");

		string metadataJson = File.ReadAllText(metadataPath);
		string termsJson = File.ReadAllText(termsPath);
		string languagesJson = File.ReadAllText(languagesPath);
		ExportedFile result = new ExportedFile()
		{
			Metadata = JsonConvert.DeserializeObject<FileMetadata>(metadataJson),
			Terms = JsonConvert.DeserializeObject<TermMetadata[]>(termsJson),
			Languages = JsonConvert.DeserializeObject<Language[]>(languagesJson),
			Translations = new Dictionary<string, Dictionary<string, string>>()
		};

		var locales = Directory.GetFiles(localesPath, "*.json");
		foreach (var locale in locales)
		{
			string localeJson = File.ReadAllText(locale);
			string localeName = Path.GetFileNameWithoutExtension(locale);
			result.Translations.Add(localeName, JsonConvert.DeserializeObject<Dictionary<string, string>>(localeJson));
		}

		return result;
	}

	public static void ExportToCSV(string path, ExportedFile file)
	{
		using var writer = new StreamWriter(File.OpenWrite(path));
		using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

		dynamic record = new System.Dynamic.ExpandoObject();
		record.Key = "Key";
		foreach (var lang in file.Languages)
		{
			((IDictionary<string, object>)record)[lang.Code] = lang.Code;
		}
		csv.WriteRecord(record);

		foreach (var term in file.Terms)
		{
			csv.NextRecord();
			record = new System.Dynamic.ExpandoObject();
			record.Key = term.Key;
			foreach (var lang in file.Languages)
			{
				((IDictionary<string, object>)record)[lang.Code] = file.Translations[lang.Code][term.Key];
			}
			csv.WriteRecord(record);
		}
	}

	public static void ImportFromCsv(ExportedFile project, string path)
	{
		using var reader = new StreamReader(File.OpenRead(path));
		using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

		var records = csv.GetRecords<dynamic>();
		int i = 0;
		foreach (dynamic record in records)
		{
			string key = record.Key;

			foreach (dynamic lang in record)
			{
				if (lang.Key == "Key")
					continue;

				string langCode = lang.Key;
				string value = lang.Value;

				if (!project.Translations.ContainsKey(langCode)) continue;

				var langDict = project.Translations[langCode];

				if (string.IsNullOrEmpty(key))
				{
					throw new Exception($"Key is empty in language {langCode} (record #{i})");
				}

				if (langDict.ContainsKey(key))
					langDict[key] = value;
				else throw new Exception($"Key {key} not found in language {langCode} (record #{i})");
			}

			i++;
		}
	}
}
