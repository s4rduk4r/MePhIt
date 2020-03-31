using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

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
        CmdMyTestFileLoadSuccess
    }

    public class MePhItLocalization
    {
        public static MePhItLocalization Localization = new MePhItLocalization();

        private IDictionary<LanguageID, IDictionary<MessageID, string>> localizedMessages = new Dictionary<LanguageID, IDictionary<MessageID, string>>();

        public LanguageID LanguageFallback = LanguageID.en_US;
        public string Message(in LanguageID language, in MessageID messageID)
        {
            return localizedMessages[language][messageID];
        }

        public void LoadLocalizations(IEnumerable<LanguageID> localizations)
        {
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

        private void LoadLocalization(LanguageID language)
        {
            var file = new FileStream(MePhItBot.Bot.Settings.LocalizationFilePath[language], FileMode.Open, FileAccess.Read);
            var sr = new StreamReader(file, Encoding.UTF8);
            var jsonText = sr.ReadToEnd();
            sr.Close();
            file.Close();

            var jsonReader = new JsonTextReader(new StringReader(jsonText));
            string jsonProperty = string.Empty;
            localizedMessages[language] = new Dictionary<MessageID, string>();
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
