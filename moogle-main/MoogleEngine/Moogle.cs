namespace MoogleEngine;


public static class Moogle
{
    public static SearchResult Query(string query) {
        
        Corpus mycorpus = new Corpus("../Content","*.txt",query);  
         
        if (mycorpus.Error == 0)
        {
         mycorpus.ProcessDocs();
         //mycorpus.FillVocabulary();
         mycorpus.FillTFIDFMatrix();
         mycorpus.ProcessQuery(true);

         if (mycorpus.Error != 0)  // Error en la Query
         {
           SearchItem[] items = new SearchItem[1] {new SearchItem("Ninguna Coincidencia","",0)};
           return new SearchResult(items,"");
         }

         var sortlist = mycorpus.ProcessScore().OrderByDescending(x => x.Score);
        
        //return new SearchResult(sortList.ToArray(), vocabulario.GetSuggestion(query));
         if (sortlist.Count() == 0)   // No se encontraron resultados de la query original, se intenta buscar resultados de Sugerencias
          {
            string saux= mycorpus.FindSuggestion();
            //mycorpus.ProcessQuery(false);     // Procesa la query de sugerencias
           //sortlist = mycorpus.ProcessScore().OrderByDescending(x => x.Score);

            //if (sortlist.Count() == 0) // No se encontraron resultados de la Sugerencias.
            // {
               SearchItem[] items = new SearchItem[1] {new SearchItem("Ninguna coincidencia","",0)};
               return new SearchResult(items,saux);
            // }
           // else return new SearchResult(sortlist.ToArray(), saux);
          }
         else return new SearchResult(sortlist.ToArray(), mycorpus.FindSuggestion());
          //return new SearchResult(sortlist.ToArray(), "");
        }                 
        else            // Hubo error en la lectura de Ficheros
        {
         if (mycorpus.Error == 1)
          {
           SearchItem[] items = new SearchItem[1] {new SearchItem("Directorio de Ficheros no Existe","",0)};
           return new SearchResult(items,"");
          }
         else
          {
           SearchItem[] items = new SearchItem[1] {new SearchItem("No existen Ficheros en el Directorio","",0)};
           return new SearchResult(items,"");
          }
        }
        
    }
}
