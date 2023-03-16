using DataBase;
using System.Data.SQLite;
using System.Windows;

namespace MouseViewer
{
    public partial class SendWindow : Window
    {
        public SendWindow()
        {
            InitializeComponent();
        }

        private void bSendMsg_Click(object sender, RoutedEventArgs e)
        {
            string _selectPhoneQuery = "SELECT phone FROM Senders WHERE userName = 'Jonas'";
            Database databaseObject = new Database();
            SQLiteCommand _selectCommand = new SQLiteCommand(_selectPhoneQuery, databaseObject.DBConnection);
            databaseObject.OpenConnection();
            SQLiteDataReader result = _selectCommand.ExecuteReader();
            if (result != null)
            {
                MainWindow mainWindow = new MainWindow();
                result.Read();
                string phoneNumber = result["phone"].ToString();
                //mainWindow.SendMsgWhatsApp(phoneNumber, "50 записей мышки...");
                mainWindow.SendMsgMail(phoneNumber, "50 записей мышки...");
            }

            Close();
        }
    }
}
