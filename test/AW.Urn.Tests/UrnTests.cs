namespace AW.Identifiers.Test;


[TestClass]
public class UrnTests
{
    [DataTestMethod]
    [DataRow("urn:aw:foo:12345")]
    [DataRow("urn:aw:foo:12345")]
    [DataRow("urn:aw:foo:12%25345")]
    [DataRow("Urn:AW:foo:@123")]
    [DataRow("Urn:AW:@123")]
    public void TestValidUrnsFromString(string urn)
        => _ = new Urn(urn);

    [DataTestMethod]
    [DataRow("")]
    [DataRow("orn:aw:foo:12345")]
    [DataRow("urn:")]
    [DataRow("urn:a")]
    [DataRow("urn:a:")]
    [DataRow("urn:-a:b")]
    [DataRow("urn:a")]
    [DataRow("urn:foo:/bar:b")]
    public void TestInvalidUrnsFromString(string urn)
        => Assert.ThrowsException<InvalidOperationException>(() => _ = new Urn(urn));

    [DataTestMethod]
    [DataRow("urn:aw:foo:12345", "urn:aw:FOO:12345")]
    [DataRow("Urn:aw:foo:12345", "urn:aw:foo:12345")]
    [DataRow("urn:aw:foo:käse", "urn:aw:foo:k%C3%A4se")]
    [DataRow("urn:aw:foo:k%c3%a4se", "urn:aw:foo:k%C3%A4se")]
    public void TestEquality(string urn1, string urn2)
        => Assert.AreEqual(new Urn(urn1), new Urn(urn2));

}