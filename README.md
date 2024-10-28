# AW.Identifiers

#### [Flakes](#Flake)
#### [URNs](#URN)

## Installation

You can add this library to your project via NuGet

- [AW.Identifiers](https://www.nuget.org/packages/AW.Identifiers/)

## Flake

Flake IDs provide compact, unique, and time-orderable IDs, ideal for distributed systems. Each ID encodes a timestamp (48 bits), machine ID (4 bits), and sequence number (11 bits), ensuring both uniqueness and natural ordering. Flake IDs are generated efficiently by a Factory, which maintains sequential IDs per machine and prevents collisions by regulating sequences and checking time consistency. These IDs are shorter and more storage-efficient than UUIDs and can be converted to custom base representations (e.g., Base62) for additional flexibility and shorter IDs, making them perfect for scalable, high-throughput systems needing sortable, readable IDs.

### Single machine example

```
using System;
using AW.Identifiers;

class Program
{
    static void Main()
    {
        // Create a single FlakeFactory instance (default machine ID 0)
        var factory = FlakeFactory.Instance;

        // Generate a single Flake ID
        var flakeId = factory.NewFlake();
        Console.WriteLine($"Generated Flake ID: {flakeId}");
        Console.WriteLine($"Machine ID: {flakeId.MachineId}, Sequence: {flakeId.SequenceNumber}, Timestamp: {flakeId.Time}");

        // Generate multiple Flake IDs sequentially
        Console.WriteLine("\nGenerating multiple Flake IDs:");
        for (int i = 0; i < 5; i++)
        {
            var id = factory.NewFlake();
            Console.WriteLine($"ID: {id} | Machine ID: {id.MachineId} | Sequence: {id.SequenceNumber} | Time: {id.Time}");
        }

        // Example of converting Flake ID to a Base62 string representation
        var flakeIdBase62 = flakeId.ToBase62();
        Console.WriteLine($"\nBase62 representation of Flake ID: {flakeIdBase62}");
    }
}

```

**Explanation**

- **Single Factory Instance:** `FlakeFactory.Instance` creates the `Flake` IDs using the default machineId (0). This setup is simple and suited for cases where only one instance of the application generates IDs.
- **Single ID Generation:** A single ID is generated and displayed with details such as Machine ID, Sequence, and Timestamp.
- **Multiple IDs:** The loop generates multiple IDs in sequence, showing how `Flake` IDs are unique and ordered over time.
- **Base62 Conversion:** Converts a `Flake` ID to a shorter, Base62-encoded string, which is handy for compact ID storage or transmission.

### Two or more service services example

```
using System;
using System.Threading;
using AW.Identifiers;

class Program
{
    static void Main()
    {
        // Example: Two services generating Flake IDs with different machine IDs
        var factoryService1 = new FlakeFactory(1); // Machine ID 1
        var factoryService2 = new FlakeFactory(2); // Machine ID 2

        // Generate IDs in parallel from both services
        Console.WriteLine("Service 1 - Flake IDs:");
        for (int i = 0; i < 5; i++)
        {
            var flakeId1 = factoryService1.NewFlake();
            Console.WriteLine($"ID: {flakeId1} | Machine ID: {flakeId1.MachineId} | Time: {flakeId1.Time}");
        }

        Console.WriteLine("\nService 2 - Flake IDs:");
        for (int i = 0; i < 5; i++)
        {
            var flakeId2 = factoryService2.NewFlake();
            Console.WriteLine($"ID: {flakeId2} | Machine ID: {flakeId2.MachineId} | Time: {flakeId2.Time}");
        }
    }
}

```

**Explanation**

- **Factory Setup:** Each service must have its own `FlakeFactory` instance, with a unique machineId (1 for factoryService1 and 2 for factoryService2). This setup prevents collisions since each machine ID is unique.
- **ID Generation:** Each loop generates and displays a `Flake` ID, including its MachineId and Time, which reflect the machine ID used by the respective service.


## URN

URN (Uniform Resource Name) class provides a robust way to create unique and human-readable identifiers that inherently convey information about the type or namespace of an object. Unlike UUIDs or auto-incremented values, which are generic and carry no type information, URNs allow for a discriminator—a namespace within the identifier that specifies the type or context. This feature makes URNs particularly valuable in systems that need structured, semantically meaningful identifiers, such as APIs, microservices, and distributed applications, where object type distinction is essential. By following RFC 2141 standards, the URN class ensures compatibility and interoperability across systems.

### Examples

#### 1. Basic URN Creation

Create a URN using namespace identifiers (NIDs) and a namespace-specific string (NSS).

```
using AW.Identifiers;

class Program
{
    static void Main()
    {
        // Create a URN with NIDs and NSS
        var urn = new Urn("12345", new[] { "my-app", "customer" });
        Console.WriteLine(urn); // Outputs: urn:my-app:customer:12345
    }
}
```

#### 2. Creating a URN from a GUID

Use a `Guid` as the NSS and provide NIDs.

```
var guidUrn = Urn.CreateFromGuid(Guid.NewGuid(), new[] { "my-app", "user" });
Console.WriteLine(guidUrn); // Outputs a URN in the form urn:my-app:user:<guid>
```

#### 3. Generating a URN with a Flake ID as the NSS

Generate a URN using a Flake ID as the NSS for compact, unique identifiers.

```
var flakeUrn = Urn.CreateFromNewFlake(new[] { "my-app", "offer" });
Console.WriteLine(flakeUrn); // Outputs a URN in the form urn:my-app:offer:<flakeId>
```

#### 4. Parsing and Validating a URN String

Parse a URN string to create an instance of the `Urn` struct and check if it’s valid.

```
var urnString = "urn:my-app:order:12345";
if (Urn.TryParse(urnString, out var parsedUrn))
{
    Console.WriteLine("Parsed URN: " + parsedUrn);
}
else
{
    Console.WriteLine("Invalid URN format.");
}
```

#### 5. Working with URN Components

Access the namespace identifiers (NIDs) and the namespace-specific string (NSS) directly.

```
var urn = new Urn("12345abc", new[] { "company", "my-app", "project" });
Console.WriteLine("URN: " + urn);                      // urn:company:my-app:project:12345abc
Console.WriteLine("Namespace Count: " + urn.NIDCount); // 3
Console.WriteLine("First NID: " + urn[0].ToString());  // company
Console.WriteLine("NSS: " + urn.NSS.ToString());       // 12345abc
```

## License
This project is licensed under the MIT License.
