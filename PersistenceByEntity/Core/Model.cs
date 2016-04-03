using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    //Source: https://github.com/Horusiath/AkkaCQRS
    public class Transaccion
    {
        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;
        public string NumeroCuenta { get; set; }
        public Transaccion(string numeroCuenta, decimal monto)
        {
            Monto = monto;
            NumeroCuenta = numeroCuenta;
        }
    }

    public class TransaccionRegistrada
    {
        public Transaccion Transaccion { get; set; }
        public TransaccionRegistrada(Transaccion transaccion)
        {
            Transaccion = transaccion;
        }

    }

    public class ComandoRealizarTransaccion
    {
        public string CustomerId { get; set; }
        public Transaccion Transaccion { get; set; }
        public ComandoRealizarTransaccion(Transaccion transaccion, string customerId)
        {
            Transaccion = transaccion;
            CustomerId = customerId;
        }
        public override string ToString()
        {
            return $"{ Transaccion.NumeroCuenta} { Transaccion.Monto}";
        }
    }

    public class Tarjeta
    {
        public string Numero { get; set; }
        public string Cuenta { get; set; }

        public Tarjeta(string numero, string cuenta)
        {
            Numero = numero;
            Cuenta = cuenta;
        }
    }

    public interface IEvent { }

    public class TarjetaAgregada : IEvent
    {
        public Tarjeta Tarjeta { get; set; }
        public TarjetaAgregada(Tarjeta tarjeta)
        {
            Tarjeta = tarjeta;
        }

        public override string ToString()
        {
            return $"{ Tarjeta.Cuenta} { Tarjeta.Numero}";
        }
    }

    public class ComandoAgregarTarjeta
    {
        public string CustomerId { get; }
        public Tarjeta Tarjeta { get; }
        public ComandoAgregarTarjeta(Tarjeta tarjeta, string customerId)
        {
            Tarjeta = tarjeta;
            CustomerId = customerId;
        }
    }

}
