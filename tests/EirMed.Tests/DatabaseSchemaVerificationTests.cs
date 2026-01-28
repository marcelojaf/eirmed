using EirMed.Domain.Entities;
using EirMed.Domain.Enums;
using EirMed.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EirMed.Tests;

public class DatabaseSchemaVerificationTests
{
    private static EirMedDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<EirMedDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new EirMedDbContext(options);
    }

    [Fact]
    public async Task Schema_CanCreateAllTables()
    {
        using var context = CreateContext();
        var created = await context.Database.EnsureCreatedAsync();
        Assert.True(created);
    }

    [Fact]
    public async Task Schema_CanInsertUser()
    {
        using var context = CreateContext();
        await context.Database.EnsureCreatedAsync();

        var user = new User
        {
            Id = Guid.NewGuid(),
            Nome = "João Silva",
            DataNascimento = new DateOnly(1990, 5, 15),
            TipoSanguineo = BloodType.OPositive,
            ObservacoesGerais = "Alérgico a penicilina"
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var saved = await context.Users.FindAsync(user.Id);
        Assert.NotNull(saved);
        Assert.Equal("João Silva", saved.Nome);
        Assert.Equal(BloodType.OPositive, saved.TipoSanguineo);
        Assert.NotEqual(default, saved.CreatedAt);
    }

    [Fact]
    public async Task Schema_CanInsertDoctorLinkedToUser()
    {
        using var context = CreateContext();
        await context.Database.EnsureCreatedAsync();

        var user = new User
        {
            Id = Guid.NewGuid(),
            Nome = "Maria Santos",
            DataNascimento = new DateOnly(1985, 3, 20)
        };

        var doctor = new Doctor
        {
            Id = Guid.NewGuid(),
            Nome = "Dr. Carlos Oliveira",
            Especialidade = "Cardiologia",
            RegistroProfissional = "CRM-SP 123456",
            ClinicaHospital = "Hospital São Paulo",
            UserId = user.Id,
            User = user
        };

        context.Users.Add(user);
        context.Doctors.Add(doctor);
        await context.SaveChangesAsync();

        var saved = await context.Doctors.Include(d => d.User).FirstAsync(d => d.Id == doctor.Id);
        Assert.Equal("Dr. Carlos Oliveira", saved.Nome);
        Assert.Equal(user.Id, saved.UserId);
        Assert.Equal("Maria Santos", saved.User.Nome);
    }

    [Fact]
    public async Task Schema_CanInsertFullAppointmentWithRelatedEntities()
    {
        using var context = CreateContext();
        await context.Database.EnsureCreatedAsync();

        var user = new User
        {
            Id = Guid.NewGuid(),
            Nome = "Ana Costa",
            DataNascimento = new DateOnly(1992, 7, 10)
        };

        var doctor = new Doctor
        {
            Id = Guid.NewGuid(),
            Nome = "Dra. Paula Mendes",
            Especialidade = "Ortopedia",
            UserId = user.Id,
            User = user
        };

        var medication = new Medication
        {
            Id = Guid.NewGuid(),
            Nome = "Ibuprofeno",
            PrincipioAtivo = "Ibuprofeno",
            Dosagem = "400mg",
            FormaFarmaceutica = PharmaceuticalForm.Tablet,
            TipoUso = MedicationUsageType.Occasional,
            QuantidadeAtual = 20,
            QuantidadeMinima = 5,
            UserId = user.Id,
            User = user
        };

        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            Data = DateTime.UtcNow,
            Especialidade = "Ortopedia",
            QueixaPrincipal = "Dor no joelho",
            ObservacoesGerais = "Paciente com dificuldade de locomoção",
            Diagnosticos = "Tendinite patelar",
            DoctorId = doctor.Id,
            Doctor = doctor
        };

        var exam = new Exam
        {
            Id = Guid.NewGuid(),
            TipoExame = ExamType.Image,
            Nome = "Ressonância Magnética do Joelho",
            DataRealizacao = DateTime.UtcNow,
            Laboratorio = "Lab Diagnósticos",
            AppointmentId = appointment.Id,
            Appointment = appointment
        };

        var prescription = new Prescription
        {
            Id = Guid.NewGuid(),
            DataInicio = DateOnly.FromDateTime(DateTime.Today),
            DataFim = DateOnly.FromDateTime(DateTime.Today.AddDays(14)),
            Motivo = "Alívio da dor e inflamação",
            MedicationId = medication.Id,
            Medication = medication,
            AppointmentId = appointment.Id,
            Appointment = appointment
        };

        var attachment = new AppointmentAttachment
        {
            Id = Guid.NewGuid(),
            FileName = "receita_medica.pdf",
            FileUrl = "https://storage.example.com/receita_medica.pdf",
            ContentType = "application/pdf",
            AppointmentId = appointment.Id,
            Appointment = appointment
        };

        context.Users.Add(user);
        context.Doctors.Add(doctor);
        context.Medications.Add(medication);
        context.Appointments.Add(appointment);
        context.Exams.Add(exam);
        context.Prescriptions.Add(prescription);
        context.AppointmentAttachments.Add(attachment);
        await context.SaveChangesAsync();

        // Verify all entities were saved
        Assert.Equal(1, await context.Users.CountAsync());
        Assert.Equal(1, await context.Doctors.CountAsync());
        Assert.Equal(1, await context.Medications.CountAsync());
        Assert.Equal(1, await context.Appointments.CountAsync());
        Assert.Equal(1, await context.Exams.CountAsync());
        Assert.Equal(1, await context.Prescriptions.CountAsync());
        Assert.Equal(1, await context.AppointmentAttachments.CountAsync());

        // Verify relationships
        var savedAppointment = await context.Appointments
            .Include(a => a.Doctor)
            .Include(a => a.Exams)
            .Include(a => a.Prescriptions)
            .Include(a => a.Attachments)
            .FirstAsync();

        Assert.Equal("Dra. Paula Mendes", savedAppointment.Doctor.Nome);
        Assert.Single(savedAppointment.Exams);
        Assert.Single(savedAppointment.Prescriptions);
        Assert.Single(savedAppointment.Attachments);
    }

    [Fact]
    public async Task Schema_TimestampsAreSetAutomatically()
    {
        using var context = CreateContext();
        await context.Database.EnsureCreatedAsync();

        var user = new User
        {
            Id = Guid.NewGuid(),
            Nome = "Teste Timestamp",
            DataNascimento = new DateOnly(2000, 1, 1)
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        Assert.NotEqual(default, user.CreatedAt);
        Assert.Null(user.UpdatedAt);

        // Update
        user.Nome = "Teste Atualizado";
        context.Users.Update(user);
        await context.SaveChangesAsync();

        Assert.NotNull(user.UpdatedAt);
    }

    [Fact]
    public async Task Schema_AllDbSetsAreAvailable()
    {
        using var context = CreateContext();

        Assert.NotNull(context.Users);
        Assert.NotNull(context.Doctors);
        Assert.NotNull(context.Appointments);
        Assert.NotNull(context.Exams);
        Assert.NotNull(context.Medications);
        Assert.NotNull(context.Prescriptions);
        Assert.NotNull(context.AppointmentAttachments);
    }

    [Fact]
    public async Task Schema_EnumValuesAreSavedCorrectly()
    {
        using var context = CreateContext();
        await context.Database.EnsureCreatedAsync();

        var user = new User
        {
            Id = Guid.NewGuid(),
            Nome = "Enum Test",
            DataNascimento = new DateOnly(1995, 6, 1),
            TipoSanguineo = BloodType.ABNegative
        };

        var medication = new Medication
        {
            Id = Guid.NewGuid(),
            Nome = "Medicamento Teste",
            Dosagem = "500mg",
            FormaFarmaceutica = PharmaceuticalForm.Capsule,
            TipoUso = MedicationUsageType.Continuous,
            UserId = user.Id,
            User = user
        };

        context.Users.Add(user);
        context.Medications.Add(medication);
        await context.SaveChangesAsync();

        var savedUser = await context.Users.FindAsync(user.Id);
        var savedMed = await context.Medications.FindAsync(medication.Id);

        Assert.Equal(BloodType.ABNegative, savedUser!.TipoSanguineo);
        Assert.Equal(PharmaceuticalForm.Capsule, savedMed!.FormaFarmaceutica);
        Assert.Equal(MedicationUsageType.Continuous, savedMed.TipoUso);
    }
}
