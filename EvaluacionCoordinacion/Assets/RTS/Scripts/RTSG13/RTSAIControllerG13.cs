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
using System;
using UnityEngine;

namespace es.ucm.fdi.iav.rts
{
    /*
     * Ejemplo básico sobre cómo crear un controlador basado en IA para el minijuego RTS.
     * Únicamente mandan unas órdenes cualquiera, para probar cosas aleatorias... pero no realiza análisis táctico, ni considera puntos de ruta tácticos, ni coordina acciones de ningún tipo .
     */
    public enum Creations
    {
        CreateExtractor, CreateExplorer, CreateDestructor, 
        Nothing
    }

    public enum Objectives
    {
        AllyBase, EnemyBase, EnemyBestResource, EnemyWorstResource,
        AllyProcessor, EnemyProcessor, EnemyStrogGroup, EnemyWeakGroup,
        NeutralTroups, EnemyExtractor, AllyResource, AllyExtractor,
        Watch, None
    }

    public enum Tactics
    {
        Save, Attack, Defend, Battling, None
    }

    public class RTSAIControllerG13 : RTSAIController
    {
        private List<LimitedAccess> resources;
        private List<Tower> turrets;
        private bool[] resourcesOccupied;
        private List<ExtractionUnit> extractors = new List<ExtractionUnit>();
        private List<ExplorationUnit> explorers = new List<ExplorationUnit>();
        private List<DestructionUnit> destructors = new List<DestructionUnit>();

        private List<ExtractionUnit> enemyExtractors = new List<ExtractionUnit>();
        private List<ExplorationUnit> enemyExplorer = new List<ExplorationUnit>();
        private List<DestructionUnit> enemyDestructors = new List<DestructionUnit>();
        // No necesita guardar mucha información porque puede consultar la que desee por sondeo, incluida toda la información de instalaciones y unidades, tanto propias como ajenas

        // Mi índice de controlador y un par de instalaciones para referenciar
        private GUIStyle money;
        private GUIStyle tacticTag;
        private int MyIndex { get; set; }
        private int EnemyIndex { get; set; }
        private BaseFacility MyBaseFacility { get; set; }
        private ProcessingFacility MyProcessingFacility { get; set; }
        private BaseFacility EnemyBaseFacility { get; set; }
        private ProcessingFacility EnemyProcessingFacility { get; set; }

        int dangerRange = 50;
        public MapaInfluencia map { get; set; }

        // Número de paso de pensamiento 
        bool beggining = true;
        private int ThinkStepNumber { get; set; } = 0;
        private Tactics currentTactic { get; set; } = Tactics.None;
        int extractorsnum = 0;

        // Última unidad creada
        private Unit LastUnit { get; set; }
        //private List<Unit> peloton = new List<Unit>();

        //private int defendRange = 10;
        //private int dangerRange = 20;

        //private int explorerBaseCount = 0;
        //private int explorerProcessingCount = 0;
        //private Transform objectiveT;

