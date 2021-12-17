using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shion.Data.Models
{
    /// <summary>
    /// The class that represents a Guild entity from the database.
    /// </summary>
    public class Guild
    {
        public ulong Id { get; set; }
        public string Name { get; set; }
    }
}
