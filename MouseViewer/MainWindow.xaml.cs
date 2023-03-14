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

namespace MouseViewer
{
    public partial class MainWindow : Window, IServiceMVCallback
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetCursorPos(int X, int Y);
        int lastXPos, lastYPos;

        private IKeyboardMouseEvents m_Events;
        private ObservableCollection<MouseModel> MouseEvents = new ObservableCollection<MouseModel>();
        private bool bRunIsActive = false;
        private bool filterApplied = false;
        private int eventsAmount = 0;
        private bool isConnected = false;
        private ServiceMVClient client;
        private int ID;


        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWin_Loaded(object sender, RoutedEventArgs e)
        {
            client = new ServiceMVClient(new System.ServiceModel.InstanceContext(this));
        }

        private void Subscribe(IKeyboardMouseEvents events)
        {
            m_Events = events;
            m_Events.MouseMove += M_Events_MouseMove;
            m_Events.MouseClick += M_Events_MouseClick;
        }

        private void Unsubscribe()
        {
            if (m_Events == null) return;
            m_Events.MouseMove -= M_Events_MouseMove;
            m_Events.MouseClick -= M_Events_MouseClick;
            m_Events.Dispose();
            m_Events = null;
        }

        private void M_Events_MouseMove(object sender, MouseEventArgs e)
        {
            int currentXPos = e.X, currentYPos = e.Y;

            if (Math.Abs(currentXPos - lastXPos) >= 10 ||
                Math.Abs(currentYPos - lastYPos) >= 10)
            {
                string coords = string.Format("x={0:0000}; y={1:0000}", e.X, e.Y);
                // tbCoords.Text = string.Format("x={0:0000}; y={1:0000}", e.X, e.Y);

                dgMouseEvents.ItemsSource = MouseEvents;

                MouseModel mouseEvent = new MouseModel();
                mouseEvent.M_Coords = coords;
                if (mouseEvent.M_Event == null)
                {
                    mouseEvent.M_Event = "";
                }
                InsertData(mouseEvent.CreationDate, mouseEvent.M_Event, mouseEvent.M_Coords);

                MouseEvents.Add(mouseEvent);
                tbAmount.Text = eventsAmount++.ToString();

                ScrollDown();

                lastXPos = currentXPos;
                lastYPos = currentYPos;
            }
        }

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
            InsertData(mouseEvent.CreationDate, mouseEvent.M_Event, mouseEvent.M_Coords);

            MouseEvents.Add(mouseEvent);
            tbAmount.Text = eventsAmount++.ToString();

            ScrollDown();
        }

        private void bRun_Click(object sender, RoutedEventArgs e)
        {

            if (!bRunIsActive)
            {
                Unsubscribe();
                Subscribe(Hook.GlobalEvents());
                bRunIsActive = true;
                bRun.Content = "Стоп";
            } else
            {
                Unsubscribe();
                bRunIsActive = false;
                bRun.Content = "Запуск";
            }

        }

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

        private void InsertData(string creationDate, string eventName, string coords)
        {
            string _insertQuery = "INSERT INTO MouseEvents ('creationDate', 'eventName', 'coords') VALUES (@creationDate, @eventName, @coords)";
            Database databaseObject = new Database();
            SQLiteCommand _insertCommand = new SQLiteCommand(_insertQuery, databaseObject.DBConnection);
            databaseObject.OpenConnection();

            _insertCommand.Parameters.AddWithValue("@creationDate", creationDate);
            _insertCommand.Parameters.AddWithValue("@eventName", eventName);
            _insertCommand.Parameters.AddWithValue("@coords", coords);
            var result = _insertCommand.ExecuteNonQuery();

            // Console.WriteLine("Rows added: {0}", result);

            databaseObject.CloseConnection();
        }

        private void SelectAllData()
        {
            string _selectQuery = "SELECT * FROM MouseEvents";
            Database databaseObject = new Database();
            SQLiteCommand _selectCommand = new SQLiteCommand(_selectQuery, databaseObject.DBConnection);
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
                    tbAmount.Text = eventsAmount++.ToString();
                }
            }

            ScrollDown();
            databaseObject.CloseConnection();
        }

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
            }

            filterApplied = !filterApplied;

            if (!filterApplied)
            {
                bClear_Click(sender, e);
                SelectAllData();
                tbCreationDate.Text = "";
                tbEvent.Text = "";
                bFind.Content = "Найти";
                
                return;
                
            } else
            {
                bFind.Content = "Отменить";
                // bClear_Click(sender, e);
            }

            dgMouseEvents.ItemsSource = MouseEvents;
            MouseEvents.Clear();
            eventsAmount = 0;
            string _filterQuery = $"SELECT * FROM MouseEvents WHERE creationDate LIKE '%{tbCreationDate.Text}%' AND eventName LIKE '%{tbEvent.Text}%'";
            Console.WriteLine(_filterQuery);
            Database databaseObject = new Database();
            SQLiteCommand _filterCommand = new SQLiteCommand(_filterQuery, databaseObject.DBConnection);
            databaseObject.OpenConnection();

            if (dgMouseEvents.HasItems)
            {
                SQLiteDataReader result = _filterCommand.ExecuteReader();
                if (result.HasRows)
                {
                    while (result.Read())
                    {
                        MouseModel mouseEvent = new MouseModel();
                        mouseEvent.CreationDate = result["creationDate"].ToString();
                        mouseEvent.M_Event = result["eventName"].ToString();
                        mouseEvent.M_Coords = result["coords"].ToString();
                        MouseEvents.Add(mouseEvent);
                        tbAmount.Text = eventsAmount++.ToString();
                    }
                }

                ScrollDown();
                databaseObject.CloseConnection();
            } else
            {
                System.Windows.MessageBox.Show("List is empty");
            }
        }

        private void bClear_Click(object sender, RoutedEventArgs e)
        {
            dgMouseEvents.ItemsSource = MouseEvents;
            MouseEvents.Clear();
            eventsAmount = 0;
            tbAmount.Text = "0";
        }

        private bool FindUser()
        {
            string _findUserQuery = $"SELECT * FROM Users WHERE login = '{tbLogin.Text}' AND password = '{tbPassword.Password}'";
            Database databaseObject = new Database();
            SQLiteCommand _findUserCommand = new SQLiteCommand(_findUserQuery, databaseObject.DBConnection);
            bool userFound = false;
            databaseObject.OpenConnection();

            SQLiteDataReader result = _findUserCommand.ExecuteReader();

            while (result.Read())
            {
                if (result != null)
                {
                    userFound = true;
                }
            }

            return userFound;
        }

        private void ConnectUser()
        {
            if (!FindUser())
            {
                System.Windows.MessageBox.Show("Неверное имя пользователя или пароль!");
            } else if (!isConnected)
            {
                // ID = client.Connect(tbLogin.Text);
                bRun.IsEnabled = true;
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
                bRun.IsEnabled = false;
                tbLogin.IsEnabled = true;
                tbPassword.IsEnabled = true;
                isConnected = false;
                bSignIn.Content = "Войти";
            }
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

        public void MsgCallback(string msg)
        {
            Console.WriteLine(msg);
        }
    }
}