        // Despierta el controlado y configura toda estructura interna que sea necesaria
        private void Awake()
        {
            Name = "Controlador";
            Author = "Grupo13: Oscar y Alberto";

            money = new GUIStyle();
            money.fontSize = 16;
            money.normal.textColor = Color.black;

            tacticTag = new GUIStyle();
            tacticTag.fontSize = 11;
            tacticTag.normal.textColor = Color.black;
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

            if (!beggining)
            {
                resources = RTSScenarioManager.Instance.LimitedAccesses;
                extractors = RTSGameManager.Instance.GetExtractionUnits(MyIndex);

                enemyExtractors = RTSGameManager.Instance.GetExtractionUnits(EnemyIndex);
                explorers = RTSGameManager.Instance.GetExplorationUnits(MyIndex);
                enemyExplorer = RTSGameManager.Instance.GetExplorationUnits(EnemyIndex);
                destructors = RTSGameManager.Instance.GetDestructionUnits(MyIndex);
                enemyDestructors = RTSGameManager.Instance.GetDestructionUnits(EnemyIndex);

                turrets = RTSScenarioManager.Instance.Towers;
            }

            switch (ThinkStepNumber)
            {
                case 0:
                    // Lo primer es conocer el índice que me ha asignado el gestor del juego
                    MyIndex = RTSGameManager.Instance.GetIndex(this);

                    // Obtengo referencias a mis cosas
                    MyBaseFacility = RTSGameManager.Instance.GetBaseFacilities(MyIndex)[0];
                    MyProcessingFacility = RTSGameManager.Instance.GetProcessingFacilities(MyIndex)[0];
                    resources = RTSScenarioManager.Instance.LimitedAccesses;
                    // Obtengo referencias a las cosas de mi enemigo
                    // ...
                    var indexList = RTSGameManager.Instance.GetIndexes();
                    indexList.Remove(MyIndex);
                    EnemyIndex = indexList[0];
                    EnemyBaseFacility = RTSGameManager.Instance.GetBaseFacilities(EnemyIndex)[0];
                    EnemyProcessingFacility = RTSGameManager.Instance.GetProcessingFacilities(EnemyIndex)[0];

                    resourcesOccupied = new bool[RTSScenarioManager.Instance.LimitedAccesses.Count];
                    for (int i = 0; i < resourcesOccupied.Length; i++)
                    {
                        resourcesOccupied[i] = false;
                    }

                    enemyExtractors = RTSGameManager.Instance.GetExtractionUnits(EnemyIndex);
                    explorers = RTSGameManager.Instance.GetExplorationUnits(MyIndex);
                    enemyExplorer = RTSGameManager.Instance.GetExplorationUnits(EnemyIndex);
                    destructors = RTSGameManager.Instance.GetDestructionUnits(MyIndex);
                    enemyDestructors = RTSGameManager.Instance.GetDestructionUnits(EnemyIndex);
                    extractors = RTSGameManager.Instance.GetExtractionUnits(MyIndex);

                    turrets = RTSScenarioManager.Instance.Towers;
                    // Construyo por primera vez el mapa de influencia (con las 'capas' que necesite)
                    // ...
                    sortExtractors();
                    ThinkStepNumber++;
                    beggining = false;
                    break;
                case 1:
                    currentTactic = chooseTactic();
                    switch (currentTactic)
                    {
                        case Tactics.Attack:

                            for (int i = 0; i < destructors.Count; i++)
                            {
                                Transform tr;
                                if (EnemyProcessingFacility && destructors.Count / 2 < i)
                                {
                                    tr = EnemyProcessingFacility.transform;
                                }
                                else tr = EnemyBaseFacility.transform;

                                if (destructors[i].Radius < (destructors[i].transform.position - tr.position).magnitude)
                                {
                                    destructors[i].Move(this, tr);
                                }
                                else destructors[i].Attack(this, tr);
                            }

                            for (int i = 0; i < explorers.Count; i++)
                            {
                                Transform tr;
                                if (EnemyProcessingFacility && explorers.Count / 2 < i)
                                {
                                    tr = EnemyProcessingFacility.transform;
                                }
                                else tr = EnemyBaseFacility.transform;

                                if (explorers[i].Radius < (explorers[i].transform.position - tr.position).magnitude)
                                {
                                    explorers[i].Move(this, tr);
                                }
                                else explorers[i].Stop(this);
                            }

                            int moneyAttack = RTSGameManager.Instance.GetMoney(MyIndex);
                            
                            if (explorers.Count + 2 > destructors.Count && moneyAttack >= RTSGameManager.Instance.DestructionUnitCost && destructors.Count < RTSGameManager.Instance.DestructionUnitsMax)
                            {
                                RTSGameManager.Instance.CreateUnit(this, MyBaseFacility, RTSGameManager.UnitType.DESTRUCTION);
                            }
                            else  if (moneyAttack >= RTSGameManager.Instance.ExplorationUnitCost && explorers.Count < RTSGameManager.Instance.ExplorationUnitsMax)
                            {
                                RTSGameManager.Instance.CreateUnit(this, MyBaseFacility, RTSGameManager.UnitType.EXPLORATION);
                            }
                            else if (moneyAttack >= RTSGameManager.Instance.ExtractionUnitCost && extractors.Count < RTSGameManager.Instance.ExtractionUnitsMax)
                            {
                                Unit e = RTSGameManager.Instance.CreateUnit(this, MyBaseFacility, RTSGameManager.UnitType.EXTRACTION);
                                goToClosestResource(e);
                            }
                            break;
                        case Tactics.Battling:

                            for (int i = 0; i < destructors.Count; i++)
                            {
                                int ext = 0;
                                Transform tr;
                                if (enemyExtractors.Count > 0 && enemyExtractors[ext] && destructors.Count / 2 < i)
                                {
                                    tr = enemyExtractors[ext].transform;
                                    ext++;
                                    if (ext > enemyExtractors.Count) ext = 0;
                                }
                                else tr = EnemyProcessingFacility.transform;

                                if (destructors[i].Radius < (destructors[i].transform.position - tr.position).magnitude)
                                {
                                    destructors[i].Move(this, tr);
                                }
                                else destructors[i].Attack(this, tr);
                            }

                            for (int i = 0; i < explorers.Count; i++)
                            {
                                int ext = 0;
                                Transform tr;
                                if (enemyExtractors.Count > 0 && enemyExtractors[ext] && explorers.Count / 2 < i)
                                {
                                    tr = enemyExtractors[ext].transform;
                                    ext++;
                                    if (ext > enemyExtractors.Count) ext = 0;
                                }
                                else tr = EnemyProcessingFacility.transform;

                                if (explorers[i].Radius < (explorers[i].transform.position - tr.position).magnitude)
                                {
                                    explorers[i].Move(this, tr);
                                }
                                else explorers[i].Stop(this);
                            }

                            int moneyBattle = RTSGameManager.Instance.GetMoney(MyIndex);
                            //Priorizamos la compra de destructores sobre los exploradores
                            if (moneyBattle >= RTSGameManager.Instance.ExplorationUnitCost &&
                                    explorers.Count < RTSGameManager.Instance.ExplorationUnitsMax)
                            {
                                RTSGameManager.Instance.CreateUnit(this, MyBaseFacility, RTSGameManager.UnitType.EXPLORATION);

                            }
                            else if (moneyBattle >= RTSGameManager.Instance.DestructionUnitCost &&
                                    destructors.Count < RTSGameManager.Instance.DestructionUnitsMax)
                            {
                                RTSGameManager.Instance.CreateUnit(this, MyBaseFacility, RTSGameManager.UnitType.DESTRUCTION);

                            }
                            else if (moneyBattle >= RTSGameManager.Instance.ExtractionUnitCost && extractors.Count < RTSGameManager.Instance.ExtractionUnitsMax)
                            {
                                Unit e = RTSGameManager.Instance.CreateUnit(this, MyBaseFacility, RTSGameManager.UnitType.EXTRACTION);
                                goToClosestResource(e);
                            }
                            break;
                        case Tactics.Defend:
                            
                            if (needToProtectBase())
                            {
                                for (int i = 0; i < destructors.Count/2; i++)
                                {
                                    Transform tr = MyBaseFacility.transform;

                                    if (destructors[i].Radius < (destructors[i].transform.position - tr.position).magnitude)
                                    {
                                        destructors[i].Move(this, tr);
                                    }
                                    else destructors[i].Stop(this);
                                }

                                for (int i = 0; i < explorers.Count / 2; i++)
                                {
                                    Transform tr = MyBaseFacility.transform;

                                    if (explorers[i].Radius < (explorers[i].transform.position - tr.position).magnitude)
                                    {
                                        explorers[i].Move(this, tr);
                                    }
                                    else explorers[i].Stop(this);
                                }
                            }
                            if (MyBaseFacility && needToProtectProccessing())
                            {
                                for (int i = destructors.Count / 2; i < destructors.Count; i++)
                                {
                                    Transform tr = MyProcessingFacility.transform;

                                    if (destructors[i].Radius < (destructors[i].transform.position - tr.position).magnitude)
                                    {
                                        destructors[i].Move(this, tr);
                                    }
                                    else destructors[i].Stop(this);
                                }

                                for (int i = explorers.Count / 2; i < explorers.Count; i++)
                                {
                                    Transform tr = MyProcessingFacility.transform;

                                    if (explorers[i].Radius < (explorers[i].transform.position - tr.position).magnitude)
                                    {
                                        explorers[i].Move(this, tr);
                                    }
                                    else explorers[i].Stop(this);
                                }
                            }

                            int moneyDefense = RTSGameManager.Instance.GetMoney(MyIndex);

                            if (extractors.Count >= 2 && explorers.Count < RTSGameManager.Instance.ExplorationUnitsMax / 3 && moneyDefense >= RTSGameManager.Instance.ExplorationUnitCost && explorers.Count < RTSGameManager.Instance.ExplorationUnitsMax)
                            {
                                RTSGameManager.Instance.CreateUnit(this, MyBaseFacility, RTSGameManager.UnitType.EXPLORATION);

                            }
                            else if (extractors.Count >= 2 && destructors.Count < RTSGameManager.Instance.DestructionUnitsMax / 3 && moneyDefense >= RTSGameManager.Instance.DestructionUnitCost && destructors.Count < RTSGameManager.Instance.DestructionUnitsMax)
                            {
                                RTSGameManager.Instance.CreateUnit(this, MyBaseFacility, RTSGameManager.UnitType.DESTRUCTION).GetComponent<DestructionUnit>();

                            }
                            else if (extractors.Count < 2 && moneyDefense >= RTSGameManager.Instance.ExtractionUnitCost && extractors.Count < RTSGameManager.Instance.ExtractionUnitsMax)
                            {
                                Unit e = RTSGameManager.Instance.CreateUnit(this, MyBaseFacility, RTSGameManager.UnitType.EXTRACTION);
                                goToClosestResource(e);
                            }

                            break;
                        case Tactics.Save:
                            for (int i = 0; i < destructors.Count / 2; i++)
                            {
                                int ext = 0;
                                Transform tr = null;
                                if (extractors.Count > 0 && extractors[ext] && destructors.Count / 2 < i)
                                {
                                    tr = extractors[ext].transform;
                                    ext++;
                                    if (ext > extractors.Count) ext = 0;
                                }
                                else if (MyProcessingFacility) tr = MyProcessingFacility.transform;

                                if (destructors[i].Radius < (destructors[i].transform.position - tr.position).magnitude)
                                {
                                    destructors[i].Move(this, tr);
                                }
                                else destructors[i].Stop(this);
                            }

                            for (int i = 0; i < explorers.Count / 2; i++)
                            {
                                int ext = 0;
                                Transform tr = null;
                                if (extractors.Count > 0 && extractors[ext] && explorers.Count / 2 < i)
                                {
                                    tr = extractors[ext].transform;
                                    ext++;
                                    if (ext > extractors.Count) ext = 0;
                                }
                                else if (MyProcessingFacility) tr = MyProcessingFacility.transform;

                                if (explorers[i].Radius < (explorers[i].transform.position - tr.position).magnitude)
                                {
                                    explorers[i].Move(this, tr);
                                }
                                else explorers[i].Stop(this);
                            }

                            int moneySave = RTSGameManager.Instance.GetMoney(MyIndex);

                            if ((extractors.Count <= 3 || explorers.Count >= RTSGameManager.Instance.ExplorationUnitsMax / 2) && moneySave >= RTSGameManager.Instance.ExtractionUnitCost && extractors.Count < RTSGameManager.Instance.ExtractionUnitsMax)
                            {
                                Unit e = RTSGameManager.Instance.CreateUnit(this, MyBaseFacility, RTSGameManager.UnitType.EXTRACTION);
                                goToClosestResource(e);
                            }
                            else if (moneySave >= RTSGameManager.Instance.ExplorationUnitCost && explorers.Count < RTSGameManager.Instance.ExplorationUnitsMax)
                            {
                                RTSGameManager.Instance.CreateUnit(this, MyBaseFacility, RTSGameManager.UnitType.EXPLORATION);
                            }
                            break;
                    }
                    break;
                case 2:
                    Stop = true;
                    break;
            }

            if (RTSGameManager.Instance.GetBaseFacilities(MyIndex).Count <= 0 || RTSGameManager.Instance.GetBaseFacilities(EnemyIndex).Count <= 0)
            {
                ThinkStepNumber = 2;
            }
        }

