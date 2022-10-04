public class RData
{
    public List<String> Names;
    public List<dynamic> Vectors;

    public RData()
    {
        Names = new List<String>();
        Vectors = new List<dynamic>();
    }
}

public class Data
{
    public string? TestData { get; set; }
    public RData Rdata { get; set; }
    public bool File { get; set; }

    public Data()
    {
        Rdata = new RData();
        File = false;
    }
}