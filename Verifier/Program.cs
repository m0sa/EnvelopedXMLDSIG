using System.Xml;
using System.Security.Cryptography.Xml;
using System.Security.Cryptography;

if (args.Length < 1)
{
    Console.WriteLine("Usage: Verifier <xml-file> [<private-key-file>] [--attach]");
}


if (args.Length > 2 && args[2] == "--attach")
{
    System.Diagnostics.Debugger.Launch();
}

// load document
var xmlDoc = new XmlDocument();
var xmlContent = File.ReadAllText(args[0]);
xmlDoc.LoadXml(xmlContent);

// get ds:Signature
var signedXml = new SignedXml(xmlDoc);
var signature = xmlDoc.GetElementsByTagName("Signature", SignedXml.XmlDsigNamespaceUrl).OfType<XmlElement>().Single();
signedXml.LoadXml(signature);

// verify signature using embedded keyinfo
var result = true;
         // just for padding                rsaKey
result = PrintInfo(signedXml.CheckSignature(/*  */));

// verify signature  using private key from pem
if (args.Length > 1)
{
    var privateKey = File.ReadAllText(args[1]);
    using var rsaKey = RSA.Create();
    rsaKey.ImportFromPem(privateKey.ToCharArray());
    result = PrintInfo(signedXml.CheckSignature(rsaKey)) && result;
}

if (!result)
{
    System.Environment.Exit(1);
}


static bool PrintInfo(bool result, [System.Runtime.CompilerServices.CallerArgumentExpression(nameof(result))] string? caller = null)
{
    Console.WriteLine(
        "{0}: {1} (System.Security.Cryptography.Xml: {2}, .NET: {3})",
        caller,
        result ? "PASS" : "FAIL",
        System.Diagnostics.FileVersionInfo.GetVersionInfo(typeof(SignedXml).Assembly.Location).ProductVersion,
        Environment.Version);
    return result;
}