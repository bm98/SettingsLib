using System.Collections.Concurrent;

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
