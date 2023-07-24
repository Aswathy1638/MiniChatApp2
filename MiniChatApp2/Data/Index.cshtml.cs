using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MiniChatApp2.Model;

namespace MiniChatApp2.Data
{
    public class IndexModel : PageModel
    {
        private readonly MiniChatApp2.Data.MiniChatApp2Context _context;

        public IndexModel(MiniChatApp2.Data.MiniChatApp2Context context)
        {
            _context = context;
        }

        public IList<User> User { get;set; } = default!;

        public async Task OnGetAsync()
        {
            if (_context.User != null)
            {
                User = await _context.User.ToListAsync();
            }
        }
    }
}
