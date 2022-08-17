namespace MoogleEngine;

///<summary>
///Clase Document:
///Para cada documento del Corpus, existen los Atributos docsdictionary, filename, filepath, filesnippet y score.
///En el Cosntructor, se lee la informacion de cada fichero, se guardan los atributos y se llama a un metodo Privado que llena el Diccionario.
///</summary>
public class Document
{
    ///<summary>
    ///Este Diccionario, es usado para almacenar informacion de las palabras de los Documentos.
    ///Key: Cada Palabra
    ///Valores: Lista de Enteros, con las Posiciones de la palabra en el Documento.
    ///</summary>
    private Dictionary<string, List<int>> docsdictionary = new Dictionary<string, List<int>>();

    private string filename;
    private string filepath;
    private string filesnippet;

    private string text;

    private float score = 0f;

    private int wordcount; // Cantidad de palabras en el Documento

    private const int lengthsnipe = 100;

    ///<summary>
    ///Constructor de la Clase Document.
    ///Parametro de entrada: Camino y Nombre del Fichero a Procesar.
    ///Lee el contenido del Fichero en una String. llama al Metodo FillDictionarys para llenar el Diccionario docsdictionary, le da valores a las variables
    ///wordcount, filename, filesnippet y filepath.
    ///a la variable filename, se le agrega la fecha de Modificacio del fichero y se muestra en el resultado de la busqueda.
    ///</summary>
    public Document(string pathfilename, Dictionary<string, int> v)
    {
        DateTime dt = File.GetLastWriteTime(pathfilename);

        StreamReader read = new StreamReader(pathfilename);
        string linestext;
        try
        {
            linestext = read.ReadToEnd().ToLower(); //lee todo el documento y lo devuelve como un string, con todas las palabras en minusculas
            read.Close(); //cierro el stream despues de leer el archivo
        }
        catch (EndOfStreamException ex)
        {
            throw new Exception(ex.Message);
        }
        // this.text = " "+linestext.Replace('á', 'a')
        //     .Replace('é', 'e')
        //     .Replace('í', 'i')
        //     .Replace('ó', 'o')
        //     .Replace('ú', 'u')
        //     .Replace('\n', ' ')
        //     .Replace('\r', ' ')
        //     .Replace('\t', ' ')
        //     .Replace('\\', ' ')
        //     +" ";
        filename =
            pathfilename.Substring(pathfilename.LastIndexOf("/") + 1)
            + " ("
            + dt.ToString("dd/MM/yy")
            + ")"; //extrae substring a partir del ultimo / +1
        filesnippet = " "; //linestext.Length >= lengthsnipe ? linestext.Substring(0, 100) : linestext;                   //si el texto tiene mas de 100 terminos devuelve solo esta cantidad sino me devuelve los que tengan
        filepath = pathfilename;

        wordcount = FillDictionarys(docsdictionary, linestext, v); // llena Diccionario de Documentos y Vocabulario
    }

    ///<summary>
    ///Llena el Diccionario docsdictionary y Vocabulary
    ///Parametro de entrada: Diccionario de Doc a llenar, una string con todas las palabras del Documento y el Diccionario Vocabulario.
    ///Despues de Procesado el Ultimo Documento, se ha llenado el Vocabulario del Corpus con Todas las palabras (key) y como valor la cantidad de Doc que contienen la palabra.
    ///</summary>
    private int FillDictionarys(
        Dictionary<string, List<int>> dictionary,
        string textread,
        Dictionary<string, int> v
    ) //textread: Contenido leido desde los Ficheros
    {
        this.text = " "+textread
            .Replace('\n', ' ') // En words, las palabras separadas por espacio. Las palabras quedan sin acentos.
            .Replace('_', ' ') // DSignos de puntuacion son sustituidos por espacio.
            .Replace(',', ' ')
            .Replace('.', ' ')
            .Replace(';', ' ')
            .Replace('-', ' ')
            .Replace('/', ' ')
            .Replace(':', ' ')
            .Replace('\r', ' ')
            .Replace('\t', ' ')
            .Replace('\\', ' ')
            .Replace('á', 'a')
            .Replace('é', 'e')
            .Replace('í', 'i')
            .Replace('ó', 'o')
            .Replace('ú', 'u')+" ";
            var words=text.Trim().Split(" ", StringSplitOptions.RemoveEmptyEntries);
            //.Trim()
            //.Split(" ", StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < words.Length; i++) // Ciclo para Obtener cada palabra
        {
            if (dictionary.ContainsKey(words[i])) //comprobar si el diccionario de Documentos contiene la palabra, si la tiene agrego un nuevo elemento a la lista con la posicion de la palabra
            {
                dictionary[words[i]].Add(i);
            }
            else
            {
                dictionary.Add(words[i], new List<int> { i }); // Si la palabra no existe en el Dic de Doc., se agrega como Key y se agrega un elemento a la lista con la posicion

                if (v.ContainsKey(words[i])) //si la palabra ya esta en Vocabulario del Corpus, se incrementa en uno, para contar que existe en ese Documento
                {
                    v[words[i]]++;
                }
                else
                {
                    v.Add(words[i], 1); // agrego la palabra al vocabulario e inicializo el contador en 1
                }
            }
        }

        return words.Length;
    }

    ///<summary>
    ///Propiedades para Devolver los valores de docsdictionary, filename, filepath, filesnippet, score y wordcount
    ///</summary>

    public Dictionary<string, List<int>> Docsdictionary
    {
        get { return docsdictionary; }
    }

    public string Filename
    {
        get { return filename; }
    }

    public string Filepath
    {
        get { return filepath; }
    }
    public string Filesnippet
    {
        get { return filesnippet; }
        set { filesnippet = value; }
    }

    public string Text
    {
        get { return text; }
    }
    public float Score
    {
        get { return score; }
        set { score = value; }
    }

    public int Wordcount
    {
        get { return wordcount; }
    }
}
