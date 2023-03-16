using System;
using System.Windows;
using System.Windows.Forms;
using Gma.System.MouseKeyHook;
using System.Runtime.InteropServices;
using System.Collections.ObjectModel;
using MouseViewer.Models;
using System.Data.SQLite;
using DataBase;
using MouseViewer.ServiceMV;
using System.Windows.Threading;
using System.Windows.Media;
using System.Net.Mail;
using System.Threading;
using System.Net;

namespace MouseViewer
{
    public partial class MainWindow : Window, IServiceMVCallback
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetCursorPos(int X, int Y);
        private int lastXPos, lastYPos;

        private IKeyboardMouseEvents m_Events;
        private ObservableCollection<MouseModel> MouseEvents = new ObservableCollection<MouseModel>();

        private bool bRecordIsActive = false;
        private bool filterApplied = false;
        private bool isConnected = false;
        private bool bRecordClicked = false;
        private bool DBConnected = false;

        private int eventsAmount = 0;
        private ServiceMVClient client;
        private int ID;


        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Creates a client
        /// </summary>
        private void MainWin_Loaded(object sender, RoutedEventArgs e)
        {
            client = new ServiceMVClient(new System.ServiceModel.InstanceContext(this));
        }

        /// <summary>
        /// Start record
        /// </summary>
        private void Subscribe(IKeyboardMouseEvents events)
        {
            m_Events = events;
            m_Events.MouseMove += M_Events_MouseMove;
            m_Events.MouseClick += M_Events_MouseClick;
        }

        /// <summary>
        /// Stop record
        /// </summary>
        private void Unsubscribe()
        {
            if (m_Events == null) return;
            m_Events.MouseMove -= M_Events_MouseMove;
            m_Events.MouseClick -= M_Events_MouseClick;
            m_Events.Dispose();
            m_Events = null;
        }

        /// <summary>
        /// Ask for send message each 50 records
        /// </summary>
        private void ShowPopUpSendMsg()
        {
            SendWindow sendWindow = new SendWindow();
            sendWindow.Show();
        }

        private void AddDataToDataGrid(MouseModel mouseEvent)
        {
            InsertData(mouseEvent.CreationDate, mouseEvent.M_Event, mouseEvent.M_Coords);
            MouseEvents.Add(mouseEvent);
            eventsAmount++;
            tbAmount.Text = eventsAmount.ToString();
            ScrollDown();
        }

        /// <summary>
        /// Handle mouse moving
        /// </summary>
        private void M_Events_MouseMove(object sender, MouseEventArgs e)
        {
            int currentXPos = e.X, currentYPos = e.Y;

            if (!bRecordClicked)
            {
                bRecordClicked = true;
                lastXPos = currentXPos;
                lastYPos = currentYPos;
            } else if (Math.Abs(currentXPos - lastXPos) >= 10 ||
                       Math.Abs(currentYPos - lastYPos) >= 10)
            {
                string coords = string.Format("x={0:0000}; y={1:0000}", e.X, e.Y);

                dgMouseEvents.ItemsSource = MouseEvents;
                MouseModel mouseEvent = new MouseModel();
                mouseEvent.M_Coords = coords;
                if (mouseEvent.M_Event == null)
                {
                    mouseEvent.M_Event = "";
                }

                AddDataToDataGrid(mouseEvent);

                if (eventsAmount % 50 == 0)
                {
                    StartStopRecord();
                    ShowPopUpSendMsg();
                }

                lastXPos = currentXPos;
                lastYPos = currentYPos;
            }
        }

        /// <summary>
        /// Handle clicking
        /// </summary>
        private void M_Events_MouseClick(object sender, MouseEventArgs e)
        {
            dgMouseEvents.ItemsSource = MouseEvents;
            MouseModel mouseEvent = new MouseModel();
            string mEvent = "";

            switch(e.Button)
            {
                case MouseButtons.Left:
                    mEvent = "LMB";
                    break;
                case MouseButtons.Right:
                    mEvent = "RMB";
                    break;
                case MouseButtons.Middle:
                    mEvent = "MMB";
                    break;
            }

            mouseEvent.M_Event = mEvent;

            AddDataToDataGrid(mouseEvent);

            if (eventsAmount % 50 == 0)
            {
                StartStopRecord();
                ShowPopUpSendMsg();
            }
        }

