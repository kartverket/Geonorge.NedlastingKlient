namespace Geonorge.MassivNedlasting.Gui
{
    public class Projections
    {

        public Projections(dynamic item)
        {
            Epsg = "EPSG:" + item["epsgcode"];
            Name = item["label"];
        }

        public string Epsg { get; set; }
        public string Name { get; set; }
    }
}