using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.ServiceModel;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;

namespace wcf_chat
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ServiceChat : IServiceChat
    {
        private static List<ServerUser> users = new List<ServerUser>();
        private static List<string> userList = new List<string>();
        private static List<string> userAvatars = new List<string>();

        string _userAvatar;


        /// <summary>
        /// Вызывается при подключении пользователя к чату
        /// </summary>
        /// <param name="name"> Имя пользователя</param>
        /// <param name="password"> Пароль пользователя</param>
        /// <returns></returns>
        public int Connect(string name, string password)
        {
            ServerUser user = new ServerUser();
            if (loginUser(name, password))
            {
                user.ID = getUserIdFromName(name);
                user.isConnected = true;
                user.Name = name;
                user.operationContext = OperationContext.Current;
                user.Avatar = _userAvatar;
                SendMessage(": " + user.Name + " подключился к чату!", 0);
                users.Add(user);
                userList.Add(user.Name);
                userAvatars.Add(user.Avatar);
                Console.WriteLine("[" + DateTime.Now.ToShortTimeString() + "] " + user.Name + " ID: " + user.ID + " connected! Avatar: " + user.Avatar);
            }
            else
                user.ID = -1;

            return user.ID;
        }

        /// <summary>
        /// Происходит при выходе из чата или закрытии приложения
        /// </summary>
        /// <param name="id">Уникальный идентификатор пользователя</param>
        public void Disconnect(int id)
        {
            var user = users.FirstOrDefault(i => i.ID == id);
            if(user != null)
            {
                user.isConnected = false;
                Console.WriteLine("[" + DateTime.Now.ToShortTimeString()+"] " + user.Name + " Disconnected!");
                userList.Remove(user.Name);
                userAvatars.Remove(user.Avatar);
                users.Remove(user);
                SendMessage(": " + user.Name + " вышел из чата!", 0);
            }
        }

        /// <summary>
        /// Обработка подключения пользователя и считываение с БД MySQL данных
        /// </summary>
        /// <param name="username">Логин пользователя</param>
        /// <param name="password">Пароль</param>
        /// <returns></returns>
        public bool loginUser(string username, string password)
        {
            DataBase db = new DataBase();
            DataTable table = new DataTable();
            MySqlDataAdapter adapter = new MySqlDataAdapter();
            MySqlCommand cmd = new MySqlCommand("SELECT * FROM `users` WHERE `login` = @user", db.getDBConnection());
            cmd.Parameters.Add("@user", MySqlDbType.VarChar).Value = username;
            adapter.SelectCommand = cmd;
            adapter.Fill(table);

            if (table.Rows.Count > 0)
            {
                if (password.Equals(table.Rows[0]["password"].ToString()))
                {
                    _userAvatar = table.Rows[0]["avatar"].ToString();
                    return true;
                }
                else
                    return false;
            }
            else
            {
                createNewUser(username, password);
                return true;
            }
        }
        /// <summary>
        /// Сохранение пользователя в БД MySQL при выходе из приложения
        /// </summary>
        /// <param name="id">Уникальный идентификатор пользователя</param>
        /// <param name="avatar">Используемый аватар пользователя в чате</param>
        public void saveUser(int id, string avatar)
        {
            DataBase db = new DataBase();
            MySqlCommand cmd = new MySqlCommand("UPDATE users SET avatar = @avatar WHERE id = @id", db.getDBConnection());
            cmd.Parameters.Add("@avatar", MySqlDbType.VarChar).Value = avatar;
            cmd.Parameters.Add("@id", MySqlDbType.VarChar).Value = id;
            db.openDBConnection();
            if (cmd.ExecuteNonQuery() == 1)
                Console.WriteLine("Пользователь [" + id + "] сохранен");
            else
                Console.WriteLine("Ошибка сохранения пользователя");

            db.closeDBConnection();
        }

        /// <summary>
        /// Создание записи в БД MySQL пользователя
        /// </summary>
        /// <param name="username">Логин</param>
        /// <param name="password">Пароль</param>
        private void createNewUser(string username, string password)
        {
            DataBase db = new DataBase();
            MySqlCommand cmd = new MySqlCommand("INSERT INTO `users` (`login`, `password`, `avatar`) VALUES (@login, @password, @avatar)", db.getDBConnection());
            cmd.Parameters.Add("@login", MySqlDbType.VarChar).Value = username;
            cmd.Parameters.Add("@password", MySqlDbType.VarChar).Value = password;
            cmd.Parameters.Add("@avatar", MySqlDbType.VarChar).Value = "user_standart.png";
            db.openDBConnection();
            if (cmd.ExecuteNonQuery() == 1)
                Console.WriteLine("Создан новый пользователь");
            else
                Console.WriteLine("Ошибка создания нового пользователя");

            db.closeDBConnection();
            loginUser(username, password);
        }

        /// <summary>
        /// Получаение уникального идентификатора пользователя по логину
        /// </summary>
        /// <param name="username">Логин пользователя</param>
        /// <returns></returns>
        private int getUserIdFromName(string username)
        {
            int id = -1;
            DataBase db = new DataBase();
            DataTable table = new DataTable();
            MySqlDataAdapter adapter = new MySqlDataAdapter();
            MySqlCommand cmd = new MySqlCommand("SELECT * FROM `users` WHERE `login` = @user", db.getDBConnection());
            cmd.Parameters.Add("@user", MySqlDbType.VarChar).Value = username;
            adapter.SelectCommand = cmd;
            adapter.Fill(table);

            if (table.Rows.Count > 0)
                id = Convert.ToInt32(table.Rows[0]["id"]);
            
            return id;
        }

        /// <summary>
        /// Проверка пользователя на подключение к серверу
        /// </summary>
        /// <param name="username">имя пользователя</param>
        /// <returns></returns>
        public bool getUserConnect(string username)
        {
            var user = users.Find(i => i.Name == username);
            if (user == null)
                return false;
            
            return user.isConnected;
        }


        /// <summary>
        /// Получаем аватар с помощью имени пользователя
        /// </summary>
        /// <param name="name">имя пользователя</param>
        /// <returns></returns>
        public string getUserAvatar(string name)
        {
            var user = users.Find(i => i.Name == name);
            if (user != null)
                return user.Avatar;
            
            return null;
        }


        /// <summary>
        /// Перенос сообщений на новую строку при достижении n символов
        /// </summary>
        /// <param name="str">сообщение</param>
        /// <param name="n">кол-во симоволов для переноса строки</param>
        /// <returns></returns>
        public static string SplitToLines(string str, int n)
        {
            return Regex.Replace(str, ".{" + n + "}(?!$)", "$0\n");
        }
        
        /// <summary>
        /// Отправка сообщения пользователям, подключенным к серверу
        /// </summary>
        /// <param name="msg">сообщение</param>
        /// <param name="id">уникальный идентификатор пользователя, по которому ищем имя отправителя</param>
        public void SendMessage(string msg, int id)
        {
            foreach(var item in users)
            {
                string answer = DateTime.Now.ToShortTimeString();

                var user = users.FirstOrDefault(i => i.ID == id);
                if (user != null)
                {
                    answer += ": " + user.Name + ": ";
                }
                answer += msg;
                item.operationContext.GetCallbackChannel<IServerChatCallback>().MessageCallback(SplitToLines(answer,100));
            }
        }


        /// <summary>
        /// Получаем список подключенных пользователей к серверу
        /// </summary>
        public void SendUserList()
        {
            //SendUserData(_user.ID, _user.Avatar, _user.Name);
            foreach (var user in users)
            {
                user.operationContext.GetCallbackChannel<IServerChatCallback>().UserConnectedCallback(userList, userAvatars);
            }
        }
    }
}
