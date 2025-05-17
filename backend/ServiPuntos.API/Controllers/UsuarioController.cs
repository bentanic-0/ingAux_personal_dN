﻿using Microsoft.AspNetCore.Mvc;
using ServiPuntos.Core.Entities;
using ServiPuntos.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiPuntos.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioService _iUsuarioService;

        public UsuarioController(IUsuarioService usuarioService)
        {
            _iUsuarioService = usuarioService;
        }

        // Obtener todos los usuarios.

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var usuarios = await _iUsuarioService.GetAllUsuariosAsync();
            return Ok(usuarios);
        }

        // Obtener un usuario por ID.

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUsuario(Guid id)
        {
            var usuario = await _iUsuarioService.GetUsuarioAsync(id);
            return usuario == null ? NotFound() : Ok(usuario);
        }

        // Crear usuario

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] Usuario usuario)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            usuario.Id = Guid.NewGuid();
            usuario.FechaCreacion = DateTime.UtcNow;
            usuario.FechaModificacion = DateTime.UtcNow;

            // Encriptar contraseña si viene en texto plano
            if (!string.IsNullOrEmpty(usuario.Password))
                usuario.Password = BCrypt.Net.BCrypt.HashPassword(usuario.Password);

            await _iUsuarioService.AddUsuarioAsync(usuario);
            return CreatedAtAction(nameof(GetUsuario), new { id = usuario.Id }, usuario);
        }


        // Actualizar un usuario existente.
    
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] Usuario usuario)
        {
            var existente = await _iUsuarioService.GetUsuarioAsync(id);
            if (existente == null)
                return NotFound();

            // Actualiza solo los campos necesarios
            existente.Nombre = usuario.Nombre;
            existente.Email = usuario.Email;
            existente.FechaModificacion = DateTime.UtcNow;
            existente.Puntos = usuario.Puntos;
            existente.TenantId = usuario.TenantId;

            // Actualizar contraseña si se proporciona
            if (!string.IsNullOrWhiteSpace(usuario.Password))
                existente.Password = BCrypt.Net.BCrypt.HashPassword(usuario.Password);

            await _iUsuarioService.UpdateUsuarioAsync(existente);
            return NoContent();
        }

        // Eliminar un usuario por ID

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var usuario = await _iUsuarioService.GetUsuarioAsync(id);
            if (usuario == null)
                return NotFound();

            await _iUsuarioService.DeleteUsuarioAsync(id);
            return NoContent();
        }
    }
}
