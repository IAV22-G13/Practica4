/*    
   Copyright (C) 2020 Federico Peinado
   http://www.federicopeinado.com

   Este fichero forma parte del material de la asignatura Inteligencia Artificial para Videojuegos.
   Esta asignatura se imparte en la Facultad de Informática de la Universidad Complutense de Madrid (España).

   Autores originales: Opsive (Behavior Designer Samples)
   Revisión: Federico Peinado 
   Contacto: email@federicopeinado.com
*/
using System.Collections.Generic;
using UnityEngine;

namespace es.ucm.fdi.iav.rts
{
    /*
     * Ejemplo básico sobre cómo crear un controlador basado en IA para el minijuego RTS.
     * Únicamente mandan unas órdenes cualquiera, para probar cosas aleatorias... pero no realiza análisis táctico, ni considera puntos de ruta tácticos, ni coordina acciones de ningún tipo .
     */
    public class RTSAIControllerG13 : RTSAIController
    {

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

        private List<ExtractionUnit> extractors = new List<ExtractionUnit>();
        private List<ExplorationUnit> explorers = new List<ExplorationUnit>();
        private List<DestructionUnit> destructors = new List<DestructionUnit>();

        private List<ExtractionUnit> enemyExtractors = new List<ExtractionUnit>();
        private List<ExplorationUnit> enemyExplorer = new List<ExplorationUnit>();
        private List<DestructionUnit> enemyDestructors = new List<DestructionUnit>();
        // No necesita guardar mucha información porque puede consultar la que desee por sondeo, incluida toda la información de instalaciones y unidades, tanto propias como ajenas

        // Mi índice de controlador y un par de instalaciones para referenciar
        private int MyIndex { get; set; }
        private int EnemyIndex { get; set; }
        private BaseFacility MyBaseFacility { get; set; }
        private ProcessingFacility MyProcessingFacility { get; set; }
        private BaseFacility EnemyBaseFacility { get; set; }
        private ProcessingFacility EnemyProcessingFacility { get; set; }

        public MapaInfluencia map { get; set; }

        // Número de paso de pensamiento 
        bool beggining = true; 
        private Creations ThinkStepCreation { get; set; } = 0;
        private Objectives ThinkStepObjective { get; set; } = 0;

        // Última unidad creada
        private Unit LastUnit { get; set; }
        private List<Unit> peloton = new List<Unit>();

        private float defendRange = 10.5f;

        private int explorerBaseCount = 0;
        private int explorerProcessingCount = 0;
        private Transform objectiveT;

        // Despierta el controlado y configura toda estructura interna que sea necesaria
        private void Awake()
        {
            Name = "Controlador";
            Author = "Grupo13";
        }

