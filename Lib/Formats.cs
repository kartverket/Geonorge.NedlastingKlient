using System.Linq;

namespace Geonorge.MassivNedlasting.Gui
{
    public class Formats
    {

        public Formats(dynamic item)
        {
            Epsg = "EPSG:" + item["epsgcode"];
            Name = item["label"];
        }

        public Formats()
        {

        }

        public string Epsg { get; set; }
        public string Name { get; set; }
    }

    public class FormatsViewModel
    {
        public FormatsViewModel(string epsg, string name, bool selected)
        {
            Selected = selected;
            Epsg = epsg;
            Name = name;
        }

        public bool Selected { get; set; }
        public string Epsg { get; set; }
        public string Name { get; set; }
    }
}