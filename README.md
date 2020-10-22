# MePhIT ![.NET Core](https://github.com/s4rduk4r/MePhit/workflows/.NET%20Core/badge.svg?branch=master&event=push)
MePhIT - Discord bot to ease a distance learning
## About [Русская версия](./README-RU.md)
**MePhIT** (Medical Physics and Information Technology) is a [Discord](https://discordapp.com) bot to organize the students for a distance learning on a ~~guild~~ server with a teacher as a server owner/administrator. The bot's usage is intended to be a part of a small server (~20 members at most) and provide basic teaching services to the teacher and the students.

### MePhIT features:
1. **Server organization**
- Create uncategorized text channels *#rules*, *#info*. 
- Under the *Class* category it creates text channels *#chat*, *#submit-your-work-here* and voice chat *Common*.
- *Bot controls* category holds bot control channel *#commands*
- Creates roles *@student*, *@teacher*, *@group-leader* and sets permissions for them as well as for the *@everyone*.
2. **Message management**. Bot allows to remove the given amount of messages either indiscriminately or of the specific user.
Bot has several commands to post text messages of different severity to the specific channel.
3. **Class organization**
- Bot can schedule a class with events: *class start*, *break start*, *break over*, *notify* students that the class is about to over, *class over*. The default values are for the medical university classes of 150 minutes long, so you would either require to override the necessary times manually or recompile the bot with the new defaults.
- Make a list of present and absent students. As of now bot considers offline and invisible members as being absent.
Because of the bot being purposed to ease teacher's work, the majority of commands will be ignored by non-administrator channel members.
4. **Tests**
- Bot supports tests and relies on the [**MyTestLib**](https://github.com/s4rduk4r/MyTestLib)
5. **Multiple languages**
- Bot supports multiple languages. As of now it has *english* and *russian* localization strings. New languages can be added.
6. **Multiple servers**
- Bot can be used on multiple servers simultaneously and keeps local server settings.

## Not implemented yet
~~1. Store server settings in an external file~~
2. File syncronization with Yandex.Disk, Google Drive, Mail.ru Cloud etc.

## Dependencies
1. [MyTestLib](https://github.com/s4rduk4r/MyTestLib)
2. [DSharpPlus](https://github.com/DSharpPlus/DSharpPlus)

# How to build
1. Clone repository
2. Open in Visual Studio and build
