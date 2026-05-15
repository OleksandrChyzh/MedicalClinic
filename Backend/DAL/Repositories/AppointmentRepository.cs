using DAL.Data;
using DAL.Entities;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

public class AppointmentRepository : BaseRepository<Appointment>, IAppointmentRepository
{
    public AppointmentRepository(AppDbContext context) : base(context) { }

    public async Task UpdateStatusAsync(Appointment appointment)
    {
        if (this.Context.Entry(appointment).State == EntityState.Detached)
        {
            this.DbSet.Attach(appointment);
        }

        this.Context.Entry(appointment).Property(a => a.Status).IsModified = true;
        await this.Context.SaveChangesAsync();
    }
}
