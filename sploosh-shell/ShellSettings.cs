using System;
using System.IO;

namespace AwaShell;

/// <summary>
/// 
/// </summary>
public class ShellSettings
{
    public static string SplooshDirectory { get; } = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + "sploosh";
    public static string SettingsFile { get; } = "sploosh-settings.json";
    public string Prompt { get; set; } = "$ ";
    public string HistoryFilePath { get; set; } = ".sploosh_history";
    public int MaxHistorySize { get; set; } = 1000;
    public bool EnableAutoCompletion { get; set; } = true;

    private static ShellSettings _instance;
    public static ShellSettings Instance => _instance;
    static ShellSettings()
    {
        // Ensure the sploosh directory exists
        if (!Directory.Exists(SplooshDirectory))
        {
            try
            {
                Directory.CreateDirectory(SplooshDirectory);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating sploosh directory: {ex.Message}");
            }
        }
        // Ensure the settings file exists
        var settingsFilePath = Path.Combine(SplooshDirectory, SettingsFile);
        if (!File.Exists(settingsFilePath))
        {
            _instance = new ShellSettings();
            var settingsJson = System.Text.Json.JsonSerializer.Serialize(_instance);
            try
            {
                File.WriteAllText(settingsFilePath, settingsJson); // Create an empty JSON file
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating settings file: {ex.Message}");
            }
        }
        // Load settings from the file if it exists
        else
        {
            try
            {
                string settingsJson = File.ReadAllText(settingsFilePath);
                var loadedSettings = System.Text.Json.JsonSerializer.Deserialize<ShellSettings>(settingsJson);
                if (loadedSettings != null)
                {
                    _instance = loadedSettings;
                }
                else
                {
                    _instance = new ShellSettings();
                    File.Delete(settingsFilePath); // Delete the file if deserialization fails
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading settings: {ex.Message}");
                _instance = new ShellSettings();
            }
        }
    }
}

public static class Settings
{
    public static string Prompt => ShellSettings.Instance.Prompt;
    public static string HistoryFilePath => Path.Combine(ShellSettings.SplooshDirectory, ShellSettings.Instance.HistoryFilePath);
    public static int MaxHistorySize => ShellSettings.Instance.MaxHistorySize;
    public static bool EnableAutoCompletion => ShellSettings.Instance.EnableAutoCompletion;
    public static string SplooshDirectory => ShellSettings.SplooshDirectory;
    public static string SettingsFilePath => Path.Combine(ShellSettings.SplooshDirectory, ShellSettings.SettingsFile);
}