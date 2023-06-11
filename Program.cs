using HtmlAgilityPack;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Net;
using System.Text;
using System.IO;
using System.Linq;
using Spectre.Console;
using static System.Console;


// See https://aka.ms/new-console-template for more information

// if (args == null || args.Length == 0)
// {
//     WriteLine("Need a word.");
//     return;
// }

// const string fullUrl = "https://thesaurus.weblio.jp/content/%E7%B5%82%E3%82%8F%E3%82%89%E3%81%AA%E3%81%84";
const string fullUrl = "https://thesaurus.weblio.jp/content/%E5%BF%98%E3%82%8C%E3%82%8B";
HttpClient client = new();
var response = await client.GetStringAsync(fullUrl);

if (response == null)
{
    WriteLine("No response!");
    return;
}

var htmlDocument = new HtmlDocument();
htmlDocument.LoadHtml(response);

var mainDiv = htmlDocument.DocumentNode.SelectSingleNode("//div[@id='main']/div[3]");
var titles = mainDiv.SelectNodes("//h2[@class='midashigo']"); // There can be multiple.
foreach (var title in titles)
{
    var div = title.ParentNode.SelectNodes("//div");
    var nextDiv = title.NextSibling.NextSibling;
    // var table = mainDiv.SelectSingleNode("//table");
    // var table = mainDiv.SelectSingleNode("//div[@id='main']/div[3]//table");
    var table = title.NextSibling.NextSibling.ChildNodes[1];
    // var rows = nextDiv.SelectNodes("//tr");
    var rows = table.ChildNodes.Where(cn => cn.Name == "tr");

    if (!rows.Any())
        continue;

    WriteLine($"\nTITLE: {title.InnerText}");

    var spectreTable = new Table();
    spectreTable.AddColumn("Term");
    spectreTable.AddColumn("Synonyms");

    foreach (var row in rows)
    {
        var cells = row.ChildNodes;
        spectreTable.AddRow(cells[0].InnerText, cells[1].InnerText);
    }

    AnsiConsole.Write(spectreTable);
}
