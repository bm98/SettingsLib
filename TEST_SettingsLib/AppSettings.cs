using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SettingsLib;

namespace TEST_SettingsLib
{
  sealed class AppSettings : AppSettingsBaseV2
  {
    // Singleton
    private static Lazy<AppSettings> m_lazy = new Lazy<AppSettings>( ( ) => new AppSettings( ) );
    public static AppSettings Instance { get => m_lazy.Value; }
    private AppSettings( ) { }


    /// <summary>
    /// Init a new instance to use
    /// </summary>
    /// <param name="settingsFile">An filename -> distinct for each instance used</param>
    public static void InitInstance( string settingsFile, string instance = "" )
    {
      m_lazy = new Lazy<AppSettings>( ( ) => new AppSettings( settingsFile, instance ) );
    }
    /// <summary>
    /// cTor: create an instance if such a name is given, else use the default one
    /// </summary>
    /// <param name="instance">An Instance String (defaults to empty = default instance)</param>
    private AppSettings( string settingsFile, string instance )
      : base( settingsFile, instance )
    {
    }


    // Control bound settings
    [DefaultSettingValue( "10,10" )]
    public Point FormLocation {
      get { return (Point)this["FormLocation"]; }
      set { this["FormLocation"] = value; }
    }

    [DefaultSettingValue( "100" )]
    public int IntSetting {
      get { return (int)this["IntSetting"]; }
      set { this["IntSetting"] = value; }
    }

    [DefaultSettingValue( "100.1001" )]
    public float FloatSetting {
      get { return (float)this["FloatSetting"]; }
      set { this["FloatSetting"] = value; }
    }

    [DefaultSettingValue( "True" )]
    public bool BooleanSetting {
      get { return (bool)this["BooleanSetting"]; }
      set { this["BooleanSetting"] = value; }
    }

    [DefaultSettingValue( "String: Hello World" )]
    public string StringSetting {
      get { return (string)this["StringSetting"]; }
      set { this["StringSetting"] = value; }
    }

    public enum BlaEnum { Bla1, Bla2, IBla5 }

    [DefaultSettingValue( "Bla1" )]
    public BlaEnum EnumSetting {
      get { return (BlaEnum)this["EnumSetting"]; }
      set { this["EnumSetting"] = value; }
    }



  }
}
