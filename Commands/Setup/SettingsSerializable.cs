using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace MePhIt.Commands.Setup
{
    [Serializable]
    public class SettingsSerializable
    {
        public ulong ServerId;
        public string Language;
        public string TimezoneId;

        public void Initialize(in ulong serverId, in LanguageID languageId, in TimeZoneInfo timezone)
        {
            Language = MePhItBot.Bot.Settings.Localization.GetLanguageById(languageId);
            TimezoneId = timezone.Id;
            ServerId = serverId;
        }

        public static void Serialize(SettingsSerializable settings)
        {
            var serializer = new XmlSerializer(typeof(SettingsSerializable));
            var filename = Path.Combine(MePhItBot.Bot.Settings.ServerSettingsFolder, settings.ServerId.ToString());
            using(var fs = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                serializer.Serialize(fs, settings);
                fs.Close();
            }
        }

        public static SettingsSerializable Deserialize(in ulong serverId)
        {
            var serialized = new XmlSerializer(typeof(SettingsSerializable));
            SettingsSerializable settings = null;
            var filename = Path.Combine(MePhItBot.Bot.Settings.ServerSettingsFolder, serverId.ToString());
            using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                settings = serialized.Deserialize(fs) as SettingsSerializable;
                fs.Close();
            }
            return settings;
        }
    }
}
