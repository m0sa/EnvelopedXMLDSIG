// See https://aka.ms/new-console-template for more information
using System.Xml;
using System.Security.Cryptography.Xml;
using System.Security.Cryptography;

// inline dummy document
var xmlDocument = new XmlDocument();
xmlDocument.LoadXml(
"<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" +
"<hello>\r\n" +
"  <world>Hi</world>\r\n" +
"</hello>\r\n");

// inline random certificate
var key = RSA.Create();
key.KeySize = 1024;

  // Private key export.
Console.Error.Write("-----BEGIN PRIVATE KEY-----\n");
var privateKey = Convert.ToBase64String(key.ExportPkcs8PrivateKey());
for (var i = 0; i < privateKey.Length; i += 64)
{
    Console.Error.Write(privateKey.Substring(i, Math.Min(64, privateKey.Length - i)));
    Console.Error.Write("\n");
}
Console.Error.Write("-----END PRIVATE KEY-----");


var reference = new Reference { Uri = "#xpointer(/)" };
reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
reference.AddTransform(new XmlDsigExcC14NTransform(includeComments: true));
reference.DigestMethod = SignedXml.XmlDsigSHA256Url;

var signedXml = new SignedXml(xmlDocument);
signedXml.SignedInfo.SignatureMethod = SignedXml.XmlDsigRSASHA256Url;
signedXml.SignedInfo.CanonicalizationMethod = SignedXml.XmlDsigExcC14NWithCommentsTransformUrl;

signedXml.KeyInfo = new KeyInfo();
signedXml.KeyInfo.AddClause(new RSAKeyValue(key));
signedXml.AddReference(reference);

// sign
signedXml.SigningKey = key;
signedXml.ComputeSignature();

// write output document to stdout
var signature = signedXml.GetXml();
xmlDocument.DocumentElement!.AppendChild(xmlDocument.ImportNode(signature, true));
xmlDocument.WriteTo(new XmlTextWriter(Console.Out));
