using Newtonsoft.Json;
using System.Text;
using static TGUI.Config.Config;

namespace TGUI
{
    public partial class MainPage : ContentPage
    {
        private Mutex _singleSendMutex = new Mutex();
        private static Color DefaultBtnColor;
        private static Color DefaultBtnTextColor;

        public MainPage()
        {
            InitializeComponent();

            DefaultBtnColor = send_pic.BackgroundColor;
            DefaultBtnTextColor = send_pic.TextColor;
        }

        private void onSendMsgToAll(object sender, EventArgs e)
        {
            send_pic.BackgroundColor = DefaultBtnColor;
            send_pic.TextColor = DefaultBtnTextColor;
            send_msg.BackgroundColor = (Color)Application.Current.Resources["SecondaryDarkText"];
            send_msg.TextColor = (Color)Application.Current.Resources["MidnightBlue"];

            var messageBox = new Editor
            {
                Placeholder = $"Write your message!",
                AutoSize = EditorAutoSizeOption.TextChanges,
                BackgroundColor = (Color)Application.Current.Resources["Primary"],
                WidthRequest = 500,
                HeightRequest = 300,
                FontSize = 14
            };

            var pathToList = new Label 
            { 
                Text = "-",
                TextColor = (Color)Application.Current.Resources["Gray600"],
                FontSize = 14,
                WidthRequest = 400,
            };

            var userCountLabel = new Label
            {
                Text = "-",
                FontAttributes = FontAttributes.Italic,
                TextColor = (Color)Application.Current.Resources["Gray600"],
                FontSize = 12,
                VerticalTextAlignment = TextAlignment.Center
            };

            var selectPathToList = new Button
            {
                Text = "Select User List File",
                TextColor = (Color)Application.Current.Resources["Gray600"],
                BackgroundColor = (Color)Application.Current.Resources["Secondary"],
                WidthRequest = 250,
                Command = new Command( async () =>
                {
                    var res = await FilePicker.Default.PickAsync(new PickOptions() { FileTypes = UserListFileType });
                    if (res is not null && !string.IsNullOrEmpty(res.FileName))
                    {
                        pathToList.Text = res.FullPath;

                        if (File.Exists(res.FullPath))
                        {
                            var count = 0;

                            using (var listOfUsers = new StreamReader(File.OpenRead(pathToList.Text)))
                            {
                                while (!string.IsNullOrEmpty(listOfUsers.ReadLine())) count++;
                            }

                            userCountLabel.Text = $"{count.ToString()} ids";
                        }
                    }
                })
            };

            var filePathGrid = new Grid
            {
                ColumnSpacing = 10,
                HorizontalOptions = LayoutOptions.Center,
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = 100 },
                    new ColumnDefinition { Width = 200 },
                    new ColumnDefinition { Width = 100 }
                }
            };

            var fileLabel = new Label
            {
                Text = "File path:",
                FontAttributes = FontAttributes.Bold,
                TextColor = (Color)Application.Current.Resources["Gray600"],
                VerticalTextAlignment = TextAlignment.Center
            };

            filePathGrid.SetColumn(fileLabel, 0);
            filePathGrid.Children.Add(fileLabel);
            filePathGrid.SetColumn(pathToList, 1);
            filePathGrid.Children.Add(pathToList);
            filePathGrid.SetColumn(userCountLabel, 2);
            filePathGrid.Children.Add(userCountLabel);

            var status = new Label
            {
                Text = "-",
                TextColor = (Color)Application.Current.Resources["Gray600"],
                FontSize = 14,
                WidthRequest = 150,
                Margin = 10
            };

            messageBox.Focused += ((sender, e) => status.Text = "-");

