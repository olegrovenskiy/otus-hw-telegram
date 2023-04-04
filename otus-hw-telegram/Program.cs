﻿using Microsoft.VisualBasic;
using System;
using System.Data;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;
using static System.Net.Mime.MediaTypeNames;
using System.Linq;
using System.Runtime.Intrinsics.X86;

using (UserContext db = new UserContext())
{

//---------------------------------------
    // создаем два объекта User
    User user1 = new User { Name = "Tom", Age = 33 };
    User user2 = new User { Name = "Sam", Age = 26 };

    // добавляем их в бд
    db.Users.Add(user1);
    db.Users.Add(user2);
    db.SaveChanges();

    //Customer testDB = new Customer("Oleg", "Rov", "specialist");
    //db.Customers.Add(testDB);

    //db.Customers.Add(new Customer ("Oleg", "Rov", "specialist" ));


    Console.WriteLine("Объекты успешно сохранены");

    // получаем объекты из бд и выводим на консоль
    var users = db.Users;
    Console.WriteLine("Список объектов:");
    foreach (User u in users)
    {
        Console.WriteLine("{0}.{1} - {2}", u.Id, u.Name, u.Age);
    }








    //--------------------------------


    Console.WriteLine("Hello, World!");

    // ввод администратора

    Console.WriteLine("Введите данныё администратора системы");

    Console.WriteLine("Введите Имя");

    string AdminFirstName = Console.ReadLine();

    Console.WriteLine("Введите Фамилию");

    string AdminLastName = Console.ReadLine();


    // -------------------------------




    // перечень клиентов,для тестов
   // db.Customers.Find(2).Role = "Danon";

    db.Customers.Add(new Customer(AdminFirstName, AdminLastName, "admin"));
    db.Customers.Add(new Customer("Oleg", "Ran", "specialist"));
    db.Customers.Add(new Customer("Sergei", "Ivanov", "Danon"));
    db.Customers.Add(new Customer("Igor", "Fet", "specialist"));
    db.SaveChanges();


    var customers = db.Customers;


    Console.WriteLine("Список customers:");
    foreach (Customer cc in db.Customers)
    {
        Console.WriteLine("{0}.{1} - {2}  -- {3}", cc.Id, cc.FirstName, cc.LastName, cc.Role);
    }

    
    DateTime h = new DateTime(2022, 1, 1, 01, 01, 00);



    var tickets = new List<Ticket> {
    new Ticket (1, "alarm---", "Danon", h),
    new Ticket (2, "resolev", "DanonInt", h),
    new Ticket (3, "resolev fair", "DanonInt", h)
    };

    tickets.First().TicketStatus = Ticket.Status.OnWork;
    tickets.First().Specialist = "OlegR";


    db.Tickets.Add(new TicketDB("alarm1---DB", "Danon", h));
    db.Tickets.Add(new TicketDB("alarm2---DB", "Danon", h));
    db.SaveChanges();

    var ticketFound = db.Tickets.OrderByDescending(obj => obj.Id).FirstOrDefault();


    Console.WriteLine("FOUND DB" + ticketFound.Id + ticketFound.Name);



    var tttt = db.Tickets.First();
    Console.WriteLine(tttt.Name+tttt.Specialist+tttt.TicketStatus);


    db.Tickets.First().Specialist = "URA";
    db.SaveChanges();
    var ttt = db.Tickets.First();
    Console.WriteLine(ttt.Name+ttt.Specialist+ttt.TicketStatus);




    // перечень приветствий от Бота в зависимости от роли

    const string helpTextClient = @"
- /NewTicket - открыть тикет
- /StatusClientTicket - узнать статус тикета";

    const string helpTextAdmin = @"
- /StatusTicket - узнать статус тикета
- /NewCustomer - авторизовать нового пользователя";

    const string helpTextSpecialist = @"
- /GetOpenTickets - узнать тикеты в работе;
- /SolveTicket - решить тикет";



    int route = 1;
    var mode = AppMode.Default;
    TicketDB foundTicket2 = null;



    string token = "6130961419:AAHxjGCytBkTml-ssEvmXrwLlxIWEurPnDo";

    var client = new TelegramBotClient(token);

    var me = await client.GetMeAsync();


    client.StartReceiving(UpdateHandler, ErrorHandler);


    // по умолчанию
    async Task ErrorHandler(ITelegramBotClient arg1, Exception arg2, CancellationToken arg3)
    {
        throw new NotImplementedException();
    }






    #region DefaultHandler

    async Task DefaultHandler(
        ITelegramBotClient client,
        Update update,
        CancellationToken ct)
    {
        var text = update.Message.Text;


        switch (text)
        {
            case "/start":
                await client.SendTextMessageAsync(update.Message.Chat.Id, GetGreeting(update.Message.Chat));
                break;

            case "/NewTicket":
                mode = AppMode.OpenTicket;
                await client.SendTextMessageAsync(update.Message.Chat.Id, "У вас есть проблемы?");
                await client.SendTextMessageAsync(update.Message.Chat.Id, "Опишите кратко название прблемы или /exit");
                break;

            case "/StatusTicket":
                mode = AppMode.GetStatus;

                await client.SendTextMessageAsync(update.Message.Chat.Id, $"В системе заведено {db.Tickets.Count()} тикетов");
                await client.SendTextMessageAsync(update.Message.Chat.Id, "Введите номер тикета информацию о котором вы хотите получить или /exit");
                break;


            case "/StatusClientTicket":
                mode = AppMode.GetStatusClient;

                //search client company name

                var foundCustomerCompany = db.Customers.FirstOrDefault(x => x.FirstName.Contains(update.Message.Chat.FirstName)
                        && x.LastName.Contains(update.Message.Chat.LastName));


                // search clшent tickets

                var ClientTickets = from tt in db.Tickets
                                    where tt.Client.Contains(foundCustomerCompany.Role)
                                    orderby tt.Id
                                    select tt;

                await client.SendTextMessageAsync(update.Message.Chat.Id, $"Компанией {foundCustomerCompany.Role} системе были заведены следующие тикеты");

                foreach (var t in ClientTickets)
                    await client.SendTextMessageAsync(update.Message.Chat.Id, $"Тикет {t.Id} с проблемой {t.Name} " +
                        $"зарегестрирован {t.Created} имеет статус {t.TicketStatus}");

                await client.SendTextMessageAsync(update.Message.Chat.Id, "Введите номер решённоги тикета информацию о решении которого вы хотите получить или /exit");
                break;



            case "/NewCustomer":
                mode = AppMode.NewCustomer;
                await client.SendTextMessageAsync(update.Message.Chat.Id, "Введите латинецей без цифр имя, фамилию, роль или компанию, через запятую или /exit");
                break;

            case "/GetOpenTickets":
                mode = AppMode.GetOpenTickets;
                await client.SendTextMessageAsync(update.Message.Chat.Id, "Есть следующие открытые тикеты:");
                await client.SendTextMessageAsync(update.Message.Chat.Id, "Номер     Имя    Заказчик");
               
             
                foreach (TicketDB tt in db.Tickets)
                {
                    if (tt.TicketStatus == 0)
                        await client.SendTextMessageAsync(update.Message.Chat.Id, $"   {tt.Id}       {tt.Name}    {tt.Client} ");

                }

                await client.SendTextMessageAsync(update.Message.Chat.Id, "Введите номер тикета с которым планируете работать или /exit");

                break;


            case "/SolveTicket":
                mode = AppMode.SolveTicket;

                // вывод назначеных тикетов
                await client.SendTextMessageAsync(update.Message.Chat.Id, "У вас в работе следующие тикеты");

                string first = update.Message.Chat.FirstName;
                string last = update.Message.Chat.LastName;
                string firstlast = first + last;
                Console.WriteLine(firstlast);

                var SignedTickets = from tt in db.Tickets
                                    where tt.Specialist.Contains(firstlast)         
                                    where tt.TicketStatus == TicketDB.Status.OnWork
                                    orderby tt.Id
                                    select tt;

                foreach (var t in SignedTickets)
                    await client.SendTextMessageAsync(update.Message.Chat.Id, $"номер {t.Id}, проблема {t.Name}, заказчик {t.Client}");

                await client.SendTextMessageAsync(update.Message.Chat.Id, "Введите номер решённого тикета или /exit");
                break;


            default:
                await client.SendTextMessageAsync(update.Message.Chat.Id, "Я не понял команду :( отправь пожалуйста еще раз");
                break;
        }
    }

    #endregion



    #region NewTicketHandler
    async Task NewTicketHandler(
        ITelegramBotClient client,
        Update update,
        CancellationToken ct)
    {
        var text = update.Message?.Text?.Trim();

        if (text == "/exit")
        {
            mode = AppMode.Default;
            await client.SendTextMessageAsync(update.Message.Chat.Id, "Пока");
        }
        else if (!string.IsNullOrEmpty(text))
        {
            var foundCustomer = db.Customers.FirstOrDefault(x => x.FirstName.Contains(update.Message.Chat.FirstName)
            && x.LastName.Contains(update.Message.Chat.LastName));

            // создание нового тикета, статус по умолчанию прописан как опен


            string cust = foundCustomer.Role;
            DateTime h1 = update.Message.Date;
            db.Tickets.Add(new TicketDB (text, cust, h1));
            db.SaveChanges();

            Console.WriteLine("Add ticket to DB");

            var tt = db.Tickets.OrderByDescending(obj => obj.Id).FirstOrDefault();

            Console.WriteLine("FOUND DB LAST" + tt.Id + tt.Name);

            await client.SendTextMessageAsync(update.Message.Chat.Id,
                $"Тикет номер {tt.Id} успешно создан");         

            await client.SendTextMessageAsync(update.Message.Chat.Id, "Если остались проблемы, то создайте новый тикет или /exit");

        }

    }
    #endregion


    #region TicketStatusHandler
    async Task TicketStatusHandler(
        ITelegramBotClient client,
        Update update,
        CancellationToken ct)
    {
        var text = update.Message?.Text?.Trim();

        if (text == "/exit")
        {
            mode = AppMode.Default;
            await client.SendTextMessageAsync(update.Message.Chat.Id, "Пока");
        }
        else if (!string.IsNullOrEmpty(text))
        {
            int TicketId = int.Parse(text);


            TicketDB ticketFound = db.Tickets.Find(TicketId);


            if (ticketFound != null)
            {


                if (ticketFound.Specialist != null)
                {
                    await client.SendTextMessageAsync(update.Message.Chat.Id, $"тикет {ticketFound.Id}  с проблемой {ticketFound.Name} " +
                        $"назначен на {ticketFound.Specialist} " +
                        $"статус {ticketFound.TicketStatus} создан {ticketFound.Created}");
                }
                else
                    await client.SendTextMessageAsync(update.Message.Chat.Id, $"тикет {ticketFound.Id}  с проблемой {ticketFound.Name} " +
                                        $"не назначен на специалиста, cоздан {ticketFound.Created}");


            }
            else
            {
                await client.SendTextMessageAsync(update.Message.Chat.Id,
                    "Я не нашел такого тикета");
            }
            await client.SendTextMessageAsync(update.Message.Chat.Id, "Введите тикет для поиска или /exit");
        }

    }
    #endregion


    #region TicketClientStatusHandler
    async Task TicketClientStatusHandler(
        ITelegramBotClient client,
        Update update,
        CancellationToken ct)
    {
        var text = update.Message?.Text?.Trim();

        if (text == "/exit")
        {
            mode = AppMode.Default;
            await client.SendTextMessageAsync(update.Message.Chat.Id, "Пока");
        }
        else if (!string.IsNullOrEmpty(text))
        {

            var foundCustomerCompany = db.Customers.FirstOrDefault(x => x.FirstName.Contains(update.Message.Chat.FirstName)
                                && x.LastName.Contains(update.Message.Chat.LastName));


            var ticketFound = db.Tickets.FirstOrDefault(x => x.Id.ToString().Contains(text) && x.Client.Contains(foundCustomerCompany.Role)
             && x.TicketStatus == TicketDB.Status.Closed);



            if (ticketFound != null)
            {


                await client.SendTextMessageAsync(update.Message.Chat.Id, $"По тикету {ticketFound.Id} специалистом было предложено следующее решение:");
                await client.SendTextMessageAsync(update.Message.Chat.Id, ticketFound.Solution);




            }
            else
            {
                await client.SendTextMessageAsync(update.Message.Chat.Id,
                    "Я не нашел такого тикета");
            }
            await client.SendTextMessageAsync(update.Message.Chat.Id, "Введите тикет для поиска или /exit");
        }

    }
    #endregion


    #region NewCustomerHandler
    async Task NewCustomerHandler(
        ITelegramBotClient client,
        Update update,
        CancellationToken ct)
    {
        var text = update.Message?.Text?.Trim();



        if (text == "/exit")
        {
            mode = AppMode.Default;
            await client.SendTextMessageAsync(update.Message.Chat.Id, "Пока");
        }
        else if (!string.IsNullOrEmpty(text))
        {



            Regex regex = new Regex(@"^[a-zA-Z]+,[A-Za-z]+,[A-Za-z]+$");
            if (regex.IsMatch(text))
            {


                string[] words = text.Split(new char[] { ',' });

                Console.WriteLine($" 1 - {words[0]}  2-- {words[1]}   3 --- {words[2]}");

                // проверка что такое имя и фимилия существует 



                string first = words[0]
;               string second = words[1];
                var cust = from word in db.Customers
                           where word.LastName == second
                           where word.FirstName == first
                           select word.FirstName;
      

               
                var ff = cust.FirstOrDefault();

                Console.WriteLine(ff);


                if (ff == words[0]) await client.SendTextMessageAsync(update.Message.Chat.Id, $"Пользователь {words[0]} уже существует");


                else
                {



                    db.Customers.Add(new Customer(words[0], words[1], words[2]));

                    db.SaveChanges();
                    await client.SendTextMessageAsync(update.Message.Chat.Id,
                        $"Пользователь {words[0]}  {words[1]}  {words[2]}  успешно добавлен");
                }
                await client.SendTextMessageAsync(update.Message.Chat.Id, "Введите данные ещё одного пользователя или /exit");



            }

            else
                await client.SendTextMessageAsync(update.Message.Chat.Id, "Некорректный ввод, повторите или /exit");




        }



    }
    #endregion



    #region GetOpenTicketHandler
    async Task GetOpenTicketsHandler(
        ITelegramBotClient client,
        Update update,
        CancellationToken ct)
    {
        var text = update.Message?.Text?.Trim();

        var tr1 = db.Tickets.Find(1);
        Console.WriteLine("gggg" + tr1.Id + tr1.Name + tr1.Specialist + tr1.TicketStatus);

        if (text == "/exit")
        {
            mode = AppMode.Default;
            await client.SendTextMessageAsync(update.Message.Chat.Id, "Пока");
        }
        else if (!string.IsNullOrEmpty(text))
        {
            // обработка введёного номера тикета    

            int TicketId = int.Parse(text);


            TicketDB foundTicket = db.Tickets.Find(TicketId);


            if (foundTicket != null)
            {

                // перевод тикета в работу и назначение специалиста

                db.Tickets.Find(TicketId).TicketStatus = TicketDB.Status.OnWork;

                db.Tickets.Find(TicketId).Specialist = $"{update.Message.Chat.FirstName}{update.Message.Chat.LastName}";

                db.SaveChanges();

                var tr = db.Tickets.Find(1);
                Console.WriteLine("uuu" + tr.Id + tr.Name + tr.Specialist + tr.TicketStatus);


                await client.SendTextMessageAsync(update.Message.Chat.Id, $"Тикет номер {text} назначен вам в работу");
                await client.SendTextMessageAsync(update.Message.Chat.Id, "Будете работать ещё с одним тикетом или /exit");

            }
            else
            {
                await client.SendTextMessageAsync(update.Message.Chat.Id,
                    $"Неправильный номер тикета '{text}'");
                await client.SendTextMessageAsync(update.Message.Chat.Id, "Введите номер тикета ещё раз или /exit");

            }


        }

    }
    #endregion



    #region SolveTicketHandler
    async Task SolveTicketsHandler(
        ITelegramBotClient client,
        Update update,
        CancellationToken ct)
    {
        var text = update.Message?.Text?.Trim();

        var tr = db.Tickets.Find(1);
        Console.WriteLine("xxxxx" + tr.Id + tr.Name + tr.Specialist + tr.TicketStatus);

        if (text == "/exit")
        {
            mode = AppMode.Default;
            await client.SendTextMessageAsync(update.Message.Chat.Id, "Пока");
        }




        else if (!string.IsNullOrEmpty(text))


            switch (route)
            {

                case 1:
                    {
                        //-------------------
                        {
                            
                                                      
                            
                            var foundTicket1 = db.Tickets.FirstOrDefault(x => x.Id.ToString().Equals(text)
                             && x.Specialist.Contains(update.Message.Chat.FirstName + update.Message.Chat.LastName) && x.TicketStatus == TicketDB.Status.OnWork);

                            foundTicket2 = foundTicket1;

                            if (foundTicket1 != null)
                            {
                                await client.SendTextMessageAsync(update.Message.Chat.Id, "  Запишите решение или");
                                route = 2;
                            }
                            else
                                await client.SendTextMessageAsync(update.Message.Chat.Id, "Вdедите тикет который у вас в работе или /exit");
                        }
                        //------------------------------------
                    }
                    break;

                case 2:
                    foundTicket2.TicketStatus = TicketDB.Status.Closed; /// ????????????
                    foundTicket2.Solution = text;  ////???????????????


                    await client.SendTextMessageAsync(update.Message.Chat.Id, " Тикет успешно закрыт ");
                    route = 1;
                    await client.SendTextMessageAsync(update.Message.Chat.Id, "Будете работать ещё с одним тикетом или /exit");
                    break;


            }

        db.SaveChanges();
        var tr2 = db.Tickets.Find(1);
        Console.WriteLine("sssss" + tr2.Id + tr2.Name + tr2.Specialist + tr2.TicketStatus + tr2.Solution);

    }
    #endregion



    #region UpdateHandler
    async Task UpdateHandler(ITelegramBotClient client,
        Update update,
        CancellationToken ct)

    {
        var message = update.Message;

        if (update.Type == Telegram.Bot.Types.Enums.UpdateType.MyChatMember)
        {
            return;
        }


        var chatId = update.Message.Chat.Id;

        if (message != null)
        {
            // Console.WriteLine($"Пришло новое сообщение  '{message.Text}'  дата:  {message.Date} ");
        }

        switch (mode)
        {
            case AppMode.Default:
                await DefaultHandler(client, update, ct);
                break;

            case AppMode.GetStatus:
                await TicketStatusHandler(client, update, ct);
                break;


            case AppMode.GetStatusClient:
                await TicketClientStatusHandler(client, update, ct);
                break;


            case AppMode.OpenTicket:
                await NewTicketHandler(client, update, ct);
                break;

            case AppMode.NewCustomer:
                await NewCustomerHandler(client, update, ct);
                break;


            case AppMode.GetOpenTickets:
                await GetOpenTicketsHandler(client, update, ct);
                break;

            case AppMode.SolveTicket:
                await SolveTicketsHandler(client, update, ct);
                break;


        }

    }
    #endregion



    Console.ReadKey();

    #region Metod Greeting
    string GetGreeting(Chat chat)
    {

        Console.WriteLine(chat.FirstName + "   " + chat.LastName);


        var foundCustomer = db.Customers.FirstOrDefault(x => x.FirstName.Contains(chat.FirstName) && x.LastName.Contains(chat.LastName)); // customer
        if (foundCustomer != null)
        {
            if (foundCustomer.Role == "admin")
            {
                return $@"
    Здравствуйте Администратор - {chat.FirstName} {chat.LastName}!
    Меня зовут {me.Username}, и я помогу Вам:
    {helpTextAdmin}
        ";
            }

            else if (foundCustomer.Role == "specialist")
            {
                return $@"
     Здравствуйте Специалист - {chat.FirstName} {chat.LastName}!
     Меня зовут {me.Username}, и я помогу Вам:
     {helpTextSpecialist}
        ";
            }

            else

            {
                return $@"
     Здравствуйте, {chat.FirstName} {chat.LastName}!
     Меня зовут {me.Username}, и я рад помочь компании {foundCustomer.Role}:
     {helpTextClient}
    ";
            }

        }


        else

        {

            return $@"
    Привет, {chat.FirstName} {chat.LastName}!
    Меня зовут {me.Username}, к сожалению я не нашёл Вас, обратитесь к Администратору по support@support.ru";


        }

    }
    #endregion

}
enum AppMode
{
    Default = 0,
    OpenTicket = 1,
    GetStatus = 2,
    NewCustomer = 3,
    GetOpenTickets = 4,
    SolveTicket = 5,
    GetStatusClient = 6,
}

