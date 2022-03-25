﻿using System;
using System.Collections.Generic;

namespace Repository.Models
{
    public partial class OpPuesto
    {
        public OpPuesto()
        {
            Liquidacions = new HashSet<Liquidacion>();
            OpComposicions = new HashSet<OpComposicion>();
            OpDetalleLiquidacions = new HashSet<OpDetalleLiquidacion>();
        }

        public int Id { get; set; }
        public string Detalle { get; set; } = null!;
        public int Agrupacion { get; set; }

        public virtual OpAgrupacion AgrupacionNavigation { get; set; } = null!;
        public virtual ICollection<Liquidacion> Liquidacions { get; set; }
        public virtual ICollection<OpComposicion> OpComposicions { get; set; }
        public virtual ICollection<OpDetalleLiquidacion> OpDetalleLiquidacions { get; set; }
    }
}
