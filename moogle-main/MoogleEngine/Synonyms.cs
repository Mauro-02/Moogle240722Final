using System.Text.Json;

///<summary>
///Clase Synonymous:
///Atributo: Diccionario syndic
///En el Cosntructor, crea el objeto syndic y lo llena con la informacion del Fichero json de sinonimos
///Se publica metodo GetTFQuery.
///</summary>
public class Synonymous
{
    private static Dictionary<string, string[]>? syndic; // Diccionario Key: palabra a buscar (string), Valores: Arreglo con las palabras Sinonimos.

    ///<summary>
    ///Constructor de la Clase Synonymous.
    ///crea el syndic, que contendra cada palacra y sus sinonimos.
    ///Deserializa el Fichero Json de sinonimos y llena syndic
    ///</summary>
    public Synonymous()
    {
        syndic = new Dictionary<string, string[]>();
        try
        {
            syndic = JsonSerializer.Deserialize<Dictionary<string, string[]>>(
                File.ReadAllText("../DataFiles/Synonymous.json")
            );
        }
        catch (JsonException ex)
        {
            throw new Exception(ex.Message);
        }
    }

    public Dictionary<string, string[]>? SynDic
    {
        get { return syndic; }
    }
}
