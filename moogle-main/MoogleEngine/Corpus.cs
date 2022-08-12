using System.Text;

namespace MoogleEngine;

///<summary>
///Clase Corpus:
///Clase Principal del Proyecto. Manipula los Atributos para los Documentos y Query
///En el Cosntructor, se leen los nombres de los ficheros, segun la extension pasada como parametro y se crea el objeto docs (arreglo de clase Document)
///Publica los metodos para procesamiento de Documentos, Query, Llenado del Vocabulario del Corpus, de la Matriz tfidf
///</summary>
public class Corpus
{
    private Document[] docs; // Arreglo de la Clase Document
    private string[] files; // Arreglo con los nombres y camino de cada Fichero
    private float[,] tfidfmatrix; // Matriz tfidf

    private float[] tfidfqueryvector; // Vector tfidf de la query. Tendra tantos elementos como palabras en el Vocabulario del Corpus.

    private int error = 0;

    private Query query; // para crear un Objeto Query

    private Synonymous syn; // para crear un Objeto Synonymous

    private string inputquery;

    private string suggestions;

    private Dictionary<string, int> vocabulary = new Dictionary<string, int>(); //Diccionario con tdas las palabras del campus (Key), el valor es la cantidad de documentos en los que se encuentra la palabra.

    ///<summary>
    ///Constructor de la Clase Corpus.
    ///Parametros de entrada: Camino donde se encuentran los Ficheros (string), Extension de los Ficheros (string), query de entrada (string)
    ///Despues de verificar que el camino es válido y que existen ficheros en ese camino, crea el objeto docs (arreglo de clase Document).
    ///Si el camino no existe o no existen ficheros en ese camino, inicializa la variable error en y o 2.
    ///</summary>
    public Corpus(string path, string fileext) //, string inputquery)
    {
        if (Directory.Exists(path)) // Verifica si el directorio existe
        {
            files = Directory.GetFiles(path, fileext);
            if (files.Length == 0)
                error = 2; // Directorio vacio
            else
            {
                docs = new Document[files.Length]; // Arreglo de Clase Documento. Cada elemento tiene atributos y funcionsalidsdes para cada documento.
                // this.inputquery = inputquery;
            }
        }
        else
            error = 1; // Directorio no existe
        ProcessDocs();
        FillTFIDFMatrix();
    }

    ///<summary>
    ///Metodo Publico, que procesa cada Documento del Corpus, extrayendo sus Atributos.
    ///</summary>
    public void Search(string query)
    {
        this.inputquery = query;
        ProcessQuery(true);
    }

    public void ProcessDocs()
    {
        for (int i = 0; i < files.Length; i++)
        {
            docs[i] = new Document(files[i], vocabulary); // Procesa todos los Documentos, instanciando la clase Document para cada fichero
        }
    }

    public void ProcessQuery(Boolean originalquery)
    {
        if (originalquery)
        {
            query = new Query(this.inputquery); // Procesa Query Original
            if (query.Error != 0) // se encontró un error en el procesamienro de la query
            {
                error = query.Error;
                return;
            }
        }
        else
        {
            query = new Query(this.suggestions);
            // query.Querydictionary.Clear();    // Limpio Diccionario de palabras de la query original
            //Comun.FillDictionary(this.query.Querydictionary, this.suggestions); // se llena el Dicionario con las palabras de Sugerencias
        }

        tfidfqueryvector = new float[vocabulary.Count]; // crea el objeto tfidfqueryvector
        FillVectorTFIDFQuery(); //  Calcula el vector tfidf de la query
    }

    /*
    ///<summary>
    ///Llena el Diccionario Vocabulary, con todas las palabras del Corpus.
    ///Se recorre el Diccionario de palabras de cada Documento y se va llenando el Vocavulario del Corpus.
    ///</summary>
        public void FillVocabulary()
        {
            for (int i = 0; i < docs.Length; i++)   // Se recorren todos los Documentos
            {
                foreach (var item in docs[i].Docsdictionary.Keys)   // Para cada palabra del Documento i
                {
                    if (vocabulary.ContainsKey(item))     //si la palabra ya esta en Vocabulario y no se ha contado incremento en 1 la cant de Doc que la contienen.
                    {
                        vocabulary[item]++;
                    }
                    else
                    {
                        vocabulary.Add(item, 1);                   // agrego la palabra al vocabulario e inicializo el contador en 1
                    }
                }
            }
        }*/

