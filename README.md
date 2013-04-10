## MongoDB Incremental Migrations
> Not production code, just a proof of concept. 

Points to be demonstrated:
- what should be done to achieve migration capability without taking database offline?
- what is the overhead (extra object metadata, any slowdowns during deserialization)?
- what should be changed on the C# driver?

### How?

> In order to make the migration process work, [a couple of extension points] (https://github.com/darkiri/mongo-csharp-driver/compare/master...migration) are needed on the official C# driver. It means that micro framework relies on a custom build made from the [forked repository] (https://github.com/darkiri/mongo-csharp-driver/tree/migration).

```C#
[Migration(typeof (MigrationTo_1_0))]
private class Customer
{
    public ObjectId Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}

private class MigrationTo_1_0 : IMigration<Customer>
{
    public Version To
    {
        get { return new Version(1, 0); }
    }

    public void Upgrade(Customer obj, 
                        IDictionary<string, object> extraElements)
    {
        var fullName = (string) extraElements["Name"];

        obj.LastName = fullName.Split().Last();
        obj.FirstName = fullName
                .Substring(0, fullName.Length - obj.LastName.Length)
                .Trim();
        extraElements.Remove("Name");
    }
}
```
In one of the earlier application versions Customer had the only property Name. In v1.0 the Name was splitted into the FirstName and LastName. *MigrationTo_1_0* is applied to the Customer and contains code migrating data from all older versions to the version 1.0.  Migration class contains an *Upgrade* method. It means that it is not possible to downgrade the data to the earlier versions.
