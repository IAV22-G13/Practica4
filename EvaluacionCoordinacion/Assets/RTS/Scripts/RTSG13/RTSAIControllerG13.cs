/*    
   Copyright (C) 2020 Federico Peinado
   http://www.federicopeinado.com

   Este fichero forma parte del material de la asignatura Inteligencia Artificial para Videojuegos.
   Esta asignatura se imparte en la Facultad de Inform�tica de la Universidad Complutense de Madrid (Espa�a).

   Autores originales: Opsive (Behavior Designer Samples)
   Revisi�n: Federico Peinado 
   Contacto: email@federicopeinado.com
*/
using System.Collections.Generic;
using UnityEngine;

namespace es.ucm.fdi.iav.rts
{
    /*
     * Ejemplo b�sico sobre c�mo crear un controlador basado en IA para el minijuego RTS.
     * �nicamente mandan unas �rdenes cualquiera, para probar cosas aleatorias... pero no realiza an�lisis t�ctico, ni considera puntos de ruta t�cticos, ni coordina acciones de ning�n tipo .
     */
    public class RTSAIControllerG13 : RTSAIController
    {
        public enum Actions
        {
            Begginig, 
            MoveNewExtractor, MoveNewExplorer, MoveNewDestructor,
            MoveAllExtractor, MoveAllExplorer, MoveAllDestructor,
            MoveExplorerGroup, StopExtractor, Nothing, MoveAll
        }

        public enum Creations
        {
            CreateExtractor, CreateExplorer, CreateDestructor, Nothing
        }

        public enum Objectives
        {
            AllyBase, EnemyBase, AllyProcessor, EnemyProcessor,
            ClosestEnemy, ClosestEnemyFacility, None
        }

        public GameObject[] resources;

        private List<ExtractionUnit> extractors;
        private List<ExplorationUnit> explorers;
        private List<DestructionUnit> destructors;

        private List<ExtractionUnit> enemyExtractors;
        private List<ExplorationUnit> enemyExplorer;
        private List<DestructionUnit> enemyDestructors;
        // No necesita guardar mucha informaci�n porque puede consultar la que desee por sondeo, incluida toda la informaci�n de instalaciones y unidades, tanto propias como ajenas

        // Mi �ndice de controlador y un par de instalaciones para referenciar
        private int MyIndex { get; set; }
        private int EnemyIndex { get; set; }
        private BaseFacility MyBaseFacility { get; set; }
        private ProcessingFacility MyProcessingFacility { get; set; }
        private BaseFacility EnemyBaseFacility { get; set; }
        private ProcessingFacility EnemyProcessingFacility { get; set; }

        public MapaInfluencia map { get; set; }

        // N�mero de paso de pensamiento 
        private Actions ThinkStepAction { get; set; } = 0;
        private Creations ThinkStepCreation { get; set; } = 0;
        private Objectives ThinkStepObjective { get; set; } = 0;

        // �ltima unidad creada
        private Unit LastUnit { get; set; }

        // Despierta el controlado y configura toda estructura interna que sea necesaria
        private void Awake()
        {
            Name = "Controlador";
            Author = "Grupo13";
        }

        // El m�todo de pensar que sobreescribe e implementa el controlador, para percibir (hacer mapas de influencia, etc.) y luego actuar.
        protected override void Think()
        {
            // Actualizo el mapa de influencia 
            // ...

            // Para las �rdenes aqu� estoy asumiendo que tengo dinero de sobra y que se dan las condiciones de todas las cosas...
            // (Ojo: esto no deber�a hacerse porque si me equivoco, causar� fallos en el juego... hay que comprobar que cada llamada tiene sentido y es posible hacerla)

            // Aqu� lo suyo ser�a elegir bien la acci�n a realizar. 
            // En este caso como es para probar, voy dando a cada vez una orden de cada tipo, todo de seguido y muy aleatorio... 
            if (ThinkStepAction != Actions.Begginig)
            {
                extractors = RTSGameManager.Instance.GetExtractionUnits(MyIndex);
                explorers = RTSGameManager.Instance.GetExplorationUnits(MyIndex);
                destructors = RTSGameManager.Instance.GetDestructionUnits(MyIndex);
                enemyExtractors = RTSGameManager.Instance.GetExtractionUnits(EnemyIndex);
                enemyExplorer = RTSGameManager.Instance.GetExplorationUnits(EnemyIndex);
                enemyDestructors = RTSGameManager.Instance.GetDestructionUnits(EnemyIndex);
            }

            switch (ThinkStepAction)
            {
                case 0:
                    // Lo primer es conocer el �ndice que me ha asignado el gestor del juego
                    MyIndex = RTSGameManager.Instance.GetIndex(this);

                    // Obtengo referencias a mis cosas
                    MyBaseFacility = RTSGameManager.Instance.GetBaseFacilities(MyIndex)[0];
                    MyProcessingFacility = RTSGameManager.Instance.GetProcessingFacilities(MyIndex)[0];

                    // Obtengo referencias a las cosas de mi enemigo
                    // ...
                    var indexList = RTSGameManager.Instance.GetIndexes();
                    indexList.Remove(MyIndex);
                    EnemyIndex = indexList[0];
                    EnemyBaseFacility = RTSGameManager.Instance.GetBaseFacilities(EnemyIndex)[0];
                    EnemyProcessingFacility = RTSGameManager.Instance.GetProcessingFacilities(EnemyIndex)[0];
                    // Construyo por primera vez el mapa de influencia (con las 'capas' que necesite)
                    // ...
                    break;
                case Actions.MoveNewExtractor: 

                    break;
            }

            switch (ThinkStepCreation)
            {
                case Creations.CreateExtractor:
                    Unit u = RTSGameManager.Instance.CreateUnit(this, MyBaseFacility, RTSGameManager.UnitType.EXTRACTION);
                    sendExtractorToClosestResource(u);
                    break;
                case Creations.CreateExplorer:
                    RTSGameManager.Instance.CreateUnit(this, MyBaseFacility, RTSGameManager.UnitType.EXPLORATION);
                    break;
                case Creations.CreateDestructor:
                    RTSGameManager.Instance.CreateUnit(this, MyBaseFacility, RTSGameManager.UnitType.DESTRUCTION);
                    break;
            }
            //Debug.Log("Controlador autom�tico " + MyIndex + " ha finalizado el paso de pensamiento " + ThinkStepNumber);
            if (extractors.Count > 3 && explorers.Count > (10 * destructors.Count))
            {
                ThinkStepCreation = Creations.CreateDestructor;
            }
            else if (extractors.Count > 1 && explorers.Count < extractors.Count * 5)
            {
                ThinkStepCreation = Creations.CreateExplorer;
            }
            else
            {
                ThinkStepCreation = Creations.CreateExtractor;
            }
        }

        void sendExtractorToClosestResource(Unit u)
        {
            int resourceNum = extractors.Count - 1;
            RTSGameManager.Instance.MoveUnit(this, u, resources[resourceNum].transform);
        }

        void sendExplorerToVunerableEnemyPoint() { }
        void sendExplorerToVunerableAllyPoint() { }

        void sendDestructorToVunerableEnemyPoint() { }
        void sendDestructorToVunerableAllyPoint() { }
    }
}