        // El método de pensar que sobreescribe e implementa el controlador, para percibir (hacer mapas de influencia, etc.) y luego actuar.
        protected override void Think()
        {
            // Actualizo el mapa de influencia 
            // ...

            // Para las órdenes aquí estoy asumiendo que tengo dinero de sobra y que se dan las condiciones de todas las cosas...
            // (Ojo: esto no debería hacerse porque si me equivoco, causaré fallos en el juego... hay que comprobar que cada llamada tiene sentido y es posible hacerla)

            // Aquí lo suyo sería elegir bien la acción a realizar. 
            // En este caso como es para probar, voy dando a cada vez una orden de cada tipo, todo de seguido y muy aleatorio... 
            if (beggining)
            {
                // Lo primer es conocer el índice que me ha asignado el gestor del juego
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
                beggining = false;
            }

            extractors = RTSGameManager.Instance.GetExtractionUnits(MyIndex);
            explorers = RTSGameManager.Instance.GetExplorationUnits(MyIndex);
            destructors = RTSGameManager.Instance.GetDestructionUnits(MyIndex);
            enemyExtractors = RTSGameManager.Instance.GetExtractionUnits(EnemyIndex);
            enemyExplorer = RTSGameManager.Instance.GetExplorationUnits(EnemyIndex);
            enemyDestructors = RTSGameManager.Instance.GetDestructionUnits(EnemyIndex);
            actualizeProtection();

            for (int i = 0; i < peloton.Count; i++)
            {
                RTSGameManager.Instance.MoveUnit(this, peloton[i], objectiveT);
            }
            peloton.Clear();

            switch (ThinkStepCreation)
            {
                case Creations.CreateExtractor:
                    if (RTSGameManager.Instance.GetMoney(MyIndex) > RTSGameManager.Instance.ExtractionUnitCost)
                    {
                        LastUnit = RTSGameManager.Instance.CreateUnit(this, MyBaseFacility, RTSGameManager.UnitType.EXTRACTION);
                        sendExtractorToClosestResource(LastUnit);
                    }
                    break;
                case Creations.CreateExplorer:
                    if (RTSGameManager.Instance.GetMoney(MyIndex) > RTSGameManager.Instance.ExplorationUnitCost)
                    {
                        LastUnit = RTSGameManager.Instance.CreateUnit(this, MyBaseFacility, RTSGameManager.UnitType.EXPLORATION);
                        sendNewExplorerToWatch(LastUnit);
                    }
                    break;
                case Creations.CreateDestructor:
                    if (RTSGameManager.Instance.GetMoney(MyIndex) > RTSGameManager.Instance.DestructionUnitCost)
                    {
                        LastUnit = RTSGameManager.Instance.CreateUnit(this, MyBaseFacility, RTSGameManager.UnitType.DESTRUCTION);
                        sendNewExplorerToWatch(LastUnit);
                    }
                    break;
                case Creations.Nothing:
                    break;
            }

            //Debug.Log("Controlador automático " + MyIndex + " ha finalizado el paso de pensamiento " + ThinkStepNumber);
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

            if (needToProtect())
            {
                for (int i = 0; i < destructors.Count; i++)
                {
                    if ((ThinkStepObjective == Objectives.AllyBase && !inRange(destructors[i].transform, MyBaseFacility.transform)) ||
                        (ThinkStepObjective == Objectives.AllyProcessor && !inRange(destructors[i].transform, MyProcessingFacility.transform)))
                    {
                        peloton.Add(destructors[i]);
                        break;
                    }
                }
                for (int i = 0; i < explorers.Count; i++)
                {
                    if ((ThinkStepObjective == Objectives.AllyBase && !inRange(explorers[i].transform, MyBaseFacility.transform)) ||
                        (ThinkStepObjective == Objectives.AllyProcessor && !inRange(explorers[i].transform, MyProcessingFacility.transform)))
                    {
                        peloton.Add(explorers[i]);
                    }
                    if (peloton.Count >= 5) break;
                }
            }
            else if (canAttack())
            {
                for (int i = 0; i < destructors.Count; i++)
                {
                    if ((ThinkStepObjective == Objectives.EnemyBase && !inRange(destructors[i].transform, MyBaseFacility.transform)) ||
                        (ThinkStepObjective == Objectives.EnemyProcessor && !inRange(destructors[i].transform, MyProcessingFacility.transform)))
                    {
                        peloton.Add(destructors[i]);
                        break;
                    }
                }
                for (int i = 0; i < explorers.Count; i++)
                {
                    if ((ThinkStepObjective == Objectives.EnemyBase && !inRange(explorers[i].transform, MyBaseFacility.transform)) ||
                        (ThinkStepObjective == Objectives.EnemyProcessor && !inRange(explorers[i].transform, MyProcessingFacility.transform)))
                    {
                        peloton.Add(explorers[i]);
                    }
                    if (peloton.Count > 2) break;
                }
            }
            switch (ThinkStepObjective)
            {
                case Objectives.AllyBase:
                    if (MyBaseFacility != null)
                        objectiveT = MyBaseFacility.transform;
                    break;
                case Objectives.AllyProcessor:
                    if (MyProcessingFacility != null)
                        objectiveT = MyProcessingFacility.transform;
                    break;
                case Objectives.EnemyBase:
                    if (EnemyBaseFacility != null)
                        objectiveT = EnemyBaseFacility.transform;
                    break;
                case Objectives.EnemyProcessor:
                    if (EnemyProcessingFacility != null)
                        objectiveT = EnemyProcessingFacility.transform;
                    break;
            }
        }

