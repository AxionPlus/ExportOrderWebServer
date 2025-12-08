using System.Xml.Serialization;

namespace ExportOrderWebServer.Areas.ImportDocument.Dto;

// XML модели

[XmlRoot("Manifest")]
public class ManifestModel
{
    public DocHead DocHead { get; set; }
    public string Line { get; set; }
    public string Carrier { get; set; }
    public PartiesList PartiesList { get; set; }
    public Arrival Arrival { get; set; }

    [XmlElement("BLsList")]
    public BLsList BLsList { get; set; }
}

public class DocHead
{
    public string DocName { get; set; }
    public string DocNumber { get; set; }

    [XmlElement(DataType = "date")]
    public DateTime? DocDate { get; set; }

    public string Modification { get; set; }

    [XmlElement(IsNullable = true)]
    public string? HouseNumber { get; set; }

    [XmlElement(IsNullable = true)]
    public string? Carrier { get; set; }
}

public class PartiesList
{
    [XmlElement("Party")]
    public List<Party> Parties { get; set; }
}

public class Party
{
    public string PartyFunction { get; set; }
    public string Name { get; set; }

    [XmlElement(IsNullable = true)]
    public string? ContractID { get; set; }
}

public class Arrival
{
    public string VesselID { get; set; }
    public string Vessel { get; set; }

    [XmlElement(DataType = "date")]
    public DateTime? ArrivalDate { get; set; }
}

public class BLsList
{
    [XmlElement("BL")]
    public List<BL> BLs { get; set; }
}

public class BL
{
    public DocHead DocHead { get; set; }
    public PartiesList PartiesList { get; set; }
    public string OceanLineID { get; set; }
    public string OriginID { get; set; }
    public ContainerList ContainerList { get; set; }
}

public class ContainerList
{
    [XmlElement("Container")]
    public List<Container> Containers { get; set; }
}

public class Container
{
    public string Prefix { get; set; }
    public string Number { get; set; }
    public string ISOType { get; set; }
    public bool IsEmpty { get; set; }
    public string TareWeight { get; set; }
    public string CargoName { get; set; }
    public int CargoPlaces { get; set; }
    public string CargoWeight { get; set; }
    public SealList SealList { get; set; }

    [XmlElement(IsNullable = true)]
    public TemperatureCondition? TemperatureCondition { get; set; }

    [XmlElement(IsNullable = true)]
    public DangerClasses? DangerClasses { get; set; }
}

public class SealList
{
    [XmlElement("Seal")]
    public List<string> Seals { get; set; }
}

public class TemperatureCondition
{
    public string Value { get; set; }
    public string Unit { get; set; }
}

public class DangerClasses
{
    [XmlElement("DangerClass")]
    public List<DangerClass> DangerClassesList { get; set; }
}

public class DangerClass
{
    public string IMO_class { get; set; }
    public string UN { get; set; }
}