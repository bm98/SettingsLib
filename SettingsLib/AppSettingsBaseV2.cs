using System;
using System.Linq;
using System.IO;
using System.Reflection;
using System.ComponentModel;

namespace SettingsLib
{
  /// <summary>
  /// Simple AppSettings Base Class (appended V2 to avoid clash with the .Net one)
  ///  stores Objects with string keys
  ///  similar to the AppSettings by .Net but uses Json and used defined locations
  /// </summary>
  [DefaultProperty( "Item" )]
  public abstract class AppSettingsBaseV2
  {
    // how many backups to maintain
    private const int c_MaxBackupGeneration = 5;

    // instance settings, a concurrent Dict, should allow for threaded apps
    private LiveSettings _liveSettings = new LiveSettings( );

    // filename + path 
    private string _settingsFile = "";

    // the active instance within the settings file
    private string _instance = "";

    // all settings in the file
    private AppSettingsCat _settings = null;

    // true if the file path etc is valid and usable
    private bool _valid = false;

    /// <summary>
    /// Returns the status of the AppSettings 
    /// </summary>
    public bool Valid => _valid;

    /// <summary>
    /// cTor: empty  cannot proceed
    /// </summary>
    public AppSettingsBaseV2( )
    {
      _valid = false;
    }

    /// <summary>
    /// cTor: 
    /// </summary>
    /// <param name="settingsFile">The Settings file+path</param>
    /// <param name="instance">An Instance Name (default is empty name)</param>
    /// <param name="maintainBackups">Backup Flag</param>
    public AppSettingsBaseV2( string settingsFile, string instance = "", bool maintainBackups = true )
    {
      _settingsFile = Path.GetFullPath( settingsFile );
      _instance = instance;
      _valid = InitSettings( _settingsFile, _instance, maintainBackups );
    }

    // move the backups one generation up
    private void MoveBackup( string settingsFile )
    {
      var sPath = Path.GetFullPath( settingsFile );
      var sName = Path.GetFileName( settingsFile );
      // move 4->5, 3->4,.. 1->2
      for (int gen = c_MaxBackupGeneration - 1; gen > 0; gen--) {
        var bN = settingsFile + $".{gen}";
        var bN1 = settingsFile + $".{gen + 1}";
        // never fail
        try {
          var bNtime = File.GetLastWriteTime( bN );
          if (File.Exists( bN )) File.Copy( bN, bN1, true ); // move one up
          File.SetLastWriteTime( bN1, bNtime ); // maintain the original write time
        }
        catch { }
      }
    }
    
    // BackupName: Setting.ext -> Setting.ext.1 .. 5
    // newest is always Setting.ext.1
    // max number of backup generations is maintained
    private void BackupSettingsFile( string settingsFile )
    {
      if (File.Exists( settingsFile )) {
        MoveBackup( settingsFile ); // move the old ones up
        var bN1 = settingsFile + ".1"; // the newest is 1, 
        // never fail
        try {
          var bNtime = File.GetLastWriteTime( settingsFile );
          File.Copy( settingsFile, bN1, true );
          File.SetLastWriteTime( bN1, bNtime ); // maintain the original write time
        }
        catch { }
      }
    }

    /// <summary>
    /// Initialize the Setting with a file
    /// Backup the current one if it exists
    /// </summary>
    /// <param name="settingsFile">Settings file (fully qualified path)</param>
    /// <param name="instance">An Instance Name</param>
    /// <param name="maintainBackups">Backup Flag</param>
    /// <returns>True if successfull</returns>
    private bool InitSettings( string settingsFile, string instance, bool maintainBackups )
    {
      bool retValid = false;
      _settings = new AppSettingsCat( );

      if (File.Exists( settingsFile )) {
        // there is one, try if it works
        retValid = DiskLoad_low( instance, true ); // Load to live
        if (retValid) {
          if (maintainBackups) {
            BackupSettingsFile( settingsFile );
          }
        }
        else {
          // failed due to not beeing a valid Settings file - delete it
          try {
            File.Delete( settingsFile );
          }
          catch {
            ; // don't bother, just don't fail
          }
        }
      }
      // if it is still not valid - try to create the initial one
      if (!retValid) {
        _settings = new AppSettingsCat( ); // was possibly destroyed ..
        // check the settings directory
        if (Directory.Exists( Path.GetDirectoryName( settingsFile ) )) {
          retValid = DiskUpdate_low( instance, true ); // can we write the first default one ??
        }
      }

      if (!retValid) {
        Console.WriteLine( $"AppSettingsBase.cTor: failed to initialize the AppSettings" );
      }
      return retValid;
    }

    // the update function called from the Json formatter
    // we get the current settings from the file and must apply and return the updated settings
    // note we may get settings which don't belong to our current instance (don't mess with them..)
    private AppSettingsCat UpdateFunction( AppSettingsCat oldData )
    {
      if (oldData != null) {
        // set the current one from what was just read
        _settings = oldData;
      }
      // update with our live settings and return for writing
      _settings.PUT( _liveSettings, _instance );
      return _settings;
    }

