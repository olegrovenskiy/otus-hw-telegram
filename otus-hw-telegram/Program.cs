using Microsoft.VisualBasic;
using System;
using System.Data;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;
using static System.Net.Mime.MediaTypeNames;
using System.Linq;


Console.WriteLine("Hello, World!");

// перечень клиентов, в перспективе они будут в ЬД

var customers = new List<Customer> {
    new Customer("Oleg", "R", "specialist"),
        new Customer("Igor", "Fet", "specialist"),
        new Customer("Sergei", "Ivanov", "Danon"),
};

DateTime h = new DateTime(2022, 1, 1, 01, 01, 00);



var tickets = new List<Ticket> {
    new Ticket (1, "alarm", "Danon", h, null),
    new Ticket (2, "resolev", "DanonInt", h, "ivan sergeev"),

    };




// перечень приветствий от Бота в зависимости от роли

const string helpTextClient = @"
- /NewTicket - открыть тикет
- /StatusTicket - узнать статус тикета";

const string helpTextAdmin = @"
- /StatusTicket - узнать статус тикета
- /NewCustomer - авторизовать нового пользователя";

const string helpTextSpecialist = @"
- /GetOpenTickets - узнать тикеты в работе;
- /SolveTicket - решить тикет";



int route = 1;
var mode = AppMode.Default;
Ticket foundTicket2 = null;





string token = "6130961419:AAHxjGCytBkTml-ssEvmXrwLlxIWEurPnDo";

var client = new TelegramBotClient(token);

var me = await client.GetMeAsync();


client.StartReceiving(UpdateHandler, ErrorHandler);

//Customer aa = new Customer();

// по умолчанию
async Task ErrorHandler(ITelegramBotClient arg1, Exception arg2, CancellationToken arg3)
{
  throw new NotImplementedException();
}


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

            await client.SendTextMessageAsync(update.Message.Chat.Id, $"В системе заведено {tickets.Count} тикетов");
            await client.SendTextMessageAsync(update.Message.Chat.Id, "Введите номер тикета информацию о котором вы хотите получить или /exit");
            
            break;

        case "/NewCustomer":
            mode = AppMode.NewCustomer;
            await client.SendTextMessageAsync(update.Message.Chat.Id, "Введите латинецей без цифр имя, фамилию, роль или компанию, через запятую или /exit");
            break;

        case "/GetOpenTickets":
            mode = AppMode.GetOpenTickets;
            await client.SendTextMessageAsync(update.Message.Chat.Id, "Есть следующие открытые тикеты:");
            await client.SendTextMessageAsync(update.Message.Chat.Id, "Номер     Имя    Заказчик");
            foreach (Ticket tt in tickets)
            {
                if (tt.TicketStatus == 0)
                await client.SendTextMessageAsync(update.Message.Chat.Id, $"   {tt.Number}       {tt.Name}    {tt.Client} ");

            }
            await client.SendTextMessageAsync(update.Message.Chat.Id, "Введите номер тикета с которым планируете работать или /exit");

            break;


        case "/SolveTicket":
            mode = AppMode.SolveTicket;

            // нужен вывод назначеных тикетов
            await client.SendTextMessageAsync(update.Message.Chat.Id, "У вас в работе следующие тикеты");

            var SignedTickets = from tt in tickets
                                where tt.Specialist.Contains(update.Message.Chat.FirstName+update.Message.Chat.LastName)
                                where tt.TicketStatus == Ticket.Status.OnWork
                                orderby tt.Number
                                select tt;

            foreach (var t in SignedTickets)
                await client.SendTextMessageAsync(update.Message.Chat.Id, $"номер {t.Number}, проблема {t.Name}, заказчик {t.Client}");


            await client.SendTextMessageAsync(update.Message.Chat.Id, "Введите номер решённого тикета или /exit");
            break;




        default:
            await client.SendTextMessageAsync(update.Message.Chat.Id, "Я не понял команду :( отправь пожалуйста еще раз");
            break;
    }
}




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
        var foundCustomer = customers.FirstOrDefault(x => x.FirstName.Contains(update.Message.Chat.FirstName) 
        && x.LastName.Contains(update.Message.Chat.LastName));

 // создание нового тикета, статус по умолчанию прописан как опен

        tickets.Add(new Ticket (tickets.Last().Number+1, text, foundCustomer.Role, update.Message.Date, null));


        await client.SendTextMessageAsync(update.Message.Chat.Id,
            $"Тикет номер {tickets.Last().Number} успешно создан");             // '{text}'");

        await client.SendTextMessageAsync(update.Message.Chat.Id, "Если остались проблемы, то создайте новый тикет или /exit");

    }

}



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


        //    var foundCustomer = customers.FirstOrDefault(x => x.FirstName.Contains(chat.FirstName) && x.LastName.Contains(chat.LastName));

 
        var ticketFound = tickets.FirstOrDefault(x => x.Number.ToString().Contains(text));

        if (ticketFound != null)
        {


            //   await client.SendTextMessageAsync(update.Message.Chat.Id, "Номер     Имя    Специалист  Статус  Время создания");


            if (ticketFound.Specialist != null)
            {
                await client.SendTextMessageAsync(update.Message.Chat.Id, $"тикет {ticketFound.Number}  с проблемой {ticketFound.Name} " +
                    $"назначен на {ticketFound.Specialist} " +
                    $"статус {ticketFound.TicketStatus} создан {ticketFound.Created}");
            }
            else
                await client.SendTextMessageAsync(update.Message.Chat.Id, $"тикет {ticketFound.Number}  с проблемой {ticketFound.Name} " +
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

            customers.Add(new Customer(words[0], words[1], words[2]));


            await client.SendTextMessageAsync(update.Message.Chat.Id,
                $"Пользователь {words[0]}  {words[1]}  {words[2]}  успешно добавлен");

            await client.SendTextMessageAsync(update.Message.Chat.Id, "Введите данные ещё одного пользователя или /exit");

        }

        else
            await client.SendTextMessageAsync(update.Message.Chat.Id, "Некорректный ввод, повторите или /exit");




    }



}


    






