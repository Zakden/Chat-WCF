# WCF Chat Project Документация
В решении приложения - 3 проекта:
Chat_Client - клиентская часть приложения
ChatHost - Консольное приложение, запускающее сервер на определенном хосте
wcf_chat - Серверная часть проекта, которая отвечают за всю логику и работу между клиент-серверной частью
Для правильной настройки приложения следуйте следующим шагам:
1. Для начала создайте базу данных (MySQL в моем случае) с названием 'chat'
2. Внутри базы данных создайте таблицу с названием 'users' со следующими полями: id (Тип int, Primare Key, Auto Increment), login (Тип varchar), password (Тип varchar), avatar (Тип varchar, Default: 'user_standart.png')

3. В проекте wcf_chat сделайте следующие шаги:
  3.1. Откройте класс DataBase.cs и настройте подключение к базе данных
  3.2. Откройте файл app.config и в строке dns value="localhost" измените на своё значение. Если сервер будет на локальной машине - оставьте как есть, если сервер будет общедоступным для всех из сети - поменяйте на свой ip адрес

4. В проекте ChatHost сделайте следующие шаги:
  4.1. Откройте файл App.config и в строке <add baseAddress="http://localhost:8301/"/> и <add baseAddress="net.tcp://localhost:8302/"/> - поменяйте на свой ip адрес или оставьте значение по умолчанию. Также убедитесь что     порты не заняты другими приложениями

5. В проекте Chat_Client сделайте следующие шаги:
  5.1. Откройте файл App.config и в строке <endpoint address="net.tcp://localhost:8302/" binding="netTcpBinding" - поменяйте подключение на ip-адрес сервера, к которому будет идти подключение с клиента
  5.2. Откройте MainWindow.xaml.cs и измените строку string address = "localhost"; и int port = 8301; на IP адрес и порт сервера

7. Запустите приложение с помощью проекта ChatHost, Вы увидите консольное приложение и успешность запуска
8. Запустите клиент приложения и введите имя и пароль пользователя - данные должны будут записаться в базу данных

