using System;
using System.Collections.Generic;
using System.IO;
using Geonorge.MassivNedlasting.Gui;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;


namespace Geonorge.MassivNedlasting
{
    /// <summary>
    /// Holds application settings
    /// </summary>
    public class ApplicationService
    {

        /// <summary>
        /// Returns path to the file containing the list of dataset to download.
        /// </summary>
        /// <returns></returns>
        public static string GetDownloadFilePath()
        {
            DirectoryInfo appDirectory = GetAppDirectory();

            return Path.Combine(appDirectory.FullName, "download.json");
        }

        /// <summary>
        /// Returns path to the file containing the list of projections in epsg-registry - https://register.geonorge.no/register/epsg-koder
        /// </summary>
        /// <returns></returns>
        public static string GetProjectionFilePath()
        {
            DirectoryInfo appDirectory = GetAppDirectory();

            return Path.Combine(appDirectory.FullName, "projections.json");
        }

        /// <summary>
        /// Returns path to the file containing the list of downloaded datasets.
        /// </summary>
        /// <returns></returns>
        public static string GetDownloadHistoryFilePath()
        {
            DirectoryInfo appDirectory = GetAppDirectory();

            return Path.Combine(appDirectory.FullName, "downloadHistory.json");
        }

        /// <summary>
        /// Returns path to the log file containing the list of downloaded datasets.
        /// </summary>
        /// <returns></returns>
        public static string GetDownloadLogFilePath()
        {
            var logDirectory = LogDirectory();
            var name = DateTime.Now.ToString("yyyy-M-dd--HH-mm-ss");

            return Path.Combine(logDirectory, name + ".txt");
        }


        public static string GetUserName()
        {
            return GetAppSettings().Username;
        }

        /// <summary>
        ///     App directory is located within the users AppData folder
        /// </summary>
        /// <returns></returns>
        public static DirectoryInfo GetAppDirectory()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            var appDirectory = new DirectoryInfo(appDataPath + Path.DirectorySeparatorChar + "Geonorge"
                                                 + Path.DirectorySeparatorChar + "Nedlasting");

            if (!appDirectory.Exists)
                appDirectory.Create();

            return appDirectory;
        }

        /// <summary>
        ///     App directory is located within the users AppData folder
        /// </summary>
        /// <returns></returns>
        public static DirectoryInfo GetLogAppDirectory()
        {
            var appDirectory = new DirectoryInfo(GetAppDirectory().ToString() + Path.DirectorySeparatorChar + "Log");

            if (!appDirectory.Exists)
                appDirectory.Create();

            return appDirectory;
        }

        public static AppSettings GetAppSettings()
        {
            var appSettingsFileInfo = new FileInfo(GetAppSettingsFilePath());
            if (!appSettingsFileInfo.Exists)
            {
                WriteToAppSettingsFile(new AppSettings() { DownloadDirectory = GetDefaultDownloadDirectory() });
                WriteToAppSettingsFile(new AppSettings() { LogDirectory = GetDefaultLogDirectory() });
            }

            SetDefaultIfSettingsNotSet();

            return JsonConvert.DeserializeObject<AppSettings>(System.IO.File.ReadAllText(GetAppSettingsFilePath())); ;
        }

        private static void SetDefaultIfSettingsNotSet()
        {
            AppSettings appSetting = JsonConvert.DeserializeObject<AppSettings>(System.IO.File.ReadAllText(GetAppSettingsFilePath()));

            var logDirectorySettingsIsSet = appSetting.LogDirectory != null;
            var downloadDirectorySettingsIsSet = appSetting.DownloadDirectory != null;

            if (!logDirectorySettingsIsSet || !downloadDirectorySettingsIsSet)
            {
                var logDirectorySettings = appSetting.LogDirectory ?? GetDefaultLogDirectory();
                var downloadDirectorySettings = appSetting.DownloadDirectory ?? GetDefaultDownloadDirectory();

                WriteToAppSettingsFile(new AppSettings { DownloadDirectory = downloadDirectorySettings, LogDirectory = logDirectorySettings });
            }
        }

        private static string GetDefaultDownloadDirectory()
        {
            string myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            DirectoryInfo downloadDirectory = new DirectoryInfo(Path.Combine(myDocuments, "Geonorge-nedlasting"));
            if (!downloadDirectory.Exists)
                downloadDirectory.Create();

            return downloadDirectory.FullName;
        }

        private static string GetDefaultLogDirectory()
        {
            string myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            DirectoryInfo logDirectory = new DirectoryInfo(Path.Combine(myDocuments, "Log"));
            if (!logDirectory.Exists)
                logDirectory.Create();

            return logDirectory.FullName;
        }

        public static void WriteToAppSettingsFile(AppSettings appSettings)
        {
            var serializer = new JsonSerializer();
            serializer.Converters.Add(new JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Ignore;

            using (var outputFile = new StreamWriter(GetAppSettingsFilePath(), false))
            {
                using (JsonWriter writer = new JsonTextWriter(outputFile))
                {
                    serializer.Serialize(writer, appSettings);
                    writer.Close();
                }
            }
        }

        public static void WriteNewSettingToAppSettingsFile(AppSettings appSettings)
        {
            var serializer = new JsonSerializer();
            serializer.Converters.Add(new JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Ignore;

            using (var outputFile = new StreamWriter(GetAppSettingsFilePath(), true))
            {
                using (JsonWriter writer = new JsonTextWriter(outputFile))
                {
                    serializer.Serialize(writer, appSettings);
                    writer.Close();
                }
            }
        }

        private static string GetAppSettingsFilePath()
        {
            return Path.Combine(GetAppDirectory().FullName, "settings.json");
        }

        public static string DownloadDirectory()
        {
            return GetAppSettings().DownloadDirectory;
        }

        public static string LogDirectory()
        {
            return GetAppSettings().LogDirectory;
        }
    }
}