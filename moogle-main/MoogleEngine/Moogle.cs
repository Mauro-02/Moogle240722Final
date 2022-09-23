using System.Diagnostics;

namespace MoogleEngine;

public static class Moogle
{
    
    static Corpus mycorpus = new Corpus("../Content", "*.txt");

    public static SearchResult Query(string query)
    {
        int TDocument=15;
        if (query.ToLower() == "after all this time")
        {
            return new SearchResult(
                new SearchItem[] { new SearchItem("Severus Snape", "Always", 1.0f,  "") }
            );
        }
        else
        {
            
            if (mycorpus.Error == 0)
            {
                mycorpus.Search(query,true);
                if (mycorpus.Error != 0) // Error en la Query
                {
                    SearchItem[] items = new SearchItem[1]
                    {
                        new SearchItem("Ninguna Coincidencia", "", 0, "")
                    };
                    mycorpus.Error = 0;
                    return new SearchResult(items, "");
                }
                var sortlist = mycorpus.ProcessScore().OrderByDescending(x => x.Score).Take(TDocument);
                string suggestion = mycorpus.FindSuggestion();
                if (sortlist.Count() <TDocument) // No se encontraron resultados de la query original, se intenta buscar resultados de Sugerencias
                {
                    int count = sortlist.Count();
                    string sug=mycorpus.FindSuggestionSynonyms();
                    mycorpus.Search(sug,false);
                    var sortlist2 = mycorpus.ProcessScore().OrderByDescending(x => x.Score).Take(TDocument-count);
                    var notmatch = sortlist2.Where(x => !sortlist.Any(y => y.Title == x.Title));
                    var result = sortlist.Concat(notmatch);
                    if(result.Count()==0)
                    {
                       SearchItem[] items = new SearchItem[1]
                    {
                        new SearchItem("Ninguna Coincidencia", "", 0,   "")
                    };
                    mycorpus.Error = 0;
                    return new SearchResult(items, suggestion);  
                    }
                    return new SearchResult(result.ToArray(), suggestion);
                    
                    // string saux = mycorpus.FindSuggestion();
                    // //mycorpus.ProcessQuery(false);     // Procesa la query de sugerencias
                    // //sortlist = mycorpus.ProcessScore().OrderByDescending(x => x.Score);

                    // //if (sortlist.Count() == 0) // No se encontraron resultados de la Sugerencias.
                    // // {
                    // SearchItem[] items = new SearchItem[1]
                    // {
                    //     new SearchItem("Ninguna coincidencia", "", 0)
                    // };
                    // return new SearchResult(items, saux);
                    // // }
                    // // else return new SearchResult(sortlist.ToArray(), saux);
                }
                else
                    return new SearchResult(sortlist.ToArray(), suggestion);

                //return new SearchResult(sortlist.ToArray(), "");
            }
            else // Hubo error en la lectura de Ficheros
            {
                if (mycorpus.Error == 1)
                {
                    SearchItem[] items = new SearchItem[1]
                    {
                        new SearchItem("Directorio de Ficheros no Existe", "", 0, "")
                    };
                    return new SearchResult(items, "");
                }
                else
                {
                    SearchItem[] items = new SearchItem[1]
                    {
                        new SearchItem("No existen Ficheros en el Directorio", "", 0, "")
                    };
                    return new SearchResult(items, "");
                }
            }
        }
    }

    
}
