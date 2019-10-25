using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    Board m_gameBoard;
    Spawner m_spawner;
    SoundManager m_soundManager;
    Shape m_activeShape;
    ScoreManager m_scoreManager;
    Holder m_holder;

    Vector3 m_boardCenterPosition;

    [SerializeField]
    ParticleTrigger m_gameOverFx;

    // Initial interval of time between active shape drops
    public float m_dropInterval = 0.6f;
    // Modified version of the drop interval, it changes during the game
    float m_dropIntervalModded;

    public float m_dropIntervalAcceleration = 0.05f;

    // Time use to check when the active shape should drop
    float m_timeToDrop;

    float m_timeToLand;

    float m_maxTimeToLand;

    // Serve to talk from update to late update about snap input
    bool m_snapButtonDown = false;

    enum Direction {Right, Left};


    // Delay of time it takes to land after a drop
    [Range(0.01f,1f)]
    [SerializeField]
    float m_landingWithDropDelay = 0.2f;

    // Maximum delay for a drop landing
    [Range(0.01f,10f)]
    [SerializeField]
    float m_maxLandingWithDropDelay = 0.8f;

    [Range(0.02f,1f)]
    public float m_keyRepeatRateLeftRight = 0.1f;
    [Range(0.02f,1f)]
    public float m_firstKeyRepeatRateLeftRight = 0.3f;
    float keyRepeatRateLeftRight;
    float m_timeToNextKeyLeftRight;

    [Range(0.01f,0.5f)]
    public float m_keyRepeatRateDown = 0.05f;
    float m_timeToNextKeyDown ;

    bool m_gameOver = false;

    // Is the shape landing
    bool isLanding = false;

    public GameObject m_gameOverPanel;

    public IconToggle m_rotIconToggle;
    bool m_rotClockwise = true;

    public bool m_isPaused = false;
    public GameObject m_pausePanel;

    GhostProjector m_ghost;

    // Start is called before the first frame update
    void Start()
    {
        // Initialise time to next key and time to drop
        m_timeToDrop = Time.time;

        // Initialise the drop interval
        m_dropIntervalModded = m_dropInterval;

        // Gets the board and spawner with there tags
        // There can only be one in the inspector, more causes issues 
        m_gameBoard = GameObject.FindObjectOfType<Board>();
        m_spawner = GameObject.FindObjectOfType<Spawner>();
        m_soundManager = GameObject.FindObjectOfType<SoundManager>();
        m_scoreManager = GameObject.FindObjectOfType<ScoreManager>();
        m_ghost = GameObject.FindObjectOfType<GhostProjector>();
        m_holder = GameObject.FindObjectOfType<Holder>();

        // Correct the spawner position to make it round
        // If there is no spawner, throws an error
        if (m_spawner)
        {
            m_spawner.transform.position = Vectorf.Round(m_spawner.transform.position);

            if (!m_activeShape)
            {
                m_spawner.InitQueue();
                m_activeShape = m_spawner.SpawnRandomShape();
            }
        }
        else
        {
            Debug.LogWarning($"<color=red><b>ERROR</b> There is no spawner defined!</color>");
        }

        // If there is no game board, throws an error
        if(!m_gameBoard)
        {
            Debug.LogWarning($"<color=red><b>ERROR</b> There is no game board defined!</color>");
        }
        // If there is no sound manager, throws an error
        if(!m_soundManager)
        {
            Debug.LogWarning($"<color=red><b>ERROR</b> There is no Sound Manager defined!</color>");
        }
        // If there is no score manager, throws an error
        if(!m_scoreManager)
        {
            Debug.LogWarning($"<color=red><b>ERROR</b> There is no Score Manager defined!</color>");
        }

        if (m_gameOverPanel)
        {
            m_gameOverPanel.SetActive(false);
        }
        if (m_pausePanel)
        {
            m_pausePanel.SetActive(false);
        }

        // Set up the reference position of center of the board
        Vector3 gameboardPos = new Vector3 (m_gameBoard.transform.position.x, m_gameBoard.transform.position.y, m_gameBoard.transform.position.z);
        Vector3 gameboardMiddlePoint = new Vector3 (0.5f*m_gameBoard.m_boardWidth,0.5f*m_gameBoard.m_boardHeight,0);
        m_boardCenterPosition = gameboardPos + gameboardMiddlePoint;
    }

    // Update is called once per frame
    void Update()
    {
        // If there is no gameboard, spawner, soundManager or active shape, don't run the game
        if (!m_gameBoard || !m_spawner || !m_activeShape || !m_soundManager || !m_scoreManager || m_gameOver)
        {
            return;
        }

        PlayerInput();
    }
    

    void LateUpdate() 
    {
        if (m_ghost)
        {
            m_ghost.DrawGhost(m_activeShape, m_gameBoard);

            if (m_snapButtonDown)
            {
                m_activeShape.transform.position = m_ghost.validGhostPosition;
                
                PlaySound(m_soundManager.m_snapSound, 1f);
                m_activeShape.SnapShapeFX();
                LandShape();

                m_snapButtonDown = false;
            }  
        }
    }

    void PlayerInput()
    {
        if ((Input.GetButton("MoveRight") && (Time.time > m_timeToNextKeyLeftRight)) || (Input.GetButtonDown("MoveRight")))
        {
            if (Input.GetButtonDown("MoveRight"))
            {
                keyRepeatRateLeftRight = m_firstKeyRepeatRateLeftRight;
            }
            else
            {
                keyRepeatRateLeftRight = m_keyRepeatRateLeftRight;
            }
            MoveLeftRight(Direction.Right);
        }

        else if ((Input.GetButton("MoveLeft") && (Time.time > m_timeToNextKeyLeftRight)) || (Input.GetButtonDown("MoveLeft")))
        {
            if (Input.GetButtonDown("MoveLeft"))
            {
                keyRepeatRateLeftRight = m_firstKeyRepeatRateLeftRight;
            }
            else
            {
                keyRepeatRateLeftRight = m_keyRepeatRateLeftRight;
            }
            MoveLeftRight(Direction.Left);
        }

        else if (Input.GetButtonDown("Rotate"))
        {
            Rotate();
        }

        // If the player press down OR
        // if it's time to drop the shape, increment for the next time a drop is needed
        else if (((Input.GetButton("MoveDown") && (Time.time > m_timeToNextKeyDown)) || (Time.time > m_timeToDrop) && !m_gameBoard.isClearingRows))
        {
            Drop();
        }

        // Snap button input, the behavior must reside in late update to avoid bugs
        // the boolean snapButtonDown talks to late update
        else if (Input.GetButtonDown("SnapDown"))
        {
            m_snapButtonDown = true;
        }

        else if (Input.GetButtonDown("Hold"))
        {
            Hold();
        }

        else if (Input.GetButtonDown("Pause"))
        {
            TogglePause();
        }
    }

    void MoveLeftRight(Direction dir)
    {
        AddLandingDelay();

        if (dir == Direction.Left)
        {
            m_activeShape.MoveLeft();
        }
        else
        {
            m_activeShape.MoveRight();
        }
        
        m_timeToNextKeyLeftRight = Time.time + keyRepeatRateLeftRight;

        if(!m_gameBoard.IsValidPosition(m_activeShape))
        {
            if (dir == Direction.Left)
            {
                 m_activeShape.MoveRight();
            }
            else
            {
                m_activeShape.MoveLeft();
            }
            PlaySound(m_soundManager.m_errorSound, 0.5f);
        }
        else
        {
            PlaySound(m_soundManager.m_moveSound, 0.4f);
        }
    }

    void Drop()
    {
        m_timeToDrop = Time.time + m_dropIntervalModded;
        m_timeToNextKeyDown = Time.time + m_keyRepeatRateDown;

        m_activeShape.MoveDown();

        // If the droped position is not valid, that means it's eather over limit: game over
        // or below: it as landed
        if (!m_gameBoard.IsValidPosition(m_activeShape))
        {
            if(m_gameBoard.IsOverLimit(m_activeShape))
            {
                GameOver();
            }
            else
            {
                m_activeShape.MoveUp();

                // isLanding boolean makes sure to set the timeToLand and maxTimeToLand once, when it first touch the ground
                if (!isLanding)
                {
                    m_maxTimeToLand = Time.time + m_maxLandingWithDropDelay;
                    m_timeToLand = Time.time + m_landingWithDropDelay;
                    isLanding = true;
                }

                if ((Time.time > m_timeToLand) || (Time.time > m_maxTimeToLand))
                {
                    LandShape();
                }
            }
        }
    }

    void Rotate()
    {
        AddLandingDelay();
        m_activeShape.RotateClockwise(m_rotClockwise);

        if(!m_gameBoard.IsValidPosition(m_activeShape))
        {
            m_activeShape.RotateClockwise(!m_rotClockwise);
            PlaySound(m_soundManager.m_errorSound, 0.5f);
        }
        else
        {
            PlaySound(m_soundManager.m_rotateSound, 1.5f);
        }
    }

    void AddLandingDelay()
    {
        m_timeToLand += 2*m_landingWithDropDelay;
    }

    public void Hold()
    {
        if (!m_holder)
        {
            return;
        }

        // If there is no shape already held: hold the current one and spawn the next shape
        if (!m_holder.m_heldShape)
        {
            m_holder.Catch(m_activeShape);
            m_activeShape = m_spawner.SpawnRandomShape();
            PlaySound(m_soundManager.m_holdSound, 5f);
        }
        // If there is a held shape and it can be exchanged, exchange
        else if (m_holder && m_holder.m_canRelease)
        {
            Shape shapeToHold = m_activeShape;
            m_activeShape = m_holder.Release();
            m_activeShape.transform.position = m_spawner.transform.position;
            m_holder.Catch(shapeToHold);
            PlaySound(m_soundManager.m_holdSound, 3f);
        }
        // If there is a held shape and it can't be exchanged, notify the player that he can't
        else
        {
            // Shapes cannot be exchanged until one lands
            PlaySound(m_soundManager.m_errorSound, 5f);
        }

        if (m_ghost)
        {
            m_ghost.Reset();
        }
        
    }

    void LandShape()
    {
        // Play landing sound
        PlaySound(m_soundManager.m_landSound, 2f);

        // Play particle effect
        m_activeShape.LandShapeFX();

        if (m_ghost)
        {
            m_ghost.Reset();
        }

        // When it lands, you can hold the new shape
        if (m_holder)
        {
            m_holder.m_canRelease = true;
        }

        // Make it so the player can instantly move the shape
        m_timeToNextKeyLeftRight = Time.time;
        m_timeToNextKeyDown = Time.time;

        // Store the shape's position in the "occupied" grid
        m_gameBoard.StoreShapeInGrid(m_activeShape);

        //Spawn a random shape

        // TO DO: avoid case where the next shape is the same as the active one
        // avoid case where a shape does not show up for 14 spawns
        // make it so the first 7 shapes cover all 7 shapes
        m_activeShape = m_spawner.SpawnRandomShape();

        // Clear rows that are full 
        StartCoroutine(m_gameBoard.CheckAndClearAllRows());

        if (m_gameBoard.m_completedRows > 0)
        {
            PlaySound(m_soundManager.m_clearRowSound, m_gameBoard.m_completedRows);

            if (m_scoreManager.m_didLevelUp)
            {
                m_dropIntervalModded = Mathf.Clamp(m_dropInterval - (((float) m_scoreManager.m_level - 1) * m_dropIntervalAcceleration),0.05f, 1f);
            }

            // Add completed rows to the score
            // More complex mechanics like spin, clearing or back to back will need a better system
            m_scoreManager.ScoreLines(m_gameBoard.m_completedRows);

            if (m_gameBoard.m_completedRows > 3)
            {
                PlaySound(m_soundManager.m_tetrisSound, 15f);
            }
        }

        isLanding = false;
    }

    // Linked in the inspector to the restart button
    public void Restart()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("Game");
    }

    void GameOver()
    {
        PlaySound(m_soundManager.m_gameOverSound, 5f);

        m_gameOver = true;

        StartCoroutine(GameOverRoutine());

        m_soundManager.m_musicSource.volume = m_soundManager.m_musicVolume * 0.1f;
    }

    IEnumerator GameOverRoutine()
    {
        if(m_gameOverFx)
        {
            m_gameOverFx.Play();
        }
        yield return new WaitForSeconds(0.3f);

        m_gameOverPanel.SetActive(true);
    }

    void PlaySound(AudioClip clip, float volMultiplier)
    {
        if (m_soundManager.m_sfxEnabled && clip)
        {
            AudioSource.PlayClipAtPoint(clip, m_boardCenterPosition, Mathf.Clamp(m_soundManager.m_sfxVolume * volMultiplier, 0.05f, 1f));
        }
    }

    public void ToggleRotDirection()
    {
        m_rotClockwise = !m_rotClockwise;

        if (m_rotIconToggle)
        {
            m_rotIconToggle.ToggleIcon(m_rotClockwise);
        }
    }

    public void TogglePause()
    {
        if (m_gameOver)
        {
            return;
        }

        m_isPaused = !m_isPaused;

        if (m_pausePanel)
        {
            m_pausePanel.SetActive(m_isPaused);

            if (m_soundManager)
            {
                m_soundManager.m_musicSource.volume = (m_isPaused) ? m_soundManager.m_musicVolume * 0.1f : m_soundManager.m_musicVolume;
            }

            Time.timeScale = (m_isPaused) ? 0 : 1;
        }
    }
}
