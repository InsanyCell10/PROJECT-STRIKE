using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InsanyStudios.Editor
{
    /// <summary>
    /// Builds a first playable greybox for PROJECT STRIKE without changing SampleScene.
    /// Run from Tools > INSANY STUDIOS > Build Valley Station.
    /// </summary>
    public static class WarScenarioBuilder
    {
        private const string ScenePath = "Assets/Scenes/WarScenario/ValleyStation.unity";
        private const string MaterialsPath = "Assets/Scenes/WarScenario/Materials";
        private const string PlayerPrefabPath = "Assets/Starter Assets/Runtime/FirstPersonController/Prefabs/PlayerCapsule.prefab";

        private static Material asphalt;
        private static Material concrete;
        private static Material paleConcrete;
        private static Material darkConcrete;
        private static Material rubble;
        private static Material metal;
        private static Material rust;
        private static Material sandbag;
        private static Material glass;
        private static Material water;
        private static Material roadMarking;
        private static Material warning;
        private static Material beacon;
        private static Material clinicRed;

        [MenuItem("Tools/INSANY STUDIOS/Build Valley Station War Scenario")]
        public static void Build()
        {
            if (File.Exists(ScenePath))
            {
                Debug.LogError("WarScenarioBuilder stopped: the target scene already exists, so it will not overwrite it: " + ScenePath);
                return;
            }

            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            if (playerPrefab == null)
            {
                Debug.LogError("WarScenarioBuilder could not find the Starter Assets FPS prefab at: " + PlayerPrefabPath);
                return;
            }

            LoadMaterials();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "ValleyStation";
            ConfigureAtmosphere();
            BuildLighting();
            BuildGroundAndRoads();
            BuildDistricts();
            BuildRailYardAndStation();
            BuildBridgeAndRiver();
            BuildCombatCoverAndDamage();
            BuildTransmitter();
            PlacePlayer(playerPrefab);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Created additive greybox scene: " + ScenePath + ". SampleScene was not changed.");
        }

        private static void LoadMaterials()
        {
            asphalt = MakeMaterial("Mat_War_Asphalt", new Color(0.105f, 0.115f, 0.12f));
            concrete = MakeMaterial("Mat_War_Concrete", new Color(0.34f, 0.35f, 0.34f));
            paleConcrete = MakeMaterial("Mat_War_PaleConcrete", new Color(0.47f, 0.46f, 0.42f));
            darkConcrete = MakeMaterial("Mat_War_DarkConcrete", new Color(0.19f, 0.20f, 0.19f));
            rubble = MakeMaterial("Mat_War_Rubble", new Color(0.28f, 0.27f, 0.24f));
            metal = MakeMaterial("Mat_War_Metal", new Color(0.22f, 0.25f, 0.25f), 0.55f);
            rust = MakeMaterial("Mat_War_Rust", new Color(0.28f, 0.13f, 0.075f), 0.2f);
            sandbag = MakeMaterial("Mat_War_Sandbag", new Color(0.39f, 0.35f, 0.25f));
            glass = MakeMaterial("Mat_War_Glass", new Color(0.085f, 0.12f, 0.14f), 0.25f);
            water = MakeMaterial("Mat_War_Channel", new Color(0.055f, 0.095f, 0.10f), 0.15f);
            roadMarking = MakeMaterial("Mat_War_RoadMarking", new Color(0.63f, 0.54f, 0.35f));
            warning = MakeMaterial("Mat_War_Warning", new Color(0.82f, 0.39f, 0.075f));
            beacon = MakeMaterial("Mat_War_Beacon", new Color(0.95f, 0.16f, 0.055f), 0f, true);
            clinicRed = MakeMaterial("Mat_War_ClinicSign", new Color(0.72f, 0.12f, 0.09f));
        }

        private static Material MakeMaterial(string name, Color color, float metallicValue = 0f, bool emissive = false)
        {
            string path = MaterialsPath + "/" + name + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            material = new Material(shader);
            material.name = name;
            material.color = color;
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallicValue);
            if (emissive && material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * 2.25f);
            }
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void ConfigureAtmosphere()
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.36f, 0.36f, 0.33f);
            RenderSettings.fogStartDistance = 42f;
            RenderSettings.fogEndDistance = 175f;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.31f, 0.34f, 0.35f);
            RenderSettings.ambientEquatorColor = new Color(0.23f, 0.23f, 0.21f);
            RenderSettings.ambientGroundColor = new Color(0.10f, 0.095f, 0.085f);
            RenderSettings.ambientIntensity = 0.8f;
        }

        private static void BuildLighting()
        {
            GameObject sunObject = new GameObject("Directional Light - late smoke haze");
            Light sun = sunObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.82f, 0.66f);
            sun.intensity = 1.15f;
            sun.shadows = LightShadows.Soft;
            sunObject.transform.rotation = Quaternion.Euler(34f, -28f, 0f);
            RenderSettings.sun = sun;

            GameObject distantGlow = new GameObject("Station emergency glow");
            Light stationLight = distantGlow.AddComponent<Light>();
            stationLight.type = LightType.Point;
            stationLight.color = new Color(1f, 0.23f, 0.08f);
            stationLight.intensity = 4f;
            stationLight.range = 18f;
            distantGlow.transform.position = new Vector3(8f, 10f, 27f);
        }

        private static void BuildGroundAndRoads()
        {
            GameObject ground = NewGroup("01 - Ground and route");
            Block("Lower valley bed", ground.transform, new Vector3(0f, -4f, 4f), new Vector3(110f, 2f, 228f), darkConcrete);
            Block("South district ground", ground.transform, new Vector3(0f, -0.15f, -18f), new Vector3(104f, 0.3f, 164f), concrete);
            Block("North bank ground", ground.transform, new Vector3(0f, -0.15f, 99f), new Vector3(104f, 0.3f, 22f), concrete);
            Block("Main avenue", ground.transform, new Vector3(0f, 0.045f, -16f), new Vector3(9f, 0.12f, 158f), asphalt);
            Block("Avenue centre broken marking A", ground.transform, new Vector3(0f, 0.11f, -68f), new Vector3(0.10f, 0.012f, 5f), roadMarking);
            Block("Avenue centre broken marking B", ground.transform, new Vector3(0f, 0.11f, -53f), new Vector3(0.10f, 0.012f, 5f), roadMarking);
            Block("Avenue centre broken marking C", ground.transform, new Vector3(0f, 0.11f, -38f), new Vector3(0.10f, 0.012f, 5f), roadMarking);
            Block("Avenue centre broken marking D", ground.transform, new Vector3(0f, 0.11f, 4f), new Vector3(0.10f, 0.012f, 5f), roadMarking);
            Block("Avenue centre broken marking E", ground.transform, new Vector3(0f, 0.11f, 8f), new Vector3(0.10f, 0.012f, 4f), roadMarking);

            Block("Rail yard service road", ground.transform, new Vector3(0f, 0.035f, 43f), new Vector3(90f, 0.10f, 7f), asphalt);
            Block("West service lane", ground.transform, new Vector3(-18f, 0.04f, -4f), new Vector3(6f, 0.10f, 74f), asphalt);
            Block("Evacuation route shoulder west", ground.transform, new Vector3(-8f, 0.015f, 58f), new Vector3(5f, 0.05f, 12f), rubble);
            Block("Evacuation route shoulder east", ground.transform, new Vector3(8f, 0.015f, 58f), new Vector3(5f, 0.05f, 12f), rubble);
        }

        private static void BuildDistricts()
        {
            GameObject district = NewGroup("02 - Abandoned district");

            BuildRuin("Collapsed warehouse", district.transform, new Vector3(27f, 0f, -25f), new Vector3(22f, 10f, 28f), true);
            BuildRuin("West residential block", district.transform, new Vector3(-36f, 0f, -38f), new Vector3(16f, 7f, 21f), false);
            BuildRuin("East residential block", district.transform, new Vector3(37f, 0f, 5f), new Vector3(17f, 8f, 24f), true);
            BuildRuin("North factory shell", district.transform, new Vector3(-32f, 0f, 29f), new Vector3(22f, 9f, 21f), true);
            BuildClinic(district.transform);

            // Low walls and fragments define a bypass through yards instead of one forced firing lane.
            Block("Yard wall west 1", district.transform, new Vector3(-12f, 1.1f, -35f), new Vector3(0.6f, 2.2f, 17f), paleConcrete);
            Block("Yard wall west 2", district.transform, new Vector3(-12f, 1.1f, -8f), new Vector3(0.6f, 2.2f, 11f), concrete);
            Block("Yard wall east broken", district.transform, new Vector3(12f, 0.8f, -28f), new Vector3(0.6f, 1.6f, 24f), concrete);
            Block("Ruined boundary section", district.transform, new Vector3(10f, 1.6f, -4f), new Vector3(11f, 0.7f, 0.6f), rubble, Quaternion.Euler(0f, 0f, -7f));
        }

        private static void BuildRuin(string name, Transform parent, Vector3 centre, Vector3 size, bool damagedRoof)
        {
            GameObject root = NewGroup(name, parent);
            float wallHeight = size.y;
            float wall = 0.65f;
            Block(name + " - floor", root.transform, new Vector3(centre.x, 0.06f, centre.z), new Vector3(size.x, 0.12f, size.z), darkConcrete);
            Block(name + " - north wall", root.transform, new Vector3(centre.x, wallHeight * 0.5f, centre.z + size.z * 0.5f), new Vector3(size.x, wallHeight, wall), concrete);
            Block(name + " - west wall", root.transform, new Vector3(centre.x - size.x * 0.5f, wallHeight * 0.5f, centre.z), new Vector3(wall, wallHeight, size.z), paleConcrete);
            Block(name + " - east wall", root.transform, new Vector3(centre.x + size.x * 0.5f, wallHeight * 0.5f, centre.z), new Vector3(wall, wallHeight * 0.72f, size.z), concrete);
            Block(name + " - broken south wall left", root.transform, new Vector3(centre.x - size.x * 0.31f, wallHeight * 0.42f, centre.z - size.z * 0.5f), new Vector3(size.x * 0.38f, wallHeight * 0.84f, wall), concrete);
            Block(name + " - broken south wall right", root.transform, new Vector3(centre.x + size.x * 0.40f, wallHeight * 0.31f, centre.z - size.z * 0.5f), new Vector3(size.x * 0.18f, wallHeight * 0.62f, wall), rubble);
            if (damagedRoof)
            {
                Block(name + " - surviving roof slab", root.transform, new Vector3(centre.x - size.x * 0.24f, wallHeight + 0.25f, centre.z), new Vector3(size.x * 0.45f, 0.5f, size.z), darkConcrete, Quaternion.Euler(0f, 0f, 2f));
                Block(name + " - fallen roof slab", root.transform, new Vector3(centre.x + size.x * 0.24f, 0.45f, centre.z + 1f), new Vector3(size.x * 0.32f, 0.55f, size.z * 0.42f), rubble, Quaternion.Euler(0f, 0f, -11f));
            }
            Block(name + " - interior partition", root.transform, new Vector3(centre.x - 2f, 1.7f, centre.z + 1f), new Vector3(0.35f, 3.4f, size.z * 0.48f), concrete);
            Block(name + " - broken lintel", root.transform, new Vector3(centre.x + 1f, wallHeight * 0.75f, centre.z - size.z * 0.5f), new Vector3(size.x * 0.33f, 0.6f, wall), rubble, Quaternion.Euler(0f, 0f, 5f));
        }

        private static void BuildClinic(Transform parent)
        {
            GameObject clinic = NewGroup("Field clinic - shelter point", parent);
            float x = -24f, z = -9f, width = 17f, length = 15f, height = 7f, wall = 0.6f;
            Block("Clinic floor", clinic.transform, new Vector3(x, 0.06f, z), new Vector3(width, 0.12f, length), darkConcrete);
            Block("Clinic rear wall", clinic.transform, new Vector3(x, height * 0.5f, z + length * 0.5f), new Vector3(width, height, wall), paleConcrete);
            Block("Clinic west wall", clinic.transform, new Vector3(x - width * 0.5f, height * 0.5f, z), new Vector3(wall, height, length), concrete);
            Block("Clinic east wall north", clinic.transform, new Vector3(x + width * 0.5f, height * 0.5f, z + 5f), new Vector3(wall, height, length * 0.34f), concrete);
            Block("Clinic east wall south", clinic.transform, new Vector3(x + width * 0.5f, height * 0.5f, z - 5f), new Vector3(wall, height, length * 0.34f), concrete);
            Block("Clinic front left", clinic.transform, new Vector3(x - 5.25f, height * 0.5f, z - length * 0.5f), new Vector3(6.5f, height, wall), paleConcrete);
            Block("Clinic front right", clinic.transform, new Vector3(x + 5.25f, height * 0.5f, z - length * 0.5f), new Vector3(6.5f, height, wall), paleConcrete);
            Block("Clinic roof", clinic.transform, new Vector3(x - 2.4f, height + 0.2f, z + 1.5f), new Vector3(width * 0.68f, 0.4f, length * 0.78f), rubble, Quaternion.Euler(0f, 0f, -3f));
            Block("Clinic sign plate", clinic.transform, new Vector3(x, 6.2f, z - length * 0.5f - 0.36f), new Vector3(3.2f, 1.3f, 0.18f), paleConcrete);
            Block("Clinic red cross vertical", clinic.transform, new Vector3(x, 6.2f, z - length * 0.5f - 0.48f), new Vector3(0.35f, 1.0f, 0.06f), clinicRed);
            Block("Clinic red cross horizontal", clinic.transform, new Vector3(x, 6.2f, z - length * 0.5f - 0.49f), new Vector3(1.0f, 0.35f, 0.06f), clinicRed);
            Block("Clinic shelter crates", clinic.transform, new Vector3(x - 2f, 0.75f, z - 1f), new Vector3(1.5f, 1.5f, 1.2f), sandbag);
            Block("Clinic privacy partition", clinic.transform, new Vector3(x + 4f, 1.5f, z + 2f), new Vector3(0.3f, 3f, 5f), paleConcrete);
            AddText("Clinic label", clinic.transform, "PONTO DE ABRIGO", new Vector3(x, 5.1f, z - length * 0.5f - 0.55f), Quaternion.Euler(0f, 0f, 0f), 0.32f, warning.color);
        }

        private static void BuildRailYardAndStation()
        {
            GameObject railYard = NewGroup("03 - Rail yard and station");
            Block("Rail ballast", railYard.transform, new Vector3(0f, 0.12f, 37f), new Vector3(94f, 0.24f, 8f), rubble);
            for (int i = -10; i <= 10; i++)
            {
                float x = i * 4f;
                Block("Rail sleeper " + (i + 10), railYard.transform, new Vector3(x, 0.28f, 37f), new Vector3(0.38f, 0.24f, 6.3f), rust);
            }
            Block("Rail steel west", railYard.transform, new Vector3(0f, 0.48f, 34.5f), new Vector3(92f, 0.18f, 0.18f), metal);
            Block("Rail steel east", railYard.transform, new Vector3(0f, 0.48f, 39.5f), new Vector3(92f, 0.18f, 0.18f), metal);

            GameObject station = NewGroup("Valley railway station", railYard.transform);
            float x = 0f, z = 24f, width = 29f, length = 19f, height = 8f, wall = 0.75f;
            Block("Station floor", station.transform, new Vector3(x, 0.08f, z), new Vector3(width, 0.16f, length), darkConcrete);
            Block("Station rear wall", station.transform, new Vector3(x, height * 0.5f, z + length * 0.5f), new Vector3(width, height, wall), paleConcrete);
            Block("Station west wall", station.transform, new Vector3(x - width * 0.5f, height * 0.5f, z), new Vector3(wall, height, length), concrete);
            Block("Station east wall", station.transform, new Vector3(x + width * 0.5f, height * 0.5f, z), new Vector3(wall, height, length), concrete);
            Block("Station south facade left", station.transform, new Vector3(x - 9f, height * 0.5f, z - length * 0.5f), new Vector3(11f, height, wall), paleConcrete);
            Block("Station south facade right", station.transform, new Vector3(x + 9f, height * 0.5f, z - length * 0.5f), new Vector3(11f, height, wall), paleConcrete);
            Block("Station door lintel", station.transform, new Vector3(x, height - 0.8f, z - length * 0.5f), new Vector3(7f, 1.6f, wall), concrete);
            Block("Station roof slab left", station.transform, new Vector3(x - 8.8f, height + 0.25f, z), new Vector3(11f, 0.5f, length + 1f), darkConcrete, Quaternion.Euler(0f, 0f, -2f));
            Block("Station collapsed roof section", station.transform, new Vector3(x + 7f, 6.7f, z + 2f), new Vector3(12f, 0.45f, 8f), rubble, Quaternion.Euler(0f, 0f, 8f));
            Block("Station ticket counter", station.transform, new Vector3(x - 4f, 1.1f, z - 1f), new Vector3(6f, 1.3f, 1.1f), concrete);
            Block("Station interior rubble", station.transform, new Vector3(x + 5f, 0.55f, z + 4f), new Vector3(5f, 1.1f, 3f), rubble, Quaternion.Euler(0f, 18f, 5f));
            Block("Station platform", railYard.transform, new Vector3(0f, 0.13f, 33f), new Vector3(42f, 0.26f, 3f), paleConcrete);
            Block("Platform edge warning stripe", railYard.transform, new Vector3(0f, 0.275f, 31.42f), new Vector3(42f, 0.025f, 0.12f), warning);
            AddText("Station sign", station.transform, "ESTACAO DO VALE", new Vector3(0f, 7.15f, z - length * 0.5f - 0.46f), Quaternion.identity, 0.48f, roadMarking.color);
            Block("Station hanging sign", station.transform, new Vector3(-10.7f, 5.8f, z - 1f), new Vector3(3.4f, 0.65f, 0.2f), metal);
        }

        private static void BuildBridgeAndRiver()
        {
            GameObject bridge = NewGroup("04 - Evacuation bridge and river cut");
            Block("River channel", bridge.transform, new Vector3(0f, -2.85f, 73f), new Vector3(110f, 0.16f, 24f), water);
            Block("South river retaining wall", bridge.transform, new Vector3(0f, -1.55f, 61.1f), new Vector3(108f, 2.6f, 0.7f), darkConcrete);
            Block("North river retaining wall", bridge.transform, new Vector3(0f, -1.55f, 84.9f), new Vector3(108f, 2.6f, 0.7f), darkConcrete);
            Block("Bridge deck", bridge.transform, new Vector3(0f, 0.18f, 73f), new Vector3(10.5f, 0.42f, 36f), concrete);
            Block("Bridge broken deck patch west", bridge.transform, new Vector3(-4.0f, 0.44f, 67f), new Vector3(2.4f, 0.12f, 7f), rubble, Quaternion.Euler(0f, 0f, -4f));
            Block("Bridge broken deck patch east", bridge.transform, new Vector3(4.2f, 0.40f, 79f), new Vector3(2.1f, 0.12f, 6f), rubble, Quaternion.Euler(0f, 0f, 5f));
            Block("Bridge safety barrier west near", bridge.transform, new Vector3(-5.35f, 0.9f, 62f), new Vector3(0.6f, 1.4f, 14f), paleConcrete);
            Block("Bridge safety barrier west far", bridge.transform, new Vector3(-5.35f, 0.9f, 83f), new Vector3(0.6f, 1.4f, 14f), paleConcrete);
            Block("Bridge safety barrier east broken", bridge.transform, new Vector3(5.35f, 0.75f, 72f), new Vector3(0.5f, 1.1f, 12f), rubble);
            Block("Bridge barrier fallen fragment", bridge.transform, new Vector3(6.3f, 0.45f, 82f), new Vector3(4f, 0.5f, 0.65f), rubble, Quaternion.Euler(0f, 0f, 14f));
            Block("Bridge evacuation stripe", bridge.transform, new Vector3(0f, 0.405f, 73f), new Vector3(0.12f, 0.02f, 30f), roadMarking);
            AddText("Bridge route sign", bridge.transform, "ROTA DE EVACUACAO", new Vector3(-7.2f, 3.4f, 58f), Quaternion.Euler(0f, 68f, 0f), 0.36f, warning.color);
        }

        private static void BuildCombatCoverAndDamage()
        {
            GameObject cover = NewGroup("05 - Cover, wrecks and battle damage");
            BuildVehicle("Burnt transport van", cover.transform, new Vector3(-3.2f, 0f, -42f), Quaternion.Euler(0f, -18f, 0f));
            BuildVehicle("Abandoned utility truck", cover.transform, new Vector3(4.2f, 0f, 7f), Quaternion.Euler(0f, 12f, 0f));

            BuildBarricade("South sandbag position", cover.transform, new Vector3(-8f, 0f, -56f));
            BuildBarricade("Avenue roadblock", cover.transform, new Vector3(8f, 0f, -29f));
            BuildBarricade("Station approach cover", cover.transform, new Vector3(-8f, 0f, 16f));
            BuildBarricade("Clinic yard cover", cover.transform, new Vector3(-11f, 0f, -2f));

            Block("Concrete jersey barrier A", cover.transform, new Vector3(6.2f, 0.55f, -10f), new Vector3(3.8f, 1.1f, 0.8f), paleConcrete, Quaternion.Euler(0f, 8f, 0f));
            Block("Concrete jersey barrier B", cover.transform, new Vector3(-7.2f, 0.55f, -45f), new Vector3(4.2f, 1.1f, 0.8f), concrete, Quaternion.Euler(0f, -7f, 0f));
            Block("Rail yard crate stack low", cover.transform, new Vector3(11f, 0.8f, 49f), new Vector3(2.1f, 1.6f, 1.8f), rust);
            Block("Rail yard crate stack high", cover.transform, new Vector3(11.2f, 2.45f, 49f), new Vector3(1.7f, 1.6f, 1.7f), metal, Quaternion.Euler(0f, 8f, 5f));

            ScatterRubble(cover.transform, new Vector3(-12f, 0f, -47f), 7);
            ScatterRubble(cover.transform, new Vector3(13f, 0f, -58f), 6);
            ScatterRubble(cover.transform, new Vector3(17f, 0f, 20f), 7);
            ScatterRubble(cover.transform, new Vector3(-17f, 0f, 22f), 8);
            ScatterRubble(cover.transform, new Vector3(3f, 0f, 55f), 5);

            // Simple utility poles silhouette the avenue and provide scale; no wires block the route.
            for (int i = 0; i < 4; i++)
            {
                float z = -53f + i * 27f;
                Block("Utility pole " + i, cover.transform, new Vector3(15f, 5f, z), new Vector3(0.35f, 10f, 0.35f), rust);
                Block("Utility pole crossbar " + i, cover.transform, new Vector3(15f, 9.5f, z), new Vector3(3.1f, 0.25f, 0.25f), metal);
            }
        }

        private static void BuildVehicle(string name, Transform parent, Vector3 position, Quaternion rotation)
        {
            GameObject vehicle = NewGroup(name, parent);
            Vector3 p = position;
            Block(name + " chassis", vehicle.transform, p + new Vector3(0f, 0.75f, 0f), new Vector3(2.25f, 0.9f, 4.8f), rust, rotation);
            Block(name + " cabin", vehicle.transform, p + new Vector3(0f, 1.55f, 0.2f), new Vector3(1.95f, 0.8f, 2.2f), darkConcrete, rotation);
            Block(name + " damaged bonnet", vehicle.transform, p + new Vector3(0f, 1.25f, -1.7f), new Vector3(2.1f, 0.22f, 1.1f), metal, rotation * Quaternion.Euler(0f, 0f, 4f));
            for (int side = -1; side <= 1; side += 2)
            {
                for (int axle = -1; axle <= 1; axle += 2)
                {
                    GameObject wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    wheel.name = name + " wheel";
                    wheel.transform.SetParent(vehicle.transform, false);
                    wheel.transform.SetPositionAndRotation(p + rotation * new Vector3(side * 1.12f, 0.43f, axle * 1.45f), rotation * Quaternion.Euler(0f, 0f, 90f));
                    wheel.transform.localScale = new Vector3(0.66f, 0.18f, 0.66f);
                    wheel.GetComponent<Renderer>().sharedMaterial = darkConcrete;
                }
            }
            Block(name + " broken window panel", vehicle.transform, p + new Vector3(0f, 1.6f, 0.2f), new Vector3(2f, 0.08f, 0.08f), glass, rotation * Quaternion.Euler(0f, 11f, 0f));
        }

        private static void BuildBarricade(string name, Transform parent, Vector3 centre)
        {
            for (int i = -2; i <= 2; i++)
            {
                Vector3 p = centre + new Vector3(i * 1.15f, 0.55f, 0f);
                Block(name + " lower sandbag " + i, parent, p, new Vector3(1.25f, 0.75f, 0.8f), sandbag, Quaternion.Euler(0f, i * 2f, 0f));
                if (i >= -1 && i <= 1)
                    Block(name + " upper sandbag " + i, parent, p + new Vector3(0f, 0.68f, 0f), new Vector3(1.1f, 0.62f, 0.72f), sandbag, Quaternion.Euler(0f, -i * 3f, 0f));
            }
            Block(name + " warning board", parent, centre + new Vector3(0f, 1.9f, 0.5f), new Vector3(3.6f, 0.16f, 0.12f), warning);
        }

        private static void ScatterRubble(Transform parent, Vector3 centre, int count)
        {
            for (int i = 0; i < count; i++)
            {
                float x = centre.x + ((i * 7) % 9 - 4) * 0.72f;
                float z = centre.z + ((i * 5) % 7 - 3) * 0.68f;
                float sx = 0.45f + (i % 3) * 0.28f;
                float sy = 0.22f + (i % 2) * 0.18f;
                float sz = 0.4f + ((i + 1) % 3) * 0.25f;
                Block("Rubble fragment " + i, parent, new Vector3(x, sy * 0.5f, z), new Vector3(sx, sy, sz), i % 2 == 0 ? rubble : paleConcrete, Quaternion.Euler(i * 7f, i * 19f, i % 2 == 0 ? 4f : -6f));
            }
        }

        private static void BuildTransmitter()
        {
            GameObject tower = NewGroup("06 - Emergency transmitter objective");
            Vector3 basePoint = new Vector3(10f, 0f, 28f);
            Block("Transmitter base", tower.transform, basePoint + new Vector3(0f, 0.2f, 0f), new Vector3(4f, 0.4f, 4f), darkConcrete);
            for (int x = -1; x <= 1; x += 2)
            {
                for (int z = -1; z <= 1; z += 2)
                {
                    Block("Antenna mast leg", tower.transform, basePoint + new Vector3(x * 1.3f, 6f, z * 1.3f), new Vector3(0.18f, 12f, 0.18f), metal);
                }
            }
            for (int y = 2; y <= 10; y += 2)
            {
                Block("Antenna cross brace", tower.transform, basePoint + new Vector3(0f, y, 0f), new Vector3(3f, 0.14f, 0.14f), rust);
                Block("Antenna depth brace", tower.transform, basePoint + new Vector3(0f, y, 0f), new Vector3(0.14f, 0.14f, 3f), rust);
            }
            Block("Antenna top mast", tower.transform, basePoint + new Vector3(0f, 13.2f, 0f), new Vector3(0.12f, 2.4f, 0.12f), metal);
            Block("Transmitter crossbar", tower.transform, basePoint + new Vector3(0f, 8f, 0f), new Vector3(5f, 0.16f, 0.16f), metal);
            GameObject lamp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            lamp.name = "Transmitter red beacon";
            lamp.transform.SetParent(tower.transform, false);
            lamp.transform.position = basePoint + new Vector3(0f, 14.7f, 0f);
            lamp.transform.localScale = Vector3.one * 0.55f;
            lamp.GetComponent<Renderer>().sharedMaterial = beacon;
            Light redLight = lamp.AddComponent<Light>();
            redLight.type = LightType.Point;
            redLight.color = beacon.color;
            redLight.intensity = 3.5f;
            redLight.range = 10f;
            AddText("Transmitter objective label", tower.transform, "OBJETIVO: ATIVAR O TRANSMISSOR", basePoint + new Vector3(0f, 3.8f, -2.4f), Quaternion.identity, 0.31f, roadMarking.color);
            Block("Objective equipment crate", tower.transform, basePoint + new Vector3(-3.2f, 0.7f, -2.7f), new Vector3(1.8f, 1.4f, 1.6f), rust);
        }

        private static void PlacePlayer(GameObject prefab)
        {
            GameObject player = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (player == null) player = Object.Instantiate(prefab);
            player.name = "Player - Starter Assets FPS";
            player.transform.position = new Vector3(0f, 0.1f, -82f);
            player.transform.rotation = Quaternion.identity;

            Transform cameraRoot = player.transform.Find("PlayerCameraRoot");
            if (cameraRoot == null)
            {
                Debug.LogError("Starter Assets PlayerCapsule has no PlayerCameraRoot; camera setup was skipped.");
                return;
            }

            Transform capsuleVisual = player.transform.Find("Capsule");
            if (capsuleVisual != null)
            {
                Renderer visualRenderer = capsuleVisual.GetComponent<Renderer>();
                if (visualRenderer != null) visualRenderer.enabled = false;
            }

            GameObject cameraObject = new GameObject("MainCamera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(cameraRoot, false);
            cameraObject.transform.localPosition = Vector3.zero;
            cameraObject.transform.localRotation = Quaternion.identity;
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 76f;
            camera.nearClipPlane = 0.08f;
            camera.farClipPlane = 300f;
            camera.allowHDR = true;
            cameraObject.AddComponent<AudioListener>();
        }

        private static void AddText(string name, Transform parent, string text, Vector3 position, Quaternion rotation, float size, Color color)
        {
            GameObject label = new GameObject(name);
            label.transform.SetParent(parent, false);
            label.transform.SetPositionAndRotation(position, rotation);
            TextMesh textMesh = label.AddComponent<TextMesh>();
            textMesh.text = text;
            textMesh.characterSize = size;
            textMesh.fontSize = 64;
            textMesh.fontStyle = FontStyle.Bold;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.color = color;
        }

        private static GameObject Block(string name, Transform parent, Vector3 position, Vector3 scale, Material material, Quaternion? rotation = null)
        {
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = name;
            block.transform.SetParent(parent, false);
            block.transform.SetPositionAndRotation(position, rotation ?? Quaternion.identity);
            block.transform.localScale = scale;
            Renderer renderer = block.GetComponent<Renderer>();
            if (renderer != null && material != null) renderer.sharedMaterial = material;
            return block;
        }

        private static GameObject NewGroup(string name, Transform parent = null)
        {
            GameObject group = new GameObject(name);
            if (parent != null) group.transform.SetParent(parent, false);
            return group;
        }
    }
}
