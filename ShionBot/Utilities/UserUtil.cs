using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace ShionBot.Utilities
{
    public class UserUtil
    {
        public static string GetFullUsername(IUser user)
        {
            return $"{user.Username}#{user.Discriminator}";
        }
    }
}
