﻿namespace ServiPuntos.Application.DTOs
{
    public class UsuarioDto
    {
        public int Id { get; set; }              // o int, según tu modelo
        public string Nombre { get; set; }
        public string Email { get; set; }
        public int Puntos { get; set; }

        // Agregás solo lo necesario para transferir, nunca todo el modelo completo
    }
}
