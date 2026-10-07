using Microsoft.AspNetCore.Http;
using CarShowRoom.DAL.Enums;
using System;
using System.Collections.Generic;

public class RentOrderCreateDto
{
    public int CarId { get; set; }
    public string? UserNotes { get; set; } 
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public List<IFormFile>? Documents { get; set; }
}