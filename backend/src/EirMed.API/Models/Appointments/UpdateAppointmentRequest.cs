using System.ComponentModel.DataAnnotations;

namespace EirMed.API.Models.Appointments;

public record UpdateAppointmentRequest(
    [Required(ErrorMessage = "A data da consulta é obrigatória.")]
    DateTime Data,

    [Required(ErrorMessage = "A especialidade é obrigatória.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "A especialidade deve ter entre 2 e 100 caracteres.")]
    string Especialidade,

    [StringLength(500, ErrorMessage = "A queixa principal deve ter no máximo 500 caracteres.")]
    string? QueixaPrincipal,

    [StringLength(2000, ErrorMessage = "As observações gerais devem ter no máximo 2000 caracteres.")]
    string? ObservacoesGerais,

    [StringLength(2000, ErrorMessage = "Os diagnósticos devem ter no máximo 2000 caracteres.")]
    string? Diagnosticos,

    [Required(ErrorMessage = "O profissional de saúde é obrigatório.")]
    Guid DoctorId
);
