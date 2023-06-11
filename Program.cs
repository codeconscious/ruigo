﻿using HtmlAgilityPack;
using System.Net.Http;
using System.Linq;
using Spectre.Console;
using static System.Console;

if (args == null || args.Length != 1)
{
    AnsiConsole.MarkupLine("[yellow]一つの日本語の単語を渡してください。[/]");
    return;
}

var fullUrl = $"https://thesaurus.weblio.jp/content/{args[0]}";

var client = new HttpClient();
string? response;
try
{
    response = await client.GetStringAsync(fullUrl);
    if (response == null)
    {
        WriteLine("No response!");
        return;
    }
}
catch (System.Exception e)
{
    AnsiConsole.MarkupLine($"[red]Network error: {e.Message}[/]");
    return;
}

var htmlDocument = new HtmlDocument();
htmlDocument.LoadHtml(response);
var mainDiv = htmlDocument.DocumentNode.SelectSingleNode("//div[@id='main']/div[3]//h2[@class='midashigo']");
var searchTermHeaders = mainDiv.SelectNodes("//h2[@class='midashigo']"); // There can be multiple.

foreach (var searchTermHeader in searchTermHeaders)
{
    var table = searchTermHeader.NextSibling.NextSibling.ChildNodes[1];
    var rows = table.ChildNodes.Where(cn => cn.Name == "tr");

    if (!rows.Any())
        continue;

    WriteLine($"\nTITLE: {searchTermHeader.InnerText}");

    var spectreTable = new Table();
    spectreTable.AddColumn("単語");
    spectreTable.AddColumn("類語");

    foreach (var row in rows.Skip(1)) // Skip the header row.
    {
        var cells = row.ChildNodes;
        var term = cells[0].InnerText;
        var joinedSynonyms = string.Join("　　", cells[1].ChildNodes[0].ChildNodes.Select(n => n.InnerText));
        spectreTable.AddRow(term, joinedSynonyms);
        spectreTable.AddEmptyRow();
    }

    AnsiConsole.Write(spectreTable);
}
