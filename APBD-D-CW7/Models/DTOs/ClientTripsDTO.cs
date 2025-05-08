namespace APBD_D_CW7.Models.DTOs;

public class ClientTripsDTO
{
    public int IdClient { get; set; }
    public List<TripRegistrationDTO> Trips { get; set; } = new List<TripRegistrationDTO>();
    
}