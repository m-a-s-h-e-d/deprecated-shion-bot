using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Schema
{
    public class Users
    {
        private readonly SchemaContext _context;

        public Users(SchemaContext context)
        {
            _context = context;
        }

        public async Task ModifyEmbedColor(ulong uid, string hexColor)
        {
            var user = await _context.Users
                .FindAsync(uid);

            if (user == null)
                _context.Add(new User { UserId = uid, EmbedColor = hexColor });
            else
                user.EmbedColor = hexColor;

            await _context.SaveChangesAsync();
        }

        public async Task<uint> GetEmbedColor(ulong uid)
        {
            if (await _context.Users.FindAsync(uid) == null)
            {
                var rnd = new Random();
                var newColor = $"{rnd.Next(0x1000000):X6}";
                _context.Add(new User { UserId = uid, EmbedColor = newColor });
                await _context.SaveChangesAsync();
            }

            var color = await _context.Users
                .Where(x => x.UserId == uid)
                .Select(x => x.EmbedColor)
                .FirstOrDefaultAsync();

            return await Task.FromResult(uint.Parse(color, System.Globalization.NumberStyles.HexNumber));
        }
    }
}