        Tactics chooseTactic()
        {
            int money = RTSGameManager.Instance.GetMoney(MyIndex);
            bool canAttack = (explorers.Count + destructors.Count > enemyExplorer.Count + enemyDestructors.Count + 6);

            if (extractors.Count <= 0 && money < RTSGameManager.Instance.ExtractionUnitCost)
            {
                return Tactics.Attack;
            }
            else if (needToProtect())
            {
                return Tactics.Defend;
            }
            else if (destructors.Count + explorers.Count == 0 && currentTactic == Tactics.Attack)
            {
                return Tactics.Save;
            }
            else if (currentTactic == Tactics.Save && (destructors.Count > 0 || explorers.Count > 0) && enemyExplorer.Count + enemyDestructors.Count == 0)
            {
                return Tactics.Attack;
            }
            else if (currentTactic == Tactics.Save && extractors.Count + 2 > RTSGameManager.Instance.ExtractionUnitsMax && RTSGameManager.Instance.ExplorationUnitsMax < explorers.Count * 4)
            {
                return Tactics.Battling;
            }
            else if (destructors.Count + explorers.Count < 3 && currentTactic != Tactics.Attack)
            {
                return Tactics.Save;
            }
            else if (enemyDestructors.Count + enemyExplorer.Count <= 2)
            {
                return Tactics.Attack;
            }
            else if (canAttack)
            {
                if (EnemyProcessingFacility)
                    return Tactics.Battling;
                else
                    return Tactics.Attack;
            }
            return Tactics.Save;
        }