    ///<summary>
    ///Calcula de la matriz tfidf. Matriz Bidimensional, las filas representan los documentos y las columnas las palabras del Vocabulario.
    ///Cada elemento de la Matriz [doc,pal], representa la importancia de la palabra en el Documento.
    ///tfidf(pal, doc) = tf(pal, doc) * idf(pal, Corpus)
    ///tf(pal, doc) = no. de ocurrencias de la pal en doc / total palabras en doc. (Frecuencia de la Palabra en el Documento)
    ///idf(pal, corpus) = log base10(cant. doccumentos en el corpus / cantidad de documentos que contienen la palabra)(Frecuencia Inversa del Documento)
    ///</summary>
    public void FillTFIDFMatrix()
    {
        tfidfmatrix = new float[docs.Length, vocabulary.Count]; // filas=Docs, Col= Palabras. Dimension = cant de doc x cant de palabras del Vocabulario

        for (int i = 0; i < docs.Length; i++)
        {
            int index = 0;
            foreach (var aux in vocabulary.Keys) // calcula TF-IDF de cada palabra en el vocabulario
            {
                tfidfmatrix[i, index++] =
                    GetTF(docs[i], aux)
                    * (float)Math.Log10((float)docs.Length / (float)vocabulary[aux]);
            }
        }
    }

    ///<summary>
    ///Calcula La frecuencia de la Palabra en el Documento (TF(pal, doc).
    ///Parametros: Dicionario del Documento y la Palabra.
    ///tf(pal, doc) = no. de ocurrencias de la pal en doc / total palabras en doc.
    ///Si la Palabra no está en el Documento, entonces tf=0f
    ///</summary>
    private float GetTF(Document d, string word) //devuelve TF de una Palabra dentro de un Documento
    {
        if (d.Docsdictionary.ContainsKey(word)) //si lo tiene
        {
            return (float)d.Docsdictionary[word].Count / (float)d.Wordcount; //develvo su TF = Cant. de veces que existe la palabra  en el Doc / total de palabras del documento
        }
        return 0f; //en otro caso devuelvo 0
    }

    ///<summary>
    ///Para el proceso de Similaridad es necesario vectorizar la Query.
    ///Solo tendran incidencias en el Vector (valor diferente de cero), aquellos terminos presentes tanto en la Query como en algun Documento.
    ///Cada elemento del vector con valor diferente de cero, significa coincidencia entre la palabra de la query y alguna palabra del Corpus.
    ///</summary>
    private void FillVectorTFIDFQuery()
    {
        int i = 0;

        foreach (var aux in vocabulary.Keys) // calcula TF-IDF de la query
        {
            tfidfqueryvector[i++] =
                query.GetTFQuery(aux)
                * (float)Math.Log10((float)docs.Length / (float)vocabulary[aux]);
        }
    }

    ///<summary>
    ///Averigua si hay alguna palabra de la Query con el Signo !.
    ///Si Existe alguna Palabra con !, la busca en el Diccionario del Documento, si existe, el Doc no clasifica y retorna False.
    ///Si Todas las palabras con !, no estan en el Documento. El Documento Clasifica y Retorna True.
    ///Tambien Retorna True, si ninguna palabra de la Query tiene el Operador !.
    ///</summary>
    private Boolean ThereIsNotSkipWordInDocument(int docindex)
    {
        foreach (var item in query.Querydictionary.Keys) // Recorre el Dicc de Query buscando las palabras con !
        {
            if (query.Querydictionary[item][3] != 0) // Si la Pos. 3 es distinta de cero esa palagra hay q omitirla
            {
                if (docs[docindex].Docsdictionary.ContainsKey(item)) // La palabra con !, esta en el Documento. El Documento no clasifica.
                    return false;
            }
        }

        return true;
    }

