public class RData
{
    public List<String> Names { get; set; }
    public List<dynamic> Vectors { get; }

    public RData()
    {
        Names = new List<String>();
        Vectors = new List<dynamic>();
    }
}

public class Data
{
    public RData RData { get; }
    public bool File { get; set; }
    public string Func { get; set; }

    public Data()
    {
        RData = new RData();
        File = false;
        Func = "";
    }
}
