using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DefaultNamespace;
using DG.Tweening;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

public class GameManager : MonoBehaviour
{

    [SerializeField] private CameraMover cameraMover;
    [SerializeField] private HealthShieldBar healthShieldBar;
    [SerializeField] private EndgamePanel endgamePanel;
    [SerializeField] private CanvasGroup gamePanel, loadingPanel, mainMenuPanel;
    [SerializeField] private Image mainMenuFader;
    [SerializeField, Header("Version")] private TextMeshProUGUI versionText;
    [SerializeField, TextArea(2, 2)] private string versionDescription;
    [SerializeField, Header("Game")] private TextAsset wordList;
    [SerializeField] private PlayerConfig playerConfig;
    [SerializeField] private TextMeshProUGUI lettersContainer, typedLettersContainer, energyBaseText, energyBonusText, maxEnergyText, currentEnergyText, scoreText, stageAnnouncerText;
    [SerializeField] private Slider energyBar, gameProgressBar;
    [SerializeField, Header("Main Menu")] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI progressBarText;
    [SerializeField] private GameObject howToPlayPanel;

    [SerializeField, Header("Planet")] private Transform earthTransform;
    [SerializeField] private Transform moonTransform;

    [SerializeField, Header("Skills")] private Skill[] skills;
    [SerializeField] private GameObject skillImagePrefab;
    [SerializeField] private Transform skillImagesParent;
    
    [SerializeField, Header("Player")] private Transform turretBase;
    [SerializeField] private GameObject bulletPrefab, explosionPrefab;
    [SerializeField] private ParticleSystem muzzleFlash;

    [SerializeField, Header("Enemy")] private GameObject[] enemies;
    [SerializeField] private float[] spawnTimes;
    [SerializeField] private int maxEnemies = 1;

    private static GameManager instance;
    
    List<string> words = new List<string>();
    Dictionary<KeyCode, int> frequencyDictionary = new Dictionary<KeyCode, int>();

    private KeyCode[] registeredKeycode = new[]
    {
        KeyCode.Backspace, KeyCode.Return, KeyCode.KeypadEnter, KeyCode.A, KeyCode.B, KeyCode.C, KeyCode.D, KeyCode.E, KeyCode.F, KeyCode.G, KeyCode.H, KeyCode.I, KeyCode.J,
        KeyCode.K, KeyCode.L, KeyCode.M, KeyCode.N, KeyCode.O, KeyCode.P, KeyCode.Q, KeyCode.R, KeyCode.S, KeyCode.T,
        KeyCode.U, KeyCode.V, KeyCode.W, KeyCode.X, KeyCode.Y, KeyCode.Z
    };

    private Dictionary<char, int> letterValues = new Dictionary<char, int>()
    {
        {'A', 1}, {'B', 3}, {'C', 3}, {'D', 2}, {'E', 1}, {'F', 4}, {'G', 2}, {'H', 4}, {'I', 1}, {'J', 8}, {'K', 5},
        {'L', 1}, {'M', 3}, {'N', 1}, {'O', 1}, {'P', 3}, {'Q', 10}, {'R', 1}, {'S', 1}, {'T', 1}, {'U', 1}, {'V', 4},
        {'W', 4}, {'X', 8}, {'Y', 4}, {'Z', 10}
    };
    
    private int processedWords = 0, baseE, bonusE, enemyCount;
    private bool wordMatch, gameStart, shieldActive = true;
    private float gameTime, shieldTime;
    private int gameProgress = 0;
    private Image gameProgressBarImage;
    
    private int CurrentEnergy { get; set; }
    private int MaximumEnergy { get; set; } = 400;
    public float CurrentHealth { get; private set; } = 640;
    public float MaxHealth { get; private set; } = 640;
    public float CurrentShield { get; private set; } = 320;
    public float MaxShield { get; private set; } = 320;
    public float Score { get; private set; }
    private GameConfig CurrentProgress
    {
        get => playerConfig.gameProgressions[gameProgress];
    }

    private List<(string letter, bool selected)> currentLetters = new List<(string letter, bool selected)>();
    private StringBuilder typedLetters = new StringBuilder();

    private Transform shipTransform;
    private Camera cam;

    public bool showHowToPlay = true;
    
    // Start is called before the first frame update
    void Start()
    {
        if (instance == null) instance = this;
        else Destroy(this);

        versionText.text = $"Version {Application.version}\n{versionDescription}";

        shipTransform = GameObject.FindGameObjectWithTag("Player").transform;
        gameProgressBarImage = gameProgressBar.GetComponentInChildren<Image>();
        words = wordList.text.Split('\n').ToList();
        cam = Camera.main;
        Debug.Log($"Loaded {words.Count} words");
        
        EndgamePanel.ResetGame += EndgamePanelOnResetGame;

        for (int i = 0; i < skills.Length; i++)
        {
            GameObject go = Instantiate(skillImagePrefab, skillImagesParent);
            go.GetComponent<Image>().sprite = skills[i].sprite;
            go.GetComponentInChildren<TextMeshProUGUI>().text = skills[i].energyCost.ToString("N0");
        }
        
        InitGame();
        //Process words
        StartCoroutine(ProcessWords());
    }

