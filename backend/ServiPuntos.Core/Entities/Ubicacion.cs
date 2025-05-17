﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiPuntos.Core.Entities
{
    public class Ubicacion
    {
        public Guid Id { get; set; }

        public required Guid TenantId { get; set; }
        public Tenant? Tenant { get; set; }

        public string? Nombre { get; set; }
        public string? Direccion { get; set; }
        public string? Ciudad { get; set; }
        public string? Departamento { get; set; }
        public string? TelefonoContacto { get; set; }

        public DateTime FechaCreacion { get; set; }
        public DateTime FechaModificacion { get; set; }

        public TimeSpan HoraApertura { get; set; }
        public TimeSpan HoraCierre { get; set; }

        public bool Lavado { get; set; }
        public bool CambioAceite { get; set; }

        public decimal PrecioNaftaSuper { get; set; }
        public decimal PrecioNaftaPremium { get; set; }
        public decimal PrecioDiesel { get; set; }
        public List<ProductoUbicacion> ProductosLocales { get; set; } = new List<ProductoUbicacion>();
        public List<Promocion> Promociones { get; set; } = new List<Promocion>();

        //Constructor
        public Ubicacion() { }
        public Ubicacion(Guid tenantId, string nombre, string direccion, string ciudad, string departamento, string telefonoContacto, TimeSpan horaApertura, TimeSpan horaCierre)
        {
            TenantId = tenantId;
            Nombre = nombre;
            Direccion = direccion;
            Ciudad = ciudad;
            Departamento = departamento;
            TelefonoContacto = telefonoContacto;
            HoraApertura = horaApertura;
            HoraCierre = horaCierre;
        }
    }
}
