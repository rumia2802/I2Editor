using System.Text;

namespace I2LanguagesLib;

public sealed class FileStructure
{
    public enum MissingTranslationAction : uint
    {
        Empty, Fallback,
        ShowWarning, ShowTerm
    }

    public enum AllowUnloadLanguages
    {
        Never, OnlyInDevice, EditorAndDevice
    }

    public Header Header;
    public uint UserAgreesToHaveItOnTheScene;
    public uint UserAgreesToHaveItInsideThePluginsFolder;
    public uint GoogleLiveSyncIsUptoDate;
    public Term[] Terms;
    public uint CaseInsensitiveTerms;
    public MissingTranslationAction OnMissingTranslation;
    public string TermAppname;
    public LanguageCode[] Languages;
    public uint IgnoreDeviceLanguage;
    public AllowUnloadLanguages AllowUnloadingLanguages;
    public GoogleSpreadsheetData GoogleSpreadsheet;
    public Asset[] Assets;

    #region Write
    public void WriteFile(BinaryWriter writer)
    {
        WriteHeader(writer, Header);

        writer.Write(UserAgreesToHaveItOnTheScene);
        writer.Write(UserAgreesToHaveItInsideThePluginsFolder);
        writer.Write(GoogleLiveSyncIsUptoDate);

        writer.Write(Terms.Length);
        foreach (var term in Terms)
        {
            WriteTerm(writer, term);
        }

        writer.Write(CaseInsensitiveTerms);
        writer.Write((uint)OnMissingTranslation);

        WriteString(writer, TermAppname);

        writer.Write(Languages.Length);
        foreach (var language in Languages)
        {
            WriteString(writer, language.Code);
            WriteString(writer, language.Name);
            writer.Write(language.Flags);
        }

        writer.Write(IgnoreDeviceLanguage);
        writer.Write((uint)AllowUnloadingLanguages);
        WriteGoogle(writer, GoogleSpreadsheet);
        writer.Write(Assets.Length);
        foreach (var asset in Assets)
        {
            writer.Write(asset.FileID);
            writer.Write(asset.PathID);
        }
    }

    private static void WritePadding(BinaryWriter writer)
    {
        int padding = 4 - (int)writer.BaseStream.Position % 4;
        if (padding != 4)
            writer.Write(new byte[padding]);
    }

