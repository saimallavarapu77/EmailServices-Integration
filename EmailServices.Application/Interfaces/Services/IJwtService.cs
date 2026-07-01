using System;
using System.Collections.Generic;
using System.Text;

namespace EmailServices.Application.Interfaces.Services;

public interface IJwtService
{
    string GenerateToken(int userId, string name, string email);
}

