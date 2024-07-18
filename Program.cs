using HtmlAgilityPack;
using System.Xml;

string url = "http://kotlis.kwst.net/dark/contacts.html"; // Reemplaza con la URL de la página que deseas analizar
string outputFolder = @"C:\PersonalSpace\ExportedSite\Contacts\";

Directory.CreateDirectory(outputFolder);
Directory.CreateDirectory(Path.Combine(outputFolder, "css"));
Directory.CreateDirectory(Path.Combine(outputFolder, "js"));


using (HttpClient client = new HttpClient())
{
    HttpResponseMessage response = await client.GetAsync(url);
    string pageContent = await response.Content.ReadAsStringAsync();

    HtmlDocument htmlDocument = new HtmlDocument();
    htmlDocument.LoadHtml(pageContent);

    // Extraer HTML
    string html = htmlDocument.DocumentNode.OuterHtml;

    // Extraer y guardar CSS
    List<string> stylesheets = new List<string>();
    foreach (HtmlNode link in htmlDocument.DocumentNode.SelectNodes("//link[@rel='stylesheet']"))
    {
        string href = link.GetAttributeValue("href", string.Empty);
        if (Uri.IsWellFormedUriString(href, UriKind.Relative))
        {
            href = new Uri(new Uri(url), href).ToString();
        }
        stylesheets.Add(href);
        string cssContent = await client.GetStringAsync(href);
        string cssFileName = Path.Combine(outputFolder, "css", Path.GetFileName(new Uri(href).LocalPath));
        await File.WriteAllTextAsync(cssFileName, cssContent);
        link.SetAttributeValue("href", $"css/{Path.GetFileName(new Uri(href).LocalPath)}");
    }

    // Extraer y guardar JavaScript
    List<string> scripts = new List<string>();
    foreach (HtmlNode script in htmlDocument.DocumentNode.SelectNodes("//script[@src]"))
    {
        string src = script.GetAttributeValue("src", string.Empty);
        if (Uri.IsWellFormedUriString(src, UriKind.Relative))
        {
            src = new Uri(new Uri(url), src).ToString();
        }
        scripts.Add(src);
        string jsContent = await client.GetStringAsync(src);
        string jsFileName = Path.Combine(outputFolder, "js", Path.GetFileName(new Uri(src).LocalPath));
        await File.WriteAllTextAsync(jsFileName, jsContent);
        script.SetAttributeValue("src", $"js/{Path.GetFileName(new Uri(src).LocalPath)}");
    }
 

    // Guardar HTML actualizado
    string htmlFileName = Path.Combine(outputFolder, "index.html");
    await File.WriteAllTextAsync(htmlFileName, htmlDocument.DocumentNode.OuterHtml);

    Console.WriteLine("Export completed successfully.");
}