        void sortExtractors()
        {
            for (int i = 0; i < resourcesOccupied.Length; i++)
            {
                resourcesOccupied[i] = false;
            }
            for (int i = 0; i < extractors.Count; i++)
            {
                goToClosestResource(extractors[i]);
            }
        }

        private void goToClosestResource(Unit e)
        {
            LimitedAccess newResource = null;
            float distance = 100000;
            bool occupied = false;
            int taken = -1;
            for (int i = 0; i < resources.Count; i++)
            { 
                float resourceDist = (e.transform.position - resources[i].transform.position).magnitude;
                if (!resourcesOccupied[i] && resourceDist < distance)
                {
                    newResource = resources[i];
                    distance = resourceDist;
                    taken = i;
                }
            }
            if (newResource)
            {
                e.Move(this, newResource.transform);
                resourcesOccupied[taken] = true; ;
            }
        }

        bool needToProtect() {
            bool enemy = false;
            float closestEnemyInProcessingRange = 1000000;
            for (int i = 0; i < enemyExplorer.Count; i++)
            {
                if (MyBaseFacility != null && inRange(enemyExplorer[i].transform, MyBaseFacility.transform))
                {
                    return true;
                }
                else if (MyProcessingFacility != null && inRange(enemyExplorer[i].transform, MyProcessingFacility.transform))
                {
                    return true;
                }
            }
            for (int i = 0; i < enemyDestructors.Count; i++)
            {
                if (MyBaseFacility != null && inRange(enemyDestructors[i].transform, MyBaseFacility.transform))
                {
                    return true;
                }
                else if (MyProcessingFacility != null && inRange(enemyDestructors[i].transform, MyProcessingFacility.transform))
                {
                    return true;
                }
            }
            return false;
        }