    ///<summary>
    ///Averigua si hay alguna palabra de la Query con el Signo ^.
    ///Si Existe alguna Palabra con ^, la busca en el Diccionario del Documento, si no existe, el Doc no clasifica y retorna False.
    ///Si Todas las palabras con ^, estan en el Documento. El Documento Clasifica y Retorna True.
    ///Tambien Retorna True, si ninguna palabra de la Query tiene el Operador ^.
    ///</summary>
    private Boolean ThereIsForceWordInDocument(int docindex)
    {
        foreach (var item in query.Querydictionary.Keys) // Recorre el Dicc de Query buscando las palabras con ^
        {
            if (query.Querydictionary[item][2] != 0) // Si la Pos. 2 es distinta de cero esa palagra tiene q estar en el Documento
            {
                if (!(docs[docindex].Docsdictionary.ContainsKey(item))) // La palabra con ^, no esta en el Documento
                    return false;
            }
        }

        return true;
    }

    ///<summary>
    ///Determina la distancia minima entre las Palabras de la Query con el Operador de Cercania (~) en el Documento, pasado como Parametro.
    ///Todos los Documentos que llegan aqui, ya han clasificado con score > 0 y superado las condiciones de los Operadores ! y ^
    ///Los Conceptos Score y Cercania, son inversamente proporcionales. Si Cernania es pequeña el Score Aumenta.
    ///Para esto se calcula la cercania minima entre todas las parejas de palabras que existan en los Documentos.
    ///Si alguna Palabra  no aparece en el Documento, se continua con el analisis de las otras parejas.
    ///Si no hay palabras afectadas con el Operador de Cercania, Como Distancia Retorna 1 , con lo que no se afecta el Score del Documento.
    ///Si hay varias parejas de palabras, parece justo devolver la distancia minima obtenida, asi se premia al documento.
    ///Si hay mas parejas de palabras que no existen en el Documento, se retorna como distancia la cantidad de palabras del Documento.
    ///</summary>
    private int Cercania(int docindex)
    {
        string word2;
        int near = docs[docindex].Wordcount;
        int nearaux;
        int cnearsindoc = 0; // Cuenta las parejas de cercanas en el Documento
        int cnearsnotindoc = 0; // Cuenta las Parejas de Cercanas que no estan en el Documento
        List<string> wordlist = new List<string>();

        foreach (string word1 in query.Cercanas.Keys) // Por cada palabra key en el Diccionario Cercanas
        {
            wordlist = query.Cercanas[word1]; // en worlist la lista de palabras cercanas a word1
            for (int i = 0; i < wordlist.Count; i++) // Recorre la lista de palabras cercanas a word1
            {
                word2 = wordlist.ElementAt(i); // En word2 la palabra cercana a word1
                if (
                    docs[docindex].Docsdictionary.ContainsKey(word1)
                    && docs[docindex].Docsdictionary.ContainsKey(word2)
                ) // Las dos palabras con ~, estan en el Documento.
                {
                    nearaux = DistanciaMinima(word1, word2, docindex);
                    if (nearaux == 0)
                        cnearsnotindoc++; // las palabras cercanas son iguales y aparece una sola vez
                    else if (nearaux < near) // La distancia encontrada es menor que la que existia
                    {
                        near = nearaux; // en near queda la distancia minima entre todos los pares de palabras que estan en el Documento
                        cnearsindoc++;
                    }
                }
                else
                    cnearsnotindoc++; // Al menos una de las palabras no esta en el Documento, Se cuenta y se continua
            }
        }

        if (cnearsindoc > cnearsnotindoc)
            return near; // El Documento contiene mayoria de las palabras enlazadas y se retorna la minima distancia entre ellas
        else
            return docs[docindex].Wordcount; // El Documento contiene la minoria de las palabras, sera penalizado su score.
    }

