using System.Diagnostics;

namespace AW.Identifiers.Test;


[TestClass]
public class FlakeTests
{
    [TestMethod]
    public void Test()
    {
        var t = Urn.CreateFromNewFlake(["aw", "customer"]);
    }

}