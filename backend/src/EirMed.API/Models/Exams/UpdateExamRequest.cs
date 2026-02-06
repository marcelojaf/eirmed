using System.ComponentModel.DataAnnotations;
using EirMed.Domain.Enums;

namespace EirMed.API.Models.Exams;

public record UpdateExamRequest(
    [Required(ErrorMessage = "O tipo de exame é obrigatório.")]
    ExamType TipoExame,

    [Required(ErrorMessage = "O nome do exame é obrigatório.")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 200 caracteres.")]
    string Nome,

    [Required(ErrorMessage = "A data de realização é obrigatória.")]
    DateTime DataRealizacao,

    DateTime? DataResultado,

    [StringLength(200, ErrorMessage = "O laboratório deve ter no máximo 200 caracteres.")]
    string? Laboratorio,

    [StringLength(4000, ErrorMessage = "Os resultados devem ter no máximo 4000 caracteres.")]
    string? Resultados,

    [StringLength(500, ErrorMessage = "A URL do arquivo deve ter no máximo 500 caracteres.")]
    string? FileUrl
);
