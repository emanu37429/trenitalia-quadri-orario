using Newtonsoft.Json;
using System.Xml;

// recupero stazioni
var stazs = new XmlDocument();
stazs.Load("./dati/localityTable.xml");
var dict = new Dictionary<int, string>();
foreach (XmlNode node in stazs.DocumentElement.ChildNodes)
    dict.Add(int.Parse(node["idLocality"].InnerText), node["Name"].InnerText);

//recupero stringhe
var stringXml = new XmlDocument();
stringXml.Load("./dati/stringTable.xml");
var stringList = new List<string>();
foreach (XmlNode node in stringXml.DocumentElement.ChildNodes)
    stringList.Add(node.InnerText);


//recupero orari
DirectoryInfo d = new DirectoryInfo("./dati");
FileInfo[] Files = d.GetFiles("*_data.xml");
var listFrTd = new List<FermataTrenoToDo>();
foreach (var file in Files)
{
    var quadro = new XmlDocument();
    quadro.Load(file.FullName);

    var treni = quadro.DocumentElement.ChildNodes[2].ChildNodes;
    var fermate = quadro.DocumentElement.ChildNodes[3].ChildNodes;
    var orariFermate = quadro.DocumentElement.ChildNodes[1].ChildNodes.OfType<XmlNode>().Chunk(fermate.Count).ToList();

    var idfermate = new List<int>();
    foreach (XmlNode fermid in fermate)
    {
        idfermate.Add(BitConverter.ToInt32(Convert.FromBase64String(fermid.InnerText), 5));
    }

    for (int i = 0; i < treni.Count; i++)
    {
        var bytes = Convert.FromBase64String(treni[i].InnerText).Chunk(4).ToArray();
        var indexnum = BitConverter.ToInt32(bytes[1]);
        var num = stringList.ElementAt(indexnum);
        for (int j = 0; j < fermate.Count; j++)
        {
            var bytefermata = Convert.FromBase64String(orariFermate[i][j].InnerText).ToArray();
            listFrTd.Add(new FermataTrenoToDo { num = num, stazione = idfermate[j], tempo = BitConverter.ToInt16(bytefermata, 8) });
        }

    }
}

//raggruppamento orari per treno e generazione lista
var treniFn = listFrTd.GroupBy(x => x.num);
var treniList = new List<Treno>();
var tasks = new List<Task>();
foreach (var treno in treniFn)
{
    tasks.Add(Task.Run(() =>
    {
        var num = treno.First().num;
        var fermate = new List<Fermata>();
        var fermateToDo = treno.Where(x => x.tempo >= 0).OrderBy((x) => x.tempo).GroupBy(x => new { x.stazione });
        for (int i = 0; i < fermateToDo.Count(); i++)
        {
            var cr = fermateToDo.ElementAt(i);
            var crn = cr.Last();
            fermate.Add(new Fermata { id = crn.stazione, nome = dict[crn.stazione], oraPart = $"{((int)crn.tempo / 60).ToString("00")}:{(crn.tempo % 60).ToString("00")}" });
        }
        treniList.Add(new Treno { num = num, fermate = fermate });
    }));
}
await Task.WhenAll(tasks.ToArray());
File.WriteAllText("lista.json", JsonConvert.SerializeObject(treniList));

public class FermataTrenoToDo
{
    public string num;
    public int stazione;
    public short tempo;
}

public class Treno
{
    public string num;
    public List<Fermata> fermate;
}
public class Fermata
{
    public int id;
    public string nome;
    public string oraPart;
}
