using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace DenisKudelin.Config
{
    /// <summary>
    /// Initializes, loads, and saves configuration for a static class from/to a JSON file.
    /// </summary>
    public class ConfigInitializer : IDisposable
    {
        private readonly Type type;
        private readonly string configPath;
        private readonly Timer autoSaveTimer;
        private readonly int autoSaveInterval;
        private readonly object lockObj = new object();
        private readonly FileStream fileStream;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigInitializer"/> class.
        /// </summary>
        /// <param name="type">The configuration type to load and save.</param>
        /// <param name="configPath">The path to the configuration file.</param>
        /// <param name="autoSaveInterval">The interval in milliseconds for auto-saving the configuration. 0 means no auto-save.</param>
        public ConfigInitializer(Type type, string configPath = "config.json", int autoSaveInterval = 0)
        {
            this.type = type ?? throw new ArgumentNullException(nameof(type));
            this.configPath = Path.IsPathRooted(configPath) ? configPath : Path.Combine(AppContext.BaseDirectory, configPath);
            this.autoSaveInterval = autoSaveInterval;

            this.fileStream = new FileStream(this.configPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);

            this.Load();

            if (this.autoSaveInterval > 0)
            {
                this.autoSaveTimer = new Timer(this.AutoSaveCallback, null, this.autoSaveInterval, Timeout.Infinite);
            }
        }

        private void Load()
        {
            try
            {
                this.fileStream.Seek(0, SeekOrigin.Begin);
                if (this.fileStream.Length > 0)
                {
                    using (var streamReader = new StreamReader(this.fileStream, Encoding.UTF8, false, 8192, true))
                    {
                        var configJson = streamReader.ReadToEnd();
                        var jsonDocument = JsonDocument.Parse(configJson);

                        foreach (var prop in this.type.GetProperties(BindingFlags.Public | BindingFlags.Static))
                        {
                            if (prop.CanWrite && !prop.IsDefined(typeof(ConfigIgnoreAttribute)))
                            {
                                if (jsonDocument.RootElement.TryGetProperty(prop.Name, out var jsonElement))
                                {
                                    var value = JsonSerializer.Deserialize(jsonElement.GetRawText(), prop.PropertyType);
                                    prop.SetValue(null, value);
                                }
                            }
                        }
                    }
                }
                else
                {
                    this.Save();
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error loading configuration.", ex);
            }
        }

        private void AutoSaveCallback(object state)
        {
            lock (this.lockObj)
            {
                try
                {
                    this.Save();
                }
                finally
                {
                    if (this.autoSaveInterval > 0)
                    {
                        this.autoSaveTimer.Change(this.autoSaveInterval, Timeout.Infinite);
                    }
                }
            }
        }

        public void Save()
        {
            try
            {
                this.fileStream.SetLength(0);
                this.fileStream.Seek(0, SeekOrigin.Begin);

                using (var utf8JsonWriter = new Utf8JsonWriter(this.fileStream, new JsonWriterOptions { Indented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping }))
                {
                    utf8JsonWriter.WriteStartObject();

                    foreach (var prop in this.type.GetProperties(BindingFlags.Public | BindingFlags.Static))
                    {
                        if (prop.CanRead && !prop.IsDefined(typeof(ConfigIgnoreAttribute)))
                        {
                            var value = prop.GetValue(null);
                            utf8JsonWriter.WritePropertyName(prop.Name);
                            JsonSerializer.Serialize(utf8JsonWriter, value, prop.PropertyType);
                        }
                    }

                    utf8JsonWriter.WriteEndObject();
                    utf8JsonWriter.Flush();
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error saving configuration.", ex);
            }
        }

        public void Dispose()
        {
            this.autoSaveTimer?.Dispose();
            this.Save();
            this.fileStream?.Dispose();
        }
    }
}