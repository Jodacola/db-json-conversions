DbJsonConversions includes a simple attribute and some extension methods for .NET Core Entity Framework's DbContext class, which allows you to configure JSON serialization and deserialization of complex types on model properties by applying `JsonConversionAttribute` and calling a single method in the DbContext's OnModelCreating override.

### Prerequisites
See src/DbJsonConversions.csproj for the minimal dependencies.  This project is built on top of .NET Core 2.1-related packages for Entity Framework, and uses the latest 11.x releases of Newtonsoft.Json.

### Usage
**Configuring your entities**
Simply add the `[JsonConversion]` attribute to the complex properties you'd like to be automatically serialized and deserialized:

```
    public class YourModel
    {
        [Key]
        public long Id { get; set; }

        public string Name { get; set; }

        [JsonConversion]
        public Dictionary<string, string> YourComplexProperty { get; set; }
    }
```

**Setting up the value conversions**
In your DbContext's OnModelCreating override, simply issue a call to `this.AddJsonConversions(modelBuilder)`.  This should be done directly after the call to `base.OnModelCreating(modelBuilder)` in order for any inherited behavior to have had a chance to run, such as the additional tables added by the `IdentityDbContext` class found in Microsoft.AspNetCore.Identity.EntityFrameworkCore.

This call should also be done before any other logic that modifies entities and their properties, such as the functionality provided by the [DbUnderscorer](https://github.com/Jodacola/db-underscorer) project.

```
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            this.AddJsonConversions(modelBuilder);
        }
```

### What's next?
* A NuGet package is on deck next, for simple integrations.
* The addition of more customizability of the JSON serialization and deserialization.
* Explore how this plays with other data annotations.