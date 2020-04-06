using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using DSharpPlus.Entities;

namespace MePhIt
{
    public enum LanguageID
    {
        ru_RU,
        en_US
    };

    public enum MessageID
    {
        ClassStarted,
        ClassEndingNotify,
        ClassEnded,
        BreakStarted,
        BreakEnded,
        ListHeader,
        ListPresent,
        ListAbsent,
        CmdSrvrOrgChannelNameRules,
        CmdSrvrOrgChannelNameInfo,
        CmdSrvrOrgCategoryNameClass,
        CmdSrvrOrgChannelNameChat,
        CmdSrvrOrgChannelNameCommon,
        CmdSrvrOrgChannelNameSubmit,
        CmdSrvrOrgCategoryNameControl,
        CmdSrvrOrgChannelNameCommands,
        CmdSrvrOrgRoleTeacher,
        CmdSrvrOrgRoleGroupLeader,
        CmdSrvrOrgRoleStudent,
        CmdMyTestTestsSearch,
        CmdMyTestTestsNotFound,
        CmdMyTestFileLoadSuccess,
        CmdMyTestStartHelp,
        CmdMyTestStartTime,
        CmdMyTestStartTempCategoryName,
        CmdMyTestStartTestFinished,
        CmdMyTestStartTestQuestionResult,
        CmdMyTestStartTestTotalResults,
        CmdMyTestMark,
        CmdMyTestMark0,
        CmdMyTestMark1,
        CmdMyTestMark2,
        CmdMyTestMark3,
        CmdMyTestMark4,
        CmdMyTestMark5,
        CmdMyTestMark100,
        CmdMyTestResultsLink
    }

    public class MePhItLocalization
    {
        public static MePhItLocalization Localization = new MePhItLocalization();

        /// <summary>
        /// Localization strings holder
        /// </summary>
        private IDictionary<LanguageID, IDictionary<MessageID, string>> localizedMessages = new ConcurrentDictionary<LanguageID, IDictionary<MessageID, string>>();
        
        /// <summary>
        /// Default language settings
        /// </summary>
        public LanguageID LanguageFallback { get; private set; } = LanguageID.en_US;
        
        /// <summary>
        /// Server specific language settings
        /// </summary>
        public IDictionary<DiscordGuild, LanguageID> Language { get; set; } = new ConcurrentDictionary<DiscordGuild, LanguageID>();
        
        /// <summary>
        /// Supported languages IDs
        /// </summary>
        public IList<LanguageID> LanguageIDs { get; private set; } = null;

        /// <summary>
        /// Get supported language strings in xx_YY format. ru_RU, en_US, en_UK, etc.
        /// </summary>
        /// <returns></returns>
        public IList<string> GetLanguages()
        {
            var languages = new List<string>();
            foreach(var langId in LanguageIDs)
            {
                switch(langId)
                {
                    case LanguageID.ru_RU:
                        languages.Add("ru");
                        break;
                    case LanguageID.en_US:
                        languages.Add("en");
                        break;
                }
            }
            return languages.Count != 0 ? languages : null ;
        }

        /// <summary>
        /// Converts language represenation string into language id
        /// </summary>
        /// <param name="language">Language string in xx_YY format. E.g. ru_RU, en_US, en_UK, etc.</param>
        /// <returns></returns>
        public LanguageID GetLanguageId(in string language)
        {
            LanguageID value = LanguageFallback;
            switch(language)
            {
                case "ru":
                    value = LanguageID.ru_RU;
                    break;
                case "en":
                    value = LanguageID.en_US;
                    break;
            }
            return value;
        }

        public string GetLanguageById(in LanguageID languageID)
        {
            var lang = "";
            switch(languageID)
            {
                case LanguageID.ru_RU:
                    lang = "ru";
                    break;
                case LanguageID.en_US:
                    lang = "en";
                    break;
                default:
                    lang = GetLanguageById(LanguageFallback);
                    break;
            }
            return lang;
        }

        /// <summary>
        /// Get localization string
        /// </summary>
        /// <param name="language">Localization language</param>
        /// <param name="messageID">Message ID</param>
        /// <returns></returns>
        public string Message(in LanguageID language, in MessageID messageID)
        {
            return localizedMessages[language][messageID];
        }

