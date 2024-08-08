using System;
using System.Collections.Generic;
using System.Text;

namespace DenisKudelin.Config
{
    /// <summary>
    /// Attribute to mark fields or properties that should be ignored by the configuration initializer.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ConfigIgnoreAttribute : Attribute
    {
    }
}
