using System;

public class Ticket
{
	public int Number { get; set; }
	public string Name { get; set; }

	public string Client { get; set; }
	public DateTime Created { get; set; }
	
	public string Specialist { get; set; }

	public Status TicketStatus { get; set; }
	public Ticket(int _number, string _name, string _client, DateTime _created, string _specialist)
		{
		Number = _number;
		Name = _name;
		Client = _client;
		Created = _created;
		Specialist = _specialist;
		TicketStatus = Status.Open;
		}

	public void SendTicketDataToDB()
	{

		Console.WriteLine($"запись в базу {Number}  , {Name}");

	}


    public enum Status
    {
        Open = 0,
        Closed = 1,

    }

}
