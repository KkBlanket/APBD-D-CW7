﻿namespace APBD_D_CW7.Models.DTOs;

public class ClientGetDTO
{
    public int IdClient { get; set; }
    public string FirstName {get; set;}
    public string LastName {get; set;}
    public string Email {get; set;}
    public string Telephone {get; set;}
    public string Pesel {get; set;}
}