using System.ComponentModel.DataAnnotations;

namespace EirMed.API.Models.Doctors;

public record UpdateDoctorRequest(
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 200 caracteres.")]
    string Nome,

    [Required(ErrorMessage = "A especialidade é obrigatória.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "A especialidade deve ter entre 2 e 100 caracteres.")]
    string Especialidade,

    [StringLength(50, ErrorMessage = "O registro profissional deve ter no máximo 50 caracteres.")]
    string? RegistroProfissional,

    [StringLength(200, ErrorMessage = "A clínica/hospital deve ter no máximo 200 caracteres.")]
    string? ClinicaHospital,

    [StringLength(200, ErrorMessage = "O contato deve ter no máximo 200 caracteres.")]
    string? Contato
);
