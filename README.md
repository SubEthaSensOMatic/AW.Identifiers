# AW.Urn

This repository provides a C# implementation of the Uniform Resource Name (URN) following the [RFC 2141](https://datatracker.ietf.org/doc/html/rfc2141) standard.

## Usage

### Installation

You can add this library to your project via NuGet

- [AW.Urn](https://www.nuget.org/packages/AW.Urn/)

### Creating URNs

Creation from string:
```
using AW.Identifiers;

...

var urn = new Urn("urn:namespace:example:specific-string");
...

```

Creation from namespaces and identifier:
```
using AW.Identifiers;

...
var urn = new Urn("specific-string", ["namespace", "example"]);
...

```

### Serialization

The library includes a JSON converter (`UrnJsonConverter`) and a type converter (`UrnTypeConverter`) for easy integration with .NET serialization.

## License
This project is licensed under the MIT License.