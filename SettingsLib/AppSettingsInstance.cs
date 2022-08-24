using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Collections.Concurrent;

namespace SettingsLib
{
  /// <summary>
  /// The Settings Instance
  /// </summary>
  [DataContract]
  internal class AppSettingsInstance
  {
    /// <summary>
    /// The Instance name as given by the caller of the Library
    /// </summary>
    [DataMember( IsRequired = true, Name = "instance", Order = 1 )]
    public string InstanceName { get; internal set; } = "";

    /// <summary>
    /// Settings Catalog for this instance
    /// </summary>
    [DataMember( IsRequired = true, Name = "settings", Order = 2 )]
    public AppSettings Catalog { get; internal set; } = new AppSettings( );


    // non Json

    /// <summary>
    /// Hash function to reduce the Instance name to a unsigned long
    /// </summary>
    /// <param name="instance">The instance string</param>
    /// <returns></returns>
    public static ulong InstanceHash( string instance )
    {
      instance += "@@$$SALT||salt$$__"; // just make the instance a min length string for empty or short strings
      // Knuth hash
      ulong hashedValue = 3074457345618258791ul;
      for (int i = 0; i < instance.Length; i++) {
        hashedValue += instance[i];
        hashedValue *= 3074457345618258799ul;
      }
      return hashedValue;
    }

    /// <summary>
    /// Get the settings from the Instance Catalog
    /// </summary>
    /// <param name="_liveSettings"></param>
    public bool GET( ConcurrentDictionary<string, string> _liveSettings )
    {
      // don't fail the App, just report
      try {
        _liveSettings.Clear( );
        foreach (var kv in Catalog) {
          _liveSettings.AddOrUpdate( kv.Key, kv.Value, ( k, v ) => kv.Value );
        }
        return true;
      }
      catch (Exception ex) {
        Console.WriteLine( $"AppSettingsInstance.GET: Failed - \n" + ex );
      }
      return false;
    }

    /// <summary>
    /// Fill the Instance Catalog from the live settings
    /// </summary>
    /// <param name="_liveSettings"></param>
    public bool PUT( ConcurrentDictionary<string, string> _liveSettings )
    {
      // don't fail the App, just report
      try {
        // clear and copy items from live to instance
        Catalog.Clear( );
        foreach (var kv in _liveSettings) {
          Catalog.Add( kv.Key, kv.Value );
        }
        return true;
      }
      catch (Exception ex) {
        Console.WriteLine( $"AppSettingsInstance.PUT: Failed - \n" + ex );
      }
      return false;
    }
  }
}
