using System;
using System.Security.Cryptography.X509Certificates;

public class Customer
{

	public Customer() { }
    public Customer(string _nameFirst, string _nameLast, string _role)
	{
		FirstName = _nameFirst;
		LastName = _nameLast;
		Role = _role;

	}

	public int Id { get; set; } // для БД
	public string FirstName { get; set; }
	public string LastName { get; set; }
	public string Role { get; set; }




}