        bool needToProtectBase()
        {
            bool enemy = false;
            float closestEnemyInProcessingRange = 1000000;
            for (int i = 0; i < enemyExplorer.Count; i++)
            {
                if (MyBaseFacility != null && inRange(enemyExplorer[i].transform, MyBaseFacility.transform))
                {
                    return true;
                }
            }
            for (int i = 0; i < enemyDestructors.Count; i++)
            {
                if (MyBaseFacility != null && inRange(enemyDestructors[i].transform, MyBaseFacility.transform))
                {
                    return true;
                }
            }
            return false;
        }

        bool needToProtectProccessing()
        {
            bool enemy = false;
            float closestEnemyInProcessingRange = 1000000;
            for (int i = 0; i < enemyExplorer.Count; i++)
            {
                if (MyProcessingFacility != null && inRange(enemyExplorer[i].transform, MyProcessingFacility.transform))
                {
                    return true;
                }
            }
            for (int i = 0; i < enemyDestructors.Count; i++)
            {
                if (MyProcessingFacility != null && inRange(enemyDestructors[i].transform, MyProcessingFacility.transform))
                {
                    return true;
                }
            }
            return false;
        }

        bool inRange(Transform unit, Transform place)
        {
            float dist = (unit.transform.position - place.transform.position).magnitude;
            return (dist < dangerRange);
        }

        float getRange(Transform unit, Transform place)
        {
            float dist = (unit.transform.position - place.transform.position).magnitude;
            return dist;
        }

        private void OnGUI()
        {
            float areaWidth = 150;
            float areaHeight = 250;
            if (MyIndex % 2 == 0)
                GUILayout.BeginArea(new Rect(0, 0, areaWidth, areaHeight));
            else
                GUILayout.BeginArea(new Rect(Screen.width - areaWidth, 0, Screen.width, areaHeight));
            GUILayout.BeginVertical();


            GUILayout.Label("[ C" + MyIndex + " ] " + RTSGameManager.Instance.GetMoney(MyIndex) + " solaris", money);

            GUILayout.Label("Using " + currentTactic.ToString() + " tactic", tacticTag);

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}