            main_content.Content = new StackLayout
            {
                Spacing = 10,
                Margin = 10,
                Children =
                {
                    messageBox,
                    selectPathToList,
                    filePathGrid,
                    new Button
                    {
                        Text = "Send",
                        TextColor = (Color)Application.Current.Resources["Gray600"],
                        WidthRequest = 100,
                        Command = new Command(async () =>
                        {
                            status.Text = "Preparing";
                            var url = $"https://api.telegram.org/bot{UserConfig.TelegramBotId}/sendMessage";

                            using (var listOfUsers = new StreamReader(File.OpenRead(pathToList.Text)))
                            {
                                var httpClient = new HttpClient();
                                httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                                string? user = "";
                                var tasks = new List<Task<HttpResponseMessage>>(25);

                                _singleSendMutex.WaitOne();

                                status.Text = "Sending...";

                                var count = await Task<int>.Run(() => {
                                    var _count = 0;

                                    while (!string.IsNullOrEmpty(user = listOfUsers.ReadLine()))
                                    {
                                        var payload = new
                                        {
                                            chat_id = user,
                                            text = EscapeMarkdownV2(messageBox.Text).Replace('\r', '\n'),
                                            parse_mode = "MarkdownV2"
                                        };

                                        var serializer = new JsonSerializer();
                                        using (var sw = new StringWriter(new StringBuilder()))
                                        {
                                            serializer.Serialize(sw, payload);
                                            var content = new StringContent(sw.ToString(), Encoding.UTF8, "application/json");
                                            tasks.Add(Task.Run(() => httpClient.PostAsync(new Uri(url), content)));
                                        }

                                        _count++;
                                        if (tasks.Count == 25) { Thread.Sleep(1000); Task.WaitAll(tasks.ToArray()); tasks = new List<Task<HttpResponseMessage>>(25); }
                                    }

                                    Task.WaitAll(tasks.ToArray());
                                    return _count;
                                });
                                
                                _singleSendMutex.ReleaseMutex();

                                messageBox.Text = "";
                                status.Text = $"Completed! {count} msg sent";
                            }
                        })
                    },
                    status
                }
            };
        }

        public static string EscapeMarkdownV2(string text)
        {
            var specialChars = new[] { "_", "*", "[", "]", "(", ")", "~", "`", ">", "#", "+", "-", "=", "|", "{", "}", ".", "!" };

            foreach (var ch in specialChars)
                text = text.Replace(ch, "\\" + ch);

            // Make sure \n stays as-is for MarkdownV2 to render line breaks
            return text;
        }

        private void onSendPicToAll(object sender, EventArgs e)
        {
            send_msg.BackgroundColor = DefaultBtnColor;
            send_msg.TextColor = DefaultBtnTextColor;
            send_pic.BackgroundColor = (Color)Application.Current.Resources["SecondaryDarkText"];
            send_pic.TextColor = (Color)Application.Current.Resources["MidnightBlue"];

            var messageBox = new Editor
            {
                Placeholder = $"Write your message!",
                AutoSize = EditorAutoSizeOption.TextChanges,
                BackgroundColor = (Color)Application.Current.Resources["Primary"],
                WidthRequest = 500,
                HeightRequest = 300,
                FontSize = 14
            };

            var pathToList = new Label
            {
                Text = "-",
                TextColor = (Color)Application.Current.Resources["Gray600"],
                FontSize = 14,
                WidthRequest = 400,
            };

            var userCountLabel = new Label
            {
                Text = "-",
                FontAttributes = FontAttributes.Italic,
                TextColor = (Color)Application.Current.Resources["Gray600"],
                FontSize = 12,
                VerticalTextAlignment = TextAlignment.Center
            };

            var selectPathToList = new Button
            {
                Text = "Select User List File",
                TextColor = (Color)Application.Current.Resources["Gray600"],
                BackgroundColor = (Color)Application.Current.Resources["Secondary"],
                WidthRequest = 250,
                Command = new Command(async () =>
                {
                    var res = await FilePicker.Default.PickAsync(new PickOptions() { FileTypes = UserListFileType });
                    if (res is not null && !string.IsNullOrEmpty(res.FileName))
                    {
                        pathToList.Text = res.FullPath;

                        if (File.Exists(res.FullPath))
                        {
                            var count = 0;

                            using (var listOfUsers = new StreamReader(File.OpenRead(pathToList.Text)))
                            {
                                while (!string.IsNullOrEmpty(listOfUsers.ReadLine())) count++;
                            }

                            userCountLabel.Text = $"{count.ToString()} ids";
                        }
                    }
                })
            };

            var filePathGrid = new Grid
            {
                ColumnSpacing = 10,
                HorizontalOptions = LayoutOptions.Center,
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = 100 },
                    new ColumnDefinition { Width = 200 },
                    new ColumnDefinition { Width = 100 }
                }
            };

            var fileLabel = new Label
            {
                Text = "File path:",
                FontAttributes = FontAttributes.Bold,
                TextColor = (Color)Application.Current.Resources["Gray600"],
                VerticalTextAlignment = TextAlignment.Center
            };

            filePathGrid.SetColumn(fileLabel, 0);
            filePathGrid.Children.Add(fileLabel);
            filePathGrid.SetColumn(pathToList, 1);
            filePathGrid.Children.Add(pathToList);
            filePathGrid.SetColumn(userCountLabel, 2);
            filePathGrid.Children.Add(userCountLabel);

            var pathToImage = new Entry
            {
                Placeholder = "Input path to image",
                TextColor = (Color)Application.Current.Resources["Gray600"],
                WidthRequest = 250
            };

            var status = new Label
            {
                Text = "-",
                TextColor = (Color)Application.Current.Resources["Gray600"],
                FontSize = 14,
                WidthRequest = 150,
                Margin = 10
            };

            messageBox.Focused += ((sender, e) => status.Text = "-");

            main_content.Content = new ScrollView
            {
                Content = new StackLayout
                {
                    Spacing = 10,
                    Margin = 10,
                    Children =
                    {
                        messageBox,
                        selectPathToList,
                        filePathGrid,
                        pathToImage,
                        new Button
                        {
                            Text = "Send",
                            TextColor = (Color)Application.Current.Resources["Gray600"],
                            WidthRequest = 100,
                            Command = new Command(async () =>
                            {
                                status.Text = "Preparing";
                                var url = $"https://api.telegram.org/bot{UserConfig.TelegramBotId}/sendPhoto";

                                using (var listOfUsers = new StreamReader(File.OpenRead(pathToList.Text)))
                                {
                                    var httpClient = new HttpClient();
                                    httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                                    string? user = "";
                                    var tasks = new List<Task<HttpResponseMessage>>(25);

                                    _singleSendMutex.WaitOne();

                                    status.Text = "Sending...";
                                    var _pathToImage = pathToImage.Text.TrimStart('/');

                                    var count = await Task<int>.Run(() =>
                                    {
                                        var _count = 0;

                                        while (!string.IsNullOrEmpty(user = listOfUsers.ReadLine()))
                                        {
                                            var payload = new
                                            {
                                                chat_id = user,
                                                photo = Path.Combine(UserConfig.DefaultUrlToPicture, _pathToImage).ToString(),
                                                caption = EscapeMarkdownV2(messageBox.Text).Replace('\r', '\n'),
                                                parse_mode = "MarkdownV2"
                                            };

                                            var serializer = new JsonSerializer();
                                            using (var sw = new StringWriter(new StringBuilder()))
                                            {
                                                serializer.Serialize(sw, payload);
                                                var content = new StringContent(sw.ToString(), Encoding.UTF8, "application/json");
                                                tasks.Add(Task.Run(() => httpClient.PostAsync(new Uri(url), content)));
                                            }

                                            _count++;
                                            if (tasks.Count == 25) { Thread.Sleep(1000); Task.WaitAll(tasks.ToArray()); tasks = new List<Task<HttpResponseMessage>>(25); }
                                        }

                                        Task.WaitAll(tasks.ToArray());
                                        return _count;
                                    });

                                    
                                    _singleSendMutex.ReleaseMutex();

                                    messageBox.Text = "";
                                    status.Text = $"Completed! {count} msg sent";
                                }
                            })
                        },
                        status
                    }
                }
            };
        }
    }
}
