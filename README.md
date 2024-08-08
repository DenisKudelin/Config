# Config.NET
Configuration management in .NET

## Overview
`DenisKudelin.Config` is a .NET library for managing configuration settings efficiently. It automates the loading and saving of settings to a JSON file and supports auto-saving at configurable intervals.

## Installation
To install via NuGet, use the following command:
```bash
Install-Package DenisKudelin.Config
```
Or via .NET CLI:
```bash
dotnet add package DenisKudelin.Config
```

## Usage

### Define Configuration Class
Define a static class for your settings:
```csharp
using DenisKudelin.Config;

public static class AppConfig
{
    public static string StoreOwner { get; set; } = "John Doe";
    public static List<string> Fruits { get; set; } = new() { "apple", "banana", "pear" };

    [ConfigIgnore]
    public static string IgnoredProperty { get; set; }
}
```

### Initialize and Manage Configuration
Initialize and use `ConfigInitializer`:
```csharp
public class Program
{
    public static void Main()
    {
        using var configInitializer = new ConfigInitializer(typeof(AppConfig), "config.json", 5000);

        Console.WriteLine($"Hi there, my name is {AppConfig.StoreOwner}!");
        AppConfig.Fruits.ForEach(Console.WriteLine);

        AppConfig.StoreOwner = "Jane Smith";
        AppConfig.Fruits.Add("orange");

        Thread.Sleep(10000); // Demonstrate auto-saving
    }
}
```

## ConfigInitializer Class

### Constructors
```csharp
public ConfigInitializer(Type type, string configPath = "config.json", int autoSaveInterval = 0)
```

### Methods
- **Load()**: Loads settings from a JSON file.
- **Save()**: Saves current settings to a JSON file.
- **Dispose()**: Releases resources and saves settings.

## Attributes

### ConfigIgnoreAttribute
Exclude properties from being saved or loaded:
```csharp
[AttributeUsage(AttributeTargets.Property)]
public class ConfigIgnoreAttribute : Attribute
{
}
```
