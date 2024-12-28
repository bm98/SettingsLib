using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SettingsLib
{
  /// <summary>
  /// The Settings catalog 
  ///  Consist of:
  ///    a Version integer (internal SettingLib layout version)
  ///    A Dictionary of Setting Instances
  ///      Key:   InstanceName Hash (prevent non JSON compatible keys)
  ///      Value: Instance Settings object
  ///    
  /// </summary>
  [DataContract]
  internal class AppSettingsCat
  {
    /// <summary>
    /// Json Version
    /// </summary>
    [DataMember( IsRequired = true, Name = "version", Order = 1 )]
    public int LayoutVersion { get; internal set; } = LAYOUT_VERSION;

    /// <summary>
    /// Settings Instances collection
    /// Key= InstanceHash
    /// Value = AppSettingsInstance
    /// </summary>
    [DataMember( IsRequired = true, Name = "instances", Order = 2 )]
    public Dictionary<ulong, AppSettingsInstance> Instances { get; internal set; } = new Dictionary<ulong, AppSettingsInstance>( );


    // non Json

    /// <summary>
    /// The current Json Layout version 
    /// </summary>
    public static int LAYOUT_VERSION = 0;

    /// <summary>
    /// Version Update Hook
    /// </summary>
    /// <param name="cat"></param>
    internal static void VersionUp( AppSettingsCat cat )
    {
      if (cat.LayoutVersion == LAYOUT_VERSION) return; // nothing to do
      
      //  version up chores if there are
    }

    /// <summary>
    /// Load the settings from the Instance Catalog into the provided settings obj
    /// Can be empty if there is no Instance saved or the saved one is still empty
    /// </summary>
    /// <param name="_liveSettings">The settings in use</param>
    /// <param name="instance">The instance name</param>
    /// <returns>True when successful</returns>
    public bool GET( LiveSettings _liveSettings, string instance )
    {
      var iHash = AppSettingsInstance.InstanceHash( instance );
      // don't fail the App, just report
      try {
        // clear live and copy items from instance
        _liveSettings.Clear( );
        if (Instances.TryGetValue( iHash, out AppSettingsInstance iValue )) {
          return iValue.GET( _liveSettings );
        }
        else {
          return true; // no such Instance exists but - returns with an empty cat
        }
      }
      catch (Exception ex) {
        Console.WriteLine( $"AppSettingsCat.GET: Failed - \n" + ex );
      }
      return false;
    }

    /// <summary>
    /// Fill the Instance Catalog from the live settings
    /// </summary>
    /// <param name="_liveSettings">The settings in use</param>
    /// <param name="instance">The instance name</param>
    /// <returns>True when successful</returns>
    public bool PUT( LiveSettings _liveSettings, string instance )
    {
      var iHash = AppSettingsInstance.InstanceHash( instance );
      // don't fail the App, just report
      try {
        // create the Instance if needed
        if (!Instances.ContainsKey( iHash )) {
          Instances.Add( iHash, new AppSettingsInstance( ) { InstanceName = instance, Catalog = new AppSettings( ) } );
        }
        // now we should get one..
        if (Instances.TryGetValue( iHash, out AppSettingsInstance iValue )) {
          return iValue.PUT( _liveSettings );
        }
        else {
          return false; // seems we failed to use the Instance Catalog
        }
      }
      catch (Exception ex) {
        Console.WriteLine( $"AppSettingsCat.PUT: Failed - \n" + ex );
      }
      return false;
    }

  }
}
