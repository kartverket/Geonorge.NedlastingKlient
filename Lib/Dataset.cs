using System.Collections.Generic;

namespace Geonorge.MassivNedlasting
{
    public class Dataset
    {
        /// <summary>
        /// Dataset title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Dataset description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Dataset uuid
        /// </summary>
        public string Uuid { get; set; }

        /// <summary>
        /// Url to dataset in feed
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Date when dataset was updated
        /// </summary>
        public string LastUpdated { get; set; }

        /// <summary>
        /// Owner of dataset
        /// </summary>
        public string Organization { get; set; }
    }
}
