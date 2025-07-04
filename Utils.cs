using System;

namespace TGUI
{
    public partial class MainPage : ContentPage
    {
        public void SetSelectedBtn(Button button)
        {
            send_msg.BackgroundColor = button.Id == send_msg.Id ? (Color)Application.Current.Resources["SecondaryDarkText"] : DefaultBtnColor;
            send_msg.TextColor = button.Id == send_msg.Id ? (Color)Application.Current.Resources["MidnightBlue"] : DefaultBtnTextColor;
            send_msg_user.BackgroundColor = button.Id == send_msg_user.Id ? (Color)Application.Current.Resources["SecondaryDarkText"] : DefaultBtnColor;
            send_msg_user.TextColor = button.Id == send_msg_user.Id ? (Color)Application.Current.Resources["MidnightBlue"] : DefaultBtnTextColor;
            send_pic.BackgroundColor = button.Id == send_pic.Id ? (Color)Application.Current.Resources["SecondaryDarkText"] : DefaultBtnColor;
            send_pic.TextColor = button.Id == send_pic.Id ? (Color)Application.Current.Resources["MidnightBlue"] : DefaultBtnTextColor;
        }

        internal static string EscapeMarkdownV2(string text)
        {
            var specialChars = new[] { "_", "*", "[", "]", "(", ")", "~", "`", ">", "#", "+", "-", "=", "|", "{", "}", ".", "!" };

            foreach (var ch in specialChars)
                text = text.Replace(ch, "\\" + ch);

            // Make sure \n stays as-is for MarkdownV2 to render line breaks
            return text;
        }
    }
}
