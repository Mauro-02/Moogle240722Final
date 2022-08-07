
namespace MoogleEngine;

///<summary> 
///Clase Query:
///Atributos querydictionary y cercanas.
///En el Cosntructor, se procesa cada token de la query.
///Se publica metodo GetTFQuery.
///</summary>
public class Query 

{
private enum token {Importancia,Cercania,Obligacion, Omitir, ImpObli, Palabra, Ninguno};


///<summary> 
///Este Diccionario, es usado para almacenar informacion de las palabras de la Query.
///Las Palabras de la Query son Key y los valores, un arrgelo de 4 elementos, usados de la sgte manera:
///[0]: Cantidad de veces de la palabra en la query
///[1]: Operador Importancia, =0 no existen *, !=0 cantidad de *
///[2]: Operador Obligacion, =0 no existe ^, !=0 existe ^
///[3]: Operador Omitir, =0 no existe !, !=0 existe !
///</summary>
 private Dictionary<string, int[]> querydictionary = new Dictionary<string, int[]>();

///<summary> 
///Este Diccionario, es usado para enlazar una palabra con las cercanas a ellas segun el operador de Cercania.
///Key: Palabra de la Izquierda del operador de Cercania.
///Valores: Lista de Cadenas que deben estar cerca de la Key.
///</summary>
 private Dictionary<string, List<string>> cercanas = new Dictionary<string,List<string>>();  

 private int error = 0;

 private int pos = 0;