        private void StartStopRecord()
        {
            if (!bRecordIsActive)
            {
                Subscribe(Hook.GlobalEvents());
                bRecordIsActive = true;
                bRecord.Content = "Стоп";
            }
            else
            {
                Unsubscribe();
                bRecordClicked = false;
                bRecordIsActive = false;
                bRecord.Content = "Запуск";
            }
        }

        private void bRecord_Click(object sender, RoutedEventArgs e)
        {
            StartStopRecord();
        }

        /// <summary>
        /// Disconnect user when client is closed
        /// </summary>
        private void MainWin_ContextMenuClosing(object sender, System.Windows.Controls.ContextMenuEventArgs e)
        {
            DisconnectUser();
            Unsubscribe();
        }

        private void ScrollDown()
        {
            if (dgMouseEvents.Items.Count > 0)
            {
                var lastItem = dgMouseEvents.Items[dgMouseEvents.Items.Count - 1];
                dgMouseEvents.ScrollIntoView(lastItem);
            }
        }

        /// <summary>
        /// Insert to DB
        /// </summary>
        private void InsertData(string creationDate, string eventName, string coords)
        {
            string insertQuery = "INSERT INTO MouseEvents ('creationDate', 'eventName', 'coords') VALUES (@creationDate, @eventName, @coords)";
            Database databaseObject = new Database();
            SQLiteCommand _insertCommand = new SQLiteCommand(insertQuery, databaseObject.DBConnection);
            databaseObject.OpenConnection();

            _insertCommand.Parameters.AddWithValue("@creationDate", creationDate);
            _insertCommand.Parameters.AddWithValue("@eventName", eventName);
            _insertCommand.Parameters.AddWithValue("@coords", coords);
            _insertCommand.ExecuteNonQuery();

            databaseObject.CloseConnection();
        }

        /// <summary>
        /// Show all in table
        /// </summary>
        private void SelectAllData()
        {
            string selectAllQuery = "SELECT * FROM MouseEvents";
            Database databaseObject = new Database();
            SQLiteCommand _selectCommand = new SQLiteCommand(selectAllQuery, databaseObject.DBConnection);
            databaseObject.OpenConnection();
            SQLiteDataReader result = _selectCommand.ExecuteReader();

            if (result.HasRows)
            {
                while (result.Read())
                {
                    dgMouseEvents.ItemsSource = MouseEvents;
                    MouseModel mouseEvent = new MouseModel();
                    mouseEvent.CreationDate = result["creationDate"].ToString();
                    mouseEvent.M_Coords = result["coords"].ToString();
                    mouseEvent.M_Event = result["eventName"].ToString();
                    MouseEvents.Add(mouseEvent);
                    eventsAmount++;
                    tbAmount.Text = eventsAmount.ToString();
                }
            }

            ScrollDown();
            databaseObject.CloseConnection();
        }

