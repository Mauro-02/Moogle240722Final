using System.Diagnostics;

namespace MoogleEngine;

public static class Moogle
{
    static Corpus mycorpus = new Corpus("../Content", "*.txt");

    public static SearchResult Query(string query)
    {
        if (query.ToLower() == "valar morghulis")
        {
            return new SearchResult(
                new SearchItem[] { new SearchItem("Game of Thrones", "Valar Dohaeris", 1.0f) }
            );
        }
        else
        {
            if (mycorpus.Error == 0)
            {
                mycorpus.Search(query);
                if (mycorpus.Error != 0) // Error en la Query
                {
                    SearchItem[] items = new SearchItem[1]
                    {
                        new SearchItem("Ninguna Coincidencia", "", 0)
                    };
                    mycorpus.Error = 0;
                    return new SearchResult(items, "");
                }
                var sortlist = mycorpus.ProcessScore().OrderByDescending(x => x.Score);
                if (sortlist.Count() == 0) // No se encontraron resultados de la query original, se intenta buscar resultados de Sugerencias
                {
                    string saux = mycorpus.FindSuggestion();
                    //mycorpus.ProcessQuery(false);     // Procesa la query de sugerencias
                    //sortlist = mycorpus.ProcessScore().OrderByDescending(x => x.Score);

                    //if (sortlist.Count() == 0) // No se encontraron resultados de la Sugerencias.
                    // {
                    SearchItem[] items = new SearchItem[1]
                    {
                        new SearchItem("Ninguna coincidencia", "", 0)
                    };
                    return new SearchResult(items, saux);
                    // }
                    // else return new SearchResult(sortlist.ToArray(), saux);
                }
                else
                    return new SearchResult(sortlist.ToArray(), mycorpus.FindSuggestion());

                //return new SearchResult(sortlist.ToArray(), "");
            }
            else // Hubo error en la lectura de Ficheros
            {
                if (mycorpus.Error == 1)
                {
                    SearchItem[] items = new SearchItem[1]
                    {
                        new SearchItem("Directorio de Ficheros no Existe", "", 0)
                    };
                    return new SearchResult(items, "");
                }
                else
                {
                    SearchItem[] items = new SearchItem[1]
                    {
                        new SearchItem("No existen Ficheros en el Directorio", "", 0)
                    };
                    return new SearchResult(items, "");
                }
            }
        }
    }

    // Stopwatch watch = new Stopwatch();
    // watch.Start();
    // Stopwatch processdocswatch = new Stopwatch();
    // Stopwatch filldocswatch = new Stopwatch();
    // Stopwatch procesqwatch = new Stopwatch();



    //         if (mycorpus.Error == 0)29

    //         {
    //  //         processdocswatch.Start();
    //          mycorpus.ProcessDocs();
    //    //       processdocswatch.Stop();
    //       //    System.Console.WriteLine("ProcessDocs: " + processdocswatch.ElapsedMilliseconds/1000 + " segundos");
    //          //mycorpus.FillVocabulary();
    //      //    filldocswatch.Start();
    //          mycorpus.FillTFIDFMatrix();
    //        //   filldocswatch.Stop();
    //         //  System.Console.WriteLine("FillTFIDFMatrix: " + filldocswatch.ElapsedMilliseconds/1000 + " segundos");
    //          // procesqwatch.Start();
    //           mycorpus.ProcessQuery(true);
    //           //procesqwatch.Stop();
    //           //System.Console.WriteLine("ProcessQuery: " + procesqwatch.ElapsedMilliseconds/1000 + " segundos");
    //             //   watch.Stop();
    //             //    System.Console.WriteLine("Total: " + watch.ElapsedMilliseconds/1000 + " segundos");
    //          if (mycorpus.Error != 0)  // Error en la Query
    //          {
    //            SearchItem[] items = new SearchItem[1] {new SearchItem("Ninguna Coincidencia","",0)};
    //            return new SearchResult(items,"");
    //          }

    //   var sortlist = mycorpus.ProcessScore().OrderByDescending(x => x.Score);

    //return new SearchResult(sortList.ToArray(), vocabulario.GetSuggestion(query));
    //  if (sortlist.Count() == 0)   // No se encontraron resultados de la query original, se intenta buscar resultados de Sugerencias
    //   {
    //     string saux= mycorpus.FindSuggestion();
    //     //mycorpus.ProcessQuery(false);     // Procesa la query de sugerencias
    //    //sortlist = mycorpus.ProcessScore().OrderByDescending(x => x.Score);

    //     //if (sortlist.Count() == 0) // No se encontraron resultados de la Sugerencias.
    //     // {
    //        SearchItem[] items = new SearchItem[1] {new SearchItem("Ninguna coincidencia","",0)};
    //        return new SearchResult(items,saux);
    //     // }
    //    // else return new SearchResult(sortlist.ToArray(), saux);
    //   }
    //  else return new SearchResult(sortlist.ToArray(), mycorpus.FindSuggestion());

    //   //return new SearchResult(sortlist.ToArray(), "");
    // }
    // else            // Hubo error en la lectura de Ficheros
    // {
    //  if (mycorpus.Error == 1)
    //   {
    //    SearchItem[] items = new SearchItem[1] {new SearchItem("Directorio de Ficheros no Existe","",0)};
    //    return new SearchResult(items,"");
    //   }
    //  else
    //   {
    //    SearchItem[] items = new SearchItem[1] {new SearchItem("No existen Ficheros en el Directorio","",0)};
    //    return new SearchResult(items,"");
    //   }
    // }

    // }
}
