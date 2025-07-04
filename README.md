# TGUI
Telegram interface to send messages via Bots

**The code has been tested for Windows**
The code should be compatible for Mac and Linux OS, but compilation and tests have not been performed.

## How to use?
### Config
In the `.\Config` folder, edit the _Config.json_ file:<br/>
1. Add the Bot id which you can find from BotFather in your Telegram app
2. Add the URL folder where the pictures are stored; Telegram needs to access pictures which are available online

### Launch the program
You can select to send a message or a picture.
1. In the message box, input the message you want to send.
2. Select the file containing the list of Telegram Ids of the users you want to send the message to
3. (if you want to send a picture) Indicate the name of the picture you want to attach. If the picture is in a sub-folder of the default URL you have indicated, write the sub-folder structure
4. Send the message, wait until completion

When completed, a messsage saying "**Completed!**" will appear.
