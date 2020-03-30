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
        /// <summary>
        /// MePhIT global settings
        /// </summary>
        public static MePhItSettings Settings = new MePhItSettings();

        /// <summary>
        /// Localization strings
        /// </summary>
        public MePhItLocalization Localization = MePhItLocalization.Localization;

        /// <summary>
        /// Path to MyTest test files folder
        /// </summary>
        public string MyTestFolder { get; private set; }

        /// <summary>
        /// Default server language
        /// </summary>
        public LanguageID LanguageDefault { get; set; }

        /// <summary>
        /// Path to localization files
        /// </summary>
        public IDictionary<LanguageID, string> LocalizationFilePath { get; private set; } = new Dictionary<LanguageID, string>();

        /// <summary>
        /// Emojis for MyTest test variants and reactions
        /// </summary>
        public IList<DiscordEmoji> EmojiNumbers { get; private set; } = null;

        /// <summary>
        /// Reaction of successful command execution
        /// </summary>
        public DiscordEmoji EmojiReactSuccess { get; private set; } = null;

        /// <summary>
        /// Reaction of failed command execution
        /// </summary>
        public DiscordEmoji EmojiReactFail { get; private set; } = null;

        /// <summary>
        /// MePhIT's discord client
        /// </summary>
        public DiscordClient Discord { get; private set; }

        /// <summary>
        /// Bot token from https://discordapp.com/developers/applications
        /// </summary>
        private string token = null;

        /// <summary>
        /// Prepare bot for a start
        /// </summary>
        /// <returns>List of command prefix symbols</returns>
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

        /// <summary>
        /// Parse settings.json file
        /// </summary>
        /// <returns>Returns tuple with emojis, command prefixes and web proxy strings</returns>
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
                                MyTestFolder = jsonReader.Value as string;
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

        /// <summary>
        /// Gets file paths to the specific localization strings
        /// </summary>
        /// <param name="langFolder">Folder where the localizations are located</param>
        /// <param name="languages">List of languages from the settings file</param>
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

        /// <summary>
        /// Loads emojis for bot reactions and MyTest answer choices
        /// </summary>
        /// <param name="emojiReactSuccess">Bot reaction to successful command execution</param>
        /// <param name="emojiReactFail">Bot reaction to failed command execution</param>
        /// <param name="emojiNumbers">MyTest answer choice emojis</param>
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
