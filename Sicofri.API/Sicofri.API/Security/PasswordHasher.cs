namespace Sicofri.API.Security;

/// <summary>
/// Encripta y verifica contraseñas con BCrypt.
/// La contraseña real nunca se guarda: solo su versión encriptada (hash).
/// </summary>

public class PasswordHasher
{
    /// <summary>
    /// Qué tan "costoso" es calcular el hash. Más alto = más seguro pero más lento.
    /// 11 tarda unos 150 milisegundos: imperceptible para el usuario, muy lento para un atacante.
    /// </summary>
    /// 

    private const int FactorTrabajo = 11;

    public string Encriptar(string contrasena)
    {
        return BCrypt.Net.BCrypt.HashPassword(contrasena, FactorTrabajo);
    }//Fin del metodo Encriptar

    public bool Verificar(string contrasena, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(contrasena, hash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            // El hash guardado está dañado o no es de BCrypt: se trata como contraseña incorrecta.
            return false;
        }
    }//Fin del metodo Verificar

}//Fin de la clase