        /// <summary>
        /// Get localization string
        /// </summary>
        /// <param name="server">Server where bot is present</param>
        /// <param name="messageID">Message ID</param>
        /// <returns></returns>
        public string Message(in DiscordGuild server, in MessageID messageID)
        {
            LanguageID language;
            if(Language.TryGetValue(server, out language))
            {
                return localizedMessages[language][messageID];
            }
            throw new InvalidOperationException("Unknown server");
        }

        /// <summary>
        /// Load localization strings for multiple languages
        /// </summary>
        /// <param name="localizations">List of localizations to load</param>
        public void LoadLocalizations(IEnumerable<LanguageID> localizations)
        {
            LanguageIDs = new List<LanguageID>(localizations);

            foreach(var loc in localizations)
            {
                LoadLocalization(loc);
            }
            // Load Fallback Language
            IDictionary<MessageID, string> localization;
            if(!localizedMessages.TryGetValue(LanguageFallback, out localization))
            {
                LoadLocalization(LanguageFallback);
            }
        }

        /// <summary>
        /// Load localization strings for one language
        /// </summary>
        /// <param name="language">Localization to load</param>
        private void LoadLocalization(LanguageID language)
        {
            var file = new FileStream(MePhItBot.Bot.Settings.LocalizationFilePath[language], FileMode.Open, FileAccess.Read);
            var sr = new StreamReader(file, Encoding.UTF8);
            var jsonText = sr.ReadToEnd();
            sr.Close();
            file.Close();

            var jsonReader = new JsonTextReader(new StringReader(jsonText));
            string jsonProperty = string.Empty;
            localizedMessages[language] = new ConcurrentDictionary<MessageID, string>();
            while(jsonReader.Read())
            {
                switch (jsonReader.TokenType)
                {
                    case JsonToken.PropertyName:
                        jsonProperty = jsonReader.Value as string;
                        break;
                    case JsonToken.String:
                        switch (jsonProperty)
                        {
                            case "ClassStarted":
                                localizedMessages[language].Add(MessageID.ClassStarted, jsonReader.Value as string);
                                break;
                            case "BreakStarted":
                                localizedMessages[language].Add(MessageID.BreakStarted, jsonReader.Value as string);
                                break;
                            case "BreakEnded":
                                localizedMessages[language].Add(MessageID.BreakEnded, jsonReader.Value as string);
                                break;
                            case "ClassEndingNotify":
                                localizedMessages[language].Add(MessageID.ClassEndingNotify, jsonReader.Value as string);
                                break;
                            case "ClassEnded":
                                localizedMessages[language].Add(MessageID.ClassEnded, jsonReader.Value as string);
                                break;
                            case "ListHeader":
                                localizedMessages[language].Add(MessageID.ListHeader, jsonReader.Value as string);
                                break;
                            case "ListPresent":
                                localizedMessages[language].Add(MessageID.ListPresent, jsonReader.Value as string);
                                break;
                            case "ListAbsent":
                                localizedMessages[language].Add(MessageID.ListAbsent, jsonReader.Value as string);
                                break;
                            case "CmdSrvrOrgChannelNameRules":
                                localizedMessages[language].Add(MessageID.CmdSrvrOrgChannelNameRules, jsonReader.Value as string);
                                break;
                            case "CmdSrvrOrgChannelNameInfo":
                                localizedMessages[language].Add(MessageID.CmdSrvrOrgChannelNameInfo, jsonReader.Value as string);
                                break;
                            case "CmdSrvrOrgCategoryNameClass":
                                localizedMessages[language].Add(MessageID.CmdSrvrOrgCategoryNameClass, jsonReader.Value as string);
                                break;
                            case "CmdSrvrOrgChannelNameChat":
                                localizedMessages[language].Add(MessageID.CmdSrvrOrgChannelNameChat, jsonReader.Value as string);
                                break;
                            case "CmdSrvrOrgChannelNameCommon":
                                localizedMessages[language].Add(MessageID.CmdSrvrOrgChannelNameCommon, jsonReader.Value as string);
                                break;
                            case "CmdSrvrOrgChannelNameSubmit":
                                localizedMessages[language].Add(MessageID.CmdSrvrOrgChannelNameSubmit, jsonReader.Value as string);
                                break;
                            case "CmdSrvrOrgCategoryNameControl":
                                localizedMessages[language].Add(MessageID.CmdSrvrOrgCategoryNameControl, jsonReader.Value as string);
                                break;
                            case "CmdSrvrOrgChannelNameCommands":
                                localizedMessages[language].Add(MessageID.CmdSrvrOrgChannelNameCommands, jsonReader.Value as string);
                                break;
                            case "CmdSrvrOrgRoleTeacher":
                                localizedMessages[language].Add(MessageID.CmdSrvrOrgRoleTeacher, jsonReader.Value as string);
                                break;
                            case "CmdSrvrOrgRoleGroupLeader":
                                localizedMessages[language].Add(MessageID.CmdSrvrOrgRoleGroupLeader, jsonReader.Value as string);
                                break;
                            case "CmdSrvrOrgRoleStudent":
                                localizedMessages[language].Add(MessageID.CmdSrvrOrgRoleStudent, jsonReader.Value as string);
                                break;
                            case "CmdMyTestTestsSearch":
                                localizedMessages[language].Add(MessageID.CmdMyTestTestsSearch, jsonReader.Value as string);
                                break;
                            case "CmdMyTestTestsNotFound":
                                localizedMessages[language].Add(MessageID.CmdMyTestTestsNotFound, jsonReader.Value as string);
                                break;
                            case "CmdMyTestFileLoadSuccess":
                                localizedMessages[language].Add(MessageID.CmdMyTestFileLoadSuccess, jsonReader.Value as string);
                                break;
                            case "CmdMyTestStartHelp":
                                localizedMessages[language].Add(MessageID.CmdMyTestStartHelp, jsonReader.Value as string);
                                break;
                            case "CmdMyTestStartTime":
                                localizedMessages[language].Add(MessageID.CmdMyTestStartTime, jsonReader.Value as string);
                                break;
                            case "CmdMyTestStartTempCategoryName":
                                localizedMessages[language].Add(MessageID.CmdMyTestStartTempCategoryName, jsonReader.Value as string);
                                break;
                            case "CmdMyTestStartTestFinished":
                                localizedMessages[language].Add(MessageID.CmdMyTestStartTestFinished, jsonReader.Value as string);
                                break;
                            case "CmdMyTestStartTestQuestionResult":
                                localizedMessages[language].Add(MessageID.CmdMyTestStartTestQuestionResult, jsonReader.Value as string);
                                break;
                            case "CmdMyTestStartTestTotalResults":
                                localizedMessages[language].Add(MessageID.CmdMyTestStartTestTotalResults, jsonReader.Value as string);
                                break;
                            case "CmdMyTestMark":
                                localizedMessages[language].Add(MessageID.CmdMyTestMark, jsonReader.Value as string);
                                break;
                            case "CmdMyTestMark0":
                                localizedMessages[language].Add(MessageID.CmdMyTestMark0, jsonReader.Value as string);
                                break;
                            case "CmdMyTestMark1":
                                localizedMessages[language].Add(MessageID.CmdMyTestMark1, jsonReader.Value as string);
                                break;
                            case "CmdMyTestMark2":
                                localizedMessages[language].Add(MessageID.CmdMyTestMark2, jsonReader.Value as string);
                                break;
                            case "CmdMyTestMark3":
                                localizedMessages[language].Add(MessageID.CmdMyTestMark3, jsonReader.Value as string);
                                break;
                            case "CmdMyTestMark4":
                                localizedMessages[language].Add(MessageID.CmdMyTestMark4, jsonReader.Value as string);
                                break;
                            case "CmdMyTestMark5":
                                localizedMessages[language].Add(MessageID.CmdMyTestMark5, jsonReader.Value as string);
                                break;
                            case "CmdMyTestMark100":
                                localizedMessages[language].Add(MessageID.CmdMyTestMark100, jsonReader.Value as string);
                                break;
                            case "CmdMyTestResultsLink":
                                localizedMessages[language].Add(MessageID.CmdMyTestResultsLink, jsonReader.Value as string);
                                break;
                            default:
                                break;
                        }
                        break;
                    default:
                        break;
                }
            }
            jsonReader.Close();
        }

    }
}
