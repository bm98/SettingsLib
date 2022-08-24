using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace SettingsLib
{
  /// <summary>
  /// Type of the Live Settings
  ///   concurrent Dict to allow threaded Apps to use SettingLib
  /// </summary>
  internal class LiveSettings : ConcurrentDictionary<string, string>
  {
  }
}