    private void InitGame()
    {
        cameraMover.speed = 0.001f;
        MaximumEnergy = playerConfig.maximumEnergy;
        MaxHealth = playerConfig.startingHealth;
        MaxShield = playerConfig.startingShield;
        CurrentHealth = MaxHealth;
        CurrentShield = MaxShield;
        CurrentEnergy = playerConfig.startingEnergy;

        gameProgressBar.value = 0;
        gameProgressBar.maxValue = 100;
        energyBar.value = (float) CurrentEnergy / MaximumEnergy;
        maxEnergyText.text = MaximumEnergy.ToString("n0");
        currentEnergyText.text = CurrentEnergy.ToString("n0");
        gamePanel.alpha = 0;
        endgamePanel.gameObject.SetActive(false);
    }

    private void EndgamePanelOnResetGame()
    {
        showHowToPlay = false;
        cam.transform.position = Vector3.zero;
        currentLetters.Clear();
        earthTransform.position = new Vector3(2000, 0, 1);
        moonTransform.position = new Vector3(2000, 0, 1);
        gameProgress = 0;
        gameTime = 0;
        shieldTime = 0;
        shieldActive = true;
        InitGame();
        shipTransform.gameObject.SetActive(true);
        mainMenuFader.DOFade(0f, 2f).onComplete = () =>
        {
            mainMenuFader.gameObject.SetActive(false);
        };
    }

    private IEnumerator ProcessWords()
    {
        for (; processedWords < words.Count; processedWords++)
        {
            foreach (var letter in words[processedWords])
            {
                KeyCode key = LetterToKey(letter.ToString());
                if (key != KeyCode.Escape)
                {
                    if (frequencyDictionary.ContainsKey(key))
                        frequencyDictionary[key]++;
                    else frequencyDictionary.Add(key, 1);
                }
            }

            //yield return null;
            if (processedWords > 0 && processedWords % 200 == 0) yield return null;
        }
        
        progressBarText.text = $"Loading Complete";
        loadingPanel.DOFade(0f, 2f);
        mainMenuFader.DOColor(new Color(0, 0, 0, 0), 2f).onComplete = () =>
        {
            mainMenuFader.gameObject.SetActive(false);
        };
    }

    public void StartGame()
    {
        
        mainMenuPanel.DOFade(0f, 1f).onComplete = () =>
        {
            mainMenuPanel.gameObject.SetActive(false);
            if(showHowToPlay) howToPlayPanel.SetActive(true);
            else SetupGame();
        };
    }

    private void SetupGame()
    {
        Vector3 camT = Camera.main.transform.position;
        earthTransform.position = new Vector3(camT.x + 30, 0, 1);
        moonTransform.position = new Vector3(camT.x + 100, 0, 1);
        earthTransform.DOMove(new Vector3(camT.x - 10, 0, 1), 0.6f).onComplete = () =>
        {
            gamePanel.DOFade(1f, 0.75f);
            DOTween.To(() => cameraMover.speed, (x) => cameraMover.speed = x, 0.5f, 0.75f);
            gameStart = true;
            AnimateStageAnnouncer();
        };
            
        healthShieldBar.SetCurrentHealth(MaxHealth);
        healthShieldBar.SetCurrentShield(MaxShield);
    }

    public void ExitGame()
    {
        #if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
        #else
        Application.Quit();
        #endif
        
    }

