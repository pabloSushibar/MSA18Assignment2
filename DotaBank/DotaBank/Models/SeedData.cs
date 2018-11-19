using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotaBank.Models
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new DotaBankContext(
                serviceProvider.GetRequiredService<DbContextOptions<DotaBankContext>>()))
            {
                // Look for any movies.
                if (context.DotaItem.Count() > 0)
                {
                    return;   // DB has been seeded
                }

                context.DotaItem.AddRange(
                    new DotaItem
                    {
                        Title = "Is Mayo an Instrument?",
                        Url = "https://i.kym-cdn.com/photos/images/original/001/371/723/be6.jpg",
                        Tags = "spongebob",
                        Uploaded = "07-10-18 4:20T18:25:43.511Z",
                        Width = "768",
                        Height = "432"
                    }


                );
                context.SaveChanges();
            }
        }
    }
}
