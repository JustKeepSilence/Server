using Microsoft.AspNetCore.Mvc;
using Server.Commons;

namespace Server.Controllers;

public class CustomController(ILogger<CustomController> logger) : Controller{


    private readonly ILogger<CustomController> Logger_ = logger;

   


}