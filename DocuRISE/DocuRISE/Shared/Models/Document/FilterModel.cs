namespace DocuRISE.Shared.Models.Document
{
    public class FilterModel
    {
        public string FilteredByFileName { get; set; }
        public string FilteredByCompanyName { get; set; }
        public string FilteredByType { get; set; }
        public string FilteredByStatus { get; set;}

        public FilterModel(string filteredByFileName, string filteredByCompanyName, string filteredByType, string filteredByStatus )
        {
            FilteredByFileName = filteredByFileName;
            FilteredByCompanyName = filteredByCompanyName;
            FilteredByType = filteredByType;
            FilteredByStatus = filteredByStatus;
        }
    }
}
