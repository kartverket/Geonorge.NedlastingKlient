namespace Geonorge.MassivNedlasting
{
    /// <summary>
    /// Holds various application settings.
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// Where the downloaded files will be written.
        /// </summary>
        public string DownloadDirectory { get; set; }
        public string LogDirectory { get; set; }
        
        public string Username { get; set; }
        public string Password { get; set; }

    }
}