using System;
using System.Collections.Generic;
using System.Text;

namespace SettingsLib
{
  /// <summary>
  /// Specifies the default value for an application settings property.
  ///  Kindly provided by .Net...
  /// </summary>
  [AttributeUsage( AttributeTargets.Property )]
  public sealed class DefaultSettingValueAttribute : Attribute
  {
    
    /// <summary>
    /// Initializes an instance of the DefaultSettingValueAttribute class.
    /// </summary>
    /// <param name="value">A System.String that represents the default value for the property.</param>
    public DefaultSettingValueAttribute( string value )
    {
      Value = value;
    }
    
    /// <summary>
    /// Gets the default value for the application settings property.
    /// </summary>
    public string Value { get; set; }
  }
}