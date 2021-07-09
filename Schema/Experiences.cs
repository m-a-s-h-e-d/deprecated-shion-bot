using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Schema
{
    public class Experiences
    {
        private readonly SchemaContext _context;

        public Experiences(SchemaContext context)
        {
            _context = context;
        }

        public async Task ModifyExperience(ulong uid, long expChange)
        {
            var user = await _context.Experiences
                .FindAsync(uid);

            if (user == null)
                _context.Add(new Experience { UserId = uid, Exp = expChange, Level = 1 });
            else
                user.Exp += expChange;

            await _context.SaveChangesAsync();

            // Check if the user can level up
            //TODO await ModifyLevel(uid);
        }

        public async Task<long> GetExperience(ulong uid)
        {
            if (await _context.Experiences.FindAsync(uid) == null)
            {
                _context.Add(new Experience { UserId = uid, Exp = 0, Level = 1 });
                await _context.SaveChangesAsync();
            }

            var experience = await _context.Experiences
                .Where(x => x.UserId == uid)
                .Select(x => x.Exp)
                .FirstOrDefaultAsync();

            return await Task.FromResult(experience);
        }

        //TODO Update level when level up threshold is reached
        /*public async Task ModifyLevel(ulong uid)
        {
            var level = await _context.Experiences
                .Where(x => x.UserId == uid)
                .Select(x => x.Level)
                .FirstOrDefaultAsync();

            var currentExperience = await _context.Experiences
                .Where(x => x.UserId == uid)
                .Select(x => x.Experience)
                .FirstOrDefaultAsync();

            if (currentExperience / (5 * (level ^ 2) + (25 * level) + 100) > 0)

            await _context.SaveChangesAsync();
        }*/

        public async Task<long> GetLevel(ulong uid)
        {
            if (await _context.Experiences.FindAsync(uid) == null)
            {
                _context.Add(new Experience { UserId = uid, Exp = 0, Level = 1 });
                await _context.SaveChangesAsync();
            }

            var level = await _context.Experiences
                .Where(x => x.UserId == uid)
                .Select(x => x.Level)
                .FirstOrDefaultAsync();

            return await Task.FromResult(level);
        }
    }
}
