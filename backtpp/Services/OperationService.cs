﻿using backtpp.Interfaces;
using backtpp.Models;
using backtpp.Modelsdtos.Commons;
using backtpp.Modelsdtos.Operations;
using Microsoft.Data.SqlClient;
using System.Data;

namespace backtpp.Services
{
    public class OperationService : IOperationService
    {
        private readonly IStoreProcedure _storeProcedure;

        private readonly tppContext _context;
        public OperationService(IStoreProcedure storeProcedure, tppContext context)
        {
            _storeProcedure = storeProcedure;
            _context = context;
        }

        public Operacion Add(Operacion operacion)
        {
            _context.Operacions.Add(operacion);
            _context.SaveChanges();
            return operacion;
        }

        public IEnumerable<OperationModel> GetAll(bool? finalizada = false)
        {
            List<SqlParameter> Params = new();
            Params.Add(new SqlParameter("@finalizada", finalizada));
            DataTable dt = _storeProcedure.SpWhithDataSet("OperationGetAll", Params);
            List<OperationModel> lst = new();
            foreach (DataRow row in dt.Rows)
            {
                lst.Add(new OperationModel()  // -- >     Al Automapper
                {
                    Id = (long)row["Id"],
                    Inicio = (DateTime)row["Inicio"],
                    Destino = row["Destino"].ToString(),
                    Cliente = row["Cliente"].ToString(),
                    Esquema = row["Esquema"].ToString(),
                    IdEsquema = (int)row["IdEsquema"],
                    Puerto = (int)row["Puerto"]
                });
            }
            return lst;
        }

        public IEnumerable<ManiobraModel> GetManiobrasActivas(bool? finalizada = false)
        {
            List<SqlParameter> Params = new();
            Params.Add(new SqlParameter("@finalizada", finalizada));
            DataTable dt = _storeProcedure.SpWhithDataSet("ManiobrasGetActivas", Params);
            List<ManiobraModel> lst = new();
            foreach (DataRow row in dt.Rows)
            {
                lst.Add(new ManiobraModel()  // -- >     Al Automapper
                {
                    Id = (long)row["Id"],
                    Fecha = (DateTime)row["Fecha"],
                    Turno = (int)row["Turno"],
                    Maniobra = (int)row["Maniobra"],
                    TurnoDesc = row["TurnoDesc"].ToString(),
                    ManiobraNombre = row["ManiobraNombre"].ToString(),
                    Produccion = (decimal)row["Produccion"],
                    Lluvia = (bool)row["Lluvia"],
                    Insalubre = (bool)row["Insalubre"],
                    Sobrepeso = (bool)row["Sobrepeso"],
                    Operacion = (long)row["Operacion"],
                });
            }
            return lst;
        }

        public IEnumerable<Turno> GetTurnoByEsquema(int esquema)
        {
            IQueryable<Turno>? turnos = from t in _context.Turnos
                                        where t.Esquema == esquema
                                        select t;
            return turnos;
        }

        public IEnumerable<OpManiobra> GetManiobrasAll()
        {
            return _context.OpManiobras.ToList();
        }

        public bool AddManiobraOperacion(OperacionManiobra operacionManiobra)
        {
            _context.OperacionManiobras.Add(operacionManiobra);
            _context.SaveChanges();
            return true;
        }

        public bool UpdateManiobraOperacion(OperacionManiobra operacionManiobra)
        {
            OperacionManiobra? op = _context.OperacionManiobras.Find(operacionManiobra.Id);
            if (op == null)
            {
                return false;
            }

            if (op.Finalizada == true)
            {
                throw new Exception("No se pueden modificar las Maniobras finalizadas");
            }
            _context.ChangeTracker.Clear();
            _context.OperacionManiobras.Update(operacionManiobra);
            _context.SaveChanges();
            return true;
        }

