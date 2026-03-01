namespace Assura.Domain.Entities;

public class AssetSpecifications
{
    public ComputerSpecs? Computer { get; set; }
    public ServerSpecs? Server { get; set; }
    public NetworkingSpecs? Networking { get; set; }
    public PrintingSpecs? Printing { get; set; }
    public FurnitureSpecs? Furniture { get; set; }
}

public class ComputerSpecs
{
    public string? Display { get; set; }
    public string? RAM { get; set; }
    public string? GPU { get; set; }
    public string? Storage { get; set; }
    public string? OS { get; set; }
}

public class ServerSpecs
{
    public string? OS { get; set; }
    public string? RAM { get; set; }
    public string? CPU { get; set; }
    public string? IPAddress { get; set; }
    public string? Storage { get; set; }
}

public class NetworkingSpecs
{
    public string? PortCount { get; set; }
    public string? DataRate { get; set; }
    public string? FormFactor { get; set; }
    public string? MACAddress { get; set; }
}

public class PrintingSpecs
{
    public string? Type { get; set; }
    public string? PrintingTechnology { get; set; }
    public string? Connectivity { get; set; }
    public string? PrintResolution { get; set; }
}

public class FurnitureSpecs
{
    public string? Material { get; set; }
    public string? Length { get; set; }
    public string? Width { get; set; }
    public string? Height { get; set; }
    public string? Color { get; set; }
    public string? Adjustability { get; set; }
}
