using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TEST_SettingsLib
{
  public partial class Form1 : Form
  {
    public Form1( )
    {
      InitializeComponent( );
    }


    private string settingsFile = "Settings.json";

    private void button1_Click( object sender, EventArgs e )
    {
      AppSettings.InitInstance( settingsFile );

      AppSettings.Instance.ReLoad( );
      RTB.Text = AppSettings.Instance.FormLocation.ToString( ) + "\n";
      RTB.Text += AppSettings.Instance.IntSetting.ToString( ) + "\n";
      RTB.Text += AppSettings.Instance.FloatSetting.ToString( ) + "\n";
      RTB.Text += AppSettings.Instance.BooleanSetting + "\n";
      RTB.Text += AppSettings.Instance.StringSetting + "\n";
      RTB.Text += AppSettings.Instance.EnumSetting.ToString( ) + "\n";

      AppSettings.Instance.FormLocation = this.Location;
      AppSettings.Instance.IntSetting += 400;
      AppSettings.Instance.FloatSetting = (float)Math.PI;
      AppSettings.Instance.BooleanSetting = true;
      AppSettings.Instance.StringSetting = "Another one " + DateTime.Now.ToString();
      AppSettings.Instance.EnumSetting = AppSettings.BlaEnum.Bla2;


      AppSettings.Instance.Save( );
    }

    private void button2_Click( object sender, EventArgs e )
    {
      AppSettings.InitInstance( settingsFile ,@"\Instance Test\");

      AppSettings.Instance.ReLoad( );
      RTB.Text = AppSettings.Instance.FormLocation.ToString( ) + "\n";
      RTB.Text += AppSettings.Instance.IntSetting.ToString( ) + "\n";
      RTB.Text += AppSettings.Instance.FloatSetting.ToString( ) + "\n";
      RTB.Text += AppSettings.Instance.BooleanSetting + "\n";
      RTB.Text += AppSettings.Instance.StringSetting + "\n";
      RTB.Text += AppSettings.Instance.EnumSetting.ToString( ) + "\n";

      AppSettings.Instance.FormLocation = this.Location;
      AppSettings.Instance.IntSetting += 600;
      AppSettings.Instance.FloatSetting = (float)Math.PI* (float)Math.PI;
      AppSettings.Instance.BooleanSetting = !AppSettings.Instance.BooleanSetting;
      AppSettings.Instance.StringSetting = "Another one as instance " + DateTime.Now.ToString( ); 
      AppSettings.Instance.EnumSetting = AppSettings.BlaEnum.IBla5;


      AppSettings.Instance.Save( );
    }

  }
}
