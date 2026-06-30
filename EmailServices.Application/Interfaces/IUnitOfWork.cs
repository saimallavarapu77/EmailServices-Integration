using System;
using System.Collections.Generic;
using System.Text;

namespace EmailServices.Application.Interfaces;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync();
}