    ///<summary>
    ///Calcula la distancia minima entre dos palabras dentro de un Documento.
    ///Parametros: Palabra uno (string), Palabra dos (string), Indice del Documento.
    ///</summary>
    private int DistanciaMinima(string w1, string w2, int docindex)
    {
        List<int> posw1 = new List<int>();
        List<int> posw2 = new List<int>();

        int dm = docs[docindex].Wordcount; // Como dm se asigna inicialmente la cant de palabras del Documento
        int dmaux;

        /*posw1 = new List<int>();
        posw2 = new List<int>();*/

        posw1 = docs[docindex].Docsdictionary[w1]; // Lista de posiciones en el Documento de la palabra w1
        posw2 = docs[docindex].Docsdictionary[w2]; // Lista de posiciones en el Documento de la palabra w2

        if (w1 == w2) // La query contiene enlace entre palabras iguales ej: Algoritmo ~ Algoritmo
        {
            if (posw1.Count == 1) //  La palabra existe solo una vez en el documento.
            {
                return 0;
            }
            else
            {
                // buscar la distancia minima entre palabras iguales
                for (int i = 0; i < posw1.Count - 1; i++)
                {
                    dmaux = posw1.ElementAt(i + 1) - posw1.ElementAt(i);
                    if (dmaux < dm)
                        dm = dmaux;
                }
            }
        }
        else
            foreach (int i in posw1) // recorre las posiciones de w1
            {
                foreach (int j in posw2) // recorre las posiciones de w2
                {
                    dmaux = (int)Math.Abs(i - j);
                    if (dmaux < dm)
                        dm = dmaux;
                }
            }

        return dm;
    }

    ///<summary>
    ///Para cada Documento, calcula su Score y si cumple las condiciones de eligibilidad, se agrega a la lista SearchItem.
    ///</summary>

    public List<SearchItem> ProcessScore()
    {
        List<SearchItem> sitem = new List<SearchItem>();
        float score = 0f;

        for (int i = 0; i < docs.Length; i++)
        {
            score = GetScore(i); // Calcula Score de Cada Documento. Pasa como parametro el No. del Documento

            if ((score > 0) && ThereIsNotSkipWordInDocument(i) && ThereIsForceWordInDocument(i)) // El Documento Clasifica para ser mostrado, tiene score > 0, no contiene palabras a Omitir y contine las Obligadas.
            {
                if (query.Cercanas.Count != 0)
                    score = score / (float)Cercania(i); // en la Query existe el Operador ~, e influira en el Score del Documento

                docs[i].Score = score;
                Docs[i].Filesnippet = Snippet(Docs[i].Text, query.Palabras);
                sitem.Add(new SearchItem(Docs[i].Filename, Docs[i].Filesnippet, docs[i].Score));
            }
        }

        return sitem;
    }

    private string stringBetween(string Source, string Start, string End)
    {
        string result = "";
        if (Start == End && End == " ")
        {
            result = Source.Length >= 500 ? Source.Substring(0, 500) : Source;
            return result;
        }
        // if (End=="")
        // {
        //     int StartIndex = Source.IndexOf(Start, 0) + Start.Length;
        //     result=Start + Source.Substring(StartIndex);
        //     if(result.Length>=500)
        //     {
        //         result=result.Substring(0,500);
        //     }
        //     return result;
        // }
        // else
        // {
        if (Source.Contains(Start) && Source.Contains(End))
        {
            result = Start;
            int StartIndex = Source.IndexOf(Start, 0) + Start.Length;
            int EndIndex = Source.IndexOf(End, StartIndex);
            if (EndIndex == -1)
            {
                result += Source.Substring(StartIndex);
                if (result.Length >= 500)
                {
                    result = result.Substring(0, 500);
                }
                return result + End;
            }
            else
            {
                result += Source.Substring(StartIndex, EndIndex - StartIndex);
                if (result.Length >= 500)
                {
                    result = result.Substring(0, 500);
                }
                return result + End;
            }
        }
        string result2 = Source.Length >= 500 ? Source.Substring(0, 500) : Source;
        return result2;
        //}
    }