    // Update is called once per frame
    void Update()
    {
        if (processedWords < words.Count)
        {
            progressBar.maxValue = words.Count;
            progressBar.value = processedWords;
            progressBarText.text = $"Loading... {processedWords/100:N0}/{words.Count/100:N0}";
        }
        else if (gameStart)
        {

            //Begin game loop proper
            if (currentLetters.Count == 0)
            {
                currentLetters = GetLetters();
                lettersContainer.text = CurrentLetterToString(currentLetters);
                typedLetters.Clear();
                typedLettersContainer.text = "";
                energyBaseText.text = "";
                energyBonusText.text = "";
            }

            //Game Progress
            gameProgressBar.value = cam.transform.position.x;
            if (gameProgressBar.value >= gameProgressBar.maxValue)
            {
                //Victory for now
                gameStart = false;
                foreach (var enemy in FindObjectsOfType<Enemy>())
                {
                    AnimateScore(Score + enemy.ScoreReward / 4);
                    Instantiate(explosionPrefab, enemy.transform.position, Quaternion.identity);
                    Destroy(enemy.gameObject);
                }

                StartCoroutine(GameOver(true));
                return;
            }
            //Process Game Progression
            if (gameProgress < playerConfig.gameProgressions.Length-1)
            {
                if (gameProgressBar.value / gameProgressBar.maxValue >=
                    playerConfig.gameProgressions[gameProgress + 1].progressThreshold)
                {
                    gameProgress++;
                    gameProgressBarImage.DOColor(CurrentProgress.barColor, 0.5f);
                    AnimateStageAnnouncer();
                }
            }
            
            gameTime += Time.deltaTime;
            //Spawn enemies
            if (gameTime >= spawnTimes[Mathf.Min(enemyCount, spawnTimes.Length - 1)])
            {
                GameObject enemyObject = enemies[UnityEngine.Random.Range(0, enemies.Length)];
                Instantiate(enemyObject, new Vector3(cam.transform.position.x + 20, UnityEngine.Random.Range(-10f, 10f), 1),
                    enemyObject.transform.rotation);
                enemyCount++;
                gameTime = 0;
            }

            //Shield regen
            if (shieldTime >= playerConfig.shieldRegenDelay)
            {
                CurrentShield = Mathf.Min(CurrentShield + Time.deltaTime * playerConfig.shieldRegenRate, MaxShield);
                healthShieldBar.SetCurrentShield(CurrentShield);

                if (!shieldActive && CurrentShield >= MaxShield * playerConfig.shieldActivateThreshold)
                {
                    shieldActive = true;
                    healthShieldBar.ActivateShield();
                }
            }
            else shieldTime += Time.deltaTime;

            //Inputs
            if (Input.GetKeyDown(KeyCode.Tab) && CurrentEnergy >= 10)
            {
                currentLetters.Clear();
                AnimateEnergy(CurrentEnergy - 10);
            }
            for (int i = 0; i < registeredKeycode.Length; i++)
            {
                if (Input.GetKeyDown(registeredKeycode[i]))
                {
                    //Enter
                    if (registeredKeycode[i] == KeyCode.KeypadEnter || registeredKeycode[i] == KeyCode.Return)
                    {
                        if (wordMatch)
                        {
                            AnimateEnergy(CurrentEnergy + baseE + bonusE);
                            AnimateScore(Score + baseE * 20 + bonusE * 50);
                            typedLetters.Clear();
                            currentLetters.Clear();
                            wordMatch = false;
                        }
                    }
                    else
                    {
                        //Backspace
                        if (registeredKeycode[i] == KeyCode.Backspace)
                        {
                            if (typedLettersContainer.text.Length > 0)
                            {
                                string letter = typedLetters[typedLetters.Length - 1].ToString();
                                currentLetters[currentLetters.IndexOf((letter, true))] = (letter, false);
                                typedLetters.Remove(typedLetters.Length - 1, 1);
                            }
                        }
                        //Letters
                        else
                        {
                            string letter = KeyToLetter(registeredKeycode[i]);
                            if (currentLetters.Contains((letter, false)))
                            {
                                typedLetters.Append(letter);
                                currentLetters[currentLetters.IndexOf((letter, false))] = (letter, true);
                            }
                        }
                        
                        string typedAsString = typedLetters.ToString();
                        lettersContainer.text = CurrentLetterToString(currentLetters);
                        typedLettersContainer.text = typedAsString;
                        
                        //Check word and update scores
                        if (typedAsString.Length >= CurrentProgress.minimumWordLength && words.Any(x=>x.Equals(typedAsString, StringComparison.OrdinalIgnoreCase)))
                        {
                            wordMatch = true;
                            baseE = 0;
                            bonusE = typedAsString.Length >= CurrentProgress.minimumBonusLength ? Mathf.Max(Mathf.RoundToInt(Mathf.Pow(2f, typedAsString.Length - 5) * CurrentProgress.bonusEnergyGainRate) , 0) : 0;
                            foreach (var chr in typedAsString)
                            {
                                baseE += letterValues[chr] * CurrentProgress.baseEnergyGainRate;
                            }

                            energyBaseText.text = $"+{baseE} Energy";
                            energyBonusText.text = $"+{bonusE:N0} Bonus";
                        }
                        else
                        {
                            wordMatch = false;
                            energyBaseText.text = "";
                            energyBonusText.text = "";
                        }
                    }
                }
            }
        }
    }

    private void AnimateEnergy(int energy)
    {
        DOTween.To(() => CurrentEnergy, (x) =>
        {
            CurrentEnergy = x;
            energyBar.value = (float) x / MaximumEnergy;
            currentEnergyText.text = x.ToString("n0");
        }, Mathf.Clamp(energy, 0, MaximumEnergy), 0.4f);
    }

