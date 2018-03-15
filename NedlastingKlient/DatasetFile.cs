﻿using System;
using System.Data;
using System.IO;

namespace NedlastingKlient
{

    public class DatasetFile
    {
        private DatasetFileViewModel datasetFileViewModel;
        private const string NorwayDigitalRestricted = "norway digital restricted";
        private const string Restricted = "restricted";
        private const string NoRestrictions = "norway digital restricted";

        public DatasetFile(DatasetFileViewModel datasetFileViewModel)
        {
            Title = datasetFileViewModel.Title;
            Description = datasetFileViewModel.Description;
            Url = datasetFileViewModel.Url;
            LastUpdated = datasetFileViewModel.LastUpdated;
            Organization = datasetFileViewModel.Organization;
            Proportion = datasetFileViewModel.Category;
            DatasetId = datasetFileViewModel.DatasetId;
            DatasetUrl = datasetFileViewModel.DatasetUrl;
            Restrictions = datasetFileViewModel.Restrictions;
        }

        public DatasetFile()
        {
        }

        public string Title { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public string LastUpdated { get; set; }
        public string Organization { get; set; }
        public string Proportion { get; set; }
        public string DatasetId { get; set; }
        public string DatasetUrl { get; set; }
        public string Restrictions { get; set; }

        public string GetId()
        {
            return Title + "_" + Proportion;
        }

        public bool IsRestricted()
        {
            return Restrictions == Restricted || Restrictions == NorwayDigitalRestricted;
        }

        public string LocalFileName()
        {
            var fileName = Path.GetFileName(new Uri(Url).LocalPath);
            var extension = Path.GetExtension(fileName);
            if (string.IsNullOrWhiteSpace(extension))
                return null;

            return fileName;
        }

        public bool HasLocalFileName()
        {
            return !string.IsNullOrWhiteSpace(LocalFileName());
        }
    }

    public class DatasetFileViewModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public string LastUpdated { get; set; }
        public string Organization { get; set; }
        public string Category { get; set; }
        public string DatasetId { get; set; }
        public string DatasetUrl { get; set; }
        public bool SelectedForDownload { get; set; }
        public bool IsRestricted { get; set; }
        public string Restrictions { get; set; }

        public string GetId()
        {
            return DatasetId + "_" + Title + "_" + Category;
        }

        public DatasetFileViewModel(DatasetFile datasetFile, bool selectedForDownload = false)
        {
            Title = datasetFile.Title;
            Description = datasetFile.Description;
            Url = datasetFile.Url;
            LastUpdated = datasetFile.LastUpdated;
            Organization = datasetFile.Organization;
            Category = datasetFile.Proportion;
            DatasetId = datasetFile.DatasetId;
            DatasetUrl = datasetFile.DatasetUrl;
            Id = GetId();
            SelectedForDownload = selectedForDownload;
            IsRestricted = datasetFile.IsRestricted();
            Restrictions = datasetFile.Restrictions;
        }
    }
}
