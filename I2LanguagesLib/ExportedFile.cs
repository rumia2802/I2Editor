namespace I2LanguagesLib;

public sealed class ExportedFile
{
    public FileMetadata Metadata { get; set; }
    public TermMetadata[] Terms { get; set; }
    public Language[] Languages { get; set; }
    public Dictionary<string, Dictionary<string, string>> Translations { get; set; }

    public ExportedFile()
    {
    }

    public ExportedFile(FileStructure file)
    {
        Translations = new Dictionary<string, Dictionary<string, string>>();
        Languages = new Language[file.Languages.Length];
        for (int i = 0; i < file.Languages.Length; i++)
        {
            var language = file.Languages[i];
            var terms = new Dictionary<string, string>();
            for (int j = 0; j < file.Terms.Length; j++)
            {
                var term = file.Terms[j];
                terms.Add(term.Key, term.Languages[i]);
            }

            Languages[i] = new Language
            {
                Name = language.Name,
                Code = language.Code,
                Flags = language.Flags,
            };

            Translations.Add(language.Code, terms);
        }

        Metadata = new FileMetadata
        {
            Header_Unknown = file.Header.unk_0,
            Header_Name = file.Header.Name,
            UserAgreesToHaveItOnTheScene = file.UserAgreesToHaveItOnTheScene,
            UserAgreesToHaveItInsideThePluginsFolder = file.UserAgreesToHaveItInsideThePluginsFolder,
            GoogleLiveSyncIsUptoDate = file.GoogleLiveSyncIsUptoDate,
            CaseInsensitiveTerms = file.CaseInsensitiveTerms,
            OnMissingTranslation = (uint)file.OnMissingTranslation,
            TermAppname = file.TermAppname,
            IgnoreDeviceLanguage = file.IgnoreDeviceLanguage,
            AllowUnloadingLanguages = (uint)file.AllowUnloadingLanguages,
            WebserviceURL = file.GoogleSpreadsheet.WebserviceURL,
            SpreadsheetKey = file.GoogleSpreadsheet.SpreadsheetKey,
            SpreadsheetName = file.GoogleSpreadsheet.SpreadsheetName,
            LastUpdatedVersion = file.GoogleSpreadsheet.LastUpdatedVersion,
            UpdateFrequency = (uint)file.GoogleSpreadsheet.UpdateFrequency,
            InEditorCheckFrequency = file.GoogleSpreadsheet.InEditorCheckFrequency,
            UpdateSynchronization = (uint)file.GoogleSpreadsheet.UpdateSynchronization,
            UpdateDelay = file.GoogleSpreadsheet.UpdateDelay,
            Assets = file.Assets,
        };

        Terms = new TermMetadata[file.Terms.Length];
        for (int i = 0; i < file.Terms.Length; i++)
        {
            var term = file.Terms[i];
            Terms[i] = new TermMetadata
            {
                Key = term.Key,
                TermType = term.TermType,
                Description = term.Description,
                Flags = term.Flags,
                LanguagesTouch = term.LanguagesTouch
            };
        }
    }

    public FileStructure Reconstruct()
    {
        FileStructure file = new();
        file.Header.unk_0 = Metadata.Header_Unknown;
        file.Header.Name = Metadata.Header_Name;
        file.UserAgreesToHaveItOnTheScene = Metadata.UserAgreesToHaveItOnTheScene;
        file.UserAgreesToHaveItInsideThePluginsFolder = Metadata.UserAgreesToHaveItInsideThePluginsFolder;
        file.GoogleLiveSyncIsUptoDate = Metadata.GoogleLiveSyncIsUptoDate;
        file.CaseInsensitiveTerms = Metadata.CaseInsensitiveTerms;
        file.OnMissingTranslation = (FileStructure.MissingTranslationAction)Metadata.OnMissingTranslation;
        file.TermAppname = Metadata.TermAppname;
        file.IgnoreDeviceLanguage = Metadata.IgnoreDeviceLanguage;
        file.AllowUnloadingLanguages = (FileStructure.AllowUnloadLanguages)Metadata.AllowUnloadingLanguages;
        file.GoogleSpreadsheet.WebserviceURL = Metadata.WebserviceURL;
        file.GoogleSpreadsheet.SpreadsheetKey = Metadata.SpreadsheetKey;
        file.GoogleSpreadsheet.SpreadsheetName = Metadata.SpreadsheetName;
        file.GoogleSpreadsheet.LastUpdatedVersion = Metadata.LastUpdatedVersion;
        file.GoogleSpreadsheet.UpdateFrequency = (GoogleSpreadsheetData.GoogleUpdateFrequency)Metadata.UpdateFrequency;
        file.GoogleSpreadsheet.InEditorCheckFrequency = Metadata.InEditorCheckFrequency;
        file.GoogleSpreadsheet.UpdateSynchronization = (GoogleSpreadsheetData.GoogleUpdateSynchronization)Metadata.UpdateSynchronization;
        file.GoogleSpreadsheet.UpdateDelay = Metadata.UpdateDelay;
        file.Assets = Metadata.Assets;

        file.Languages = new LanguageCode[Languages.Length];
        for (int i = 0; i < Languages.Length; i++)
        {
            var language = Languages[i];
            file.Languages[i] = new LanguageCode
            {
                Name = language.Name,
                Code = language.Code,
                Flags = language.Flags
            };
        }

        file.Terms = new Term[Terms.Length];
        for (int i = 0; i < Terms.Length; i++)
        {
            var term = Terms[i];
            file.Terms[i] = new Term
            {
                Key = term.Key,
                TermType = term.TermType,
                Description = term.Description,
                Flags = term.Flags,
                LanguagesTouch = term.LanguagesTouch
            };
        }

        for (int i = 0; i < Languages.Length; i++)
        {
            var language = Languages[i];
            for (int j = 0; j < Terms.Length; j++)
            {
                var term = Terms[j];
                if (file.Terms[j].Languages == null)
                    file.Terms[j].Languages = new string[Languages.Length];
                file.Terms[j].Languages[i] = Translations[language.Code][term.Key];
            }
        }

        return file;
    }
}

public class Language
{
    public string Name { get; set; }
    public string Code { get; set; }
    public uint Flags { get; set; }
}

public class FileMetadata
{
    public byte[] Header_Unknown { get; set; }
    public string Header_Name { get; set; }
    public uint UserAgreesToHaveItOnTheScene { get; set; }
    public uint UserAgreesToHaveItInsideThePluginsFolder { get; set; }
    public uint GoogleLiveSyncIsUptoDate { get; set; }
    public uint CaseInsensitiveTerms { get; set; }
    public uint OnMissingTranslation { get; set; }
    public string TermAppname { get; set; }
    public uint IgnoreDeviceLanguage { get; set; }
    public uint AllowUnloadingLanguages { get; set; }
    public string WebserviceURL { get; set; }
    public string SpreadsheetKey { get; set; }
    public string SpreadsheetName { get; set; }
    public string LastUpdatedVersion { get; set; }
    public uint UpdateFrequency { get; set; }
    public uint InEditorCheckFrequency { get; set; }
    public uint UpdateSynchronization { get; set; }
    public uint UpdateDelay { get; set; }
    public Asset[] Assets { get; set; }
}

public class TermMetadata
{
    public string Key { get; set; }
    public uint TermType { get; set; }
    public string Description { get; set; }
    public byte[] Flags { get; set; }
    public string[] LanguagesTouch { get; set; }
}