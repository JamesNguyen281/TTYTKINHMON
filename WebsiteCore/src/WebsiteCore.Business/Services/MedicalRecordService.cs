using Microsoft.EntityFrameworkCore;
using WebsiteCore.Data;
using WebsiteCore.Data.Entities;

namespace WebsiteCore.Business.Services;

public interface IMedicalRecordService
{
    Task<List<MedicalRecord>> GetByPatientAsync(Guid patientUserId, int take = 100);
    Task<MedicalRecord?> GetByIdAsync(Guid id);
    Task<List<Prescription>> GetPrescriptionsAsync(Guid medicalRecordId);
    Task<string> NextRecordNoAsync();
    Task<Guid> CreateAsync(MedicalRecord input, IEnumerable<Prescription>? prescriptions, Guid staffUserId);
}

public class MedicalRecordService : IMedicalRecordService
{
    private readonly TtytlpDbContext _db;
    public MedicalRecordService(TtytlpDbContext db) => _db = db;

    public Task<List<MedicalRecord>> GetByPatientAsync(Guid patientUserId, int take = 100) =>
        _db.MedicalRecords
            .Where(m => m.PatientUserId == patientUserId && m.ActiveFlag == 1)
            .OrderByDescending(m => m.VisitDate)
            .Take(take)
            .ToListAsync();

    public Task<MedicalRecord?> GetByIdAsync(Guid id) =>
        _db.MedicalRecords.FirstOrDefaultAsync(m => m.Id == id);

    public Task<List<Prescription>> GetPrescriptionsAsync(Guid medicalRecordId) =>
        _db.Prescriptions
            .Where(p => p.MedicalRecordId == medicalRecordId)
            .ToListAsync();

    public async Task<string> NextRecordNoAsync()
    {
        // Format: HS<yyMMdd><4-digit-counter>
        var todayPrefix = "HS" + DateTime.Today.ToString("yyMMdd");
        var count = await _db.MedicalRecords.CountAsync(m => m.RecordNo!.StartsWith(todayPrefix));
        return $"{todayPrefix}{(count + 1):D4}";
    }

    public async Task<Guid> CreateAsync(MedicalRecord input, IEnumerable<Prescription>? prescriptions, Guid staffUserId)
    {
        if (input == null) throw new ArgumentNullException(nameof(input));
        // Validation hard-stops — toàn vẹn dữ liệu bệnh án (NOT NULL constraint)
        if (input.PatientUserId == Guid.Empty)
            throw new InvalidOperationException("PatientUserId là bắt buộc.");
        if (string.IsNullOrWhiteSpace(input.Diagnosis))
            throw new InvalidOperationException("Chẩn đoán là bắt buộc.");

        input.Id          = Guid.NewGuid();
        input.ActiveFlag  = 1;
        input.CreatedDate = DateTime.Now;
        input.CreatedBy   = staffUserId;

        // Race-safe RecordNo: retry tối đa 5 lần khi đụng unique constraint
        for (int attempt = 0; attempt < 5; attempt++)
        {
            if (string.IsNullOrEmpty(input.RecordNo) || attempt > 0)
                input.RecordNo = await NextRecordNoAsync();
            try
            {
                _db.MedicalRecords.Add(input);
                if (prescriptions != null)
                {
                    foreach (var p in prescriptions)
                    {
                        if (string.IsNullOrWhiteSpace(p.DrugName)) continue;
                        p.Id = Guid.NewGuid();
                        p.MedicalRecordId = input.Id;
                        _db.Prescriptions.Add(p);
                    }
                }
                await _db.SaveChangesAsync();
                return input.Id;
            }
            catch (DbUpdateException) when (attempt < 4)
            {
                // Detach và retry với RecordNo mới
                foreach (var entry in _db.ChangeTracker.Entries().ToList())
                    entry.State = EntityState.Detached;
            }
        }
        throw new InvalidOperationException("Không thể tạo bệnh án — thử lại sau.");
    }
}
