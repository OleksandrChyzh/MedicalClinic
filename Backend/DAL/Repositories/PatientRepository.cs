using DAL.Data;
using DAL.Entities;
using DAL.Interfaces;


namespace DAL.Repositories;

public class PatientRepository : BaseRepository<Patient>, IPatientRepository
{
    public PatientRepository(AppDbContext context) : base(context) { }
}
