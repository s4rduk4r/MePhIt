using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using DSharpPlus;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using System.IO;

namespace MePhIt
{
    public class MePhItSettings
    {
        public static MePhItSettings Settings = new MePhItSettings();

        public MePhItLocalization Localization = MePhItLocalization.Localization;
        // Default server language
        public LanguageID LanguageDefault { get; set; }
        public IDictionary<LanguageID, string> LocalizationFilePath { get; private set; } = new Dictionary<LanguageID, string>();
        public IList<DiscordEmoji> EmojiNumbers { get; private set; } = null;
        public DiscordEmoji EmojiReactSuccess { get; private set; } = null;
        public DiscordEmoji EmojiReactFail { get; private set; } = null;
        public DiscordClient Discord { get; private set; }
        private string token = null;

        public IList<string> LoadSettings()
        {
            string emojiReactSuccess;
            string emojiReactFail;
            IList<string> emojiNumbers = new List<string>();
            IList<string> commandPrefixes = new List<string>();
            string webProxyAddress = "";


            (emojiReactSuccess, emojiReactFail, emojiNumbers, commandPrefixes, webProxyAddress) = ReadSettings();
            Discord = new DiscordClient(new DiscordConfiguration()
            {
                Token = this.token,
                TokenType = TokenType.Bot,
                Proxy = webProxyAddress.Length == 0 ? null : new WebProxy(webProxyAddress),
#if DEBUG
                UseInternalLogHandler = true,
                LogLevel = LogLevel.Debug
#endif
            }
            );
            LoadEmojis(emojiReactSuccess, emojiReactFail, emojiNumbers);

            return commandPrefixes;
        }

        private 
            (string emojiReactSuccess, string emojiReactFail, IList<string> emojiNumbers, IList<string> commandPrefixes, string webProxyAddress) 
            ReadSettings()
        {
            var file = new FileStream("settings.json", FileMode.Open, FileAccess.Read);
            var sr = new StreamReader(file);
            var jsonText = sr.ReadToEnd();
            sr.Close();
            file.Close();

            var jsonReader = new JsonTextReader(new StringReader(jsonText));
            var emojiReactSuccess = "";
            var emojiReactFail = "";
            var commandPrefixes = new List<string>();
            var emojiNumbers = new List<string>();
            var webProxyAddress = "";
            var langFolder = "";
            var languages = new List<string>();
            var languagesID = new List<LanguageID>();

            string jsonProperty = "";
            while (jsonReader.Read())
            {
                switch (jsonReader.TokenType)
                {
                    case JsonToken.PropertyName:
                        jsonProperty = jsonReader.Value as string;
                        break;
                    case JsonToken.String:
                        switch (jsonProperty)
                        {
                            case "token":
                                token = jsonReader.Value as string;
                                break;
                            case "command_prefix":
                                commandPrefixes.Add(jsonReader.Value as string);
                                break;
                            case "proxy_settings":
                                webProxyAddress = jsonReader.Value as string;
                                break;
                            case "emoji_success":
                                emojiReactSuccess = jsonReader.Value as string;
                                break;
                            case "emoji_fail":
                                emojiReactFail = jsonReader.Value as string;
                                break;
                            case "emoji_answer":
                                emojiNumbers.Add(jsonReader.Value as string);
                                break;
                            case "mytest_folder":
                                // TODO: MyTest
                                break;
                            case "localization_folder":
                                langFolder = (jsonReader.Value as string);
                                break;
                            case "localization":
                                var value = jsonReader.Value as string;
                                //languages.Add(jsonReader.Value as string);
                                languages.Add(value);
                                switch (value)
                                {
                                    case "ru_RU":
                                        languagesID.Add(LanguageID.ru_RU);
                                        break;
                                    case "en_US":
                                        languagesID.Add(LanguageID.en_US);
                                        break;
                                    default:
                                        break;
                                }
                                break;
                            case "language":
                                switch(jsonReader.Value as string)
                                {
                                    case "ru_RU":
                                        Localization.LanguageFallback = LanguageID.ru_RU;
                                        break;
                                    case "en_US":
                                        Localization.LanguageFallback = LanguageID.en_US;
                                        break;
                                    default:
                                        break;
                                }
                                break;
                        }
                        break;
                }
            }
            jsonReader.Close();

            GetLocalizationFilePaths(langFolder, languages);
            Localization.LoadLocalizations(languagesID);

            return (emojiReactSuccess, emojiReactFail, emojiNumbers, commandPrefixes, webProxyAddress);
        }

        private void GetLocalizationFilePaths(string langFolder, IEnumerable<string> languages)
        {
            foreach (var lang in languages)
            {
                switch (lang)
                {
                    case "ru_RU":
                        LocalizationFilePath[LanguageID.ru_RU] = Path.Combine(langFolder, $"{lang.ToLower()}.json");
                        break;
                    case "en_US":
                        LocalizationFilePath[LanguageID.en_US] = Path.Combine(langFolder, $"{lang.ToLower()}.json");
                        break;
                    default:
                        break;
                }
            }
        }

        private void LoadEmojis(in string emojiReactSuccess, 
                                in string emojiReactFail, in IList<string> emojiNumbers)
        {
            EmojiReactSuccess = DiscordEmoji.FromName(Discord, emojiReactSuccess);
            EmojiReactFail = DiscordEmoji.FromName(Discord, emojiReactFail);
            EmojiNumbers = new List<DiscordEmoji>(emojiNumbers.Count);
            foreach (var emojiNum in emojiNumbers)
            {
                EmojiNumbers.Add(DiscordEmoji.FromName(Discord, emojiNum));
            }
        }
    }
}