    private static void WriteString(BinaryWriter writer, string str)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(str);
        writer.Write(bytes.Length);
        writer.Write(bytes);
        WritePadding(writer);
    }

    private static void WriteHeader(BinaryWriter writer, Header header)
    {
        writer.Write(header.unk_0);
        WriteString(writer, header.Name);
    }

    private static void WriteTerm(BinaryWriter writer, Term term)
    {
        WriteString(writer, term.Key);
        writer.Write(term.TermType);
        WriteString(writer, term.Description);
        writer.Write(term.Languages.Length);
        foreach (var language in term.Languages)
        {
            WriteString(writer, language);
        }
        writer.Write(term.Flags.Length);
        writer.Write(term.Flags);
        writer.Write(term.LanguagesTouch.Length);
        foreach (var language in term.LanguagesTouch)
        {
            WriteString(writer, language);
        }
    }

    private static void WriteGoogle(BinaryWriter writer, GoogleSpreadsheetData google)
    {
        WriteString(writer, google.WebserviceURL);
        WriteString(writer, google.SpreadsheetKey);
        WriteString(writer, google.SpreadsheetName);
        WriteString(writer, google.LastUpdatedVersion);
        writer.Write((uint)google.UpdateFrequency);
        writer.Write(google.InEditorCheckFrequency);
        writer.Write((uint)google.UpdateSynchronization);
        writer.Write(google.UpdateDelay);
    }

    #endregion

    #region Read
    public static FileStructure ReadFile(BinaryReader reader)
    {
        FileStructure fileStructure = new()
        {
            Header = ReadHeader(reader)
        };
        ReadPadding(reader);

        fileStructure.UserAgreesToHaveItOnTheScene = reader.ReadUInt32();
        fileStructure.UserAgreesToHaveItInsideThePluginsFolder = reader.ReadUInt32();
        fileStructure.GoogleLiveSyncIsUptoDate = reader.ReadUInt32();

        int termsCount = reader.ReadInt32();
        fileStructure.Terms = new Term[termsCount];
        for (int i = 0; i < termsCount; i++)
        {
            fileStructure.Terms[i] = ReadTerm(reader);
        }

        fileStructure.CaseInsensitiveTerms = reader.ReadUInt32();
        fileStructure.OnMissingTranslation = (MissingTranslationAction)reader.ReadUInt32();

        fileStructure.TermAppname = ReadString(reader);
        ReadPadding(reader);

        int languagesCount = reader.ReadInt32();
        fileStructure.Languages = new LanguageCode[languagesCount];
        for (int i = 0; i < languagesCount; i++)
        {
            fileStructure.Languages[i] = ReadLanguageCode(reader);
        }

        fileStructure.IgnoreDeviceLanguage = reader.ReadUInt32();
        fileStructure.AllowUnloadingLanguages = (AllowUnloadLanguages)reader.ReadUInt32();
        fileStructure.GoogleSpreadsheet = ReadGoogle(reader);
        fileStructure.Assets = new Asset[reader.ReadInt32()];
        for (int i = 0; i < fileStructure.Assets.Length; i++)
        {
            fileStructure.Assets[i] = new Asset
            {
                FileID = reader.ReadUInt32(),
                PathID = reader.ReadUInt64()
            };
        }

        return fileStructure;
    }

    private static Header ReadHeader(BinaryReader reader)
    {
        Header header = new()
        {
            unk_0 = reader.ReadBytes(0x1C),
            Name = ReadString(reader)
        };
        return header;
    }

    private static string ReadString(BinaryReader reader)
    {
        int length = reader.ReadInt32();
        if (length == 0) return string.Empty;
        return Encoding.UTF8.GetString(reader.ReadBytes(length));
    }

    private static Term ReadTerm(BinaryReader reader)
    {
        Term term = new()
        {
            Key = ReadString(reader),
        };

        ReadPadding(reader);

        term.TermType = reader.ReadUInt32();
        term.Description = ReadString(reader);
        ReadPadding(reader);
        int languagesCount = reader.ReadInt32();
        term.Languages = ReadLanguageLines(reader, languagesCount);
        int count = reader.ReadInt32();
        term.Flags = reader.ReadBytes(count);
        int touchCount = reader.ReadInt32();
        term.LanguagesTouch = ReadLanguageLines(reader, touchCount);

        return term;
    }

    private static string[] ReadLanguageLines(BinaryReader reader, int count)
    {
        string[] strings = new string[count];
        for (int i = 0; i < count; i++)
        {
            strings[i] = ReadString(reader);
            ReadPadding(reader);
        }
        return strings;
    }

    private static LanguageCode ReadLanguageCode(BinaryReader reader)
    {
        LanguageCode languageCode = new()
        {
            Code = ReadString(reader),
        };
        ReadPadding(reader);
        languageCode.Name = ReadString(reader);
        ReadPadding(reader);
        languageCode.Flags = reader.ReadUInt32();
        return languageCode;
    }

    private static void ReadPadding(BinaryReader reader)
    {
        int padding = 4 - (int)reader.BaseStream.Position % 4;
        if (padding != 4)
            reader.ReadBytes(padding);
    }

    private static GoogleSpreadsheetData ReadGoogle(BinaryReader reader)
    {
        GoogleSpreadsheetData google = new();
        google.WebserviceURL = ReadString(reader);
        ReadPadding(reader);
        google.SpreadsheetKey = ReadString(reader);
        ReadPadding(reader);
        google.SpreadsheetName = ReadString(reader);
        ReadPadding(reader);
        google.LastUpdatedVersion = ReadString(reader);
        ReadPadding(reader);
        google.UpdateFrequency = (GoogleSpreadsheetData.GoogleUpdateFrequency)reader.ReadUInt32();
        google.InEditorCheckFrequency = reader.ReadUInt32();
        google.UpdateSynchronization = (GoogleSpreadsheetData.GoogleUpdateSynchronization)reader.ReadUInt32();
        google.UpdateDelay = reader.ReadUInt32();
        return google;
    }
    #endregion
}

public struct Header
{
    public byte[] unk_0;
    public string Name;
}

public struct Term
{
    public string Key;
    public uint TermType;
    public string Description;
    public string[] Languages;
    public byte[] Flags;
    public string[] LanguagesTouch;
}

public struct LanguageCode
{
    public string Code;
    public string Name;
    public uint Flags;
}

public struct Asset
{
    public uint FileID;
    public ulong PathID;
}

public struct GoogleSpreadsheetData
{
    public enum GoogleUpdateFrequency
    {
        Always, Never, Daily, Weekly,
        Monthly, OnlyOnce, EveryOtherDay
    }

    public enum GoogleUpdateSynchronization
    {
        Manual, OnSceneLoaded, AsSoonAsDownloaded
    }

    public string WebserviceURL;
    public string SpreadsheetKey;
    public string SpreadsheetName;
    public string LastUpdatedVersion;
    public GoogleUpdateFrequency UpdateFrequency;
    public uint InEditorCheckFrequency;
    public GoogleUpdateSynchronization UpdateSynchronization;
    public uint UpdateDelay;
}