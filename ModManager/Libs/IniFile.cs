using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace ModManager.Libs;

public class IniFile
{
    public string Path
    {
        get => _path;
        internal set
        {
            if (!File.Exists(value))
            {
                File.WriteAllText(value, "", Encoding.Unicode);
            }

            _path = value;
        }
    }

    public IniFile(string iniPath)
    {
        Path = iniPath;
    }

    public bool HasKey(string section, string name)
    {
        return IniReadValue(section, name) != null;
    }

    public string GetString(string section, string name, string defaultValue = "", bool autoSave = false)
    {
        var value = IniReadValue(section, name);
        if (!string.IsNullOrEmpty(value))
        {
            return value;
        }

        if (autoSave)
        {
            SetString(section, name, defaultValue);
        }

        return defaultValue;
    }

    public void SetString(string section, string name, string value)
    {
        IniWriteValue(section, name, value.Trim());
    }

    public int GetInt(string section, string name, int defaultValue = 0, bool autoSave = false)
    {
        int value;
        if (int.TryParse(IniReadValue(section, name), out value))
        {
            return value;
        }

        if (autoSave)
        {
            SetInt(section, name, defaultValue);
        }

        return defaultValue;
    }

    public void SetInt(string section, string name, int value)
    {
        IniWriteValue(section, name, value.ToString());
    }

    public float GetFloat(string section, string name, float defaultValue = 0f, bool autoSave = false)
    {
        float value;
        if (float.TryParse(IniReadValue(section, name), out value))
        {
            return value;
        }

        if (autoSave)
        {
            SetFloat(section, name, defaultValue);
        }

        return defaultValue;
    }

    public void SetFloat(string section, string name, float value)
    {
        IniWriteValue(section, name, value.ToString());
    }

    public bool GetBool(string section, string name, bool defaultValue = false, bool autoSave = false)
    {
        var sVal = GetString(section, name, null);
        if ("true".Equals(sVal) || "1".Equals(sVal) || "0".Equals(sVal) || "false".Equals(sVal))
        {
            return "true".Equals(sVal) || "1".Equals(sVal);
        }

        if (autoSave)
        {
            SetBool(section, name, defaultValue);
        }

        return defaultValue;
    }

    public void SetBool(string section, string name, bool value)
    {
        IniWriteValue(section, name, value ? "true" : "false");
    }

    [DllImport("KERNEL32.DLL", EntryPoint = "GetPrivateProfileStringW", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true,
        CallingConvention = CallingConvention.StdCall)]
    private static extern int GetPrivateProfileString(string lpSection, string lpKey, string lpDefault, StringBuilder lpReturnString, int nSize,
        string lpFileName);

    [DllImport("KERNEL32.DLL", EntryPoint = "WritePrivateProfileStringW", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true,
        CallingConvention = CallingConvention.StdCall)]
    private static extern int WritePrivateProfileString(string lpSection, string lpKey, string lpValue, string lpFileName);

    private void IniWriteValue(string section, string key, string value)
    {
        WritePrivateProfileString(section, key, value, Path);
    }

    private string IniReadValue(string section, string key)
    {
        const int maxChars = 1023;
        var result = new StringBuilder(maxChars);
        GetPrivateProfileString(section, key, " _", result, maxChars, Path);
        if (result.ToString().Equals(" _"))
        {
            return null;
        }

        return result.ToString();
    }

    private string _path = "";
}