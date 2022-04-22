**RESUMEN ENUNCIADO**



Contenidos requeridos para la práctica:

           

Restricciones

   

Extras:

    

**PUNTO DE PARTIDA**

El juego trata de dos bandos en un campo de batalla(Harkonnen y Fremen), que será lo que represente nuestra escena. El escenario es un mapa con dunas y poblados como obstáculos.
Además las tropas de cada uno navegarán por él y tendrán sus instalaciones dispuestan en el mapa.
Elementos de ambos bandos:
    -Base Facility: Edificio base del equipo. En él se generan las nuevas tropas con el script "BaseFacilities". Si se destruye(20 de vida) pierde la partida.
    -Processing Facility: Edificio en el que los extractores depositan el recurso obtenido. Tiene 2 script, "Processing Facility", que guarda la posición en la que las
        extractoras pueden dejar la carga, y "Limited Acces", para bloquear el acceso de más de un extractor a la vez y dar posiciones en las que esperen las demás que quieran descargar.
    -Exploration Unit: Soldados básicos que exploran y atacan, aunque con menos daño que los destructores. Son más rápidas y proactivas gracias al script "ExplorationUnit", que lo mueve
        combinado con el árbol de comportamiento Combat, que selecciona un objetivo al que atacar y le persigue hasta lograrlo. Cambia de objetivo si en este proceso se ve amenzado por
        otra unidad. En caso de no tener objetivo irá a por instalaciones cercanas.
    -Destruction Unit: Combatiente más fuerte y lento, que buscará un objetivo y se centrará en él hasta que lo elimine. Esto se realiza en el script "DestructionUnit" y con el árbol de
        comportamiento de ataque, dejando siempre la amenaza a false, al contrario que los exploradores.
    -Extraction Unit: Unidad extractora de mineral que al llevar al Processing convierte en dinero. Usa el script "Extraction Unit" y el árbol de comportamientos Extraction, el cual
        le lleva al objetivo que se le haya mandado, en caso de no tener y estar cargado irá a dejar la carga a la estación de procesado más cercana. Si no lleva carga irá a extraer
        a la zona de mineral más cercana.
Elementos del mapa:
    -Dunas: Objetos estáticos que bloquean el movimiento de tropas e impiden que pasen por ahí.
    -MelangeFields: Campos de recurso, a los que accederán las extractoras a obtenerlo.
    -Cámaras: La central que visualiza todo el mapa y una en cada bando.
    -GrabenCamp: Campamentos neutros que podrán ser atacados por los dos bandos:
        -GrabenVillage: Campamento central que ordena, mediante el script "Village", que controla la distancia a la que están las tropas atacantes y cuando atacarlas.
        -GrabenTower: Torretas neutrales que atacan a las tropas que se acercan a ellas mediante el script "Tower" y el árbol de comportamiento Defense, que ordena atacar al objetivo
            hasta eliminarlo.
ScenarioManager: Controla todos los elementos neutrales del mapa activos, haciendo que las tropas puedan responder a las diversas situaciones tácticas.
GameManager: A partir del script "RTSGameManager" controla toda la información del juego, los controladores, el coste de cada unidad y su prefab asociado. Inicia el juego y
    reacciona a acciones de las unidades.

El script "Facility" es una clase abstracta para todas las facilities que haya en el juego, detecta los golpes y elimina el edificio si este es destruido.
Todas las tropas y edificios constan del componente "Health", que controla la vida que tiene y recibe daño.
El script "Proyectile" es el que llevan las balas generadas por las torretas.
Además de todos estos comportamientos, existen scripts para que el jugador pueda controlar a uno de los bandos de forma controlada o random.

La carpeta dentro de scripts de comportamientos contiene todas las acciones que utilizan los distinots árboles de comportamiento.

**COMPORTAMIENTOS A AÑADIR**



**IMPLEMENTACIÓN FINAL**



Video YouTube: 
    