async Task GetOpenTicketsHandler(
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
        // обработка введёного номера тикета    
        //var foundContact = contacts.FirstOrDefault(x => x.FirstName.ToLower() == text || x.LastName.ToLower() == text);

        var foundTicket = tickets.FirstOrDefault(x => x.Number.ToString().Equals(text));

        if (foundTicket != null)
        {

            // перевод тикета в работу и назначение специалиста

            foundTicket.TicketStatus = Ticket.Status.OnWork;
            foundTicket.Specialist = $"{update.Message.Chat.FirstName}{update.Message.Chat.LastName}";

            await client.SendTextMessageAsync(update.Message.Chat.Id, $"Тике номер {text} назначен вам в работу");
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





async Task SolveTicketsHandler(
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

        switch (route)
        {

            case 1:
                {
                    //-------------------
                    {
                        var foundTicket1 = tickets.FirstOrDefault(x => x.Number.ToString().Equals(text)
                         && x.Specialist.Contains(update.Message.Chat.FirstName + update.Message.Chat.LastName) && x.TicketStatus == Ticket.Status.OnWork);

                        foundTicket2 = foundTicket1;
                        
                        if (foundTicket1 != null)
                        {
                            await client.SendTextMessageAsync(update.Message.Chat.Id, "  Запишите решение или");
                            route = 2;
                        }
                        else
                            await client.SendTextMessageAsync(update.Message.Chat.Id, "ВВедите тикет который у вас в работе или /exit");
                    }
                    //------------------------------------
                }
                 break;

                case 2:
                foundTicket2.TicketStatus = Ticket.Status.Closed;
                foundTicket2.Solution = text;


                await client.SendTextMessageAsync(update.Message.Chat.Id, " Тикет успешно закрыт ");
                route = 1;
                await client.SendTextMessageAsync(update.Message.Chat.Id, "Будете работать ещё с одним тикетом или /exit");
                break;


        }

}






        // обработчик сообщений от клиента бота

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




Console.ReadKey();
string GetGreeting (Chat chat)
{

    Console.WriteLine(chat.FirstName + "   " + chat.LastName);


    var foundCustomer = customers.FirstOrDefault(x => x.FirstName.Contains(chat.FirstName) && x.LastName.Contains(chat.LastName));
    if (foundCustomer != null)
    {
        if (foundCustomer.Role == "admin")
        {
            return $@"
    Привет, {chat.FirstName} {chat.LastName}!
    Меня зовут {me.Username}, и я помогу Вам:
    {helpTextAdmin}
        ";
        }

        else if (foundCustomer.Role == "specialist")
        {
            return $@"
     Привет, {chat.FirstName} {chat.LastName}!
     Меня зовут {me.Username}, и я помогу Вам:
     {helpTextSpecialist}
        ";
        }

        else

        {
            return $@"
     Привет, {chat.FirstName} {chat.LastName}!
     Меня зовут {me.Username}, и я помогу Вам:
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

enum AppMode
{
    Default = 0,
    OpenTicket = 1,
    GetStatus = 2,
    NewCustomer = 3,
    GetOpenTickets = 4,
    SolveTicket = 5,
}

