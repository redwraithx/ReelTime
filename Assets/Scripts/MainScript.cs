using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainScript : MonoBehaviour, PlayerScript.IPlayerListener
{
    public GameObject playerPrefab;
    public GameObject fish1Prefab;
    public GameObject fish2Prefab;
    public GameObject fish3Prefab;
    public GameObject linePowerupPrefab;
    public GameObject bagPrefab;
    public GameObject tirePrefab;
    public GameObject wastePrefab;
    public GameObject rewindPrefab;

    public float levelLength;
    public float spawnRateStart;
    public float spawnRateMultiplier;
    public float spawnRateMin;
    public float fishSpeedStart;
    public float fishSpeedMultiplier;
    public float fishSpeedMax;
    public float healthTickStart;
    public float healthTickMultiplier;
    public float healthTickMax;
    public float rewindTick;
    public float maxRewind;
    public float maxHealth;

    private GameObject player;
    private PlayerScript playerScript;
    private Dictionary<int, GameObject> fishMap = new Dictionary<int, GameObject>();

    [SerializeField] private Image menuBacking;
    [SerializeField] private Button playButton;
    [SerializeField] private Image logoImage;
    [SerializeField] private Image tutorialImage;
    [SerializeField] private TMPro.TextMeshProUGUI levelText;

    [SerializeField] Camera camera;

    private float rewind;
    [SerializeField] private HealthBar rewindBar;

    private int score;
    [SerializeField] private TMPro.TextMeshProUGUI scoreText;

    private float health;
    [SerializeField] private HealthBar healthBar;

    private int level = 1;
    private float levelTimer = 0;
    private float spawnTimer = 0;
    private float spawnRate = 0;
    private float fishSpeed = 0;
    private float healthTick = 0;
    private bool isGameActive = false;
    private bool isRewindActive = false;

    // Background:
    // light blue - #7CBDF9 = rgb(124,189,249)
    // dark blue - #314D77 = rgb(49,77,119)
    // transition is -75, -112, -130 over 30 levels
    float rChange = 2.5f;
    float gChange = 3.733333333333333f;
    float bChange = 3.966666666666667f;

    private float bgRstart = 124;
    private float bgGstart = 189;
    private float bgBstart = 249;
    private float bgRvalue = 0;
    private float bgGvalue = 0;
    private float bgBvalue = 0;

    private void ResetState()
    {
        level = 1;
        levelText.text = "Level " + level;

        levelTimer = 0;
        spawnTimer = 0;
        spawnRate = spawnRateStart;
        fishSpeed = fishSpeedStart;
        healthTick = healthTickStart;

        rewind = 0;
        rewindBar.SetHealth(rewind);

        score = 0;
        scoreText.text = score.ToString();

        health = maxHealth;
        healthBar.SetHealth(health);

        bgRvalue = bgRstart;
        bgGvalue = bgGstart;
        bgBvalue = bgBstart;

        Color color = new Color(bgRvalue / 255f, bgGvalue / 255f, bgBvalue / 255f);
        //GameObject.Find("MainCamera").GetComponent<Camera>().backgroundColor = color;
        camera.backgroundColor = color;
    }

    private void setupColliders()
    {
        // Setup camera colliders
        float colThickness = 1f;
        float zPosition = 0f;
        Vector2 screenSize;

        Dictionary<string, Transform> colliders = new Dictionary<string, Transform>();
        colliders.Add("Top", new GameObject().transform);
        colliders.Add("Bottom", new GameObject().transform);
        colliders.Add("Right", new GameObject().transform);
        colliders.Add("Left", new GameObject().transform);

        //Generate world space point information for position and scale calculations
        Vector3 cameraPos = Camera.main.transform.position;
        screenSize.x = Vector2.Distance(Camera.main.ScreenToWorldPoint(new Vector2(0, 0)), Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, 0))) * 0.5f;
        //Grab the world-space position values of the start and end positions of the screen, then calculate the distance between them and store it as half, since we only need half that value for distance away from the camera to the edge
        screenSize.y = Vector2.Distance(Camera.main.ScreenToWorldPoint(new Vector2(0, 0)), Camera.main.ScreenToWorldPoint(new Vector2(0, Screen.height))) * 0.5f;
        //For each Transform/Object in our Dictionary
        foreach (KeyValuePair<string, Transform> valPair in colliders)
        {
            valPair.Value.gameObject.AddComponent<BoxCollider2D>(); //Add our colliders. Remove the "2D", if you would like 3D colliders.
            valPair.Value.name = valPair.Key + "Collider"; //Set the object's name to it's "Key" name, and take on "Collider".  i.e: TopCollider
            valPair.Value.parent = transform; //Make the object a child of whatever object this script is on (preferably the camera)

            if (valPair.Key == "Left" || valPair.Key == "Right") //Scale the object to the width and height of the screen, using the world-space values calculated earlier
                valPair.Value.localScale = new Vector3(colThickness, screenSize.y * 2, colThickness);
            else
                valPair.Value.localScale = new Vector3(screenSize.x * 2, colThickness, colThickness);
        }
        //Change positions to align perfectly with outter-edge of screen, adding the world-space values of the screen we generated earlier, and adding/subtracting them with the current camera position, as well as add/subtracting half out objects size so it's not just half way off-screen
        colliders["Right"].position = new Vector3(cameraPos.x + screenSize.x + (colliders["Right"].localScale.x * 0.5f), cameraPos.y, zPosition);
        colliders["Left"].position = new Vector3(cameraPos.x - screenSize.x - (colliders["Left"].localScale.x * 0.5f), cameraPos.y, zPosition);
        colliders["Top"].position = new Vector3(cameraPos.x, cameraPos.y + screenSize.y + (colliders["Top"].localScale.y * 0.5f), zPosition);
        colliders["Bottom"].position = new Vector3(cameraPos.x, cameraPos.y - screenSize.y - (colliders["Bottom"].localScale.y * 0.5f), zPosition);

        colliders["Right"].tag = "Edge";
        colliders["Left"].tag = "Edge";
        colliders["Top"].tag = "Edge";
        colliders["Bottom"].tag = "Edge";
    }

    void Start()
    {
        // Setup level text
        //levelText = GameObject.Find("LevelText").GetComponent<TMPro.TextMeshProUGUI>();

        // Setup play button
        //playButton = GameObject.Find("PlayButton").GetComponent<Button>();
        playButton.onClick.AddListener(OnPlayClicked);

        // Setup logo & tutorial
        //logoImage = GameObject.Find("LogoImage").GetComponent<Image>();
        //tutorialImage = GameObject.Find("Tutorial").GetComponent<Image>();

        // Setup rewind bar
        //rewindBar = GameObject.Find("RewindBar").GetComponent<HealthBar>();
        rewindBar.SetHealth(0);

        // Setup score text
        //scoreText = GameObject.Find("ScoreText").GetComponent<TMPro.TextMeshProUGUI>();

        // Setup health bar
        //healthBar = GameObject.Find("HealthBar").GetComponent<HealthBar>();
        health = maxHealth;
        healthBar.SetHealth(health);

        // Add player
        player = Instantiate(playerPrefab, new Vector3(-1.8f, 3, 0), Quaternion.identity);
        player.SetActive(false);
        playerScript = player.GetComponent<PlayerScript>();
        playerScript.listener = this;
    }

    void Update()
    {
        if (!isGameActive) { return; }

        if (health <= 0)
        {
            GameOver();
            return;
        }

        levelTimer += Time.deltaTime;
        if (levelTimer >= levelLength)
        {
            levelTimer = 0;

            score += level;
            scoreText.text = score.ToString();

            level++;
            levelText.text = "Level " + level;

            bgRvalue -= rChange;
            bgGvalue -= gChange;
            bgBvalue -= bChange;

            //Debug.Log(bgRvalue);

            Color color = new Color(bgRvalue / 255f, bgGvalue / 255f, bgBvalue / 255f);
            //GameObject.Find("MainCamera").GetComponent<Camera>().backgroundColor = color;
            camera.backgroundColor = color;

            spawnRate *= spawnRateMultiplier;
            spawnRate = spawnRate < spawnRateMin ? spawnRateMin : spawnRate;

            fishSpeed *= fishSpeedMultiplier;
            fishSpeed = fishSpeed > fishSpeedMax ? fishSpeedMax : fishSpeed;

            healthTick *= healthTickMultiplier;
            healthTick = healthTick > healthTickMax ? healthTickMax : healthTick;

            //Debug.Log("level: " + level);
            //Debug.Log("spawn rate: " + spawnRate);
            //Debug.Log("fish speed: " + fishSpeed);
        }

        rewind = rewind > maxRewind ? maxRewind : rewind;
        rewindBar.SetHealth(rewind);

        // Spawn a fish per spawn rate - pause during rewind
        if (isRewindActive)
        {
            spawnTimer -= Time.deltaTime;
            health += Time.deltaTime * healthTick;
        }
        else
        {
            spawnTimer += Time.deltaTime;
            health -= Time.deltaTime * healthTick;
        }

        health = health > maxHealth ? maxHealth : health;
        healthBar.SetHealth(health);

        if (spawnTimer >= spawnRate)
        {
            SpawnFish();
            spawnTimer = 0;
        }

        if (rewind > 0 && (Input.GetKeyDown(KeyCode.Mouse1) || Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)))
        {
            isRewindActive = true;
        }

        if (rewind <=0 || Input.GetKeyUp(KeyCode.Mouse1) || Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift) || Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.A))
        {
            isRewindActive = false;
        }

        if (isRewindActive)
        {
            rewind -= Time.deltaTime * rewindTick;
            rewind = rewind <= 0 ? 0 : rewind;
            rewindBar.SetHealth(rewind);
        }

        float fishDirection = isRewindActive ? 100f : -100f;
        float fishStep = fishSpeed * Time.deltaTime;

        List<int> destroyFishList = new List<int>();
        foreach (KeyValuePair<int, GameObject> entry in fishMap)
        {
            GameObject fish = entry.Value;
            //Debug.Log(fish.tag);

            switch (fish.tag)
            {
                case "Fish1":
                    fishStep = 1.5f * fishSpeed * Time.deltaTime;
                    break;
                case "Fish2":
                    fishStep = 1.0f * fishSpeed * Time.deltaTime;
                    break;
                case "Fish3":
                    fishStep = 0.8f * fishSpeed * Time.deltaTime;
                    break;
                case "LinePowerup":
                    fishStep = 1.2f * fishSpeed * Time.deltaTime;
                    break;
                case "Bag":
                    fishStep = 1.0f * fishSpeed * Time.deltaTime;
                    break;
                case "Tire":
                    fishStep = 1.2f * fishSpeed * Time.deltaTime;
                    break;
                case "Waste":
                    fishStep = 0.5f * fishSpeed * Time.deltaTime;
                    break;
                case "Rewind":
                    fishStep = 1.2f * fishSpeed * Time.deltaTime;
                    break;
            }

            //Debug.Log(fish.transform.position.x);
            
            if (fish.transform.position.x < -30f)
            {
                destroyFishList.Add(entry.Key);
                Destroy(fish);
            } else
            {
                Vector2 target = new Vector2(fishDirection, fish.transform.position.y);
                fish.transform.position = Vector3.MoveTowards(fish.transform.position, target, fishStep);
            }
        }

        
        foreach (int key in destroyFishList)
        {
            //Debug.Log("Destroying fish: " + key);
            fishMap.Remove(key);
        }

        //Debug.Log("Active fish: " + fishMap.Keys.Count);
    }

    // Wait 2 seconds after dieing to enable play
    IEnumerator PlayDelayCoroutine()
    {
        yield return new WaitForSeconds(2);
        menuBacking.gameObject.SetActive(true);
        playButton.gameObject.SetActive(true);
        logoImage.gameObject.SetActive(true);
        tutorialImage.gameObject.SetActive(true);
    }

    private void GameOver()
    {
        healthBar.SetHealth(0);
        playerScript.PlayerDead();
        SoundManager.PlaySound(SoundManager.Sound.Game_Over);
        isGameActive = false;
        StartCoroutine(PlayDelayCoroutine());
    }

    private void OnPlayClicked()
    {
        menuBacking.gameObject.SetActive(false);
        playButton.gameObject.SetActive(false);
        logoImage.gameObject.SetActive(false);
        tutorialImage.gameObject.SetActive(false);

        DestroyEverything();
        ResetState();
        playerScript.ResetPlayer();
        setupColliders();
        SpawnFish();
        SoundManager.PlaySound(SoundManager.Sound.Reel);

        isGameActive = true;
    }

    private void SpawnFish()
    {
        // Assuming height range -4.5 to 4.5
        float randomY = Random.Range(0.0f, 9.0f) - 4.5f;

        int fish1range = 15;
        int fish2range = 30 + fish1range;
        int fish3range = 50 + fish2range;
        int linerange = 15 + fish3range;
        int bagrange = 80 + linerange;
        int tirerange = 50 + bagrange;
        int wasterange = 25 + tirerange;
        int rewindrange = 15 + wasterange;
        int totalrange = rewindrange + 1;
        //Debug.Log(totalrange);

        int spawnType = Random.Range(1, totalrange);
        GameObject fishType = null;
        if (spawnType <= fish1range)
        {
            fishType = fish1Prefab;
        } else if (spawnType <= fish2range)
        {
            fishType = fish2Prefab;
        }
        else if (spawnType <= fish3range)
        {
            fishType = fish3Prefab;
        }
        else if (spawnType <= linerange)
        {
            fishType = linePowerupPrefab;
        }
        else if (spawnType <= bagrange)
        {
            fishType = bagPrefab;
        }
        else if (spawnType <= tirerange)
        {
            fishType = tirePrefab;
        }
        else if (spawnType <= wasterange)
        {
            fishType = wastePrefab;
        }
        else if (spawnType <= rewindrange)
        {
            fishType = rewindPrefab;
        }

        //Debug.Log(spawnType);

        GameObject newFish = Instantiate(fishType, new Vector3(10, randomY, 0), Quaternion.identity);
        fishMap.Add(newFish.GetInstanceID(), newFish);
    }

    private void DestroyEverything()
    {
        foreach (KeyValuePair<int, GameObject> entry in fishMap)
        {
            Destroy(entry.Value);
        }
        fishMap.Clear();
    }

    // Called when the player collides with something and returns the object collided with
    public void OnCollision(GameObject obj)
    {
        //Debug.Log("Collision with " + obj.GetInstanceID().ToString());
        //Debug.Log("Collision with " + obj.tag);

        switch (obj.tag)
        {
            case "Fish1":
                score += 5;
                health += 10;
                SoundManager.PlaySound(SoundManager.Sound.Fish_Catch);
                break;
            case "Fish2":
                score += 2;
                health += 20;
                SoundManager.PlaySound(SoundManager.Sound.Fish_Catch);
                break;
            case "Fish3":
                score += 1;
                health += 30;
                SoundManager.PlaySound(SoundManager.Sound.Fish_Catch);
                break;
            case "LinePowerup":
                health = 100;
                SoundManager.PlaySound(SoundManager.Sound.Got_Line);
                break;
            case "Bag":
                health -= 10;
                SoundManager.PlaySound(SoundManager.Sound.Got_Bad_Item);
                break;
            case "Tire":
                health -= 20;
                SoundManager.PlaySound(SoundManager.Sound.Got_Bad_Item);
                break;
            case "Waste":
                health -= 40;
                SoundManager.PlaySound(SoundManager.Sound.Got_Bad_Item);
                break;
            case "Rewind":
                rewind += 25;
                SoundManager.PlaySound(SoundManager.Sound.Got_Reel);
                break;
            case "Edge":
                GameOver();
                break;
        }
        
        scoreText.text = score.ToString();

        Destroy(obj);
        fishMap.Remove(obj.GetInstanceID());
    }
}
