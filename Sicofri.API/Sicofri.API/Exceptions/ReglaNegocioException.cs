namespace Sicofri.API.Exceptions;

/// <summary>
/// Error esperado del negocio: datos invalidos, usuario repetido, falta de producto,
/// etc.
/// Lleva el mensaje en espanol y el codigo HTTP que debe recibir la app.
/// </summary>

public class ReglaNegocioException : Exception
{
    public int CodigoEstado { get; }

    public ReglaNegocioException(string mensaje, int codigoEstado = StatusCodes.Status400BadRequest)
        : base(mensaje)
    {
        CodigoEstado = codigoEstado;

    }//Fin de la reglaNegocioException
}//Fin de la clase
