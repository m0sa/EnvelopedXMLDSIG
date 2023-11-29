# Motivation

This repository contains a narrowed down reporduction of how a version update of the `System.Security.Cryptography.Xml`
package from `6.0.1` to `7.0.0` breaks enveloped XML signature verification for documents that use a `<ds:Reference URI="#xpointer(/)">`.

# Running

To run all test permutations run `docker compose up`. The only prerequirement is a recent docker version running on your machine.

The dockerfile generates a single `sample.xml` which contains an enveloped signature, signed with the private RSA key exported to `key.pem`.
The [Verifier](/Verifier/Program.cs#L16-L29) application tries to load the signed XML and verify it (with and without the private RSA key).
Different combinations of the `System.Security.Cryptography.Xml` and dotnet runtime are used.
The containers also include the [`xmlsig` tool](https://github.com/amdonov/xmlsig) to verify that the generated `sample.xml`` is signed correctly, before attempting to verify it using the .NET `SignedXml` class.

# Debugging

Requires dotnet SDK 6 and SDK 8 installed. Run the following command:

```
dotnet run -p:NugetVersion=6.0.1 -f net6.0 --project .\Verifier\Verifier.csproj -- exampleSigned.xml exampleKey.pem --attach
```

- The `System.Security.Cryptography.Xml` can by modifying the `-p:NugetVersion=<version>` value.
- The `dotnet` runtime can be controlled by modifying the `-f <runtimeVersion>` value.
- The `--attach` switch is optional if you want to attach a debugger.
