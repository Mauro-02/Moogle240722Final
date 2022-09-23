# Proyecto Moogle! Mauro Eduardo Campver Barrios C-112

Moogle es un motor de busqueda desarrollado con el objetivo de emparejar una query con los documentos relacionados a esta de una forma u otra. Se implemento un modelo vectorial en el cual se representan los pesos(Tf-Idf) de las palabras presentes en cada documento, quedando un vector por documento que luego sera comparado a con el vector de la query a traves de la similitud del coseno. Este proceso arroja un score por documento, posteriormente se organiza de forma descendente, premiando a los documentos con más puntuacion.

## Clases
1. ***Corpus***
2. ***Document***
3. ***Moogle***
4. ***Query***
5. ***SearchItem***
6. ***SearchResult***
7. ***Synonyms***

***[Corpus](https://github.com/Mauro-02/Moogle240722Final/blob/5dab749c342e9c04bdcbe0244181cdd3826cec7a/moogle-main/MoogleEngine/Corpus.cs)***
