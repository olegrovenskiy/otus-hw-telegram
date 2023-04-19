using System;

public class Ticket
{
	public int Id { get; set; }
	public string Name { get; set; }

	public string Client { get; set; }
	public DateTime Created { get; set; }
	
	public string Specialist { get; set; }

	public Status TicketStatus { get; set; }

	public string Solution { get; set; }


	public Ticket(int _number, string _name, string _client, DateTime _created)
		{
		Id = _number;
		Name = _name;
		Client = _client;
		Created = _created;
		Specialist = "empty";
		TicketStatus = Status.Open;
		Solution = null;
		}

	/*
	public void SendTicketDataToDB()
	{

		Console.WriteLine($"запись в базу {Number}  , {Name}");

	}

	*/

    public enum Status
    {
        Open = 0,
        Closed = 1,
		OnWork = 2,
    }

}


// файл лишний