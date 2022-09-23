# Proyecto Moogle! Mauro Eduardo Campver Barrios C-112

Moogle es un motor de busqueda desarrollado con el objetivo de emparejar una query con los documentos relacionados a esta de una forma u otra. Se implemento un modelo vectorial en el cual se representan los pesos(Tf-Idf) de las palabras presentes en cada documento, quedando un vector por documento que luego sera comparado a con el vector de la query a traves de la similitud del coseno. Este proceso arroja un score por documento, posteriormente se organiza de forma descendente, premiando a los documentos con más puntuacion.

---
## Clases
* ***Corpus*** 
  * ProcessQuery
  * FillTFIDFMatrix
  * Operadores:
     * ThereIsNotSkipWordInDocument(!)
     * ThereIsForceWordInDocument(^)
     * Cercania(~)
  * ProcessScore
  * Snippet
  * GetScore
  * LevenshteinSimilarity
   

* ***Document***
* ***Moogle***
* ***Query***
* ***SearchItem***
* ***SearchResult***
* ***Synonyms***

---

***[Corpus](https://github.com/Mauro-02/Moogle240722Final/blob/5dab749c342e9c04bdcbe0244181cdd3826cec7a/moogle-main/MoogleEngine/Corpus.cs)***

Aqui se desarrollan la gran mayoria de los procesos del proyecto, es la clase principal del Proyecto, manipula los atributos para los Documentos y Query. Cuando se ejecuta el constructor de la clase, esta comprueba la existencia de la carpeta Content, y la existencia de archivos .txt dentro de esta, realiza el procesado de los documentos a traves de la clase Document, y llena la matriz tfidf que contendra la relacion del peso de cada palabra en los documentos que la contienen

Dentro de esta clase hay que destacar varios metodos, se enumeraran a continuacion y se dara una breve explicacion de su funcion

* ProcessQuery
>Se encarga de la procesado de la query, es un metodo Boolean ya que varia su ejecucion si es la 1ra vez que se ejecuta(seria verdadero) o no(falso)
``` c#
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
        }

        tfidfqueryvector = new float[vocabulary.Count]; // crea el objeto tfidfqueryvector
        FillVectorTFIDFQuery(); //  Calcula el vector tfidf de la query
    }
```
* FillTFIDFMatrix()
>Se encarga del llenaddo de la matriz tfidf que relaciona los documentos con las palabras de este y su peso. El tf-idf forma parte del modelo vectoria. El Tf se calcula a traves del metodo *GetTF*, su explicacion breve seria la división del número de ocurrencias de la palabra en el documento / total palabras en el documeno, si la palabra no está en el Documento, entonces tf=0f. El Idf sería el log base10(cantidad de documentos en el corpus / cantidad de documentos que contienen la palabra)
```c#
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
```
* **Operadores**:

El proyecto cuenta con 4 operadores principales, en este clase se desarrolan 3 de ellos:  
* ThereIsNotSkipWordInDocument(!)
>   Averigua si hay alguna palabra de la Query con el Signo !, si existe alguna palabra con !, la busca en el diccionario del Documento, si existe, el Documento no clasifica y retorna False, si todas las palabras con !, no estan en el Documento, el Documento clasifica y retorna True, también retorna True, si ninguna palabra de la Query tiene el Operador !.
```c#
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

```

* ThereIsForceWordInDocument(^)
>    Averigua si hay alguna palabra de la Query con el Signo ^, si existe alguna palabra con ^, la busca en el Diccionario del Documento, si no existe, el Documento no clasifica y retorna False, si todas las palabras con ^, están en el Documento, el documento clasifica y retorna True, tambien Retorna True, si ninguna palabra de la Query tiene el Operador ^.

```c#
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
```

* Cercania(~)
>   Determina la distancia mínima entre las palabras de la Query con el Operador de Cercania (~) en el Documento, pasado como parametro. Todos los Documentos que llegan aqui, ya han clasificado con score > 0 y superado las condiciones de los Operadores ! y ^, los conceptos Score y Cercania, son inversamente proporcionales, si Cernania es pequeña el Score aumenta, para esto se calcula la cercania mínima entre todas las parejas de palabras que existan en los Documentos, si alguna palabra no aparece en el Documento, se continua con el análisis de las otras parejas, si no hay palabras afectadas con el Operador de Cercania, como Distancia retorna 1 , con lo que no se afecta el Score del Documento, si hay varias parejas de palabras, parece justo devolver la distancia minima obtenida, asi se premia al documento, si hay mas parejas de palabras que no existen en el Documento, se retorna como distancia la cantidad de palabras del Documento.

```c#
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

```

* ProcessScore
>Para cada Documento, calcula su Score y si cumple las condiciones de eligibilidad( *ThereIsNotSkipWordInDocument* y *ThereIsForceWordInDocument* ) y se agrega a la lista SearchItem junto al titulo del documento y el Snippet

```c#
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
```
    
* Snippet
>En este punto el documento a analizar tiene una cierta relacion con la Query, por lo que al menos una palabra de esta está en el documento, se recorre la Query comprobando una por una la existencia de esta en el documento, luego se extrae un *Substring* del documento a partir de la posicion de la 1ra palabra encontrada. Si este *string* tiene más de 500 caracteres se extrae un *Substring* de este desde el inicio hasta 500 caracteres a la derecha, sino se devuelve completo
```c#
    private string Snippet(string text, List<string> words)
    {
        string result = "";
        string word = " ";
        for (int i = 0; i < words.Count; i++)
        {
            if (text.Contains(" " + words[i] + " "))
            {
                word = " " + words[i] + " ";
                break;
            }
        }
        result = text.Substring(text.IndexOf(word));
        result = result.Length >= 500 ? result.Substring(0, 500) : result.Substring(0, result.Length);
        return result;
    }
```


***[Document](https://github.com/Mauro-02/Moogle240722Final/blob/9ef82afdbf9aa1ae5992096a7156b3a5f2aa78ec/moogle-main/MoogleEngine/Document.cs)***

***[Moogle](https://github.com/Mauro-02/Moogle240722Final/blob/18014439a5de6e7cad86da7ae843756e34d4332c/moogle-main/MoogleEngine/Moogle.cs)***

***[Query](https://github.com/Mauro-02/Moogle240722Final/blob/f8492faf8c9cfdfef421e193091fd34b61a0e3ea/moogle-main/MoogleEngine/Query.cs)***

***[SearchItem](https://github.com/Mauro-02/Moogle240722Final/blob/0fbfc711ab947b2284521e912c8b5778c9385b7a/moogle-main/MoogleEngine/SearchItem.cs)***

***[SearchResult](https://github.com/Mauro-02/Moogle240722Final/blob/f8492faf8c9cfdfef421e193091fd34b61a0e3ea/moogle-main/MoogleEngine/SearchResult.cs)***

***[Synonyms](https://github.com/Mauro-02/Moogle240722Final/blob/f8492faf8c9cfdfef421e193091fd34b61a0e3ea/moogle-main/MoogleEngine/Synonyms.cs)***