        private bool SQLSelect(string query)
        {
            Database databaseObject = new Database();
            SQLiteCommand command = new SQLiteCommand(query, databaseObject.DBConnection);
            databaseObject.OpenConnection();

            if (dgMouseEvents.HasItems)
            {
                SQLiteDataReader result = command.ExecuteReader();
                if (result.HasRows)
                {
                    while (result.Read())
                    {
                        MouseModel mouseEvent = new MouseModel();
                        mouseEvent.CreationDate = result["creationDate"].ToString();
                        mouseEvent.M_Event = result["eventName"].ToString();
                        mouseEvent.M_Coords = result["coords"].ToString();
                        MouseEvents.Add(mouseEvent);
                        eventsAmount++;
                        tbAmount.Text = eventsAmount.ToString();
                    }
                }

                ScrollDown();
                databaseObject.CloseConnection();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Search filter
        /// </summary>
        private void bFind_Click(object sender, RoutedEventArgs e)
        {
            if (!dgMouseEvents.HasItems)
            {
                System.Windows.MessageBox.Show("Список пустой!");
                return;
            } else if (tbEvent.Text == "" && tbCreationDate.Text == "")
            {
                System.Windows.MessageBox.Show("Поля пустые!");
                return;
            } else
            {
  
                bClear_Click(sender, e);
                SelectAllData();

                dgMouseEvents.ItemsSource = MouseEvents;
                MouseEvents.Clear();
                eventsAmount = 0;

                string filterQuery = $"SELECT * FROM MouseEvents WHERE creationDate LIKE '%{tbCreationDate.Text}%' AND eventName LIKE '%{tbEvent.Text}%'";
                if (!SQLSelect(filterQuery))
                {
                    System.Windows.MessageBox.Show("Не найдено");
                }
            }
        }

        /// <summary>
        /// Clear table
        /// </summary>
        private void bClear_Click(object sender, RoutedEventArgs e)
        {
            dgMouseEvents.ItemsSource = MouseEvents;
            MouseEvents.Clear();
            eventsAmount = 0;
            tbAmount.Text = "0";
        }

        private bool FindUser()
        {
            string findUserQuery = $"SELECT * FROM Users WHERE login = '{tbLogin.Text}' AND password = '{tbPassword.Password}'";
            Database databaseObject = new Database();
            SQLiteCommand findUserCommand = new SQLiteCommand(findUserQuery, databaseObject.DBConnection);
            bool userFound = false;
            databaseObject.OpenConnection();

            SQLiteDataReader result = findUserCommand.ExecuteReader();

            while (result.Read())
            {
                if (result != null)
                {
                    userFound = true;
                    break;
                }
            }

            return userFound;
        }

        private void ConnectUser()
        {
            if (!FindUser())
            {
                string failAuthMessage = $"Неверный логин или пароль!";
                ShowWarning(failAuthMessage, 3, "Red");
            } else if (!isConnected)
            {
                // ID = client.Connect(tbLogin.Text);
                
                string successAuthMessage = $"Вы авторизовались как {tbLogin.Text}";
                ShowWarning(successAuthMessage, 3, "Green");

                bRecord.IsEnabled = true;
                tbLogin.IsEnabled = false;
                tbPassword.IsEnabled = false;
                isConnected = true;
                bSignIn.Content = "Выйти";
                SelectAllData();
            }
        }

        private void DisconnectUser()
        {
            if (isConnected)
            {
                // client.Disconnect(ID);
                bRecord.IsEnabled = false;
                tbLogin.IsEnabled = true;
                tbPassword.IsEnabled = true;
                isConnected = false;
                bSignIn.Content = "Войти";
            }
        }

        private void ShowWarning(string message, int seconds, string color)
        {
            BrushConverter brushConverter = new BrushConverter();
            Brush colorBrush = (Brush)brushConverter.ConvertFromString(color);

            lWarning.Foreground = colorBrush;
            lWarning.Visibility = Visibility.Visible;
            lWarning.Content = message;

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(seconds);
            timer.Tick += (s, e) =>
            {
                lWarning.Visibility = Visibility.Hidden;
                timer.Stop();
            };
            timer.Start();
        }

        private void bSignIn_Click(object sender, RoutedEventArgs e)
        {
            if (!isConnected)
            {
                ConnectUser();
            } else
            {
                DisconnectUser();
                bClear_Click(sender, e);
            }
        }

        /// <summary>
        /// Write a console message when client connected/disconnected
        /// </summary>
        public void MsgCallback(string msg)
        {
            Console.WriteLine(msg);
        }

        public void SendMsgWhatsApp(string number, string message)
        {
            try
            {
                number = "+7" + number;
                number = number.Replace(" ", "");
                System.Diagnostics.Process.Start($"http://api.whatsapp.com/send?phone={number}&text={message}");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        private void bCancel_Click(object sender, RoutedEventArgs e)
        {
            bClear_Click(sender, e);
            SelectAllData();
        }

        public void SendMsgMail(string mail, string message)
        {
            string mailServer = "evinius.ya@yandex.ru";
            string mailClient= "evinius.ya@yandex.ru";

            MailAddress server = new MailAddress(mailServer, "Server");
            MailAddress client = new MailAddress(mailClient, "Client");
            Thread.Sleep(100);

            using (MailMessage msg = new MailMessage(client, server))
            {
                using (SmtpClient smtp = new SmtpClient())
                {
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential(client.Address, "qijoiuhuczworhir");
                    msg.Subject = "Мышка";
                    msg.Body = "<h4>50 событий мышки</h4>";
                    msg.IsBodyHtml = true;

                    smtp.Host = "smtp.yandex.ru";
                    smtp.Port = 587;
                    smtp.EnableSsl = true;
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;

                    Thread.Sleep(100);
                    smtp.Send(msg);
                    Thread.Sleep(100);
                }
            }
        }
    }
}