 private int wordquerycount = 0;      // cantidad de palabras en la Query

///<summary> 
///Constructor de la Clase Query.
///Parametro de Entrada: La Query (string).
///Hace un barrido de los elementos de la Query y los clasifica en Tokens, segun la clasificacion definida por el enum token.
///llamando al Metodo FillQueryDictionary, llena el Diccionario de la Query.
///llamando al Metodo NextToken, va sacando los Tokens de la Query.
///Hace algunos chequeos en la Query, en caso de error se aborta y se muestra que no Hay Coincidencias.
///</summary>
public Query(string query)
{
   int contimp = 0;        // Contador de Asteriscos
   string beforeword="";
   string currentword="";
   string qaux = query;
   Boolean cercaniapendiente = false;
   Boolean wordalreadyprocess = false;
   token currenttoken = token.Ninguno;
   token beforetoken = token.Ninguno;


   // Convierte en minusculas las palabras de la Query y sustituye las signos de puntuacion por espacios y letras con acentos se cambias a sin acentos.
   qaux = qaux.ToLower();
   qaux = qaux.Replace('\n', ' ').Replace('_', ' ').Replace(',', ' ').Replace('.', ' ').Replace(';', ' ').Replace('-', ' ').Replace('/', ' ').Replace('\\', ' ')
                .Replace('á', 'a')
                .Replace('é', 'e')
                .Replace('í', 'i')
                .Replace('ó', 'o')
                .Replace('ú', 'u');
   qaux = qaux.Trim(' ');
  
   while (pos < qaux.Length)             // Recorre la Query, Pidiendo y analizando los Tokens
   {
      currenttoken = NextToken (qaux, out currentword);
      switch (currenttoken)
      {
        case token.Importancia:     // encontrado un *
         if ((beforetoken == token.Palabra) || (beforetoken == token.Ninguno) || (beforetoken == token.Importancia) || ((beforetoken == token.Cercania) && (beforeword != "")))  
         {
            contimp++;
            beforetoken = token.Importancia;
            pos++;
         }
         else
         {
           error = 1;     // Error en Operadores: Delante de un * no puede haber otro operador que no sea *, o palabra o cercania o nada
           return;
         }
         break;

        case token.Cercania:                        // se encontro ~ 
          if (beforetoken != token.Palabra)         
          {
              error = 2;                            // se encontro ~ sin la primera palabra de cercania
              return;
          }
          else                                   // Delante de ~ habia una palabra. Cercania pendiente de procesar.
          {
             beforetoken = token.Cercania;
             pos++;
             cercaniapendiente = true;
          }
         break;

        case token.Obligacion:                   // Se encontro ^
         if ((beforetoken == token.Palabra) || (beforetoken == token.Ninguno) || (beforetoken == token.Importancia) || ((beforetoken == token.Cercania) && (beforeword != "")))
         {
            beforetoken = token.Obligacion;
            pos++;
         }
         else
         {
           error = 3;     // Error en Operadores: Delante de un ^ no puede haber otro operador que no sea *, se permite *^, Cercania 
          return;
         }
         break;     

        case token.Omitir:                      // Se encontro !
         if ((beforetoken == token.Palabra) || (beforetoken == token.Ninguno))  
         {
            beforetoken = token.Omitir;
            pos++;
         }
         else
         {
           error = 4;     // Error en Operadores: Delante de un ! no puede haber otro operador
          return;
         }
         break;

        case token.Palabra:          //se encontro una Palabra
         this.wordquerycount++;
         wordalreadyprocess = false;
         if (beforetoken == token.Cercania)   // anterior a la palabra un ~ siguiendo a beforeword
           {
            FillQueryDictionary(currentword,beforeword,token.Cercania,0,wordalreadyprocess);   // Se procesa operador de cercania para las los palabras. wordalreadyprocess = false
            cercaniapendiente = false;  // Cercania ya procesada
           }
         else
         if (beforetoken == token.Importancia)  // Operador * delante de la palabra
          {
           if (cercaniapendiente)               // Cercania pendiente de procesar. se permite palabra1 ~ *palabra2
           {
             FillQueryDictionary(currentword,beforeword,token.Cercania,0,wordalreadyprocess);      // Se procesa operador de cercania para las los palabras. wordalreadyprocess = false
             cercaniapendiente = false; // Cercania ya procesada
             wordalreadyprocess = true; // currentword ya procesada
           }
           FillQueryDictionary(currentword,"",token.Importancia,contimp, wordalreadyprocess);  // Se procesa Operador * para currentword. wordalreadyprocess= true
           contimp=0;
          }
         else 
         if (beforetoken == token.Omitir)  // Operador ! delante de la palabra
           FillQueryDictionary(currentword,"",token.Omitir,0,wordalreadyprocess);    // Se procesa Operador ! para currentword.  wordalreadyprocess = false
         else  
         if (beforetoken == token.Obligacion)  // Operador ^ delante de la palabra
          {
           if (cercaniapendiente)               // se permite palabra1 ~ ^palabra2
           {
             FillQueryDictionary(currentword,beforeword,token.Cercania,0,wordalreadyprocess);   // Se procesa operador de cercania para las los palabras. wordalreadyprocess = false
             cercaniapendiente = false;
             wordalreadyprocess= true;
           } 
           if (contimp != 0)    // La palabra tenia * y ^
            {
              FillQueryDictionary(currentword,"",token.ImpObli,contimp,wordalreadyprocess);   // Se procesan los operadores * y ^ para currentword. 
              contimp=0;
            }
            else FillQueryDictionary(currentword,"",token.Obligacion,0,wordalreadyprocess);   // Se procesa el operador ^ para currentword. 
          }
         else 
          if ((beforetoken == token.Ninguno) || (beforetoken == token.Palabra)) 
           FillQueryDictionary(currentword,"",token.Palabra,0,wordalreadyprocess);          // La palabra no tenia Operadores, se procesa como token
         beforetoken = token.Palabra;
         beforeword = currentword;     // guardo la ultima palabra encontrada
         break;

        case token.Ninguno:
         
         break;
      }
   }

   if (currenttoken != token.Palabra)
   {
       error = 5;              // Fin de Query inesperado
       return;
   }
 
}


///<summary> 
///Devuelve el proximo Token en la Query, segun la clasificacion del enum token.
//Parametro de Entrada: La Query (string).
///Parametro de Salida: Una Palabra de la Query (string), si el tipo de Token es Palabra. 
///</summary>
private token NextToken(string query, out string word )
 {
   word="";
   while (pos < query.Length)
   {
    if (query[pos] == '*')
     {
      return token.Importancia;
     }
    else 
    if (query[pos] == '^')
     {
      return token.Obligacion;
     }
    else 
    if (query[pos] == '!')
     {
      return token.Omitir;
     }
    else 
    if (query[pos] == '~')
     {
      return token.Cercania;
     }
    else
    if (query[pos] == ' ')
     {
      pos++;
     }
    else
     if (Char.IsLetterOrDigit(query[pos]))   // Inicio de Palabra
     {
       while ((pos < query.Length) && (Char.IsLetterOrDigit(query[pos])))
       {
         word +=query[pos];
         pos++;
       }
       return token.Palabra;
     }
     else pos++;
   }
  
  return token.Ninguno;
 }

///<summary> 
///Llena del Diccionario de la Query.
///Parametro de Entrada Uno: Palabra Actual de la Query (string)
///Parametro de Entrada Dos: Palabra Anterior de la Query (string). Se usa para el Operador de Cercania. Para otros operadores se pasa en Blanco.
///Paramero de Entrada tres: Tipo de Token.
///Parametro de Entrada cuatro: Cantidad de * delante de la Palabra. Se pasa cero para el resto de los Token.
///Parametro de entrada cinco: Valor Booleano, y define si la Palabra ya habia sido procesada.
///</summary>
 private void FillQueryDictionary (string word1, string word2, token tk, int contimp, Boolean wap)
 {
    
    if (querydictionary.ContainsKey(word1))                   //Existe la Palabra
      {
        if (!wap) querydictionary[word1][0]++;                // La palabra ya existe en el Dicc., pero en otra Posici{on dentro de la Query, No se habia procesado antes. Se cuenta nuevamente.
      }
    else
      {
        querydictionary.Add(word1, new int[4]{1,0,0,0});      // agrego la palabra al Diccionario, contador de veces en 1. El resto en 0
      }
    switch (tk)
      {
        case token.Importancia:       // *
          querydictionary[word1][1]= contimp;  // cantidad de *
        break;

        case token.Obligacion:       // ^
           querydictionary[word1][2]= 1;  
        break;

        case token.Omitir:           // !
           querydictionary[word1][3]= 1;  
        break;

        case token.ImpObli:           // * y ^
           querydictionary[word1][1]= contimp;  
           querydictionary[word1][2]= 1; 
        break;

        case token.Cercania:           
         if (cercanas.ContainsKey(word1))                      // word1 ya esta en el Diccionario como Key
          {
           if (cercanas[word1].IndexOf(word2) == -1)          // word2 no estaba en la lista se agrega 
            cercanas[word1].Add(word2);                      // Agrego word2 a la lista de enlaces de word1
          }          
         else cercanas.Add(word1,new List<string>{word2});    // Si la palabra no existe en el Dic, se agrega como Key y se agrega word2 a la lista    
        break;
      }      
 }

///<summary> 
///Retorna  el TF (float) de las Palabras de la Query con Relación a las Palabras del Vocabulario.
///Parametro de Entrada: Palabras del Corpus (Extraidas del Vocabulario) (string).
///Si alguna palabra de la Query, está en el Corpus y tiene el Operador *, Su TF se multiplica por la cantidad de * y por 1.5.
///</summary>
public float GetTFQuery(string word)  
  {
   float auxtf = 0f;
   if (querydictionary.ContainsKey(word))      //la palabra del Vocabulario (pasada como parametro) esta en el Diccionario de La Query
      {
        auxtf = (float)querydictionary[word][0]/(float)wordquerycount; // en [0], esta la cantidad de veces q aparece la palabra en la Query
        if (querydictionary[word][1] != 0)  
          auxtf =  (float) auxtf * (float) querydictionary[word][1] * 1.5f;   // si pos [1] !=0 esa palabra tenia *, y el valor es la cantidad de *
      }  
     
   return auxtf;                                       //en otro caso devuelvo 0
   
  }


///<summary> 
///Propiedades para Devolver los valores de querydictionary, error, wordquerycount y cernanas.
///</summary>
public Dictionary<string, int[]> Querydictionary          
  {
    get
      {
          return querydictionary ;
      }
    set
      {
          querydictionary = value;
      } 
  }

public int Error           
  {
    get
      {
          return error;
      }
  }

public int Wordquerycount          
  {
    get
      {
          return wordquerycount;
      }
  }

public Dictionary<string, List<string>> Cercanas          
  {     
    get
      {
          return cercanas;
      }
  }

}