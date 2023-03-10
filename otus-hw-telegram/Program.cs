using Microsoft.VisualBasic;
using System;
using System.Globalization;
using Telegram.Bot;
using Telegram.Bot.Types;
using static System.Net.Mime.MediaTypeNames;


Console.WriteLine("Hello, World!");

// перечень клиентов, в перспективе они будут в ЬД

var customers = new List<Customer> {
    new Customer("Oleg", "R", "admin11"),
        new Customer("Igor", "Fet", "specialist"),
        new Customer("Sergei", "Ivanov", "Danon"),
};

DateTime h = new DateTime(2022, 1, 1, 01, 01, 00);



var tickets = new List<Ticket> {
    new Ticket (1, "alarm", "Danon", h, "ivan"),
    };




// перечень приветствий от Бота в зависимости от роли

const string helpTextClient = @"
- /NewTicket - открыть тикет
- /StatusTicket - узнать статус тикета";

const string helpTextAdmin = @"
- /StatusTicket - узнать статус тикета
- /NewCustomer - авторизовать нового пользователя";

const string helpTextSpecialist = @"
- /WorkTicket - узнать тикеты в работе
- /SolveTicket - решить тикет";




var mode = AppMode.Default;





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
            await client.SendTextMessageAsync(update.Message.Chat.Id, "Введите кратко название прблемы или /exit");
            break;

        case "/StatusTicket":
            mode = AppMode.GetStatus;
            await client.SendTextMessageAsync(update.Message.Chat.Id, "Введите номер вашего тикета или /exit");
            break;

        case "/NewCustomer":
            mode = AppMode.NewCustomer;
            await client.SendTextMessageAsync(update.Message.Chat.Id, "Введите имя, фамилию, роль или компанию, через запятую или /exit");
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

        foreach (var ll in tickets)
        {
            Console.WriteLine(ll.Number + ll.Name + ll.Client + ll.Created + ll.Specialist + ll.TicketStatus);
        }
        await client.SendTextMessageAsync(update.Message.Chat.Id,
            $"Тикет успешно создан");             // '{text}'");
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
            await client.SendTextMessageAsync(update.Message.Chat.Id,
            $"Ура, вы выиграли приз!!!! {ticketFound.TicketStatus}");
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

        string input = text;
        string[] words = text.Split(new char[] { ',' });
        customers.Add(new Customer(words[0], words[1], words[2]));

        foreach (string s in words)
        {
            Console.WriteLine(s);
        }


        foreach (var f in customers)
            Console.WriteLine(f.FirstName + f.LastName + f.Role);

        await client.SendTextMessageAsync(update.Message.Chat.Id,
            $"Пользователь успешно добавлен");
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



    }



    /*
    await client.SendTextMessageAsync(chatId,
           $"Здравствуйте, я помогу Вам 😀");


    await client.SendTextMessageAsync(chatId, 
        $"вы отправли сообщение '*{message.Text}*'",
        Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);

  
    await client.SendTextMessageAsync(chatId,
            $"напиши ещё что нибудь 😀");

    */

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
    Меня зовут {me.Username}, к сожалению я не нашёл Вас";


    }

}

enum AppMode
{
    Default = 0,
    OpenTicket = 1,
    GetStatus = 2,
    NewCustomer = 3,
    WorkTicket = 4,
    SolveTicket = 5,
}

