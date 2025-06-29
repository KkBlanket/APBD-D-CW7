﻿namespace APBD_D_CW7.Models.DTOs;

public class TripGetDTO
{
    public int IdTrip { get; set; }
    public string Name {get; set;}
    public string Description {get; set;}
    public DateTime DateFrom {get; set;}
    public DateTime DateTo {get; set;}
    public int MaxPeople {get; set;}
}