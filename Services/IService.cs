using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SALT
{
    /// <summary>Marks a class as a service handler</summary>
    public interface IService
    {
    }

    /// <summary>Marks a class as a internal service handler</summary>
    internal interface IServiceInternal : IService
    {
    }
}