    private void AnimateScore(float score)
    {
        DOTween.To(() => Score, (x) =>
        {
            Score = x;
            scoreText.text = x.ToString("N0");
        }, score, 0.4f);
    }

    public void Damage(float amount)
    {
        if (shieldActive)
        {
            shieldTime = 0;
            if (CurrentShield >= amount) CurrentShield -= amount;
            else
            {
                shieldActive = false;
                healthShieldBar.DeactivateShield();
                CurrentHealth -= amount - CurrentShield;
                CurrentShield = 0;
            }
        }
        else CurrentHealth -= amount;

        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            gameStart = false;
            shipTransform.gameObject.SetActive(false);
            Instantiate(explosionPrefab, shipTransform.position, Quaternion.identity);
            StartCoroutine(GameOver());
        }
        healthShieldBar.SetCurrentHealth(CurrentHealth);
        healthShieldBar.SetCurrentShield(CurrentShield);
    }

    IEnumerator GameOver(bool victory = false)
    {
        gameStart = false;
        DOTween.To(() => cameraMover.speed, (x) => cameraMover.speed = x, 0f, 0.75f);
        yield return new WaitForSeconds(2f);
        endgamePanel.gameObject.SetActive(true);
        endgamePanel.OpenPanel(victory, Score);
    }

    void AnimateStageAnnouncer()
    {
        stageAnnouncerText.text =
            $"<b>Stage {gameProgress + 1}</b>\n<size=40>{CurrentProgress.numberOfLetters} Letters\n{CurrentProgress.minimumWordLength} Letter Words</size>";
        Sequence seq = DOTween.Sequence();
        seq.Append(stageAnnouncerText.transform.DOScale(Vector3.one, 0.5f))
            .Insert(0f, stageAnnouncerText.DOColor(Color.white, 0.5f))
            .AppendInterval(2f)
            .Append(stageAnnouncerText.transform.DOScale(new Vector3(1.4f, 1.4f, 1.4f), 0.5f))
            .Insert(2.5f, stageAnnouncerText.DOColor(new Color(1, 1, 1, 0), 0.5f));
    }

    public static void EnemyClick(Enemy e, int btn)
    {
        Skill s = instance.skills[btn];
        Debug.Log($"Shooting {s.skillName} {s.projectileAmount} times");
        if (instance.CurrentEnergy >= s.energyCost)
        {
            instance.turretBase.rotation = Quaternion.LookRotation(instance.turretBase.forward,
                e.transform.position - instance.turretBase.position);
            instance.StartCoroutine(instance.Shoot(s));
            instance.AnimateEnergy(instance.CurrentEnergy - s.energyCost);
        }
    }

    public static void FinishHowToPlay()
    {
        instance.SetupGame();
    }

    IEnumerator Shoot(Skill skill)
    {
        for(int i = skill.projectileAmount; i>0;i--)
        {
            instance.muzzleFlash.Play();
            
            GameObject go = Instantiate(skill.projectile, turretBase.position, turretBase.rotation);
            go.GetComponent<DefaultNamespace.Projectile>()
                .SetupProjectile(turretBase.transform.parent.gameObject, skill.damage / skill.projectileAmount);
            Debug.Log($"Fire! {go.name} {skill.damage} dmg");
            if (i > 1) yield return new WaitForSeconds(1f / skill.projectileFireRate);
        }
    }

    public static void ProjectileHit(DefaultNamespace.Projectile proj)
    {
        instance.Damage(proj.damage);
    }

    public static void AddScore(float score)
    {
        instance.AnimateScore(instance.Score + score);
    }

    List<(string letter, bool selected)> GetLetters()
    {
        List<(string letter, bool selected)> sb = new List<(string letter, bool selected)>();
        //Loop dictionary
        int weightSum = frequencyDictionary.Sum(x => x.Value);

        for (int i = 0; i < CurrentProgress.numberOfLetters; i++)
        {
            float rnd = UnityEngine.Random.Range(0, weightSum);
            
            foreach (var dict in frequencyDictionary)
            {
                rnd -= dict.Value;
                if (rnd <= 0)
                {
                    sb.Add((KeyToLetter(dict.Key), false));
                    break;
                }
            }
        }

        return sb;
    }

    public KeyCode LetterToKey(string letter)
    {
        if (Enum.TryParse(letter, true, out KeyCode res)) return res;
        return KeyCode.Escape;
    }

    public string KeyToLetter(KeyCode key)
    {
        return key.ToString().ToUpper();
    }

    public string CurrentLetterToString(List<(string letter, bool selected)> currentLetter)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < currentLetter.Count; i++)
        {
            if(currentLetter[i].selected)
                sb.Append($"<color=green>{currentLetter[i].letter}</color>");
            else sb.Append(currentLetter[i].letter);
        }

        return sb.ToString();
    }
}