    // private string Highlight(string text, List<string> words)
    // {
    //     StringBuilder result=new StringBuilder(text);
    //     foreach (string word in words)
    //     {
    //         result.Replace(word, "<b>" + word + "</b>");
    //     }
    //     return result.ToString();
    // }
    private string Snippet(string text, List<string> words)
    {
        int count = words.Count;
        if (count != 1)
        {
            string star = " ";
            string end = " ";
            for (int i = 0; i < count; i++)
            {
                if (text.Contains(words[i]))
                {
                    star = words[i];
                    break;
                }
            }
            for (int i = count - 1; i >= 0; i--)
            {
                if (text.Contains(words[i]))
                {
                    end = words[i];
                    break;
                }
            }
            //         if(star==end)
            //         {

            //             string sippet=stringBetween(text,star,"");
            //     return sippet;
            //         }
            // else {
            string snippet = stringBetween(text, star, end);
            return snippet;
        }
        // }

        else
        {
            string result = "";
            result = words[0];
            int StartIndex = text.IndexOf(words[0], 0) + words[0].Length;
            result += text.Substring(StartIndex);
            if (result.Length >= 500)
            {
                result = result.Substring(0, 500);
            }
            //  string filetext=text.Length >= 500 ? text.Substring(0, 500) : text;
            return result;
        }
        //     List<List<int>> pos = new List<List<int>>();
        // for (int i = 0; i < words.Count; i++)
        // {
        //     pos.Add(new List<int>(docs[i].Docsdictionary[words[i]] ));
        // }
        // int min=pos.Max(x=>x.Min());
        // int max=pos.Max(x=>x.Max());
        // string snippet = text.Substring(min, max);
        // return snippet;
    }

    ///<summary>
    ///Calcula el Score, del documento pasado como Parametro.
    ///Para ellos se basa en el Calculo del Coeficiente Similaridad del Coseno.
    ///Para el calculo se usan los valores de la matrix-tfidf y el queryvector-tfidf.
    ///http://ccdoc-tecnicasrecuperacioninformacion.blogspot.com
    ///</summary>


    public float GetScore(int docnumber)
    {
        float similaridadprodescalar = 0f; // Suma de los productos de cada elemento de la matriz-tfidf asociada al documento docnumber y los elementos del vector tfidfquery

        float matrizsquaresum = 0f; // Suma del cuadrado de cada elemento de la matriz-tfidf asociada al documento docnumber

        float vectorsquaresum = 0f; // suma del cuadrado de los elemntos del vector tfidf

        float similaridadcoseno = 0f; // similaridadprodescalar / ((raizcuadrada(matrizsquaresum)) * (raizcuadrada(vectorsquaresum)))

        for (int i = 0; i < vocabulary.Count; i++)
        {
            similaridadprodescalar += tfidfmatrix[docnumber, i] * tfidfqueryvector[i]; // Similaridad por el producto escalar del vector query y la Fila de la matriz-tfidf del documento docnumber

            matrizsquaresum += (float)Math.Pow(tfidfmatrix[docnumber, i], 2); // Suma del cuadrado de cada elemento de la matriz-tfidf asociada al documento docnumber
            vectorsquaresum += (float)Math.Pow(tfidfqueryvector[i], 2); // suma del cuadrado de los elemntos del vector tfidf
        }

        float deno = (float)((float)Math.Sqrt(matrizsquaresum) * (float)Math.Sqrt(vectorsquaresum));
        if (deno != 0f)
            similaridadcoseno = (float)similaridadprodescalar / deno;

        return similaridadcoseno;
    }