        void actualizeExtractors()
        {
            for (int i = 0; i < extractors.Count - 1; i++)
            {
                RTSGameManager.Instance.MoveUnit(this, extractors[i], resources[i].transform);
            }
        }

        void sendExtractorToClosestResource(Unit u)
        {
            actualizeExtractors();
            int resourceNum = extractors.Count - 1;
            RTSGameManager.Instance.MoveUnit(this, u, resources[resourceNum].transform);
        }

        void actualizeProtection()
        {
            explorerBaseCount = 0;
            explorerProcessingCount = 0;
            for (int i = 0; i < explorers.Count; i++)
            {
                if (MyBaseFacility != null && inRange(explorers[i].transform, MyBaseFacility.transform))
                    explorerBaseCount++;
                else if (MyProcessingFacility != null && inRange(explorers[i].transform, MyProcessingFacility.transform))
                    explorerProcessingCount++;
            }
        }

        void sendNewExplorerToWatch(Unit u)
        {
            if (MyBaseFacility != null && explorerBaseCount >= explorerProcessingCount)
            {
                Transform t = MyBaseFacility.gameObject.transform;
                float r = Random.Range(-defendRange, defendRange);
                float f = Random.Range(-defendRange, defendRange);
                t.position = new Vector3(t.position.x + r, t.position.y, t.position.z + f);
                RTSGameManager.Instance.MoveUnit(this, u, t);
            }
            else if (MyProcessingFacility != null) 
            {
                Transform t = MyProcessingFacility.gameObject.transform;
                float r = Random.Range(-defendRange, defendRange);
                float f = Random.Range(-defendRange, defendRange);
                t.position = new Vector3(t.position.x + r, t.position.y, t.position.z + f);
                RTSGameManager.Instance.MoveUnit(this, u, t);
            }
        }

        bool needToProtect() {
            int enemyInRangeBase = 0;
            int enemyInRangeProcessing = 0;
            for (int i = 0; i < enemyExplorer.Count; i++)
            {
                if (MyBaseFacility != null && inRange(enemyExplorer[i].transform, MyBaseFacility.transform))
                    enemyInRangeBase++;
                else if (MyProcessingFacility != null && inRange(enemyExplorer[i].transform, MyProcessingFacility.transform))
                    enemyInRangeProcessing++;
            }
            if (enemyInRangeBase > explorerBaseCount)
            {
                ThinkStepObjective = Objectives.AllyBase;
                return true;
            }
            else if (enemyInRangeProcessing > explorerProcessingCount)
            {
                ThinkStepObjective = Objectives.AllyProcessor;
                return true;
            }
            return false;
        }

        bool canAttack()
        {
            int enemyInRangeBase = 0;
            int enemyInRangeProcessing = 0;
            for (int i = 0; i < enemyExplorer.Count; i++)
            {
                if (EnemyBaseFacility!=null && inRange(enemyExplorer[i].transform, EnemyBaseFacility.transform))
                    enemyInRangeBase++;
                else if (EnemyProcessingFacility != null && inRange(enemyExplorer[i].transform, EnemyProcessingFacility.transform))
                    enemyInRangeProcessing++;
            }
            if (enemyInRangeBase + 3 <= explorerBaseCount)
            {
                ThinkStepObjective = Objectives.EnemyBase;
                return true;
            }
            else if (enemyInRangeProcessing + 3 <= explorerProcessingCount)
            {
                ThinkStepObjective = Objectives.EnemyProcessor;
                return true;
            }
            return false;
        }

        bool inRange(Transform unit, Transform place)
        {
            return (place.position.x - defendRange < unit.position.x && place.position.x + defendRange > unit.position.x &&
                place.position.z - defendRange < unit.position.z && place.position.z + defendRange > unit.position.z);
        }
    }
}