    // Low Level Disk Update of the Catalog
    private bool DiskUpdate_low( string instance, bool force = false )
    {
      if (!force && !_valid) return false;

      // we need to read the cat, update the LiveValues and write it back
      // so other instances updates are included in the settings file
      try {
        return Formatter.UpdateJsonFile<AppSettingsCat>( _settingsFile, UpdateFunction );
      }
      catch (Exception ex) {
        Console.WriteLine( $"AppSettingsBase.Save_low: Failed - \n" + ex );
      }
      return false;
    }

    // Low Level Load of the Catalog
    private bool DiskLoad_low( string instance, bool force = false )
    {
      if (!force && !_valid) return false;

      // this will cancel changes in the LiveSettings after the last Save
      try {
        _settings = Formatter.FromJsonFile<AppSettingsCat>( _settingsFile );
        if (_settings == null) return false;

        return _settings.GET( _liveSettings, instance );
      }
      catch (Exception ex) {
        Console.WriteLine( $"AppSettingsBase.Load_low: Failed - \n" + ex );
      }
      return false;
    }

    // try to find the property info for THIS type in the given assembly
    private PropertyInfo GetPInfo( string propName, Assembly caller )
    {
      var tn = this.GetType( ).Name;
      // try to find the calling Property in the calling Type(class) 
      PropertyInfo pInfo = null;
      //20221109 - the calling Type must be checked else it may find other Properties with the matching name
      var defType = caller.DefinedTypes.FirstOrDefault( x => x == this.GetType( ) );
      pInfo = defType?.GetProperty( propName );
      return pInfo;
    }

    /// <summary>
    /// Get;Set: The indexed item as object
    /// 
    /// Exceptions:
    ///  InvalidOperationException: if the Provider is in invalid state (filename or permission problem)
    ///  ArgumentException: if the Setting cannot be evaluated
    ///  CustomAttributeFormatException: if the DefaultValue Attribute is missing
    ///  ArgumentException: if the Setting Type has no conversion and cannot be serialized
    ///  
    /// </summary>
    /// <param name="sKey">Settings Key - ignored we use the calling Member but need an argument for the Indexer</param>
    /// <param name="memberName">Auto Generated: calling member</param>
    /// <returns>The object or null</returns>
    public object this[string sKey, [System.Runtime.CompilerServices.CallerMemberName] string memberName = ""] {
      get {
        if (!_valid) throw new InvalidOperationException( "AppSettings is in invalid state, can be a filename or permission issue" );

        // try to find the calling Property in the calling Type(class) 
        PropertyInfo pInfo = GetPInfo( memberName, Assembly.GetCallingAssembly( ) );
        if (pInfo == null) {
          // no success above
          throw new ArgumentException( $"Cannot evaluate the Setting: {memberName}" );
        }
        // get the string value to convert and return
        string strValue;
        // get the stored value or a default one
        if (_liveSettings.TryGetValue( memberName, out string value )) {
          strValue = value;
        }
        else {
          // try to get the default attribute 
          var attr = pInfo.GetCustomAttribute<DefaultSettingValueAttribute>( );
          if (attr == null) {
            // there is no Default Attribute..
            throw new CustomAttributeFormatException( $"Missing DefaultSettingValue for Setting: {memberName}" );
          }
          strValue = attr.Value;
        }

        // shortcut if it alread a string type
        if (pInfo.PropertyType == typeof( string )) {
          return strValue;
        }
        else {
          // try to convert from string to the Property type if there is a TypeConverter
          if (!TypeDescriptor.GetConverter( pInfo.PropertyType ).IsValid( strValue )) {
            throw new ArgumentException( $"Value <{strValue}> cannot be converted for Setting: {memberName}" );
          }
          // finally
          return TypeDescriptor.GetConverter( pInfo.PropertyType ).ConvertFromInvariantString( strValue );
        }

      }

      set {
        if (!_valid) return;

        // try to find the calling Property in the calling Type(class) 
        PropertyInfo pInfo = GetPInfo( memberName, Assembly.GetCallingAssembly( ) );
        if (pInfo == null) {
          // no success above
          throw new ArgumentException( $"Cannot evaluate the Setting: {memberName}" );
        }
        // shortcut if it alread a string type
        var strValue = "";
        if (pInfo.PropertyType == typeof( string )) {
          strValue = (string)value;
        }
        else {
          // convert from type to a string value and store it
          strValue = TypeDescriptor.GetConverter( pInfo.PropertyType ).ConvertToInvariantString( value );
        }

        _liveSettings.AddOrUpdate( memberName, strValue, ( k, v ) => strValue );
      }
    }

    /// <summary>
    /// Reload the Settings from the Disk
    /// Note: this will cancel all changes done after the last Save
    /// </summary>
    public void Reload( )
    {
      if (!_valid) return;

      DiskLoad_low( _instance );
    }

    /// <summary>
    /// Save Settings to Disk
    /// </summary>
    public void Save( )
    {
      if (!_valid) return;

      DiskUpdate_low( _instance );
    }

  }
}