        public IEnumerable<ComposicionManiobraDto> GetComposicionByManiobra(long id)
        {
            try
            {

                List<SqlParameter> Params = new();
                Params.Add(new SqlParameter("@id", id));
                DataTable dt = _storeProcedure.SpWhithDataSet("ManiobrasGetComposicionByid", Params);
                List<ComposicionManiobraDto> lst = new();
                foreach (DataRow row in dt.Rows)
                {
                    lst.Add(new ComposicionManiobraDto()  // -- >     Al Automapper
                    {
                        IdManiobra = (int)row["IdManiobra"],
                        IdPuesto = (int)row["IdPuesto"],
                        Operacion = (long)row["Operacion"],
                        Cantidad = (int)row["Cantidad"],
                        Liquidadas = (int)row["Liquidadas"],
                        Puesto = row["Puesto"].ToString(),
                    });
                }
                return lst;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public IEnumerable<LiquidacionByManiobraDto> GetLiquidacionByManiobra(long op, int puesto)
        {
            List<SqlParameter> Params = new();
            Params.Add(new SqlParameter("@Op", op));
            Params.Add(new SqlParameter("@Puesto", puesto));
            DataTable dt = _storeProcedure.SpWhithDataSet("ManiobrasGetLiquidacionesByOp", Params);
            List<LiquidacionByManiobraDto> lst = new();
            foreach (DataRow row in dt.Rows)
            {
                lst.Add(new LiquidacionByManiobraDto()  // -- >     Al Automapper
                {
                    Operacion = (long)row["Operacion"],
                    Turno = (int)row["Turno"],
                    Horario = row["Horario"].ToString(),
                    Fecha = (DateTime)row["Fecha"],
                    Puesto = row["Puesto"].ToString(),
                    IdPuesto = (int)row["IdPuesto"],
                    Liquidacion = (long)row["Liquidacion"],
                    IdEmpleado = (long)row["IdEmpleado"],
                    Empleado = (long)row["Empleado"],
                    Nombre = row["Nombre"].ToString(),
                    Tipo = row["Tipo"].ToString(),
                    Color = row["Color"].ToString(),
                    Abierta = (bool)row["Abierta"],
                    Confirmada = (bool)row["Confirmada"],
                    Pagado = (bool)row["Pagado"],
                    Llave = (decimal)row["Llave"],
                });
            }
            return lst;
        }

        public bool LiquidacionesCerrarByOpByPuesto(long operacion, int puesto, string operador)
        {
            try
            {

                List<SqlParameter> Params = new();
                Params.Add(new SqlParameter("@operacion", operacion));
                Params.Add(new SqlParameter("@puesto", puesto));
                Params.Add(new SqlParameter("@operador", operador));
                bool res = _storeProcedure.ExecuteNonQuery("LiquidacionesCerrarByOpByPuesto", Params);

                return res;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public bool LiquidacionesLLave(long operacion, int puesto, int llave)
        {
            try
            {

                List<SqlParameter> Params = new();
                Params.Add(new SqlParameter("@operacion", operacion));
                Params.Add(new SqlParameter("@puesto", puesto));
                Params.Add(new SqlParameter("@llave", llave));
                bool res = _storeProcedure.ExecuteNonQuery("LiquidacionesLLave", Params);

                return res;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public bool LiquidacionesConfirmarByOpByPuesto(long operacion, int puesto, string operador, int perfil)
        {
            List<SqlParameter> Params = new();
            Params.Add(new SqlParameter("@operacion", operacion));
            Params.Add(new SqlParameter("@puesto", puesto));
            Params.Add(new SqlParameter("@operador", operador));
            Params.Add(new SqlParameter("@perfil", perfil));
            _storeProcedure.ExecuteNonQuery("LiquidacionesConfirmarByOpByPuesto", Params);

            return true;
        }

        public bool LiquidacionesReabrirByOpByPuesto(long operacion, int puesto, string operador)
        {
            try
            {

                List<SqlParameter> Params = new();
                Params.Add(new SqlParameter("@operacion", operacion));
                Params.Add(new SqlParameter("@puesto", puesto));
                Params.Add(new SqlParameter("@operador", operador));
                bool res = _storeProcedure.ExecuteNonQuery("LiquidacionesReabrirByOpByPuesto", Params);

                return res;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public LiquidacionesByOp LiquidacionesByOp(long operacion)
        {
            try
            {
                List<SqlParameter> Params = new();
                Params.Add(new SqlParameter("@operacion", operacion));
                DataSet ds = _storeProcedure.SpWhithDataSetPure("LiquidacionesByOp", Params);
                LiquidacionesByOp lst = new();
                List<OperationModel> operationModels = new();
                List<OperacionManiobraModel> maniobraModels = new();
                List<LiquidacionModel> liquidacionModels = new();
                List<LiquidacionDetalleModel> liquidacionDetalleModels = new();
                List<EmpresaDto> empresaDtos = new();
                DataTable dtOp = new();
                DataTable dtMan = new();
                DataTable dtLiq = new();
                DataTable dtDet = new();
                DataTable dtEmp = new();
                dtOp = ds.Tables[0];
                dtMan = ds.Tables[1];
                dtLiq = ds.Tables[2];
                dtDet = ds.Tables[3];
                dtEmp = ds.Tables[4];
                foreach (DataRow rowOp in dtOp.Rows)
                {
                    operationModels.Add(new OperationModel()
                    {
                        Id = (long)rowOp["Id"],
                        Inicio = (DateTime)rowOp["Inicio"],
                        Destino = rowOp["Destino"].ToString(),
                        Cliente = rowOp["Cliente"].ToString(),
                        Esquema = rowOp["Esquema"].ToString(),
                        IdEsquema = (int)rowOp["IdEsquema"],
                    });
                    foreach (DataRow rowMan in dtMan.Rows)
                    {
                        maniobraModels.Add(new OperacionManiobraModel()
                        {
                            IdManiobra = (long)rowMan["IdManiobra"],
                            Operacion = (long)rowMan["Operacion"],
                            Horario = rowMan["Horario"].ToString(),
                            Maniobra = rowMan["Maniobra"].ToString(),
                            Produccion = (decimal)rowMan["Produccion"],
                            Lluvia = (bool)rowMan["Lluvia"],
                            Sobrepeso = (bool)rowMan["Sobrepeso"],
                            Insalubre = (bool)rowMan["Insalubre"],
                            Fecha = (DateTime)rowMan["Fecha"],
                        });
                    };
                    foreach (DataRow rowLiq in dtLiq.Rows)
                    {
                        liquidacionModels.Add(new LiquidacionModel()
                        {
                            Liquidacion = (long)rowLiq["Liquidacion"],
                            Maniobra = (long)rowLiq["Maniobra"],
                            IdEmpleado = (long)rowLiq["IdEmpleado"],
                            Cuit = (long)rowLiq["Cuit"],
                            Nombre = rowLiq["Nombre"].ToString(),
                            IdPuesto = (int)rowLiq["IdPuesto"],
                            Puesto = rowLiq["Puesto"].ToString(),
                            Llave = (decimal)rowLiq["Llave"],
                            Haberes = (decimal)rowLiq["Haberes"],
                            Descuentos = (decimal)rowLiq["Descuentos"],
                            Remunerativos = (decimal)rowLiq["Remunerativos"],
                            NoRemunerativos = (decimal)rowLiq["NoRemunerativos"],
                            Neto = (decimal)rowLiq["Neto"],
                            EnLetras = rowLiq["EnLetras"].ToString(),
                            Cbu = rowLiq["Cbu"].ToString(),
                        });
                    };
                    foreach (DataRow rowDet in dtDet.Rows)
                    {
                        liquidacionDetalleModels.Add(new LiquidacionDetalleModel()
                        {
                            Liquidacion = (long)rowDet["Liquidacion"],
                            Maniobra = (long)rowDet["Maniobra"],
                            Puesto = (int)rowDet["Puesto"],
                            Codigo = rowDet["Codigo"].ToString(),
                            Concepto = rowDet["Concepto"].ToString(),
                            Cantidad = (decimal)rowDet["Cantidad"],
                            Monto = (decimal)rowDet["Monto"],
                            Haber = (bool)rowDet["Haber"],
                            Remunerativo = (bool)rowDet["Remunerativo"],
                            IdEmpleado = (long)rowDet["IdEmpleado"],
                        });
                    };
                    foreach (DataRow rowEmp in dtEmp.Rows)
                    {
                        empresaDtos.Add(new EmpresaDto()
                        {
                            Contacto = rowEmp["Contacto"].ToString(),
                            Cuit = rowEmp["Cuit"].ToString(),
                            Razon = rowEmp["Razon"].ToString(),
                            Domicilio = rowEmp["Domicilio"].ToString(),
                        });
                    };
                }
                lst.OperationModel = operationModels;
                lst.OperacionManiobra = maniobraModels;
                lst.LiquidacionModel = liquidacionModels;
                lst.LiquidacionDetalleModel = liquidacionDetalleModels;
                lst.EmpresaDtos = empresaDtos;
                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public bool LiquidacionesAdd(List<LiquidacionInsert> liquidacion)
        {
            try
            {
                List<SqlParameter> Params = new();
                if (liquidacion != null)
                {
                    SqlParameter Param = new("@liquidacion", AuxDataTable.ToDataTable(liquidacion))
                    {
                        TypeName = "LiquidacionTable",
                        SqlDbType = SqlDbType.Structured
                    };
                    Params.Add(Param);
                };
                bool res = _storeProcedure.ExecuteNonQuery("LiquidacionesAdd", Params);
                return res;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public bool LiquidacionesDelete(long operacion, int puesto)
        {
            try
            {


                List<SqlParameter> Params = new();
                Params.Add(new SqlParameter("@operacion", operacion));
                Params.Add(new SqlParameter("@puesto", puesto));
                return _storeProcedure.ExecuteNonQuery("LiquidacionesDeleteByOpByPuesto", Params);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public bool LiquidacionesDelete(long liquidacion)
        {
            try
            {

                Liquidacion? liq = _context.Liquidacions.Find(liquidacion);
                if (liq == null)
                {
                    return false;
                }

                if (liq.Abierta == false)
                {
                    throw new Exception("No Puede Eliminar Liquidaciones Cerradas");
                }

                _context.Liquidacions.Remove(liq);
                _context.SaveChanges();
                return true;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public bool ManiobrasFinalizar(long operacion)
        {
            try
            {

                List<SqlParameter> Params = new();
                Params.Add(new SqlParameter("@operacion", operacion));
                bool res = _storeProcedure.ExecuteNonQuery("ManiobrasFinalizar", Params);

                return res;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public LiquidacionesByOp LiquidacionesPayPending()
        {
            try
            {
                DataSet ds = _storeProcedure.SpWhithDataSetPure("EmpleadosPagosPendientes");
                LiquidacionesByOp lst = new();

                List<LiquidacionModel> liquidacionModels = new();
                List<EmpresaDto> empresaDtos = new();
                List<LiquidacionesPagos> liquidacionPagos = new();

                DataTable dtLiq = new();
                DataTable dtPagos = new();
                DataTable dtEmp = new();

                dtLiq = ds.Tables[0];
                dtPagos = ds.Tables[1];
                dtEmp = ds.Tables[2];

                foreach (DataRow rowLiq in dtLiq.Rows)
                {
                    liquidacionModels.Add(new LiquidacionModel()
                    {
                        Liquidacion = (long)rowLiq["Liquidacion"],
                        IdEmpleado = (long)rowLiq["IdEmpleado"],
                        Cuit = (long)rowLiq["Cuit"],
                        Nombre = rowLiq["Nombre"].ToString(),
                        Puesto = rowLiq["Puesto"].ToString(),
                        Llave = (decimal)rowLiq["Llave"],
                        Neto = (decimal)rowLiq["Neto"],
                        Cbu = rowLiq["Cbu"].ToString(),
                        Fecha = (DateTime)rowLiq["Fecha"],
                        Turno = rowLiq["Turno"].ToString(),
                        Destino = rowLiq["Destino"].ToString(),
                    });
                };

                foreach (DataRow rowPagos in dtPagos.Rows)
                {
                    liquidacionPagos.Add(new LiquidacionesPagos()
                    {
                        IdEmpleado = (long)rowPagos["IdEmpleado"],
                        Cuit = rowPagos["Cuit"].ToString(),
                        Nombre = rowPagos["Nombre"].ToString(),
                        Cbu = rowPagos["Cbu"].ToString(),
                        Total = (decimal)rowPagos["Total"],
                    });
                };

                foreach (DataRow rowEmp in dtEmp.Rows)
                {
                    empresaDtos.Add(new EmpresaDto()
                    {
                        Cuit = rowEmp["Cuit"].ToString(),
                    });
                };

                lst.LiquidacionModel = liquidacionModels;
                lst.LiquidacionesPagos = liquidacionPagos;
                lst.EmpresaDtos = empresaDtos;
                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public IEnumerable<LiquidacionesLotePago> LiquidacionesPay(List<LiquidacionPay> liquidaciones)
        {
            try
            {
                List<SqlParameter> Params = new();
                if (liquidaciones != null)
                {
                    SqlParameter Param = new("@liquidacion", AuxDataTable.ToDataTable(liquidaciones))
                    {
                        TypeName = "LiquidacionesTable",
                        SqlDbType = SqlDbType.Structured
                    };
                    Params.Add(Param);
                };
                DataTable? res = _storeProcedure.SpWhithDataSet("LiquidacionesPay", Params);
                List<LiquidacionesLotePago> lst = new();
                foreach (DataRow rowPagos in res.Rows)
                {
                    lst.Add(new LiquidacionesLotePago()
                    {
                        Cantidad = (int)rowPagos["Cantidad"],
                        Cuit = rowPagos["Cuit"].ToString(),
                        Lote = (long)rowPagos["Lote"],

                    });
                };
                return lst;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public bool SacLiquida(int semestre, int año, string operador)
        {
            try
            {

                List<SqlParameter> Params = new();
                Params.Add(new SqlParameter("@semestre", semestre));
                Params.Add(new SqlParameter("@año", año));
                Params.Add(new SqlParameter("@operador", operador));
                bool res = _storeProcedure.ExecuteNonQuery("SacLiquida", Params);

                return res;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public bool SacConfirma(int semestre, int año, string operador)
        {
            try
            {

                List<SqlParameter> Params = new();
                Params.Add(new SqlParameter("@semestre", semestre));
                Params.Add(new SqlParameter("@año", año));
                Params.Add(new SqlParameter("@operador", operador));
                bool res = _storeProcedure.ExecuteNonQuery("SacConfirma", Params);

                return res;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public bool SacReabre(int semestre, int año, string operador)
        {
            try
            {

                List<SqlParameter> Params = new();
                Params.Add(new SqlParameter("@semestre", semestre));
                Params.Add(new SqlParameter("@año", año));
                Params.Add(new SqlParameter("@operador", operador));
                bool res = _storeProcedure.ExecuteNonQuery("SacReabre", Params);

                return res;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public IEnumerable<LiquidacionesLotePago> SacPay(List<LiquidacionPay> liquidaciones)
        {
            try
            {
                List<SqlParameter> Params = new();
                if (liquidaciones != null)
                {
                    SqlParameter Param = new("@liquidacion", AuxDataTable.ToDataTable(liquidaciones))
                    {
                        TypeName = "LiquidacionesTable",
                        SqlDbType = SqlDbType.Structured
                    };
                    Params.Add(Param);
                };
                DataTable? res = _storeProcedure.SpWhithDataSet("SacPay", Params);
                List<LiquidacionesLotePago> lst = new();
                foreach (DataRow rowPagos in res.Rows)
                {
                    lst.Add(new LiquidacionesLotePago()
                    {
                        Cantidad = (int)rowPagos["Cantidad"],
                        Cuit = rowPagos["Cuit"].ToString(),
                        Lote = (long)rowPagos["Lote"],

                    });
                };
                return lst;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public SacByPeriodo GetSacByPeriodo(int? año)
        {
            try
            {
                if (año is null)
                {
                    año = DateTime.Now.Year;
                }

                List<SqlParameter> Params = new();
                Params.Add(new SqlParameter("@año", año));
                DataSet ds = _storeProcedure.SpWhithDataSetPure("SacByAño", Params);
                SacByPeriodo lst = new();
                List<SacPeriod> sacPeriod = new();
                List<SacModel> sacModel = new();
                List<LiquidacionDetalleModel> liquidacionDetalleModels = new();
                List<EmpresaDto> empresaDtos = new();
                DataTable dtSac = new();
                DataTable dtModel = new();
                DataTable dtDet = new();
                DataTable dtEmp = new();
                dtSac = ds.Tables[0];
                dtModel = ds.Tables[1];
                dtDet = ds.Tables[2];
                dtEmp = ds.Tables[3];
                foreach (DataRow rowSac in dtSac.Rows)
                {
                    sacPeriod.Add(new SacPeriod()
                    {
                        Semestre = (int)rowSac["Semestre"],
                        Año = (int)rowSac["Año"],
                    });

                    foreach (DataRow rowMod in dtModel.Rows)
                    {
                        sacModel.Add(new SacModel()
                        {
                            Liquidacion = (long)rowMod["Liquidacion"],
                            Fecha = (DateTime)rowMod["Fecha"],
                            Semestre = (int)rowMod["Semestre"],
                            Año = (int)rowMod["Año"],
                            Empleado = (long)rowMod["Empleado"],
                            Cuil = rowMod["Cuil"].ToString(),
                            Cbu = rowMod["Cbu"].ToString(),
                            Nombre = rowMod["Nombre"].ToString(),
                            Haberes = (decimal)rowMod["Haberes"],
                            Descuentos = (decimal)rowMod["Descuentos"],
                            Remunerativos = (decimal)rowMod["Remunerativos"],
                            NoRemunerativos = (decimal)rowMod["NoRemunerativos"],
                            Neto = (decimal)rowMod["Neto"],
                            Confirmada = (bool)rowMod["Confirmada"],
                            Operador = rowMod["Operador"].ToString(),
                            OperadorConfirma = rowMod["OperadorConfirma"].ToString(),
                            Pagado = (bool)rowMod["Pagado"],
                            EnLetras = rowMod["EnLetras"].ToString(),
                        });
                    };
                    foreach (DataRow rowDet in dtDet.Rows)
                    {
                        liquidacionDetalleModels.Add(new LiquidacionDetalleModel()
                        {
                            Liquidacion = (long)rowDet["Liquidacion"],
                            Codigo = rowDet["Codigo"].ToString(),
                            Concepto = rowDet["Concepto"].ToString(),
                            Cantidad = (decimal)rowDet["Cantidad"],
                            Monto = (decimal)rowDet["Monto"],
                            Haber = (bool)rowDet["Haber"],
                            Remunerativo = (bool)rowDet["Remunerativo"],
                        });
                    };
                    foreach (DataRow rowEmp in dtEmp.Rows)
                    {
                        empresaDtos.Add(new EmpresaDto()
                        {
                            Contacto = rowEmp["Contacto"].ToString(),
                            Cuit = rowEmp["Cuit"].ToString(),
                            Razon = rowEmp["Razon"].ToString(),
                            Domicilio = rowEmp["Domicilio"].ToString(),
                        });
                    };
                }
                lst.SacPeriods = sacPeriod;
                lst.SacModels = sacModel;
                lst.LiquidacionDetalleModel = liquidacionDetalleModels;
                lst.EmpresaDtos = empresaDtos;
                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
