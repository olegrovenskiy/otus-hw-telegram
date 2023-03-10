using System;

public class Ticket
{
	public int Number { get; set; }
	public string Name { get; set; }

	public string Type { get; set; }
	
	public Ticket()
	{
	}

	public void SendTicketDataToDB()
	{

		Console.WriteLine($"запись в базу {Number}  , {Name}");

	}


}