    ///<summary>
    ///Busca cada palabra de la Query en el Vocabulario, si está la agrega a Sugerencias
    ///si no, llama al metodo similitudLevenshtein entre la palabra de la query y la del vocabulario.
    /// Si la similitud es >= 0.45 y mayor que la mayor similitud encontrada, agrega a Suggestios la palabra del Vocabulario
    ///Si no se encuentra Sugerencia para una Palabra de la Query, se busca esa Palabra, en el Diccionario de Sinonimos
    ///Si esa Palabra tiene sinonimos y alguno de ellos, esta en el vocabulario, se agrega a sugerencias. Si ningun sinonimos esta en Vocabulario
    ///Se busca alguna similitud entre los sinonimos y las palabras del Vocabulario, si se encuentra se agrega a Sugerencias.
    ///</summary>
    public string FindSuggestion()
    {
        const float similaritypercent = 0.45f;
        float similarity = 0f;
        string suggestion = "";
        this.suggestions = "";
        string[] auxsyn;
        float auxsimi;

        foreach (var wordquery in query.Querydictionary.Keys) // para cada palabra de la query
        {
            if (vocabulary.ContainsKey(wordquery)) // pregunta si esa palabra de la query existe en el vocabulario
            {
                suggestion = wordquery; // Si existe, agrega esa palabra a la sugerencia.
            }
            else
                foreach (var wordvoc in vocabulary.Keys) // Palabra de la Query no esta en el Coprpus.
                {
                    auxsimi = LevenshteinSimilarity(wordvoc, wordquery); //se busca la similitud entre cada palabra de la query y el vocabulario. usando Levenshtein
                    if ((auxsimi >= similaritypercent) && (auxsimi > similarity)) // Si la similitud es mayor a 0.45 y mayor que la similitus que ya se tenia la palabra se incorpora a Sugerencia
                    {
                        similarity = auxsimi;
                        suggestion = wordvoc;
                    }
                }

            if (suggestion == "") // No hay similitud entre la Palabra de la Query y las Palabras del Corpus, pruebo con los Sinonimos
            {
                if (syn is null)
                    syn = new Synonymous(); // Si no se ha creado el objeto syn, se instancia la clase Synonymous
                if (syn.SynDic!.ContainsKey(wordquery)) // Existen Sinonimos para la palabra de la Query
                {
                    auxsyn = syn.SynDic[wordquery]; // en auxsyn el arreglo de sinonimos de wordquery
                    foreach (string synword in auxsyn) // Por cada Sinonimo, se averigua si existe en el Corpus
                        if (vocabulary.ContainsKey(synword)) // El sinonimo esta en algun Documento del Corpus, se agrega a Sugerencia.
                        {
                            suggestion = synword; // lo agrego a Suggestion y Salgo del foreach
                            break;
                        }
                    // este fragmento permite sacar palabras pareceidas a los sininimos, se pudiera quitar.
                    if (suggestion == "") // Ningun Sinonimo esta en los Documentos del Corpus. Se aplica Levenshtein Para buscar Similitud entre los Sinonimos y las palabras del Corpus
                    {
                        foreach (string synword in auxsyn)
                        {
                            foreach (var wordvoc in vocabulary.Keys)
                            {
                                auxsimi = LevenshteinSimilarity(wordvoc, synword); // Se busca Similitud entre cada sinonimo y las palabras del Corpus. El primer sinonimo que cumpla el % de similitud se agrega a sugerencia y se termina n los ciclos
                                if (auxsimi >= similaritypercent)
                                {
                                    suggestion = wordvoc; // lo agrego a Suggestion
                                    break; //Salgo del Ciclo wordvoc
                                }
                            }
                            if (suggestion != "")
                                break; //Salgo del Ciclo synword
                        }
                    }
                }
            }

            similarity = 0f;
            this.suggestions += suggestion;
            this.suggestions += " ";
            suggestion = "";
        }
        this.suggestions = this.suggestions.TrimEnd(' ');
        return this.suggestions;
    }

    ///<summary>
    ///calcula la distancia entre dos cadenas y devuelve la similitud entre ellas.
    ///Para ello normaliza la distancia, dividiendola entre la longitud de la cadena mayor (se obtiene un valor entre 0  y 1).
    ///valores de la distancia Normalizada cercanos a 0, corresponde a cadenas semejantes.
    ///finalmente la similitud entre dos cadenas, puede verse como el inverso de la distancia normalizada.
    ///Cuando la distancia Normalizada es pequeña la similitud es grande, por tanto se define que: sim(c1,c2) = 1 - DNorm(c1,c2)
    ///</summary>
    private float LevenshteinSimilarity(string wordvoc, string wordquery)
    {
        int dist = 0;
        int m = wordvoc.Length;
        int n = wordquery.Length;
        int[,] d = new int[m + 1, n + 1];
        if ((n == 0) || (m == 0))
            return 0f; // si una de las cadenas es vacia => Similitud 0

        for (int i = 0; i <= m; i++)
        {
            d[i, 0] = i;
        }
        for (int j = 0; j <= n; j++)
        {
            d[0, j] = j;
        }

        for (int i = 1; i <= m; i++)
        {
            for (int j = 1; j <= n; j++)
            {
                dist = (wordvoc[i - 1] == wordquery[j - 1]) ? 0 : 1;
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + dist
                );
            }
        }

        float distNorm = m > n ? ((float)d[m, n] / (float)m) : ((float)d[m, n] / (float)n); // Normaliza la Distancia
        return 1 - distNorm;
    }

    public Document[] Docs
    {
        get { return docs; }
    }

    public int Error
    {
        get { return error; }
        set { error = value; }
    }
}
