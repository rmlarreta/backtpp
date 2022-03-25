﻿using System;
using System.Collections.Generic;

namespace Repository.Models
{
    public partial class ProveedorImputacion
    {
        public ProveedorImputacion()
        {
            Clientes = new HashSet<Cliente>();
        }

        public string Id { get; set; } = null!;

        public virtual ICollection<Cliente> Clientes { get; set; }
    }
}
