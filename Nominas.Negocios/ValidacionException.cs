namespace Nominas.Negocios;

public class ValidacionException : Exception
{
    public ValidacionException(string mensaje) : base(mensaje) { }
}